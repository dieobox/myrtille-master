/*
    Myrtille: A native HTML4/5 Remote Desktop Protocol client.

    Copyright(c) 2014-2020 Cedric Coste
    Copyright(c) 2018 Paul Oliver (Olive Innovations)

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

using System.Collections.Generic;

namespace Myrtille.Services.Contracts
{
    public interface IEnterpriseAdapter
    {
        void Initialize();

        /// <summary>
        /// Authenticate user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="adminGroup"></param>
        /// <param name="domain">the name of your domain (i.e. MYDOMAIN or mydomain.local) or the domain controller FQDN or IP</param>
        /// <param name="netbiosDomain">the netbios domain name (i.e. MYDOMAIN)</param>
        /// <returns></returns>
        EnterpriseSession Authenticate(string username, string password, string adminGroup, string domain, string netbiosDomain);

        /// <summary>
        /// Delete user session
        /// </summary>
        /// <param name="sessionID"></param>
        void Logout(string sessionID);

        /// <summary>
        /// Add new host to the platform
        /// </summary>
        /// <param name="editHost"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        long? AddHost(EnterpriseHostEdit editHost, string sessionID);

        /// <summary>
        /// Get host information from ID and session ID
        /// </summary>
        /// <param name="hostID"></param>
        /// <param name="sessionID"></param>
        /// <returns>Host information for connection or null if invalid hostid or sessionId specified</returns>
        EnterpriseHostEdit GetHost(long hostID, string sessionID);

        /// <summary>
        /// Update host information
        /// </summary>
        /// <param name="editHost"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        bool UpdateHost(EnterpriseHostEdit editHost, string sessionID);

        /// <summary>
        /// Delete host
        /// </summary>
        /// <param name="hostID"></param>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        bool DeleteHost(long hostID, string sessionID);

        /// <summary>
        /// Retrieve a list of hosts the user session is allowed to access
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        List<EnterpriseHost> SessionHosts(string sessionID);

        /// <summary>
        /// Get the connection details for the session and host
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="hostID"></param>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        EnterpriseConnectionDetails GetSessionConnectionDetails(string sessionID, long hostID, string sessionKey);

        /// <summary>
        /// Create a session, the session URL returned can be given to external users to connect to a specific host using a URL
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="hostID"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        string CreateUserSession(string sessionID, long hostID, string username, string password, string domain);

        /// <summary>
        /// Change password for user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <param name="domain"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        bool ChangeUserPassword(string username, string oldPassword, string newPassword, string domain);

        /// <summary>
        /// Add override credentials for specific session host
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        bool AddSessionHostCredentials(EnterpriseHostSessionCredentials credentials);
    }
}