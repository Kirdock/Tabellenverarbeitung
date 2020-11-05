using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
using DataTableConverter.Extensions;
using DataTableConverter.View;
using DataTableConverter.View.WorkProcViews;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
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
        private readonly HistoryHelper historyHelper;
        private string tableValueBefore;
        private DataTable sourceTable;
        private string SortingOrder = string.Empty;
        private Dictionary<string, SortOrder> dictSorting;
        private int rowBefore; //When a row in DataGridView is changed, then I'm not able to get the value through dgTable[col, row], because the DataGridView was sorted again at this moment
        private List<string> dictKeys;
        private string FilePath = string.Empty;
        private OrderType OrderType = OrderType.Windows;
        private readonly Dictionary<string, int> ColumnWidths;
        private readonly int ColumnWidthTolerance = 10;
        private int RowCount = 0;
        internal int FileEncoding = 0;
        private decimal MaxPages => Math.Ceiling(((decimal)(sourceTable?.Rows.Count ?? 1) / Properties.Settings.Default.MaxRows));
        private decimal Page {
            get {
                return NumPage.Value;
            }
            set
            {
                dgTable.RowsAdded -= dgTable_RowsAdded;
                dgTable.AllowUserToAddRows = NumPage.Value == MaxPages;
                dgTable.RowsAdded += dgTable_RowsAdded;
                SetPage();
                AssignDataSource();
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

        internal Form1(DataTable table = null, string path = null)
        {
            dictSorting = new Dictionary<string, SortOrder>();
            ColumnWidths = new Dictionary<string, int>();
            dictKeys = new List<string>();
            InitializeComponent();
            DatabaseHelper.Init();
            SetSize();
            ExportHelper.CheckRequired();
            LoadProcedures();
            LoadWorkflows();
            LoadTolerances();
            LoadCases();
            SetMenuEnabled(false);
            historyHelper = new HistoryHelper();
            öffnenToolStripMenuItem1.Click += (sender, e) => importToolStripMenuItem_Click();
            trimToolStripMenuItem.Click += (sender, e) => trimToolStripMenuItem_Click(sender, e);
            cSVToolStripMenuItem.Click += (sender, e) => cSVToolStripMenuItem_Click(sender, e);
            dBASEToolStripMenuItem1.Click += (sender, e) => dBASEToolStripMenuItem_Click(sender, e);
            excelToolStripMenuItem1.Click += (sender, e) => excelToolStripMenuItem_Click(sender, e);
            if (table != null)
            {
                AddDataSourceNewTable(table);
            }
            ViewHelper.SetDataGridViewStyle(dgTable);
            UpdateHelper.CheckUpdate(true, pgbLoading, this);
            if(path != null)
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
            foreach(ToolStripMenuItem item in new ToolStripMenuItem[] { speichernAlsToolStripMenuItem, benutzerdefiniertesSpeichernToolStripMenuItem1, arbeitsablaufToolStripMenuItem, funktionenToolStripMenuItem, ersetzenToolStripMenuItem, duplikateToolStripMenuItem })
            {
                SetSubItemEnabled(status, item);
            }
            
            foreach(ToolStripMenuItem item in new ToolStripMenuItem[] { speichernToolStripMenuItem2 , sortierenToolStripMenuItem })
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
            SortingOrder = order;
            dictSorting = ViewHelper.GenerateSortingList(GetSorting());
            dictKeys = dictSorting.Keys.ToList();
        }


        private void AssignDataSource(DataTable table = null, bool adjustColumnWidth = false)
        {
            if (adjustColumnWidth)
            {
                ColumnWidths.Clear();
            }
            else
            {
                SaveWidthOfDataGridViewColumns();
            }
            int scrollBarHorizontal = dgTable.HorizontalScrollingOffset;
            OrderType orderType = OrderType;
            (dgTable.DataSource as DataView)?.Dispose(); //in hope to remove all remaining lazy loading
            dgTable.DataSource = null; //else some columns (added through History) will be shown at index 0 instead of the right one
            sourceTable = table ?? sourceTable;
            int rowCount = sourceTable.Rows.Count;
            dgTable.DataSource = sourceTable.GetSortedView(SortingOrder, OrderType, (int) Page, delegate { AddDataSourceAddRow(rowCount, orderType); });
            RestoreDataGridSortMode();
            SetWidth();

            dgTable.HorizontalScrollingOffset = scrollBarHorizontal;
        }


        private void RestoreDataGridSortMode()
        {

            for (int i = 0; i < dgTable.Columns.Count; i++)
            {
                dgTable.Columns[i].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
        }

        private void AssignDataSourceColumnChange(DataTable table, string column, string newColumn = null)
        {
            SetSorting(ViewHelper.AdjustSort(GetSorting(), column, newColumn));
            
            AssignDataSource(table);            
            RestoreDataGridSortMode();
        }

        private string GetSorting()
        {
            return SortingOrder;
        }

        private DataTable GetDataSource(bool withSort = false)
        {
            DataTable table = sourceTable;
            if (table != null)
            {
                int rowCountBefore = table.Rows.Count;
                EndEdit();
                table = sourceTable.Copy();
                if (withSort)
                {
                    table = table.GetSortedView(SortingOrder,OrderType,-1).ToTable();
                }

                while(rowCountBefore < table.Rows.Count)
                {
                    table.Rows.RemoveAt(table.Rows.Count - 1);
                }
            }
            table?.AcceptChanges();
            return table;
        }

        private void EndEdit()
        {
            ViewHelper.EndDataGridViewEdit(dgTable);
        }

        #region Add History Entry

        private void AddDataSourceValueChange(DataTable tableNew)
        {
            List<CellMatrix> oldValues = tableNew.ChangesOfDataTable();
            historyHelper.AddHistory(new History { State = State.ValueChange, Table = oldValues}, GetSorting());
            AssignDataSource(tableNew);
        }

        private void AddDataSourceNewTable(DataTable table)
        {
            historyHelper.ResetHistory();
            SetSorting(string.Empty);
            AssignDataSource(table, true);
            SetMenuEnabled(true);
            ResetPage();
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
            string result = sourceTable.AsEnumerable().Select(row => row[col.Name].ToString()).Concat(new string[] { col.Name }).Aggregate(string.Empty, (seed, f) => f.Length > seed.Length ? f : seed);
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

        private void AddDataSourceAddHistory(List<CellMatrix> newEntry)
        {
            historyHelper.AddHistory(new History { State = State.ValueChange, Table = newEntry }, GetSorting());
        }

        private void AddDataSourceAddColumn(int columnIndex)
        {
            historyHelper.AddHistory(new History { State = State.InsertColumn, ColumnIndex = columnIndex }, GetSorting());
        }

        private void AddDataSourceAddColumnAndRows(int columnIndex, int rowIndex)
        {
            historyHelper.AddHistory(new History { State = State.AddColumnsAndRows, ColumnIndex = columnIndex, RowIndex = rowIndex }, GetSorting());
        }

        private void AddDataSourceAddRow(int rowIndex, OrderType orderType = OrderType.Windows)
        {
            historyHelper.AddHistory(new History { State = State.InsertRow, RowIndex = rowIndex, OrderType = orderType }, GetSorting());
        }

        private void AddDataSourceHeaderChange(int columnIndex, string text)
        {
            historyHelper.AddHistory(new History { State = State.HeaderChange, NewText = text, ColumnIndex = columnIndex }, GetSorting());
        }

        private void AddDataSourceHeadersChange(object[] oldHeader)
        {
            object[][] newItem = new object[1][];
            newItem[0] = oldHeader;
            historyHelper.AddHistory(new History { State = State.HeadersChange, Row = newItem}, GetSorting());
        }

        private void AddDataSourceDeleteColumn(DataColumn col, int index, object[] columnValues)
        {
            DataColumn[] newItem = new DataColumn[1];
            newItem[0] = col;
            object[][] newValues = new object[1][];
            newValues[0] = columnValues;
            historyHelper.AddHistory(new History { State = State.DeleteColumn, ColumnIndex = index, Column = newItem, ColumnValues = newValues }, GetSorting());
        }

        private void AddDataSourceCellChanged(string newText, int columnIndex, int rowIndex)
        {
            historyHelper.AddHistory(new History { State = State.CellValueChange, NewText = newText, ColumnIndex = columnIndex, RowIndex = rowIndex }, GetSorting());
        }

        #endregion

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
                string name = workflowsCopy[i].Name.Replace("&","&&");
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
            tolerances = tol ??  ImportHelper.LoadTolerances();
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
            DataTable table = GetDataSource();
            List<string> headers = new List<string>(table.HeadersOfDataTableAsString());
            

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

                WorkflowHelper.CheckHeaders(headers, notFoundColumns, wpHeaders);
                

                if(notFoundColumns.Count > 0)
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
                SelectDuplicateColumns form = new SelectDuplicateColumns(columns.ToArray(), DatabaseHelper.GetColumnAliasMapping(), false);
                if (form.ShowDialog(this) == DialogResult.OK) {
                    string[] from = form.Table.AsEnumerable().Select(row => row.ItemArray[0].ToString()).ToArray();
                    string[] to = form.Table.AsEnumerable().Select(row => row.ItemArray[1].ToString()).ToArray();

                    foreach (NotFoundHeaders nf in notFound)
                    {
                        string[] wpHeaders = nf.Wp.GetHeaders();
                        for (int y = 0; y < wpHeaders.Length; y++)
                        {
                            for (int i = 0; i < from.Length; i++)
                            {
                                if(to[i] == SelectDuplicateColumns.IgnoreColumn)
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
                form.Dispose();
            }
        }

        private void ReplaceThroughTemp(List<WorkProc> temp, Action finished)
        {
            DataTable table = GetDataSource();
            StartLoadingBar();
            Thread thread = new Thread(() =>
            {
                try
                {
                    table.AcceptChanges();
                    List<History> history = new List<History>();
                    foreach (WorkProc t in temp)
                    {
                        if (t.ReplacesTable)
                        {
                            history.Add(new History { State = State.ValueChange, Table = table.ChangesOfDataTable(), Order = GetSorting()});
                            
                            ReplaceProcedure(table, null, t, out int[] newIndices);
                            history.Add(new History { State = State.ValueChange, Table = table.ChangesOfDataTable(), Order = GetSorting()});
                            if (table.Columns.Contains(Extensions.DataTableExtensions.TempSort))
                            {
                                table.Columns.Remove(Extensions.DataTableExtensions.TempSort);
                            }
                            table.AcceptChanges();
                            history.Add(new History { State = State.OrderIndexChange, NewOrderIndices = newIndices, Order = GetSorting() });
                        }
                        else
                        {
                            ReplaceProcedure(table, null, t, out _);
                        }
                        if (t.CommitDelete)
                        {
                            history.Add(new History { State = State.ValueChange, Table = table.ChangesOfDataTable(), Order = GetSorting() });
                            if (table.Columns.Contains(Extensions.DataTableExtensions.TempSort))
                            {
                                table.Columns.Remove(Extensions.DataTableExtensions.TempSort);
                            }
                            table.AcceptChanges();
                        }
                    }
                    history.Add(new History { State = State.ValueChange, Table = table.ChangesOfDataTable(), Order = GetSorting() });

                    historyHelper.AddHistory(history.ToArray());
                    dgTable.Invoke(new MethodInvoker(() =>
                    {
                        AssignDataSource(table);
                    }));
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
            bool validMerge = state == ImportState.Merge && ProcAddTableColumns.CheckFile(FilePath,ref mergePath);
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
            if(tableName != null) {
                switch (state)
                {
                    case ImportState.Merge:
                        StartMerge(tableName, filename, encoding);
                        DatabaseHelper.SetSavepoint();
                        break;

                    case ImportState.Append:
                        DatabaseHelper.ConcatTable(tableName, Path.GetFileName(FilePath), filename);
                        DatabaseHelper.SetSavepoint();
                        break;

                    case ImportState.Header:
                        DatabaseHelper.RenameColumns(tableName);
                        List<string> newHeaders = DatabaseHelper.GetSortedColumnsAsAlias(tableName);
                        dgTable.BeginInvoke(new MethodInvoker(() =>
                        {
                            for (int i = 0; i < newHeaders.Count && i < dgTable.ColumnCount; ++i)
                            {
                                dgTable.Columns[i].Name = newHeaders[i];
                            }
                        }));
                        DatabaseHelper.SetSavepoint();
                        break;

                    default:
                        ResetValidRowLabel();
                        //delete main table and rename table with tableName to main
                        DatabaseHelper.ReplaceTable(tableName);
                        //dgTable.BeginInvoke(new MethodInvoker(() => { AddDataSourceNewTable(tableName); }));
                        lblRows.GetCurrentParent().BeginInvoke(new MethodInvoker(() => { SetRowCount(); }));
                        break;
                }
                LoadData();
                
            }
            StopLoadingBar();
        }

        private void LoadData()
        {
            dgTable.Invoke(new MethodInvoker(() =>
            {
                dynamic datasource = dgTable.DataSource;
                datasource?.Dispose();
                dgTable.DataSource = DatabaseHelper.GetData(SortingOrder, OrderType, (int)((Page - 1) * Properties.Settings.Default.MaxRows));
                dgTable.Columns[0].Visible = false;
            }));
        }

        private void StartMerge(string importTable, string filename, int encoding)
        {
            string[] importColumnNames = new string[0];
            string sourceIdentifierColumnName = null;
            string importIdentifierColumnName = null;
            DialogResult result = DialogResult.No;
            int importRowCount = DatabaseHelper.GetRowCount(importTable);
            int originalRowCount = DatabaseHelper.GetRowCount();
            Dictionary<string,string> importTableColumnAliasMapping = DatabaseHelper.GetColumnAliasMapping(importTable);
            Dictionary<string, string> originalTableColumnAliasMapping = DatabaseHelper.GetColumnAliasMapping();

            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.PVMIdentifier))
            {
                result = ShowMergeForm(ref importColumnNames, ref sourceIdentifierColumnName, ref importIdentifierColumnName, originalTableColumnAliasMapping, originalRowCount, importTableColumnAliasMapping, importRowCount, filename, this);
            }
            else
            {
                if (originalTableColumnAliasMapping.ContainsKey(Properties.Settings.Default.PVMIdentifier))
                {
                    sourceIdentifierColumnName = Properties.Settings.Default.PVMIdentifier;
                    
                    
                    if (importTableColumnAliasMapping.ContainsKey(Properties.Settings.Default.PVMIdentifier))
                    {
                        importIdentifierColumnName = Properties.Settings.Default.PVMIdentifier;
                        
                        importColumnNames = importTableColumnAliasMapping.Values.Cast<string>().ToArray();
                        result = DialogResult.Yes;
                        if (originalRowCount != importRowCount)
                        {
                            result = this.MessagesYesNo(MessageBoxIcon.Warning, $"Die Zeilenanzahl der beiden Tabellen stimmt nicht überein ({originalRowCount} zu {importRowCount })!\nTrotzdem fortfahren?");
                        }
                    }
                    else
                    {
                        this.MessagesOK(MessageBoxIcon.Warning, $"Die zu importierende Tabelle \"{filename}\" hat keine Spalte mit der Bezeichnung {Properties.Settings.Default.PVMIdentifier}");
                        result = ShowMergeForm(ref importColumnNames, ref sourceIdentifierColumnName, ref importIdentifierColumnName, originalTableColumnAliasMapping, originalRowCount, importTableColumnAliasMapping, importRowCount, filename, this);
                    }
                }
                else
                {
                    this.MessagesOK(MessageBoxIcon.Warning, $"Die Haupttabelle hat keine Spalte mit der Bezeichnung {Properties.Settings.Default.PVMIdentifier}");
                    result = ShowMergeForm(ref importColumnNames, ref sourceIdentifierColumnName, ref importIdentifierColumnName, originalTableColumnAliasMapping, originalRowCount, importTableColumnAliasMapping, importRowCount, filename, this);
                }
            }
            
            if (result == DialogResult.Yes)
            {
                Thread thread = new Thread(() =>
                {
                    try {
                        string invalidColumnAlias = Properties.Settings.Default.InvalidColumnName;
                        if (!importColumnNames.Contains(invalidColumnAlias))
                        {
                            SelectDuplicateColumns f = new SelectDuplicateColumns(new string[] { invalidColumnAlias }, importTableColumnAliasMapping, true);
                            DialogResult res = DialogResult.Cancel;
                            Invoke(new MethodInvoker(() =>
                            {
                                res = f.ShowDialog(this);
                            }));
                            if (res == DialogResult.OK)
                            {
                                invalidColumnAlias = f.Table.AsEnumerable().First()[1].ToString();
                            }
                        }

                        bool abort = DatabaseHelper.PVMImport(importTable, importColumnNames, sourceIdentifierColumnName, importIdentifierColumnName, originalTableColumnAliasMapping, this);

                        if (abort) return;

                        
                        int count = 0;
                        if (Properties.Settings.Default.SplitPVM)
                        {
                            count = DatabaseHelper.PVMSplit(FilePath, this, encoding, invalidColumnAlias);
                        }
                        DatabaseHelper.DeleteInvalidRows();

                        dgTable.Invoke(new MethodInvoker(()=> {
                            AssignDataSource();
                        }));

                        pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Value = pgbLoading.Maximum = 0; }));

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
            }
        }

        internal static DialogResult ShowMergeForm(ref string[] importColumns, ref string sourceColumnName, ref string importColumnName, Dictionary<string,string> originalTableHeaders, int originalRowCount, Dictionary<string, string> importTableHeaders, int importRowCount, string filename, Form invokeForm)
        {
            MergeTable form = new MergeTable(originalTableHeaders, importTableHeaders, filename, originalRowCount, importRowCount);
            bool result;
            DialogResult res = DialogResult.Cancel;
            invokeForm.Invoke(new MethodInvoker(() =>
            {
                res = form.ShowDialog(invokeForm);
            }));
            if (result = ( res == DialogResult.Yes))
            {
                importColumns = form.SelectedColumns.ToArray();
                sourceColumnName = form.OriginalIdentifierColumnName;
                importColumnName = form.ImportIdentifierColumnName;
            }
            form.Dispose();
            return result ? DialogResult.Yes : DialogResult.No;
        }

        private void SetRowCount()
        {
            SetRowCount(DatabaseHelper.GetRowCount());
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
                TrimForm form = new TrimForm(sourceTable.HeadersOfDataTableAsString());
                if(form.ShowDialog(this) == DialogResult.OK)
                {
                    StartLoadingBar();
                    DataTable table = GetDataSource();
                    new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.IsBackground = true;
                            string order = GetSorting();
                            form.Proc.DoWork(table, ref order, null, null, null, FilePath, contextGlobal, OrderType, this, out int[] newIndices);
                            dgTable.Invoke(new MethodInvoker(() =>
                            {
                                AddDataSourceValueChange(table);
                            }));
                            
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
            if(form.ShowDialog(this) == DialogResult.OK)
            {
                Proc proc = new Proc(form.Table,form.CheckTotal, form.CheckWord, form.LeaveEmpty);
                procedure_Click(proc);
            }
            form.Dispose();
        }

        private void procedure_Click(Proc procedure)
        {
            Formula formula = new Formula(FormulaState.Procedure, sourceTable.HeadersOfDataTable());
            if (formula.ShowDialog(this) == DialogResult.OK)
            {
                string[] columns = formula.SelectedHeaders();

                if (columns.Length > 0)
                {
                    ProcUser user = new ProcUser(columns, formula.HeaderName(), formula.OldColumn);

                    DataTable newTable = GetDataSource();
                    Thread thread = new Thread(() =>
                    {
                        try
                        {
                            ReplaceProcedure(newTable, procedure, user, out int[] newIndices);
                            if (newIndices.Length == 0)
                            {
                                dgTable.Invoke(new MethodInvoker(() => { AddDataSourceValueChange(newTable); }));
                            }
                            else
                            {
                                historyHelper.AddHistory(new History { State = State.OrderIndexChange, NewOrderIndices = newIndices }, GetSorting());
                                dgTable.Invoke(new MethodInvoker(() => { AssignDataSource(newTable); }));
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex, this);
                        }
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
            }
            formula.Dispose();
        }

        private void ReplaceProcedure(DataTable table, Proc procedure, WorkProc wp, out int[] newOrderIndices)
        {
            string newOrder = GetSorting();
            wp.DoWork(table, ref newOrder, GetCaseThroughId(wp.ProcedureId), tolerances, procedure ?? GetProcedure(wp.ProcedureId), FilePath, contextGlobal, OrderType, this, out int[] newIndices);
            newOrderIndices = newIndices;
            SetSorting(newOrder);
        }

        private Proc GetProcedure(int id)
        {
            Proc proc = null;
            int index = GetProcedureThroughId(id);
            if (index != -1) {
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
                    DataTable table = GetDataSource(true);
                    new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.IsBackground = true;
                            ExportHelper.ExportExcel(table, Path.GetDirectoryName(path),path, this);
                            StopLoadingBar();
                            SaveFinished();
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex, this);
                        }
                    }).Start();
                }
            }
        }

        private void spalteHinzufügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newColumn = Microsoft.VisualBasic.Interaction.InputBox("Bitte Spaltennamen eingeben", "Spalte hinzufügen", string.Empty);
            if (!string.IsNullOrWhiteSpace(newColumn))
            {
                DataTable data = GetDataSource();
                data.Columns.Add(new DataColumn(newColumn));
                AddDataSourceAddColumn(data.Columns.Count-1);
                AssignDataSource(data);
                SetRowCount();
            }
        }

        private void dgTable_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                selectedRow =  dgTable.HitTest(e.X, e.Y).RowIndex;
                selectedColumn = dgTable.HitTest(e.X, e.Y).ColumnIndex;
                if(selectedColumn > -1 && selectedRow > -1 && !dgTable[selectedColumn, selectedRow].Selected)
                {
                    foreach(DataGridViewCell cell in dgTable.SelectedCells)
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
                else if(selectedColumn >= 0)
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

                DataTable table = GetDataSource();
                table.Columns[selectedColumn].ColumnName = newText;
                
                AddDataSourceHeaderChange(selectedColumn, oldText);
                
                AssignDataSourceColumnChange(table, oldText, newText);
            }
        }

        private void spalteLöschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable table = GetDataSource();
            DataColumn col = table.Columns[selectedColumn];
            
            
            AddDataSourceDeleteColumn(col, selectedColumn, table.ColumnValues(selectedColumn));
            table.Columns.RemoveAt(selectedColumn);
            AssignDataSourceColumnChange(table, col.ColumnName);
            SetRowCount();
        }


        private void zeilenZusammenfügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Merge formula = new Merge(sourceTable.HeadersOfDataTable(), contextGlobal);
            if (formula.ShowDialog(this) == DialogResult.OK)
            {
                StartSingleWorkflow(formula.Proc);
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
                    DataTable table = GetDataSource(true);
                    StartLoadingBarCount(table.Rows.Count);
                    new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.IsBackground = true;
                            bool saved = ExportHelper.ExportDbase(Path.GetFileNameWithoutExtension(path), table, Path.GetDirectoryName(path), this, UpdateLoadingBar);
                            StopLoadingBar();
                            if (saved)
                            {
                                SaveFinished();
                            }
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
            DataTable oldTable = GetDataSource();
            StartLoadingBar();

            new Thread(() =>
            {
                try {
                    DataTable newTable = historyHelper.GoBack(oldTable, GetSorting());
                    dgTable.Invoke(new MethodInvoker(() =>
                    {
                        TakeOverHistory(newTable, historyHelper.OrderString);
                    }));
                    StopLoadingBar();
                }
                catch (Exception ex)
                {
                    ErrorHelper.LogMessage(ex, this);
                }
            }).Start();
            
            
        }

        private void TakeOverHistory(DataTable table, string orderString)
        {
            SetSorting(orderString);
            AssignDataSource(table, true);
            SetRowCount();
        }

        private void wiederholenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable oldTable = GetDataSource();
            StartLoadingBar();

            new Thread(() =>
            {
                try
                {
                    DataTable newTable = historyHelper.Repeat(GetDataSource(), GetSorting());
                    dgTable.Invoke(new MethodInvoker(() =>
                    {
                        TakeOverHistory(newTable, historyHelper.OrderString);
                    }));
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
            int width = TextRenderer.MeasureText(dgTable[e.ColumnIndex, e.RowIndex].Value.ToString(), dgTable.DefaultCellStyle.Font).Width + ColumnWidthTolerance;
            DataGridViewColumn col = dgTable.Columns[e.ColumnIndex];
            if (width > col.Width)
            {
                ColumnWidths[col.Name] = col.Width = width;
            }
            AssignDataSource();
            AddDataSourceCellChanged(tableValueBefore, e.ColumnIndex, rowBefore);
        }

        private void dgTable_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            //tableValueBefore = dgTable[e.ColumnIndex, e.RowIndex].Value.ToString();
            //rowBefore = e.RowIndex >= 0 && (RealRowCount()-1) < sourceTable.Rows.Count ? GetDataTableRowIndexOfDataGridView(e.RowIndex) : e.RowIndex >= 0 ? e.RowIndex : 0;
        }

        private void dgTable_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //var cell = dgTable.SelectedCells[0];
            //    var rowIndex = cell.RowIndex;
            //    var columnIndex = cell.ColumnIndex;
            //    AddDataSourceAddRow(sourceTable.Rows.Count, OrderType);
            //    //EndEdit();

            //    dgTable.BeginInvoke(new MethodInvoker(() =>
            //    {
            //        EndEdit();
            //        AssignDataSource();
            //        cell = dgTable[columnIndex, rowIndex];
            //        cell.Selected = true;
            //        dgTable.CurrentCell = dgTable[columnIndex, rowIndex];
            //        dgTable.BeginEdit(false);
            //    }));

            //SetRowCount();
        }

        private void tabellenZusammenfügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(ImportState.Merge);
        }

        private void verwaltungToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Administration form = new Administration(sourceTable?.HeadersOfDataTable() ?? new object[0], contextGlobal, procedures,workflows,cases,tolerances, sourceTable);
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
            SetMenuEnabled(sourceTable != null);
        }

        private void cSVToolStripMenuItem_Click(object sender, EventArgs e, DataTable dt = null, string path = null)
        {
            if (dgTable.DataSource != null)
            {
                DataTable table = dt ?? GetDataSource(true);
                SaveFileDialog saveFileDialog1 = new SaveFileDialog
                {
                    Filter = $"CSV Dateien ({ImportHelper.CsvExt})|{ImportHelper.CsvExt}|Alle Dateien (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };
                
                if (path != null || saveFileDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    path = path ?? saveFileDialog1.FileName;
                    StartLoadingBarCount(table.Rows.Count);
                    new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.IsBackground = true;
                            ExportHelper.ExportCsv(table, Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), FileEncoding, this, UpdateLoadingBar);
                            StopLoadingBar();
                            SaveFinished();
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex, this);
                        }
                    }).Start();
                }
            }
        }

        private void UpdateLoadingBar()
        {
            try
            {
                pgbLoading.Invoke(new MethodInvoker(() =>
                {
                    if (pgbLoading.Value < pgbLoading.Maximum)
                    {
                        pgbLoading.Value++;
                    }
                }));
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage($"{ex.ToString() + Environment.NewLine} Maximum:{pgbLoading.Maximum}; Minimum:{pgbLoading.Minimum} Value:{pgbLoading.Value}",this, false);
            }
        }

        private void StartLoadingBarCount(int length)
        {
            pgbLoading.Style = ProgressBarStyle.Continuous;
            pgbLoading.Maximum = length;
            pgbLoading.Minimum = 0;
            pgbLoading.Value = 0;
        }

        private void SaveFinished()
        {
            this.MessagesOK(MessageBoxIcon.Information, "Gespeichert");
        }

        private void postwurfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Formula formula = new Formula(FormulaState.Export, sourceTable.HeadersOfDataTable());
            if(formula.ShowDialog(this) == DialogResult.OK)
            {
                StartLoadingBarCount(Properties.Settings.Default.PVMSaveTwice ? sourceTable.Rows.Count * 2 : sourceTable.Rows.Count);
                new ProcPVMExport(formula.SelectedHeaders(), UpdateLoadingBar).DoWork(sourceTable,ref SortingOrder, null,null,null,FilePath,null,OrderType, this, out int[] newIndices);
                StopLoadingBar();
                SaveFinished();
            }
            formula.Dispose();
        }

        private void nachWertInSpalteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportCustom export = new ExportCustom(sourceTable.HeadersOfDataTable(), sourceTable);
            if(export.ShowDialog(this) == DialogResult.OK)
            {
                StartLoadingBar();
                ExportHelper.ExportTableWithColumnCondition(GetDataSource(true), export.Items, FilePath, StopLoadingBar, SaveFinished, FileEncoding, this, export.ContinuedNumberName);
            }
            export.Dispose();
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
            ExportCount export = new ExportCount(sourceTable.HeadersOfDataTable(), sourceTable);
            
            if(export.ShowDialog(this) == DialogResult.OK)
            {
                StartLoadingBar();
                Thread thread = new Thread(() =>
                {
                    DataTable newTable = ExportHelper.ExportCount(export.getSelectedValue(), export.CountChecked ? export.Count : 0, export.ShowFromTo, GetDataSource(), OrderType);
                    BeginInvoke(new MethodInvoker(() =>
                    {
                        new Form1(newTable).Show(this);
                    }));
                    StopLoadingBar();
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            export.Dispose();
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

                    SetSorting($"[{col.Name}] {(asc ? "DESC" : "ASC")}");
                    AssignDataSource();
                }
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
            AssignDataSource();
        }

        private void zeileLöschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteRows(ViewHelper.SelectedRowsOfDataGridView(dgTable));
        }

        private void DeleteRows(int[] rows)
        {
            List<CellMatrix> newHistoryEntry = new List<CellMatrix>();
            DataTable table = sourceTable;
            foreach (int row in rows.OrderByDescending(index => index))
            {
                DataRow oldRow = ((DataRowView)dgTable.Rows[row].DataBoundItem).Row;
                int tableIndex = table.Rows.IndexOf(oldRow);

                object[][] oldContent = new object[1][];
                oldContent[0] = oldRow.ItemArray.Clone() as object[];


                newHistoryEntry.Add(new CellMatrix(new History { State = State.DeleteRow, Row = oldContent, RowIndex = tableIndex }));

                table.Rows.RemoveAt(tableIndex);
            }
            AddDataSourceAddHistory(newHistoryEntry);
            SetRowCount();
        }

        private void zeileEinfügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable table = GetDataSource();
            DataRow row = table.NewRow();
            int tableRowIndex = string.IsNullOrEmpty(SortingOrder) ? GetDataTableRowIndexOfDataGridView(selectedRow) : table.Rows.Count;
            table.Rows.InsertAt(row, tableRowIndex);
            AssignDataSource(table);

            AddDataSourceAddRow(tableRowIndex);
        }

        private void tabelleHinzufügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(ImportState.Append);
        }

        private void MergeTables(DataTable table, string filename)
        {
            DataTable originalTable = GetDataSource();
            int ColumnIndexNew = originalTable.Columns.Count;
            int RowIndexNew = originalTable.Rows.Count;
            originalTable.ConcatTable(table, Path.GetFileName(FilePath), filename);

            AssignDataSource(originalTable);
            AddDataSourceAddColumnAndRows(ColumnIndexNew, RowIndexNew);
        }

        private int GetDataTableRowIndexOfDataGridView(int rowIndex)
        {
            DataRow oldRow = ((DataRowView)dgTable.Rows[rowIndex].DataBoundItem).Row;
            return sourceTable.Rows.IndexOf(oldRow);
        }

        private void überschriftenEinlesenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(ImportState.Header);
        }

        private void dgTable_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex > -1)
            {
                string header = e.Value.ToString();
                var count = dictKeys.IndexOf(header);
                SortOrder order = SortOrder.None;
                if (count >= 0)
                {
                    count++;
                    e.Graphics.FillRectangle(new SolidBrush(e.CellStyle.BackColor), e.CellBounds);
                    e.Paint(e.ClipBounds, (DataGridViewPaintParts.All & ~DataGridViewPaintParts.Background));
                    
                    e.Graphics.DrawString($"{count}", dgTable.DefaultCellStyle.Font, new SolidBrush(Color.Black), e.CellBounds.X + e.CellBounds.Width-20, e.CellBounds.Y + 5, StringFormat.GenericDefault);
                    e.Handled = true;
                    order = dictSorting[header];
                }
                dgTable.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = order;
            }

        }

        private void großKleinschreibungToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpLowCaseForm form = new UpLowCaseForm(sourceTable.HeadersOfDataTable());
            if (form.ShowDialog(this) == DialogResult.OK) {
                StartSingleWorkflow(form.Procedure);
            }
        }

        private void rundenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RoundForm form = new RoundForm(sourceTable.HeadersOfDataTable());
            if(form.ShowDialog(this) == DialogResult.OK)
            {
                StartSingleWorkflow(form.Procedure);
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
                cSVToolStripMenuItem_Click(null, null, null, FileName);
            }
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateHelper.CheckUpdate(false, pgbLoading, this);
        }

        private void zeilenZusammenfügenToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            DataTable table = GetDataSource();
            
            MergeColumns form = new MergeColumns(table.HeadersOfDataTable());
            if(form.ShowDialog(this) == DialogResult.OK)
            {
                string identifier = form.Identifier;
                int identifierIndex = form.IdentifierIndex;
                bool separator = form.Separator;
                List<PlusListboxItem> additionalColumns = form.AdditionalColumns;

                new Thread(() =>
                {
                    try
                    {
                        string tempSortName = table.MergeRows(identifier, additionalColumns, separator, pgbLoading, this);

                        dgTable.Invoke(new MethodInvoker(() =>
                        {
                            List<CellMatrix> oldValues = table.ChangesOfDataTable();
                            historyHelper.AddHistory(new History { State = State.ValueChange, Table = oldValues }, GetSorting());
                            table.Columns.Remove(tempSortName);
                            AssignDataSource(table);
                        }));
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

        private void zeichenAuffüllenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PaddingForm form = new PaddingForm(sourceTable.HeadersOfDataTable());
            if(form.ShowDialog(this) == DialogResult.OK)
            {
                StartSingleWorkflow(form.Proc);
            }
        }

        private void textErsetzenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InsertText form = new InsertText("Spalte mit Text befüllen", "Bitte Text eingeben");
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                DataTable newTable = GetDataSource();
                string newText = form.NewText;

                foreach (DataRow row in newTable.Rows)
                {
                    row[selectedColumn] = newText;
                }

                AddDataSourceValueChange(newTable);
            }
        }

        private void nummerierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NumerationForm form = new NumerationForm(sourceTable.HeadersOfDataTable());
            if(form.ShowDialog(this) == DialogResult.OK)
            {
                StartSingleWorkflow(form.Procedure);
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
            SubstringForm form = new SubstringForm(sourceTable.HeadersOfDataTable());
            if(form.ShowDialog(this) == DialogResult.OK)
            {
                StartSingleWorkflow(form.Procedure);
            }
        }

        private void textErsetzenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ReplaceWholeForm form = new ReplaceWholeForm(sourceTable.HeadersOfDataTable(), contextGlobal);
            if(form.ShowDialog(this) == DialogResult.OK)
            {
                ProcReplaceWhole proc = new ProcReplaceWhole(form.Table);
                StartSingleWorkflow(proc);
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
                AssignDataSource(null,true);
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
            CompareForm form = new CompareForm(sourceTable.HeadersOfDataTable(), sourceTable);
            if(form.ShowDialog(this) == DialogResult.OK)
            {
                StartSingleWorkflow(form.Procedure);
            }
            form.Dispose();
        }

        private void PrüfzifferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable table = GetDataSource();
            SelectHeader headerForm = new SelectHeader(table.HeadersOfDataTable());
            if (headerForm.ShowDialog(this) == DialogResult.OK)
            {
                string column = headerForm.Column;
                bool continueLoop = false;
                StartLoadingBarCount(table.Rows.Count);
                new Thread(() =>
                {
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        DataRow row = table.Rows[i];
                        string value = row[column].ToString();
                        int checkSum = ChecksumEAN9(value);
                        if (checkSum == -1 && !continueLoop)
                        {
                            DialogResult result = this.MessagesYesNo(MessageBoxIcon.Warning, $"Ungültige Zahl in Zeile {i + 1}. Trotzdem fortfahren?");
                            if (result == DialogResult.No)
                            {
                                break;
                            }
                            else
                            {
                                continueLoop = true;
                            }
                        }
                        else
                        {
                            row[column] = value + checkSum;
                        }
                        UpdateLoadingBar();
                    }
                    StopLoadingBar();
                    dgTable.Invoke(new MethodInvoker(() =>
                    {
                        AddDataSourceValueChange(table);
                    }));
                }).Start();
            }
            headerForm.Dispose();
        }

        private int ChecksumEAN9(string data)
        {
            int result = -1;
            if (int.TryParse(data, out int _))
            {
                int sum1 = 0;
                for(int i = data.Length-2; i >= 0; i -= 2)
                {
                    sum1 += (int) char.GetNumericValue(data[i]);
                }

                int sum2 = 0;
                for (int i = data.Length - 1; i >= 0; i -= 2)
                {
                    sum2 += (int)char.GetNumericValue(data[i]);
                }

                int checksum_digit = 10 - ((sum1 + (sum2*3)) % 10);

                result = checksum_digit == 10 ? 0 : checksum_digit;
            }
            return result;
        }

        private void längsteZeileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MaxRowLengthForm form = new MaxRowLengthForm(sourceTable.HeadersOfDataTable());

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                DataTable table = GetDataSource();
                string shortcut = form.Shortcut;
                string newColumn = form.NewColumn;
                int minLength = form.MinLength;
                string column = form.Column;
                StartLoadingBar();
                Thread thread = new Thread(() =>
                {
                    int maxIndex = 0;
                    int maxLength = 0;
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        int max = table.Rows[i].ItemArray.Sum(item => item.ToString().Length);
                        if (max > maxLength)
                        {
                            maxLength = max;
                            maxIndex = i;
                        }
                    }

                    if (shortcut != string.Empty && newColumn != string.Empty)
                    {
                        newColumn = table.TryAddColumn(newColumn);
                        if (minLength != -1)
                        {
                            foreach(DataRow row in table.AsEnumerable())
                            {
                                if(row[column].ToString().Length >= minLength)
                                {
                                    row[newColumn] = shortcut;
                                }
                            }
                        }
                        else
                        {
                            table.Rows[maxIndex][newColumn] = shortcut;
                        }
                        dgTable.Invoke(new MethodInvoker(()=> {
                            AddDataSourceValueChange(table);
                        }));
                    }
                    if (minLength != -1)
                    {
                        SelectDataGridViewRow(maxIndex);
                    }
                    StopLoadingBar();
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            form.Dispose();
        }

        private void SelectDataGridViewRow(int index)
        {
            dgTable.Invoke(new MethodInvoker(() =>
            {
                dgTable.ClearSelection();
                dgTable.Rows[index].Selected = true;
                dgTable.FirstDisplayedScrollingRowIndex = index;
            }));
        }

        private void suchenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SearchForm form = new SearchForm(sourceTable.HeadersOfDataTableAsString());
            if(form.ShowDialog(this) == DialogResult.OK)
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
                StartSingleWorkflow(proc, delegate { SearchAndSelect(proc.SearchText, proc.Header, form.CheckTotal, sourceTable.GetSortedTable(SortingOrder,OrderType)); });
            }
            form.Dispose();
        }

        private void SearchAndSelect(string searchText, string header, bool totalSearch, IEnumerable<DataRow> table)
        {
            int index = 0;
            Func<string, string, bool> searchMethod;
            if (totalSearch)
            {
                searchMethod = searchTotal;
            }
            else
            {
                searchMethod = searchPartial;
            }

            foreach(DataRow row in table)
            {
                if(searchMethod.Invoke(row[header].ToString(), searchText))
                {
                    SelectDataGridViewRow(index);
                    break;
                }
                index++;
            }

            bool searchTotal(string value, string search)
            {
                return value == search;
            }

            bool searchPartial(string value, string search)
            {
                return value.Contains(search);
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            importToolStripMenuItem_Click(ImportState.None,(string[])e.Data.GetData(DataFormats.FileDrop));
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
            SplitFormMain form = new SplitFormMain(sourceTable.HeadersOfDataTable());
            if(form.ShowDialog(this) == DialogResult.OK)
            {
                StartSingleWorkflow(form.Procedure);
            }
        }

        private void dgTable_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            e.Column.FillWeight = 10;
        }

        private void NumPage_ValueChanged(object sender, EventArgs e)
        {
            int page = (int)NumPage.Value;
            if(page > 0 && page <= MaxPages)
            {
                Page = page;
            }
        }

        private void zeilenLöschenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DeleteRows form = new DeleteRows(sourceTable.Rows.Count, sourceTable.HeadersOfDataTableAsString());
            form.ShowDialog();
            if (form.DialogResult == DialogResult.OK)
            {
                if (form.Range == null)
                {
                    DeleteRowsMatchingText(form.ColumnText, form.Column, form.EqualsText);
                }
                else
                {
                    DeleteRows(form.Range);
                }
            }
        }

        private void DeleteRowsMatchingText(string value, string column, bool equals)
        {
            pgbLoading.StartLoadingBar(sourceTable.Rows.Count, this);
            Thread thread = new Thread(() =>
            {
                List<CellMatrix> newHistoryEntry = new List<CellMatrix>();
                for (int i = sourceTable.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow row = sourceTable.Rows[i];
                    if (equals ? row[column].ToString() == value : row[column].ToString().Contains(value))
                    {
                        object[][] oldContent = new object[1][];
                        oldContent[0] = row.ItemArray.Clone() as object[];

                        newHistoryEntry.Add(new CellMatrix(new History { State = State.DeleteRow, Row = oldContent, RowIndex = i }));

                        row.Delete();
                    }
                    pgbLoading.UpdateLoadingBar(this);
                }
                StopLoadingBar();

                AddDataSourceAddHistory(newHistoryEntry);
                sourceTable.AcceptChanges();
                Invoke(new MethodInvoker(() =>
                {
                    ResetPage();
                    AssignDataSource();
                }));
                SetRowCount();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void sortierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortForm form = new SortForm(sourceTable.HeadersOfDataTable(), SortingOrder, OrderType);
            if(form.ShowDialog(this) == DialogResult.OK)
            {
                OrderType = form.OrderType;
                SetSorting(form.SortString);
                AssignDataSource();
            }
            form.Dispose();
        }
    }
}
