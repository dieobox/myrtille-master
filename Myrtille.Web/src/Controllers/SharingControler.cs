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
using System.Web.Http;
using Myrtille.Services.Contracts;

namespace Myrtille.Web
{
    /// <summary>
    /// Provides the ability to share a remote session
    /// </summary>
    public class SharingController : ApiController
    {
        private ISharingService _sharingService;

        public SharingController(ISharingService sharingService)
        {
            _sharingService = sharingService;
        }

        public SharingController()
        {
            _sharingService = new SharingService();
        }

        [HttpGet]
        public Guid AddGuest(Guid connectionId, bool allowControl)
        {
            if (connectionId == Guid.Empty)
            {
                throw new ArgumentException(nameof(connectionId));
            }

            return _sharingService.AddGuest(connectionId, allowControl);
        }

        [HttpGet]
        public List<GuestInfo> GetGuests(Guid connectionId)
        {
            if (connectionId == Guid.Empty)
            {
                throw new ArgumentException(nameof(connectionId));
            }

            return _sharingService.GetGuests(connectionId);
        }

        [HttpGet]
        public GuestInfo GetGuest(Guid guestId)
        {
            if (guestId == Guid.Empty)
            {
                throw new ArgumentException(nameof(guestId));
            }

            return _sharingService.GetGuest(guestId);
        }

        [HttpGet]
        public GuestInfo UpdateGuest(Guid guestId, bool allowControl)
        {
            if (guestId == Guid.Empty)
            {
                throw new ArgumentException(nameof(guestId));
            }

            return _sharingService.UpdateGuest(guestId, allowControl);
        }

        [HttpGet]
        public bool RemoveGuest(Guid guestId)
        {
            if (guestId == Guid.Empty)
            {
                throw new ArgumentException(nameof(guestId));
            }

            return _sharingService.RemoveGuest(guestId);
        }
    }
}