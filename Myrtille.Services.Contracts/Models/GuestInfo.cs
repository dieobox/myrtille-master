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
    public class GuestInfo
    {
        /// <summary>
        /// id of the guest
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// id of the connection the guest is bound to
        /// </summary>
        public Guid ConnectionId { get; set; }

        /// <summary>
        /// True if the guest can interact with the remote session (limited to keyboard and mouse events)
        /// </summary>
        public bool Control { get; set; }
        
        /// <summary>
        /// True if the guest is connected to the remote session (the one time sharing link was used)
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// True if the guest uses a websocket connection to the myrtille gateway
        /// </summary>
        public bool Websocket { get; set; }
    }
}