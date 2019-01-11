using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.Assisstant
{
    class UpdateHelper
    {
        private readonly static string Repository = "https://github.com/Kirdock/Tabellenverarbeitung/releases";
        private readonly static string Tags = $"{Repository}/latest";
        private readonly static string FileName = "Anwendung.zip";
        private readonly static string Download = Repository+"/download/{0}/"+FileName;
        internal static void CheckUpdate(Form1 Form)
        {
            try
            {
                WebRequest request = WebRequest.Create(Tags);
                WebResponse response = request.GetResponse();

                string version = response.ResponseUri.ToString().Split(new char[] { '/' }).Last();
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

                Console.WriteLine(version);
                Console.WriteLine(fvi.FileVersion);
                if (version != fvi.FileVersion.Substring(0, version.Length))
                {
                    DownloadFile(version, Form);
                }
                else
                {
                    MessageHandler.MessagesOK(System.Windows.Forms.MessageBoxIcon.Information, "Sie besitzen bereits die aktuelle Version");
                }
}
            catch(Exception ex)
            {
                ErrorHelper.LogMessage(ex.ToString());
            }
        }

        private static void DownloadFile(string version, Form1 Form)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(string.Format(Download,version), FileName);

                string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string ZipPath = Path.Combine(path, FileName);
                ZipArchive archive = ZipFile.OpenRead(ZipPath);

                foreach (ZipArchiveEntry file in archive.Entries)
                {
                    string completeFileName = Path.Combine(path, file.FullName);
                    if (file.Name == "")
                    {// Assuming Empty for Directory
                        Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                        continue;
                    }
                    file.ExtractToFile(completeFileName, true);
                }
                archive.Dispose();
                File.Delete(ZipPath);
                RestartApp(Form);
                
            }
        }

        private static void RestartApp(Form1 Form)
        {
            Process.Start(Application.ExecutablePath); // to start new instance of application
            Form.Close();
        }
    }
}
