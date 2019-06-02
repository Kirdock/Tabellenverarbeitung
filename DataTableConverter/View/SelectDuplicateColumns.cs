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
        internal static readonly string IgnoreColumn = "Ignorieren";
        private int ComboBoxIndex;
        internal SelectDuplicateColumns(string[] caseHeaders, object[] headers, bool mustBeAssigned)
        {
            InitializeComponent();
            setDataGridView(caseHeaders, headers, mustBeAssigned);
            ViewHelper.AdjustComboBoxGridView(dgDuplicate, ComboBoxIndex, headers);
        }

        private void setDataGridView(string[] caseHeaders, object[] headers, bool mustBeAssigned)
        {
            DataTable table = new DataTable { TableName = "Duplicates" };
            table.Columns.Add("Spalte");
            table.Columns.Add("Zuweisung");
            object[] newHeaders;
            if (mustBeAssigned)
            {
                newHeaders = headers;
            }
            else
            {
                newHeaders = new object[headers.Length + 1];
                newHeaders[0] = IgnoreColumn;
                for (int i = 0; i < headers.Length; i++)
                {
                    newHeaders[i + 1] = headers[i];
                }
            }

            dgDuplicate.DataSource = table;

            DataGridViewComboBoxColumn cmb = new DataGridViewComboBoxColumn
            {
                DataSource = newHeaders,
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
