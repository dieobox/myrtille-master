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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Myrtille.Helpers
{
    public static class ProcessHelper
    {
        // based on https://stackoverflow.com/a/3346055/6121074 (all credits to the original author)

        [StructLayout(LayoutKind.Sequential)]
        public struct ParentProcessUtilities
        {
            // These members must match PROCESS_BASIC_INFORMATION
            internal IntPtr Reserved1;
            internal IntPtr PebBaseAddress;
            internal IntPtr Reserved2_0;
            internal IntPtr Reserved2_1;
            internal IntPtr UniqueProcessId;
            internal IntPtr InheritedFromUniqueProcessId;
        }

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

        /// <summary>
        /// Gets the parent process of the current process.
        /// </summary>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess()
        {
            return GetParentProcess(Process.GetCurrentProcess().Handle);
        }

        /// <summary>
        /// Gets the parent process of the specified process.
        /// </summary>
        /// <param name="id">The process id.</param>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess(int id)
        {
            var process = Process.GetProcessById(id);
            return GetParentProcess(process.Handle);
        }

        /// <summary>
        /// Gets the parent process of the specified process.
        /// </summary>
        /// <param name="handle">The process handle.</param>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess(IntPtr handle)
        {
            var pbi = new ParentProcessUtilities();
            int returnLength;
            int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
            if (status != 0)
                throw new Win32Exception(status);

            try
            {
                return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
            }
            catch (ArgumentException)
            {
                // not found
                return null;
            }
        }

        public static void SetProcessWorkingSet(
            int minSize,
            int maxSize)
        {
            try
            {
                var process = Process.GetCurrentProcess();
                process.MinWorkingSet = (IntPtr)minSize;
                process.MaxWorkingSet = (IntPtr)maxSize;

                Trace.TraceInformation("Set process working set (min size: " + process.MinWorkingSet.ToString() + " bytes, max size: " + process.MaxWorkingSet.ToString() + " bytes)");
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to set the process working set ({0})", exc);
            }
        }

        public static void MinimizeProcessWorkingSet()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                process.MaxWorkingSet = process.MaxWorkingSet;

                Trace.TraceInformation("Minimize process working set (current size: " + process.WorkingSet64.ToString() + " bytes)");

                process.Dispose();
            }
            catch (Exception exc)
            {
                Trace.TraceError("Failed to minimize the process working set ({0})", exc);
            }
        }
    }
}