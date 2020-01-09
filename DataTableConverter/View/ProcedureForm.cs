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
    public partial class ProcedureForm : Form
    {
        internal DataTable Table;
        internal bool CheckTotal => cbCheckTotal.Checked;
        internal bool CheckWord => CbCheckWord.Checked;
        internal bool LeaveEmpty => CBLeaveEmpty.Checked;
        private ViewHelper Helper;

        public ProcedureForm(ContextMenuStrip ctxRow)
        {
            InitializeComponent();
            Table = new DataTable();
            Table.Columns.Add("Ersetze");
            Table.Columns.Add("Durch");
            DGVProcedure.DataSource = Table;
            Helper = new ViewHelper(ctxRow, null, null);
            ViewHelper.SetDataGridViewStyle(DGVProcedure);
            Helper.AddContextMenuToDataGridView(DGVProcedure, this, true);

        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            ViewHelper.EndDataGridViewEdit(DGVProcedure);
            if(Table.AsEnumerable().Any(row => row.ItemArray.Any(item => !string.IsNullOrEmpty(item.ToString()))))
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
