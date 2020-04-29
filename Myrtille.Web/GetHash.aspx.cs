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
using System.IO;
using System.Security.Cryptography;
using System.Web.UI;
using Myrtille.Helpers;

namespace Myrtille.Web
{
    public partial class GetHash : Page
    {
        /// <summary>
        /// retrieve the mouse cursor from the remote session and send it to the browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(
            object sender,
            EventArgs e)
        {

            try
            {
                // retrieve params
                using (StreamWriter sw = new StreamWriter(Response.OutputStream))
                {
                    String password = Request.QueryString["Password"];
                    String encrypted = CryptoHelper.RDP_Encrypt(password);
                    sw.WriteLine(encrypted);
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Trace.TraceError("Failed");
            }
        }

     }
}