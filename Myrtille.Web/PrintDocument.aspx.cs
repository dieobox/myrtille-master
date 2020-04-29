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
using Myrtille.Helpers;

namespace Myrtille.Web
{
    public partial class PrintDocument : Page
    {
        private PrinterClient _printerClient = new PrinterClient();
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

                // retrieve the pdf file
                if (!string.IsNullOrEmpty(Request["name"]))
                {
                    try
                    {
                        var fileStream = _printerClient.GetPdfFile(_remoteSession.Id, Request["name"]);
                        FileHelper.DownloadFile(Response, fileStream, Request["name"], true, "application/pdf", Request["disposition"]);
                    }
                    catch (ThreadAbortException)
                    {
                        // occurs because the response is ended after sending the pdf
                    }
                    catch (Exception exc)
                    {
                        System.Diagnostics.Trace.TraceError("Failed to download pdf file ({0})", exc);
                    }
                    finally
                    {
                        // remove the pdf file
                        _printerClient.DeletePdfFile(_remoteSession.Id, Request["name"]);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // occurs because the response is ended after deleting the pdf
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed to retrieve the active remote session ({0})", exc);
            }
        }
    }
}