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
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Myrtille.Admin.Web.Properties;
using Myrtille.Services.Contracts;

namespace Myrtille.Admin.Web
{
    /*

    README!

    this project is a mockup to demonstrate how to use myrtille within iframes into a page, which could be an external website on the same domain
    it shows how to generate connection(s) identifier(s) (with a connection API) in order to avoid leaking any connection(s) details to the end user (only the ids are known to the browser, details are retrieved by the backend)
    it allows to capture user activity and provides tools to share a remote session and manage its participants

    this project requires "cookieless=UseUri" session state into Myrtille web.config
    this is because all iframes would share the same http session otherwise, as ASP.NET stores the http session id into a cookie set for the domain

    */

    public partial class Default : Page
    {
        private static ConnectionClient connectionClient = new ConnectionClient(Settings.Default.ConnectionServiceUrl);

        protected void Page_Load(object sender, EventArgs e)
        {
            // CAUTION! ensure the website is running on the same domain as the myrtille iframe(s)
            // otherwise, the browser cross-domain restrictions will prevent the iframe(s) to communicate with the website and read/write the cookies they need to operate,
            // which would result in new connection(s) each time the website is reloaded

            // for example, if the website is http://mywebsite.com, the myrtille iframe(s) url(s) could be:
            // http://mywebsite.com/myrtille1
            // http://mywebsite.com/myrtille2
            // etc.
            // with "myrtille1" and "myrtille2" being 2 distinct gateways (load balancing scenario)

            // HTTPS support for REST calls
            // the line below should work but doesn't with RestSharp (?)
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            // disabling ssl certificate validation; this is a mockup, don't do this in production!
            ServicePointManager.ServerCertificateValidationCallback += (caller, certificate, chain, sslPolicyErrors) => true;

            // in the example below, the myrtille gateways are the same for 2 iframes
            // if the same connection info (user + host) is used, you will need the RDS role installed on the target server for that to work, configured to allow multiple sessions per user (GPO config)
            // use shift + tab to switch focus from one iframe to another
            LoadMyrtille(
                myrtille_1,
                "https://server/myrtille/",
                new ConnectionInfo
                {
                    User = new UserInfo
                    {
                        UserName = "user",
                        Password = "password"
                    },
                    Host = new HostInfo
                    {
                        IPAddress = "1.2.3.4",
                    },
                    AllowRemoteClipboard = true,
                    AllowFileTransfer = false,
                    AllowPrintDownload = true,
                    AllowAudioPlayback = true,
                    MaxActiveGuests = 2
                },
                true);

            LoadMyrtille(
                myrtille_2,
                "https://server/myrtille/",
                new ConnectionInfo
                {
                    User = new UserInfo
                    {
                        Domain = "domain",
                        UserName = "user",
                        Password = "password"
                    },
                    Host = new HostInfo
                    {
                        IPAddress = "1.2.3.4",
                    },
                    AllowRemoteClipboard = true,
                    AllowFileTransfer = false,
                    AllowPrintDownload = true,
                    AllowAudioPlayback = true,
                    MaxActiveGuests = 0                 // disable session sharing
                },
                false);

            UpdateControls();
        }

