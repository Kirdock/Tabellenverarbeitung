using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace DataTableConverter
{
    public partial class Form1 : Form
    {
        private int selectedRow = 0, selectedColumn = 0;
        private readonly string dbfExt = "*.dbf";
        private readonly string csvExt = "*.csv";
        private readonly string excelExt = "*.xlsx;*.xlsm;*.xlsb;*.xltx;*.xltm;*.xls;*.xlt;*.xls;*.xml;*.xml;*.xlam;*.xla;*.xlw;*.xlr;";
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

        internal Form1(DataTable table = null)
        {
            dictSorting = new Dictionary<string, SortOrder>();
            dictKeys = new List<string>();
            InitializeComponent();
            ExportHelper.CheckFolders();
            LoadProcedures();
            LoadWorkflows();
            LoadTolerances();
            LoadCases();
            historyHelper = new HistoryHelper();
            öffnenToolStripMenuItem1.Click += (sender, e) => importToolStripMenuItem_Click(sender, e);
            trimToolStripMenuItem.Click += (sender, e) => trimToolStripMenuItem_Click(sender, e);
            cSVToolStripMenuItem.Click += (sender, e) => cSVToolStripMenuItem_Click(sender, e);
            dBASEToolStripMenuItem1.Click += (sender, e) => dBASEToolStripMenuItem_Click(sender, e);
            excelToolStripMenuItem1.Click += (sender, e) => excelToolStripMenuItem_Click(sender, e);
            if (table != null)
            {
                AssignDataSource(table);
            }
            UpdateHelper.CheckUpdate(true, pgbLoading);
        }

        private void SetSorting(string order)
        {
            SortingOrder = order;
            dictSorting = ViewHelper.GenerateSortingList(GetSorting());
            dictKeys = dictSorting.Keys.ToList();
        }


        private void AssignDataSource(DataTable table = null)
        {
            sourceTable = table ?? sourceTable;

            
            int scrollBarHorizontal = dgTable.FirstDisplayedScrollingColumnIndex;
            int scrollBarVertical = dgTable.FirstDisplayedScrollingRowIndex;

            dgTable.DataSource = null;
            dgTable.DataSource = ViewHelper.GetSortedView(SortingOrder, sourceTable);
            if (scrollBarHorizontal != -1)
            {
                dgTable.FirstDisplayedScrollingColumnIndex = scrollBarHorizontal;
            }
            if (scrollBarVertical != -1)
            {
                dgTable.FirstDisplayedScrollingRowIndex = scrollBarVertical;
            }
            RestoreDataGridSortMode();
            dgTable.Columns.Cast<DataGridViewColumn>().Last().AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
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
            dgTable.BindingContext[dgTable.DataSource].EndCurrentEdit();
        }

        private DataView GetDataView()
        {
            EndEdit();
            return (DataView)dgTable.DataSource;
        }

        #region Add History Entry

        private void AddDataSourceValueChange(DataTable tableNew)
        {
            List<CellMatrix> oldValues = DataHelper.ChangesOfDataTable(tableNew);
            historyHelper.AddHistory(new History { State = State.ValueChange, Table = oldValues}, GetSorting());
            AssignDataSource(tableNew);
        }

        private void AddDataSourceNewTable(DataTable table)
        {
            historyHelper.ResetHistory();
            SetSorting(string.Empty);
            AssignDataSource(table);
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

        private void AddDataSourceAddRow(int rowIndex)
        {
            historyHelper.AddHistory(new History { State = State.InsertRow, RowIndex = rowIndex }, GetSorting());
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
            for (int i = 0; i < workflows.Count; i++)
            {
                int index = i;
                ToolStripMenuItem item = new ToolStripMenuItem(workflows[i].Name);
                item.Click += (sender, e) => workflow_Click(sender, e, workflows[index]);
                arbeitsablaufToolStripMenuItem.DropDownItems.Add(item);
            }
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
            List<string> headers = DataHelper.HeadersToLower(table);
            

            foreach (WorkProc wp in workflow.Procedures)
            {
                List<string> notFoundColumns = new List<string>();
                string[] wpHeaders = wp.GetHeaders();
                WorkflowHelper.CheckHeaders(headers, notFoundColumns, wpHeaders);
                if (!string.IsNullOrWhiteSpace(wp.NewColumn))
                {
                    headers.Add(wp.NewColumn);
                }
                else if (wp.CopyOldColumn)
                {
                    headers.AddRange(wpHeaders.Select(header => header + DataHelper.OldAffix));
                }

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
                    foreach (string col in nf.Headers) {
                        columns.Add(col);
                    }
                }
                SelectDuplicateColumns form = new SelectDuplicateColumns(columns.ToArray(), DataHelper.HeadersOfDataTable(table));
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
                                if (wpHeaders[y] == from[i] && nf.Headers.Contains(from[i])) //Kann sein, dass eine Spalte hinzugefügt wird und sie bei manchen Valid und bei manchen inValid ist, je nachdem wann sie ausgeführt werden
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
            new Thread(() =>
            {
                try
                {
                    table.AcceptChanges();
                    foreach (WorkProc t in temp)
                    {
                        ReplaceProcedure(table, null, t);
                    }

                    dgTable.Invoke(new MethodInvoker(() =>
                    {
                        AddDataSourceValueChange(table);
                    }));
                }
                catch(Exception ex)
                {
                    ErrorHelper.LogMessage(ex);
                }
                StopLoadingBar();
            }).Start();
        }

        private int GetProcedureThroughId(int id)
        {
            return procedures.FindIndex(p => p.Id == id);
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e, ImportState state = ImportState.None)
        {
            string textExt = "*.txt";
            string AccessExt = "*.accdb;*.accde;*.accdt;*.accdr;*.mdb";

            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = $"Text-, CSV-, DBASE und Excel-Dateien ({csvExt}, {textExt}, {AccessExt}, {dbfExt}*.xl*)|{csvExt};{textExt}; {excelExt}; {dbfExt}; {AccessExt}"
                            + $"|Textdateien ({textExt})|{textExt}"
                            + $"|Access-Dateien ({AccessExt})|{AccessExt}"
                            + $"|CSV-Dateien ({csvExt})|{csvExt}"
                            + $"|dBase-Dateien ({dbfExt})|{dbfExt}"
                            + $"|Excel-Dateien (*.xl*)|{excelExt}"
                            + "|Alle Dateien (*.*)|*.*",
                RestoreDirectory = true,
                Multiselect = state == ImportState.None || state == ImportState.Append
            };
            DataTable oldTable = GetDataSource();
            string mergePath = string.Empty;
            bool validMerge = state == ImportState.Merge && ProcAddTableColumns.CheckFile(FilePath,ref mergePath);
            if (validMerge || dialog.ShowDialog() == DialogResult.OK)
            {
                string[] filenames = validMerge ? new string[1] {mergePath} : dialog.FileNames;
                if (state == ImportState.None)
                {
                    SetFileName(filenames[0]);
                }

                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    StartLoadingBar();
                    bool multipleFiles = filenames.Length > 1;
                    Dictionary<string, ImportSettings> takeOver = new Dictionary<string, ImportSettings>();
                    DataTable newTable = null;
                    string fileNameBefore = Path.GetFileName(filenames[0]);
                    foreach (string file in filenames)
                    {
                        try
                        {
                            string filename = Path.GetFileName(file);
                            string extension = Path.GetExtension(file).ToLower();
                            DataTable table = null;

                            if (extension == ".dbf")
                            {
                                table = ImportHelper.OpenDBF(file);
                            }
                            else if (extension != string.Empty && AccessExt.Contains(extension))
                            {
                                table = ImportHelper.OpenMSAccess(file);
                            }
                            else if (extension != string.Empty && excelExt.Contains(extension))
                            {
                                table = ImportHelper.OpenExcel(file, this);
                            }
                            else
                            {
                                if (takeOver.TryGetValue(extension, out ImportSettings settings))
                                {
                                    if (settings.Values != null)
                                    {
                                        string data = File.ReadAllText(file, Encoding.GetEncoding(settings.CodePage));
                                        table = ImportHelper.OpenTextFixed(data, file, settings.Values, settings.Headers);
                                    }
                                    else if(settings.Separator != null)
                                    {
                                        table = ImportHelper.OpenText(file, settings.Separator, settings.CodePage, settings.ContainsHeaders);
                                    }
                                    else
                                    {
                                        table = ImportHelper.OpenTextBetween(file, settings.CodePage, settings.TextBegin, settings.TextEnd, settings.ContainsHeaders);
                                    }

                                }
                                else
                                {
                                    TextFormat form = new TextFormat(file, multipleFiles);
                                    //BeginInvoke(new MethodInvoker(() =>
                                    //{
                                        if (form.ShowDialog() == DialogResult.OK)
                                        {
                                            if (form.TakeOver)
                                            {
                                                takeOver.Add(extension, form.ImportSettings);
                                            }
                                            table = form.DataTable;
                                        }
                                    //}));
                                }
                            }
                            if (table != null)
                            {
                                if (newTable != null)
                                {
                                    DataHelper.ConcatTables(newTable, table, fileNameBefore, filename);
                                }
                                else
                                {
                                    newTable = table;
                                }
                                fileNameBefore = filename;
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex);
                        }
                    }
                    FinishImport(newTable, state, oldTable, Path.GetFileName(filenames[0]));
                }).Start();
            }
        }

        private void SetFileName(string path)
        {
            lblFilename.Text = Path.GetFileName(path);
            FilePath = path;
        }

        private void FinishImport(DataTable table, ImportState state, DataTable oldTable, string filename)
        {
            StopLoadingBar();
            if(table != null) {
                DataHelper.RemoveNull(table);
                switch (state)
                {
                    case ImportState.Merge:
                        ShowMergeForm(table, oldTable);
                        break;

                    case ImportState.Append:
                        dgTable.BeginInvoke(new MethodInvoker(() => { MergeTables(table, filename); }));
                        break;

                    case ImportState.Header:
                        object[] headers = DataHelper.HeadersOfDataTable(oldTable);
                        DataHelper.SetHeaders(table, oldTable);
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

        private void ShowMergeForm(DataTable importTable, DataTable sourceTable)
        {
            MergeTable form = new MergeTable(DataHelper.HeadersOfDataTable(sourceTable), DataHelper.HeadersOfDataTable(importTable));
            if (form.ShowDialog() == DialogResult.OK)
            {
                string[] ImportColumns = form.getSelectedColumns();
                int SourceMergeIndex = form.getSelectedOriginal();
                int ImportMergeIndex = form.getSelectedMerge();
                bool SortColumn = form.OrderColumnName() != string.Empty;
                string orderColumnName = form.OrderColumnName();
                new Thread(() =>
                {
                    try {
                        
                        #region Compare Everything
                        int oldCount = sourceTable.Columns.Count;
                        int newColumnIndex = oldCount + ImportColumns.Length -1; //-1: without identifier

                        for (int i = 0; i < ImportColumns.Length; i++)
                        {
                            if (i == ImportMergeIndex) continue;

                            DataHelper.AddColumn(ImportColumns[i], sourceTable);
                        }
                        if (SortColumn)
                        {
                            string SortColumnName = "[Sortierung]";
                            DataHelper.AddColumn(orderColumnName, sourceTable);
                            int LastIndex = importTable.Columns.Count;
                            DataHelper.AddColumn(SortColumnName, importTable);
                            for(int i = 0; i < importTable.Rows.Count; i++)
                            {
                                importTable.Rows[i][LastIndex] = i.ToString();
                            }
                            ImportColumns = new List<string>(ImportColumns)
                            {
                                SortColumnName
                            }.ToArray();
                        }

                        pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Value = 0; pgbLoading.Maximum = importTable.Rows.Count; }));


                        HashSet<int> hs = new HashSet<int>();
                    

                        for (int y = 0; y < sourceTable.Rows.Count; y++)
                        {
                            DataRow source = sourceTable.Rows[y];
                        
                            for (int rowIndex = 0; rowIndex < importTable.Rows.Count; rowIndex++)
                            {

                                DataRow row = importTable.Rows[rowIndex];
                                if (source.ItemArray[SourceMergeIndex].ToString() == row.ItemArray[ImportMergeIndex].ToString())
                                {
                                    int Offset = 0;
                                    for (int i = 0; i < ImportColumns.Length; i++)
                                    {
                                        if (i == ImportMergeIndex)
                                        {
                                            Offset++;
                                        }
                                        else
                                        {
                                            source.SetField(oldCount + i - Offset, row.ItemArray[i]);
                                        }
                                    }
                                    importTable.Rows.RemoveAt(rowIndex);
                                    break;
                                
                                }
                            }

                            pgbLoading.BeginInvoke(new MethodInvoker(() => { pgbLoading.Value++; }));
                        }
                        #endregion

                        dgTable.Invoke(new MethodInvoker(() => { AddDataSourceValueChange(sourceTable); }));
                        pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Value = pgbLoading.Maximum = 0; }));
                    }
                    catch (Exception ex)
                    {
                        ErrorHelper.LogMessage(ex);
                    }
                }).Start();
            }
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
                StartLoadingBar();
                new Thread(() =>
                {
                    try
                    {
                        TrimDataTable(GetDataSource());
                    }
                    catch (Exception ex)
                    {
                        ErrorHelper.LogMessage(ex);
                    }
                }).Start();
            }
        }

        private void procedure_Click(object sender, EventArgs e, Proc procedure)
        {
            Formula formula = new Formula(FormulaState.Procedure, DataHelper.HeadersOfDataTable(sourceTable));
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
                            ReplaceProcedure(newTable, procedure, user);
                            dgTable.Invoke(new MethodInvoker(() => { AddDataSourceValueChange(newTable); }));
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex);
                        }
                    }).Start();
                }
            }
        }

        private void ReplaceProcedure(DataTable table, Proc procedure, WorkProc wp)
        {
            wp.doWork(table, out string newOrder, GetCaseThroughId(wp.ProcedureId), tolerances, procedure ?? GetProcedure(wp.ProcedureId));
            if (newOrder != string.Empty)
            {
                SetSorting(newOrder);
            }
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

        private void TrimDataTable(DataTable dt)
        {
            Thread.CurrentThread.IsBackground = true;
            ProcTrim proc = new ProcTrim();
            proc.doWork(dt, out string sortingOrder, null, null, null);
            dgTable.Invoke(new MethodInvoker(() =>
            {
                AddDataSourceValueChange(dt);
            }));
            StopLoadingBar();
        }

        private void excelToolStripMenuItem_Click(object sender, EventArgs e, string path = null)
        {
            if (dgTable.DataSource != null)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog
                {
                    Filter = $"Excel Dateien|{excelExt}|Alle Dateien (*.*)|*.*",
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
                            string newPath = ExportHelper.ExportExcel(table, Path.GetDirectoryName(path),path);
                            if(newPath != null)
                            {
                                SetFileName(newPath);
                            }
                            StopLoadingBar();
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
            
            
            AddDataSourceDeleteColumn(col, selectedColumn, DataHelper.ColumnValues(table, selectedColumn));
            table.Columns.RemoveAt(selectedColumn);
            AssignDataSourceColumnChange(table, col.ColumnName);
            SetRowCount();
        }


        private void zeilenZusammenfügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Merge formula = new Merge(DataHelper.HeadersOfDataTable(GetDataSource()), contextGlobal);
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
                    Filter = $"DBASE Dateien ({dbfExt})|{dbfExt}|Alle Dateien (*.*)|*.*",
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
                            ExportHelper.ExportDbase(Path.GetFileNameWithoutExtension(path), table, Path.GetDirectoryName(path));
                            StopLoadingBar();
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
            AssignDataSource(table);
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
            Administration form = new Administration(dgTable.DataSource != null ? DataHelper.HeadersOfDataTable(GetDataSource()) : new object[0], contextGlobal);
            form.FormClosed += new FormClosedEventHandler(administrationFormClosed);
            form.Show();
        }

        private void administrationFormClosed(object sender, FormClosedEventArgs e)
        {
            Administration admin = ((Administration)sender);
            LoadProcedures(admin.Procedures);
            LoadWorkflows(admin.Workflows);
            LoadTolerances(admin.Tolerances);
            LoadCases(admin.Cases);
        }

        private void cSVToolStripMenuItem_Click(object sender, EventArgs e, DataTable dt = null, string path = null)
        {
            if (dgTable.DataSource != null)
            {
                DataTable table = dt ?? GetDataSource(true);
                SaveFileDialog saveFileDialog1 = new SaveFileDialog
                {
                    Filter = $"CSV Dateien ({csvExt})|{csvExt}|Alle Dateien (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };
                
                if (path != null || saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    StartLoadingBar();
                    path = path ?? saveFileDialog1.FileName;
                    new Thread(() =>
                    {
                        try
                        {
                            Thread.CurrentThread.IsBackground = true;
                            ExportHelper.ExportCsv(table, Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                            StopLoadingBar();
                        }
                        catch (Exception ex)
                        {
                            ErrorHelper.LogMessage(ex);
                        }
                    }).Start();
                }
            }
        }

        private void postwurfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Formula formula = new Formula(FormulaState.Export, DataHelper.HeadersOfDataTable(sourceTable));
            if(formula.ShowDialog() == DialogResult.OK)
            {
                DataTable table = GetDataSource(true);
                HashSet<int> columns = new HashSet<int>(DataHelper.HeaderIndices(table, formula.SelectedHeaders()));

                int realCounter = 0;
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (!columns.Contains(realCounter))
                    {
                        table.Columns.RemoveAt(i);
                        i--;
                    }
                    realCounter++;
                }

                cSVToolStripMenuItem_Click(null, null, table);
            }
        }

        private void nachWertInSpalteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportCustom formula = new ExportCustom(DataHelper.HeadersOfDataTable(sourceTable), sourceTable);
            formula.FormClosed += new FormClosedEventHandler(saveExportClosed);
            formula.Show();
        }

        private void saveExportClosed(object sender, FormClosedEventArgs e)
        {
            DataTable originalTable = GetDataSource(true);
            ExportCustom export = (ExportCustom)sender;
            if (export.DialogResult == DialogResult.OK)
            {
                Dictionary<string, List<DataTable>> Dict = new Dictionary<string, List<DataTable>>();
                DataTable tableSkeleton = new DataTable() { TableName = null };
                originalTable.Columns.Cast<DataColumn>().Select(Column => Column.ColumnName).ToList().ForEach(Header => tableSkeleton.Columns.Add(Header));
                if (export.AllValues())
                {
                    export.ValuesOfColumn().ForEach(value => Dict.Add(value, new List<DataTable> { tableSkeleton.Copy() }));
                }
                else
                {
                    foreach(string key in export.Files.Keys)
                    {
                        if(export.Files.TryGetValue(key,out List<string> values))
                        {
                            DataTable temp = tableSkeleton.Copy();
                            temp.TableName = key;
                            foreach (string value in values) {
                                if (Dict.TryGetValue(value, out List<DataTable> tables))
                                {
                                    tables.Add(temp);
                                }
                                else
                                {
                                    Dict.Add(value, new List<DataTable> { temp });
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < originalTable.Rows.Count; i++)
                {
                    if(Dict.TryGetValue(originalTable.Rows[i][export.getColumnIndex()].ToString(),out List<DataTable> tables))
                    {
                        tables.ForEach(table => table.ImportRow(originalTable.Rows[i]));
                    }
                }
                StartLoadingBar();
                int Format = export.SelectedFormat;
                new Thread(() =>
                {
                    foreach (string Key in Dict.Keys)
                    {
                        if (Dict.TryGetValue(Key, out List<DataTable> tables))
                        {
                            foreach (DataTable Table in tables)
                            {
                                string FileName = Table.TableName == string.Empty ? $"{Path.GetFileNameWithoutExtension(FilePath)} {Key}" : Table.TableName;
                                string path = Path.GetDirectoryName(FilePath);
                                switch (Format)
                                {
                                    //CSV
                                    case 0:
                                        {
                                            ExportHelper.ExportCsv(Table, path, FileName);
                                        }
                                        break;

                                    //Dbase
                                    case 1:
                                        {
                                            ExportHelper.ExportDbase(FileName, Table, path);
                                        }
                                        break;

                                    //Excel
                                    case 2:
                                        {
                                            ExportHelper.ExportExcel(Table, path, FileName);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    StopLoadingBar();
                }).Start();
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
            }));
        }

        private void zählenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportCount form = new ExportCount(DataHelper.HeadersOfDataTable(GetDataSource()));
            form.FormClosed += new FormClosedEventHandler(CountContendClosed);
            form.ShowDialog();
        }

        private void CountContendClosed(object sender, FormClosedEventArgs e)
        {
            ExportCount export = (ExportCount)sender;
            if (export.DialogResult == DialogResult.OK)
            {
                string selectedValue = export.getSelectedValue();
                int columnIndex = export.getColumnIndex();
                int count = export.CountChecked ? export.Count : 0;
                DataTable oldTable = GetDataSource();

                new Thread(() =>
                {
                    StartLoadingBar();
                    DataTable table = ViewHelper.GetSortedView($"[{selectedValue}] asc", oldTable).ToTable();
                    DataTable newTable = new DataTable();

                    if (count == 0)
                    {
                        Dictionary<string, int> pair = new Dictionary<string, int>();
                        foreach (DataRow row in table.Rows)
                        {
                            string item = row[columnIndex].ToString();
                            if (pair.ContainsKey(item))
                            {
                                pair[item] = pair[item] + 1;
                            }
                            else
                            {
                                pair.Add(item, 1);
                            }
                        }
                        newTable = DataHelper.DictionaryToDataTable(pair, selectedValue);
                    }
                    else
                    {
                        Dictionary<string, int> pair = new Dictionary<string, int>();
                        foreach(string col in DataHelper.HeadersOfDataTable(table))
                        {
                            newTable.Columns.Add(col);
                        }
                        foreach (DataRow row in table.Rows)
                        {
                            string item = row[columnIndex].ToString();
                            bool contains;
                            if ((contains = pair.ContainsKey(item)) && pair[item] < count)
                            {
                                newTable.Rows.Add(row.ItemArray);
                                pair[item] = pair[item] + 1;
                            }
                            else if(!contains)
                            {
                                pair.Add(item, 1);
                                newTable.Rows.Add(row.ItemArray);
                            }
                        }
                    }
                    BeginInvoke(new MethodInvoker(() =>
                    {
                        Form1 form = new Form1(newTable);
                        form.Show();
                    }));
                    StopLoadingBar();
                }).Start();
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
                    ResetSort(col);
                }
                else
                {
                    bool asc = col.HeaderCell.SortGlyphDirection == SortOrder.Ascending;

                    SetSorting($"[{col.Name}] {(asc ? "DESC" : "ASC")}");
                    AssignDataSource();
                }
            }
        }

        private void ResetSort(DataGridViewColumn col)
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
            DataHelper.ConcatTables(originalTable, table, Path.GetFileName(FilePath), filename);

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
            UpLowCaseForm form = new UpLowCaseForm(DataHelper.HeadersOfDataTable(GetDataSource()));
            if (form.ShowDialog() == DialogResult.OK) {
                StartSingleWorkflow(form.Procedure);
            }
        }

        private void rundenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RoundForm form = new RoundForm(DataHelper.HeadersOfDataTable(GetDataSource()));
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
            else if (extension != string.Empty && excelExt.Contains(extension))
            {
                excelToolStripMenuItem_Click(null, null, FileName);
            }
            else
            {
                cSVToolStripMenuItem_Click(null, null, null, FileName);
            }
        }

        private void einstellungenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new SettingForm().Show();
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateHelper.CheckUpdate(false, pgbLoading);
        }

        private void zeilenZusammenfügenToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            DataTable table = GetDataSource();
            
            MergeColumns form = new MergeColumns(DataHelper.HeadersOfDataTable(table));
            if(form.ShowDialog() == DialogResult.OK)
            {
                string identifier = form.Identifier;
                int identifierIndex = form.IdentifierIndex;
                List<PlusListboxItem> additionalColumns = form.AdditionalColumns;

                new Thread(() =>
                {
                    StartLoadingBar();

                    SetStatusLabel("Daten werden geladen");

                    int lastIndex = table.Columns.Count;
                    table.Columns.Add(DataHelper.TempSort);
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        table.Rows[i][lastIndex] = i;
                    }
                    int rowCount = table.Rows.Count;
                    table = ViewHelper.GetSortedView($"[{identifier}] asc", table).ToTable();
                    while(rowCount < table.Rows.Count)
                    {
                        table.Rows.RemoveAt(table.Rows.Count - 1);
                    }
                    table.AcceptChanges();

                    SetStatusLabel("Daten werden überprüft");
                    Dictionary<string, DataRowArray> dict = new Dictionary<string, DataRowArray>();
                    //RowIdentifier, Values
                    string oldIdenfifier = table.Rows[0][identifierIndex].ToString();
                    int counter = 0;
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        DataRow oldRow = table.Rows[i];
                        string newIdenfifier = oldRow[identifier].ToString();

                        counter = newIdenfifier == oldIdenfifier ? counter + 1 : 1;

                        if (dict.ContainsKey(newIdenfifier) && dict.TryGetValue(newIdenfifier, out DataRowArray dataRowArray))
                        {
                            List<string> values = new List<string>();
                            foreach (PlusListboxItem additionalColumn in additionalColumns)
                            {
                                values.Add(oldRow[additionalColumn.ToString()].ToString());
                            }
                            dataRowArray.Add(values);
                        }
                        else
                        {
                            dict.Add(newIdenfifier, new DataRowArray(oldRow, new List<string>()));
                        }


                        if (counter > 1)
                        {
                            table.Rows[i].Delete();
                        }

                        oldIdenfifier = newIdenfifier;
                    }

                    int newColumns = dict.Values.Max(dataRowArray => dataRowArray.Values.Count)-1;

                    SetStatusLabel("Neue Spalten werden hinzugefügt");
                    for (int i = 1; i <= newColumns; i++)
                    {
                        foreach (PlusListboxItem additionalColumn in additionalColumns.Where(item => !item.Checked))
                        {
                            DataHelper.AddColumn(additionalColumn.ToString() + i, table);
                        }
                    }

                    int itemCount = dict.Values.Count;
                    counter = 0;
                    foreach (DataRowArray dataRowArray in dict.Values)
                    {
                        if (dataRowArray.Values.Count > 1)
                        {
                            SetStatusLabel($"Daten werden geschrieben {counter}/{itemCount}");
                            for (int i = 1; i < dataRowArray.Values.Count; i++) //except first one
                            {
                                for (int y = 0; y < dataRowArray.Values[i].Count; y++)
                                {
                                    //sum
                                    if (additionalColumns[y].Checked)
                                    {
                                        dataRowArray.DataRow[additionalColumns[y].ToString()] = DataHelper.AddStringAsFloat(dataRowArray.DataRow[additionalColumns[y].ToString()].ToString(), dataRowArray.Values[i][y]);
                                    }
                                    //additional Column
                                    else
                                    {
                                        dataRowArray.DataRow[additionalColumns[y].ToString() + i] = dataRowArray.Values[i][y];
                                    }
                                }
                            }
                        }
                        counter++;
                    }


                    dgTable.Invoke(new MethodInvoker(() =>
                    {
                        DataTable originalSortTable = ViewHelper.GetSortedView($"[{DataHelper.TempSort}] asc", table.Copy()).ToTable();
                        
                        List<CellMatrix> oldValues = DataHelper.ChangesOfDataTable(table);
                        historyHelper.AddHistory(new History { State = State.ValueChange, Table = oldValues }, GetSorting());
                        originalSortTable.Columns.Remove(DataHelper.TempSort);

                        AssignDataSource(originalSortTable);
                    }));
                    SetStatusLabel(string.Empty);
                    StopLoadingBar();

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
            PaddingForm form = new PaddingForm(DataHelper.HeadersOfDataTable(GetDataSource()));
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
            NumerationForm form = new NumerationForm(DataHelper.HeadersOfDataTable(sourceTable));
            if(form.ShowDialog() == DialogResult.OK)
            {
                StartSingleWorkflow(form.Procedure);
            }
        }

        private void StartSingleWorkflow(WorkProc proc)
        {
            workflow_Click(null, null, new Work(string.Empty, new List<WorkProc> { proc }, 0));
        }

        private void substringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SubstringForm form = new SubstringForm(DataHelper.HeadersOfDataTable(sourceTable));
            if(form.ShowDialog() == DialogResult.OK)
            {
                StartSingleWorkflow(form.Procedure);
            }
        }

        private void textErsetzenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ReplaceWholeForm form = new ReplaceWholeForm(DataHelper.HeadersOfDataTable(sourceTable));
            if(form.ShowDialog() == DialogResult.OK)
            {
                ProcReplaceWhole proc = new ProcReplaceWhole(form.SelectedHeaders, form.ReplaceText);
                StartSingleWorkflow(proc);
            }
        }

        private void einstellungenToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            SettingForm form = new SettingForm();
            form.Show();
        }

        private void sortierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortForm form = new SortForm(DataHelper.HeadersOfDataTable(GetDataSource()), SortingOrder);
            if(form.ShowDialog() == DialogResult.OK)
            {
                SetSorting(form.SortString);
                AssignDataSource();
            }
        }
    }
}
