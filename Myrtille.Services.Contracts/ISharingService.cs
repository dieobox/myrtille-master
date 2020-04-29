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

namespace Myrtille.Services.Contracts
{
    public interface ISharingService
    {
        /// <summary>
        /// create a guest for a shared remote session
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="allowControl">ability for the guest to interact with the remote session</param>
        /// <returns>Id of the newly created guest or Guid.Empty in case of any issue</returns>
        Guid AddGuest(Guid connectionId, bool allowControl);

        /// <summary>
        /// retrieve the list of guests for a shared remote session
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns>list of guests or null in case of any issue</returns>
        List<GuestInfo> GetGuests(Guid connectionId);

        /// <summary>
        /// retrieve a guest information
        /// </summary>
        /// <param name="guestId"></param>
        /// <returns>guest or null in case of any issue</returns>
        GuestInfo GetGuest(Guid guestId);

        /// <summary>
        /// update a guest, providing the ability to remove or add a previously granted control over the shared remote session
        /// </summary>
        /// <param name="guestId"></param>
        /// <param name="allowControl"></param>
        /// <returns>updated guest or null in case of any issue</returns>
        GuestInfo UpdateGuest(Guid guestId, bool allowControl);

        /// <summary>
        /// remove a guest, disconnecting it in the process
        /// </summary>
        /// <param name="guestId"></param>
        /// <returns>True if sucessfully removed, False otherwise</returns>
        bool RemoveGuest(Guid guestId);
    }
}