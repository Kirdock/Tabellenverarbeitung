﻿using DataTableConverter.Classes.WorkProcs;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DataTableConverter.View.WorkProcViews
{
    public partial class NumerationForm : Form
    {
        internal ProcNumber Procedure;
        private List<string> Headers;

        internal NumerationForm(List<string> headers)
        {
            InitializeComponent();
            Headers = headers;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNewColumn.Text))
            {
                if (Headers.Contains(txtNewColumn.Text))
                {
                    this.MessagesOK(MessageBoxIcon.Warning, "Der Spaltenname wird bereits verwendet. Bitte verwenden Sie einen anderen");
                }
                else
                {
                    Procedure = new ProcNumber(txtNewColumn.Text, (int)nbNumberStart.Value, (int)nbNumberEnd.Value, cbNumberRepeat.Checked);
                    DialogResult = DialogResult.OK;
                }
            }
            else
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Bitte geben Sie einen Spaltennamen an");
            }
        }
    }
}
