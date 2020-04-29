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
using System.Diagnostics;
using System.ServiceModel;
using Myrtille.Services.Contracts;

namespace Myrtille.Web
{
    public class MFAAuthenticationClient : ClientBase<IMFAAuthentication>, IMFAAuthentication
    {
        public bool GetState()
        {
            try
            {
                return Channel.GetState();
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to get mfa adapter state ({0})", exc);
                return false;
            }
        }

        public bool Authenticate(string username, string password, string clientIP = null)
        {
            try
            {
                return Channel.Authenticate(username, password, clientIP);
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to mfa authenticate user {0} ({1})", username, exc);
                return false;
            }
        }

        public string GetPromptLabel()
        {
            try
            {
                return Channel.GetPromptLabel();
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to get mfa prompt label ({0})", exc);
                return null;
            }
        }

        public string GetProviderURL()
        {
            try
            {
                return Channel.GetProviderURL();
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to get mfa provider url ({0})", exc);
                return null;
            }
        }
    }
}