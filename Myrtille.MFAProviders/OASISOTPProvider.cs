/*
    OASIS One time passcode service integration

    Copyright(c) 2017-2018 Olive Innovations

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

using System.Configuration;
using OASIS.Integration;
using OASIS.Integration.Models;
using Myrtille.Services.Contracts;

namespace Myrtille.MFAProviders
{
    // OASIS OTP Provider is released under Apache 2.0 project.
    // Source available at https://github.com/oliveinnovations/oasis

    public class OASISOTPProvider : IMultifactorAuthenticationAdapter
    {
        /// <summary>
        /// Authenticate users one time passcode against the OASIS platform
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Authenticate(string username, string password, string clientIP = null)
        {
            var apiKey = ConfigurationManager.AppSettings["OASISApiKey"];
            var appKey = ConfigurationManager.AppSettings["OASISAppKey"];
            var appID = long.Parse(ConfigurationManager.AppSettings["OASISAppID"]);

            if (!string.IsNullOrEmpty(clientIP) && (clientIP.Equals("127.0.0.1") || clientIP.Equals("::1")))
                clientIP = null;

            var oasis = new OTPProvider(appID, appKey, apiKey, RemoteIP:clientIP);

            var state = oasis.RequestAuthorisationState(new RequestAuthorisationState
            {
                Username = username,
                VerificationType = VerificationTypeEnum.LOGIN
            });

            if (state.State == UserAuthenticatorStateEnum.SKIPAUTHENTICATION)
                return true;

            if (state.State != UserAuthenticatorStateEnum.AUTHENTICATE)
                return false;

            var result = oasis.VerifyUserOTP(new VerifyUserOTP
            {
                Username = username,
                OTPCode = password,
                VerificationType = VerificationTypeEnum.LOGIN
            });

            return result.State == UserAuthenticatorStateEnum.VALID;
        }

        public string PromptLabel
        {
            get
            {
                return "OASIS OTP Code";
            }
        }

        public string ProviderURL
        {
            get
            {
                return "http://www.oliveinnovations.com";
            }
        }
    }
}