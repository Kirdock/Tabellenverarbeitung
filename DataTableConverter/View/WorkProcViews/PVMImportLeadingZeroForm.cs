using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.View.WorkProcViews
{
    public partial class PVMImportLeadingZeroForm : Form
    {
        internal string LeadingZeroText => textBoxCharacter.Text;
        internal string LeadingZeroColumn => comboBoxColumnReplaceLeadingZero.SelectedValue.ToString();

        internal PVMImportLeadingZeroForm(Dictionary<string, string> importHeaders)
        {
            InitializeComponent();
            //key: alias, value: columnName
            ViewHelper.SetComboboxWithDictionary(comboBoxColumnReplaceLeadingZero, importHeaders);
            if (importHeaders.TryGetValue(Properties.Settings.Default.PVMLeadingZeroAlias, out string value))
            {
                comboBoxColumnReplaceLeadingZero.SelectedValue = value;
            }
            textBoxCharacter.Text = Properties.Settings.Default.PVMLeadingZeroText;
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            if (textBoxCharacter.Text.Length == 0)
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Es muss ein Zeichen oder ein Text angegeben werden, der die führende Null ersetzen soll!");
            }
            else if (comboBoxColumnReplaceLeadingZero.SelectedIndex == -1)
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Es wurde keine Spalte angegeben, die für das Ersetzen der führenden Null hergenommen werden soll!");
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
