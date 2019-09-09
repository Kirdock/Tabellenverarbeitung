using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
using DataTableConverter.Extensions;
using DataTableConverter.View;
using DataTableConverter.View.WorkProcViews;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private HistoryHelper historyHelper;
        private string tableValueBefore;
        private DataTable sourceTable;
        private string SortingOrder;
        private Dictionary<string, SortOrder> dictSorting;
        private int rowBefore; //Wenn eine Zelle im DataGridView geändert wird, kann ich mit dgTable[col, row] den geänderten Wert nicht holen, da die DataGridView zu diesem Zeitpunkt wieder sortiert wurde
        private List<string> dictKeys;
        private string FilePath = string.Empty;
        private OrderType OrderType = OrderType.Windows;
        private Dictionary<string, int> ColumnWidths;

        internal Form1(DataTable table = null)
        {
            dictSorting = new Dictionary<string, SortOrder>();
            ColumnWidths = new Dictionary<string, int>();
            dictKeys = new List<string>();
            InitializeComponent();
            SetSize();
            ExportHelper.CheckRequired();
            LoadProcedures();
            LoadWorkflows();
            LoadTolerances();
            LoadCases();
            SetMenuEnabled(false);
            historyHelper = new HistoryHelper();
            öffnenToolStripMenuItem1.Click += (sender, e) => importToolStripMenuItem_Click(sender, e);
            trimToolStripMenuItem.Click += (sender, e) => trimToolStripMenuItem_Click(sender, e);
            cSVToolStripMenuItem.Click += (sender, e) => cSVToolStripMenuItem_Click(sender, e);
            dBASEToolStripMenuItem1.Click += (sender, e) => dBASEToolStripMenuItem_Click(sender, e);
            excelToolStripMenuItem1.Click += (sender, e) => excelToolStripMenuItem_Click(sender, e);
            if (table != null)
            {
                AddDataSourceNewTable(table);
            }
            ViewHelper.SetDataGridViewStyle(dgTable);
            UpdateHelper.CheckUpdate(true, pgbLoading);
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
            Size = Properties.Settings.Default.Form1Size;
        }

        private void SetSorting(string order)
        {
            SortingOrder = order;
            dictSorting = ViewHelper.GenerateSortingList(GetSorting());
            dictKeys = dictSorting.Keys.ToList();
        }


        private void AssignDataSource(DataTable table = null, bool adjustColumnWidth = false)
        {
            if (!adjustColumnWidth)
            {
                SaveWidthOfDataGridViewColumns();
            }
            else
            {
                ColumnWidths.Clear();
            }
            int scrollBarHorizontal = dgTable.HorizontalScrollingOffset;
            OrderType orderType = OrderType;
            (dgTable.DataSource as DataView)?.Dispose(); //in hope to remove all remaining lazy loading
            dgTable.DataSource = null; //else some columns (added through History) will be shown at index 0 instead of the right one
            sourceTable = table ?? sourceTable;
            int rowCount = sourceTable.Rows.Count;
            dgTable.DataSource = sourceTable.GetSortedView(SortingOrder, OrderType, delegate { AddDataSourceAddRow(rowCount, orderType); });
            
            RestoreDataGridSortMode();
            SetWidth(adjustColumnWidth);

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
                EndEdit();
                int rowCountBefore = table.Rows.Count;
                if (withSort)
                {
                    table = GetDataView().ToTable();
                }
                else
                {
                    table = sourceTable.Copy();
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

        private DataView GetDataView()
        {
            EndEdit();
            return (DataView)dgTable.DataSource;
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
        }

        private void SetWidth(bool adjustColumnWidth)
        {
            if (Properties.Settings.Default.FullWidthImport)
            {
                if (adjustColumnWidth)
                {
                    foreach(DataGridViewColumn col in dgTable.Columns)
                    {
                        SetOptimalColumnWidth(col);
                    }
                }
                else
                {
                    foreach(string col in ColumnWidths.Keys)
                    {
                        if (dgTable.Columns.Contains(col))
                        {
                            dgTable.Columns[col].Width = ColumnWidths[col];
                        }
                        else
                        {
                            if (dgTable.Columns.Contains(col))
                            {
                                SetOptimalColumnWidth(dgTable.Columns[col]);
                            }
                        }
                    }
                }
            }
        }

        private void SetOptimalColumnWidth(DataGridViewColumn col)
        {
            string result = sourceTable.AsEnumerable().Select(row => row[col.Name].ToString()).Concat(new string[] { col.Name }).Aggregate(string.Empty, (seed, f) => f.Length > seed.Length ? f : seed);
            col.Width = TextRenderer.MeasureText(result, dgTable.DefaultCellStyle.Font).Width + 5;

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

        void AddDataSourceAddRow(int rowIndex, OrderType orderType = OrderType.Windows)
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
            
            procedures = proc ?? ImportHelper.LoadProcedures();
            for (int i = 0; i < procedures.Count; i++)
            {
                int index = i;
                ToolStripMenuItem item = new ToolStripMenuItem(procedures[i].Name);
                item.Click += (sender, e) => procedure_Click(sender, e, procedures[index]);
                ersetzenToolStripMenuItem.DropDownItems.Add(item);
            }   
        }

        private void LoadWorkflows(List<Work> work = null)
        {
            arbeitsablaufToolStripMenuItem.DropDownItems.Clear();

            workflows = work ?? ImportHelper.LoadWorkflows();

            List<Work> workflowsCopy = GetCopyOfWorkflows(workflows);

            for (int i = 0; i < workflowsCopy.Count; i++)
            {
                int index = i;
                ToolStripMenuItem item = new ToolStripMenuItem(workflowsCopy[i].Name);
                item.Click += (sender, e) => workflow_Click(sender, e, workflowsCopy[index]);
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
                item.Click += (sender, e) => case_Click(sender, e, cases[index], null, GetDataSource());
                duplikateToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void case_Click(object sender, EventArgs e, Case cas, string[] caseColumnsWorkflow, DataTable originalTable)
        {
            ProcDuplicate procDuplicate = new ProcDuplicate(0, cas.Id, cas.Name)
            {
                DuplicateColumns = cas.Columns.Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToArray()
            };
            StartSingleWorkflow(procDuplicate);
        }

        private void workflow_Click(object sender, EventArgs e, Work workflow)
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
                ReplaceThroughTemp(workflow.Procedures);
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
                SelectDuplicateColumns form = new SelectDuplicateColumns(columns.ToArray(), table.HeadersOfDataTable(), false);
                if (form.ShowDialog() == DialogResult.OK) {
                    string[] from = form.Table.Rows.Cast<DataRow>().Select(row => row.ItemArray[0].ToString()).ToArray();
                    string[] to = form.Table.Rows.Cast<DataRow>().Select(row => row.ItemArray[1].ToString()).ToArray();

                    foreach (NotFoundHeaders nf in notFound)
                    {
                        string[] wpHeaders = nf.Wp.GetHeaders();
                        for (int y = 0; y < wpHeaders.Length; y++)
                        {
                            for (int i = 0; i < from.Length; i++)
                            {
                                if(to[i] == SelectDuplicateColumns.IgnoreColumn)
                                {
                                    nf.Wp.removeHeader(from[i]);
                                }
                                else if (wpHeaders[y] == from[i] && nf.Headers.Contains(from[i])) //Kann sein, dass eine Spalte hinzugefügt wird und sie bei manchen Valid und bei manchen inValid ist, je nachdem wann sie ausgeführt werden
                                {
                                    nf.Wp.renameHeaders(from[i], to[i]);
                                }
                            }
                        }
                    }
                    ReplaceThroughTemp(workflow.Procedures);
                }
            }
        }

        private void ReplaceThroughTemp(List<WorkProc> temp)
        {
            DataTable table = GetDataSource();
            StartLoadingBar();
            Thread thread = new Thread(() =>
            {
                try
                {
                    table.AcceptChanges();
                    foreach (WorkProc t in temp)
                    {
                        if (t.ReplacesTable)
                        {
                            historyHelper.AddHistory(new History { State = State.ValueChange, Table = table.ChangesOfDataTable() }, GetSorting());
                        }
                        ReplaceProcedure(table, null, t, out int[] newIndices);

                        if (t.ReplacesTable)
                        {
                            historyHelper.AddHistory(new History { State = State.OrderIndexChange, NewOrderIndices = newIndices }, GetSorting());
                        }
                    }

                    dgTable.Invoke(new MethodInvoker(() =>
                    {
                        AddDataSourceValueChange(table);
                    }));
                }
                catch (Exception ex)
                {
                    ErrorHelper.LogMessage(ex);
                }
                StopLoadingBar();
                MessageHandler.MessagesOK(MessageBoxIcon.Information, "Arbeitsablauf ausgeführt!");
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private int GetProcedureThroughId(int id)
        {
            return procedures.FindIndex(p => p.Id == id);
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e, ImportState state = ImportState.None)
        {
            OpenFileDialog dialog = ImportHelper.GetOpenFileDialog(state == ImportState.None || state == ImportState.Append);
            DataTable oldTable = GetDataSource();
            string mergePath = string.Empty;
            bool validMerge = state == ImportState.Merge && ProcAddTableColumns.CheckFile(FilePath,ref mergePath);
            if (validMerge || dialog.ShowDialog() == DialogResult.OK)
            {
                string[] filenames = validMerge ? new string[1] {mergePath} : dialog.FileNames;

                Thread thread = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    bool fileNameSet = state != ImportState.None;
                    StartLoadingBar();
                    bool multipleFiles = filenames.Length > 1;
                    Dictionary<string, ImportSettings> fileImportSettings = new Dictionary<string, ImportSettings>();
                    DataTable newTable = null;
                    string fileNameBefore = Path.GetFileName(filenames[0]);
                    foreach (string file in filenames)
                    {
                        try
                        {

                            string filename = Path.GetFileName(file);
                            DataTable table = ImportHelper.ImportFile(file, this, multipleFiles, fileImportSettings, contextGlobal);
                            if (table != null)
                            {
                                if (newTable != null)
                                {
                                    newTable.ConcatTable(table, fileNameBefore, filename);
                                }
                                else
                                {
                                    newTable = table;
                                }
                                fileNameBefore = filename;
                                if (!fileNameSet)
                                {
                                    fileNameSet = true;
                                    SetFileName(file);
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex);
                        }
                    }
                    FinishImport(newTable, state, oldTable, Path.GetFileName(filenames[0]));
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        private void SetFileName(string path)
        {
            lblFilename.GetCurrentParent().BeginInvoke(new MethodInvoker(() =>
            {
                lblFilename.Text = Path.GetFileName(path);
            }));
            
            FilePath = path;
        }

        private void FinishImport(DataTable table, ImportState state, DataTable oldTable, string filename)
        {
            StopLoadingBar();
            if(table != null) {
                switch (state)
                {
                    case ImportState.Merge:
                        StartMerge(table, oldTable, filename);
                        break;

                    case ImportState.Append:
                        dgTable.BeginInvoke(new MethodInvoker(() => { MergeTables(table, filename); }));
                        break;

                    case ImportState.Header:
                        object[] headers = oldTable.HeadersOfDataTable();
                        oldTable.OverrideHeaders(table);
                        dgTable.BeginInvoke(new MethodInvoker(() => { AssignDataSource(oldTable); }));
                        AddDataSourceHeadersChange(headers);
                        break;

                    default:
                        dgTable.BeginInvoke(new MethodInvoker(() => { AddDataSourceNewTable(table); }));
                        lblRows.GetCurrentParent().BeginInvoke(new MethodInvoker(() => { SetRowCount(table.Rows.Count); }));
                        break;
                }
            }
        }

        private void StartMerge(DataTable importTable, DataTable sourceTable, string filename)
        {
            string[] importColumns = new string[0];
            int sourceMergeIndex = -1;
            int importMergeIndex = -1;
            DialogResult result = DialogResult.No;

            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.PVMIdentifier))
            {
                result = ShowMergeForm(ref importColumns, ref sourceMergeIndex, ref importMergeIndex, sourceTable, importTable, filename);
            }
            else
            {
                if ((sourceMergeIndex = sourceTable.Columns.IndexOf(Properties.Settings.Default.PVMIdentifier)) > -1)
                {
                    if ((importMergeIndex = importTable.Columns.IndexOf(Properties.Settings.Default.PVMIdentifier)) > -1)
                    {
                        importColumns = importTable.HeadersOfDataTableAsString();
                        result = DialogResult.Yes;
                        if (sourceTable.Rows.Count != importTable.Rows.Count)
                        {
                            result = MessageHandler.MessagesYesNo(MessageBoxIcon.Warning, $"Die Zeilenanzahl der beiden Tabellen stimmt nicht überein ({sourceTable.Rows.Count} zu {importTable.Rows.Count})!\nTrotzdem fortfahren?");
                        }
                    }
                    else
                    {
                        MessageHandler.MessagesOK(MessageBoxIcon.Warning, $"Die zu importierende Tabelle \"{filename}\" hat keine Spalte mit der Bezeichnung {Properties.Settings.Default.PVMIdentifier}");
                        result = ShowMergeForm(ref importColumns, ref sourceMergeIndex, ref importMergeIndex, sourceTable, importTable, filename);
                    }
                }
                else
                {
                    MessageHandler.MessagesOK(MessageBoxIcon.Warning, $"Die Haupttabelle hat keine Spalte mit der Bezeichnung {Properties.Settings.Default.PVMIdentifier}");
                    result = ShowMergeForm(ref importColumns, ref sourceMergeIndex, ref importMergeIndex, sourceTable, importTable, filename);
                }
            }
            
            if (result == DialogResult.Yes)
            {
                Thread thread = new Thread(() =>
                {
                    try {
                        sourceTable.AddColumnsOfDataTable(importTable, importColumns, sourceMergeIndex, importMergeIndex, out int[] newIndices, pgbLoading);

                        if (Properties.Settings.Default.SplitPVM)
                        {
                            string invalidColumnName = Properties.Settings.Default.InvalidColumnName;
                            if (!sourceTable.Columns.Contains(invalidColumnName))
                            {
                                SelectDuplicateColumns f = new SelectDuplicateColumns(new string[] { invalidColumnName }, sourceTable.HeadersOfDataTable(), true);
                                if (f.ShowDialog() == DialogResult.OK)
                                {
                                    invalidColumnName = f.Table.AsEnumerable().First()[1].ToString();
                                    sourceTable.SplitDataTable(FilePath, invalidColumnName);
                                }
                            }
                            else
                            {
                                sourceTable.SplitDataTable(FilePath, invalidColumnName);
                            }
                        }

                        dgTable.Invoke(new MethodInvoker(() => { AddDataSourceValueChange(sourceTable); }));
                        historyHelper.AddHistory(new History { State = State.OrderIndexChange, NewOrderIndices = newIndices }, GetSorting());
                        pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Value = pgbLoading.Maximum = 0; }));
                    }
                    catch (Exception ex)
                    {
                        ErrorHelper.LogMessage(ex);
                    }
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        private DialogResult ShowMergeForm(ref string[] importColumns, ref int sourceMergeIndex, ref int importMergeIndex, DataTable sourceTable, DataTable importTable, string filename)
        {
            MergeTable form = new MergeTable(sourceTable.HeadersOfDataTable(), importTable.HeadersOfDataTable(), filename, sourceTable.Rows.Count, importTable.Rows.Count);
            bool result;
            if (result = (form.ShowDialog() == DialogResult.OK))
            {
                importColumns = form.getSelectedColumns();
                sourceMergeIndex = form.getSelectedOriginal();
                importMergeIndex = form.getSelectedMerge();
            }
            form.Dispose();
            return result ? DialogResult.Yes : DialogResult.No;
        }

        private void SetRowCount()
        {
            SetRowCount(dgTable.Rows.Count - 1);
        }

        private void SetRowCount(int count)
        {
            lblRows.Text = count.ToString();
        }

        private void trimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgTable.DataSource != null)
            {
                TrimForm form = new TrimForm();
                if(form.ShowDialog() == DialogResult.OK)
                {
                    StartLoadingBar();
                    DataTable table = GetDataSource();
                    new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.IsBackground = true;
                            string order = GetSorting();
                            form.Proc.doWork(table, ref order, null, null, null, FilePath, contextGlobal, OrderType, this, out int[] newIndices);
                            dgTable.Invoke(new MethodInvoker(() =>
                            {
                                AddDataSourceValueChange(table);
                            }));
                            
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex);
                        }
                        StopLoadingBar();
                    }).Start();
                }
            }
        }

        private void procedure_Click(object sender, EventArgs e, Proc procedure)
        {
            Formula formula = new Formula(FormulaState.Procedure, sourceTable.HeadersOfDataTable());
            if (formula.ShowDialog() == DialogResult.OK)
            {
                string[] columns = formula.SelectedHeaders();

                if (columns.Length > 0)
                {
                    ProcUser user = new ProcUser(columns, formula.HeaderName(), formula.OldColumn);
                    DataTable newTable = GetDataSource();

                    new Thread(() =>
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
                            ErrorHelper.LogMessage(ex);
                        }
                    }).Start();
                }
            }
        }

        private void ReplaceProcedure(DataTable table, Proc procedure, WorkProc wp, out int[] newOrderIndices)
        {
            string newOrder = GetSorting();
            wp.doWork(table, ref newOrder, GetCaseThroughId(wp.ProcedureId), tolerances, procedure ?? GetProcedure(wp.ProcedureId), FilePath, contextGlobal, OrderType, this, out int[] newIndices);
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

                if (path != null || saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    path = path ?? saveFileDialog1.FileName;
                    StartLoadingBar();
                    DataTable table = GetDataSource(true);
                    new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.IsBackground = true;
                            ExportHelper.ExportExcel(table, Path.GetDirectoryName(path),path);
                            StopLoadingBar();
                            SaveFinished();
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex);
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
                
                selectedRow = dgTable.HitTest(e.X, e.Y).RowIndex;
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
            if (formula.ShowDialog() == DialogResult.OK)
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

                if (path != null || saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    path = path ?? saveFileDialog1.FileName;
                    DataTable table = GetDataSource(true);
                    StartLoadingBarCount(table.Rows.Count);
                    new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.IsBackground = true;
                            bool saved = ExportHelper.ExportDbase(Path.GetFileNameWithoutExtension(path), table, Path.GetDirectoryName(path), UpdateLoadingBar);
                            StopLoadingBar();
                            if (saved)
                            {
                                SaveFinished();
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex);
                        }
                    }).Start();
                }
            }
        }

        private void dgTable_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            ViewHelper.AddNumerationToDataGridView(sender, e, Font);
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
                    ErrorHelper.LogMessage(ex);
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
                    ErrorHelper.LogMessage(ex);
                }
            }).Start();
        }

        private void dgTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int width = TextRenderer.MeasureText(dgTable[e.ColumnIndex, e.RowIndex].Value.ToString(), dgTable.DefaultCellStyle.Font).Width + 5;
            DataGridViewColumn col = dgTable.Columns[e.ColumnIndex];
            if (width > col.Width)
            {
                ColumnWidths[col.Name] = col.Width = width;
            }
            AddDataSourceCellChanged(tableValueBefore, e.ColumnIndex, rowBefore);
        }

        private void dgTable_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            tableValueBefore = dgTable[e.ColumnIndex, e.RowIndex].Value.ToString();
            rowBefore = e.RowIndex >= 0 && e.RowIndex < sourceTable.Rows.Count ? GetDataTableRowIndexOfDataGridView(e.RowIndex) : 0;
        }

        private void dgTable_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            SetRowCount();
        }

        private void tabellenZusammenfügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(null, null, ImportState.Merge);
        }

        private void verwaltungToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Administration form = new Administration(sourceTable?.HeadersOfDataTable() ?? new object[0], contextGlobal, procedures,workflows,cases,tolerances);
            form.FormClosed += new FormClosedEventHandler(administrationFormClosed);
            form.Show();
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
                
                if (path != null || saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    path = path ?? saveFileDialog1.FileName;
                    StartLoadingBarCount(table.Rows.Count);
                    new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.IsBackground = true;
                            ExportHelper.ExportCsv(table, Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), UpdateLoadingBar);
                            StopLoadingBar();
                            SaveFinished();
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex);
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
                    pgbLoading.Value++;
                }));
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage($"{ex.ToString() + Environment.NewLine} Maximum:{pgbLoading.Maximum}; Minimum:{pgbLoading.Minimum} Value:{pgbLoading.Value}",false);
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
            MessageHandler.MessagesOK(MessageBoxIcon.Information, "Gespeichert");
        }

        private void postwurfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Formula formula = new Formula(FormulaState.Export, sourceTable.HeadersOfDataTable());
            if(formula.ShowDialog() == DialogResult.OK)
            {
                StartLoadingBarCount(Properties.Settings.Default.PVMSaveTwice ? sourceTable.Rows.Count * 2 : sourceTable.Rows.Count);
                new ProcPVMExport(formula.SelectedHeaders(), UpdateLoadingBar).doWork(sourceTable,ref SortingOrder, null,null,null,FilePath,null,OrderType, this, out int[] newIndices);
                StopLoadingBar();
                SaveFinished();
            }
        }

        private void nachWertInSpalteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportCustom export = new ExportCustom(sourceTable.HeadersOfDataTable(), sourceTable);
            if(export.ShowDialog() == DialogResult.OK)
            {
                StartLoadingBar();
                ExportHelper.ExportTableWithColumnCondition(GetDataSource(true), export.Items, FilePath, StopLoadingBar, SaveFinished);
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
            ExportCount export = new ExportCount(sourceTable.HeadersOfDataTable());
            
            if(export.ShowDialog() == DialogResult.OK)
            {
                StartLoadingBar();
                Thread thread = new Thread(() =>
                {
                    DataTable newTable = ExportHelper.ExportCount(export.getSelectedValue(), export.CountChecked ? export.Count : 0, export.ShowFromTo, GetDataSource(), OrderType);
                    BeginInvoke(new MethodInvoker(() =>
                    {
                        new Form1(newTable).Show();
                    }));
                    StopLoadingBar();
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }


        private void dgTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != 0 && (ModifierKeys & Keys.Alt) != 0)
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

        private void ResetSort()
        {
            SetSorting(string.Empty);
            AssignDataSource();
        }

        private void zeileLöschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int[] rows = ViewHelper.SelectedRowsOfDataGridView(dgTable);
            List<CellMatrix> newHistoryEntry = new List<CellMatrix>();
            DataTable table = ((DataView)dgTable.DataSource).Table;
            foreach (int row in rows)
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
            importToolStripMenuItem_Click(null, null, ImportState.Append);
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
            importToolStripMenuItem_Click(null, null, ImportState.Header);
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
            if (form.ShowDialog() == DialogResult.OK) {
                StartSingleWorkflow(form.Procedure);
            }
        }

        private void rundenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RoundForm form = new RoundForm(sourceTable.HeadersOfDataTable());
            if(form.ShowDialog() == DialogResult.OK)
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
            UpdateHelper.CheckUpdate(false, pgbLoading);
        }

        private void zeilenZusammenfügenToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            DataTable table = GetDataSource();
            
            MergeColumns form = new MergeColumns(table.HeadersOfDataTable());
            if(form.ShowDialog() == DialogResult.OK)
            {
                string identifier = form.Identifier;
                int identifierIndex = form.IdentifierIndex;
                List<PlusListboxItem> additionalColumns = form.AdditionalColumns;

                new Thread(() =>
                {
                    try
                    {
                        StartLoadingBar();
                        string tempSortName = table.MergeRows(identifier, identifierIndex, additionalColumns, OrderType, SetStatusLabel);

                        dgTable.Invoke(new MethodInvoker(() =>
                        {
                            DataTable originalSortTable = table.Copy().GetSortedView($"[{tempSortName}] asc", OrderType).ToTable();

                            List<CellMatrix> oldValues = table.ChangesOfDataTable();
                            historyHelper.AddHistory(new History { State = State.ValueChange, Table = oldValues }, GetSorting());
                            originalSortTable.Columns.Remove(tempSortName);

                            AssignDataSource(originalSortTable);
                        }));
                    }
                    catch(Exception ex)
                    {
                        ErrorHelper.LogMessage(ex);
                    }
                    finally
                    {
                        SetStatusLabel(string.Empty);
                        StopLoadingBar();
                    }

                }).Start();
            }
        }

        private void SetStatusLabel(string text)
        {
            StatusLabel.GetCurrentParent().Invoke(new MethodInvoker(() =>
            {
                StatusLabel.Text = text;
            }));
        }

        private void zeichenAuffüllenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PaddingForm form = new PaddingForm(sourceTable.HeadersOfDataTable());
            if(form.ShowDialog() == DialogResult.OK)
            {
                StartSingleWorkflow(form.Proc);
            }
        }

        private void textErsetzenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newText = Microsoft.VisualBasic.Interaction.InputBox("Bitte Text eingeben", "Spalte mit Text befüllen", string.Empty);
            if (!string.IsNullOrWhiteSpace(newText))
            {
                DataTable newTable = GetDataSource();

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
            if(form.ShowDialog() == DialogResult.OK)
            {
                StartSingleWorkflow(form.Procedure);
            }
        }

        private void StartSingleWorkflow(WorkProc proc)
        {
            workflow_Click(null, null, new Work(string.Empty, new List<WorkProc> { GetCopyOfWorkProc(proc) }, 0));
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
            if(form.ShowDialog() == DialogResult.OK)
            {
                StartSingleWorkflow(form.Procedure);
            }
        }

        private void textErsetzenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ReplaceWholeForm form = new ReplaceWholeForm(sourceTable.HeadersOfDataTable(), contextGlobal);
            if(form.ShowDialog() == DialogResult.OK)
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
            form.Show();
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
        }

        private void SaveSize()
        {
            Properties.Settings.Default.Form1Size = Size;
            Properties.Settings.Default.Save();
        }

        private void spaltenVergleichenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CompareForm form = new CompareForm(sourceTable.HeadersOfDataTable());
            if(form.ShowDialog() == DialogResult.OK)
            {
                StartSingleWorkflow(form.Procedure);
            }
        }

        private void sortierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortForm form = new SortForm(sourceTable.HeadersOfDataTable(), SortingOrder, OrderType);
            if(form.ShowDialog() == DialogResult.OK)
            {
                OrderType = form.OrderType;
                SetSorting(form.SortString);
                AssignDataSource();
            }
        }
    }
}
