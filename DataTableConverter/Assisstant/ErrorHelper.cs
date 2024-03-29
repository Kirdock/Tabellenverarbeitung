﻿using System;
using System.Diagnostics;
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
            if (exception is FileNotFoundException path)
            {
                ShowError($"Die Datei {path.FileName} kann nicht gefunden werden", mainForm);
            }
            else if(exception is FileLoadException ex)
            {
                ShowError($"Die Datei {ex.FileName} kann nicht geladen werden\nWird sie von einem anderen Prozess verwendet?", mainForm);
                LogMessage(exception.ToString(), mainForm, false);
            }
            else
            {
                LogMessage(exception.ToString(), mainForm, showMessage);
            }
        }

        internal static void LogMessage(string text, Form mainForm, bool showMessage = true)
        {
            try
            {
                File.AppendAllText(path, $"{Environment.NewLine}{DateTime.Now}; Version:{FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion} {Environment.NewLine}{text}{Environment.NewLine}");
                if (showMessage)
                {
                    ShowError(ErrorMessage, mainForm);
                }
            }
            catch (IOException)
            {
                ShowError($"Es kann nicht auf die Datei \"Logs.log\" zugegriffen werden{Environment.NewLine}Ursprünglicher Fehler:{Environment.NewLine}{text}", mainForm);
            }
            catch (Exception ex)
            {
                ShowError($"{ErrorMessage}{Environment.NewLine}Ursprünglicher Fehler:{Environment.NewLine}{text}{Environment.NewLine}Log Fehler:{Environment.NewLine}{ex}", mainForm);
            }
        }

        internal static void ShowError(string message, Form mainForm)
        {
            mainForm.MessagesOK(MessageBoxIcon.Error, message);
        }
    }
}
