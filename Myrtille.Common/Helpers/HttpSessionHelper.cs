/*
    Myrtille: A native HTML4/5 Remote Desktop Protocol client.

    Copyright(c) 2014-2020 Cedric Coste

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

using System;
using System.Configuration;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;

namespace Myrtille.Helpers
{
    public static class HttpSessionHelper
    {
        // adapted from https://dopeydev.com/session-fixation-attack-and-prevention-in-asp-net/

        /// <summary>
        /// logout the current http session
        /// </summary>
        /// <returns></returns>
        public static void AbandonSession()
        {
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
            HttpContext.Current.Session.RemoveAll();

            var sessionStateSection = (SessionStateSection)ConfigurationManager.GetSection("system.web/sessionState");
            if (!string.IsNullOrEmpty(sessionStateSection.CookieName) && HttpContext.Current.Request.Cookies[sessionStateSection.CookieName] != null)
            {
                HttpContext.Current.Response.Cookies[sessionStateSection.CookieName].Value = string.Empty;
                HttpContext.Current.Response.Cookies[sessionStateSection.CookieName].Expires = DateTime.Now.AddMonths(-20);
            }
        }

        /// <summary>
        /// create a new ID for the current http session
        /// </summary>
        /// <returns>new session ID</returns>
        public static string CreateSessionId()
        {
            var manager = new SessionIDManager();
            return manager.CreateSessionID(HttpContext.Current);
        }

        /// <summary>
        /// set the ID of the current http session
        /// </summary>
        /// <param name="id">session ID</param>
        /// <returns></returns>
        public static void SetSessionId(string id)
        {
            var manager = new SessionIDManager();
            bool redirected, cookieAdded;
            manager.SaveSessionID(HttpContext.Current, id, out redirected, out cookieAdded);
        }

        // adapted from https://stackoverflow.com/a/4420114/6121074

        /// <summary>
        /// prevent http session fixation attack by generating a new http session ID upon login
        /// </summary>
        /// <remarks>
        /// https://www.owasp.org/index.php/Session_Fixation
        /// </remarks>
        /// <returns>new session ID</returns>
        public static string RegenerateSessionId()
        {
            // create a new session id
            var manager = new SessionIDManager();
            var oldId = manager.GetSessionID(HttpContext.Current);
            var newId = manager.CreateSessionID(HttpContext.Current);
            bool redirected, cookieAdded;
            manager.SaveSessionID(HttpContext.Current, newId, out redirected, out cookieAdded);

            // retrieve the current session
            var application = HttpContext.Current.ApplicationInstance;
            var session = (SessionStateModule)application.Modules.Get("Session");
            var fields = session.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            // parse the session fields
            SessionStateStoreProviderBase store = null;
            FieldInfo rqIdField = null, rqLockIdField = null, rqStateNotFoundField = null;
            SessionStateStoreData rqItem = null;
            foreach (var field in fields)
            {
                switch (field.Name)
                {
                    case "_store":
                        store = (SessionStateStoreProviderBase)field.GetValue(session);
                        break;

                    case "_rqId":
                        rqIdField = field;
                        break;

                    case "_rqLockId":
                        rqLockIdField = field;
                        break;

                    case "_rqSessionStateNotFound":
                        rqStateNotFoundField = field;
                        break;

                    case "_rqItem":
                        rqItem = (SessionStateStoreData)field.GetValue(session);
                        break;
                }
            }

            // remove the session from the store
            var lockId = rqLockIdField.GetValue(session);
            if (lockId != null && oldId != null)
            {
                store.RemoveItem(HttpContext.Current, oldId, lockId, rqItem);
            }

            // assign the new id to the session
            // the session will be added back to the store, with the new id, on the next http request
            rqStateNotFoundField.SetValue(session, true);
            rqIdField.SetValue(session, newId);

            return newId;
        }
    }
}