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
using System.Threading;
using System.Web;
using Myrtille.Services.Contracts;

namespace Myrtille.Web
{
    public class CaptureService : ICaptureService
    {
        public void SetScreenshotConfig(Guid connectionId, int intervalSecs, CaptureFormat format, string path)
        {
            try
            {
                HttpContext.Current.Application.Lock();

                var remoteSessions = (IDictionary<Guid, RemoteSession>)HttpContext.Current.Application[HttpApplicationStateVariables.RemoteSessions.ToString()];
                if (!remoteSessions.ContainsKey(connectionId))
                {
                    throw new Exception(string.Format("connection {0} not found", connectionId));
                }
                else
                {
                    var remoteSession = remoteSessions[connectionId];
                    remoteSession.Manager.SendCommand(RemoteSessionCommand.SetScreenshotConfig, string.Format("{0}|{1}|{2}", intervalSecs, (int)format, path));
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to set screenshot config for connection {0}, interval {1} secs, format {2}, path {3} ({4})", connectionId, intervalSecs, format, path, exc);
                throw;
            }
            finally
            {
                HttpContext.Current.Application.UnLock();
            }
        }

        public void StartTakingScreenshots(Guid connectionId)
        {
            try
            {
                HttpContext.Current.Application.Lock();

                var remoteSessions = (IDictionary<Guid, RemoteSession>)HttpContext.Current.Application[HttpApplicationStateVariables.RemoteSessions.ToString()];
                if (!remoteSessions.ContainsKey(connectionId))
                {
                    throw new Exception(string.Format("connection {0} not found", connectionId));
                }
                else
                {
                    var remoteSession = remoteSessions[connectionId];
                    remoteSession.Manager.SendCommand(RemoteSessionCommand.StartTakingScreenshots);
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to start taking screenshots for connection {0} ({1})", connectionId, exc);
                throw;
            }
            finally
            {
                HttpContext.Current.Application.UnLock();
            }
        }

        public void StopTakingScreenshots(Guid connectionId)
        {
            try
            {
                HttpContext.Current.Application.Lock();

                var remoteSessions = (IDictionary<Guid, RemoteSession>)HttpContext.Current.Application[HttpApplicationStateVariables.RemoteSessions.ToString()];
                if (!remoteSessions.ContainsKey(connectionId))
                {
                    throw new Exception(string.Format("connection {0} not found", connectionId));
                }
                else
                {
                    var remoteSession = remoteSessions[connectionId];
                    remoteSession.Manager.SendCommand(RemoteSessionCommand.StopTakingScreenshots);
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to stop taking screenshots for connection {0} ({1})", connectionId, exc);
                throw;
            }
            finally
            {
                HttpContext.Current.Application.UnLock();
            }
        }

        public byte[] TakeScreenshot(Guid connectionId)
        {
            try
            {
                HttpContext.Current.Application.Lock();

                var remoteSessions = (IDictionary<Guid, RemoteSession>)HttpContext.Current.Application[HttpApplicationStateVariables.RemoteSessions.ToString()];
                if (!remoteSessions.ContainsKey(connectionId))
                {
                    throw new Exception(string.Format("connection {0} not found", connectionId));
                }
                else
                {
                    var remoteSession = remoteSessions[connectionId];

                    RemoteSessionImage screenshot = null;

                    // one screenshot at a time
                    if (remoteSession.Manager.ScreenshotEventLock == null)
                    {
                        remoteSession.Manager.ScreenshotEventLock = new object();

                        // request a screenshot (async) and wait it (up to 10 seconds)
                        lock (remoteSession.Manager.ScreenshotEventLock)
                        {
                            remoteSession.Manager.SendCommand(RemoteSessionCommand.TakeScreenshot);
                            remoteSession.Manager.ScreenshotEventPending = true;
                            if (Monitor.Wait(remoteSession.Manager.ScreenshotEventLock, 10000))
                            {
                                screenshot = remoteSession.Manager.GetCachedUpdate(remoteSession.Manager.ScreenshotImageIdx);
                            }
                            remoteSession.Manager.ScreenshotEventPending = false;
                            Monitor.Pulse(remoteSession.Manager.ScreenshotEventLock);
                        }

                        remoteSession.Manager.ScreenshotEventLock = null;
                    }

                    return screenshot?.Data;
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to take screenshot for connection {0} ({1})", connectionId, exc);
                throw;
            }
            finally
            {
                HttpContext.Current.Application.UnLock();
            }
        }
    }
}