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

namespace Myrtille.Web
{
    public partial class EnterpriseChangePassword : Page
    {
        private EnterpriseClient _enterpriseClient = new EnterpriseClient();

        private bool _localAdmin;

        /// <summary>
        /// page load (postback data is now available)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            // retrieve the user
            if (Request["userName"] != null)
            {
                userName.Value = Request["userName"];
            }
            else
            {
                changePassword.Disabled = true;
            }

            // local admin
            if (!string.IsNullOrEmpty(Request["mode"]) && Request["mode"].Equals("admin"))
            {
                _localAdmin = true;
            }
        }

        /// <summary>
        /// change a user password
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ChangePasswordButtonClick(
            object sender,
            EventArgs e)
        {
            if (_enterpriseClient == null || string.IsNullOrEmpty(userName.Value))
                return;

            try
            {
                if (string.IsNullOrEmpty(oldPassword.Value))
                {
                    changeError.InnerText = "Old password must be specified";
                }
                else if (!string.Equals(newPassword.Value, confirmPassword.Value))
                {
                    changeError.InnerText = "New and confirmed passwords do not match";
                }
                // ensure password change for local admin
                // for the enterprise domain, this is delegated to the domain policy (whether to accept an empty password, same as the old one, on change)
                else if (_localAdmin)
                {
                    if (string.IsNullOrEmpty(newPassword.Value))
                    {
                        changeError.InnerText = "New password must be specified";
                    }
                    else if (string.Equals(oldPassword.Value, newPassword.Value))
                    {
                        changeError.InnerText = "Old and new passwords must be different";
                    }
                }

                if (string.IsNullOrEmpty(changeError.InnerText))
                {
                    if (_enterpriseClient.ChangeUserPassword(userName.Value, oldPassword.Value, newPassword.Value))
                    {
                        Response.Redirect(Request.RawUrl + (Request.RawUrl.Contains("?") ? "&" : "?") + "change=success");
                    }
                    else
                    {
                        changeError.InnerText = "Password change failed";
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // occurs because the response is ended after redirect
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to change user {0} password ({1})", userName.Value, exc);
                changeError.InnerText = "Password change failed";
            }
        }
    }
}