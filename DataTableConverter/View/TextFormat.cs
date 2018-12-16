using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class TextFormat : Form
    {
        private string path, data;
        private EventHandler ctxRowDeleteRowHandler;
        private EventHandler ctxRowClipboard;

        internal DataTable DataTable { get; set; }
        internal TextFormat(string path)
        {
            InitializeComponent();
            this.path = path;
            setEncodingCmb();

            
        }

        private void TextFormat_Load(object sender, EventArgs e)
        {
            if (readData())
            {
                DialogResult = DialogResult.Abort;
                Dispose();
            }
            else
            {
                loadPresets();
                cmbVariant.SelectedIndex = 0;
                adjustSettingsDataGrid();
                dgvSetting.CellValueChanged += new DataGridViewCellEventHandler(dgvSetting_CellValueChanged);
                loadSettings();
                radioButton_CheckedChanged(null, null);
                cmbEncoding.SelectedIndexChanged += (sender2, e2) => cmbEncoding_SelectedIndexChanged(sender2, e2);
            }
        }

        private bool readData()
        {
            bool error = false;
            try
            {
                data = File.ReadAllText(path, Encoding.GetEncoding(((EncodingInfo)cmbEncoding.SelectedItem).CodePage));
            }
            catch
            {
                error = true;
                MessageHandler.MessagesOK(MessageBoxIcon.Error, "Die Datei kann nicht geöffnet werden. Wird sie gerade benutzt?");
            }
            return error;
        }


        private void setEncodingCmb()
        {
            cmbEncoding.DataSource = Encoding.GetEncodings();

            cmbEncoding.DisplayMember = "DisplayName";
            cmbEncoding.ValueMember = "CodePage";
            cmbEncoding.SelectedValue = Properties.Settings.Default.Encoding;
        }

        private void cmbEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            readData();
            radioButton_CheckedChanged(null, null);
        }


        private void loadSettings()
        {
            int radioSelected = Properties.Settings.Default.TabSelected;
            rbSeparated.Checked = Properties.Settings.Default.TextSeparated;
            rbFixed.Checked = !Properties.Settings.Default.TextSeparated;
            txtSeparator.Text = Properties.Settings.Default.Separator;
            txtBegin.Text = Properties.Settings.Default.TextBegin;
            txtEnd.Text = Properties.Settings.Default.TextEnd;
            rbTab.Checked = radioSelected == 0;
            rbSep.Checked = radioSelected == 1;
            rbBetween.Checked = radioSelected == 2;
        }

        private void adjustSettingsDataGrid()
        {
            DataTable table;
            if(dgvSetting.DataSource == null)
            {
                table = new DataTable { TableName = "Setting" };
                table.Columns.Add("Bezeichnung", typeof(string));
            }
            else
            {
                table = ((DataTable)dgvSetting.DataSource).Copy();
                for (int count = table.Columns.Count-1; count > 0; count--)
                {
                    table.Columns.RemoveAt(count);
                }
            }
            
            if (cmbVariant.SelectedIndex == 0)
            {
                table.Columns.Add("Länge", typeof(int));
            }
            else{
                table.Columns.Add("Von", typeof(int));
                table.Columns.Add("Bis", typeof(int));
            }
            dgvSetting.DataSource = table;

            foreach (DataGridViewColumn col in dgvSetting.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void loadPresets()
        {
            try
            {
                cmbPresets.Items.Clear();
                cmbPresets.Items.AddRange(Directory.GetFiles(ExportHelper.ProjectPresets, "*.xml")
                                         .Select(Path.GetFileNameWithoutExtension)
                                         .ToArray());
                cmbPresets.SelectedIndex = 0;
            }
            catch (Exception) { }
        }

        void dgvSetting_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            ViewHelper.handleDataGridViewNumber(sender, e);
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            gbFixed.Visible = rbFixed.Checked;
            gbSeparated.Visible = rbSeparated.Checked;
            if (rbSeparated.Checked)
            {
                radioButton2_CheckedChanged(null, null);
            }
            else
            {
                dgvSetting_CellValueChanged(null, null);
            }
        }

        private void TextFormat_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.TextSeparated = rbSeparated.Checked;
            Properties.Settings.Default.Separator = txtSeparator.Text;
            Properties.Settings.Default.TabSelected = rbTab.Checked ? 0 : rbSep.Checked ? 1 : 2;
            Properties.Settings.Default.Encoding = ((EncodingInfo)cmbEncoding.SelectedItem).CodePage;
            Properties.Settings.Default.TextBegin = txtBegin.Text;
            Properties.Settings.Default.TextEnd = txtEnd.Text;
            Properties.Settings.Default.Save();
        }

        private void btnAcceptSeparate_Click(object sender, EventArgs e)
        {
            if(rbSep.Checked && txtSeparator.Text != null && txtSeparator.Text.Length > 0)
            {
                DataTable = ImportHelper.openText(path, txtSeparator.Text, getCodePage());
            }
            else if (rbTab.Checked)
            {
                DataTable = ImportHelper.openText(path, "\t", getCodePage());
            }
            else if (rbBetween.Checked && checkBetweenText())
            {
                DataTable = ImportHelper.openTextBetween(path, getCodePage(), txtBegin.Text, txtEnd.Text);
            }

            if(DataTable != null)
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void btnAcceptFixed_Click(object sender, EventArgs e)
        {
            getDataGridViewItems(out List<int> values, out List<string> headers);
            DataTable = ImportHelper.openTextFixed(data, path, values, headers);
        }

        private void dgvSetting_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // sync preview
            dgvSetting.BindingContext[dgvSetting.DataSource].EndCurrentEdit();
            getDataGridViewItems(out List<int> values, out List<string> headers);
            dgvPreview.DataSource = ImportHelper.openTextFixed(data, path, values, headers, true);
        }

        private bool checkFromToEntered(DataGridViewCellValidatingEventArgs e)
        {
            bool valid = true;
            
            string currentElement = e.FormattedValue?.ToString();
            if (!string.IsNullOrWhiteSpace(currentElement) && e.ColumnIndex > 0)
            {
                int to, from;
                if (e.ColumnIndex == 1)
                {
                    string val = dgvSetting[e.ColumnIndex +1, e.RowIndex].Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        from = int.Parse(currentElement);
                        to = int.Parse(val);

                        valid = checkFromTo(from, to);
                    }
                }
                else
                {
                    string val = dgvSetting[e.ColumnIndex -1, e.RowIndex].Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        to = int.Parse(currentElement);
                        from = int.Parse(val);

                        valid = checkFromTo(from, to);
                    }
                }
            }
            return valid;
        }

        private bool checkFromTo(int from, int to)
        {
            return (to - from) >= 0;
        }

        private void txtSeparator_TextChanged(object sender, EventArgs e)
        {
            if (txtSeparator.Text != null && txtSeparator.Text.Length > 0)
            {
                dgvPreview.DataSource = ImportHelper.openText(path, txtSeparator.Text, getCodePage(), true);
            }
        }

        private int getCodePage()
        {
            return ((EncodingInfo)cmbEncoding.SelectedItem).CodePage;
        }
        private void btnSavePreset_Click(object sender, EventArgs e)
        {
            string filename = Microsoft.VisualBasic.Interaction.InputBox("Bitte Name der Vorlage eingeben", "Vorlage speichern", string.Empty);
            if (!string.IsNullOrWhiteSpace(filename))
            {
                string path = $@"{ ExportHelper.ProjectPresets }\{ filename}.xml";
                if (File.Exists(path))
                {
                    MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Es existiert bereits eine Datei mit demselben Namen!");
                    btnSavePreset_Click(null, null);
                }
                else
                {
                    ((DataTable)dgvSetting.DataSource).WriteXml(path, XmlWriteMode.WriteSchema);
                    loadPresets();
                }
            }
        }

        private void btnLoadPreset_Click(object sender, EventArgs e)
        {
            if(cmbPresets.SelectedIndex != -1)
            {
                try
                {
                    cmbVariant.SelectedIndex = 0;
                    DataTable data = new DataTable();
                    data.ReadXml($@"{ExportHelper.ProjectPresets}\{cmbPresets.SelectedItem.ToString()}.xml");
                    dgvSetting.DataSource = data;
                    dgvSetting_CellValueChanged(null, null);
                }
                catch (IOException)
                {
                    MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Die Datei konnte nicht gefunden werden");
                }
            }
        }

        private void dgvSetting_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == 0) {
                foreach (DataGridViewRow row in dgvSetting.Rows)
                {
                    if (row.Cells[0].Value != null && row.Cells[0].Value.Equals(e.FormattedValue) && row.Index != e.RowIndex)
                    {
                        e.Cancel = true;
                        MessageHandler.MessagesOK(MessageBoxIcon.Warning, "Es gibt bereits eine Spalte mit dieser Bezeichnung!");
                        break;
                    }
                }
            }
            else if(e.ColumnIndex > 0 && cmbVariant.SelectedIndex != 0 && !checkFromToEntered(e))
            {
                e.Cancel = true;
                MessageHandler.MessagesOK(MessageBoxIcon.Warning, "\"Von\" darf \"Bis\"nicht überschreiten!");
            }
        }

        private void cmbVariant_SelectedIndexChanged(object sender, EventArgs e)
        {
            adjustSettingsDataGrid();
        }

        private void btnDeletePreset_Click(object sender, EventArgs e)
        {
            if (cmbPresets.SelectedIndex != -1)
            {
                string path = $@"{ ExportHelper.ProjectPresets }\{cmbPresets.SelectedItem.ToString()}.xml";
                if (File.Exists(path))
                {
                    File.Delete(path);
                    loadPresets();
                }
            }
        }

        private void dgvSetting_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            ViewHelper.addNumerationToDataGridView(sender, e, Font);
        }

        private void getDataGridViewItems(out List<int> values, out List<string> headers)
        {
            values = new List<int>();
            headers = new List<string>();

            DataTable table = ((DataTable)dgvSetting.DataSource).Copy();
            if (cmbVariant.SelectedIndex != 0)
            {
                table.DefaultView.Sort = table.Columns[1].ColumnName + " ASC";
                table = table.DefaultView.ToTable();
                if (!tableValid(table, out string message))
                {
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        MessageHandler.MessagesOK(MessageBoxIcon.Warning, message);
                    }
                    return;
                }
            }

            foreach (DataRow dr in table.Rows)
            {
                string bez = dr.ItemArray[0]?.ToString();
                string config = dr.ItemArray[1]?.ToString();
                if (!string.IsNullOrWhiteSpace(bez) && !string.IsNullOrWhiteSpace(config))
                {
                    int from = int.Parse(config);
                    if (cmbVariant.SelectedIndex != 0)
                    {
                        string toString = dr.ItemArray[2]?.ToString();
                        if (!string.IsNullOrWhiteSpace(toString))
                        {
                            int to = int.Parse(toString);
                            values.Add(to - from + 1);
                            headers.Add(bez);
                        }
                    }
                    else
                    {
                        values.Add(from);
                        headers.Add(bez);
                    }
                }
            }
        }

        private bool tableValid(DataTable table, out string message)
        {
            message = string.Empty;
            if(table.Rows.Count == 0 || !table.Rows[0].ItemArray[1].ToString().Equals("1"))
            {
                return false;
            }

            bool status = removeEmptyRows(table); //false wenn eine Spalte (von oder bis) leer ist
            if (status)
            {
                int row1 = 0, row2 = 0;
                for (int i = 0; i < table.Rows.Count - 1; i++)
                {
                    int bis1 = int.Parse(table.Rows[i].ItemArray[2].ToString()); //kann nicht null und muss eine Zahl sein, da es vorher schon überprüft wird
                    int von2 = int.Parse(table.Rows[i + 1].ItemArray[1].ToString());
                    if((bis1+1) != von2)
                    {
                        status = false;
                        row1 = i + 1;
                        row2 = i + 2;
                        break;
                    }
                    
                }
                if (!status)
                {
                    message = $"Es gibt Überschneidungen oder fehlende Werte bei den Grenzen in den Zeilen {row1} und {row2}";
                }
            }
            else
            {
                message = "Bitte füllen Sie die Werte \"von\" und \"bis\" aus!";
            }
            
            return status;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            dgvPreview.DataSource = null;
            if (txtSeparator.ReadOnly = txtBegin.ReadOnly = txtEnd.ReadOnly = rbTab.Checked)
            {
                dgvPreview.DataSource = ImportHelper.openText(path, "\t", getCodePage(), true);
            }
            
            else if ((txtBegin.ReadOnly = txtEnd.ReadOnly = rbSep.Checked))
            {
                txtSeparator.ReadOnly = false;
                if (txtSeparator.Text != null && txtSeparator.Text.Length > 0)
                {
                    dgvPreview.DataSource = ImportHelper.openText(path, txtSeparator.Text, getCodePage(), true);
                }
            }
            else if (txtSeparator.ReadOnly = rbBetween.Checked)
            {
                txtBegin.ReadOnly = txtEnd.ReadOnly = false;
                if (checkBetweenText())
                {
                    dgvPreview.DataSource = ImportHelper.openTextBetween(path, getCodePage(), txtBegin.Text, txtEnd.Text, true);
                }
            }
        }

        private void dgvSetting_MouseClick(object send, MouseEventArgs e)
        {
            DataGridView sender = (DataGridView)send;
            if (e.Button == MouseButtons.Right)
            {
                int selectedRow = sender.HitTest(e.X, e.Y).RowIndex;
                int selectedColumn = sender.HitTest(e.X, e.Y).ColumnIndex;
                if (selectedColumn > -1 && selectedRow > -1 && !sender.SelectedCells.Contains(sender[selectedColumn, selectedRow]))
                {
                    sender.SelectedCells.Cast<DataGridViewCell>().ToList().ForEach(cell => cell.Selected = false);
                    sender[selectedColumn, selectedRow].Selected = true;
                }
                if (ctxRowClipboard != null)
                {
                    zwischenablageEinfügenToolStripMenuItem.Click -= ctxRowClipboard;
                }

                zwischenablageEinfügenToolStripMenuItem.Click += ctxRowClipboard = (sender2, e2) => zwischenablageEinfügenToolStripMenuItem_Click(sender, e2, selectedRow);

                if (zeileLöschenToolStripMenuItem.Visible = (selectedRow > -1 && selectedRow != sender.Rows.Count - 1))
                {
                    if (ctxRowDeleteRowHandler != null)
                    {
                        zeileLöschenToolStripMenuItem.Click -= ctxRowDeleteRowHandler;
                    }
                    List<int> selectedRows = ViewHelper.SelectedRows(sender);

                    zeileLöschenToolStripMenuItem.Text = (selectedRows.Count > 1) ? "Zeilen löschen" : "Zeile löschen";
                    zeileLöschenToolStripMenuItem.Click += ctxRowDeleteRowHandler = (sender2, e2) => zeileLöschenToolStripMenuItem_Click(sender2, e2, sender, selectedRows);
                }
                ctxRow.Show(sender, new Point(e.X, e.Y));
            }
        }

        private void zeileLöschenToolStripMenuItem_Click(object sender, EventArgs e, DataGridView dgView, List<int> rowIndizes)
        {
            for (int i = rowIndizes.Count - 1; i >= 0; i--)
            {
                dgView.Rows.RemoveAt(rowIndizes[i]);
            }
            dgvSetting_CellValueChanged(null, null);
        }

        private void zwischenablageEinfügenToolStripMenuItem_Click(object sender, EventArgs e, int selectedRow)
        {
            ViewHelper.insertClipboardToDataGridView((DataGridView)sender, selectedRow, dgvSetting_CellValueChanged);
        }

        private void txtBegin_TextChanged(object sender, EventArgs e)
        {
            if (checkBetweenText())
            {
                dgvPreview.DataSource = ImportHelper.openTextBetween(path, getCodePage(), txtBegin.Text, txtEnd.Text, true);
            }
            else
            {
                dgvPreview.DataSource = null;
            }
        }

        private bool checkBetweenText()
        {
            return txtBegin.Text.Length > 0 && txtEnd.Text.Length > 0;
        }



        //Zeilen werden gelöscht, wenn "von" UND "bis" leer ist
        //Zusätzlich wird ein Wahrheitswert übergeben, der true ist, wenn "von" UND "bis" nicht leer sind
        private bool removeEmptyRows(DataTable table)
        {
            bool isValid = true;
            for (int i = 0; i < table.Rows.Count; i++)
            {
                string row1 = table.Rows[i].ItemArray[1]?.ToString();
                string row2 = table.Rows[i].ItemArray[2]?.ToString();
                if (string.IsNullOrWhiteSpace(row1) && string.IsNullOrWhiteSpace(row2))
                {
                    table.Rows.RemoveAt(i);
                    i--;
                }
                //else if (string.IsNullOrWhiteSpace(row1) || string.IsNullOrWhiteSpace(row2))
                //{
                //    isValid = false;
                //}
            }
            return isValid;

        }
    }
}
