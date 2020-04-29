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
using Newtonsoft.Json;
using Myrtille.Services.Contracts;
using RestSharp;

namespace Myrtille.Web
{
    public class ConnectionClient
    {
        private RestClient restClient;
        private static IDictionary<Guid, RemoteSessionState> connectionsState = new Dictionary<Guid, RemoteSessionState>();
        private static object connectionsLock = new object();

        public ConnectionClient(string connectionServiceUrl)
        {
            restClient = new RestClient(connectionServiceUrl);
        }

        public ConnectionInfo GetConnectionInfo(Guid connectionId)
        {
            var restRequest = new RestRequest($"GetConnectionInfo?connectionId={connectionId}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to call GetConnectionInfo; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
            return JsonConvert.DeserializeObject<ConnectionInfo>(restResponse.Content);
        }

        public bool IsUserAllowedToConnectHost(string domain, string userName, string hostIPAddress, Guid vmGuid)
        {
            var restRequest = new RestRequest($"IsUserAllowedToConnectHost?domain={domain}&userName={userName}&hostIPAddress={hostIPAddress}&vmGuid={vmGuid}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to call IsUserAllowedToConnectHost; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
            return JsonConvert.DeserializeObject<bool>(restResponse.Content);
        }

        public bool SetConnectionState(Guid connectionId, string IPAddress, Guid vmGuid, RemoteSessionState state)
        {
            // don't resend an unchanged connection state
            var send = true;

            lock (connectionsLock)
            {
                if (!connectionsState.ContainsKey(connectionId))
                {
                    connectionsState.Add(connectionId, state);
                }
                else
                {
                    send = connectionsState[connectionId] != state;
                    connectionsState[connectionId] = state;
                }
            }

            if (!send)
            {
                return true;
            }

            var restRequest = new RestRequest($"SetConnectionState?connectionId={connectionId}&IPAddress={IPAddress}&vmGuid={vmGuid}&state={state}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to call SetConnectionState; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
            return JsonConvert.DeserializeObject<bool>(restResponse.Content);
        }

        public bool SetConnectionExitCode(Guid connectionId, string IPAddress, Guid vmGuid, RemoteSessionExitCode exitCode)
        {
            var restRequest = new RestRequest($"SetConnectionExitCode?connectionId={connectionId}&IPAddress={IPAddress}&vmGuid={vmGuid}&exitCode={exitCode}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to call SetConnectionExitCode; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
            return JsonConvert.DeserializeObject<bool>(restResponse.Content);
        }
    }
}