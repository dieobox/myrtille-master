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
    public interface IDisconnectionService
    {
        /// <summary>
        /// disconnect a given remote session managed by the gateway
        /// </summary>
        /// <param name="connectionId">connection id</param>
        /// <returns>true if successful, false if not</returns>
        bool Disconnect(Guid connectionId);

        /// <summary>
        /// disconnect all the remote sessions managed by the gateway
        /// </summary>
        /// <returns>true if successful, false if not</returns>
        bool DisconnectAll();
    }
}