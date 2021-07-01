using System.Windows.Forms;

namespace DataTableConverter
{
    internal static class MessageHandler
    {
        private static readonly string WarningText = "Warnung!";
        private static readonly string ErrorText = "Warnung!";
        private static readonly string InfoText = "Info";

        internal static DialogResult MessagesYesNo(this Form mainForm, MessageBoxIcon messageBoxIcon, string text)
        {
            string warnung = (messageBoxIcon == MessageBoxIcon.Exclamation) ? WarningText : ((messageBoxIcon == MessageBoxIcon.Error)) ? ErrorText : InfoText;
            DialogResult result = DialogResult.Cancel;
            mainForm.Invoke(new MethodInvoker(() =>
            {
                result = MessageBox.Show(mainForm,
                                text,
                                warnung,
                                MessageBoxButtons.YesNo,
                                messageBoxIcon,
                                MessageBoxDefaultButton.Button1);
            }));
            return result;
        }

        internal static DialogResult MessagesYesNoCancel(this Form mainForm, MessageBoxIcon messageBoxIcon, string text)
        {
            string warnung = (messageBoxIcon == MessageBoxIcon.Exclamation) ? WarningText : ((messageBoxIcon == MessageBoxIcon.Error)) ? ErrorText : InfoText;
            DialogResult result = DialogResult.Cancel;
            mainForm.Invoke(new MethodInvoker(() =>
            {
                result = MessageBox.Show(mainForm,
                                text,
                                warnung,
                                MessageBoxButtons.YesNoCancel,
                                messageBoxIcon,
                                MessageBoxDefaultButton.Button1);
            }));
            return result;
        }

        internal static DialogResult MessagesOkCancel(this Form mainForm, MessageBoxIcon messageBoxIcon, string text)
        {
            string warnung = (messageBoxIcon == MessageBoxIcon.Exclamation) ? WarningText : ((messageBoxIcon == MessageBoxIcon.Error)) ? ErrorText : InfoText;
            DialogResult result = DialogResult.Cancel;
            mainForm.Invoke(new MethodInvoker(() =>
            {
                result = MessageBox.Show(mainForm,
                                text,
                                warnung,
                                MessageBoxButtons.OKCancel,
                                messageBoxIcon,
                                MessageBoxDefaultButton.Button1);
            }));
            return result;
        }

        internal static void MessagesOK(this Form mainForm, MessageBoxIcon messageBoxIcon, string text)
        {
            mainForm.Invoke(new MethodInvoker(() =>
            {
                MessageBox.Show(mainForm,
                                text,
                                (messageBoxIcon == MessageBoxIcon.Exclamation) ? WarningText : (messageBoxIcon == MessageBoxIcon.Error) ? ErrorText : InfoText,
                                MessageBoxButtons.OK,
                                messageBoxIcon,
                                MessageBoxDefaultButton.Button1);
            }));
        }
    }
}
