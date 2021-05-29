using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DataTableConverter.View
{
    public partial class TextFormat : Form
    {
        private readonly string path;
        private EventHandler ctxRowDeleteRowHandler;
        private EventHandler ctxRowClipboard;
        internal bool TakeOver { get { return cbTakeOver.Checked; } }
        internal ImportSettings ImportSettings;
        private readonly ViewHelper ViewHelper;
        private List<string> Separators = new List<string>();
        private readonly string MultiSeparatorText = "(Mehrfach)";
        private readonly ContextMenuStrip GlobalContext;
        private readonly int DetectedEncoding;
        private readonly string TableName;
        private readonly DatabaseHelper DatabaseHelper;
        private readonly ImportHelper ImportHelper;
        private readonly ExportHelper ExportHelper;

        internal TextFormat(DatabaseHelper databaseHelper, ImportHelper importHelper, ExportHelper exportHelper, string tableName, string path, bool multipleFiles, ContextMenuStrip ctxRow, bool isDifferentExtention)
        {
            InitializeComponent();
            LblDifferentExtension.Visible = isDifferentExtention;
            DatabaseHelper = databaseHelper;
            ImportHelper = importHelper;
            ExportHelper = exportHelper;
            TableName = tableName;
            GlobalContext = ctxRow;
            ViewHelper = new ViewHelper(ctxRow, EndHeadersEdit, null);
            SetHeaderDataGridView();
            SetSize();
            this.path = path;
            ViewHelper.SetEncodingCmb(cmbEncoding);
            cmbEncoding.SelectedValue = DetectedEncoding = DetectEncoding(path).CodePage;
            cbTakeOver.Visible = multipleFiles;
            SetDataGridViewStyle();
            SetFileName(Path.GetFileName(path));
        }

        private void TextFormat_Load(object sender, EventArgs e)
        {
            LoadPresets();
            LoadHeaderPresets();
            cmbVariant.SelectedIndex = 0;
            adjustSettingsDataGrid();
            dgvSetting.CellValueChanged += new DataGridViewCellEventHandler(dgvSetting_CellValueChanged);
            loadSettings();
            cmbEncoding.SelectedIndexChanged += (sender2, e2) => cmbEncoding_SelectedIndexChanged(sender2, e2);
        }

        private void SetFileName(string fileName)
        {
            LblFileName.Text = fileName;
        }

        private Encoding DetectEncoding(string filename)
        {
            byte[] b = File.ReadAllBytes(filename);
            DetectTextEncoding detect = new DetectTextEncoding();
            return detect.GetEncoding(detect.DetectEncoding(b, b.Length));
        }

        private void SetDataGridViewStyle()
        {
            DataGridView[] dataGridViews = new DataGridView[]
            {
                dgvHeaders,
                dgvPreview,
                dgvSetting
            };
            foreach (DataGridView view in dataGridViews)
            {
                ViewHelper.SetDataGridViewStyle(view);
            }
        }

        private void SetHeaderDataGridView()
        {
            DataTable table = new DataTable { TableName = "Columns" };
            table.Columns.Add("Überschrift", typeof(string));
            dgvHeaders.DataSource = table;
            ViewHelper.AddContextMenuToDataGridView(dgvHeaders, this, true);
        }

        private void SetSize()
        {
            if (Properties.Settings.Default.TextFormatMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                Size = Properties.Settings.Default.TextFormatSize;
            }
        }

        private void cmbEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckEncoding((int)cmbEncoding.SelectedValue);
        }

        private void CheckEncoding(int codePage)
        {
            LblCodePageMessage.Visible = DetectedEncoding != codePage;
        }

        private bool IsDetectedEncoding()
        {
            return DetectedEncoding == (int)cmbEncoding.SelectedValue;
        }

        private void loadSettings()
        {
            int radioSelected = Properties.Settings.Default.TabSelected;
            rbSeparated.Checked = Properties.Settings.Default.TextSeparated;
            rbFixed.Checked = !Properties.Settings.Default.TextSeparated;
            txtSeparator.Text = Properties.Settings.Default.Separator;
            if (txtSeparator.Text.Length > 0)
            {
                Separators = new List<string> { txtSeparator.Text };
            }

            txtBegin.Text = Properties.Settings.Default.TextBegin;
            txtEnd.Text = Properties.Settings.Default.TextEnd;
            cbTakeOver.Checked = Properties.Settings.Default.TakeOverAllFiles;
            cbContainsHeaders.Checked = Properties.Settings.Default.HeaderInFirstRow;
            rbTab.Checked = radioSelected == 0;
            rbSep.Checked = radioSelected == 1;
            rbBetween.Checked = radioSelected == 2;
            radioButton_CheckedChanged(null, null);
        }

        private void adjustSettingsDataGrid()
        {
            DataTable table;
            if (dgvSetting.DataSource == null)
            {
                table = new DataTable { TableName = "Setting" };
                table.Columns.Add("Bezeichnung", typeof(string));
            }
            else
            {
                table = ((DataTable)dgvSetting.DataSource).Copy();
                for (int count = table.Columns.Count - 1; count > 0; count--)
                {
                    table.Columns.RemoveAt(count);
                }
            }

            if (cmbVariant.SelectedIndex == 0)
            {
                table.Columns.Add("Länge", typeof(int));
            }
            else
            {
                table.Columns.Add("Von", typeof(int));
                table.Columns.Add("Bis", typeof(int));
            }
            dgvSetting.DataSource = table;

            SetSortMode();
        }

        private void SetSortMode()
        {
            foreach (DataGridViewColumn col in dgvSetting.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void LoadPresets()
        {
            try
            {
                cmbPresets.Items.Clear();
                cmbPresets.Items.AddRange(ImportHelper.LoadPresetsByName());
                if (cmbPresets.Items.Count > 0)
                {
                    cmbPresets.SelectedIndex = 0;
                }
            }
            catch (Exception) { }
        }

        private void LoadHeaderPresets()
        {
            try
            {
                cmbHeaderPresets.Items.Clear();
                cmbHeaderPresets.Items.AddRange(ImportHelper.LoadHeaderPresetsByName());
                if (cmbHeaderPresets.Items.Count > 0)
                {
                    cmbHeaderPresets.SelectedIndex = 0;
                }
            }
            catch (Exception) { }
        }

        void dgvSetting_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            ViewHelper.HandleDataGridViewNumber(sender, e);
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            gbFixed.Visible = rbFixed.Checked;
            gbSeparated.Visible = rbSeparated.Checked;
            if (!rbSeparated.Checked)
            {
                dgvSetting_CellValueChanged(null, null);
            }
            else
            {
                dgvPreview.DataSource = null;
            }
        }

        private void TextFormat_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.TextSeparated = rbSeparated.Checked;
            if (txtSeparator.Text != MultiSeparatorText)
            {
                Properties.Settings.Default.Separator = txtSeparator.Text;
            }
            Properties.Settings.Default.TabSelected = rbTab.Checked ? 0 : rbSep.Checked ? 1 : 2;
            Properties.Settings.Default.Encoding = (int)cmbEncoding.SelectedValue;
            Properties.Settings.Default.TextBegin = txtBegin.Text;
            Properties.Settings.Default.TextEnd = txtEnd.Text;
            Properties.Settings.Default.TakeOverAllFiles = cbTakeOver.Checked;
            Properties.Settings.Default.HeaderInFirstRow = cbContainsHeaders.Checked;
            if (WindowState != FormWindowState.Maximized)
            {
                Properties.Settings.Default.TextFormatSize = Size;
            }
            Properties.Settings.Default.TextFormatMaximized = WindowState == FormWindowState.Maximized;
            Properties.Settings.Default.Save();
            ViewHelper.Clear();
        }

        private void btnAcceptSeparate_Click(object sender, EventArgs e)
        {
            if (EncodingPromptContinue())
            {
                List<string> separator = null;
                object[] headers = GetHeaders();
                bool valid = false;
                if (rbSep.Checked && txtSeparator.Text != null && txtSeparator.Text.Length > 0)
                {
                    separator = txtSeparator.ReadOnly ? Separators : new List<string> { txtSeparator.Text };
                }
                else if (rbTab.Checked)
                {
                    separator = new List<string> { "\t" };
                }
                else if (rbBetween.Checked && checkBetweenText())
                {
                    ImportSettings = new ImportSettings(getCodePage(), txtBegin.Text, txtEnd.Text, cbContainsHeaders.Checked, headers);
                    valid = true;
                }

                if (separator != null)
                {
                    valid = true;
                    ImportSettings = new ImportSettings(separator, getCodePage(), cbContainsHeaders.Checked, headers);
                }

                if (valid)
                {
                    DialogResult = DialogResult.OK;
                }
            }
        }

        private object[] GetHeaders()
        {
            return (dgvHeaders.DataSource as DataTable)?.ColumnValues(0).Where(element => !string.IsNullOrWhiteSpace(element?.ToString())).ToArray() ?? new object[0];
        }

        private bool EncodingPromptContinue()
        {
            DialogResult result = DialogResult.Yes;
            if (!IsDetectedEncoding())
            {
                result = MessageHandler.MessagesYesNo(this, MessageBoxIcon.Warning, "Die CodePage entspricht nicht der festgestellten CodePage\nTrotzdem fortfahren?");
            }
            return result == DialogResult.Yes;
        }

        private void btnAcceptFixed_Click(object sender, EventArgs e)
        {
            if (EncodingPromptContinue())
            {
                GetDataGridViewItems(out List<int> values, out List<string> headers);
                ImportSettings = new ImportSettings(values, headers, getCodePage(), CBFixedHasRowBreak.Checked);
            }
        }

        private void dgvSetting_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // sync preview
            ViewHelper.EndDataGridViewEdit(dgvSetting);
            GetDataGridViewItems(out List<int> values, out List<string> headers);
            bool created = ImportHelper.OpenTextFixed(TableName, path, values, headers, (cmbEncoding.SelectedItem as NewEncodingInfo).CodePage, true, CBFixedHasRowBreak.Checked, null, this);
            if (created)
            {
                dgvPreview.DataSource = DatabaseHelper.GetData(TableName);
            }
        }

        private bool CheckFromToEntered(DataGridViewCellValidatingEventArgs e)
        {
            bool valid = true;
            string currentElement = e.FormattedValue?.ToString();

            if (!string.IsNullOrWhiteSpace(currentElement) && e.ColumnIndex > 0 && dgvSetting.Columns.Count > 2)
            {
                int to, from;
                if (e.ColumnIndex == 1)
                {
                    string val = dgvSetting[e.ColumnIndex + 1, e.RowIndex].Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        from = int.Parse(currentElement);
                        to = int.Parse(val);

                        valid = checkFromTo(from, to);
                    }
                }
                else
                {
                    string val = dgvSetting[e.ColumnIndex - 1, e.RowIndex].Value?.ToString();
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
            if (!txtSeparator.ReadOnly && txtSeparator.Text != null && txtSeparator.Text.Length > 0)
            {
                ImportHelper.OpenText(TableName, path, new List<string> { txtSeparator.Text }, getCodePage(), cbContainsHeaders.Checked, GetHeaders(), true, null, this);
                dgvPreview.DataSource = DatabaseHelper.GetData(TableName);
            }
        }

        private int getCodePage()
        {
            return (int)cmbEncoding.SelectedValue;
        }
        private void btnSavePreset_Click(object sender, EventArgs e)
        {
            string filename = Microsoft.VisualBasic.Interaction.InputBox("Bitte Name der Vorlage eingeben", "Vorlage speichern", string.Empty);
            if (!string.IsNullOrWhiteSpace(filename))
            {
                string path = Path.Combine(ExportHelper.ProjectPresets, $"{filename}.bin");
                if (File.Exists(path))
                {
                    this.MessagesOK(MessageBoxIcon.Warning, "Es existiert bereits eine Datei mit demselben Namen!");
                    btnSavePreset_Click(null, null);
                }
                else
                {
                    TextImportTemplate template = new TextImportTemplate()
                    {
                        Encoding = (int)cmbEncoding.SelectedValue,
                        Table = (dgvSetting.DataSource as DataTable),
                        Variant = cmbVariant.SelectedIndex,
                        HasRowBreak = CBFixedHasRowBreak.Checked
                    };
                    ExportHelper.SaveTextImportTemplate(template, path);
                    LoadPresets();
                }
            }
        }

        private void btnLoadPreset_Click(object sender, EventArgs e)
        {
            if (cmbPresets.SelectedIndex != -1)
            {
                string path = Path.Combine(ExportHelper.ProjectPresets, $"{cmbPresets.SelectedItem}.bin");
                if (File.Exists(path))
                {
                    TextImportTemplate template = ImportHelper.LoadTextImportTemplate(path);
                    if (template != null)
                    {
                        cmbVariant.SelectedIndex = template.Variant;
                        cmbEncoding.SelectedValue = template.Encoding;
                        CBFixedHasRowBreak.CheckedChanged -= CBFixedHasRowBreak_CheckedChanged;
                        CBFixedHasRowBreak.Checked = template.HasRowBreak;
                        CBFixedHasRowBreak.CheckedChanged += CBFixedHasRowBreak_CheckedChanged;

                        dgvSetting.DataSource = template.Table;
                        SetSortMode();
                        dgvSetting_CellValueChanged(null, null);
                    }
                }
                else
                {
                    this.MessagesOK(MessageBoxIcon.Warning, "Die Datei konnte nicht gefunden werden");
                }
            }
        }

        private void dgvSetting_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                foreach (DataGridViewRow row in dgvSetting.Rows)
                {
                    if (row.Cells[0].Value != null && row.Cells[0].Value.Equals(e.FormattedValue) && row.Index != e.RowIndex)
                    {
                        e.Cancel = true;
                        this.MessagesOK(MessageBoxIcon.Warning, "Es gibt bereits eine Spalte mit dieser Bezeichnung!");
                        break;
                    }
                }
            }
            else if (e.ColumnIndex > 0 && cmbVariant.SelectedIndex != 0 && !CheckFromToEntered(e))
            {
                e.Cancel = true;
                this.MessagesOK(MessageBoxIcon.Warning, "\"Von\" darf \"Bis\"nicht überschreiten!");
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
                DialogResult result = this.MessagesYesNoCancel(MessageBoxIcon.Warning, "Wollen Sie die Vorlage wirklich löschen?");
                if (result == DialogResult.Yes)
                {
                    string path = Path.Combine(ExportHelper.ProjectPresets, $"{cmbPresets.SelectedItem.ToString()}.bin");
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        LoadPresets();
                    }
                }
            }
        }

        private void dgvSetting_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            ViewHelper.AddNumerationToDataGridView(sender, e, Font);
        }

        private void GetDataGridViewItems(out List<int> values, out List<string> headers)
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
                        this.MessagesOK(MessageBoxIcon.Warning, message);
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
            if (table.Rows.Count == 0 || !table.Rows[0].ItemArray[1].ToString().Equals("1"))
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
                    if ((bis1 + 1) != von2)
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
                ImportHelper.OpenText(TableName, path, new List<string> { "\t" }, getCodePage(), cbContainsHeaders.Checked, (dgvHeaders.DataSource as DataTable)?.ColumnValues(0) ?? new object[0], true, null, this);
                dgvPreview.DataSource = DatabaseHelper.GetData(TableName);
            }

            else if ((txtBegin.ReadOnly = txtEnd.ReadOnly = rbSep.Checked))
            {
                bool multiSeparator = Separators.Count > 1;
                txtSeparator.ReadOnly = multiSeparator;
                if (multiSeparator || txtSeparator.Text != null && txtSeparator.Text.Length > 0)
                {
                    ImportHelper.OpenText(TableName, path, multiSeparator ? Separators : new List<string> { txtSeparator.Text }, getCodePage(), cbContainsHeaders.Checked, GetHeaders(), true, null, this);
                    dgvPreview.DataSource = DatabaseHelper.GetData(TableName);
                }
            }
            else if (txtSeparator.ReadOnly = rbBetween.Checked)
            {
                txtBegin.ReadOnly = txtEnd.ReadOnly = false;
                if (checkBetweenText())
                {
                    ImportHelper.OpenTextBetween(TableName, path, getCodePage(), txtBegin.Text, txtEnd.Text, cbContainsHeaders.Checked, GetHeaders(), true, null, this);
                    dgvPreview.DataSource = DatabaseHelper.GetData(TableName);
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

                zwischenablageEinfügenToolStripMenuItem.Click += ctxRowClipboard = (sender2, e2) => zwischenablageEinfügenToolStripMenuItem_Click(sender, selectedRow);

                if (zeileLöschenToolStripMenuItem.Visible = (selectedRow > -1 && selectedRow != sender.Rows.Count - 1))
                {
                    if (ctxRowDeleteRowHandler != null)
                    {
                        zeileLöschenToolStripMenuItem.Click -= ctxRowDeleteRowHandler;
                    }
                    List<int> selectedRows = ViewHelper.SelectedRows(sender);

                    zeileLöschenToolStripMenuItem.Text = (selectedRows.Count > 1) ? "Zeilen löschen" : "Zeile löschen";
                    zeileLöschenToolStripMenuItem.Click += ctxRowDeleteRowHandler = (sender2, e2) => zeileLöschenToolStripMenuItem_Click(sender, selectedRows);
                }
                ctxRow.Show(sender, new Point(e.X, e.Y));
            }
        }

        private void zeileLöschenToolStripMenuItem_Click(DataGridView dgView, List<int> rowIndizes)
        {
            for (int i = rowIndizes.Count - 1; i >= 0; i--)
            {
                dgView.Rows.RemoveAt(rowIndizes[i]);
            }
            dgvSetting_CellValueChanged(null, null);
        }

        private void zwischenablageEinfügenToolStripMenuItem_Click(object sender, int selectedRow)
        {
            ViewHelper.InsertClipboardToDataGridView((DataGridView)sender, selectedRow, this, dgvSetting_CellValidating, dgvSetting_CellValueChanged);
        }

        private bool checkBetweenText()
        {
            return txtBegin.Text.Length > 0 && txtEnd.Text.Length > 0;
        }

        private void BtnRenamePreset_Click(object sender, EventArgs e)
        {
            if (cmbPresets.SelectedIndex != -1)
            {
                string oldName = cmbPresets.SelectedItem.ToString();
                string newName = Microsoft.VisualBasic.Interaction.InputBox("Bitte Vorlagenname eingeben", "Vorlage umbenennen", oldName);
                if (!string.IsNullOrWhiteSpace(newName) && oldName != newName)
                {
                    if (cmbPresets.Items.Contains(newName))
                    {
                        this.MessagesOK(MessageBoxIcon.Warning, "Es gibt bereits eine Vorlage mit demselben Namen\nBitte wählen Sie einen anderen");
                        BtnRenamePreset_Click(sender, e);
                    }
                    else
                    {
                        string path = Path.Combine(ExportHelper.ProjectPresets, $"{oldName}.bin");
                        string newPath = Path.Combine(ExportHelper.ProjectPresets, $"{newName}.bin");

                        File.Move(path, newPath);
                        LoadPresets();
                    }
                }
            }
        }

        private void dgvHeaders_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            EndHeadersEdit(sender, e);
        }

        private void EndHeadersEdit(object sender, EventArgs e)
        {
            if (!cbContainsHeaders.Checked)
            {
                ViewHelper.EndDataGridViewEdit(dgvHeaders);
                radioButton2_CheckedChanged(null, null);
            }
        }

        private void dgvHeaders_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            ViewHelper.AddNumerationToDataGridView(sender, e, Font);
        }

        private void btnHeaderSave_Click(object sender, EventArgs e)
        {
            string filename = Microsoft.VisualBasic.Interaction.InputBox("Bitte Name der Vorlage eingeben", "Vorlage speichern", string.Empty);
            if (!string.IsNullOrWhiteSpace(filename))
            {
                string path = Path.Combine(ExportHelper.ProjectHeaderPresets, filename + ".bin");
                if (File.Exists(path))
                {
                    this.MessagesOK(MessageBoxIcon.Warning, "Es existiert bereits eine Überschriftenvorlage mit demselben Namen!");
                    btnHeaderSave_Click(sender, e);
                }
                else
                {
                    TextImportTemplate template = new TextImportTemplate()
                    {
                        Encoding = (int)cmbEncoding.SelectedValue,
                        Table = (dgvHeaders.DataSource as DataTable),
                        ContainsHeaders = cbContainsHeaders.Checked,
                        BeginSeparator = txtBegin.Text,
                        EndSeparator = txtEnd.Text,
                        Separators = Separators.Count > 0 ? Separators : new List<string> { txtSeparator.Text },
                        SelectedSeparated = rbTab.Checked ? TextImportTemplate.SelectedSeparatedState.Tab : rbSep.Checked ? TextImportTemplate.SelectedSeparatedState.TabCharacter : TextImportTemplate.SelectedSeparatedState.Between
                    };
                    ExportHelper.SaveTextImportTemplate(template, path);
                    LoadHeaderPresets();
                }
            }
        }

        private void btnHeaderLoad_Click(object sender, EventArgs e)
        {
            if (cmbHeaderPresets.SelectedIndex != -1)
            {
                string path = Path.Combine(ExportHelper.ProjectHeaderPresets, cmbHeaderPresets.SelectedItem.ToString() + ".bin");
                if (File.Exists(path))
                {
                    TextImportTemplate template = ImportHelper.LoadTextImportTemplate(path);
                    if (template != null)
                    {
                        cmbEncoding.SelectedValue = template.Encoding;

                        rbTab.Checked = template.SelectedSeparated == TextImportTemplate.SelectedSeparatedState.Tab;
                        rbSep.Checked = template.SelectedSeparated == TextImportTemplate.SelectedSeparatedState.TabCharacter;
                        rbBetween.Checked = template.SelectedSeparated == TextImportTemplate.SelectedSeparatedState.Between;

                        txtBegin.Text = template.BeginSeparator;
                        txtEnd.Text = template.EndSeparator;
                        txtSeparator.ReadOnly = template.Separators.Count > 1;
                        txtSeparator.Text = template.Separators.Count == 1 ? template.Separators.First() : template.separators.Count > 1 ? MultiSeparatorText : string.Empty;
                        Separators = template.Separators;
                        dgvHeaders.DataSource = template.Table;
                        cbContainsHeaders.Checked = template.ContainsHeaders;
                        radioButton2_CheckedChanged(null, null);
                    }
                }
                else
                {
                    this.MessagesOK(MessageBoxIcon.Warning, "Die Datei konnte nicht gefunden werden");
                }
            }
        }

        private void btnHeaderDelete_Click(object sender, EventArgs e)
        {
            if (cmbHeaderPresets.SelectedIndex != -1)
            {
                DialogResult result = this.MessagesYesNoCancel(MessageBoxIcon.Warning, "Wollen Sie die Vorlage wirklich löschen?");
                if (result == DialogResult.Yes)
                {
                    string path = Path.Combine(ExportHelper.ProjectHeaderPresets, cmbHeaderPresets.SelectedItem.ToString() + ".bin");
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        LoadHeaderPresets();
                    }
                }
            }
        }

        private void btnHeaderRename_Click(object sender, EventArgs e)
        {
            if (cmbHeaderPresets.SelectedIndex != -1)
            {
                string oldName = cmbHeaderPresets.SelectedItem.ToString();
                string newName = Microsoft.VisualBasic.Interaction.InputBox("Bitte Vorlagenname eingeben", "Vorlage umbenennen", oldName);
                if (!string.IsNullOrWhiteSpace(newName) && oldName != newName)
                {
                    if (cmbHeaderPresets.Items.Contains(newName))
                    {
                        this.MessagesOK(MessageBoxIcon.Warning, "Es gibt bereits eine Vorlage mit demselben Namen\nBitte wählen Sie einen anderen");
                        btnHeaderRename_Click(sender, e);
                    }
                    else
                    {
                        string path = Path.Combine(ExportHelper.ProjectHeaderPresets, $"{oldName}.bin");
                        string newPath = Path.Combine(ExportHelper.ProjectHeaderPresets, $"{newName}.bin");

                        File.Move(path, newPath);
                        LoadHeaderPresets();
                    }
                }
            }
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

        private void BtnEditSeparators_Click(object sender, EventArgs e)
        {
            if (rbSep.Checked)
            {
                SeparatorForm form = new SeparatorForm(Separators, GlobalContext);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    Separators = form.Separators;
                    if ((txtSeparator.ReadOnly = Separators.Count > 1))
                    {
                        txtSeparator.Text = MultiSeparatorText;
                    }
                    else if (Separators.Count == 1)
                    {
                        txtSeparator.Text = Separators.First();
                    }
                    ImportHelper.OpenText(TableName, path, Separators, getCodePage(), cbContainsHeaders.Checked, GetHeaders(), true, null, this);
                    dgvPreview.DataSource = DatabaseHelper.GetData(TableName);
                }
                form.Dispose();
            }
        }

        private void dgvPreview_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            e.Column.FillWeight = 10;
        }

        private void btnSyncPreview_Click(object sender, EventArgs e)
        {
            radioButton2_CheckedChanged(null, null);
        }

        private void CBFixedHasRowBreak_CheckedChanged(object sender, EventArgs e)
        {
            dgvSetting_CellValueChanged(null, null);
        }
    }

}
