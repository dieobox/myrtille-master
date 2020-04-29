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
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Myrtille.Helpers
{
    public static class CryptoHelper
    {
        #region RDP

        /*
        adapted from https://gallery.technet.microsoft.com/scriptcenter/Password-Text-String-34711a5e
        original script by Ken Sweet

        the powershell script ("password51.ps1", located into the myrtille bin folder at runtime or into the Myrtille.Services project under Visual Studio)
        was modified to use "LocalMachine" instead of "CurrentUser" because the powershell user account is different than IIS
        it was also modified to use unicode (UTF-16LE) instead of UTF-8 in order to work with .rdp files ("password 51:b:")
        
        to generate a password hash, you can use the script on the myrtille gateway (requires access to the machine)
        you may require to update your script execution policy: https://technet.microsoft.com/en-us/library/ee176961.aspx
        to run the script (from its location folder): ". .\password51.ps1" (more info about the functions usage into the script itself)
        the password hash is only valid on the machine which generated it (the myrtille gateway); it won't work on another machine
        the password hash is 492 chars length
        
        there is another, older, method to generate rdp passwords: https://www.remkoweijnen.nl/blog/2007/10/18/how-rdp-passwords-are-encrypted/
        for a .NET/C# implementation (Remko Weijnen's tool is written in Delphi/Pascal), see https://msdn.microsoft.com/en-us/library/aa302402.aspx
        the script method described above seems to be backward compatible with it (the 492 chars hashed passwords are decrypted by the Remko Weijnen's tool and work into .rdp files)
        the password hash is 1329 chars length
                
        mstsc.exe may not save the hashed passwords by default into .rdp files; see https://superuser.com/questions/139665/windows-7-group-policy-allow-rdp-credentials-to-be-saved/140322
        to create .rdp files with remoteapp: https://technet.microsoft.com/en-us/library/gg674996(v=ws.10).aspx
        full remoteapp walkthrough: https://mizitechinfo.wordpress.com/2013/07/26/fast-and-easy-how-to-deploy-remoteapp-on-windows-server-2012/
        */

        public static string RDP_Encrypt(string password)
        {
            try
            {
                var bytes = ProtectedData.Protect(Encoding.Unicode.GetBytes(password), null, DataProtectionScope.LocalMachine);

                var hex = new StringBuilder(bytes.Length * 2);
                foreach (var _byte in bytes)
                {
                    hex.AppendFormat("{0:X2}", _byte);
                }

                return hex.ToString();
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to encrypt rdp password {0} ({1})", password, exc);
                throw;
            }
        }

        public static string RDP_Decrypt(string passwordHash)
        {
            try
            {
                var bytes = new byte[passwordHash.Length / 2];

                var i = 0;
                foreach (var hex in Regex.Matches(passwordHash, "(..)"))
                {
                    bytes[i++] = Convert.ToByte(hex.ToString(), 16);
                }

                return Encoding.Unicode.GetString(ProtectedData.Unprotect(bytes, null, DataProtectionScope.LocalMachine));
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to decrypt rdp password hash {0} ({1})", passwordHash, exc);
                throw;
            }
        }

        #endregion

        #region AES

        public static string AES_Encrypt(string stringToBeEncrypted, string passwordString)
        {
            string encrypted;
            var passwordBytes = Encoding.UTF8.GetBytes(passwordString);
            var bytesToBeEncrypted = Encoding.UTF8.GetBytes(stringToBeEncrypted);

            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (var ms = new MemoryStream())
            {
                using (var AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encrypted = Convert.ToBase64String(ms.ToArray());
                }
            }

            return encrypted;
        }

        public static string AES_Decrypt(string stringToBeDecrypted, string passwordString)
        {
            string decrypted;
            var bytesToBeDecrypted = Convert.FromBase64String(stringToBeDecrypted);
            var passwordBytes = Encoding.UTF8.GetBytes(passwordString);

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (var ms = new MemoryStream())
            {
                using (var AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decrypted = Encoding.UTF8.GetString(ms.ToArray());
                }
            }

            return decrypted;
        }

        #endregion
    }
}