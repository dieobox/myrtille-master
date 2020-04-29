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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace Myrtille.Web
{
    public partial class PushUpdates : Page
    {
        /// <summary>
        /// push image(s) updates(s) (region(s) or fullscreen(s)) from the remote session to the browser
        /// this is done through a long-polling request (also known as reverse ajax or ajax comet) issued by a zero sized iframe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            // if cookies are enabled, the http session id is added to the http request headers; otherwise, it's added to the http request url
            // in both cases, the given http session is automatically bound to the current http context

            RemoteSession remoteSession = null;

            try
            {
                if (Session[HttpSessionStateVariables.RemoteSession.ToString()] == null)
                    throw new NullReferenceException();

                // retrieve the remote session for the current http session
                remoteSession = (RemoteSession)Session[HttpSessionStateVariables.RemoteSession.ToString()];

                try
                {
                    // retrieve params
                    var longPollingDuration = int.Parse(Request.QueryString["longPollingDuration"]);
                    var imgIdx = int.Parse(Request.QueryString["imgIdx"]);

                    // stream image(s) data within the response for the given duration
                    // the connection will be automatically reseted by the client when the request ends
                    var startTime = DateTime.Now;
                    var remainingTime = longPollingDuration;
                    var currentImgIdx = imgIdx;

                    // notifications
                    var messageQueue = (List<RemoteSessionMessage>)remoteSession.Manager.MessageQueues[Session.SessionID];

                    while (remainingTime > 0)
                    {
                        // unstack message(s)
                        while (messageQueue.Count > 0)
                        {
                            var message = messageQueue[0];

                            switch (message.Type)
                            {
                                case MessageType.Connected:
                                    // add a slight delay before writing data
                                    // the output stream may not be ready yet if the remote session was just reconnected
                                    Thread.Sleep(2000);
                                    Response.Write("<script>parent.inject('if (parent != null && window.name != \\'\\') { parent.setCookie(window.name, window.location.href); };myrtille.initClient();');</script>");
                                    Response.Flush();
                                    break;

                                case MessageType.Disconnected:
                                    Response.Write("<script>parent.inject('window.location.href = config.getHttpServerUrl();');</script>");
                                    Response.Flush();
                                    break;

                                case MessageType.PageReload:
                                    Response.Write("<script>parent.inject('window.location.href = window.location.href;');</script>");
                                    Response.Flush();
                                    break;

                                case MessageType.RemoteClipboard:
                                    Response.Write(string.Format("<script>parent.inject('writeClipboard(\\'{0}\\');');</script>", message.Text.Replace(@"\", @"\\\\").Replace("\r", @"\\r").Replace("\n", @"\\n").Replace("'", @"\\\'")));
                                    Response.Flush();
                                    break;

                                case MessageType.TerminalOutput:
                                    Response.Write(string.Format("<script>parent.inject('writeTerminal(\\'{0}\\');');</script>", message.Text.Replace(@"\", @"\\\\").Replace("\r", @"\\r").Replace("\n", @"\\n").Replace("'", @"\\\'")));
                                    Response.Flush();
                                    break;

                                case MessageType.PrintJob:
                                    Response.Write(string.Format("<script>parent.inject('downloadPdf(\\'{0}\\');');</script>", message.Text));
                                    Response.Flush();
                                    break;
                            }

                            lock (((ICollection)messageQueue).SyncRoot)
                            {
                                messageQueue.RemoveAt(0);
                            }
                        }

                        // retrieve the next update, if available; otherwise, wait it for the remaining time
                        // stop waiting if a message is received during that time
                        var image = remoteSession.Manager.GetNextUpdate(currentImgIdx, remainingTime);
                        if (image != null)
                        {
                            System.Diagnostics.Trace.TraceInformation("Pushing image {0} ({1}), remote session {2}", image.Idx, (image.Fullscreen ? "screen" : "region"), remoteSession.Id);

                            var imgData =
                                image.Idx + "," +
                                image.PosX + "," +
                                image.PosY + "," +
                                image.Width + "," +
                                image.Height + "," +
                                "\\'" + image.Format.ToString().ToLower() + "\\'," +
                                image.Quality + "," +
                                image.Fullscreen.ToString().ToLower() + "," +
                                "\\'" + Convert.ToBase64String(image.Data) + "\\'";

                            imgData = "<script>parent.inject('processImage(" + imgData + ");');</script>";

                            // write the output
                            Response.Write(imgData);
                            Response.Flush();

                            currentImgIdx = image.Idx;
                        }

                        remainingTime = longPollingDuration - Convert.ToInt32((DateTime.Now - startTime).TotalMilliseconds);
                    }
                }
                catch (HttpException)
                {
                    // this occurs if the user reloads the page while the long-polling request is going on...
                }
                catch (Exception exc)
                {
                    System.Diagnostics.Trace.TraceError("Failed to push update(s), remote session {0} ({1})", remoteSession.Id, exc);
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to retrieve the active remote session ({0})", exc);
            }
        }
    }
}