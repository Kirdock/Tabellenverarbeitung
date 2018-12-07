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
    public partial class SelectDuplicateColumns : Form
    {
        internal DataTable Table { get; set; }
        internal SelectDuplicateColumns(string[] caseHeaders, object[] headers)
        {
            InitializeComponent();
            setDataGridView(caseHeaders, headers);
        }

        private void setDataGridView(string[] caseHeaders, object[] headers)
        {
            DataTable table = new DataTable { TableName = "Duplicates" };
            table.Columns.Add("Spalte");
            table.Columns.Add("Zuweisung");

            dgDuplicate.DataSource = table;

            DataGridViewComboBoxColumn cmb = new DataGridViewComboBoxColumn();
            cmb.DataSource = headers;
            
            cmb.DataPropertyName = "Zuweisung";

            dgDuplicate.Columns.Add(cmb);

            foreach (string header in caseHeaders)
            {
                table.Rows.Add(header);
            }
            dgDuplicate.Columns[0].ReadOnly = dgDuplicate.Columns[1].ReadOnly = true;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            setDataTable();
            DataTable tab = getDataSource();
            bool isValid = true;
            foreach(DataRow row in tab.Rows)
            {
                if (string.IsNullOrWhiteSpace(row.ItemArray[1]?.ToString()))
                {
                    isValid = false;
                }
            }
            if (isValid)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private DataTable getDataSource()
        {
            dgDuplicate.BindingContext[dgDuplicate.DataSource].EndCurrentEdit();
            return ((DataTable)dgDuplicate.DataSource).Copy();
        }

        private void setDataTable()
        {
            dgDuplicate.BindingContext[dgDuplicate.DataSource].EndCurrentEdit();
            Table = ((DataTable)dgDuplicate.DataSource).Copy();

        }

        private void dgDuplicate_MouseDown(object sender, MouseEventArgs e)
        {
            DataGridView.HitTestInfo info = dgDuplicate.HitTest(e.X, e.Y);

            if (info.Type == DataGridViewHitTestType.Cell)
            {
                switch (info.ColumnIndex)
                {
                    // Add and remove case statements as necessary depending on
                    // which columns have ComboBoxes in them.

                    case 1: // Column index 1
                    case 2: // Column index 3
                        dgDuplicate.CurrentCell =
                            dgDuplicate.Rows[info.RowIndex].Cells[info.ColumnIndex];
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
