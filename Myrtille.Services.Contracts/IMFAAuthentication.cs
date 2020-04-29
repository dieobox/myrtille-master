/*
    Myrtille: A native HTML4/5 Remote Desktop Protocol client.

    Copyright(c) 2014-2020 Cedric Coste
    Copyright(c) 2018 Paul Oliver (Olive Innovations)

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
using System.ServiceModel;

namespace Myrtille.Services.Contracts
{
    [ServiceContract]
    public interface IMFAAuthentication
    {
        [OperationContract]
        bool GetState();

        [OperationContract]
        bool Authenticate(string username, string password, string clientIP = null);

        [OperationContract]
        string GetPromptLabel();

        [OperationContract]
        string GetProviderURL();
    }
}