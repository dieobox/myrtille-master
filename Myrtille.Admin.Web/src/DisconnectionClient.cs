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
using Newtonsoft.Json;
using RestSharp;

namespace Myrtille.Admin.Web
{
    public class DisconnectionClient
    {
        private static RestClient restClient;

        public DisconnectionClient(string disconnectionServiceUrl)
        {
            restClient = new RestClient(disconnectionServiceUrl);
        }

        public bool Disconnect(Guid connectionId)
        {
            var restRequest = new RestRequest($"Disconnect?connectionId={connectionId}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to call Disconnect; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
            return JsonConvert.DeserializeObject<bool>(restResponse.Content);
        }

        public bool DisconnectAll()
        {
            var restRequest = new RestRequest("DisconnectAll", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to call DisconnectAll; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
            return JsonConvert.DeserializeObject<bool>(restResponse.Content);
        }
    }
}