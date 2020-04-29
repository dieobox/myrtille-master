/*
    pdf scribe virtual pdf printer
    all credits to Sherman Chan (https://github.com/stchan/PdfScribe)
    this code is licensed under LGPL v3 (https://www.gnu.org/licenses/lgpl-3.0.en.html)
    changes from original code are surrounded by "myrtille" region tags
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Myrtille.Helpers;

namespace Myrtille.Printer
{
    public class Program
    {
        #region Message constants

        const string errorDialogCaption = "PDF Scribe"; // Error taskdialog caption text

        const string errorDialogInstructionPDFGeneration = "There was a PDF generation error.";
        const string errorDialogInstructionCouldNotWrite = "Could not create the output file.";
        const string errorDialogInstructionUnexpectedError = "There was an internal error. Enable tracing for details.";

        const string errorDialogTextFileInUse = "{0} is being used by another process.";
        const string errorDialogTextGhostScriptConversion = "Ghostscript error code {0}.";

        const string warnFileNotDeleted = "{0} could not be deleted.";

        #endregion

        #region Other constants

        const string traceSourceName = "PdfScribe";

        const string defaultOutputFilename = "PDFSCRIBE.PDF";

        #endregion

        static TraceSource logEventSource = new TraceSource(traceSourceName);

        #region myrtille

        static Process parentProcess = null;

        #endregion

        [STAThread]
        static void Main(string[] args)
        {
            // Install the global exception handler
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Application_UnhandledException);

            #region myrtille

            // enable the line below to debug this postscript redirector; disable otherwise
            // CAUTION! if this print job is from FreeRDP windowless (non user interactive), use the sleep delay instead
            //MessageBox.Show("Attach the .NET debugger to the 'PDF Debug' Myrtille.Printer.exe process now for debug. Click OK when ready...", "PDF Debug");
            //Thread.Sleep(10000);

            // retrieve the parent process
            parentProcess = ProcessHelper.GetParentProcess();
            if (parentProcess == null || parentProcess.ProcessName != "spoolsv")
            {
                if (Environment.UserInteractive)
                {
                    MessageBox.Show("This program is meant to be called by the spooler service",
                                    errorDialogCaption,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error,
                                    MessageBoxDefaultButton.Button1,
                                    MessageBoxOptions.DefaultDesktopOnly);
                }
                return;
            }

            #endregion

            String standardInputFilename = Path.GetTempFileName();
            String outputFilename = String.Empty;

            try
            {
                using (BinaryReader standardInputReader = new BinaryReader(Console.OpenStandardInput()))
                {
                    using (FileStream standardInputFile = new FileStream(standardInputFilename, FileMode.Create, FileAccess.ReadWrite))
                    {
                        standardInputReader.BaseStream.CopyTo(standardInputFile);
                    }
                }

                if (GetPdfOutputFilename(ref outputFilename))
                {
                    // Remove the existing PDF file if present
                    File.Delete(outputFilename);
                    // Only set absolute minimum parameters, let the postscript input
                    // dictate as much as possible
                    String[] ghostScriptArguments = { "-dBATCH", "-dNOPAUSE", "-dSAFER",  "-sDEVICE=pdfwrite",
                                                String.Format("-sOutputFile={0}", outputFilename), standardInputFilename };

                    #region myrtille

                    //GhostScript64.CallAPI(ghostScriptArguments);

                    // the current process will usually run in 32 bits, but that also depends on the OS, spooler, printer drivers, executable location, etc.
                    // call the ghostscript dll accordingly
                    if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
                    {
                        GhostScript64.CallAPI(ghostScriptArguments);
                    }
                    else
                    {
                        GhostScript32.CallAPI(ghostScriptArguments);
                    }

                    #endregion
                }
            }
            catch (IOException ioEx)
            {
                // We couldn't delete, or create a file
                // because it was in use
                logEventSource.TraceEvent(TraceEventType.Error,
                                          (int)TraceEventType.Error,
                                          errorDialogInstructionCouldNotWrite +
                                          Environment.NewLine +
                                          "Exception message: " + ioEx.Message);
                DisplayErrorMessage(errorDialogCaption,
                                    errorDialogInstructionCouldNotWrite + Environment.NewLine +
                                    String.Format("{0} is in use.", outputFilename));
            }
            catch (UnauthorizedAccessException unauthorizedEx)
            {
                // Couldn't delete a file
                // because it was set to readonly
                // or couldn't create a file
                // because of permissions issues
                logEventSource.TraceEvent(TraceEventType.Error,
                                          (int)TraceEventType.Error,
                                          errorDialogInstructionCouldNotWrite +
                                          Environment.NewLine +
                                          "Exception message: " + unauthorizedEx.Message);
                DisplayErrorMessage(errorDialogCaption,
                                    errorDialogInstructionCouldNotWrite + Environment.NewLine +
                                    String.Format("Insufficient privileges to either create or delete {0}", outputFilename));
            }
            catch (ExternalException ghostscriptEx)
            {
                // Ghostscript error
                logEventSource.TraceEvent(TraceEventType.Error,
                                          (int)TraceEventType.Error,
                                          String.Format(errorDialogTextGhostScriptConversion, ghostscriptEx.ErrorCode.ToString()) +
                                          Environment.NewLine +
                                          "Exception message: " + ghostscriptEx.Message);
                DisplayErrorMessage(errorDialogCaption,
                                    errorDialogInstructionPDFGeneration + Environment.NewLine +
                                    String.Format(errorDialogTextGhostScriptConversion, ghostscriptEx.ErrorCode.ToString()));
            }
            finally
            {
                try
                {
                    File.Delete(standardInputFilename);
                }
                catch
                {
                    logEventSource.TraceEvent(TraceEventType.Warning,
                                              (int)TraceEventType.Warning,
                                              String.Format(warnFileNotDeleted, standardInputFilename));
                }
                logEventSource.Flush();
            }
        }

        /// <summary>
        /// All unhandled exceptions will bubble their way up here -
        /// a final error dialog will be displayed before the crash and burn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logEventSource.TraceEvent(TraceEventType.Critical,
                                      (int)TraceEventType.Critical,
                                      ((Exception)e.ExceptionObject).Message + Environment.NewLine +
                                                                        ((Exception)e.ExceptionObject).StackTrace);
            #region myrtille

            // if an exception message is set, use it
            var message = ((Exception)e.ExceptionObject).Message;
            DisplayErrorMessage(errorDialogCaption,
                                //errorDialogInstructionUnexpectedError);
                                !string.IsNullOrEmpty(message) ? message : errorDialogInstructionUnexpectedError);

            #endregion
        }

        static bool GetPdfOutputFilename(ref String outputFile)
        {
            bool filenameRetrieved = false;

            #region myrtille

            // is this print job from FreeRDP? (look into the spooler environment)
            outputFile = GetFreeRDPOutputFilename(parentProcess);
            if (!string.IsNullOrEmpty(outputFile))
            {
                filenameRetrieved = true;
            }

            #endregion

            // if not a FreeRDP print job, follow the standard pdf scribe code
            else
            {
                // ensure the current process is running in user interactive mode before showing any dialog
                //switch (Properties.Settings.Default.AskUserForOutputFilename)
                switch (Properties.Settings.Default.AskUserForOutputFilename && Environment.UserInteractive)
                {
                    case (true):
                        using (SetOutputFilename dialogOwner = new SetOutputFilename())
                        {
                            dialogOwner.TopMost = true;
                            dialogOwner.TopLevel = true;
                            dialogOwner.Show(); // Form won't actually show - Application.Run() never called
                                                // but having a topmost/toplevel owner lets us bring the SaveFileDialog to the front
                            dialogOwner.BringToFront();
                            using (SaveFileDialog pdfFilenameDialog = new SaveFileDialog())
                            {
                                pdfFilenameDialog.AddExtension = true;
                                pdfFilenameDialog.AutoUpgradeEnabled = true;
                                pdfFilenameDialog.CheckPathExists = true;
                                pdfFilenameDialog.Filter = "pdf files (*.pdf)|*.pdf";
                                pdfFilenameDialog.ShowHelp = false;
                                pdfFilenameDialog.Title = "PDF Scribe - Set output filename";
                                pdfFilenameDialog.ValidateNames = true;
                                if (pdfFilenameDialog.ShowDialog(dialogOwner) == DialogResult.OK)
                                {
                                    outputFile = pdfFilenameDialog.FileName;
                                    filenameRetrieved = true;
                                }
                            }
                            dialogOwner.Close();
                        }
                        break;
                    default:
                        outputFile = GetOutputFilename();
                        filenameRetrieved = true;
                        break;
                }
            }

            return filenameRetrieved;
        }

        private static String GetOutputFilename()
        {
            String outputFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), defaultOutputFilename);
            if (!String.IsNullOrEmpty(Properties.Settings.Default.OutputFile) &&
                !String.IsNullOrWhiteSpace(Properties.Settings.Default.OutputFile))
            {
                if (IsFilePathValid(Properties.Settings.Default.OutputFile))
                {
                    outputFilename = Properties.Settings.Default.OutputFile;
                }
                else
                {
                    if (IsFilePathValid(Environment.ExpandEnvironmentVariables(Properties.Settings.Default.OutputFile)))
                    {
                        outputFilename = Environment.ExpandEnvironmentVariables(Properties.Settings.Default.OutputFile);
                    }
                }
            }
            else
            {
                logEventSource.TraceEvent(TraceEventType.Warning,
                                          (int)TraceEventType.Warning,
                                          String.Format("Using default output filename {0}",
                                                        outputFilename));
            }
            return outputFilename;
        }

        #region myrtille

        private static String GetFreeRDPOutputFilename(Process spooler)
        {
            var filename = string.Empty;

            // myrtille print jobs are prefixed by "FREERDPjob" and concatenate the wfreerdp process id and a timestamp, thus should be unique and secure
            // the resulting pdf files are deleted once downloaded to the browser

            if (spooler != null &&
                spooler.StartInfo != null &&
                spooler.StartInfo.EnvironmentVariables != null &&
                !string.IsNullOrEmpty(spooler.StartInfo.EnvironmentVariables["REDMON_DOCNAME"]) &&
                spooler.StartInfo.EnvironmentVariables["REDMON_DOCNAME"].StartsWith("FREERDPjob"))
            {
                var systemTempPath = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);
                var pdfFile = string.Concat(spooler.StartInfo.EnvironmentVariables["REDMON_DOCNAME"], ".pdf");
                filename = Path.Combine(systemTempPath, pdfFile);
            }

            return filename;
        }

        #endregion

        static bool IsFilePathValid(String filePath)
        {
            bool pathIsValid = false;

            if (!String.IsNullOrEmpty(filePath) && filePath.Length <= 260)
            {
                String directoryName = Path.GetDirectoryName(filePath);
                String filename = Path.GetFileName(filePath);

                if (Directory.Exists(directoryName))
                {
                    // Check for invalid filename chars
                    Regex containsABadCharacter = new Regex("["
                                                    + Regex.Escape(new String(System.IO.Path.GetInvalidPathChars())) + "]");
                    pathIsValid = !containsABadCharacter.IsMatch(filename);
                }
            }
            else
            {
                logEventSource.TraceEvent(TraceEventType.Warning,
                                          (int)TraceEventType.Warning,
                                          "Output filename is longer than 260 characters, or blank.");
            }
            return pathIsValid;
        }

        /// <summary>
        /// Displays up a topmost, OK-only message box for the error message
        /// </summary>
        /// <param name="boxCaption">The box's caption</param>
        /// <param name="boxMessage">The box's message</param>
        static void DisplayErrorMessage(String boxCaption,
                                        String boxMessage)
        {
            #region myrtille

            // ensure the current process is running in user interactive mode
            if (Environment.UserInteractive)
            {
                MessageBox.Show(boxMessage,
                                boxCaption,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.DefaultDesktopOnly);
            }

            #endregion
        }
    }
}