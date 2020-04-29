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

namespace Myrtille.Web.popups
{
    public partial class CredentialsPrompt : Page
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
                    if (Request["hostId"] != null)
                    {
                        if (!long.TryParse(Request["hostId"], out _hostId))
                        {
                            hostID.Value = _hostId.ToString();
                        }
                    }

                    if (Request["edit"] != null)
                    {
                        hostID.Value = _hostId.ToString();
                    }

                    if (!IsPostBack)
                    {
                        promptDomain.Value = _enterpriseSession.Domain;
                    }
                }
                catch (ThreadAbortException)
                {
                    // occurs because the response is ended after redirect
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to load credentials prompt ({0})", exc);
            }
        }

        /// <summary>
        /// create session host credentials and connect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ConnectButtonClick(
            object sender,
            EventArgs e)
        {
            if (_enterpriseClient == null || _enterpriseSession == null || _hostId == 0)
                return;

            try
            {
                var enterpriseCredentials = new EnterpriseHostSessionCredentials
                {
                    HostID = _hostId,
                    SessionID = _enterpriseSession.SessionID,
                    SessionKey = _enterpriseSession.SessionKey,
                    Domain = promptDomain.Value,
                    Username = promptUserName.Value,
                    Password = promptPassword.Value
                };

                if (!_enterpriseClient.AddSessionHostCredentials(enterpriseCredentials))
                {
                    throw new Exception("Failed to add session host credentials");
                }

                // connect to remote host
                Response.Redirect(Request.RawUrl + (Request.RawUrl.Contains("?") ? "&" : "?") + "edit=success", false);
            }
            catch (ThreadAbortException)
            {
                // occurs because the response is ended after redirect
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to create host session credentials ({0})", exc);
            }
        }
    }
}