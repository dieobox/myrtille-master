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
    public class SharingService : ISharingService
    {
        public Guid AddGuest(Guid connectionId, bool allowControl)
        {
            var guestId = Guid.Empty;

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
                    if (remoteSession.State != RemoteSessionState.Connected)
                    {
                        throw new Exception(string.Format("remote session {0} is not connected", connectionId));
                    }
                    else
                    {
                        var sharedSessions = (IDictionary<Guid, SharingInfo>)HttpContext.Current.Application[HttpApplicationStateVariables.SharedRemoteSessions.ToString()];
                        var sharingInfo = new SharingInfo
                        {
                            RemoteSession = remoteSession,
                            GuestInfo = new GuestInfo
                            {
                                Id = Guid.NewGuid(),
                                ConnectionId = remoteSession.Id,
                                Control = allowControl
                            }
                        };
                        sharedSessions.Add(sharingInfo.GuestInfo.Id, sharingInfo);
                        guestId = sharingInfo.GuestInfo.Id;
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to add a guest for connection {0} ({1})", connectionId, exc);
            }
            finally
            {
                HttpContext.Current.Application.UnLock();
            }

            return guestId;
        }

        public List<GuestInfo> GetGuests(Guid connectionId)
        {
            List<GuestInfo> guests = null;

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
                    if (remoteSession.State != RemoteSessionState.Connected)
                    {
                        throw new Exception(string.Format("remote session {0} is not connected", connectionId));
                    }
                    else
                    {
                        guests = new List<GuestInfo>();
                        var sharedSessions = (IDictionary<Guid, SharingInfo>)HttpContext.Current.Application[HttpApplicationStateVariables.SharedRemoteSessions.ToString()];
                        foreach (var sharingInfo in sharedSessions.Values)
                        {
                            if (sharingInfo.RemoteSession.Id == connectionId)
                            {
                                guests.Add(sharingInfo.GuestInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to retrieve the list of guests for connection {0} ({1})", connectionId, exc);
            }
            finally
            {
                HttpContext.Current.Application.UnLock();
            }

            return guests;
        }

        public GuestInfo GetGuest(Guid guestId)
        {
            GuestInfo guest = null;

            try
            {
                HttpContext.Current.Application.Lock();

                var sharedSessions = (IDictionary<Guid, SharingInfo>)HttpContext.Current.Application[HttpApplicationStateVariables.SharedRemoteSessions.ToString()];
                if (!sharedSessions.ContainsKey(guestId))
                {
                    throw new Exception(string.Format("guest {0} not found", guestId));
                }
                else
                {
                    var sharingInfo = sharedSessions[guestId];
                    if (sharingInfo.RemoteSession.State != RemoteSessionState.Connected)
                    {
                        throw new Exception(string.Format("remote session {0} is not connected", sharingInfo.RemoteSession.Id));
                    }
                    else
                    {
                        guest = sharingInfo.GuestInfo;
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to retrieve information for guest {0} ({1})", guestId, exc);
            }
            finally
            {
                HttpContext.Current.Application.UnLock();
            }

            return guest;
        }

        public GuestInfo UpdateGuest(Guid guestId, bool allowControl)
        {
            GuestInfo guest = null;

            try
            {
                HttpContext.Current.Application.Lock();

                var sharedSessions = (IDictionary<Guid, SharingInfo>)HttpContext.Current.Application[HttpApplicationStateVariables.SharedRemoteSessions.ToString()];
                if (!sharedSessions.ContainsKey(guestId))
                {
                    throw new Exception(string.Format("guest {0} not found", guestId));
                }
                else
                {
                    var sharingInfo = sharedSessions[guestId];
                    if (sharingInfo.RemoteSession.State != RemoteSessionState.Connected)
                    {
                        throw new Exception(string.Format("remote session {0} is not connected", sharingInfo.RemoteSession.Id));
                    }
                    else
                    {
                        guest = sharingInfo.GuestInfo;
                        guest.Control = allowControl;
                        if (guest.Active && sharingInfo.HttpSession != null)
                        {
                            sharingInfo.HttpSession[HttpSessionStateVariables.GuestInfo.ToString()] = guest;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to update guest {0} ({1})", guestId, exc);
            }
            finally
            {
                HttpContext.Current.Application.UnLock();
            }

            return guest;
        }

        public bool RemoveGuest(Guid guestId)
        {
            var success = false;

            try
            {
                HttpContext.Current.Application.Lock();

                var sharedSessions = (IDictionary<Guid, SharingInfo>)HttpContext.Current.Application[HttpApplicationStateVariables.SharedRemoteSessions.ToString()];
                if (!sharedSessions.ContainsKey(guestId))
                {
                    throw new Exception(string.Format("guest {0} not found", guestId));
                }
                else
                {
                    var sharingInfo = sharedSessions[guestId];
                    if (sharingInfo.RemoteSession.State != RemoteSessionState.Connected)
                    {
                        throw new Exception(string.Format("remote session {0} is not connected", sharingInfo.RemoteSession.Id));
                    }
                    else
                    {
                        if (sharingInfo.GuestInfo.Active && sharingInfo.HttpSession != null)
                        {
                            sharingInfo.HttpSession[HttpSessionStateVariables.RemoteSession.ToString()] = null;
                            sharingInfo.HttpSession[HttpSessionStateVariables.GuestInfo.ToString()] = null;

                            if (sharingInfo.RemoteSession.ActiveGuests > 0)
                            {
                                sharingInfo.RemoteSession.ActiveGuests--;
                            }

                            // have the removed guest back to the login page
                            sharingInfo.RemoteSession.Manager.SendMessage(new RemoteSessionMessage { Type = MessageType.PageReload, Prefix = "reload" });
                        }
                        sharedSessions.Remove(guestId);
                        success = true;
                    }
                }
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to remove guest {0} ({1})", guestId, exc);
            }
            finally
            {
                HttpContext.Current.Application.UnLock();
            }

            return success;
        }
    }
}