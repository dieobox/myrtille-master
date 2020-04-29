/*
    pdf scribe virtual pdf printer
    all credits to Sherman Chan (https://github.com/stchan/PdfScribe)
    this code is licensed under LGPL v3 (https://www.gnu.org/licenses/lgpl-3.0.en.html)
    changes from original code are surrounded by "myrtille" region tags
*/

using System;
using System.Runtime.InteropServices;

namespace Myrtille.Printer
{
    internal class GhostScript64
    {
        /*
        This code was adapted from Matthew Ephraim's Ghostscript.Net project
        external dll definitions moved into NativeMethods to
        satisfy FxCop requirements
        https://github.com/mephraim/ghostscriptsharp
        */

        /// <summary>
        /// Calls the Ghostscript API with a collection of arguments to be passed to it
        /// </summary>
        public static void CallAPI(string[] args)
        {
            // Get a pointer to an instance of the Ghostscript API and run the API with the current arguments
            IntPtr gsInstancePtr;
            lock (resourceLock)
            {
                NativeMethods64.CreateAPIInstance(out gsInstancePtr, IntPtr.Zero);
                try
                {
                    int result = NativeMethods64.InitAPI(gsInstancePtr, args.Length, args);

                    if (result < 0)
                    {
                        throw new ExternalException("Ghostscript conversion error", result);
                    }
                }
                finally
                {
                    Cleanup(gsInstancePtr);
                }
            }
        }

        /// <summary>
        /// Frees up the memory used for the API arguments and clears the Ghostscript API instance
        /// </summary>
        private static void Cleanup(IntPtr gsInstancePtr)
        {
            NativeMethods64.ExitAPI(gsInstancePtr);
            NativeMethods64.DeleteAPIInstance(gsInstancePtr);
        }


        /// <summary>
        /// GS can only support a single instance, so we need to bottleneck any multi-threaded systems.
        /// </summary>
        private static object resourceLock = new object();
    }
}