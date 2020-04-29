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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Myrtille.Services.Contracts;

namespace Myrtille.Web
{
    /// <summary>
    /// Provides the ability to capture a remote session display
    /// </summary>
    public class CaptureController : ApiController
    {
        private ICaptureService _captureService;

        public CaptureController(ICaptureService captureService)
        {
            _captureService = captureService;
        }

        public CaptureController()
        {
            _captureService = new CaptureService();
        }

        [HttpGet]
        public void SetScreenshotConfig(Guid connectionId, int intervalSecs, CaptureFormat format, string path)
        {
            if (connectionId == Guid.Empty)
            {
                throw new ArgumentException(nameof(connectionId));
            }

            if (intervalSecs <= 0)
            {
                throw new ArgumentException(nameof(intervalSecs));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            _captureService.SetScreenshotConfig(connectionId, intervalSecs, format, path);
        }

        [HttpGet]
        public void StartTakingScreenshots(Guid connectionId)
        {
            if (connectionId == Guid.Empty)
            {
                throw new ArgumentException(nameof(connectionId));                
            }

            _captureService.StartTakingScreenshots(connectionId);
        }

        [HttpGet]
        public void StopTakingScreenshots(Guid connectionId)
        {
            if (connectionId == Guid.Empty)
            {
                throw new ArgumentException(nameof(connectionId));
            }

            _captureService.StopTakingScreenshots(connectionId);
        }

        [HttpGet]
        public HttpResponseMessage TakeScreenshot(Guid connectionId)
        {
            if (connectionId == Guid.Empty)
            {
                throw new ArgumentException(nameof(connectionId));
            }

            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(_captureService.TakeScreenshot(connectionId));
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return result;
        }
    }
}