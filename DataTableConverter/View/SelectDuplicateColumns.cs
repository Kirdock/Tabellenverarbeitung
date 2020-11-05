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
        internal SelectDuplicateColumns(string[] caseHeaders, Dictionary<string,string> importTableColumnAliasMapping, bool mustBeAssigned, string heading = null, string firstColumnName = null)
        {
            InitializeComponent();
            if(heading != null)
            {
                Text = heading;
            }
            setDataGridView(caseHeaders, importTableColumnAliasMapping, mustBeAssigned, firstColumnName);
            ViewHelper.AdjustComboBoxGridView(dgDuplicate, ComboBoxIndex, importTableColumnAliasMapping.Values.Cast<object>().ToArray());
            ViewHelper.SetDataGridViewStyle(dgDuplicate);
        }

        internal SelectDuplicateColumns(string[] caseHeaders, string[] headers, bool mustBeAssigned, string heading = null, string firstColumnName = null)
        {
            InitializeComponent();
            if (heading != null)
            {
                Text = heading;
            }
            setDataGridView(caseHeaders, headers, mustBeAssigned, firstColumnName);
            ViewHelper.AdjustComboBoxGridView(dgDuplicate, ComboBoxIndex, headers);
            ViewHelper.SetDataGridViewStyle(dgDuplicate);
        }

        private void setDataGridView(string[] caseHeaders, object importTableColumnAliasMapping, bool mustBeAssigned, string firstColumnName)
        {
            DataTable table = new DataTable { TableName = "Duplicates" };
            table.Columns.Add(firstColumnName ?? "Spalte");
            table.Columns.Add("Zuweisung");
            Dictionary<string,string> newHeadersDict = new Dictionary<string, string>();
            string[] newHeadersArray = new string[0];
            string firstValue;
            bool isDictionary = importTableColumnAliasMapping is Dictionary<string, string>;

            if (isDictionary)
            {
                if (mustBeAssigned)
                {
                    newHeadersDict = (Dictionary<string, string>)importTableColumnAliasMapping;
                }
                else
                {
                    newHeadersDict = new Dictionary<string, string>
                {
                    { IgnoreColumn, null },
                };
                    foreach (KeyValuePair<string, string> pair in (Dictionary<string, string>)importTableColumnAliasMapping)
                    {
                        newHeadersDict.Add(pair.Key, pair.Value);
                    }
                }
                firstValue = newHeadersDict.First().Value;
            }
            else
            {
                if (mustBeAssigned)
                {
                    newHeadersArray = (string[]) importTableColumnAliasMapping;
                }
                else
                {
                    string[] headers = (string[])importTableColumnAliasMapping;
                    newHeadersArray = new string[headers.Length + 1];
                    newHeadersArray[0] = IgnoreColumn;
                    for (int i = 0; i < headers.Length; i++)
                    {
                        newHeadersArray[i + 1] = headers[i];
                    }
                }
                firstValue = newHeadersArray[0];
            }
             

            dgDuplicate.DataSource = table;

            DataGridViewComboBoxColumn cmb;
            if (isDictionary)
            {
                cmb = new DataGridViewComboBoxColumn()
                {
                    DataSource = new BindingSource(newHeadersDict, null),
                    DisplayMember = "key",
                    ValueMember = "value",

                };
            }
            else
            {
                cmb = new DataGridViewComboBoxColumn()
                {
                    DataSource = newHeadersArray,
                };
            }
            
            cmb.DataPropertyName = "Zuweisung";
            cmb.HeaderText = "Zuweisung ";

            ComboBoxIndex = dgDuplicate.Columns.Count;
            dgDuplicate.Columns.Add(cmb);
            
            foreach (string header in caseHeaders)
            {
                table.Rows.Add(new object[] { header, firstValue });
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
            ViewHelper.EndDataGridViewEdit(dgDuplicate);
            return ((DataTable)dgDuplicate.DataSource).Copy();
        }

        private void setDataTable()
        {
            ViewHelper.EndDataGridViewEdit(dgDuplicate);
            Table = ((DataTable)dgDuplicate.DataSource).Copy();

        }
    }
}
