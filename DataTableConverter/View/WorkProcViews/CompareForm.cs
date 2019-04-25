﻿using DataTableConverter.Classes.WorkProcs;
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
    public partial class CompareForm : Form
    {
        internal ProcCompare Procedure;
        internal CompareForm(object[] headers)
        {
            InitializeComponent();
            cbFirstColumn.Items.AddRange(headers);
            cbSecondColumn.Items.AddRange(headers);
            cbFirstColumn.SelectedIndex = cbSecondColumn.SelectedIndex = 0;
            SetNewColumnVisibility(false);
        }

        private void cbNewColumn_CheckedChanged(object sender, EventArgs e)
        {
            bool checkState = ((CheckBox)sender).Checked;
            if (cbOldColumn.Visible = !checkState)
            {
                txtNewColumn.Text = string.Empty;
            }
            else
            {
                cbOldColumn.Checked = false;
            }
            SetNewColumnVisibility(checkState);
        }

        private void SetNewColumnVisibility(bool status)
        {
            lblPadNewColumn.Visible = txtNewColumn.Visible = status;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if(cbFirstColumn.SelectedItem == cbSecondColumn.SelectedItem)
            {
                MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Es kann nicht zwei mal dieselbe Spalte ausgewählt werden!");
            }
            else if(cbNewColumn.Checked && string.IsNullOrWhiteSpace(txtNewColumn.Text))
            {
                MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Bitte geben Sie einen Spaltennamen an!");
            }
            else
            {
                Procedure = new ProcCompare(txtNewColumn.Text, cbOldColumn.Checked, cbFirstColumn.SelectedItem.ToString(), cbSecondColumn.SelectedItem.ToString());
                DialogResult = DialogResult.OK;
            }
        }
    }
}