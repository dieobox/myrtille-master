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
using System.Web.Http;
using Myrtille.Services.Contracts;

namespace Myrtille.Admin.Services
{
    /// <summary>
    /// Provides the Myrtille gateway with connection information functionality
    /// </summary>
    [RoutePrefix("ConnectionService")]
    public class ConnectionController : ApiController
    {
        private readonly IConnectionService _connectionService;

        public ConnectionController(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public ConnectionController()
        {
            _connectionService = new ConnectionService();
        }

        [HttpPost]
        [Route("GetConnectionId")]
        public Guid GetConnectionId(ConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
            {
                throw new ArgumentException(nameof(connectionInfo));
            }

            return _connectionService.GetConnectionId(connectionInfo);
        }

        [HttpGet]
        [Route("GetConnectionInfo")]
        public ConnectionInfo GetConnectionInfo(Guid connectionId)
        {
            if (connectionId == Guid.Empty)
            {
                throw new ArgumentException(nameof(connectionId));                
            }

            return _connectionService.GetConnectionInfo(connectionId);
        }

        [HttpGet]
        [Route("IsUserAllowedToConnectHost")]
        public bool IsUserAllowedToConnectHost(string domain, string userName, string hostIPAddress, Guid VMGuid)
        {
            // domain can be null (if there is no domain for the connection)
            //if (string.IsNullOrWhiteSpace(domain))
            //{
            //    throw new ArgumentException(nameof(domain));
            //}

            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException(nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(hostIPAddress) && VMGuid == Guid.Empty)
            {
                throw new ArgumentException("hostIPAddress and VMGuid cannot both be null");
            }

            return _connectionService.IsUserAllowedToConnectHost(domain, userName, hostIPAddress, VMGuid);
        }

        [HttpGet]
        [Route("SetConnectionState")]
        public bool SetConnectionState(Guid connectionId, string IPAddress, Guid vmGuid, RemoteSessionState state)
        {
            if (string.IsNullOrWhiteSpace(IPAddress) && vmGuid == Guid.Empty)
            {
                throw new ArgumentException("ipAddress and VmGuid cannot both be null");
            }

            if (!Enum.IsDefined(typeof(RemoteSessionState), state))
            {
                throw new ArgumentException(nameof(state));
            }

            return _connectionService.SetConnectionState(connectionId, IPAddress, vmGuid, state);
        }

        [HttpGet]
        [Route("SetConnectionExitCode")]
        public bool SetConnectionExitCode(Guid connectionId, string IPAddress, Guid vmGuid, RemoteSessionExitCode exitCode)
        {
            if (string.IsNullOrWhiteSpace(IPAddress) && vmGuid == Guid.Empty)
            {
                throw new ArgumentException("ipAddress and VmGuid cannot both be null");
            }

            // the exit code enum list is not exhaustive (wfreerdp returns a GetLastError() integer and I wasn't able to test every possible ways for the process to exit or crash)
            // you may want to allow an (int) exit code which is not present into the enum
            // if/when you identify its meaning, you can add it into the enum
            //if (!Enum.IsDefined(typeof(RemoteSessionExitCode), exitCode))
            //{
            //    throw new ArgumentException(nameof(exitCode));
            //}

            return _connectionService.SetConnectionExitCode(connectionId, IPAddress, vmGuid, exitCode);
        }
    }
}