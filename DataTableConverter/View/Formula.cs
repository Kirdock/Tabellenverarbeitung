using CheckComboBoxTest;
using DataTableConverter.Classes;
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
    public partial class Formula : Form
    {
        private FormulaState Type;

        internal Formula(FormulaState type, object[] headers)
        {
            InitializeComponent();
            Type = type;
            adjustForm();
            
            cbHeaders.Items.AddRange(headers);
            if (Type == FormulaState.Export)
            {
                loadSetting();
            }
        }

        private void loadSetting()
        {
            if (Properties.Settings.Default.ExportSpec != null)
            {

                foreach (string header in Properties.Settings.Default.ExportSpec)
                {
                    int index = cbHeaders.Items.IndexOf(header);
                    if (index != -1)
                    {
                        cbHeaders.SetItemChecked(index, true);
                    }
                }
            }
        }

        internal string[] getSelectedHeaders()
        {
            return ViewHelper.GetSelectedHeaders(cbHeaders);
        }

        private void adjustForm()
        {
            //Bei Funktion: Keine Formel
            //Bei Export: Nur Spaltenauswahl
            txtFormula.Visible = lblFormula.Visible = cbNewColumn.Checked = Type == FormulaState.Merge;
            cbNewColumn.Visible = Type == FormulaState.Procedure;

            if (btnUncheckAll.Visible = btnCheckAll.Visible = Type != FormulaState.Merge)
            {
                int height = 40;
                adjustControlHeight(cbNewColumn, height);
                adjustControlHeight(lblHeader, height);
                adjustControlHeight(txtHeader, height);
                Height -= height;
            }
            setNewColumnVisibility();
        }

        private void adjustControlHeight(Control control, int height)
        {
            control.Location = new Point(control.Location.X, control.Location.Y - height);
        }

        private void setNewColumnVisibility()
        {
            lblHeader.Visible = txtHeader.Visible = cbNewColumn.Checked;
        }

        private void addColumn(string column)
        {
            if (!txtFormula.Text.Contains(column))
            {
                string separator = txtFormula.Text.Length > 0 ? " " : string.Empty;
                txtFormula.Text = $"{txtFormula.Text}{separator}[{column}]";
            }
        }

        internal string getHeaderName()
        {
            return txtHeader.Text;
        }

        private void removeColumn(string column)
        {
            txtFormula.Text = txtFormula.Text.Replace($" [{column}]", "");
        }

        private void txtFormula_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                if (cbNewColumn.Checked && string.IsNullOrWhiteSpace(txtHeader.Text))
                {
                    MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Bitte geben Sie einen Spaltennamen an!");
                }
                else if(cbNewColumn.Checked && isDuplicate(txtHeader.Text))
                {
                    MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Es gibt bereits eine Spalte mit diesem Namen.\nBitte geben Sie einen anderen an");
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private bool isDuplicate(string text)
        {
            return cbHeaders.Items.Contains(text);
        }

        internal string getFormula()
        {
            return txtFormula.Text;
        }

        private void cbNewColumn_CheckedChanged(object sender, EventArgs e)
        {
            setNewColumnVisibility();
            if (!cbNewColumn.Checked)
            {
                txtHeader.Text = string.Empty;
            }
        }

        private void cbHeaders_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                addColumn(cbHeaders.Items[e.Index].ToString());
            }
            else
            {
                removeColumn(cbHeaders.Items[e.Index].ToString());
            }
        }

        private void Formula_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.ExportSpec = new System.Collections.ArrayList(cbHeaders.CheckedItems.Cast<string>().ToArray());
            Properties.Settings.Default.Save();
        }

        private void btnCheckAll_Click(object sender, EventArgs e)
        {
            ViewHelper.CheckAllItemsOfCheckedCombobox(cbHeaders, true);
        }


        private void btnUncheckAll_Click(object sender, EventArgs e)
        {
            ViewHelper.CheckAllItemsOfCheckedCombobox(cbHeaders, false);
        }
    }
}
