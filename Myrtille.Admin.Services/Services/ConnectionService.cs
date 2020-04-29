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
using System.Collections;
using System.Diagnostics;
using Myrtille.Services.Contracts;

namespace Myrtille.Admin.Services
{
    public class ConnectionService : IConnectionService
    {
        // connections are simply stored (mapped) into memory

        // TODO: this could be improved by persisting them into a database (for example) so that, if this service is restarted, connections are not lost
        // also, this service should be secured so that only Myrtille can call it; otherwise, any third party could retrieve a connection details from a connection identifier!
        // to mitigate that risk (for now), access to a connection details is only allowed once

        private static Hashtable connections = new Hashtable();

        public Guid GetConnectionId(ConnectionInfo connectionInfo)
        {
            var connection = new Connection
            {
                Id = Guid.NewGuid(),
                State = RemoteSessionState.NotConnected,
                Info = connectionInfo,
                InfoAccessed = false,
                ExitCode = null
            };

            lock (connections.SyncRoot)
            {
                connections.Add(connection.Id, connection);
            }

            Trace.TraceInformation("GetConnectionId: {0}, domain={1}, user={2}, host={3}, vm={4}", connection.Id, connection.Info.User.Domain, connection.Info.User.UserName, connection.Info.Host.IPAddress, connection.Info.VM != null ? connection.Info.VM.Guid.ToString() : string.Empty);

            return connection.Id;
        }

        public ConnectionInfo GetConnectionInfo(Guid connectionId)
        {
            var connection = connections[connectionId] as Connection;
            if (connection != null)
            {
                if (connection.InfoAccessed)
                {
                    Trace.TraceWarning("GetConnectionInfo: {0}, info was already accessed, access denied", connection.Id);
                    return null;
                }

                connection.InfoAccessed = true;
                Trace.TraceInformation("GetConnectionInfo: {0}, domain={1}, user={2}, host={3}, vm={4}", connection.Id, connection.Info.User.Domain, connection.Info.User.UserName, connection.Info.Host.IPAddress, connection.Info.VM != null ? connection.Info.VM.Guid.ToString() : string.Empty);
                return connection.Info;
            }
            else
            {
                Trace.TraceInformation("invalid connection: {0}", connectionId);
                return null;
            }
        }

        public bool IsUserAllowedToConnectHost(string domain, string userName, string hostIPAddress, Guid VMGuid)
        {
            // this method is just an empty shell
            // have your own implementation to allow or deny user access to a given host
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(hostIPAddress))
            {
                Trace.TraceInformation("IsUserAllowedToConnectHost, access granted: domain={0}, user={1}, host={2}, vm={3}", domain, userName, hostIPAddress, VMGuid != Guid.Empty ? VMGuid.ToString() : string.Empty);
                return true;
            }

            Trace.TraceInformation("IsUserAllowedToConnectHost, access denied: domain={0}, user={1}, host={2}, vm={3}", domain, userName, hostIPAddress, VMGuid != Guid.Empty ? VMGuid.ToString() : string.Empty);
            return false;
        }

        public bool SetConnectionState(Guid connectionId, string IPAddress, Guid VMGuid, RemoteSessionState state)
        {
            var connection = connections[connectionId] as Connection;
            if (connection != null)
            {
                Trace.TraceInformation("SetConnectionState: {0}, ip={1}, vm={2}, state={3}", connectionId, IPAddress, VMGuid != Guid.Empty ? VMGuid.ToString() : string.Empty, state);
                connection.State = state;
                return true;
            }

            Trace.TraceInformation("invalid connection: {0}", connectionId);
            return false;
        }

        public bool SetConnectionExitCode(Guid connectionId, string IPAddress, Guid VMGuid, RemoteSessionExitCode exitCode)
        {
            var connection = connections[connectionId] as Connection;
            if (connection != null)
            {
                Trace.TraceInformation("SetConnectionExitCode: {0}, ip={1}, vm={2}, exit code={3}", connectionId, IPAddress, VMGuid != Guid.Empty ? VMGuid.ToString() : string.Empty, exitCode);
                connection.ExitCode = exitCode;
                return true;
            }

            Trace.TraceInformation("invalid connection: {0}", connectionId);
            return false;
        }
    }
}