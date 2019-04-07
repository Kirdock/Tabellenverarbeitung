﻿using System;
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
            try
            {
                File.AppendAllText(path, exception.ToString() + Environment.NewLine);
                if (showMessage)
                {
                    ShowError(ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ShowError(ErrorMessage + '\n' +ex.ToString());
            }

        }

        private static void ShowError(string Message)
        {
            MessageHandler.MessagesOK(System.Windows.Forms.MessageBoxIcon.Error, Message);
        }
    }
}
