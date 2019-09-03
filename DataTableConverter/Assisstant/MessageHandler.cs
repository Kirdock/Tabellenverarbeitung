using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter
{
    class MessageHandler
    {
        private static readonly string WarningText = "Warnung!";
        private static readonly string ErrorText = "Warnung!";
        private static readonly string InfoText = "Info";

        public static DialogResult MessagesYesNo(MessageBoxIcon messageBoxIcon, string text)
        {
            string warnung = (messageBoxIcon == MessageBoxIcon.Exclamation) ? WarningText : ((messageBoxIcon == MessageBoxIcon.Error)) ? ErrorText : InfoText;
            return MessageBox.Show(text,
                                warnung,
                                MessageBoxButtons.YesNo,
                                messageBoxIcon,
                                MessageBoxDefaultButton.Button1);
        }

        public static DialogResult MessagesYesNoCancel(MessageBoxIcon messageBoxIcon, string text)
        {
            string warnung = (messageBoxIcon == MessageBoxIcon.Exclamation) ? WarningText : ((messageBoxIcon == MessageBoxIcon.Error)) ? ErrorText : InfoText;
            return MessageBox.Show(text,
                                warnung,
                                MessageBoxButtons.YesNoCancel,
                                messageBoxIcon,
                                MessageBoxDefaultButton.Button1);
        }

        public static DialogResult MessagesOkCancel(MessageBoxIcon messageBoxIcon, string text)
        {
            string warnung = (messageBoxIcon == MessageBoxIcon.Exclamation) ? WarningText : ((messageBoxIcon == MessageBoxIcon.Error)) ? ErrorText : InfoText;
            return MessageBox.Show(text,
                                warnung,
                                MessageBoxButtons.OKCancel,
                                messageBoxIcon,
                                MessageBoxDefaultButton.Button1);
        }

        public static void MessagesOK(MessageBoxIcon messageBoxIcon, string text)
        {
            MessageBox.Show(text,
                            (messageBoxIcon == MessageBoxIcon.Exclamation) ? WarningText : (messageBoxIcon == MessageBoxIcon.Error) ? ErrorText : InfoText,
                            MessageBoxButtons.OK,
                            messageBoxIcon,
                            MessageBoxDefaultButton.Button1);
        }
    }
}
