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
using System.IO.Pipes;
using System.Text;

namespace Myrtille.Helpers
{
    public static class PipeHelper
    {
        /// <summary>
        /// read data from a named pipe
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="pipeName"></param>
        /// <param name="sizeHeader"></param>
        /// <param name="bufferSize"></param>
        public static byte[] ReadPipeData(
            PipeStream pipe,
            string pipeName,
            bool sizeHeader = true,
            int bufferSize = 4096)
        {
            if (pipe == null)
            {
                Trace.TraceError("Failed to read message from pipe {0} (not set)", (string.IsNullOrEmpty(pipeName) ? "<unknown>" : pipeName));
                return null;
            }

            if (!pipe.IsConnected)
            {
                Trace.TraceError("Failed to read message from pipe {0} (not connected)", (string.IsNullOrEmpty(pipeName) ? "<unknown>" : pipeName));
                return null;
            }

            byte[] buffer;

            try
            {
                if (pipe.CanRead)
                {
                    // byte mode
                    if (pipe.TransmissionMode == PipeTransmissionMode.Byte)
                    {
                        // the first 4 bytes (int32) contains the size of the data buffer
                        if (sizeHeader)
                        {
                            int bytesRead;
                            var bytesToRead = 4;
                            buffer = new byte[bytesToRead];
                            if ((bytesRead = pipe.Read(buffer, 0, bytesToRead)) == bytesToRead)
                            {
                                bytesToRead = BitConverter.ToInt32(buffer, 0);
                                buffer = new byte[bytesToRead];
                                if ((bytesRead = pipe.Read(buffer, 0, bytesToRead)) == bytesToRead)
                                {
                                    return buffer;
                                }
                            }
                            return null;
                        }
                        else
                        {
                            buffer = new byte[bufferSize];
                            using (var memoryStream = new MemoryStream())
                            {
                                memoryStream.Write(buffer, 0, pipe.Read(buffer, 0, bufferSize));
                                return memoryStream.ToArray();
                            }
                        }
                    }
                    // message mode
                    else
                    {
                        buffer = new byte[bufferSize];
                        using (var memoryStream = new MemoryStream())
                        {
                            do
                            {
                                memoryStream.Write(buffer, 0, pipe.Read(buffer, 0, bufferSize));
                            } while (pipe != null && pipe.IsConnected && pipe.CanRead && !pipe.IsMessageComplete);
                            return memoryStream.ToArray();
                        }
                    }
                }
                else
                {
                    throw new Exception("not readable");
                }
            }
            catch (IOException)
            {
                Trace.TraceWarning("Failed to read message from pipe {0} (I/O error)", (string.IsNullOrEmpty(pipeName) ? "<unknown>" : pipeName));
                throw;
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to read message from pipe {0} ({1})", (string.IsNullOrEmpty(pipeName) ? "<unknown>" : pipeName), exc);
                throw;
            }
            finally
            {
                buffer = null;
            }
        }

        /// <summary>
        /// write data to a named pipe
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="pipeName"></param>
        /// <param name="message"></param>
        /// <param name="sizeHeader"></param>
        public static void WritePipeData(
            PipeStream pipe,
            string pipeName,
            string message,
            bool sizeHeader = true)
        {
            if (pipe == null)
            {
                Trace.TraceError("Failed to write message to pipe {0} (not set)", (string.IsNullOrEmpty(pipeName) ? "<unknown>" : pipeName));
                return;
            }

            if (!pipe.IsConnected)
            {
                Trace.TraceError("Failed to write message to pipe {0} (not connected)", (string.IsNullOrEmpty(pipeName) ? "<unknown>" : pipeName));
                return;
            }

            byte[] buffer;

            try
            {
                if (pipe.CanWrite)
                {
                    if (pipe.TransmissionMode == PipeTransmissionMode.Byte && sizeHeader)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            var bytes = Encoding.UTF8.GetBytes(message);
                            memoryStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
                            memoryStream.Write(bytes, 0, bytes.Length);
                            buffer = memoryStream.ToArray();
                        }
                    }
                    else
                    {
                        buffer = Encoding.UTF8.GetBytes(message);
                    }

                    pipe.Write(buffer, 0, buffer.Length);
                    pipe.Flush();
                }
                else
                {
                    throw new Exception("not writable");
                }
            }
            catch (IOException)
            {
                Trace.TraceWarning("Failed to write message to pipe {0} (I/O error)", (string.IsNullOrEmpty(pipeName) ? "<unknown>" : pipeName));
                throw;
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to write message to pipe {0} ({1})", (string.IsNullOrEmpty(pipeName) ? "<unknown>" : pipeName), exc);
                throw;
            }
            finally
            {
                buffer = null;
            }
        }
    }
}