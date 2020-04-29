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
using System.Threading;
using System.Web.UI;
using Myrtille.Services.Contracts;

namespace Myrtille.Web
{
    public partial class EditHostSession : Page
    {
        private EnterpriseClient _enterpriseClient = new EnterpriseClient();
        private EnterpriseSession _enterpriseSession;
        private long _hostId = 0;

        /// <summary>
        /// page load (postback data is now available)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            try
            {
                if (Session[HttpSessionStateVariables.EnterpriseSession.ToString()] == null)
                    throw new NullReferenceException();

                _enterpriseSession = (EnterpriseSession)Session[HttpSessionStateVariables.EnterpriseSession.ToString()];

                try
                {
                    if (!_enterpriseSession.IsAdmin)
                    {
                        Response.Redirect("~/", true);
                    }

                    // retrieve the host
                    if (Request["hostId"] != null)
                    {
                        long hostId;
                        if (!long.TryParse(Request["hostId"], out hostId))
                        {
                            hostId = 0;
                        }

                        if (hostId != 0)
                        {
                            _hostId = hostId;

                            try
                            {
                                var host = _enterpriseClient.GetHost(_hostId, _enterpriseSession.SessionID);
                                if (host != null)
                                {
                                    hostName.InnerText = host.HostName;
                                }
                            }
                            catch (Exception exc)
                            {
                                System.Diagnostics.Trace.TraceError("Failed to retrieve host {0}, ({1})", _hostId, exc);
                            }
                        }
                    }

                    if (!IsPostBack)
                    {
                        userDomain.Value = _enterpriseSession.Domain;
                    }
                }
                catch (ThreadAbortException)
                {
                    // occurs because the response is ended after redirect
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to retrieve the active enterprise session ({0})", exc);
            }
        }

        /// <summary>
        /// create an host session URL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void CreateSessionUrlButtonClick(
            object sender,
            EventArgs e)
        {
            if (_enterpriseClient == null || _enterpriseSession == null || _hostId == 0 || string.IsNullOrEmpty(userName.Value) || string.IsNullOrEmpty(userPassword.Value))
                return;

            try
            {
                var url = _enterpriseClient.CreateUserSession(_enterpriseSession.SessionID, _hostId, userName.Value, userPassword.Value, userDomain.Value);
                if (!string.IsNullOrEmpty(url))
                {
                    sessionUrl.Value = Request.Url.Scheme + "://" + Request.Url.Host + (Request.Url.Port != 80 && Request.Url.Port != 443 ? ":" + Request.Url.Port : "")  + Request.ApplicationPath + "/"  + url + "&__EVENTTARGET=&__EVENTARGUMENT=&connect=Connect%21";
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to create session url for host {0} ({1})", _hostId, exc);
            }
        }
    }
}