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
        private int ComboBoxIndex;
        internal SelectDuplicateColumns(string[] caseHeaders, object[] headers)
        {
            InitializeComponent();
            setDataGridView(caseHeaders, headers);
            ViewHelper.AdjustComboBoxGridView(dgDuplicate, ComboBoxIndex, headers);
        }

        private void setDataGridView(string[] caseHeaders, object[] headers)
        {
            DataTable table = new DataTable { TableName = "Duplicates" };
            table.Columns.Add("Spalte");
            table.Columns.Add("Zuweisung");

            dgDuplicate.DataSource = table;

            DataGridViewComboBoxColumn cmb = new DataGridViewComboBoxColumn
            {
                DataSource = headers,
                DataPropertyName = "Zuweisung",
                HeaderText = "Zuweisung "
            };
            ComboBoxIndex = dgDuplicate.Columns.Count;
            dgDuplicate.Columns.Add(cmb);

            foreach (string header in caseHeaders)
            {
                table.Rows.Add(header);
            }
            dgDuplicate.Columns[0].ReadOnly = true;
            dgDuplicate.Columns[1].Visible = false;
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
    }
}
