using System;
using System.IO;
using System.Windows.Forms;

namespace DataTableConverter.Assisstant
{
    class ErrorHelper
    {
        private static readonly string path = Path.Combine(ExportHelper.ProjectPath, "Logs.log");
        private static readonly string ErrorMessage = "Es ist ein Fehler aufgetreten!\nBitte kontaktieren Sie Ihren Administrator.";

        internal static void LogMessage(Exception exception, Form mainForm, bool showMessage = true)
        {
            LogMessage(exception.ToString(), mainForm, showMessage);
        }

        internal static void LogMessage(string text, Form mainForm, bool showMessage = true)
        {
            try
            {
                File.AppendAllText(path, $"{Environment.NewLine}{DateTime.Today} {text}{Environment.NewLine}");
                if (showMessage)
                {
                    ShowError(ErrorMessage, mainForm);
                }
            }
            catch (IOException)
            {
                ShowError($"Es kann nicht auf die Datei \"Logs.log\" zugegriffen werden{Environment.NewLine}Ursprünglicher Fehler:{Environment.NewLine}" + text, mainForm);
            }
            catch (Exception ex)
            {
                ShowError(ErrorMessage + $"{Environment.NewLine}Ursprünglicher Fehler:{Environment.NewLine}" + text + $"{Environment.NewLine}Log Fehler:{Environment.NewLine}" + ex.ToString(), mainForm);
            }
        }

        private static void ShowError(string Message, Form mainForm)
        {
            mainForm.MessagesOK(MessageBoxIcon.Error, Message);
        }
    }
}
