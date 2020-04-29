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
using Myrtille.Services.Contracts;
using RestSharp;

namespace Myrtille.Admin.Web
{
    public class ConnectionClient
    {
        private static RestClient restClient;

        public ConnectionClient(string connectionServiceUrl)
        {
            restClient = new RestClient(connectionServiceUrl);
        }

        public Guid GetConnectionId(ConnectionInfo connectionInfo)
        {
            var restRequest = new RestRequest("GetConnectionId", Method.POST);
            restRequest.AddJsonBody(connectionInfo);

            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to get connection id; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }

            return JsonConvert.DeserializeObject<Guid>(restResponse.Content);
        }
    }
}