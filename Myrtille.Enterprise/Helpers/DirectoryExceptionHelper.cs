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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using Myrtille.Services.Contracts;

namespace Myrtille.Enterprise
{
    public class DirectoryExceptionHelper
    {
        public EnterpriseAuthenticationErrorCode ErrorCode { get; private set; }

        public DirectoryExceptionHelper(EnterpriseAuthenticationErrorCode e)
        {
            ErrorCode = e;

        }
        
        public static implicit operator DirectoryExceptionHelper(DirectoryServicesCOMException e)
        {
            var errorDict = e.ExtendedErrorMessage
                            .Split(',')
                            .Select(x => x.Trim())
                            .ToDictionary(s => s.Split(new[] { ' ' }, 2)[0], s => s);

            var errorCode = errorDict["data"].Split(new[] { ' ' }, 2)[1];

            switch (errorCode)
            {
                case "525":
                    return new DirectoryExceptionHelper(EnterpriseAuthenticationErrorCode.USER_NOT_FOUND);
                case "52e":
                    return new DirectoryExceptionHelper(EnterpriseAuthenticationErrorCode.INVALID_LOGIN_CREDENTIALS);
                case "530":
                    return new DirectoryExceptionHelper(EnterpriseAuthenticationErrorCode.LOGIN_NOT_PERMITTED_AT_THIS_TIME);
                case "531":
                    return new DirectoryExceptionHelper(EnterpriseAuthenticationErrorCode.LOGIN_NOT_PERMITTED_AT_THIS_WORKSTATION);
                case "532":
                    return new DirectoryExceptionHelper(EnterpriseAuthenticationErrorCode.PASSWORD_EXPIRED);
                case "533":
                    return new DirectoryExceptionHelper(EnterpriseAuthenticationErrorCode.ACCOUNT_DISABLED);
                case "534":
                    return new DirectoryExceptionHelper(EnterpriseAuthenticationErrorCode.USER_NOT_GRANTED_LOGIN_AT_THIS_MACHINE);
                case "701":
                    return new DirectoryExceptionHelper(EnterpriseAuthenticationErrorCode.ACCOUNT_EXPIRED);
                case "773":
                    return new DirectoryExceptionHelper(EnterpriseAuthenticationErrorCode.USER_MUST_RESET_PASSWORD);
                case "775":
                    return new DirectoryExceptionHelper(EnterpriseAuthenticationErrorCode.USER_ACCOUNT_LOCKED);
                default:
                    return new DirectoryExceptionHelper(EnterpriseAuthenticationErrorCode.UNKNOWN_ERROR);
            }
        }
    }
}
