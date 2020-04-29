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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Myrtille.Services.ConnectionBroker
{
    [Table("rds.Session")]
    public partial class Session
    {
        public Guid Id { get; set; }

        public Guid TargetId { get; set; }

        public long? UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string UserName { get; set; }

        [Required]
        [StringLength(256)]
        public string UserDomain { get; set; }

        public int SessionId { get; set; }

        public long CreateTime { get; set; }

        public long? DisconnectTime { get; set; }

        [StringLength(256)]
        public string InitialProgram { get; set; }

        public byte? ProtocolType { get; set; }

        public byte? State { get; set; }

        public int ResolutionWidth { get; set; }

        public int ResolutionHeight { get; set; }

        public int ColorDepth { get; set; }

        public virtual Target Target { get; set; }

        public virtual User User { get; set; }
    }
}