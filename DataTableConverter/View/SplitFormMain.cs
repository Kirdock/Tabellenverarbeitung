using DataTableConverter.Classes.WorkProcs;
using System;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class SplitFormMain : Form
    {
        internal ProcSplit Procedure;

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
                Procedure = new ProcSplit(CmBHeader.SelectedItem.ToString(), TxTSplitText.Text, TxTNewColumn.Text);
                DialogResult = DialogResult.OK;
            }
        }
    }
}
