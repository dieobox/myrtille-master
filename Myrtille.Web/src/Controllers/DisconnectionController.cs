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

namespace Myrtille.Web
{
    /// <summary>
    /// Provides the ability to disconnect remote sessions
    /// </summary>
    public class DisconnectionController : ApiController
    {
        private IDisconnectionService _disconnectionService;

        public DisconnectionController(IDisconnectionService disconnectionService)
        {
            _disconnectionService = disconnectionService;
        }

        public DisconnectionController()
        {
            _disconnectionService = new DisconnectionService();
        }

        [HttpGet]
        public bool Disconnect(Guid connectionId)
        {
            if (connectionId == Guid.Empty)
            {
                throw new ArgumentException(nameof(connectionId));
            }

            return _disconnectionService.Disconnect(connectionId);
        }

        [HttpGet]
        public bool DisconnectAll()
        {
            return _disconnectionService.DisconnectAll();
        }
    }
}