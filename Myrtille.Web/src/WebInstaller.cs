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
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Windows.Forms;
using System.Xml;
using Myrtille.Helpers;

namespace Myrtille.Web
{
    [RunInstaller(true)]
    public class WebInstaller : Installer
    {
        public override void Install(
            IDictionary stateSaver)
        {
            // enable the line below to debug this installer; disable otherwise
            //MessageBox.Show("Attach the .NET debugger to the 'MSI Debug' msiexec.exe process now for debug. Click OK when ready...", "MSI Debug");

            // if the installer is running in repair mode, it will try to re-install Myrtille... which is fine
            // problem is, it won't uninstall it first... which is not fine because some components can't be installed twice!
            // thus, prior to any install, try to uninstall first

            Context.LogMessage("Myrtille.Web is being installed, cleaning first");

            try
            {
                Uninstall(null);
            }
            catch (Exception exc)
            {
               Context.LogMessage(string.Format("Failed to clean Myrtille.Web ({0})", exc));
            }

            Context.LogMessage("Installing Myrtille.Web");

            base.Install(stateSaver);

            try
            {
                var process = new Process();

                bool debug = true;

                #if !DEBUG
                    debug = false;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                #endif

                // the install.ps1 (powershell) script enable the myrtille prerequisites (IIS, .NET, websocket, WCF/HTTP activation, etc.),
                // create a self-signed certificate (if requested), an application pool and a web application for myrtille, etc.
                // it can be adapted and run manually outside of this installer, if needed
                // its output is logged under <install path>\log\install.log
                process.StartInfo.FileName = string.Format(@"{0}\WindowsPowerShell\v1.0\powershell.exe", Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess ? Environment.SystemDirectory.ToLower().Replace("system32", "sysnative") : Environment.SystemDirectory);
                process.StartInfo.Arguments = "-ExecutionPolicy Bypass" +
                    " -Command \"& '" + Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "Myrtille.Web.Install.ps1") + "'" +
                    " -InstallPath '" + Path.GetFullPath(Context.Parameters["targetdir"]) + "'" +
                    " -SslCert " + (!string.IsNullOrEmpty(Context.Parameters["SSLCERT"]) ? "1" : "0") +
                    " -DebugMode " + (debug ? "1" : "0") +
                    " | Tee-Object -FilePath '" + Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "log", "Myrtille.Web.Install.log") + "'" + "\"";

                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new Exception(string.Format("An error occured while running {0}. See {1} for more information.",
                        Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "Myrtille.Web.Install.ps1"),
                        Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "log", "Myrtille.Web.Install.log")));
                }

                // load config
                var config = new XmlDocument();
                var configPath = Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "Web.config");
                config.Load(configPath);

                var navigator = config.CreateNavigator();

                // services port
                int servicesPort = 8080;
                if (!string.IsNullOrEmpty(Context.Parameters["SERVICESPORT"]))
                {
                    int.TryParse(Context.Parameters["SERVICESPORT"], out servicesPort);
                }

                if (servicesPort != 8080)
                {
                    // client endpoints
                    var client = XmlTools.GetNode(navigator, "/configuration/system.serviceModel/client");
                    if (client != null)
                    {
                        client.InnerXml = client.InnerXml.Replace("8080", servicesPort.ToString());
                    }
                }

                // admin services port
                int adminServicesPort = 8008;
                if (!string.IsNullOrEmpty(Context.Parameters["ADMINSERVICESPORT"]))
                {
                    int.TryParse(Context.Parameters["ADMINSERVICESPORT"], out adminServicesPort);
                }

                if (adminServicesPort != 8008)
                {
                    // application settings
                    var settings = XmlTools.GetNode(navigator, "/configuration/applicationSettings/Myrtille.Web.Properties.Settings");
                    if (settings != null)
                    {
                        settings.InnerXml = settings.InnerXml.Replace("8008", adminServicesPort.ToString());
                    }
                }

                // pdf printer
                var appSettings = XmlTools.GetNode(navigator, "/configuration/appSettings");
                if (appSettings != null)
                {
                    XmlTools.WriteConfigKey(appSettings, "AllowPrintDownload", (!string.IsNullOrEmpty(Context.Parameters["PDFPRINTER"])).ToString().ToLower());
                }

                // http session state
                // the default mode into web.config is cookieless="UseUri"
                // if this should change, reverse the code below
                if (string.IsNullOrEmpty(Context.Parameters["SESSIONURL"]))
                {
                    var systemWeb = XmlTools.GetNode(navigator, "/configuration/system.web");
                    if (systemWeb != null)
                    {
                        XmlNode sessionStateUseUri = null;
                        XmlNode sessionStateUseCookies = null;

                        foreach (XmlNode node in systemWeb.ChildNodes)
                        {
                            // http session id passed into url
                            // allows multiple connections/tabs or iframes
                            if (node is XmlElement && node.Name == "sessionState" && node.OuterXml.Contains("cookieless=\"UseUri\""))
                            {
                                sessionStateUseUri = node;
                            }
                            // http session id stored into a cookie
                            // same connection for all tabs because a cookie is set for a domain
                            else if (node is XmlComment && node.Value.StartsWith("<sessionState") && node.Value.Contains("cookieless=\"UseCookies\""))
                            {
                                sessionStateUseCookies = node;
                            }
                        }

                        // comment cookieless="UseUri"
                        if (sessionStateUseUri != null)
                        {
                            XmlTools.CommentNode(config, systemWeb, sessionStateUseUri);
                        }

                        // uncomment cookieless="UseCookies"
                        if (sessionStateUseCookies != null)
                        {
                            XmlTools.UncommentNode(config, systemWeb, sessionStateUseCookies);
                        }
                    }
                }

                // save config
                config.Save(configPath);

                // add write permission to the targetdir "log" folder for MyrtilleAppPool, so that Myrtille.Web can save logs into it
                PermissionsHelper.AddDirectorySecurity(
                    Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "log"),
                    "IIS AppPool\\MyrtilleAppPool",
                    FileSystemRights.Write,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow);

                Context.LogMessage("Installed Myrtille.Web");
            }
            catch (Exception exc)
            {
                Context.LogMessage(string.Format("Failed to install Myrtille.Web ({0})", exc));
                throw;
            }
        }

        public override void Commit(
            IDictionary savedState)
        {
            base.Commit(savedState);
            // insert code as needed
        }

        public override void Rollback(
            IDictionary savedState)
        {
            base.Rollback(savedState);
            DoUninstall();
        }

        public override void Uninstall(
            IDictionary savedState)
        {
            base.Uninstall(savedState);
            DoUninstall();
        }

        private void DoUninstall()
        {
            // enable the line below to debug this installer; disable otherwise
            //MessageBox.Show("Attach the .NET debugger to the 'MSI Debug' msiexec.exe process now for debug. Click OK when ready...", "MSI Debug");

            Context.LogMessage("Uninstalling Myrtille.Web");

            try
            {
                var process = new Process();

                bool debug = true;

                #if !DEBUG
                    debug = false;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                #endif

                // same logic as for install, with an uninstall script
                process.StartInfo.FileName = string.Format(@"{0}\WindowsPowerShell\v1.0\powershell.exe", Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess ? Environment.SystemDirectory.ToLower().Replace("system32", "sysnative") : Environment.SystemDirectory);
                process.StartInfo.Arguments = "-ExecutionPolicy Bypass" +
                    " -Command \"& '" + Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "Myrtille.Web.Uninstall.ps1") + "'" +
                    " -DebugMode " + (debug ? "1" : "0") +
                    " | Tee-Object -FilePath '" + Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "log", "Myrtille.Web.Uninstall.log") + "'" + "\"";

                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new Exception(string.Format("An error occured while running {0}. See {1} for more information.",
                        Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "Myrtille.Web.Uninstall.ps1"),
                        Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "log", "Myrtille.Web.Uninstall.log")));
                }

                Context.LogMessage("Uninstalled Myrtille.Web");
            }
            catch (Exception exc)
            {
                // in case of any error, don't rethrow the exception or myrtille won't be uninstalled otherwise (rollback action)
                // if myrtille can't be uninstalled, it can't be re-installed either! (at least, with an installer of the same product code)
                Context.LogMessage(string.Format("Failed to uninstall Myrtille.Web ({0})", exc));
            }
        }
    }
}