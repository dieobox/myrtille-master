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

namespace Myrtille.Admin.Web
{
    public class SharingClient
    {
        private static RestClient restClient;

        public SharingClient(string sharingServiceUrl)
        {
            restClient = new RestClient(sharingServiceUrl);
        }

        public Guid AddGuest(Guid connectionId, bool allowControl)
        {
            var restRequest = new RestRequest($"AddGuest?connectionId={connectionId}&allowControl={allowControl}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to add guest; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
            return JsonConvert.DeserializeObject<Guid>(restResponse.Content);
        }

        public List<GuestInfo> GetGuests(Guid connectionId)
        {
            var restRequest = new RestRequest($"GetGuests?connectionId={connectionId}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to retrieve guests; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
            return JsonConvert.DeserializeObject<List<GuestInfo>>(restResponse.Content);
        }

        public GuestInfo GetGuest(Guid guestId)
        {
            var restRequest = new RestRequest($"GetGuest?guestId={guestId}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to retrieve guest; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
            return JsonConvert.DeserializeObject<GuestInfo>(restResponse.Content);
        }

        public GuestInfo UpdateGuest(Guid guestId, bool allowControl)
        {
            var restRequest = new RestRequest($"UpdateGuest?guestId={guestId}&allowControl={allowControl}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to update guest; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
            return JsonConvert.DeserializeObject<GuestInfo>(restResponse.Content);
        }

        public bool RemoveGuest(Guid guestId)
        {
            var restRequest = new RestRequest($"RemoveGuest?guestId={guestId}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to remove guest; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
            return JsonConvert.DeserializeObject<bool>(restResponse.Content);
        }
    }
}