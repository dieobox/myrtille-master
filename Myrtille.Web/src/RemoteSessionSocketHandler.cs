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
using System.Text;
using System.Web.SessionState;
using Microsoft.Web.WebSockets;
using Myrtille.Services.Contracts;

namespace Myrtille.Web
{
    public class RemoteSessionSocketHandler : WebSocketHandler
    {
        private HttpSessionState _session;
        private RemoteSession _remoteSession;

        public bool BinaryMode { get; private set; }

        public RemoteSessionSocketHandler(HttpSessionState session, bool binaryMode)
            : base()
        {
            _session = session;
            BinaryMode = binaryMode;

            try
            {
                if (session[HttpSessionStateVariables.RemoteSession.ToString()] == null)
                    throw new NullReferenceException();

                // retrieve the remote session for the given http session
                _remoteSession = (RemoteSession)session[HttpSessionStateVariables.RemoteSession.ToString()];
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to retrieve the remote session for the http session {0}, ({1})", session.SessionID, exc);
            }
        }

        public override void OnOpen()
        {
            Trace.TraceInformation("Opening websocket, remote session {0}", _remoteSession.Id);
            _remoteSession.Manager.WebSockets.Add(this);

            base.OnOpen();

            // unregister the message queue for the current http session, if exists
            // the http client is now using HTML5 mode
            lock (_remoteSession.Manager.MessageQueues.SyncRoot)
            {
                if (_remoteSession.Manager.MessageQueues.ContainsKey(_session.SessionID))
                {
                    _remoteSession.Manager.MessageQueues.Remove(_session.SessionID);
                }
            }

            // update guest information
            if (!_session.SessionID.Equals(_remoteSession.OwnerSessionID))
            {
                if (_session[HttpSessionStateVariables.GuestInfo.ToString()] != null)
                {
                    ((GuestInfo)_session[HttpSessionStateVariables.GuestInfo.ToString()]).Websocket = true;
                }
            }
            // connect the remote server
            else if (_remoteSession.State == RemoteSessionState.Connecting && !_remoteSession.Manager.HostClient.ProcessStarted)
            {
                try
                {
                    // create pipes for the web gateway and the host client to talk
                    _remoteSession.Manager.Pipes.CreatePipes();

                    // the host client does connect the pipes when it starts; when it stops (either because it was closed, crashed or because the remote session had ended), pipes are released
                    // as the process command line can be displayed into the task manager / process explorer, the connection settings (including user credentials) are now passed to the host client through the inputs pipe
                    // use http://technet.microsoft.com/en-us/sysinternals/dd581625 to track the existing pipes
                    _remoteSession.Manager.HostClient.StartProcess(
                        _remoteSession.Id,
                        _remoteSession.HostType,
                        _remoteSession.SecurityProtocol,
                        _remoteSession.ServerAddress,
                        _remoteSession.VMGuid,
                        _remoteSession.UserDomain,
                        _remoteSession.UserName,
                        _remoteSession.StartProgram,
                        _remoteSession.ClientWidth,
                        _remoteSession.ClientHeight,
                        _remoteSession.AllowRemoteClipboard,
                        _remoteSession.AllowPrintDownload,
                        _remoteSession.AllowAudioPlayback);
                }
                catch (Exception exc)
                {
                    Trace.TraceError("Failed to connect the remote session {0} ({1})", _remoteSession.Id, exc);
                }
            }
            
            // send a disconnect notification
            if (_remoteSession.State == RemoteSessionState.Disconnected)
            {
                Trace.TraceInformation("Sending disconnected state on websocket, remote session {0}", _remoteSession.Id);
                Send("disconnected");
            }
        }

        public override void OnClose()
        {
            Trace.TraceInformation("Closing websocket, remote session {0}", _remoteSession.Id);
            _remoteSession.Manager.WebSockets.Remove(this);

            base.OnClose();
        }

        public override void OnError()
        {
            Trace.TraceError("Websocket error, remote session {0} ({1})", _remoteSession.Id, Error);
            base.OnError();
        }

        public override void OnMessage(string message)
        {
            ProcessMessage(message);
        }

        public override void OnMessage(byte[] message)
        {
            ProcessMessage(Encoding.UTF8.GetString(message));
        }

        private void ProcessMessage(string message)
        {
            try
            {
                var msgParams = message.Split(new[] { "&" }, StringSplitOptions.None);

                var data = Uri.UnescapeDataString(msgParams[0]);
                var imgIdx = int.Parse(msgParams[1]);       // if needed
                var latency = int.Parse(msgParams[2]);      // if needed
                var timestamp = long.Parse(msgParams[3]);

                // process input(s)
                if (!string.IsNullOrEmpty(data))
                {
                    _remoteSession.Manager.ProcessInputs(_session, data);
                }

                // acknowledge the message processing with the given timestamp; it will be used by the client to compute the roundtrip time
                SendMessage(new RemoteSessionMessage { Type = MessageType.Ack, Prefix = "ack,", Text = timestamp.ToString() });
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to process websocket message, remote session {0} ({1})", _remoteSession.Id, exc);
            }
        }

        private string GetImageText(RemoteSessionImage image)
        {
            return
                image.Idx + "," +
                image.PosX + "," +
                image.PosY + "," +
                image.Width + "," +
                image.Height + "," +
                image.Format.ToString().ToLower() + "," +
                image.Quality + "," +
                image.Fullscreen.ToString().ToLower() + "," +
                Convert.ToBase64String(image.Data);
        }

        private byte[] GetImageBytes(RemoteSessionImage image)
        {
            using (var memoryStream = new MemoryStream())
            {
                // tag (4 bytes)
                memoryStream.Write(BitConverter.GetBytes(0), 0, 4);

                // info (32 bytes)
                memoryStream.Write(BitConverter.GetBytes(image.Idx), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(image.PosX), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(image.PosY), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(image.Width), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(image.Height), 0, 4);
                memoryStream.Write(BitConverter.GetBytes((int)image.Format), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(image.Quality), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(image.Fullscreen ? 1 : 0), 0, 4);

                // data
                memoryStream.Write(image.Data, 0, image.Data.Length);

                return memoryStream.ToArray();
            }
        }

        public void SendImage(RemoteSessionImage image)
        {
            if (!BinaryMode)
            {
                Send(GetImageText(image) + ";");
            }
            else
            {
                Send(GetImageBytes(image));
            }
        }

        public void SendMessage(RemoteSessionMessage message)
        {
            if (!BinaryMode)
            {
                Send(string.Concat(message.Prefix, message.Text));
            }
            else
            {
                Send(Encoding.UTF8.GetBytes(string.Concat(message.Prefix, message.Text)));
            }
        }
    }
}