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
using System.Windows.Forms;
using System.Xml;
using Myrtille.Helpers;

namespace Myrtille.Admin.Services
{
    [RunInstaller(true)]
    public class ServicesInstaller : Installer
	{
        public override void Install(
            IDictionary stateSaver)
        {
            // enable the line below to debug this installer; disable otherwise
            //MessageBox.Show("Attach the .NET debugger to the 'MSI Debug' msiexec.exe process now for debug. Click OK when ready...", "MSI Debug");

            // if the installer is running in repair mode, it will try to re-install Myrtille... which is fine
            // problem is, it won't uninstall it first... which is not fine because some components can't be installed twice!
            // thus, prior to any install, try to uninstall first

            Context.LogMessage("Myrtille.Admin.Services is being installed, cleaning first");

            try
            {
                Uninstall(null);
            }
            catch (Exception exc)
            {
               Context.LogMessage(string.Format("Failed to clean Myrtille.Admin.Services ({0})", exc));
            }

            Context.LogMessage("Installing Myrtille.Admin.Services");

            base.Install(stateSaver);

            try
            {
                var process = new Process();

                bool debug = true;

                #if !DEBUG
                    debug = false;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                #endif

                process.StartInfo.FileName = string.Format(@"{0}\WindowsPowerShell\v1.0\powershell.exe", Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess ? Environment.SystemDirectory.ToLower().Replace("system32", "sysnative") : Environment.SystemDirectory);
                process.StartInfo.Arguments = "-ExecutionPolicy Bypass" +
                    " -Command \"& '" + Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "bin", "Myrtille.Admin.Services.Install.ps1") + "'" +
                    " -BinaryPath '" + Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "bin", "Myrtille.Admin.Services.exe") + "'" +
                    " -DebugMode " + (debug ? "1" : "0") +
                    " | Tee-Object -FilePath '" + Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "log", "Myrtille.Admin.Services.Install.log") + "'" + "\"";

                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new Exception(string.Format("An error occured while running {0}. See {1} for more information.",
                        Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "bin", "Myrtille.Admin.Services.Install.ps1"),
                        Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "log", "Myrtille.Admin.Services.Install.log")));
                }

                // load config
                var config = new XmlDocument();
                var configPath = Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "bin", "Myrtille.Admin.Services.exe.config");
                config.Load(configPath);

                var navigator = config.CreateNavigator();

                // admin services port
                int adminServicesPort = 8008;
                if (!string.IsNullOrEmpty(Context.Parameters["ADMINSERVICESPORT"]))
                {
                    int.TryParse(Context.Parameters["ADMINSERVICESPORT"], out adminServicesPort);
                }

                if (adminServicesPort != 8008)
                {
                    // application settings
                    var settings = XmlTools.GetNode(navigator, "/configuration/applicationSettings/Myrtille.Admin.Services.Properties.Settings");
                    if (settings != null)
                    {
                        settings.InnerXml = settings.InnerXml.Replace("8008", adminServicesPort.ToString());
                    }
                }

                // save config
                config.Save(configPath);

                Context.LogMessage("Installed Myrtille.Admin.Services");
            }
            catch (Exception exc)
            {
                Context.LogMessage(string.Format("Failed to install Myrtille.Admin.Services ({0})", exc));
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

            Context.LogMessage("Uninstalling Myrtille.Admin.Services");

            try
            {
                var process = new Process();

                bool debug = true;

                #if !DEBUG
                    debug = false;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                #endif

                process.StartInfo.FileName = string.Format(@"{0}\WindowsPowerShell\v1.0\powershell.exe", Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess ? Environment.SystemDirectory.ToLower().Replace("system32", "sysnative") : Environment.SystemDirectory);
                process.StartInfo.Arguments = "-ExecutionPolicy Bypass" +
                    " -Command \"& '" + Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "bin", "Myrtille.Admin.Services.Uninstall.ps1") + "'" +
                    " -DebugMode " + (debug ? "1" : "0") +
                    " | Tee-Object -FilePath '" + Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "log", "Myrtille.Admin.Services.Uninstall.log") + "'" + "\"";

                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new Exception(string.Format("An error occured while running {0}. See {1} for more information.",
                        Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "bin", "Myrtille.Admin.Services.Uninstall.ps1"),
                        Path.Combine(Path.GetFullPath(Context.Parameters["targetdir"]), "log", "Myrtille.Admin.Services.Uninstall.log")));
                }

                Context.LogMessage("Uninstalled Myrtille.Admin.Services");
            }
            catch (Exception exc)
            {
                // in case of any error, don't rethrow the exception or myrtille won't be uninstalled otherwise (rollback action)
                // if myrtille can't be uninstalled, it can't be re-installed either! (at least, with an installer of the same product code)
                Context.LogMessage(string.Format("Failed to uninstall Myrtille.Admin.Services ({0})", exc));
            }
        }
	}
}