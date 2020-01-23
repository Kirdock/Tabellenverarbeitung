using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class SplitFormMain : Form
    {
        internal string SplitText => TxTSplitText.Text;
        internal string NewColumn => TxTNewColumn.Text;
        internal string Column => CmBHeader.SelectedItem.ToString();

        internal SplitFormMain(object[] headers)
        {
            InitializeComponent();
            CmBHeader.Items.AddRange(headers);
            CmBHeader.SelectedIndex = 0;
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (CmBHeader.Items.Contains(TxTNewColumn.Text))
            {
                MessageHandler.MessagesOK(this, MessageBoxIcon.Warning, "Der Spaltenname ist bereits vergeben!");
            }
            else if (string.IsNullOrWhiteSpace(TxTNewColumn.Text))
            {
                MessageHandler.MessagesOK(this, MessageBoxIcon.Warning, "Bitte geben Sie einen Bezeichnung für die neue Spalte an");
            }
            else if(TxTSplitText.Text == string.Empty)
            {
                MessageHandler.MessagesOK(this, MessageBoxIcon.Warning, "Bitte geben Sie an nach welchem Text/Zeichen aufgeteilt werden soll");
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
