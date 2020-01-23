using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.Assisstant
{
    class UpdateHelper
    {
        private readonly static string Repository = "https://github.com/Kirdock/Tabellenverarbeitung/releases";
        private readonly static string Tags = $"{Repository}/latest";
        private readonly static string FileNameWithoutExtension = "Anwendung";
        private readonly static string FileName = FileNameWithoutExtension+".zip";
        private readonly static string Download = Repository+"/download/{0}/"+FileName;
        
        internal static void CheckUpdate(bool prompt, ProgressBar progressBar, Form mainForm)
        {
            if (!prompt || !Properties.Settings.Default.UpdateDialogShowed)
            {
                new Thread(() =>
                {
                    try
                    {
                        WebRequest request = WebRequest.Create(Tags);
                        WebResponse response = request.GetResponse();

                        string version = response.ResponseUri.ToString().Split(new char[] { '/' }).Last();
                        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

                        if (version != fvi.FileVersion.Substring(0, version.Length))
                        {
                            if (prompt)
                            {
                                DialogResult result = mainForm.MessagesYesNoCancel(MessageBoxIcon.Information, "Eine neue Version steht zur Verfügung. Möchten Sie sie runterladen?");
                                if (result == DialogResult.Yes)
                                {
                                    SetUpdateShowed(false);
                                    DownloadFile(version, progressBar, mainForm);
                                }
                                else
                                {
                                    SetUpdateShowed(true);
                                }
                            }
                            else
                            {
                                DownloadFile(version, progressBar, mainForm);
                            }
                        }
                        else if (!prompt)
                        {
                            mainForm.MessagesOK(MessageBoxIcon.Information, "Sie besitzen bereits die aktuellste Version");
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHelper.LogMessage(ex, mainForm, false);
                        ErrorHelper.LogMessage("Update nicht möglich! Besteht eine Internetverbindung?", mainForm);
                    }
                }).Start();
            
            }
        }

        private static void SetUpdateShowed(bool status)
        {
            Properties.Settings.Default.UpdateDialogShowed = status;
            Properties.Settings.Default.Save();
        }

        private static void DownloadFile(string version, ProgressBar progressBar, Form mainForm)
        {
            progressBar.Invoke(new MethodInvoker(() =>
            {
                progressBar.Style = ProgressBarStyle.Marquee;
            }));
            using (var client = new WebClient())
            {
                client.DownloadFile(string.Format(Download,version), FileName);

                string path = GetCurrentDirectory();
                string zipPath = Path.Combine(path, FileName);
                ZipArchive archive = ZipFile.OpenRead(zipPath);
                bool finished = true;
                try
                {
                    foreach (ZipArchiveEntry file in archive.Entries)
                    {
                        string completeFileName = Path.Combine(path, file.FullName);
                        var a = Path.GetDirectoryName(completeFileName);
                        if (!Directory.Exists(Path.GetDirectoryName(completeFileName)))
                        {
                            Directory.CreateDirectory(completeFileName);
                        }

                        if (file.Name != string.Empty)
                        {
                            file.ExtractToFile(completeFileName, true);
                        }
                    }
                }
                catch (Exception e)
                {
                    finished = false;
                    ErrorHelper.LogMessage($"{e.Message}\nZipPath: {zipPath}; ApplicationPath: {path}", mainForm, false);
                    ErrorHelper.LogMessage("Das Archiv konnte nicht enpackt werden", mainForm, true);
                }
                finally
                {
                    archive.Dispose();
                    File.Delete(zipPath);
                }
                if (finished)
                {
                    RestartApp();
                }
                
            }
        }

        internal static string GetCurrentDirectory()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private static void RestartApp()
        {
            string directory = GetCurrentDirectory();
            string BatchPath = Path.Combine(directory, "Updater.bat");
            string ZipFolder = Path.Combine(directory, FileNameWithoutExtension);
            StreamWriter writer = new StreamWriter(BatchPath);
            writer.WriteLine($"timeout /T 1"); //delay in seconds; waiting for application to be closed
            writer.WriteLine($"move \"{Path.Combine(ZipFolder, "*")}\" \"{directory}\"");
            writer.WriteLine($"start \"\" \"{Path.Combine(directory, AppDomain.CurrentDomain.FriendlyName)}\"");
            writer.WriteLine($"rmdir \"{ZipFolder}\"");
            writer.WriteLine($"del /Q \"{BatchPath}\"");
            
            writer.Close();
            Process process = new Process();
            process.StartInfo.FileName = BatchPath;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            Application.Exit();

        }
    }
}
