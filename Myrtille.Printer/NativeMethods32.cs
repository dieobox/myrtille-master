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
    internal static class NativeMethods32
    {
        /*
        This code was adapted from Matthew Ephraim's Ghostscript.Net project -
        external dll definitions moved into NativeMethods to
        satisfy FxCop requirements
        https://github.com/mephraim/ghostscriptsharp
        */

        #region Hooks into Ghostscript DLL

        [DllImport("gsdll32.dll", EntryPoint = "gsapi_new_instance")]
        internal static extern int CreateAPIInstance(out IntPtr pinstance, IntPtr caller_handle);

        [DllImport("gsdll32.dll", EntryPoint = "gsapi_init_with_args")]
        internal static extern int InitAPI(IntPtr instance, int argc, string[] argv);

        [DllImport("gsdll32.dll", EntryPoint = "gsapi_exit")]
        internal static extern int ExitAPI(IntPtr instance);

        [DllImport("gsdll32.dll", EntryPoint = "gsapi_delete_instance")]
        internal static extern void DeleteAPIInstance(IntPtr instance);

        #endregion

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetDllDirectory(string lpPathName);
    }
}