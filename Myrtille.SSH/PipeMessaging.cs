/*
    Myrtille: A native HTML4/5 Remote Desktop Protocol client.

    Copyright(c) 2018 Paul Oliver (Olive Innovations)
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
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using Myrtille.Helpers;
using Myrtille.Services.Contracts;

namespace Myrtille.SSH
{
    internal class PipeMessaging
    {
        private string _remotesessionID;
        private NamedPipeClientStream _inputsPipe;
        private NamedPipeClientStream _updatesPipe;

        public delegate void MessageReceivedEvent(RemoteSessionCommand command, string data = "");
        public event MessageReceivedEvent OnMessageReceivedEvent;

        public PipeMessaging(string remoteSessionID)
        {
            _remotesessionID = remoteSessionID;
        }

        public bool ConnectPipes()
        {
            try
            {
                _inputsPipe = new NamedPipeClientStream(
                    ".",
                    "remotesession_" + _remotesessionID + "_inputs",
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous,
                    TokenImpersonationLevel.Impersonation);

                _updatesPipe = new NamedPipeClientStream(
                    ".",
                    "remotesession_" + _remotesessionID + "_updates",
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous,
                    TokenImpersonationLevel.Impersonation);
                
                _inputsPipe.Connect();
                _updatesPipe.Connect();
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to connect pipes, remote session {0} ({1})", _remotesessionID, exc);
                return false;
            }

            return true;
        }

        public void ClosePipes()
        {
            ClosePipe("remoteSession_" + _remotesessionID + "_inputs", ref _inputsPipe);
            ClosePipe("remoteSession_" + _remotesessionID + "_updates", ref _updatesPipe);
        }

        public void ReadInputsPipe()
        {
            while (_inputsPipe != null && _inputsPipe.IsConnected)
            {
                var msg = PipeHelper.ReadPipeData(_inputsPipe, "remotesession_" + _remotesessionID + "_inputs", false);
                if (msg != null && msg.Length > 0)
                {
                    ProcessInputsPipeMessage(msg);
                }
            }
        }

        private void ProcessInputsPipeMessage(byte[] msg)
        {
            var message = Encoding.UTF8.GetString(msg);
            var commandsWithArgs = message.Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var commandWithArgs in commandsWithArgs)
            {
                var command = (RemoteSessionCommand)RemoteSessionCommandMapping.FromPrefix[commandWithArgs.Substring(0, 3)];
                var data = commandWithArgs.Substring(3);
                OnMessageReceivedEvent?.Invoke(command, data);
            }
        }

        public void SendUpdatesPipeMessage(string msg)
        {
            PipeHelper.WritePipeData(
                _updatesPipe,
                "remotesession_" + _remotesessionID + "_updates",
                msg,
                false);
        }

        private void ClosePipe(string pipeName, ref NamedPipeClientStream pipe)
        {
            if (pipe != null)
            {
                try
                {
                    // CAUTION! closing a pipe in use can make .NET to crash! disconnect it first...
                    if (pipe.IsConnected)
                    {
                        pipe.WaitForPipeDrain();
                    }

                    pipe.Close();
                }
                catch (Exception exc)
                {
                    Trace.TraceError("Failed to close pipe {0}, remote session {1} ({2})", pipeName, _remotesessionID, exc);
                }
                finally
                {
                    pipe.Dispose();
                    pipe = null;
                }
            }
        }
    }
}