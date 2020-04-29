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
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using Myrtille.Helpers;
using Myrtille.Services.Contracts;

namespace Myrtille.Enterprise
{
    public class ActiveDirectory : IEnterpriseAdapter
    {
        public void Initialize()
        {
            using (var db = new MyrtilleEnterpriseDBContext())
            {
                db.Session.RemoveRange(db.Session);
                db.SaveChanges();
            }
        }

        public EnterpriseSession Authenticate(string username, string password, string adminGroup, string domain, string netbiosDomain)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, domain, username, password))
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);

                    DirectoryEntry entry = (DirectoryEntry)user.GetUnderlyingObject();

                    if(user.IsAccountLockedOut())
                    {
                        return new EnterpriseSession
                        {
                            AuthenticationErrorCode = EnterpriseAuthenticationErrorCode.USER_ACCOUNT_LOCKED
                        };
                    }

                    if(user.Enabled != null && !(bool)user.Enabled)
                    {
                        return new EnterpriseSession
                        {
                            AuthenticationErrorCode = EnterpriseAuthenticationErrorCode.ACCOUNT_DISABLED
                        };
                    }

                    if(user.AccountExpirationDate != null && (DateTime)user.AccountExpirationDate <= DateTime.Now)
                    {
                        return new EnterpriseSession
                        {
                            AuthenticationErrorCode = EnterpriseAuthenticationErrorCode.ACCOUNT_EXPIRED
                        };
                    }

                    if (!user.PasswordNeverExpires )//&& !user.UserCannotChangePassword)
                    {
                        var expDate = (DateTime)entry.InvokeGet("PasswordExpirationDate");
                        // if the expiration date is not set, its default value is 1970/01/01
                        if (expDate <= DateTime.Now && expDate > new DateTime(1970, 1, 1))
                        {
                            return new EnterpriseSession
                            {
                                AuthenticationErrorCode = EnterpriseAuthenticationErrorCode.PASSWORD_EXPIRED
                            };
                        }
                    }


                    var directoryGroups = new List<string>();

                    try
                    {
                        directoryGroups.AddRange(user.GetGroups().Select(m => m.Name).ToList<string>());
                    }
                    catch (Exception e)
                    {
                        //There is an issue accessing user primary ad group remotely, 
                        //Exception: Information about the domain could not be retrieved (1355).
                        //in that case use another method which will exclude the primary domain 
                        // might need to find another way to do this!
                        directoryGroups.AddRange(GetDirectoryGroups(entry));
                    }

                    //Add user to directory group to allow restriction to host to specific username
                    directoryGroups.Add(username);

                    bool isAdmin = directoryGroups.Any(m => m.Equals(adminGroup, StringComparison.InvariantCultureIgnoreCase));

                    string sessionID = Guid.NewGuid().ToString();
                    string sessionKey = Guid.NewGuid().ToString("n");
                    using (var db = new MyrtilleEnterpriseDBContext())
                    {
                        var session = db.Session.FirstOrDefault(m => m.Username == username);
                        if (session != null)
                        {
                            db.Session.Remove(session);
                            db.SaveChanges();
                        }

                        session = new Session
                        {
                            Domain = netbiosDomain,
                            Username = username,
                            Password = CryptoHelper.AES_Encrypt(CryptoHelper.RDP_Encrypt(password), sessionKey),
                            SessionID = sessionID,
                            IsAdmin = isAdmin
                        };

                        db.Session.Add(session);
                        db.SaveChanges();

                        var groups = directoryGroups.Select(x => new SessionGroup
                        {
                            SessionID = session.ID,
                            DirectoryGroup = x
                        });

                        db.SessionGroup.AddRange(groups);
                        db.SaveChanges();
                        return new EnterpriseSession
                        {
                            Domain = netbiosDomain,
                            UserName = username,
                            SessionID = sessionID,
                            SessionKey = sessionKey,
                            IsAdmin = isAdmin,
                            SingleUseConnection = false
                        };
                    }
                }
            }
            catch(DirectoryServicesCOMException e)
            {

                var formattedError = (DirectoryExceptionHelper)e;

                return new EnterpriseSession
                {
                    AuthenticationErrorCode = formattedError.ErrorCode
                };
            }
            catch(PrincipalOperationException e)
            {
                return null;
            }
            catch (Exception e)
            {
                return new EnterpriseSession
                {
                    AuthenticationErrorCode = EnterpriseAuthenticationErrorCode.UNKNOWN_ERROR
                };
            }
        }

        public void Logout(string sessionID)
        {
            using (var db = new MyrtilleEnterpriseDBContext())
            {
                var session = db.Session.FirstOrDefault(m => m.SessionID == sessionID);
                if (session != null)
                {
                    db.Session.Remove(session);
                    db.SaveChanges();
                }
            }
        }

        public long? AddHost(EnterpriseHostEdit editHost, string sessionID)
        {
            using (var db = new MyrtilleEnterpriseDBContext())
            {
                if (!db.Session.Any(m => m.SessionID == sessionID && m.IsAdmin && m.Expire > DateTime.Now)) return null;

                if (db.Host.Any(m => m.HostName.Equals(editHost.HostName,StringComparison.InvariantCultureIgnoreCase))) return null;

                List<string> groups = editHost.DirectoryGroups.Split(',').ToList();

                var host = new Host
                {
                    HostName = editHost.HostName,
                    HostAddress = editHost.HostAddress,
                    VMGuid = editHost.VMGuid,
                    VMEnhancedMode = editHost.VMEnhancedMode,
                    Protocol = editHost.Protocol,
                    HostType = editHost.HostType,
                    StartRemoteProgram = editHost.StartRemoteProgram,
                    PromptForCredentials = editHost.PromptForCredentials
                };

                db.Host.Add(host);

                db.SaveChanges();

                var hostAccess = groups.Select(x => new HostAccessGroups
                {
                    HostID = host.ID,
                    AccessGroup = x.Trim()
                });

                db.HostAccessGroups.AddRange(hostAccess.Where(m => m.AccessGroup != ""));
                db.SaveChanges();
                return host.ID;
            }
        }

        public EnterpriseHostEdit GetHost(long hostID, string sessionID)
        {
            using (var db = new MyrtilleEnterpriseDBContext())
            {
                if (!db.Session.Any(m => m.SessionID == sessionID && m.IsAdmin && m.Expire > DateTime.Now)) return null;

                var host = db.Host.FirstOrDefault(m => m.ID == hostID);

                if (host == null) return null;

                var directoryGroupList = db.HostAccessGroups
                            .Where(m => m.HostID == hostID)
                            .Select(m => m.AccessGroup)
                            .ToList();

                StringBuilder directoryGroups = new StringBuilder();
                var isFirst = true;
                foreach (string group in directoryGroupList)
                {
                    if (!isFirst) directoryGroups.Append(", ");
                    isFirst = false;
                    directoryGroups.Append(group);
                }
                return new EnterpriseHostEdit
                {
                    HostID = host.ID,
                    HostName = host.HostName,
                    HostAddress = host.HostAddress,
                    VMGuid = host.VMGuid,
                    VMEnhancedMode = host.VMEnhancedMode,
                    DirectoryGroups = directoryGroups.ToString(),
                    Protocol = host.Protocol,
                    HostType = host.HostType,
                    StartRemoteProgram = host.StartRemoteProgram,
                    PromptForCredentials = host.PromptForCredentials
                };
            }
        }

        public bool UpdateHost(EnterpriseHostEdit editHost, string sessionID)
        {
            using (var db = new MyrtilleEnterpriseDBContext())
            {
                if (!db.Session.Any(m => m.SessionID == sessionID && m.IsAdmin && m.Expire > DateTime.Now)) return false;

                if (db.Host.Any(m => m.HostName.Equals(editHost.HostName, StringComparison.InvariantCultureIgnoreCase) && m.ID != editHost.HostID)) return false;

                var host = db.Host.FirstOrDefault(m => m.ID == editHost.HostID);

                if (host == null) return false;

                host.HostName = editHost.HostName;
                host.HostAddress = editHost.HostAddress;
                host.VMGuid = editHost.VMGuid;
                host.VMEnhancedMode = editHost.VMEnhancedMode;
                host.Protocol = editHost.Protocol;
                host.StartRemoteProgram = editHost.StartRemoteProgram;
                host.PromptForCredentials = editHost.PromptForCredentials;

                var currentGroups = db.HostAccessGroups
                                        .Where(m => m.HostID == editHost.HostID)
                                        .ToList();

                IEnumerable<string> groups = editHost.DirectoryGroups.Split(',').ToList();

                var hostsToDelete = currentGroups.Where(m => !groups.Any(p => p.Equals(m.AccessGroup, StringComparison.InvariantCultureIgnoreCase)));

                db.HostAccessGroups.RemoveRange(hostsToDelete);

                var hostAccess = groups
                                    .Where(m => !currentGroups.Any(p => p.AccessGroup.Equals(m,StringComparison.InvariantCultureIgnoreCase)))
                                    .Select(x => new HostAccessGroups
                                        {
                                            HostID = host.ID,
                                            AccessGroup = x.Trim()
                                        });

                db.HostAccessGroups.AddRange(hostAccess.Where(m => m.AccessGroup != ""));

                db.SaveChanges();

                return true;
            }
        }

        public bool DeleteHost(long hostID, string sessionID)
        {
            using (var db = new MyrtilleEnterpriseDBContext())
            {
                if (!db.Session.Any(m => m.SessionID == sessionID && m.IsAdmin && m.Expire > DateTime.Now)) return false;

                var host = db.Host.FirstOrDefault(m => m.ID == hostID);

                if (host == null) return false;

                db.Host.Remove(host);
                db.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Get a list of active directory groups for a user
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        private List<string> GetDirectoryGroups(DirectoryEntry entry)
        {
            var directoryGroups = new List<string>();

            var groups = entry.Properties["memberOf"];

            foreach (string group in groups)
            {
                var startIndex = group.IndexOf("CN=");

                if (startIndex < 0)
                    continue;
                else
                    startIndex += 3;

                var endIndex = group.IndexOf("=", startIndex);

                if (endIndex < 0)
                    endIndex = group.Length - 3;
                else
                    endIndex = endIndex - 6;

                var length = endIndex - startIndex;

                directoryGroups.Add(group.Substring(startIndex, endIndex));
            }

            return directoryGroups;
        }

        public List<EnterpriseHost> SessionHosts(string sessionID)
        {
            using (var db = new MyrtilleEnterpriseDBContext())
            {
                var sessionInfo = db.Session
                                    .Where(m => m.SessionID == sessionID && m.Expire > DateTime.Now)
                                    .Select(m => new
                                    {
                                        SessionID = m.SessionID,
                                        IsAdmin = m.IsAdmin
                                    })
                                    .FirstOrDefault();

                if (sessionInfo == null) return new List<EnterpriseHost>();

                if (sessionInfo.IsAdmin)
                {
                    return (from s in db.Session
                            from h in db.Host 
                            where s.SessionID == sessionID
                            && s.Expire > DateTime.Now
                            select new EnterpriseHost
                            {
                                HostID = h.ID,
                                HostName = h.HostName,
                                HostAddress = h.HostAddress,
                                VMGuid = h.VMGuid,
                                VMEnhancedMode = h.VMEnhancedMode,
                                HostType = h.HostType,
                                StartRemoteProgram = h.StartRemoteProgram,
                                PromptForCredentials = h.PromptForCredentials
                            })
                        .Distinct()
                        .OrderBy(m => m.HostName)
                        .ToList();
                }
                else
                {
                    return (from s in db.Session
                            join sg in db.SessionGroup on s.ID equals sg.SessionID
                            join hag in db.HostAccessGroups on sg.DirectoryGroup equals hag.AccessGroup
                            join h in db.Host on hag.HostID equals h.ID
                            where s.SessionID == sessionID
                            && s.Expire > DateTime.Now
                            select new EnterpriseHost
                            {
                                HostID = h.ID,
                                HostName = h.HostName,
                                HostAddress = h.HostAddress,
                                VMGuid = h.VMGuid,
                                VMEnhancedMode = h.VMEnhancedMode,
                                HostType = h.HostType,
                                StartRemoteProgram = h.StartRemoteProgram,
                                PromptForCredentials = h.PromptForCredentials
                            })
                            .Distinct()
                            .OrderBy(m => m.HostName)
                            .ToList();
                }
            }
        }

        public EnterpriseConnectionDetails GetSessionConnectionDetails(string sessionID, long hostID, string sessionKey)
        {
            using (var db = new MyrtilleEnterpriseDBContext())
            {
                var sessionInfo = db.Session
                                .Where(m => m.SessionID == sessionID && m.Expire > DateTime.Now)
                                .Select(m => new
                                {
                                    SessionID = m.SessionID,
                                    OneTime = m.OneTime,
                                    IsAdmin = m.IsAdmin
                                })
                                .FirstOrDefault();

                EnterpriseConnectionDetails result = null;
                if (sessionInfo != null)
                {
                    if (sessionInfo.OneTime)
                    {
                        result = (from s in db.Session
                                    from h in db.Host
                                    where s.SessionID == sessionID
                                        && h.ID == hostID
                                        && s.Expire > DateTime.Now
                                    select new EnterpriseConnectionDetails
                                    {
                                        HostID = h.ID
                                        ,
                                        HostName = h.HostName
                                        ,
                                        HostAddress = h.HostAddress
                                        ,
                                        VMGuid = h.VMGuid
                                        ,
                                        VMEnhancedMode = h.VMEnhancedMode
                                        ,
                                        HostType = h.HostType
                                        ,
                                        Domain = s.Domain
                                        ,
                                        Username = s.Username
                                        ,
                                        Password = s.Password
                                        ,
                                        Protocol = h.Protocol
                                        ,
                                        StartRemoteProgram = h.StartRemoteProgram
                                    })
                                .FirstOrDefault();
                    }
                    else
                    {
                        if (sessionInfo.IsAdmin)
                        {
                            result = (from s in db.Session
                                      from h in db.Host
                                      join sc in db.SessionHostCredentials on new { x1 = s.ID, x2 = h.ID } equals new { x1 = sc.SessionID, x2 = sc.HostID } into scl
                                      from sc in scl.DefaultIfEmpty()
                                      where s.SessionID == sessionID
                                          && h.ID == hostID
                                          && s.Expire > DateTime.Now
                                      select new EnterpriseConnectionDetails
                                      {
                                          HostID = h.ID
                                          ,
                                          HostName = h.HostName
                                          ,
                                          HostAddress = h.HostAddress
                                          ,
                                          VMGuid = h.VMGuid
                                          ,
                                          VMEnhancedMode = h.VMEnhancedMode
                                          ,
                                          HostType = h.HostType
                                          ,
                                          Domain = (h.PromptForCredentials ? sc.Domain : s.Domain)
                                          ,
                                          Username = (h.PromptForCredentials ? sc.Username : s.Username)
                                          ,
                                          Password = (h.PromptForCredentials ? sc.Password : s.Password)
                                          ,
                                          Protocol = h.Protocol
                                          ,
                                          StartRemoteProgram = h.StartRemoteProgram
                                      })
                                    .FirstOrDefault();
                        }
                        else
                        {
                            result = (from s in db.Session
                                      join sg in db.SessionGroup on s.ID equals sg.SessionID
                                      join hag in db.HostAccessGroups on sg.DirectoryGroup equals hag.AccessGroup
                                      join h in db.Host on hag.HostID equals h.ID
                                      join sc in db.SessionHostCredentials on new { x1 = s.ID, x2 = h.ID } equals new { x1 = sc.SessionID, x2 = sc.HostID } into scl
                                      from sc in scl.DefaultIfEmpty()
                                      where s.SessionID == sessionID
                                          && h.ID == hostID
                                          && s.Expire > DateTime.Now
                                      select new EnterpriseConnectionDetails
                                      {
                                          HostID = h.ID
                                          ,
                                          HostName = h.HostName
                                          ,
                                          HostAddress = h.HostAddress
                                          ,
                                          VMGuid = h.VMGuid
                                          ,
                                          VMEnhancedMode = h.VMEnhancedMode
                                          ,
                                          HostType = h.HostType
                                          ,
                                          Domain = (h.PromptForCredentials ? sc.Domain : s.Domain)
                                          ,
                                          Username = (h.PromptForCredentials ? sc.Username : s.Username)
                                          ,
                                          Password = (h.PromptForCredentials ? sc.Password : s.Password)
                                          ,
                                          Protocol = h.Protocol
                                          ,
                                          StartRemoteProgram = h.StartRemoteProgram
                                      })
                                    .FirstOrDefault();
                        }
                    }

                    if (result != null)
                    {
                        result.Password = CryptoHelper.AES_Decrypt(result.Password, sessionKey);
                    }

                    // when connected from the login page, the session logout is based on expiration or user action
                    // when connected from a one time url, the logout is done immediately
                    if (sessionInfo.OneTime)
                    {
                        Logout(sessionID);
                    }
                }

                return result;
            }
        }

        public string CreateUserSession(string sessionID, long hostID, string username, string password, string domain)
        {
            using (var db = new MyrtilleEnterpriseDBContext())
            {
                if (!db.Session.Any(m => m.SessionID == sessionID && m.IsAdmin && m.Expire > DateTime.Now)) return null;

                if (!db.Host.Any(m => m.ID == hostID)) return null;

                string newSessionID = Guid.NewGuid().ToString();
                string sessionKey = Guid.NewGuid().ToString("n");

                var session = new Session
                {
                    Domain = domain,
                    Username = username,
                    Password = CryptoHelper.AES_Encrypt(CryptoHelper.RDP_Encrypt(password), sessionKey),
                    SessionID = newSessionID,
                    IsAdmin = false,
                    Expire = DateTime.Now.AddHours(1),
                    OneTime = true
                };

                db.Session.Add(session);
                db.SaveChanges();

                return string.Format("?SI={0}&SD={1}&SK={2}",newSessionID,hostID,sessionKey);
            }
        }

        public bool ChangeUserPassword(string username, string oldPassword, string newPassword, string domain)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, domain, username, oldPassword))
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
                    user.ChangePassword(oldPassword, newPassword);
                    user.Save();
                }
                return true;
            }catch(Exception e)
            {
                return false;
            }
        }

        public bool AddSessionHostCredentials(EnterpriseHostSessionCredentials credentials)
        {
            using (var db = new MyrtilleEnterpriseDBContext())
            {
                var session = db.Session.FirstOrDefault(m => m.SessionID == credentials.SessionID);

                if (session == null) return false;

                if (!db.Host.Any(m => m.ID == credentials.HostID)) return false;

                var sessionHost = db.SessionHostCredentials.FirstOrDefault(m => m.SessionID == session.ID
                                            && m.HostID == credentials.HostID);

                if(sessionHost != null)
                {
                    db.SessionHostCredentials.Remove(sessionHost);
                }

                sessionHost = new SessionHostCredential
                {
                    SessionID = session.ID,
                    HostID = credentials.HostID,
                    Domain = credentials.Domain,
                    Username = credentials.Username,
                    Password = CryptoHelper.AES_Encrypt(CryptoHelper.RDP_Encrypt(credentials.Password), credentials.SessionKey)
                };

                
                db.SessionHostCredentials.Add(sessionHost);
                db.SaveChanges();

                return true;
            }
        }
    }
}