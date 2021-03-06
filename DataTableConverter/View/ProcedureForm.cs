﻿using DataTableConverter.Classes;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class ProcedureForm : Form
    {
        internal DataTable Table;
        internal bool CheckTotal => cbCheckTotal.Checked;
        internal bool CheckWord => CbCheckWord.Checked;
        internal bool LeaveEmpty => CBLeaveEmpty.Checked;
        private ViewHelper Helper;

        internal ProcedureForm(ContextMenuStrip ctxRow, Proc procedure = null)
        {
            InitializeComponent();
            if (procedure == null)
            {
                Table = new DataTable();
                Table.Columns.Add("Ersetze");
                Table.Columns.Add("Durch");
            }
            else
            {
                Table = procedure.Replace.Copy();
                cbCheckTotal.Checked = procedure.CheckTotal;
                CbCheckWord.Checked = procedure.CheckWord;
                CBLeaveEmpty.Checked = procedure.LeaveEmpty;
            }
            DGVProcedure.DataSource = Table;
            Helper = new ViewHelper(ctxRow, null, null);
            ViewHelper.SetDataGridViewStyle(DGVProcedure);
            Helper.AddContextMenuToDataGridView(DGVProcedure, this, true);
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            ViewHelper.EndDataGridViewEdit(DGVProcedure);
            if (Table.AsEnumerable().Any(row => row.ItemArray.Any(item => !string.IsNullOrEmpty(item.ToString()))))
            {
                ViewHelper.EndDataGridViewEdit(DGVProcedure);
                DialogResult = DialogResult.OK;
            }
            else
            {
                this.MessagesOK(MessageBoxIcon.Warning, "Es wurde nichts eingegeben");
            }
        }

        private void ProcedureForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Helper.Clear();
        }
    }
}
