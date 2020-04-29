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
    [Table("rds.TargetProperty")]
    public partial class TargetProperty
    {
        public Guid Id { get; set; }

        public Guid TargetId { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        public string ValueStr { get; set; }

        public long? ValueInt { get; set; }

        public short? ValueType { get; set; }

        public virtual Target Target { get; set; }
    }
}