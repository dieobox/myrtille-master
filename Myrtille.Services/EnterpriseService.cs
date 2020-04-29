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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Myrtille.Enterprise;
using Myrtille.Services.Contracts;

namespace Myrtille.Services
{
    public class EnterpriseService : IEnterpriseService
    {
        public EnterpriseMode GetMode()
        {
            return Program._enterpriseAdapter == null ? EnterpriseMode.None : (Program._enterpriseAdapter is LocalAdmin ? EnterpriseMode.Local : EnterpriseMode.Domain);
        }

        public EnterpriseSession Authenticate(string username, string password)
        {
            try
            {
                Trace.TraceInformation("Requesting authentication of user {0}", string.IsNullOrEmpty(Program._enterpriseNetbiosDomain) ? username : string.Format("{0}\\{1}", Program._enterpriseNetbiosDomain, username));
                return Program._enterpriseAdapter.Authenticate(username, password, Program._enterpriseAdminGroup, Program._enterpriseDomain, Program._enterpriseNetbiosDomain);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to authenticate user {0}, ({1})", username, ex);
                return null;
            }
        }

        public void Logout(string sessionID)
        {
            try
            {
                Program._enterpriseAdapter.Logout(sessionID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to logout session {0}", sessionID, ex);
            }
        }

        public long? AddHost(EnterpriseHostEdit editHost, string sessionID)
        {
            try
            {
                Trace.TraceInformation("Add host requested, host {0}", editHost.HostName);
                return Program._enterpriseAdapter.AddHost(editHost, sessionID);
            }catch(Exception ex)
            {
                Trace.TraceError("Failed to add host {0}, ({1})",editHost.HostName, ex);
                return null;
            }
        }

        public EnterpriseHostEdit GetHost(long hostID, string sessionID)
        {
            try
            {
                Trace.TraceInformation("Edit host requested, host {0}", hostID);
                return Program._enterpriseAdapter.GetHost(hostID, sessionID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to get host {0}, ({1})", hostID, ex);
                return null;
            }
        }

        public bool UpdateHost(EnterpriseHostEdit editHost, string sessionID)
        {
            try
            {
                Trace.TraceInformation("Update host requested, host {0}", editHost.HostName);
                return Program._enterpriseAdapter.UpdateHost(editHost, sessionID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to update host {0}, ({1})", editHost.HostName, ex);
                return false;
            }
        }

        public bool DeleteHost(long hostID, string sessionID)
        {
            try
            {
                Trace.TraceInformation("Deleting host");
                return Program._enterpriseAdapter.DeleteHost(hostID, sessionID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to delete host {0} ({1})", hostID, ex);
                return false;
            }
        }

        public List<EnterpriseHost> GetSessionHosts(string sessionID)
        {
            try
            {
                Trace.TraceInformation("Requesting session host list");
                return Program._enterpriseAdapter.SessionHosts(sessionID);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to get host list {0}", ex);
                return new List<EnterpriseHost>();
            }
        }

        public EnterpriseConnectionDetails GetSessionConnectionDetails(string sessionID, long hostID, string sessionKey)
        {
            try
            {
                Trace.TraceInformation("Requesting session details");
                return Program._enterpriseAdapter.GetSessionConnectionDetails(sessionID, hostID, sessionKey);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to get session connection details {0}", ex);
                return null;
            }
        }

        public string CreateUserSession(string sessionID, long hostID, string username, string password, string domain)
        {
            try
            {
                Trace.TraceInformation("Create user session requested, host {0}, user {1}", hostID, string.IsNullOrEmpty(domain) ? username : string.Format("{0}\\{1}", domain, username));
                return Program._enterpriseAdapter.CreateUserSession(sessionID, hostID, username, password, domain);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to create session {0}, ({1})", hostID, ex);
                return null;
            }
        }

        public bool ChangeUserPassword(string username, string oldPassword, string newPassword)
        {
            try
            {
                Trace.TraceInformation("Change password for user {0}", username);
                return Program._enterpriseAdapter.ChangeUserPassword(username, oldPassword, newPassword, Program._enterpriseDomain);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to change password for user {0}, ({1})", username, ex);
                return false;
            }
        }

        public bool AddSessionHostCredentials(EnterpriseHostSessionCredentials credentials)
        {
            try
            {
                Trace.TraceInformation("creating session credentials for {0}", string.IsNullOrEmpty(credentials.Domain) ? credentials.Username : string.Format("{0}\\{1}", credentials.Domain, credentials.Username));
                return Program._enterpriseAdapter.AddSessionHostCredentials(credentials);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to set session credentials for user {0}, ({1})", credentials.Username, ex);
                return false;
            }
        }
    }
}