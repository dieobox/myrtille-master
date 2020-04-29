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
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;

namespace Myrtille.Helpers
{
    // this class uses "System.Net.Mail", available in .net 2.0+, replacing the deprecated "System.Web.Mail" (.net 1.0)
    // a section has to be defined in the project config using this class, as below

    /*
        <system.net>
          <mailSettings>
            <smtp>
              <network 
                host="relayServerHostname" 
                port="portNumber"
                userName="username"
                password="password" />
            </smtp>
          </mailSettings>
        </system.net>
     */

    public enum MailType
    {
        Text,
        Html
    }

    public static class MailHelper
    {
        // "token" is a user defined value to identify a specific email sent asynchronously. it must be unique.
        // if the email has to be sent synchronously, leave "token" null

        public static SmtpClient SendMail(
            string from,
            string to,
            string replyTo,
            string cc,
            string bcc,
            string subject,
            string body,
            ArrayList filesToAttach,
            MailType type,
            MailPriority priority,
            object token = null)
        {
            Trace.TraceInformation("Sending mail {0} from {1} to {2}", subject, from, to);

            try
            {
                var mailMessage = new MailMessage();

                mailMessage.From = new MailAddress(from);

                // multiple e-mail addresses must be separated with a comma character (",")
                mailMessage.To.Add(to);

                if (!string.IsNullOrEmpty(replyTo))
                {
                    mailMessage.ReplyTo = new MailAddress(replyTo);
                }

                if (!string.IsNullOrEmpty(cc))
                {
                    mailMessage.CC.Add(cc);
                }

                if (!string.IsNullOrEmpty(bcc))
                {
                    mailMessage.Bcc.Add(bcc);
                }

                mailMessage.Subject = subject;
                mailMessage.SubjectEncoding = Encoding.UTF8;

                mailMessage.Body = body;
                mailMessage.BodyEncoding = Encoding.UTF8;

                if (filesToAttach != null && filesToAttach.Count > 0)
                {
                    foreach (string fileToAttach in filesToAttach)
                    {
                        mailMessage.Attachments.Add(new Attachment(fileToAttach));
                    }
                }

                mailMessage.IsBodyHtml = type == MailType.Html;
                mailMessage.Priority = priority;
                mailMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                var smtpClient = new SmtpClient();

                if (token != null)
                {
                    smtpClient.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
                    smtpClient.SendAsync(mailMessage, token);
                    return smtpClient;
                }
                else
                {
                    smtpClient.Send(mailMessage);
                    return null;
                }
            }
            catch (SmtpException exc)
            {
                Trace.TraceError("An SMTP error occurred sending mail {(0)}", exc);
                throw;
            }
            catch (Exception exc)
            {
                Trace.TraceError("An error occurred sending mail {(0)}", exc);
                throw;
            }
        }

        public static void CancelMail(
            SmtpClient smtpClient)
        {
            try
            {
                if (smtpClient != null)
                {
                    smtpClient.SendAsyncCancel();
                }
            }
            catch (SmtpException exc)
            {
                Trace.TraceError("An SMTP error occurred cancelling mail {(0)}", exc);
                throw;
            }
            catch (Exception exc)
            {
                Trace.TraceError("An error occurred cancelling mail {(0)}", exc);
                throw;
            }
        }

        private static void SendCompletedCallback(
            object sender,
            AsyncCompletedEventArgs e)
        {
            var token = (string)e.UserState;

            if (e.Cancelled)
            {
                Trace.TraceWarning("The mail was canceled (id:{0})", token);
            }
            else if (e.Error != null)
            {
                Trace.TraceError("An error occurred sending mail (id:{0}, error:{1})", token, e.Error.Message);
            }
            else
            {
                Trace.TraceInformation("The mail was sent successfully (id:{0})", token);
            }
        }

        // https://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address
        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}