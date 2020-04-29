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
using System.Collections.Generic;
using System.Threading;
using System.Web.UI;
using Myrtille.Services.Contracts;

namespace Myrtille.Web
{
    public partial class ShareSession : Page
    {
        private RemoteSession _remoteSession;

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
                if (Session[HttpSessionStateVariables.RemoteSession.ToString()] == null)
                    throw new NullReferenceException();

                _remoteSession = (RemoteSession)Session[HttpSessionStateVariables.RemoteSession.ToString()];

                // if remote session sharing is enabled, only the remote session owner can share it
                if (!_remoteSession.AllowSessionSharing || !Session.SessionID.Equals(_remoteSession.OwnerSessionID))
                {
                    Response.Redirect("~/", true);
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to retrieve the active remote session ({0})", exc);
            }
        }

        /// <summary>
        /// create a shared session URL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void CreateSessionUrlButtonClick(
            object sender,
            EventArgs e)
        {
            try
            {
                Application.Lock();

                // create a new guest for the remote session
                var sharedSessions = (IDictionary<Guid, SharingInfo>)Application[HttpApplicationStateVariables.SharedRemoteSessions.ToString()];
                var sharingInfo = new SharingInfo
                {
                    RemoteSession = _remoteSession,
                    GuestInfo = new GuestInfo
                    {
                        Id = Guid.NewGuid(),
                        ConnectionId = _remoteSession.Id,
                        Control = guestControl.Checked
                    }
                };

                sharedSessions.Add(sharingInfo.GuestInfo.Id, sharingInfo);
                sessionUrl.Value = Request.Url.Scheme + "://" + Request.Url.Host + (Request.Url.Port != 80 && Request.Url.Port != 443 ? ":" + Request.Url.Port : "") + Request.ApplicationPath + "/?gid=" + sharingInfo.GuestInfo.Id;
            }
            catch (ThreadAbortException)
            {
                // occurs because the response is ended after redirect
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to generate a session sharing url ({0})", exc);
            }
            finally
            {
                Application.UnLock();
            }
        }
    }
}