        private void LoadMyrtille(HtmlIframe iframe, string url, ConnectionInfo connectionInfo, bool focus)
        {
            try
            {
                // myrtille uses a cookie to store and retrieve the iframe's url once it's set for the iframe's http session
                // if it was just using the url given in parameter, the iframe's http session would be regenerated each time the portal is reloaded (because it uses "cookieless=UseUri" session state),
                // which would trigger a new connection
                if (Request.Cookies[iframe.ClientID] == null)
                {
                    connectionInfo.GatewayUrl = url;

                    var connectionId = connectionClient.GetConnectionId(connectionInfo);

                    // save the connection id used for the myrtille iframe
                    // it will be needed for any call to the myrtille API (screenshot, etc.)
                    var cookie = new HttpCookie(string.Format("{0}_cid", iframe.ClientID));
                    cookie.Value = connectionId.ToString();
                    cookie.Path = "/";
                    Response.Cookies.Add(cookie);

                    // also save the gateway url used for the myrtille iframe
                    // it's stored and thus could be retrieved from the connection info, but GetConnectionInfo is protected against subsequent calls
                    cookie = new HttpCookie(string.Format("{0}_url", iframe.ClientID));
                    cookie.Value = url;
                    cookie.Path = "/";
                    Response.Cookies.Add(cookie);

                    iframe.Src = url + "?cid=" + connectionId + "&__EVENTTARGET=&__EVENTARGUMENT=&connect=Connect%21";
                }
                else
                {
                    iframe.Src = Request.Cookies[iframe.ClientID].Value + "?fid=" + iframe.ClientID;
                }

                if (focus)
                {
                    iframe.Attributes["onload"] = "this.contentWindow.focus();";
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("failed to load myrtille iframe {0} ({1})", iframe.ClientID, exc);
            }
        }

        private void UpdateControls()
        {
            // iframe cookies are set/cleared on session connect/disconnect

            AddGuestButton.Disabled = Request.Cookies[myrtille_1.ClientID] == null;
            GetGuestsButton.Disabled = Request.Cookies[myrtille_1.ClientID] == null;
            GetGuestButton.Disabled = Request.Cookies[myrtille_1.ClientID] == null;
            UpdateGuestButton.Disabled = Request.Cookies[myrtille_1.ClientID] == null;
            RemoveGuestButton.Disabled = Request.Cookies[myrtille_1.ClientID] == null;

            SetScreenshotConfigButton.Disabled = Request.Cookies[myrtille_1.ClientID] == null;
            StartTakingScreenshotsButton.Disabled = Request.Cookies[myrtille_1.ClientID] == null;
            StopTakingScreenshotsButton.Disabled = Request.Cookies[myrtille_1.ClientID] == null;
            TakeScreenshotButton.Disabled = Request.Cookies[myrtille_1.ClientID] == null;

            myrtille_1_disconnect.Disabled = Request.Cookies[myrtille_1.ClientID] == null;
            myrtille_2_disconnect.Disabled = Request.Cookies[myrtille_2.ClientID] == null;

            Logout.Disabled = Request.Cookies[myrtille_1.ClientID] == null && Request.Cookies[myrtille_2.ClientID] == null;
        }

        private Guid GetIFrameConnectionId(string iframeId)
        {
            var connectionId = Guid.Empty;

            if (!string.IsNullOrEmpty(iframeId))
            {
                var cookie = Request.Cookies[string.Format("{0}_cid", iframeId)];
                if (cookie != null)
                {
                    Guid.TryParse(cookie.Value, out connectionId);
                }
            }

            return connectionId;
        }

        private string GetIFrameGatewayUrl(string iframeId)
        {
            var url = string.Empty;

            if (!string.IsNullOrEmpty(iframeId))
            {
                var cookie = Request.Cookies[string.Format("{0}_url", iframeId)];
                if (cookie != null)
                {
                    url = cookie.Value;
                }
            }

            return url;
        }

        #region Screenshot

        protected void SetScreenshotConfigButtonClick(
            object sender,
            EventArgs e)
        {
            var gatewayUrl = GetIFrameGatewayUrl((sender as HtmlInputButton).Attributes["data-fid"]);
            if (!string.IsNullOrEmpty(gatewayUrl))
            {
                var connectionId = GetIFrameConnectionId((sender as HtmlInputButton).Attributes["data-fid"]);
                if (connectionId != Guid.Empty)
                {
                    var captureClient = new CaptureClient(string.Format("{0}/api/Capture/", gatewayUrl));
                    captureClient.SetScreenshotConfig(connectionId, 10, CaptureFormat.PNG, @"C:\path\to\screenshots\");
                }
            }
        }

        protected void StartTakingScreenshotsButtonClick(
            object sender,
            EventArgs e)
        {
            var gatewayUrl = GetIFrameGatewayUrl((sender as HtmlInputButton).Attributes["data-fid"]);
            if (!string.IsNullOrEmpty(gatewayUrl))
            {
                var connectionId = GetIFrameConnectionId((sender as HtmlInputButton).Attributes["data-fid"]);
                if (connectionId != Guid.Empty)
                {
                    var captureClient = new CaptureClient(string.Format("{0}/api/Capture/", gatewayUrl));
                    captureClient.StartTakingScreenshots(connectionId);
                }
            }
        }

        protected void StopTakingScreenshotsButtonClick(
            object sender,
            EventArgs e)
        {
            var gatewayUrl = GetIFrameGatewayUrl((sender as HtmlInputButton).Attributes["data-fid"]);
            if (!string.IsNullOrEmpty(gatewayUrl))
            {
                var connectionId = GetIFrameConnectionId((sender as HtmlInputButton).Attributes["data-fid"]);
                if (connectionId != Guid.Empty)
                {
                    var captureClient = new CaptureClient(string.Format("{0}/api/Capture/", gatewayUrl));
                    captureClient.StopTakingScreenshots(connectionId);
                }
            }
        }

        protected void TakeScreenshotButtonClick(
            object sender,
            EventArgs e)
        {
            var gatewayUrl = GetIFrameGatewayUrl((sender as HtmlInputButton).Attributes["data-fid"]);
            if (!string.IsNullOrEmpty(gatewayUrl))
            {
                var connectionId = GetIFrameConnectionId((sender as HtmlInputButton).Attributes["data-fid"]);
                if (connectionId != Guid.Empty)
                {
                    var captureClient = new CaptureClient(string.Format("{0}/api/Capture/", gatewayUrl));
                    // retrieve screenshot data
                    var screenshotBytes = captureClient.TakeScreenshot(connectionId);
                    if (screenshotBytes != null && screenshotBytes.Length > 0)
                    {
                        // write it into the http response
                        Response.Headers.Add("ContentType", "image/png");
                        Response.Headers.Add("Content-Disposition", "attachment; filename=screenshot.png;");
                        Response.Headers.Add("Content-Length", screenshotBytes.Length.ToString());
                        Response.OutputStream.Write(screenshotBytes, 0, screenshotBytes.Length);
                    }
                }
            }
        }

        #endregion

        #region sharing

        // sorry for the low tech UI, it's just a mockup...

        protected void AddGuestButtonClick(
            object sender,
            EventArgs e)
        {
            var gatewayUrl = GetIFrameGatewayUrl((sender as HtmlInputButton).Attributes["data-fid"]);
            if (!string.IsNullOrEmpty(gatewayUrl))
            {
                var connectionId = GetIFrameConnectionId((sender as HtmlInputButton).Attributes["data-fid"]);
                if (connectionId != Guid.Empty && !string.IsNullOrEmpty(_allowControl.Value))
                {
                    var allowControl = true;
                    if (bool.TryParse(_allowControl.Value, out allowControl))
                    {
                        var script = string.Empty;
                        var sharingClient = new SharingClient(string.Format("{0}/api/Sharing/", gatewayUrl));
                        var guestId = sharingClient.AddGuest(connectionId, allowControl);
                        if (guestId == Guid.Empty)
                        {
                            script = "alert('failed to add a guest');";
                        }
                        else
                        {
                            script = string.Format("prompt('Sharing link (copy & paste into a new browser tab or window):', '{0}');", string.Format("{0}?gid={1}", "http://mywebsite.com/Myrtille/", guestId));
                        }
                        ClientScript.RegisterClientScriptBlock(GetType(), Guid.NewGuid().ToString(), script, true);
                    }
                }
            }
        }

        protected void GetGuestsButtonClick(
            object sender,
            EventArgs e)
        {
            var gatewayUrl = GetIFrameGatewayUrl((sender as HtmlInputButton).Attributes["data-fid"]);
            if (!string.IsNullOrEmpty(gatewayUrl))
            {
                var connectionId = GetIFrameConnectionId((sender as HtmlInputButton).Attributes["data-fid"]);
                if (connectionId != Guid.Empty)
                {
                    var script = string.Empty;
                    var sharingClient = new SharingClient(string.Format("{0}/api/Sharing/", gatewayUrl));
                    var guests = sharingClient.GetGuests(connectionId);
                    if (guests == null)
                    {
                        script = "alert('failed to retrieve the guests list');";
                    }
                    else
                    {
                        var info = string.Empty;
                        foreach (var guest in guests)
                        {
                            info += string.Format("guest: {0}, control: {1}, active: {2}, websocket: {3}\\n", guest.Id, guest.Control, guest.Active, guest.Websocket);
                        }
                        script = string.Format("alert('{0}');", info);
                    }
                    ClientScript.RegisterClientScriptBlock(GetType(), Guid.NewGuid().ToString(), script, true);
                }
            }
        }

        protected void GetGuestButtonClick(
            object sender,
            EventArgs e)
        {
            var gatewayUrl = GetIFrameGatewayUrl((sender as HtmlInputButton).Attributes["data-fid"]);
            if (!string.IsNullOrEmpty(gatewayUrl))
            {
                var connectionId = GetIFrameConnectionId((sender as HtmlInputButton).Attributes["data-fid"]);
                if (connectionId != Guid.Empty)
                {
                    if (!string.IsNullOrEmpty(_guestId.Value))
                    {
                        var guestId = Guid.Empty;
                        if (Guid.TryParse(_guestId.Value, out guestId))
                        {
                            var script = string.Empty;
                            var sharingClient = new SharingClient(string.Format("{0}/api/Sharing/", gatewayUrl));
                            var guest = sharingClient.GetGuest(guestId);
                            if (guest == null)
                            {
                                script = "alert('guest not found or failed to retrieve guest');";
                            }
                            else
                            {
                                script = string.Format("alert('guest: {0}, control: {1}, active: {2}, websocket: {3}');", guest.Id, guest.Control, guest.Active, guest.Websocket);
                            }
                            ClientScript.RegisterClientScriptBlock(GetType(), Guid.NewGuid().ToString(), script, true);
                        }
                    }
                }
            }
        }

        protected void UpdateGuestButtonClick(
            object sender,
            EventArgs e)
        {
            var gatewayUrl = GetIFrameGatewayUrl((sender as HtmlInputButton).Attributes["data-fid"]);
            if (!string.IsNullOrEmpty(gatewayUrl))
            {
                var connectionId = GetIFrameConnectionId((sender as HtmlInputButton).Attributes["data-fid"]);
                if (connectionId != Guid.Empty)
                {
                    if (!string.IsNullOrEmpty(_guestId.Value) && !string.IsNullOrEmpty(_allowControl.Value))
                    {
                        var guestId = Guid.Empty;
                        if (Guid.TryParse(_guestId.Value, out guestId))
                        {
                            var allowControl = true;
                            if (bool.TryParse(_allowControl.Value, out allowControl))
                            {
                                var script = string.Empty;
                                var sharingClient = new SharingClient(string.Format("{0}/api/Sharing/", gatewayUrl));
                                var guest = sharingClient.UpdateGuest(guestId, allowControl);
                                if (guest == null)
                                {
                                    script = "alert('guest not found or failed to update guest');";
                                }
                                else
                                {
                                    script = string.Format("alert('updated guest: {0}, control: {1}, active: {2}, websocket: {3}');", guest.Id, guest.Control, guest.Active, guest.Websocket);
                                }
                                ClientScript.RegisterClientScriptBlock(GetType(), Guid.NewGuid().ToString(), script, true);
                            }
                        }
                    }
                }
            }
        }

        protected void RemoveGuestButtonClick(
            object sender,
            EventArgs e)
        {
            var gatewayUrl = GetIFrameGatewayUrl((sender as HtmlInputButton).Attributes["data-fid"]);
            if (!string.IsNullOrEmpty(gatewayUrl))
            {
                var connectionId = GetIFrameConnectionId((sender as HtmlInputButton).Attributes["data-fid"]);
                if (connectionId != Guid.Empty)
                {
                    if (!string.IsNullOrEmpty(_guestId.Value))
                    {
                        var guestId = Guid.Empty;
                        if (Guid.TryParse(_guestId.Value, out guestId))
                        {
                            var script = string.Empty;
                            var sharingClient = new SharingClient(string.Format("{0}/api/Sharing/", gatewayUrl));
                            if (!sharingClient.RemoveGuest(guestId))
                            {
                                script = "alert('guest not found or failed to remove guest');";
                            }
                            else
                            {
                                script = string.Format("alert('removed guest: {0}');", guestId);
                            }
                            ClientScript.RegisterClientScriptBlock(GetType(), Guid.NewGuid().ToString(), script, true);
                        }
                    }
                }
            }
        }

        #endregion

        #region Disconnect

        protected void DisconnectButtonClick(
            object sender,
            EventArgs e)
        {
            var gatewayUrl = GetIFrameGatewayUrl((sender as HtmlInputButton).Attributes["data-fid"]);
            if (!string.IsNullOrEmpty(gatewayUrl))
            {
                var connectionId = GetIFrameConnectionId((sender as HtmlInputButton).Attributes["data-fid"]);
                if (connectionId != Guid.Empty)
                {
                    string script;
                    var disconnectionClient = new DisconnectionClient(string.Format("{0}/api/Disconnection/", gatewayUrl));
                    if (disconnectionClient.Disconnect(connectionId))
                    {
                        script = "alert('the remote session was disconnected successfully. Press OK to open a new connection');";
                    }
                    else
                    {
                        script = "alert('failed to disconnect the remote session');";
                    }
                    ClientScript.RegisterClientScriptBlock(GetType(), Guid.NewGuid().ToString(), script, true);
                }
            }
        }

        protected void LogoutButtonClick(
            object sender,
            EventArgs e)
        {
            var success = true;

            var disconnectionClient = new DisconnectionClient("https://server/myrtille/api/Disconnection/");
            success = disconnectionClient.DisconnectAll();

            // if the page have iframes on different gateways, call DisconnectAll for each of them
            //if (success)
            //{
            //    disconnectionClient = new DisconnectionClient("<another gateway, disconnection api url>");
            //    success = disconnectionClient.DisconnectAll();
            //}

            // etc.

            var script = string.Empty;
            if (success)
            {
                script = "alert('all the remote sessions were disconnected successfully');";
            }
            else
            {
                script = "alert('failed to disconnect all the remote sessions');";
            }
            script += "window.location.href = 'logout.aspx'";
            ClientScript.RegisterClientScriptBlock(GetType(), Guid.NewGuid().ToString(), script, true);
        }

        #endregion
    }
}