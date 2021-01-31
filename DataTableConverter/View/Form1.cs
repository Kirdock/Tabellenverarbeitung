using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
using DataTableConverter.View;
using DataTableConverter.View.WorkProcViews;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace DataTableConverter
{
    public partial class Form1 : Form
    {
        private int selectedRow = 0, selectedColumn = 0;
        private List<Proc> procedures;
        private List<Work> workflows;
        private List<Tolerance> tolerances;
        private List<Case> cases;
        private string SortingOrder = string.Empty;
        private Dictionary<string, SortOrder> DictSorting;
        private Dictionary<string, string> AliasColumnMapping;
        private string FilePath = string.Empty;
        private OrderType OrderType = OrderType.Windows;
        private readonly Dictionary<string, int> ColumnWidths;
        private readonly int ColumnWidthTolerance = 10;
        private int RowCount = 0;
        private decimal MaxPages = 0;
        internal int FileEncoding = 0;
        private readonly string TableName = DatabaseHelper.DefaultTable;

        //internal because of workflows; Form1 is passed on; no need for additional parameters DatabaseHelper, ImportHelper, ExportHelper
        internal readonly DatabaseHelper DatabaseHelper;
        internal readonly ExportHelper ExportHelper;
        internal readonly ImportHelper ImportHelper;
        private enum SaveFormat { CSV, DBASE, EXCEL };
        private decimal Page
        {
            get
            {
                return NumPage.Value;
            }
            set
            {
                dgTable.RowsAdded -= dgTable_RowsAdded;
                dgTable.AllowUserToAddRows = NumPage.Value == MaxPages;
                dgTable.RowsAdded += dgTable_RowsAdded;
                SetPage();
                LoadData(true);
            }
        }
        internal int ValidRows
        {
            get
            {
                int.TryParse(ValidRowsLabel.Text, out int result);
                return result;
            }

            set
            {
                ValidRowsLabel.Text = value.ToString();
                ValidRowsText.Visible = ValidRowsLabel.Visible = true;
            }
        }

        internal Form1(string databaseName = null, string path = null)
        {
            InitializeComponent();
            DatabaseHelper = new DatabaseHelper(databaseName);
            ExportHelper = new ExportHelper(DatabaseHelper);
            DatabaseHelper.ExportHelper = ExportHelper;
            ImportHelper = new ImportHelper(ExportHelper, DatabaseHelper);
            DictSorting = new Dictionary<string, SortOrder>();
            ColumnWidths = new Dictionary<string, int>();
            SetSize();
            ExportHelper.CheckRequired();
            LoadProcedures();
            LoadWorkflows();
            LoadTolerances();
            LoadCases();
            SetMenuEnabled(false);
            öffnenToolStripMenuItem1.Click += (sender, e) => importToolStripMenuItem_Click();
            cSVToolStripMenuItem.Click += (sender, e) => cSVToolStripMenuItem_Click();
            dBASEToolStripMenuItem1.Click += (sender, e) => dBASEToolStripMenuItem_Click(sender, e);
            excelToolStripMenuItem1.Click += (sender, e) => excelToolStripMenuItem_Click(sender, e);

            ViewHelper.SetDataGridViewStyle(dgTable);
            UpdateHelper.CheckUpdate(true, pgbLoading, this);
            if (databaseName != null)
            {
                LoadData(true, false, true);
                SetMenuEnabled(true);
            }
            else if (path != null)
            {
                importToolStripMenuItem_Click(ImportState.None, new string[] { path });
            }
        }

        private void SetPage()
        {
            lblPage.Text = $"von {MaxPages}";
            NumPage.Maximum = MaxPages;
        }

        private void ResetValidRowLabel()
        {
            statusStrip1.Invoke(new MethodInvoker(() =>
            {
                ValidRowsLabel.Visible = ValidRowsText.Visible = false;
            }));
        }

        private void SetMenuEnabled(bool status)
        {
            foreach (ToolStripMenuItem item in new ToolStripMenuItem[] { speichernAlsToolStripMenuItem, benutzerdefiniertesSpeichernToolStripMenuItem1, arbeitsablaufToolStripMenuItem, funktionenToolStripMenuItem, ersetzenToolStripMenuItem, duplikateToolStripMenuItem })
            {
                SetSubItemEnabled(status, item);
            }

            foreach (ToolStripMenuItem item in new ToolStripMenuItem[] { speichernToolStripMenuItem2, sortierenToolStripMenuItem })
            {
                item.Enabled = status;
            }

            foreach (Control item in new Control[] { NumPage })
            {
                item.Enabled = status;
            }
        }

        private void SetSubItemEnabled(bool status, ToolStripMenuItem menuItem)
        {
            foreach (ToolStripItem item in menuItem.DropDownItems)
            {
                item.Enabled = status;
            }
        }

        private void SetSize()
        {
            if (Properties.Settings.Default.Form1Maximized)
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                Size = Properties.Settings.Default.Form1Size;
            }
        }

        private void SetSorting(string order)
        {
            if (order != SortingOrder)
            {
                SortingOrder = order;
                DictSorting = ViewHelper.GenerateSortingList(GetSorting());
                AliasColumnMapping = DatabaseHelper.GetAliasColumnMapping(TableName);
            }
        }



        private void RestoreDataGridSortMode()
        {

            for (int i = 0; i < dgTable.Columns.Count; i++)
            {
                dgTable.Columns[i].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
        }

        private string GetSorting()
        {
            return SortingOrder;
        }

        private void EndEdit()
        {
            ViewHelper.EndDataGridViewEdit(dgTable);
        }

        private void ResetPage()
        {
            NumPage.ValueChanged -= NumPage_ValueChanged;
            NumPage.Value = 1;
            NumPage.ValueChanged += NumPage_ValueChanged;
            SetPage();
            dgTable.AllowUserToAddRows = Page == MaxPages;
        }

        private void SetWidth()
        {
            if (Properties.Settings.Default.FullWidthImport)
            {
                foreach (DataGridViewColumn col in dgTable.Columns.Count > 100 && Properties.Settings.Default.NotAdjustColumnOver100 ? dgTable.Columns.Cast<DataGridViewColumn>().Take(100) : dgTable.Columns.Cast<DataGridViewColumn>())
                {
                    if (ColumnWidths.TryGetValue(col.Name, out int value))
                    {
                        col.Width = value;
                    }
                    else
                    {
                        SetOptimalColumnWidth(col);
                    }
                }
            }
        }

        private void SetOptimalColumnWidth(DataGridViewColumn col)
        {
            string result = (dgTable.DataSource as DataTable)?.AsEnumerable().Select(row => row[col.Name].ToString()).Concat(new string[] { col.Name }).Aggregate(string.Empty, (seed, f) => f.Length > seed.Length ? f : seed);
            col.Width = TextRenderer.MeasureText(result, dgTable.DefaultCellStyle.Font).Width + ColumnWidthTolerance;

            AddColumnWidth(col);
        }

        private void SaveWidthOfDataGridViewColumns()
        {
            foreach (DataGridViewColumn col in dgTable.Columns)
            {
                AddColumnWidth(col);
            }
        }

        private void AddColumnWidth(DataGridViewColumn col)
        {
            if (!ColumnWidths.ContainsKey(col.Name))
            {
                ColumnWidths.Add(col.Name, col.Width);
            }
            else
            {
                ColumnWidths[col.Name] = col.Width;
            }
        }

        private void LoadProcedures(List<Proc> proc = null)
        {
            ersetzenToolStripMenuItem.DropDownItems.Clear();

            procedures = proc ?? ImportHelper.LoadProcedures(this);

            ToolStripMenuItem tempProcedure = new ToolStripMenuItem("Temporäre Eingabe");
            tempProcedure.Click += TempProcedure_Click;
            ersetzenToolStripMenuItem.DropDownItems.Add(tempProcedure);
            List<Proc> visibleProc = procedures.Where(vproc => !vproc.HideInMainForm).ToList();
            for (int i = 0; i < visibleProc.Count; i++)
            {
                int index = i;
                string name = visibleProc[i].Name.Replace("&", "&&");
                ToolStripMenuItem item = new ToolStripMenuItem(name);
                item.Click += (sender, e) => procedure_Click(visibleProc[index]);
                ersetzenToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void LoadWorkflows(List<Work> work = null)
        {
            arbeitsablaufToolStripMenuItem.DropDownItems.Clear();

            workflows = work ?? ImportHelper.LoadWorkflows(this);

            List<Work> workflowsCopy = GetCopyOfWorkflows(workflows);

            for (int i = 0; i < workflowsCopy.Count; i++)
            {
                int index = i;
                string name = workflowsCopy[i].Name.Replace("&", "&&");
                ToolStripMenuItem item = new ToolStripMenuItem(name);
                item.Click += (sender, e) => workflow_Click(workflowsCopy[index]);
                arbeitsablaufToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private List<Work> GetCopyOfWorkflows(List<Work> work)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, work);
            stream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(stream) as List<Work>;
        }

        private void LoadTolerances(List<Tolerance> tol = null)
        {
            tolerances = tol ?? ImportHelper.LoadTolerances();
        }

        private void LoadCases(List<Case> cas = null)
        {
            duplikateToolStripMenuItem.DropDownItems.Clear();

            cases = cas ?? ImportHelper.LoadCases();
            for (int i = 0; i < cases.Count; i++)
            {
                int index = i;
                ToolStripMenuItem item = new ToolStripMenuItem(cases[i].Name);
                item.Click += (sender, e) => case_Click(cases[index]);
                duplikateToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void case_Click(Case cas)
        {
            ProcDuplicate procDuplicate = new ProcDuplicate(0, cas.Id, cas.Name)
            {
                DuplicateColumns = cas.Columns.AsEnumerable().Select(row => row[0].ToString()).ToArray()
            };
            StartSingleWorkflow(procDuplicate);
        }

        private void workflow_Click(Work workflow, Action finished = null)
        {
            List<NotFoundHeaders> notFound = new List<NotFoundHeaders>();
            List<string> headers = DatabaseHelper.GetSortedColumnsAsAlias(TableName);

            foreach (WorkProc wp in workflow.Procedures)
            {
                List<string> notFoundColumns = new List<string>();
                string[] wpHeaders = wp.GetHeaders();

                if (!string.IsNullOrWhiteSpace(wp.NewColumn))
                {
                    headers.Add(wp.NewColumn);
                }
                else if (wp.CopyOldColumn)
                {
                    headers.AddRange(wpHeaders.Select(header => header + Properties.Settings.Default.OldAffix));
                }

                notFoundColumns.AddRange(wpHeaders.Where(header => !headers.Contains(header, System.StringComparer.OrdinalIgnoreCase)));

                if (notFoundColumns.Count > 0)
                {
                    notFound.Add(new NotFoundHeaders(notFoundColumns, wp));
                }
            }
            if (notFound.Count == 0)
            {
                ReplaceThroughTemp(workflow.Procedures, finished);
            }
            else
            {
                HashSet<string> columns = new HashSet<string>();
                foreach (NotFoundHeaders nf in notFound)
                {
                    foreach (string col in nf.Headers)
                    {
                        columns.Add(col);
                    }
                }
                using (SelectDuplicateColumns form = new SelectDuplicateColumns(columns.ToArray(), DatabaseHelper.GetSortedColumnsAsAlias(TableName).ToArray(), false))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        string[] from = new string[form.Table.Rows.Count];
                        string[] to = new string[form.Table.Rows.Count];
                        for (int i = 0; i < form.Table.Rows.Count; ++i)
                        {
                            from[i] = form.Table.Rows[i][0].ToString();
                            to[i] = form.Table.Rows[i][1].ToString();
                        }

                        foreach (NotFoundHeaders nf in notFound)
                        {
                            string[] wpHeaders = nf.Wp.GetHeaders();
                            for (int y = 0; y < wpHeaders.Length; y++)
                            {
                                for (int i = 0; i < from.Length; i++)
                                {
                                    if (to[i] == SelectDuplicateColumns.IgnoreColumn)
                                    {
                                        nf.Wp.RemoveHeader(from[i]);
                                    }
                                    else if (wpHeaders[y] == from[i] && nf.Headers.Contains(from[i])) //Kann sein, dass eine Spalte hinzugefügt wird und sie bei manchen Valid und bei manchen inValid ist, je nachdem wann sie ausgeführt werden
                                    {
                                        nf.Wp.RenameHeaders(from[i], to[i]);
                                    }
                                }
                            }
                        }
                        ReplaceThroughTemp(workflow.Procedures, finished);
                    }
                }
            }
        }

        private void ReplaceThroughTemp(List<WorkProc> temp, Action finished)
        {
            StartLoadingBar();
            Thread thread = new Thread(() =>
            {
                try
                {
                    foreach (WorkProc t in temp)
                    {
                        ReplaceProcedure(null, t);
                    }
                    DatabaseHelper.SetSavepoint();
                    LoadData(true);
                }
                catch (Exception ex)
                {
                    ErrorHelper.LogMessage(ex, this);
                }
                StopLoadingBar();
                this.MessagesOK(MessageBoxIcon.Information, "Arbeitsablauf ausgeführt!");
                finished?.Invoke();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private int GetProcedureThroughId(int id)
        {
            return procedures.FindIndex(p => p.Id == id);
        }

        private void importToolStripMenuItem_Click(ImportState state = ImportState.None, string[] openFiles = null)
        {
            OpenFileDialog dialog = ImportHelper.GetOpenFileDialog(state != ImportState.Header);
            string mergePath = string.Empty;
            bool validMerge = state == ImportState.Merge && ProcAddTableColumns.CheckFile(FilePath, ref mergePath);
            if (openFiles != null || validMerge || dialog.ShowDialog(this) == DialogResult.OK)
            {
                string[] filenames = openFiles ?? (validMerge ? new string[1] { mergePath } : dialog.FileNames);
                ProgressBar loadingBar = pgbLoading;
                Thread thread = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    bool fileNameSet = state != ImportState.None;
                    bool multipleFiles = filenames.Length > 1;
                    Dictionary<string, ImportSettings> fileImportSettings = new Dictionary<string, ImportSettings>();
                    string newTable = null;
                    string fileNameBefore = Path.GetFileName(filenames[0]);
                    int fileEncoding = 0;
                    foreach (string file in filenames)
                    {
                        try
                        {
                            string filename = Path.GetFileName(file);
                            string tableName = ImportHelper.ImportFile(file, multipleFiles, fileImportSettings, contextGlobal, loadingBar, this, ref fileEncoding);

                            if (tableName != null)
                            {

                                if (newTable != null)
                                {
                                    DatabaseHelper.ConcatTable(newTable, tableName, fileNameBefore, filename);
                                }
                                else
                                {
                                    newTable = tableName;
                                }
                                fileNameBefore = filename;
                                if (!fileNameSet)
                                {
                                    fileNameSet = true;
                                    SetFileMeta(file, fileEncoding);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex, this);
                        }
                    }

                    FinishImport(newTable, state, Path.GetFileName(filenames[0]), fileEncoding);
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            dialog.Dispose();
        }

        private void SetFileMeta(string file, int encoding)
        {
            SetFileName(file);
            FileEncoding = encoding;
        }

        private void SetFileName(string path)
        {
            (lblFilename?.GetCurrentParent() ?? statusStrip1)?.BeginInvoke(new MethodInvoker(() =>
            {
                lblFilename.Text = Path.GetFileName(path);
            }));

            FilePath = path;
        }

        private void FinishImport(string tableName, ImportState state, string filename, int encoding)
        {
            if (tableName != null)
            {
                switch (state)
                {
                    case ImportState.Merge:
                        Thread thread = new Thread(() =>
                        {
                            try
                            {
                                StartLoadingBar();
                                int count = DataHelper.StartMerge(tableName, encoding, FilePath, Properties.Settings.Default.PVMIdentifier, Properties.Settings.Default.PVMIdentifier, Properties.Settings.Default.InvalidColumnName, GetSorting(), OrderType, this);
                                StopLoadingBar();
                                DatabaseHelper.SetSavepoint();
                                LoadData(true);

                                if (count != 0)
                                {
                                    Invoke(new MethodInvoker(() =>
                                    {
                                        ValidRows = count;
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                ErrorHelper.LogMessage(ex, this);
                            }
                        });
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                        break;

                    case ImportState.Append:
                        DatabaseHelper.ConcatTable(tableName, Path.GetFileName(FilePath), filename, TableName);
                        break;

                    case ImportState.Header:
                        DatabaseHelper.RenameColumns(tableName, TableName);
                        List<string> newHeaders = DatabaseHelper.GetSortedColumnsAsAlias(tableName);
                        dgTable.BeginInvoke(new MethodInvoker(() =>
                        {
                            for (int i = 0; i < newHeaders.Count && i < dgTable.ColumnCount; ++i)
                            {
                                dgTable.Columns[i].Name = newHeaders[i];
                            }
                        }));
                        break;

                    default:
                        ResetValidRowLabel();
                        DatabaseHelper.ReplaceTable(tableName, TableName);
                        BeginInvoke(new MethodInvoker(() =>
                        {
                            SetMenuEnabled(true);
                        }));
                        break;
                }
                DatabaseHelper.SetSavepoint();
                LoadData(true);

            }
            StopLoadingBar();
        }

        private void LoadData(bool readjustColumnWidth = true, bool preventLoading = false, bool withoutInvoke = false)
        {
            if (withoutInvoke)
            {
                load();
            }
            else
            {
                dgTable.Invoke(new MethodInvoker(() =>
                {
                    load();
                }));
            }

            void load()
            {
                if (readjustColumnWidth)
                {
                    ColumnWidths.Clear();
                }
                else
                {
                    SaveWidthOfDataGridViewColumns();
                }
                MaxPages = Math.Ceiling(DatabaseHelper.GetRowCount(TableName) / Properties.Settings.Default.MaxRows);
                SetPage();

                int scrollBarHorizontal = dgTable.HorizontalScrollingOffset;
                if (!preventLoading)
                {
                    DataTable table = DatabaseHelper.GetData(SortingOrder, OrderType, (int)((Page - 1) * Properties.Settings.Default.MaxRows), TableName);

                    dgTable.RowsAdded -= dgTable_RowsAdded;
                    dgTable.DataSource = null; //else readded columns are at the wrong index
                    dgTable.DataSource = table;
                    dgTable.Columns[0].Visible = false;
                    dgTable.RowsAdded += dgTable_RowsAdded;

                    SetRowCount(DatabaseHelper.GetRowCount(TableName));
                }
                RestoreDataGridSortMode();
                SetWidth();

                dgTable.HorizontalScrollingOffset = scrollBarHorizontal;
            }
        }



        private void SetRowCount(int count)
        {
            RowCount = count;
            lblRows.Text = count.ToString();
        }

        private void trimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgTable.DataSource != null)
            {
                TrimForm form = new TrimForm(DatabaseHelper.GetSortedColumnsAsAlias(TableName));
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    StartLoadingBar();
                    new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.IsBackground = true;
                            string order = GetSorting();
                            form.Proc.DoWork(ref order, null, null, null, FilePath, contextGlobal, OrderType, this);
                            LoadData(true);

                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex, this);
                        }
                        StopLoadingBar();
                    }).Start();
                }
                form.Dispose();
            }
        }

        private void TempProcedure_Click(object sender, EventArgs e)
        {
            ProcedureForm form = new ProcedureForm(contextGlobal);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                Proc proc = new Proc(form.Table, form.CheckTotal, form.CheckWord, form.LeaveEmpty);
                procedure_Click(proc);
            }
            form.Dispose();
        }

        private void procedure_Click(Proc procedure)
        {
            using (Formula formula = new Formula(FormulaState.Procedure, DatabaseHelper.GetSortedColumnsAsAlias(TableName)))
            {
                if (formula.ShowDialog(this) == DialogResult.OK)
                {
                    string[] columns = formula.SelectedHeaders();

                    if (columns.Length > 0)
                    {
                        ProcUser user = new ProcUser(columns, formula.HeaderName(), formula.OldColumn);
                        StartLoadingBar();
                        Thread thread = new Thread(() =>
                        {
                            try
                            {
                                ReplaceProcedure(procedure, user);
                                DatabaseHelper.SetSavepoint();
                                LoadData(true);
                            }
                            catch (Exception ex)
                            {
                                ErrorHelper.LogMessage(ex, this);
                            }
                            StopLoadingBar();
                        });
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                    }
                }
            }
        }

        private void ReplaceProcedure(Proc procedure, WorkProc wp)
        {
            string newOrder = GetSorting();

            wp.DoWork(ref newOrder, GetCaseThroughId(wp.ProcedureId), tolerances, procedure ?? GetProcedure(wp.ProcedureId), FilePath, contextGlobal, OrderType, this);

            SetSorting(newOrder);
        }

        private Proc GetProcedure(int id)
        {
            Proc proc = null;
            int index = GetProcedureThroughId(id);
            if (index != -1)
            {
                proc = procedures[index];
            }
            return proc;
        }

        private Case GetCaseThroughId(int id)
        {
            int index = cases.FindIndex(cs => cs.Id == id);
            return index != -1 ? cases[index] : null;
        }

        private void excelToolStripMenuItem_Click(object sender, EventArgs e, string path = null)
        {
            if (dgTable.DataSource != null)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog
                {
                    Filter = $"Excel Dateien|{ImportHelper.ExcelExt}|Alle Dateien (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                if (path != null || saveFileDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    path = path ?? saveFileDialog1.FileName;
                    StartLoadingBar();
                    new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.IsBackground = true;
                            Save(path, SaveFormat.EXCEL);
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex, this);
                        }
                        StopLoadingBar();
                    }).Start();
                }
            }
        }

        private void Save(string path, SaveFormat format)
        {
            StartLoadingBarCount(RowCount);
            bool saved = ExportHelper.Save(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), Path.GetExtension(FilePath), FileEncoding, (int)format, GetSorting(), OrderType, this, UpdateLoadingBar) != 0;
            StopLoadingBar();
            if (saved)
            {
                SaveFinished();
            }
        }

        private void spalteHinzufügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newColumn = Microsoft.VisualBasic.Interaction.InputBox("Bitte Spaltennamen eingeben", "Spalte hinzufügen", string.Empty);
            if (!string.IsNullOrWhiteSpace(newColumn))
            {
                if (dgTable.Columns.Cast<DataGridViewColumn>().Any(col => col.HeaderText.Equals(newColumn, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageHandler.MessagesOK(this, MessageBoxIcon.Warning, "Der Name der Spalte ist bereits vergeben");
                    spalteHinzufügenToolStripMenuItem_Click(sender, e);
                }
                else
                {
                    dgTable.Columns.Add(newColumn, newColumn);
                    DatabaseHelper.AddColumnFixedAlias(newColumn, TableName);
                    DatabaseHelper.SetSavepoint();
                    LoadData();
                }
            }
        }

        private void dgTable_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                selectedRow = dgTable.HitTest(e.X, e.Y).RowIndex;
                selectedColumn = dgTable.HitTest(e.X, e.Y).ColumnIndex;
                if (selectedColumn > -1 && selectedRow > -1 && !dgTable[selectedColumn, selectedRow].Selected)
                {
                    foreach (DataGridViewCell cell in dgTable.SelectedCells)
                    {
                        cell.Selected = false;
                    }
                    dgTable[selectedColumn, selectedRow].Selected = true;
                }
                //Header
                if (selectedColumn >= 0 && selectedRow == -1)
                {
                    ctxHeader.Show(dgTable, new Point(e.X, e.Y));
                }
                else if (selectedColumn >= 0)
                {
                    ctxBody.Show(dgTable, new Point(e.X, e.Y));
                }
            }
        }

        private void spalteUmbenennenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newText = Microsoft.VisualBasic.Interaction.InputBox("Bitte Spaltennamen eingeben", "Spalte umbenennen", dgTable.Columns[selectedColumn].HeaderText);
            if (!string.IsNullOrWhiteSpace(newText))
            {
                string oldText = dgTable.Columns[selectedColumn].HeaderText;

                if (oldText != newText)
                {
                    DatabaseHelper.RenameAlias(oldText, newText, TableName);
                    dgTable.Columns[selectedColumn].HeaderText = newText;
                    DatabaseHelper.SetSavepoint();
                }
            }
        }

        private void spalteLöschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DatabaseHelper.DeleteColumnThroughAlias(dgTable.Columns[selectedColumn].HeaderText, TableName);
            dgTable.Columns.RemoveAt(selectedColumn);
            DatabaseHelper.SetSavepoint();
        }


        private void zeilenZusammenfügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Merge formula = new Merge(DatabaseHelper.GetSortedColumnsAsAlias(TableName).ToArray(), contextGlobal))
            {
                if (formula.ShowDialog(this) == DialogResult.OK)
                {
                    StartSingleWorkflow(formula.Proc);
                }
            }
        }

        private void dBASEToolStripMenuItem_Click(object sender, EventArgs e, string path = null)
        {
            if (dgTable.DataSource != null)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog
                {
                    Filter = $"DBASE Dateien ({ImportHelper.DbfExt})|{ImportHelper.DbfExt}|Alle Dateien (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                if (path != null || saveFileDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    path = path ?? saveFileDialog1.FileName;
                    new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.IsBackground = true;
                            Save(path, SaveFormat.DBASE);
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex, this);
                        }
                    }).Start();
                }
            }
        }

        private void dgTable_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            ViewHelper.AddNumerationToDataGridView(sender, e, Font, NumPage.Value);
        }

        private void rückgängigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartLoadingBar();

            new Thread(() =>
            {
                try
                {
                    if (DatabaseHelper.Undo())
                    {
                        LoadData(true);
                    }
                    StopLoadingBar();
                }
                catch (Exception ex)
                {
                    ErrorHelper.LogMessage(ex, this);
                }
            }).Start();


        }

        private void wiederholenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartLoadingBar();

            new Thread(() =>
            {
                try
                {
                    if (DatabaseHelper.Redo())
                    {
                        LoadData(true);
                    }
                    StopLoadingBar();
                }
                catch (Exception ex)
                {
                    ErrorHelper.LogMessage(ex, this);
                }
            }).Start();
        }

        private void dgTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1 && e.ColumnIndex != -1)
            {
                string value = dgTable[e.ColumnIndex, e.RowIndex].Value.ToString();
                int id = int.Parse(dgTable[DatabaseHelper.IdColumnName, e.RowIndex].Value.ToString());
                int width = TextRenderer.MeasureText(value, dgTable.DefaultCellStyle.Font).Width + ColumnWidthTolerance;
                DataGridViewColumn col = dgTable.Columns[e.ColumnIndex];
                if (width > col.Width)
                {
                    ColumnWidths[col.Name] = col.Width = width;
                }
                DatabaseHelper.UpdateCell(value, dgTable.Columns[e.ColumnIndex].Name, id, TableName);
                DatabaseHelper.SetSavepoint();
            }
        }

        private void dgTable_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            if (dgTable.AllowUserToAddRows && e.RowIndex >= RowCount)
            {
                string newId = DatabaseHelper.InsertRow(TableName);
                DatabaseHelper.SetSavepoint();
                dgTable[DatabaseHelper.IdColumnName, e.RowIndex].Value = newId; //CellValueChanged triggered?

            }

            SetRowCount(RowCount + 1);
        }

        private void tabellenZusammenfügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(ImportState.Merge);
        }

        private void verwaltungToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Administration form = new Administration(DatabaseHelper, ExportHelper, ImportHelper, DatabaseHelper.GetSortedColumnsAsAlias(TableName).ToArray(), contextGlobal, procedures, workflows, cases, tolerances);
            form.FormClosed += new FormClosedEventHandler(administrationFormClosed);
            form.Show(this);
        }

        private void administrationFormClosed(object sender, FormClosedEventArgs e)
        {
            Administration admin = sender as Administration;
            LoadProcedures(admin.Procedures);
            LoadWorkflows(admin.Workflows);
            LoadTolerances(admin.Tolerances);
            LoadCases(admin.Cases);
            SetMenuEnabled(dgTable.DataSource != null);
        }

        private void cSVToolStripMenuItem_Click(string path = null)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = $"CSV Dateien ({ImportHelper.CsvExt})|{ImportHelper.CsvExt}|Alle Dateien (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (path != null || saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                path = path ?? saveFileDialog1.FileName;
                new Thread(() =>
                {
                    try
                    {
                        Thread.CurrentThread.IsBackground = true;
                        Save(path, SaveFormat.CSV);
                    }
                    catch (Exception ex)
                    {
                        ErrorHelper.LogMessage(ex, this);
                    }
                }).Start();
            }
        }

        private void UpdateLoadingBar()
        {
            try
            {
                if (pgbLoading.Value < pgbLoading.Maximum)
                {
                    pgbLoading.Invoke(new MethodInvoker(() =>
                    {
                        pgbLoading.Value++;
                    }));
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage($"{ex.ToString() + Environment.NewLine} Maximum:{pgbLoading.Maximum}; Minimum:{pgbLoading.Minimum} Value:{pgbLoading.Value}", this, false);
            }
        }

        private void StartLoadingBarCount(int length)
        {
            pgbLoading.Invoke(new MethodInvoker(() =>
            {
                pgbLoading.Style = ProgressBarStyle.Continuous;
                pgbLoading.Maximum = length;
                pgbLoading.Minimum = 0;
                pgbLoading.Value = 0;
            }));
        }

        private void SaveFinished()
        {
            this.MessagesOK(MessageBoxIcon.Information, "Gespeichert");
        }

        private void postwurfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Formula formula = new Formula(FormulaState.Export, DatabaseHelper.GetSortedColumnsAsAlias(TableName)))
            {
                if (formula.ShowDialog(this) == DialogResult.OK)
                {
                    StartLoadingBarCount((Properties.Settings.Default.PVMSaveTwice ? 2 : 1) * RowCount);
                    new ProcPVMExport(formula.SelectedHeaders(), UpdateLoadingBar).DoWork(ref SortingOrder, null, null, null, FilePath, null, OrderType, this);
                    StopLoadingBar();
                    SaveFinished();
                }
            }
        }

        private void nachWertInSpalteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ExportCustom export = new ExportCustom(DatabaseHelper.GetAliasColumnMapping(TableName), DatabaseHelper, TableName))
            {
                if (export.ShowDialog(this) == DialogResult.OK)
                {
                    StartLoadingBar();
                    ExportHelper.ExportTableWithColumnCondition(export.Items, FilePath, StopLoadingBar, SaveFinished, FileEncoding, GetSorting(), OrderType, this, export.ContinuedNumberName);

                }
            }
        }

        private void StartLoadingBar()
        {
            pgbLoading.BeginInvoke(new MethodInvoker(() =>
            {
                pgbLoading.Style = ProgressBarStyle.Marquee;
            }));
        }

        private void StopLoadingBar()
        {
            pgbLoading.BeginInvoke(new MethodInvoker(() =>
            {
                pgbLoading.Style = ProgressBarStyle.Blocks;
                pgbLoading.Maximum = pgbLoading.Value = 0;
            }));
        }

        private void zählenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ExportCount export = new ExportCount(DatabaseHelper.GetAliasColumnMapping(TableName), DatabaseHelper))
            {
                if (export.ShowDialog(this) == DialogResult.OK)
                {
                    StartLoadingBar();
                    Thread thread = new Thread(() =>
                    {
                        string newTable = ExportHelper.ExportCount(export.getSelectedValue(), export.CountChecked ? export.Count : 0, export.ShowFromTo, OrderType, TableName);
                        BeginInvoke(new MethodInvoker(() =>
                        {
                            DatabaseHelper.CopyToNewDatabaseFile(newTable);
                            new Form1(newTable).Show(this);
                        }));
                        StopLoadingBar();
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
            }
        }


        private void dgTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (Properties.Settings.Default.EditShortcut == (ModifierKeys | Keys.LButton))
            {
                dgTable.BeginEdit(true);
            }
            if (e.RowIndex == -1 && e.ColumnIndex != -1)
            {
                DataGridViewColumn col = dgTable.Columns[e.ColumnIndex];
                if (dgTable.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection == SortOrder.Descending)
                {
                    ResetSort();
                }
                else
                {
                    bool asc = col.HeaderCell.SortGlyphDirection == SortOrder.Ascending;

                    SetSorting($"[{DatabaseHelper.GetColumnName(col.Name, TableName)}] COLLATE NATURALSORT {(asc ? "DESC" : "ASC")}");
                }
                LoadData(true);
            }
        }

        private void dgTable_KeyDown(object sender, KeyEventArgs e)
        {

            if (Properties.Settings.Default.EditShortcut == e.KeyData)
            {
                dgTable.BeginEdit(true);
            }
        }

        private void ResetSort()
        {
            SetSorting(string.Empty);
        }

        private void zeileLöschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int[] rows = ViewHelper.SelectedRowsOfDataGridView(dgTable);
            DeleteRows(rows);
            SetRowCount(RowCount - rows.Length);
            DatabaseHelper.SetSavepoint();
        }

        private void DeleteRows(int[] rows)
        {
            foreach (int row in rows.OrderByDescending(index => index))
            {
                DatabaseHelper.DeleteRow(int.Parse(dgTable[DatabaseHelper.IdColumnName, row].Value.ToString()), TableName);
                dgTable.Rows.RemoveAt(row);
            }
        }

        private void zeileEinfügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GetSorting()))
            {
                MessageHandler.MessagesOK(this, MessageBoxIcon.Warning, "Es kann keine Zeile hinzugefügt werden während eine Sortierung aktiv ist");
            }
            else
            {
                DatabaseHelper.InsertRowAt(dgTable[0, selectedRow].Value.ToString(), TableName);
                DatabaseHelper.SetSavepoint();
                LoadData(false);
            }

        }

        private void tabelleHinzufügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(ImportState.Append);
        }

        private void überschriftenEinlesenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(ImportState.Header);
        }

        private void dgTable_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex > -1 && AliasColumnMapping != null && AliasColumnMapping.TryGetValue(e.Value.ToString(), out string header))
            {
                int count = DictSorting.Keys.ToList().IndexOf(header);
                SortOrder order = SortOrder.None;
                if (count >= 0)
                {
                    count++;
                    e.Graphics.FillRectangle(new SolidBrush(e.CellStyle.BackColor), e.CellBounds);
                    e.Paint(e.ClipBounds, (DataGridViewPaintParts.All & ~DataGridViewPaintParts.Background));

                    e.Graphics.DrawString($"{count}", dgTable.DefaultCellStyle.Font, new SolidBrush(Color.Black), e.CellBounds.X + e.CellBounds.Width - 20, e.CellBounds.Y + 5, StringFormat.GenericDefault);
                    e.Handled = true;
                    order = DictSorting[header];
                }
                dgTable.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = order;

            }

        }

        private void großKleinschreibungToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (UpLowCaseForm form = new UpLowCaseForm(DatabaseHelper.GetSortedColumnsAsAlias(TableName).ToArray()))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    StartSingleWorkflow(form.Procedure);
                }
            }
        }

        private void rundenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (RoundForm form = new RoundForm(DatabaseHelper.GetSortedColumnsAsAlias(TableName).ToArray()))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    StartSingleWorkflow(form.Procedure);
                }
            }
        }

        private void speichernToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            string FileName = FilePath;
            string extension = Path.GetExtension(FileName).ToLower();

            if (extension == ".dbf")
            {
                dBASEToolStripMenuItem_Click(null, null, FileName);
            }
            else if (extension != string.Empty && ImportHelper.ExcelExt.Contains(extension))
            {
                excelToolStripMenuItem_Click(null, null, FileName);
            }
            else
            {
                cSVToolStripMenuItem_Click(FileName);
            }
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateHelper.CheckUpdate(false, pgbLoading, this);
        }

        private void zeilenZusammenfügenToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            using (MergeColumns form = new MergeColumns(DatabaseHelper.GetAliasColumnMapping(TableName)))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    string columnName = form.Identifier;
                    bool separator = form.Separator;
                    List<PlusListboxItem> additionalColumns = form.AdditionalColumns;

                    new Thread(() =>
                    {
                        try
                        {
                            StartLoadingBarCount(RowCount);
                            DatabaseHelper.MergeRows(columnName, additionalColumns, separator, this, UpdateLoadingBar, TableName);
                            DatabaseHelper.SetSavepoint();
                            LoadData(true);
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex, this);
                        }
                        StopLoadingBar();

                    }).Start();
                }
            }
        }

        private void zeichenAuffüllenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (PaddingForm form = new PaddingForm(DatabaseHelper.GetSortedColumnsAsAlias(TableName).ToArray()))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    StartSingleWorkflow(form.Proc);
                }
            }
        }

        private void textErsetzenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (InsertText form = new InsertText("Spalte mit Text befüllen", "Bitte Text eingeben"))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    string newText = form.NewText;

                    DatabaseHelper.SetColumnValues(dgTable.Columns[selectedColumn].HeaderText, newText, TableName);
                    foreach (DataGridViewRow row in dgTable.Rows)
                    {
                        row.Cells[selectedColumn].Value = newText;
                    }
                    DatabaseHelper.SetSavepoint();
                }
            }
        }

        private void nummerierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (NumerationForm form = new NumerationForm(DatabaseHelper.GetSortedColumnsAsAlias(TableName)))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    StartSingleWorkflow(form.Procedure);
                }
            }
        }

        private void StartSingleWorkflow(WorkProc proc, Action finished = null)
        {
            workflow_Click(new Work(string.Empty, new List<WorkProc> { GetCopyOfWorkProc(proc) }, 0), finished);
        }

        private WorkProc GetCopyOfWorkProc(WorkProc proc)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, proc);
            stream.Seek(0, SeekOrigin.Begin);
            return formatter.Deserialize(stream) as WorkProc;
        }

        private void substringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SubstringForm form = new SubstringForm(DatabaseHelper.GetSortedColumnsAsAlias(TableName).ToArray()))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    StartSingleWorkflow(form.Procedure);
                }
            }
        }

        private void textErsetzenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (ReplaceWholeForm form = new ReplaceWholeForm(DatabaseHelper.GetSortedColumnsAsAlias(TableName).ToArray(), contextGlobal))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    ProcReplaceWhole proc = new ProcReplaceWhole(form.Table);
                    StartSingleWorkflow(proc);
                }
            }
        }

        private void einstellungenToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            SettingForm form = new SettingForm();
            int oldHeight = Properties.Settings.Default.RowHeight;
            form.FormClosed += (s2, e2) => SettingForm_FormClosed(s2, e2, oldHeight);
            form.Show(this);
        }

        private void SettingForm_FormClosed(object sender, FormClosedEventArgs e, int oldHeight)
        {
            dgTable.DefaultCellStyle.Font = new Font(dgTable.DefaultCellStyle.Font.Name, Properties.Settings.Default.TableFontSize);
            dgTable.RowTemplate.Height = Properties.Settings.Default.RowHeight;

            if (oldHeight != Properties.Settings.Default.RowHeight)
            {
                ColumnWidths.Clear();
                LoadData(true, false);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSize();
            DatabaseHelper.Close();
        }

        private void SaveSize()
        {
            if (WindowState != FormWindowState.Maximized)
            {
                Properties.Settings.Default.Form1Size = Size;
            }
            Properties.Settings.Default.Form1Maximized = WindowState == FormWindowState.Maximized;
            Properties.Settings.Default.Save();
        }

        private void spaltenVergleichenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (CompareForm form = new CompareForm(DatabaseHelper.GetSortedColumnsAsAlias(TableName).ToArray(), DatabaseHelper))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    StartSingleWorkflow(form.Procedure);
                }
            }
        }

        private void PrüfzifferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SelectHeader headerForm = new SelectHeader(DatabaseHelper.GetAliasColumnMapping(TableName)))
            {
                if (headerForm.ShowDialog(this) == DialogResult.OK)
                {
                    string column = headerForm.Column;
                    StartLoadingBarCount(RowCount);
                    new Thread(() =>
                    {
                        DatabaseHelper.SetCheckSum(column, UpdateLoadingBar, this, TableName);
                        StopLoadingBar();
                        DatabaseHelper.SetSavepoint();
                        LoadData(true);
                    }).Start();
                }
            }
        }

        private void längsteZeileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (MaxRowLengthForm form = new MaxRowLengthForm(DatabaseHelper.GetAliasColumnMapping(TableName)))
            {

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    string shortcut = form.Shortcut;
                    string newColumn = form.NewColumn;
                    int minLength = form.MinLength;
                    string columnName = form.Column;
                    StartLoadingBar();
                    Thread thread = new Thread(() =>
                    {
                        int id = DatabaseHelper.GetRowWithMaxCharacters(GetSorting(), OrderType, out int index, TableName);

                        if (shortcut != string.Empty && newColumn != string.Empty)
                        {
                            newColumn = DatabaseHelper.AddColumnWithAdditionalIfExists(newColumn, TableName);

                            if (minLength != -1)
                            {
                                DatabaseHelper.UpdateRowsWithMinCharacters(newColumn, minLength, shortcut, newColumn, TableName);
                            }
                            else
                            {
                                DatabaseHelper.UpdateCell(shortcut, newColumn, id, TableName, true);
                            }
                            DatabaseHelper.SetSavepoint();
                            LoadData(true);
                        }
                        if (minLength != -1)
                        {
                            SelectDataGridViewRow(index);
                        }
                        StopLoadingBar();
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
            }
        }

        private void SelectDataGridViewRow(int index)
        {
            int maxRecords = (int)Properties.Settings.Default.MaxRows;
            int desiredPage = (index + 1) / maxRecords;
            if (desiredPage != Page)
            {
                Page = desiredPage;
            }
            int newIndex = index;
            while (newIndex > maxRecords)
            {
                newIndex -= maxRecords;
            }

            dgTable.Invoke(new MethodInvoker(() =>
            {
                dgTable.ClearSelection();
                dgTable.Rows[newIndex].Selected = true;
                dgTable.FirstDisplayedScrollingRowIndex = newIndex;
            }));
        }

        private void suchenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SearchForm form = new SearchForm(DatabaseHelper.GetSortedColumnsAsAlias(TableName).ToArray()))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    ProcSearch proc;
                    if (form.FromToSelected)
                    {
                        proc = new ProcSearch(form.SearchText, form.Header, form.From, form.To, form.NewColumn, form.CheckTotal);
                    }
                    else
                    {
                        proc = new ProcSearch(form.SearchText, form.Header, form.NewColumn, form.CheckTotal, form.Shortcut);
                    }
                    StartSingleWorkflow(proc, delegate { SearchAndSelect(proc.SearchText, proc.Header, form.CheckTotal); });
                }
            }
        }

        private void SearchAndSelect(string searchText, string alias, bool totalSearch)
        {
            int index = DatabaseHelper.SearchValue(searchText, alias, totalSearch, GetSorting(), OrderType, TableName);
            SelectDataGridViewRow(index);
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            importToolStripMenuItem_Click(ImportState.None, (string[])e.Data.GetData(DataFormats.FileDrop));
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void aufteilenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SplitFormMain form = new SplitFormMain(DatabaseHelper.GetSortedColumnsAsAlias(TableName).ToArray()))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    StartSingleWorkflow(form.Procedure);
                }
            }
        }

        private void dgTable_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            e.Column.FillWeight = 10;
        }

        private void NumPage_ValueChanged(object sender, EventArgs e)
        {
            int page = (int)NumPage.Value;
            if (page > 0 && page <= MaxPages)
            {
                Page = page;
            }
        }

        private void zeilenLöschenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (DeleteRows form = new DeleteRows(RowCount, DatabaseHelper.GetAliasColumnMapping(TableName)))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    StartLoadingBar();
                    int deleted;
                    if (form.Range == null)
                    {
                        deleted = DatabaseHelper.DeleteRowByMatch(form.ColumnText, form.ColumnName, form.EqualsText, TableName);
                    }
                    else
                    {
                        DeleteRows(form.Range);
                        deleted = form.Range.Length;
                    }
                    SetRowCount(RowCount - deleted);
                    DatabaseHelper.SetSavepoint();
                    StopLoadingBar();
                }
            }
        }

        private void sortierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SortForm form = new SortForm(DatabaseHelper.GetAliasColumnMapping(TableName), SortingOrder, OrderType))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {

                    OrderType = form.OrderType;
                    SetSorting(form.SortString);
                    LoadData(true);
                }
            }
        }
    }
}
