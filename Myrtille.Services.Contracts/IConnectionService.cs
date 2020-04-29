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
    public interface IConnectionService
    {
        /// <summary>
        /// Gets a connection identifier to initiate a connection with Myrtille
        /// </summary>
        /// <param name="connectionInfo"></param>
        /// <remarks>
        /// this method is called by the portal mock to acquire a connection identifier for each iframe in the page
        /// </remarks>
        /// <returns></returns>
        Guid GetConnectionId(ConnectionInfo connectionInfo);

        /// <summary>
        /// Gets the ConnectionInfo by ConnectionId that is passed in the Myrtille Url query string.
        /// </summary>
        /// <param name="connectionId">The ConnectionId to return ConnectionInfo for.</param>
        /// <returns>ConnectionInfo or returns null if not found.</returns>
        ConnectionInfo GetConnectionInfo(Guid connectionId);

        /// <summary>
        /// Determines whether the given user is allowed to connect to the given server
        /// </summary>
        /// <param name="domain">the user domain</param>
        /// <param name="userName">the username</param>
        /// <param name="hostIPAddress">the server</param>
        /// <param name="VMGuid">the vm guid</param>
        /// <remarks>
        /// Provide either ipAddress or vmGuid
        /// </remarks>
        /// <returns>True if the user can connect, false if not</returns>
        bool IsUserAllowedToConnectHost(string domain, string userName, string hostIPAddress, Guid vmGuid);

        /// <summary>
        /// Sets the connection state of a Myrtille session
        /// </summary>
        /// <param name="connectionId">connection id session - note this will be ignored</param>
        /// <param name="IPAddress">ip address for the session</param>
        /// <param name="vmGuid">the vm guid</param>
        /// <param name="state">state to set</param>
        /// <remarks>
        /// connectionId is unused, please ensure you set either ipAddress or vmGuid
        /// </remarks>
        /// <returns>True if the set was successful, false if not</returns>
        bool SetConnectionState(Guid connectionId, string IPAddress, Guid vmGuid, RemoteSessionState state);

        /// <summary>
        /// Sets the remote session exit code for the given Ip Address.
        /// </summary>
        /// <param name="connectionId">The connection id to set the exit code for - note this will be ignored</param>
        /// <param name="IPAddress">ip address for the session</param>
        /// <param name="vmGuid">the vm guid</param>
        /// <param name="exitCode">The WFreeRdp exit code to set for the remote session - note this will be an integer value if not defined into the enum (the list is not exhaustive!)</param>
        /// <remarks>
        /// connectionId is unused, please ensure you set either ipAddress or vmGuid
        /// </remarks>
        /// <returns>True if the exit code was set and false if this failed.</returns>
        bool SetConnectionExitCode(Guid connectionId, string IPAddress, Guid vmGuid, RemoteSessionExitCode exitCode);
    }
}