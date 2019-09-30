using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Assisstant
{
    class ErrorHelper
    {
        private static readonly string path = Path.Combine(ExportHelper.ProjectPath,"Logs.log");
        private static readonly string ErrorMessage = "Es ist ein Fehler aufgetreten!\nBitte kontaktieren Sie Ihren Administrator.";

        internal static void LogMessage(Exception exception, bool showMessage = true)
        {
            LogMessage(exception.ToString(), showMessage);
        }

        internal static void LogMessage(string text, bool showMessage = true)
        {
            try
            {
                File.AppendAllText(path, $"{Environment.NewLine}{DateTime.Today} {text}{Environment.NewLine}");
                if (showMessage)
                {
                    ShowError(ErrorMessage);
                }
            }
            catch (IOException)
            {
                ShowError($"Es kann nicht auf die Datei \"Logs.log\" zugegriffen werden{Environment.NewLine}Ursprünglicher Fehler:{Environment.NewLine}" + text);
            }
            catch (Exception ex)
            {
                ShowError(ErrorMessage + $"{Environment.NewLine}Ursprünglicher Fehler:{Environment.NewLine}" + text + $"{Environment.NewLine}Log Fehler:{Environment.NewLine}" + ex.ToString());
            }
        }

        private static void ShowError(string Message)
        {
            MessageHandler.MessagesOK(System.Windows.Forms.MessageBoxIcon.Error, Message);
        }
    }
}
