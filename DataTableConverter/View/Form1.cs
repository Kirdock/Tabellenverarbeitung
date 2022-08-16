using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
using DataTableConverter.View;
using DataTableConverter.View.WorkProcViews;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace DataTableConverter
{
    public partial class Form1 : Form
    {
        private int selectedRow = 0, selectedColumn = 0;
        private List<Proc> procedures;
        private List<Work> Workflows;
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
        private static HashSet<string> FormInstances = new HashSet<string>();
        private decimal Page
        {
            get
            {
                return NumPage.Value;
            }
            set
            {
                SetPage();
                LoadData(true, false, true);
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
            bool isExistingDatabase = databaseName != null;
            if(FormInstances.Contains(databaseName))
            {
                databaseName = Guid.NewGuid().ToString();
            }
            FormInstances.Add(databaseName);

            InitializeComponent();
            DatabaseHelper = new DatabaseHelper(databaseName, !isExistingDatabase);
            ExportHelper = new ExportHelper(DatabaseHelper);
            DatabaseHelper.ExportHelper = ExportHelper;
            ImportHelper = new ImportHelper(ExportHelper, DatabaseHelper);
            DictSorting = new Dictionary<string, SortOrder>();
            ColumnWidths = new Dictionary<string, int>();
            SetSize();
            ExportHelper.CheckRequired();
            LoadFiles();
            SetMenuEnabled(false);
            öffnenToolStripMenuItem1.Click += (sender, e) => importToolStripMenuItem_Click();
            cSVToolStripMenuItem.Click += (sender, e) => cSVToolStripMenuItem_Click();
            dBASEToolStripMenuItem1.Click += (sender, e) => dBASEToolStripMenuItem_Click(sender, e);
            excelToolStripMenuItem1.Click += (sender, e) => excelToolStripMenuItem_Click(sender, e);

            ViewHelper.SetDataGridViewStyle(dgTable);
            UpdateHelper.CheckUpdate(true, pgbLoading, this);
#if DEBUG
            commitToolStripMenuItem.Click += commitToolStripMenuItem_Click;
            commitToolStripMenuItem.Visible = true;
#endif

            if (isExistingDatabase)
            {
                LoadData(true, false, true);
                SetMenuEnabled(true);
            }
            else if (path != null)
            {
                importToolStripMenuItem_Click(ImportState.None, new string[] { path });
            }
        }

        private void LoadFiles()
        {
            LoadProcedures();
            LoadWorkflows();
            LoadTolerances();
            LoadCases();
        }

#if DEBUG

        private void commitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DatabaseHelper.Commit();
        }
#endif

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Properties.Settings.Default.LongPathEnabled)
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\FileSystem"))
                    {
                        if (key?.GetValue("LongPathsEnabled")?.ToString() != "1")
                        {
                            DialogResult result = this.MessagesYesNo(MessageBoxIcon.Warning, "Lange Pfade werden nicht unterstützt.\nBitte führen Sie die Datei \"LongPath.reg\" aus.\nSoll diese Nachricht zukünftig ausgeblendet werden?");
                            if (result == DialogResult.Yes)
                            {
                                Properties.Settings.Default.LongPathEnabled = true;
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    ErrorHelper.LogMessage(ex, this, false);
                }
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

        private void SetWidth()
        {
            if (Properties.Settings.Default.FullWidthImport)
            {
                foreach (DataGridViewColumn col in dgTable.Columns.Cast<DataGridViewColumn>())
                {
                    if (ColumnWidths.TryGetValue(col.HeaderText, out int value))
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
            string result = (dgTable.DataSource as DataTable)
                ?.AsEnumerable()
                .Select(row => row[col.Name].ToString())
                .Concat(new string[] { col.HeaderText })
                .Aggregate(string.Empty, (seed, f) => f.Length > seed.Length ? f : seed);
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
            if (!ColumnWidths.ContainsKey(col.HeaderText))
            {
                ColumnWidths.Add(col.HeaderText, col.Width);
            }
            else
            {
                ColumnWidths[col.HeaderText] = col.Width;
            }
        }

        private void LoadProcedures(List<Proc> procs = null)
        {
            ersetzenToolStripMenuItem.DropDownItems.Clear();

            procedures = procs ?? ImportHelper.LoadProcedures(this);

            ToolStripMenuItem tempProcedure = new ToolStripMenuItem("Temporäre Eingabe");
            tempProcedure.Click += TempProcedure_Click;
            ersetzenToolStripMenuItem.DropDownItems.Add(tempProcedure);
            List<Proc> visibleProc = procedures.Where(vproc => !vproc.HideInMainForm).ToList();
            foreach(Proc proc in visibleProc)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(proc.Name.Replace("&", "&&"));
                item.Click += (sender, e) => procedure_Click(proc);
                ersetzenToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void LoadWorkflows(List<Work> workflow = null)
        {
            ClearDropDownItems(WorkflowItem19, WorkflowItemAF, WorkflowItemGL, WorkflowItemMQ, WorkflowItemRZ);

            Workflows = workflow ?? ImportHelper.LoadWorkflows(this);

            List<Work> workflowsCopy = GetCopyOfWorkflows(Workflows); //copy because then the real source will not be modified (e.g. column name change)

            foreach(Work work in workflowsCopy)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(work.Name.Replace("&", "&&"));
                item.Click += (sender, e) => workflow_Click(work);
                AddWorkflowItem(item);
            }
        }

        private void ClearDropDownItems(params ToolStripMenuItem[] items)
        {
            foreach(ToolStripMenuItem item in items)
            {
                item.DropDownItems.Clear();
            }
        }

        private void AddWorkflowItem(ToolStripMenuItem newItem)
        {
            ToolStripMenuItem item;
            if (Regex.IsMatch(newItem.Text, "^[1-9]", RegexOptions.IgnoreCase))
            {
                item = WorkflowItem19;
            }
            else if(Regex.IsMatch(newItem.Text, "^[A-F]", RegexOptions.IgnoreCase))
            {
                item = WorkflowItemAF;
            }
            else if (Regex.IsMatch(newItem.Text, "^[G-L]", RegexOptions.IgnoreCase))
            {
                item = WorkflowItemGL;
            }
            else if (Regex.IsMatch(newItem.Text, "^[M-Q]", RegexOptions.IgnoreCase))
            {
                item = WorkflowItemMQ;
            }
            else
            {
                item = WorkflowItemRZ;
            }
            item.DropDownItems.Add(newItem);
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

                // ProcSplit: No way to check if the column is available or not because the amount of created columns is dynamic and depends on the cells

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
                                    else if (to[i] != SelectDuplicateColumns.KeepColumn && wpHeaders[y] == from[i] && nf.Headers.Contains(from[i]))
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
                SetWorkflowText();
            });
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        internal void SetWorkflowText(string text = "")
        {
            statusStrip1.BeginInvoke(new MethodInvoker(() =>
            {
                LblWorkProc.Text = text;
            }));
        }

        private int GetProcedureThroughId(int id)
        {
            return procedures.FindIndex(p => p.Id == id);
        }

        private void importToolStripMenuItem_Click(ImportState state = ImportState.None, string[] openFiles = null)
        {
            using (OpenFileDialog dialog = ImportHelper.GetOpenFileDialog(state != ImportState.Header))
            {
                string mergePath = string.Empty;
                bool validMerge = state == ImportState.Merge && ProcAddTableColumns.CheckFile(FilePath, ref mergePath);
                if (openFiles != null || validMerge || dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string[] filenames = openFiles ?? (validMerge ? new string[1] { mergePath } : dialog.FileNames);
                    ProgressBar loadingBar = pgbLoading;
                    Thread thread = new Thread(() =>
                    {
                        bool fileNameSet = state != ImportState.None;
                        bool multipleFiles = filenames.Length > 1;
                        Dictionary<string, ImportSettings> fileImportSettings = new Dictionary<string, ImportSettings>();
                        string newTable = null;
                        string fileNameBefore = Path.GetFileName(filenames[0]);
                        int fileEncoding = 0;
                        string password = string.Empty;
                        foreach (string file in filenames)
                        {
                            try
                            {
                                string filename = Path.GetFileName(file);
                                string tableName = ImportHelper.ImportFile(file, multipleFiles, fileImportSettings, contextGlobal, loadingBar, this, ref fileEncoding, ref password);

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
                        try
                        {
                            FinishImport(newTable, state, Path.GetFileName(filenames[0]), fileEncoding);
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex, this);
                        }
                    });
                    thread.IsBackground = true;
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
            }
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
                                DataHelper.StartMerge(tableName, encoding, FilePath, Properties.Settings.Default.PVMIdentifier, Properties.Settings.Default.PVMIdentifier, Properties.Settings.Default.InvalidColumnName, this, TableName);
                                StopLoadingBar();
                                DatabaseHelper.SetSavepoint();
                                LoadData(true);
                            }
                            catch (Exception ex)
                            {
                                ErrorHelper.LogMessage(ex, this);
                            }
                        });
                        thread.IsBackground = true;
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                        break;

                    case ImportState.Append:
                        DatabaseHelper.ConcatTable(TableName, tableName, Path.GetFileName(FilePath), filename);
                        DatabaseHelper.SetSavepoint();
                        LoadData(true);
                        break;

                    case ImportState.Header:
                        DatabaseHelper.RenameColumns(tableName, TableName);
                        DatabaseHelper.SetSavepoint();
                        LoadData(true);
                        break;

                    default:
                        ResetValidRowLabel();
                        SetSorting(string.Empty);
                        DatabaseHelper.ReplaceTable(tableName, TableName);
                        DatabaseHelper.SetSavepoint();
                        BeginInvoke(new MethodInvoker(() =>
                        {
                            SetMenuEnabled(true);
                        }));
                        LoadData(true);
                        break;
                }

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
                try
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
                    CheckAllowToAddRows();

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
                catch(OutOfMemoryException)
                {
                    ErrorHelper.ShowError("Es können nicht so viele Zeilen gelesen werden. Bitte reduzieren sie die maximal geladenen Zeilen pro Seite in den Einstellungen", this);
                    GC.Collect();
                }
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
                            form.Proc.DoWork(ref order, null, null, null, FilePath, contextGlobal, OrderType, this, TableName);
                            SetWorkflowText();
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
            using (Formula formula = new Formula(DatabaseHelper.GetSortedColumnsAsAlias(TableName)))
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
                            SetWorkflowText();
                        });
                        thread.IsBackground = true;
                        thread.SetApartmentState(ApartmentState.STA);
                        thread.Start();
                    }
                }
            }
        }

        private void ReplaceProcedure(Proc procedure, WorkProc wp)
        {
            string newOrder = GetSorting();

            wp.DoWork(ref newOrder, GetCaseThroughId(wp.ProcedureId), tolerances, procedure ?? GetProcedure(wp.ProcedureId), FilePath, contextGlobal, OrderType, this, TableName);

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
            bool saved = ExportHelper.Save(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), Path.GetExtension(FilePath), FileEncoding, format, GetSorting(), OrderType, this, TableName, UpdateLoadingBar) != 0;
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
                    string newAlias = DatabaseHelper.RenameAlias(oldText, newText, TableName);
                    dgTable.Columns[selectedColumn].HeaderText = newAlias;
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
                    Thread.CurrentThread.IsBackground = true;
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
                    Thread.CurrentThread.IsBackground = true;
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
                long id = long.Parse(dgTable[DatabaseHelper.IdColumnName, e.RowIndex].Value.ToString());
                int width = TextRenderer.MeasureText(value, dgTable.DefaultCellStyle.Font).Width + ColumnWidthTolerance;
                DataGridViewColumn col = dgTable.Columns[e.ColumnIndex];
                if (width > col.Width)
                {
                    ColumnWidths[col.HeaderText] = col.Width = width;
                }
                DatabaseHelper.UpdateCell(value, dgTable.Columns[e.ColumnIndex].HeaderText, id, TableName);
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
                SetRowCount(RowCount + 1);
            }
        }

        private void tabellenZusammenfügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(ImportState.Merge);
        }

        private void verwaltungToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] aliases = new string[0];
            try
            {
                aliases = DatabaseHelper.GetSortedColumnsAsAlias(TableName).ToArray();
            }
            catch { }
            Administration form = new Administration(DatabaseHelper, ExportHelper, ImportHelper, aliases, contextGlobal, procedures, Workflows, cases, tolerances, TableName);
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

        internal void UpdateLoadingBar()
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

        internal void StartLoadingBarCount(int length)
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
            using (PVMExport formula = new PVMExport(DatabaseHelper.GetSortedColumnsAsAlias(TableName)))
            {
                if (formula.ShowDialog(this) == DialogResult.OK)
                {
                    new ProcPVMExport(formula.SelectedHeaders(), formula.SelectedFormat).DoWork(ref SortingOrder, null, null, null, FilePath, null, OrderType, this, TableName);
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
                    string continuedNumberName = export.ContinuedNumberName;
                    IEnumerable<ExportCustomItem> items = export.Items;
                    Thread thread = new Thread(() =>
                    {
                        try
                        {
                            ExportHelper.ExportTableWithColumnCondition(items, FilePath, FileEncoding, GetSorting(), OrderType, this, continuedNumberName, TableName, SetWorkflowText);
                            StopLoadingBar();
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex, this);
                        }
                        SetWorkflowText();
                        SaveFinished();
                    });
                    thread.IsBackground = true;
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
            }
        }

        internal void StartLoadingBar()
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
                pgbLoading.Maximum = 1;
                pgbLoading.Value = 0;
            }));
        }

        private void zählenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ExportCount export = new ExportCount(DatabaseHelper.GetAliasColumnMapping(TableName), DatabaseHelper, TableName))
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
                    thread.IsBackground = true;
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

                    SetSorting($"[{DatabaseHelper.GetColumnName(col.HeaderText, TableName)}] COLLATE NATURALSORT {(asc ? "DESC" : "ASC")}");
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
            DatabaseHelper.SetSavepoint();
            LoadData(false);
        }

        private void DeleteRows(int[] rows)
        {
            DatabaseHelper.DeleteRowsByIndex(rows, GetSorting(), OrderType, TableName);
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
            if (e.RowIndex == -1 && e.ColumnIndex > -1 && AliasColumnMapping != null)
            {
                string header = AliasColumnMapping.FirstOrDefault(pair => pair.Key.Equals(e.Value.ToString(), StringComparison.OrdinalIgnoreCase)).Value;
                if (header != null)
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
                            Thread.CurrentThread.IsBackground = true;
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
                    DatabaseHelper.SetSavepoint();
                    LoadData(true);
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
            LoadFiles();
            dgTable.DefaultCellStyle.Font = new Font(dgTable.DefaultCellStyle.Font.Name, Properties.Settings.Default.TableFontSize);
            dgTable.RowTemplate.Height = Properties.Settings.Default.RowHeight;

            if (oldHeight != Properties.Settings.Default.RowHeight)
            {
                ColumnWidths.Clear();
                LoadData(true, true);
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
            using (CompareForm form = new CompareForm(DatabaseHelper.GetSortedColumnsAsAlias(TableName).ToArray(), DatabaseHelper, TableName))
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
                        Thread.CurrentThread.IsBackground = true;
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

                        int index = -1;
                        if (shortcut != string.Empty && newColumn != string.Empty)
                        {
                            newColumn = DatabaseHelper.AddColumnWithAdditionalIfExists(newColumn, TableName);

                            if (minLength != -1)
                            {
                                DatabaseHelper.UpdateRowsWithMinCharacters(columnName, minLength, shortcut, newColumn, TableName);
                            }
                            else
                            {
                                long id = DatabaseHelper.GetRowWithMaxCharacters(GetSorting(), OrderType, out index, TableName);
                                DatabaseHelper.UpdateCell(shortcut, newColumn, id, TableName, true);
                            }
                            DatabaseHelper.SetSavepoint();
                            LoadData(true);
                        }
                        if (index != -1)
                        {
                            SelectDataGridViewRow(index);
                        }
                        StopLoadingBar();
                    });
                    thread.IsBackground = true;
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
            }
        }

        private void SelectDataGridViewRow(long index)
        {
            decimal maxRecords = Properties.Settings.Default.MaxRows;
            decimal desiredPage = Math.Ceiling((index + 1) / maxRecords);
            decimal newIndex = index;
            while (newIndex >= maxRecords)
            {
                newIndex -= maxRecords;
            }

            Invoke(new MethodInvoker(() =>
            {
                if (desiredPage != Page)
                {
                    NumPage.Value = desiredPage;
                }

                dgTable.ClearSelection();
                dgTable.Rows[(int)newIndex].Selected = true;
                dgTable.FirstDisplayedScrollingRowIndex = (int)newIndex;
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
            long index = DatabaseHelper.SearchValue(searchText, alias, totalSearch, GetSorting(), OrderType, TableName);
            if (index == -1)
            {
                MessageHandler.MessagesOK(this, MessageBoxIcon.Warning, $"Der Suchtext \"{searchText}\" konnte nicht gefunden werden");
            }
            else
            {
                SelectDataGridViewRow(index);
            }
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
                    if (form.Range == null)
                    {
                        DatabaseHelper.DeleteRowByMatch(form.ColumnText, form.ColumnName, form.EqualsText, TableName);
                    }
                    else
                    {
                        DeleteRows(form.Range);
                    }
                    DatabaseHelper.SetSavepoint();
                    LoadData(false);
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

        private void tausenderTrennzeichenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Formula formula = new Formula(DatabaseHelper.GetSortedColumnsAsAlias(TableName)))
            {
                if (formula.ShowDialog(this) == DialogResult.OK)
                {
                    string[] columns = formula.SelectedHeaders();

                    if (columns.Length > 0)
                    {
                        ProcThousandSeparator proc = new ProcThousandSeparator(columns, formula.HeaderName(), formula.OldColumn);
                        StartSingleWorkflow(proc);
                    }
                }
            }
        }

        private void dividierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (DivideForm form = new DivideForm(DatabaseHelper.GetSortedColumnsAsAlias(TableName)))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    StartSingleWorkflow(form.Proc);
                }
            }
        }

        private void CheckAllowToAddRows()
        {
            dgTable.RowsAdded -= dgTable_RowsAdded;
            dgTable.AllowUserToAddRows = NumPage.Value == MaxPages;
            dgTable.RowsAdded += dgTable_RowsAdded;
        }
    }
}
