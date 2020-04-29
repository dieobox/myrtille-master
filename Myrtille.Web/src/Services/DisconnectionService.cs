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
using System.Web;
using Myrtille.Services.Contracts;

namespace Myrtille.Web
{
    public class DisconnectionService : IDisconnectionService
    {
        public bool Disconnect(Guid connectionId)
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
                    if (remoteSession.State == RemoteSessionState.Connecting ||
                        remoteSession.State == RemoteSessionState.Connected)
                    {
                        remoteSession.Manager.SendCommand(RemoteSessionCommand.CloseClient);
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to disconnect the remote session {0} ({1})", connectionId, exc);
                return false;
            }
            finally
            {
                HttpContext.Current.Application.UnLock();
            }

            return true;
        }

        public bool DisconnectAll()
        {
            try
            {
                HttpContext.Current.Application.Lock();

                var remoteSessions = (IDictionary<Guid, RemoteSession>)HttpContext.Current.Application[HttpApplicationStateVariables.RemoteSessions.ToString()];
                foreach (var remoteSession in remoteSessions.Values)
                {
                    if (remoteSession.State == RemoteSessionState.Connecting ||
                        remoteSession.State == RemoteSessionState.Connected)
                    {
                        remoteSession.Manager.SendCommand(RemoteSessionCommand.CloseClient);
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to disconnect all remote sessions ({0})", exc);
                return false;
            }
            finally
            {
                HttpContext.Current.Application.UnLock();
            }

            return true;
        }
    }
}