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

namespace Myrtille.Services.Contracts
{
    public static class EnterpriseAuthenticationErrorHelper
    {
        public static string GetErrorDescription(EnterpriseAuthenticationErrorCode errorCode)
        {
            switch (errorCode)
            {
                case EnterpriseAuthenticationErrorCode.USER_NOT_FOUND:
                    return "User not found";
                case EnterpriseAuthenticationErrorCode.INVALID_LOGIN_CREDENTIALS:
                    return "Invalid credentials";
                case EnterpriseAuthenticationErrorCode.LOGIN_NOT_PERMITTED_AT_THIS_TIME:
                    return "Not permitted to login at this time";
                case EnterpriseAuthenticationErrorCode.LOGIN_NOT_PERMITTED_AT_THIS_WORKSTATION:
                    return "Not permitted to login at this workstation";
                case EnterpriseAuthenticationErrorCode.PASSWORD_EXPIRED:
                    return  "Password expired";
                case EnterpriseAuthenticationErrorCode.ACCOUNT_DISABLED:
                    return "Account disabled";
                case EnterpriseAuthenticationErrorCode.USER_NOT_GRANTED_LOGIN_AT_THIS_MACHINE:
                    return "The user has not been granted the requested logon type at this machine";
                case EnterpriseAuthenticationErrorCode.ACCOUNT_EXPIRED:
                    return "Account expired";
                case EnterpriseAuthenticationErrorCode.USER_MUST_RESET_PASSWORD:
                    return "User must reset password but it cannot be done remotely";
                case EnterpriseAuthenticationErrorCode.USER_ACCOUNT_LOCKED:
                    return "User account locked";
                case EnterpriseAuthenticationErrorCode.UNKNOWN_ERROR:
                default:
                    return "Unknown error occured";
            }
        }
    }

    public enum EnterpriseAuthenticationErrorCode
    {
        NONE,
        USER_NOT_FOUND,
        INVALID_LOGIN_CREDENTIALS,
        LOGIN_NOT_PERMITTED_AT_THIS_TIME,
        LOGIN_NOT_PERMITTED_AT_THIS_WORKSTATION,
        PASSWORD_EXPIRED,
        ACCOUNT_DISABLED,
        USER_NOT_GRANTED_LOGIN_AT_THIS_MACHINE,
        ACCOUNT_EXPIRED,
        USER_MUST_RESET_PASSWORD,
        USER_ACCOUNT_LOCKED,
        UNKNOWN_ERROR
    }
}
