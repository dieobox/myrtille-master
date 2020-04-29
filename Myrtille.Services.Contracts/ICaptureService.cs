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

namespace Myrtille.Services.Contracts
{
    public interface ICaptureService
    {
        /// <summary>
        /// screenshot config for the remote session
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="intervalSecs"></param>
        /// <param name="format"></param>
        /// <param name="path">the location to save the screenshot (i.e: a network share); the screenshot filename will be prefixed by the connection id, followed by a timestamp</param>
        /// <remarks>
        /// CAUTION! the screenshot will be saved by the wfreerdp process responsible for the connection; ensure the machine running myrtille have write access to the output location!
        /// </remarks>
        /// <returns></returns>
        void SetScreenshotConfig(Guid connectionId, int intervalSecs, CaptureFormat format, string path);

        /// <summary>
        /// start taking screenshots of the remote session (based on the configured interval)
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        void StartTakingScreenshots(Guid connectionId);

        /// <summary>
        /// stop taking screenshots of the remote session
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        void StopTakingScreenshots(Guid connectionId);

        /// <summary>
        /// take a screenshot of the remote session
        /// </summary>
        /// <param name="connectionId"></param>
        /// <remarks>
        /// CAUTION! the screenshot will be saved by the wfreerdp process responsible for the connection; ensure the machine running myrtille have write access to the output location!
        /// </remarks>
        /// <returns></returns>
        byte[] TakeScreenshot(Guid connectionId);
    }
}