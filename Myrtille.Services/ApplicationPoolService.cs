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

using Myrtille.Helpers;
using Myrtille.Services.Contracts;

namespace Myrtille.Services
{
    public class ApplicationPoolService : IApplicationPoolService
    {
        public void RecycleApplicationPool(string poolName)
        {
            /*
            for this to work, the myrtille services need to run on the same machine as the myrtille gateway
            the myrtille gateway can't recycle its own application pool because it doesn't have enough privileges
            the recycling action creates a new w3wp.exe process, running under the pool identity
            the old (recycled) w3wp.exe process remains alive as long as there is an ongoing http request to the application (graceful exit)
            when it's over, the old process is stopped, all its allocated resources are returned to the operating system and the new process takes over
            */

            IISHelper.RecycleIISApplicationPool(poolName);
        }
    }
}