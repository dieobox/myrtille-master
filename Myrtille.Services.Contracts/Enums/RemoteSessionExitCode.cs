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

namespace Myrtille.Services.Contracts
{
    // these are some wfreerdp exit codes (not exhaustive list)
    // as a remote session is protocol agnostic for the gateway, use the same exit codes for any host type

    public enum RemoteSessionExitCode
    {
        Success = 0,
        ProcessKilled = -1,
        ProcessKilledFromTaskManager = 1,
        SessionDisconnectFromConsole = 65537,
        SessionLogoutFromConsole = 65538,
        SessionIdleTimeout = 65539,
        SessionMaxTime = 65540,
        SessionDisconnectFromMenu = 65547,
        SessionLogoutFromMenu = 65548,
        InvalidServerAddress = 131077,
        InvalidSecurityProtocol = 131084,
        MissingUserName = 131083,
        MissingPassword = 131085,
        InvalidCredentials = 131092,
        InvalidConfiguration = 888888,
        Unknown = 999999
    }
}