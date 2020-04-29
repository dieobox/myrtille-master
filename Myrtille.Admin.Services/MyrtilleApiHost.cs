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
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using Myrtille.Admin.Services.Properties;

namespace Myrtille.Admin.Services
{
    public static class MyrtilleApiHost
    {
        private static IDisposable _selfHostObject;

        public static void Start()
        {
            var url = "http://*:" + Settings.Default.WebApiPort + "/MyrtilleAdmin/";
            Console.WriteLine($"{DateTime.UtcNow} - Starting Myrtille Admin API at url: " + url);

            try
            {
                _selfHostObject = WebApp.Start<Startup>(url);
            }
            catch (Exception ex)
            {
                _selfHostObject?.Dispose();
                Console.WriteLine($"{DateTime.UtcNow} - Failed to start Myrtille Admin API with error {ex}");
            }
        }

        public static void Stop()
        {
            _selfHostObject?.Dispose();
            Console.WriteLine($"{DateTime.UtcNow} - Stopping Myrtille Admin API");
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class Startup
        {
            // This code configures Web API. The Startup class is specified as a type
            // parameter in the WebApp.Start method.
            public void Configuration(IAppBuilder appBuilder)
            {
                // Configure Web API for self-host.
                var config = new HttpConfiguration();

                // NOTE we are not using the default startup types as we want to control the routes in the classes themselves.
                config.MapHttpAttributeRoutes();

                appBuilder.UseWebApi(config);
            }
        }
    }
}