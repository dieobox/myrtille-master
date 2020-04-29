/*
    Myrtille: A native HTML4/5 Remote Desktop and SSH Protocol client.

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
using System.Text;
using System.Threading;
using System.Windows.Forms;
using log4net.Config;
using Renci.SshNet;
using Renci.SshNet.Common;
using Myrtille.Services.Contracts;

namespace Myrtille.SSH
{
    public class Program
    {
        private static PipeMessaging pipeMessaging;
        private static SshClient client;
        private static ShellStream shellStream;
        private static bool loginMessage;

        private static int Main(string[] args)
        {
            // enable the code below for debug; disable otherwise
            //if (Environment.UserInteractive)
            //{
            //    MessageBox.Show("Attach the .NET debugger to the 'SSH Debug' Myrtille.SSH.exe process now for debug. Click OK when ready...", "SSH Debug");
            //}
            //else
            //{
            //    Thread.Sleep(10000);
            //}

            // logger
            XmlConfigurator.Configure();

            string argKeyValueSeparator = ":";
            foreach (string arg in args)
            {
                var argParts = arg.Trim().Split(argKeyValueSeparator.ToCharArray(), 2);
                parseCommandLineArg(argParts[0].ToLower(), (argParts.Length > 1 ? argParts[1] : ""));
            }

            if (!ValidConfig)
            {
                return (int)RemoteSessionExitCode.InvalidConfiguration;
            }

            pipeMessaging = new PipeMessaging(RemoteSessionID);

            if (pipeMessaging.ConnectPipes())
            {
                pipeMessaging.OnMessageReceivedEvent += PipeMessaging_OnMessageReceivedEvent;

                try
                {
                    pipeMessaging.ReadInputsPipe();
                }
                catch (Exception e)
                {
                    if (ConsoleOutput)
                    {
                        Console.WriteLine(e.Message);
                    }

                    Trace.TraceError("SSH error, remote session {0} ({1})", RemoteSessionID, e);

                    if (e is SshAuthenticationException)
                    {
                        if (e.Message == "Missing Username")
                            return (int)RemoteSessionExitCode.MissingUserName;
                        else if (e.Message == "Missing Password")
                            return (int)RemoteSessionExitCode.MissingPassword;
                        else
                            return (int)RemoteSessionExitCode.InvalidCredentials;
                    }

                    return (int)RemoteSessionExitCode.Unknown;
                }
                finally
                {
                    pipeMessaging.ClosePipes();
                    DisconnectSSHClient();
                }
            }

            return (int)RemoteSessionExitCode.Success;
        }

        /// <summary>
        /// Handle commands from inputs pipe
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        private static void PipeMessaging_OnMessageReceivedEvent(RemoteSessionCommand command, string data = "")
        {
            switch (command)
            {
                case RemoteSessionCommand.RequestFullscreenUpdate:
                    WriteOutput(command, data);
                    ClearOrExitTerminal(data);
                    break;
                case RemoteSessionCommand.SendUserDomain:
                    WriteOutput(command, data);
                    Domain = data;
                    break;
                case RemoteSessionCommand.SendUserName:
                    WriteOutput(command, data);
                    UserName = string.IsNullOrEmpty(Domain) ? data : string.Format("{0}\\{1}", Domain, data);
                    break;
                case RemoteSessionCommand.SendServerAddress:
                    WriteOutput(command, data);
                    ServerAddress = data;
                    break;
                case RemoteSessionCommand.SendUserPassword:
                    WriteOutput(command, "Credentials received");
                    Password = data;
                    break;
                case RemoteSessionCommand.ConnectClient:
                    WriteOutput(command, "Connecting to remote host");
                    ConnectSSHClient();
                    break;
                case RemoteSessionCommand.CloseClient:
                    WriteOutput(command, "Disconnecting from remote host");
                    pipeMessaging.ClosePipes();
                    break;
                case RemoteSessionCommand.SendKeyUnicode:
                    WriteOutput(command, string.IsNullOrEmpty(data) ? "," : data);
                    HandleKeyboardInput(string.IsNullOrEmpty(data) ? "," : data);
                    break;
            }
        }

        private static void ClearOrExitTerminal(string fsuType)
        {
            // initial FSU (page (re)load)
            if (fsuType == "initial")
            {
                // don't clear the terminal on first initial FSU so that the SSH welcome message upon login can be seen
                if (!loginMessage)
                {
                    loginMessage = true;
                    return;
                }

                // windows
                if (client.ConnectionInfo.ServerVersion.Contains("Windows"))
                {
                    // cancel the current command line, if any
                    SendSSHClientData(Encoding.UTF8.GetBytes("\u001b"));    // esc (27)

                    // give some time to process data
                    Thread.Sleep(1000);

                    // clear the terminal
                    SendSSHClientData(Encoding.UTF8.GetBytes("cls\r"));     // cls + enter (13)
                }
                // others
                else
                {
                    // cancel the current command line, if any
                    SendSSHClientData(Encoding.UTF8.GetBytes("\u0003"));    // break (3)

                    // give some time to process data
                    Thread.Sleep(1000);

                    // clear the terminal
                    SendSSHClientData(Encoding.UTF8.GetBytes("clear\r"));   // clear + enter (13)
                }
            }
            // periodical or adaptive FSU
            // close the ssh client if disconnected
            else
            {
                CheckSSHClientState();
            }
        }

        #region ssh client
        /// <summary>
        /// Send data to ssh client
        /// </summary>
        /// <param name="byteData"></param>
        private static void SendSSHClientData(byte[] byteData)
        {
            try
            {
                shellStream.Write(byteData, 0, byteData.Length);
                shellStream.Flush();
            }
            catch (Exception e)
            {
                Trace.TraceError("Failed to send data to ssh client, remote session {0} ({1})", RemoteSessionID, e);
                throw;
            }
        }

        /// <summary>
        /// Send data to ssh client
        /// </summary>
        /// <param name="byteData"></param>
        private static void SendSSHClientData(byte byteData)
        {
            try
            {
                shellStream.WriteByte(byteData);
                shellStream.Flush();
            }
            catch (Exception e)
            {
                Trace.TraceError("Failed to send data to ssh client, remote session {0} ({1})", RemoteSessionID, e);
                throw;
            }
        }

        /// <summary>
        /// Disconnect SSH client from Host
        /// </summary>
        private static void DisconnectSSHClient()
        {
            try
            {
                if (client?.IsConnected ?? false)
                {
                    client.Disconnect();
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Failed to disconnect ssh client, remote session {0} ({1})", RemoteSessionID, e);
            }
            finally
            {
                client?.Dispose();
                client = null;
            }
        }

        /// <summary>
        /// Connect SSH client to remote host
        /// </summary>
        private static void ConnectSSHClient()
        {
            try
            {
                if (string.IsNullOrEmpty(UserName))
                    throw new SshAuthenticationException("Missing Username");

                if (string.IsNullOrEmpty(Password))
                    throw new SshAuthenticationException("Missing Password");

                Uri serverUri;
                var serverHost = ServerAddress;
                var serverPort = 22;
                if (Uri.TryCreate("tcp://" + ServerAddress, UriKind.Absolute, out serverUri))
                {
                    serverHost = serverUri.Host;
                    if(serverUri.Port > 0)
                        serverPort = serverUri.Port;
                }

                var connectionInfo = new Renci.SshNet.ConnectionInfo(serverHost, serverPort, UserName, new PasswordAuthenticationMethod(UserName, Password));
                connectionInfo.Encoding = Encoding.UTF8;

                client = new SshClient(connectionInfo);
                client.Connect();

                shellStream = client.CreateShellStream("xterm", Columns, Rows, Width, Height, 1024);
                shellStream.DataReceived += ShellStream_DataReceived;
            }
            catch (Exception e)
            {
                if (ConsoleOutput)
                    Console.WriteLine(e.Message);

                Trace.TraceError("Failed to connect ssh client, remote session {0} ({1})", RemoteSessionID, e);
                throw;
            }
            finally
            {
                if (client != null && !client.IsConnected)
                {
                    client.Dispose();
                    pipeMessaging.ClosePipes();
                }
            }
        }

        /// <summary>
        /// Receive data from SSH client and send to updates pipe for processing by web client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ShellStream_DataReceived(object sender, ShellDataEventArgs e)
        {
            try
            {
                pipeMessaging.SendUpdatesPipeMessage("term|" + Encoding.UTF8.GetString(e.Data));
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to process terminal updates, remote session {0} ({1})", RemoteSessionID, exc);
                pipeMessaging.ClosePipes();
            }
        }

        /// <summary>
        /// Check if ssh client is still connected, if not close pipes and exit program
        /// </summary>
        private static void CheckSSHClientState()
        {
            try
            {
                if (client != null && !client.IsConnected)
                {
                    pipeMessaging.ClosePipes();
                }
                else
                {
                    try
                    {
                        // dummy write to see if the shellstream is still opened (closed after an exit command, for example)
                        shellStream.Write(null);
                        shellStream.Flush();
                    }
                    catch
                    {
                        pipeMessaging.ClosePipes();
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Failed to check ssh client state, remote session {0} ({1})", RemoteSessionID, e);
                throw;
            }
        }
        #endregion

        #region handle keyboard data
        /// <summary>
        /// Handle received keyboard input and send to ssh client
        /// </summary>
        /// <param name="keyCode"></param>
        private static void HandleKeyboardInput(string keyCode)
        {
            SendSSHClientData(Encoding.UTF8.GetBytes(keyCode));
        }

        /// <summary>
        /// Write output to console window if required
        /// </summary>
        /// <param name="command"></param>
        /// <param name="data"></param>
        private static void WriteOutput(RemoteSessionCommand command, string data)
        {
            var output = string.Format("CMD: {0}, DATA: {1}", (string)RemoteSessionCommandMapping.ToPrefix[command], data);
            if (ConsoleOutput) Console.WriteLine(output);
            if (LoggingEnabled) Trace.TraceInformation(output);
        }
        #endregion

        #region configuration
        private static string Domain { get; set; } // Domain use to create SSH connection
        private static string UserName { get; set; } // Username use to create SSH connection
        private static string Password { get; set; } // Password use to create SSH connection
        private static string ServerAddress { get; set; } //Host to establish SSH connection with
        private static string RemoteSessionID { get; set; } //Myrtille session ID used for pipe messaging
        private static bool LoggingEnabled { get; set; } //Myrtille logging parameter
        private static bool ConsoleOutput { get; set; } //Output comms to console window
        private static uint Height { get; set; } //Height of ssh terminal
        private static uint Width { get; set; } //Width of SSH terminal
        private static uint Columns { get { return (Width / 10); } } //Number of columns within the terminal window
        private static uint Rows { get { return (Height / 17); } } //Number of rows within the terminal window

        /// <summary>
        /// Indicate all command line parameters for correct operation have been received.
        /// </summary>
        private static bool ValidConfig
        {
            get
            {
                if (string.IsNullOrEmpty(RemoteSessionID)) return false;
                if (Height == 0) return false;
                if (Width == 0) return false;
                return true;
            }
        }

        /// <summary>
        /// Parse command line arguments
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="value"></param>
        private static void parseCommandLineArg(string arg, string value)
        {
            switch (arg)
            {
                case "/myrtille-sid":
                    RemoteSessionID = value.Trim();
                    break;
                case "/myrtille-window":
                    ConsoleOutput = true;
                    break;
                case "/myrtille-log":
                    LoggingEnabled = true;
                    break;
                case "/h":
                    Height = uint.Parse(value);
                    break;
                case "/w":
                    Width = uint.Parse(value);
                    break;
            }
        }
        #endregion
    }
}