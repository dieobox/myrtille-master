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
using Myrtille.Services.Contracts;
using RestSharp;

namespace Myrtille.Admin.Web
{
    public class CaptureClient
    {
        private static RestClient restClient;

        public CaptureClient(string captureServiceUrl)
        {
            restClient = new RestClient(captureServiceUrl);
        }

        public void SetScreenshotConfig(Guid connectionId, int intervalSecs, CaptureFormat format, string path)
        {
            var restRequest = new RestRequest($"SetScreenshotConfig?connectionId={connectionId}&intervalSecs={intervalSecs}&format={format}&path={path}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to set screenshot config; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
        }

        public void StartTakingScreenshots(Guid connectionId)
        {
            var restRequest = new RestRequest($"StartTakingScreenshots?connectionId={connectionId}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to start taking screenshots; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
        }

        public void StopTakingScreenshots(Guid connectionId)
        {
            var restRequest = new RestRequest($"StopTakingScreenshots?connectionId={connectionId}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to stop taking screenshots; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
        }

        public byte[] TakeScreenshot(Guid connectionId)
        {
            var restRequest = new RestRequest($"TakeScreenshot?connectionId={connectionId}", Method.GET);
            var restResponse = restClient.Execute(restRequest);
            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                throw new Exception(string.Format("Failed to take screenshot; response status: {0}, code: {1}, message: {2}", restResponse.ResponseStatus, restResponse.StatusCode, restResponse.ErrorMessage));
            }
            return restResponse.RawBytes;
        }
    }
}