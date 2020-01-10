using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataTableConverter.Extensions;

namespace DataTableConverter.View
{
    public partial class SeparatorForm : Form
    {
        internal List<string> Separators
        {
            get
            {
                return (DGVSeparators.DataSource as DataTable).ColumnValuesAsString(0).Where(separator => separator.Length > 0).ToList();
            }
        }
        private ViewHelper ViewHelper;


        internal SeparatorForm(List<string> separators, ContextMenuStrip ctxRow)
        {
            InitializeComponent();
            ViewHelper = new ViewHelper(ctxRow, null, null);
            InitDataGridView(separators);
            ViewHelper.SetDataGridViewStyle(DGVSeparators);
            ViewHelper.AddContextMenuToDataGridView(DGVSeparators, this, false);

        }

        private void InitDataGridView(List<string> separators)
        {
            DataTable table = new DataTable { TableName = "Separators" };
            table.Columns.Add("Trennzeichen", typeof(string));
            foreach(string separator in separators)
            {
                table.Rows.Add(separator);
            }
            DGVSeparators.DataSource = table;
        }

        private void SeparatorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ViewHelper.Clear();
        }

        private void DGVSeparators_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ViewHelper.EndDataGridViewEdit(DGVSeparators);
        }


        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            ViewHelper.EndDataGridViewEdit(DGVSeparators);
            DialogResult = DialogResult.OK;
        }
    }
}
