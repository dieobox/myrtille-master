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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Optimization;
using System.Web.Routing;
using log4net.Config;

namespace Myrtille.Web
{
    public enum HttpApplicationStateVariables
    {
        Cache,
        RemoteSessions,
        SharedRemoteSessions
    }

    public enum HttpSessionStateVariables
    {
        ClientIP,
        ClientKey,
        EnterpriseSession,
        RemoteSession,
        GuestInfo
    }

    public class Global : HttpApplication
    {
        protected void Application_Start(
            object sender,
            EventArgs e)
        {
            try
            {
                // logger
                XmlConfigurator.Configure();

                // application cache
                Application[HttpApplicationStateVariables.Cache.ToString()] = Context.Cache;

                // remote sessions
                Application[HttpApplicationStateVariables.RemoteSessions.ToString()] = new Dictionary<Guid, RemoteSession>();

                // shared remote sessions
                Application[HttpApplicationStateVariables.SharedRemoteSessions.ToString()] = new Dictionary<Guid, SharingInfo>();

                // Lame MP3 dlls require the web application bin path to be present into the PATH environment variable
                // additionally, the application pool must have read access to the bin folder
                AddBinToPathEnvironment();

                // Web Api
                RouteTable.Routes.MapHttpRoute(
                    name: "WebApi",
                    routeTemplate: "api/{controller}/{action}/{id}",
                    defaults: new { id = RouteParameter.Optional });

                // scripts/styles bundling
                BundleConfig.RegisterBundles(BundleTable.Bundles);
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to start Myrtille.Web application ({0})", exc);
                throw;
            }
        }

        // https://github.com/Corey-M/NAudio.Lame/wiki/Using-NAudio.Lame-with-MVC
        private void AddBinToPathEnvironment()
        {
            // bin path
            var binPath = Path.Combine(new string[] { AppDomain.CurrentDomain.BaseDirectory, "bin" });

            // PATH environment
            var path = Environment.GetEnvironmentVariable("PATH") ?? "";

            // add the bin path if not already present
            if (!path.Split(Path.PathSeparator).Contains(binPath, StringComparer.CurrentCultureIgnoreCase))
            {
                path = string.Join(Path.PathSeparator.ToString(), new string[] { path, binPath });
                Environment.SetEnvironmentVariable("PATH", path);
            }
        }
    }
}