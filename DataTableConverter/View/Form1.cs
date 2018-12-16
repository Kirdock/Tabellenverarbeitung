using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
using DataTableConverter.View;
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
        Dictionary<string, SortOrder> dictSorting;
        private int rowBefore; //Wenn eine Zelle im DataGridView geändert wird, kann ich mit dgTable[col, row] den geänderten Wert nicht holen, da die DataGridView zu diesem Zeitpunkt wieder sortiert wurde
        private List<string> dictKeys;

        internal Form1(DataTable table = null)
        {
            dictSorting = new Dictionary<string, SortOrder>();
            dictKeys = new List<string>();
            InitializeComponent();
            ExportHelper.checkFolders();
            loadProcedures();
            loadWorkflows();
            loadTolerances();
            loadCases();
            historyHelper = new HistoryHelper();
            öffnenToolStripMenuItem1.Click += (sender, e) => importToolStripMenuItem_Click(sender, e);
            trimToolStripMenuItem.Click += (sender, e) => trimToolStripMenuItem_Click(sender, e);
            cSVToolStripMenuItem.Click += (sender, e) => cSVToolStripMenuItem_Click(sender, e);
            if (table != null)
            {
                assignDataSource(table);
            }
        }

        private void setSorting(string order)
        {
            SortingOrder = order;
            dictSorting = ViewHelper.generateSortingList(getSorting());
            dictKeys = dictSorting.Keys.ToList();
        }


        private void assignDataSource(DataTable table = null)
        {
            sourceTable = table ?? sourceTable;

            
            int scrollBarHorizontal = dgTable.FirstDisplayedScrollingColumnIndex;
            int scrollBarVertical = dgTable.FirstDisplayedScrollingRowIndex;

            dgTable.DataSource = null;
            dgTable.DataSource = ViewHelper.getSortedView(SortingOrder,sourceTable);

            if (scrollBarHorizontal != -1)
            {
                dgTable.FirstDisplayedScrollingColumnIndex = scrollBarHorizontal;
            }
            if (scrollBarVertical != -1)
            {
                dgTable.FirstDisplayedScrollingRowIndex = scrollBarVertical;
            }
            restoreDataGridSortMode();
        }


        private void restoreDataGridSortMode()
        {

            for (int i = 0; i < dgTable.Columns.Count; i++)
            {
                dgTable.Columns[i].SortMode = DataGridViewColumnSortMode.Programmatic;
            }
        }

        private void assignDataSourceColumnChange(DataTable table, string column, string newColumn = null)
        {
            setSorting(ViewHelper.adjustSort(getSorting(), column, newColumn));
            
            assignDataSource(table);            
            restoreDataGridSortMode();
        }

        private string getSorting()
        {
            return SortingOrder;
        }

        private DataTable getDataSource(bool withSort = false)
        {
            DataTable table = sourceTable;
            if (table != null)
            {
                endEdit();
                if (withSort)
                {
                    table = getDataView().ToTable();
                }
                else
                {
                    table = sourceTable.Copy();
                }
            }
            return table;
        }

        private void endEdit()
        {
            dgTable.BindingContext[dgTable.DataSource].EndCurrentEdit();
        }

        private DataView getDataView()
        {
            endEdit();
            return (DataView)dgTable.DataSource;
        }

        #region Add History Entry

        private void addDataSourceValueChange(DataTable tableOld, DataTable tableNew)
        {
            List<CellMatrix> oldValues = DataHelper.getChangesOfDataTable(tableOld, tableNew, -1);
            historyHelper.addHistory(new History { State = State.ValueChange, Table = oldValues}, getSorting());
            assignDataSource(tableNew);
        }

        private void addDataSourceNewTable(DataTable table)
        {
            historyHelper.resetHistory();
            assignDataSource(table);
        }

        private void addDataSourceAddColumn(int columnIndex)
        {
            historyHelper.addHistory(new History { State = State.InsertColumn, ColumnIndex = columnIndex }, getSorting());
        }

        private void addDataSourceAddColumnAndRows(int columnIndex, int rowIndex)
        {
            historyHelper.addHistory(new History { State = State.AddColumnsAndRows, ColumnIndex = columnIndex, RowIndex = rowIndex }, getSorting());
        }

        private void addDataSourceAddRow(int rowIndex)
        {
            historyHelper.addHistory(new History { State = State.InsertRow, RowIndex = rowIndex }, getSorting());
        }

        private void addDataSourceHeaderChange(int columnIndex, string text)
        {
            historyHelper.addHistory(new History { State = State.HeaderChange, NewText = text, ColumnIndex = columnIndex }, getSorting());
        }

        private void addDataSourceHeadersChange(object[] oldHeader)
        {
            object[][] newItem = new object[1][];
            newItem[0] = oldHeader;
            historyHelper.addHistory(new History { State = State.HeadersChange, Row = newItem}, getSorting());
        }

        private void addDataSourceDeleteColumn(DataColumn col, int index, object[] columnValues)
        {
            DataColumn[] newItem = new DataColumn[1];
            newItem[0] = col;
            object[][] newValues = new object[1][];
            newValues[0] = columnValues;
            historyHelper.addHistory(new History { State = State.DeleteColumn, ColumnIndex = index, Column = newItem, ColumnValues = newValues }, getSorting());
        }

        private void addDataSourceColumnValuesChanged(DataTable oldTable, DataTable newTable, int selectedColumn)
        {
            historyHelper.addHistory(new History { State = State.ValueChange, Table = DataHelper.getChangesOfDataTable(oldTable, newTable, selectedColumn)}, getSorting());
        }

        private void addDataSourceDeleteRow(object[] itemArray, int rowIndex)
        {
            object[][] newItem = new object[1][];
            newItem[0] = itemArray;
            historyHelper.addHistory(new History { State = State.DeleteRow, RowIndex = rowIndex, Row = newItem }, getSorting());
        }

        private void addDataSourceCellChanged(string newText, int columnIndex, int rowIndex)
        {
            historyHelper.addHistory(new History { State = State.CellValueChange, NewText = newText, ColumnIndex = columnIndex, RowIndex = rowIndex }, getSorting());
        }

        #endregion

        private void loadProcedures(List<Proc> proc = null)
        {
            for (int i = 2; i < funktionenToolStripMenuItem.DropDownItems.Count;)
            {
                funktionenToolStripMenuItem.DropDownItems.RemoveAt(i);
            }
            procedures = proc ?? ImportHelper.loadProcedures();
            for (int i = 0; i < procedures.Count; i++)
            {
                int index = i;
                ToolStripMenuItem item = new ToolStripMenuItem(procedures[i].Name);
                item.Click += (sender, e) => procedure_Click(sender, e, procedures[index]);
                funktionenToolStripMenuItem.DropDownItems.Add(item);
            }   
        }

        private void loadWorkflows(List<Work> work = null)
        {
            arbeitsablaufToolStripMenuItem.DropDownItems.Clear();

            workflows = work ?? ImportHelper.loadWorkflows();
            for (int i = 0; i < workflows.Count; i++)
            {
                int index = i;
                ToolStripMenuItem item = new ToolStripMenuItem(workflows[i].Name);
                item.Click += (sender, e) => workflow_Click(sender, e, workflows[index]);
                arbeitsablaufToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void loadTolerances(List<Tolerance> tol = null)
        {
            tolerances = tol ??  ImportHelper.loadTolerances();
        }

        private void loadCases(List<Case> cas = null)
        {
            duplikateToolStripMenuItem.DropDownItems.Clear();

            cases = cas ?? ImportHelper.loadCases();
            for (int i = 0; i < cases.Count; i++)
            {
                int index = i;
                ToolStripMenuItem item = new ToolStripMenuItem(cases[i].Name);
                item.Click += (sender, e) => case_Click(sender, e, cases[index], null, getDataSource());
                duplikateToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void case_Click(object sender, EventArgs e, Case cas, string[] caseColumnsWorkflow, DataTable originalTable)
        {
            List<WorkProc> procs = new List<WorkProc>();
            ProcDuplicate procDuplicate = new ProcDuplicate(0, cas.Id, cas.Name);
            procDuplicate.DuplicateColumns = cas.Columns.Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToArray();
            procs.Add(procDuplicate);

            Work workflow = new Work(string.Empty, procs, 0);
            workflow_Click(null, null, workflow);
        }

        private void workflow_Click(object sender, EventArgs e, Work workflow)
        {
            List<NotFoundHeaders> notFound = new List<NotFoundHeaders>();
            DataTable table = getDataSource();
            List<string> headers = DataHelper.getHeadersToLower(table);
            

            foreach (WorkProc wp in workflow.Procedures)
            {
                List<string> notFoundColumns = new List<string>();
                
                WorkflowHelper.checkHeaders(headers, notFoundColumns, wp.getHeaders());
                if (!string.IsNullOrWhiteSpace(wp.NewColumn))
                {
                    headers.Add(wp.NewColumn);
                }

                if(notFoundColumns.Count > 0)
                {
                    notFound.Add(new NotFoundHeaders(notFoundColumns, wp));
                }
            }
            if (notFound.Count == 0)
            {
                replaceThroughTemp(workflow.Procedures);
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
                SelectDuplicateColumns form = new SelectDuplicateColumns(columns.ToArray(), DataHelper.getHeadersOfDataTable(table));
                if (form.ShowDialog() == DialogResult.OK) {
                    string[] from = form.Table.Rows.Cast<DataRow>().Select(row => row.ItemArray[0].ToString()).ToArray();
                    string[] to = form.Table.Rows.Cast<DataRow>().Select(row => row.ItemArray[1].ToString()).ToArray();

                    foreach (NotFoundHeaders nf in notFound)
                    {
                        string[] wpHeaders = nf.Wp.getHeaders();
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
                    replaceThroughTemp(workflow.Procedures);
                }
            }
        }

        private void replaceThroughTemp(List<WorkProc> temp)
        {
            DataTable table = getDataSource();
            pgbLoading.Style = ProgressBarStyle.Marquee;
            new Thread(() =>
            {
                foreach (WorkProc t in temp)
                {
                    replaceProcedure(table, null, null, t);
                }
                dgTable.Invoke(new MethodInvoker(() => {
                    addDataSourceValueChange(getDataSource(), table);
                }));
                pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Blocks; }));
            }).Start();
        }

        private int getProcedureThroughId(int id)
        {
            return procedures.FindIndex(p => p.Id == id);
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e, ImportState state = ImportState.None)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            string textExt = "*.txt";
            

            dialog.Filter = $"Text-, CSV-, DBASE und Excel-Dateien ({csvExt}, {textExt}, {dbfExt}*.xl*)|{csvExt};{textExt}; {excelExt}; {dbfExt}"
                            + $"|Textdateien ({textExt})|{textExt}"
                            + $"|CSV-Dateien ({csvExt})|{csvExt}"
                            + $"|dBase-Dateien ({dbfExt})|{dbfExt}"
                            + $"|Excel-Dateien (*.xl*)|{excelExt}"
                            + "|Alle Dateien (*.*)|*.*";
            dialog.RestoreDirectory = true;
            DataTable oldTable = getDataSource();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                lblFilename.Text = Path.GetFileName(dialog.FileName);
                string extension = Path.GetExtension(dialog.FileName).ToLower();

                if (extension == ".dbf")
                {
                    pgbLoading.Style = ProgressBarStyle.Marquee;
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        DataTable dt = ImportHelper.openDBF(dialog.FileName);
                        finishImport(dt, state, oldTable);
                        
                    }).Start();
                }
                else if (excelExt.Contains(extension))
                {
                    pgbLoading.Style = ProgressBarStyle.Marquee;
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        DataTable dt = ImportHelper.openExcel(dialog.FileName, this);
                        finishImport(dt, state, oldTable);
                    }).Start();
                }
                else
                {
                    TextFormat form = new TextFormat(dialog.FileName);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        finishImport(form.DataTable, state, oldTable);
                    }
                }
            }
        }

        private void finishImport(DataTable table, ImportState state, DataTable oldTable)
        {
            pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Blocks; }));
            switch (state)
            {
                case ImportState.Append:
                    showMergeForm(table, oldTable);
                    break;

                case ImportState.Merge:
                    dgTable.Invoke(new MethodInvoker(() => { mergeTables(table); }));
                    break;

                case ImportState.Header:

                    object[] headers = DataHelper.getHeadersOfDataTable(oldTable);
                    DataHelper.setHeaders(table, oldTable);
                    dgTable.Invoke(new MethodInvoker(() => { assignDataSource(oldTable); }));
                    addDataSourceHeadersChange(headers);
                    break;

                default:
                    dgTable.Invoke(new MethodInvoker(() => { addDataSourceNewTable(table); }));
                    lblRows.GetCurrentParent().Invoke(new MethodInvoker(() => { setRowCount(table.Rows.Count); }));
                    break;
        }
        }

        private void showMergeForm(DataTable importTable, DataTable sourceTable)
        {
            MergeTable form = new MergeTable(DataHelper.getHeadersOfDataTable(sourceTable), DataHelper.getHeadersOfDataTable(importTable));
            if (form.ShowDialog() == DialogResult.OK)
            {
                new Thread(() =>
                {
                    DataTable tableOld = sourceTable.Copy();
                    string[] columns = form.getSelectedColumns();
                    int oldIndex = form.getSelectedOriginal();
                    int mergeIndex = form.getSelectedMerge();
                    bool newColumn = form.getOrderColumnName() != string.Empty;

                    #region Compare Everything
                    int oldCount = sourceTable.Columns.Count;
                    int newColumnIndex = oldCount + columns.Length;

                    for (int i = 0; i < columns.Length; i++)
                    {
                        DataHelper.addColumn(columns[i], sourceTable);
                    }
                    if (newColumn)
                    {
                        DataHelper.addColumn(form.getOrderColumnName(), sourceTable);
                    }
                    pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Marquee; }));


                    HashSet<int> hs = new HashSet<int>();
                    for (int y = 0; y < importTable.Rows.Count; y++)
                    {
                        DataRow row = importTable.Rows[y];
                        for (int rowIndex = 0; rowIndex < sourceTable.Rows.Count; rowIndex++)
                        {
                            if (hs.Contains(rowIndex)) continue;

                            DataRow source = sourceTable.Rows[rowIndex];
                            if (source.ItemArray[oldIndex].ToString() == row.ItemArray[mergeIndex].ToString())
                            {
                                for (int i = 0; i < columns.Length; i++)
                                {
                                    source.SetField(oldCount + i, row.ItemArray[i]);
                                }
                                if (newColumn)
                                {
                                    source.SetField(newColumnIndex, (y + 1).ToString());
                                }
                                hs.Add(rowIndex);
                                break;
                            }
                        }
                    }
                    #endregion

                    dgTable.Invoke(new MethodInvoker(() => { addDataSourceValueChange(tableOld, sourceTable); }));
                    pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Blocks; }));
                }).Start();
            }
        }

        private void setRowCount()
        {
            setRowCount(dgTable.Rows.Count - 1);
        }

        private void setRowCount(int count)
        {
            lblRows.Text = count.ToString();
        }

        private void trimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgTable.DataSource != null)
            {
                pgbLoading.Style = ProgressBarStyle.Marquee;
                new Thread(() => trimDataTable(getDataSource())).Start();
            }
        }

        private void procedure_Click(object sender, EventArgs e, Proc procedure)
        {
            Formula formula = new Formula(FormulaState.Procedure, DataHelper.getHeadersOfDataTable(getDataSource()));
            formula.Text = "Bitte Spalten angeben";
            formula.FormClosed += (sender2, e2) => procedureClosed(sender2, e2, procedure);
            formula.Show();
        }

        private void replaceProcedure(DataTable table, Proc procedure, string columnHeader, WorkProc wp)
        {
            wp.doWork(table, out string newOrder, getCaseThroughId(wp.ProcedureId), tolerances, procedure ?? getProcedure(wp.ProcedureId));
            if (newOrder != string.Empty)
            {
                setSorting(newOrder);
            }
        }

        private Proc getProcedure(int id)
        {
            Proc proc = null;
            int index = getProcedureThroughId(id);
            if (index != -1) {
                proc = procedures[index];
            }
            return proc;
        }

        private Case getCaseThroughId(int id)
        {
            int index = cases.FindIndex(cs => cs.Id == id);
            return index != -1 ? cases[index] : null;
        }

        private void procedureClosed(object sender, FormClosedEventArgs e, Proc procedure)
        {
            if (((Formula)sender).DialogResult == DialogResult.OK)
            {
                string[] columns = ((Formula)sender).getSelectedHeaders();
                
                if (columns.Length > 0)
                {
                    ProcUser user = new ProcUser(columns);

                    DataTable newTable = getDataSource();
                    new Thread(() =>
                    {
                        replaceProcedure(newTable, procedure, ((Formula)sender).getHeaderName(), user);
                        dgTable.Invoke(new MethodInvoker(() => { addDataSourceValueChange(getDataSource(), newTable); }));
                    }).Start();
                }
            }
        }

        private void trimDataTable(DataTable dt)
        {
            Thread.CurrentThread.IsBackground = true;
            ProcTrim proc = new ProcTrim();
            proc.doWork(dt, out string sortingOrder, null, null, null);
            dgTable.Invoke(new MethodInvoker(() =>
            {
                addDataSourceValueChange(getDataSource(), dt);
            }));
            pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Blocks; }));
        }

        private void speichernToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgTable.DataSource != null)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = $"Excel Dateien|{excelExt}|Alle Dateien (*.*)|*.*";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    pgbLoading.Style = ProgressBarStyle.Marquee;
                    DataTable table = getDataSource(true);
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        ExportHelper.exportExcel(table, Path.GetDirectoryName(saveFileDialog1.FileName), Path.GetFileNameWithoutExtension(saveFileDialog1.FileName));
                        pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Blocks; }));
                    }).Start();
                }
            }
            //ExportHelper.exportExcel(dgTable);
        }

        private void spalteHinzufügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newColumn = Microsoft.VisualBasic.Interaction.InputBox("Bitte Spaltennamen eingeben", "Spalte hinzufügen", string.Empty);
            if (!string.IsNullOrWhiteSpace(newColumn))
            {
                DataTable data = getDataSource();
                data.Columns.Add(new DataColumn(newColumn));
                addDataSourceAddColumn(data.Columns.Count-1);
                assignDataSource(data);
                setRowCount();
            }
        }

        private void dgTable_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                selectedRow = dgTable.HitTest(e.X, e.Y).RowIndex;
                selectedColumn = dgTable.HitTest(e.X, e.Y).ColumnIndex;
                //Header
                if (selectedColumn >= 0 && selectedRow == -1)
                {
                    ctxHeader.Show(dgTable, new Point(e.X, e.Y));
                }
                else if (selectedColumn == -1)
                {
                    if (selectedRow > -1 && selectedRow != dgTable.Rows.Count - 1)
                    {
                        ctxRow.Show(dgTable, new Point(e.X, e.Y));
                    }
                }
                //body
                else
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

                DataTable table = getDataSource();
                table.Columns[selectedColumn].ColumnName = newText;
                
                addDataSourceHeaderChange(selectedColumn, oldText);
                
                assignDataSourceColumnChange(table, oldText, newText);
            }
        }

        private void spalteLöschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable table = getDataSource();
            DataColumn col = table.Columns[selectedColumn];
            
            
            addDataSourceDeleteColumn(col, selectedColumn, DataHelper.getColumnValues(table, selectedColumn));
            table.Columns.RemoveAt(selectedColumn);
            assignDataSourceColumnChange(table, col.ColumnName);
            setRowCount();
        }


        private void zeilenZusammenfügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Formula formula = new Formula(FormulaState.Merge, DataHelper.getHeadersOfDataTable(getDataSource()));
            formula.FormClosed += new FormClosedEventHandler(formulaClosed);
            formula.Show();

        }

        private void formulaClosed(object sender, FormClosedEventArgs e)
        {
            Formula formula = (Formula)sender;
            if (formula.DialogResult == DialogResult.OK)
            {
                DataTable data = getDataSource();
                List<string> notFoundColumns = new List<string>();
                ProcMerge proc = new ProcMerge(formula.getFormula());

                WorkflowHelper.checkHeaders(DataHelper.getHeadersToLower(data), notFoundColumns, proc.getHeaders());
                int column = data.Columns.Count;

                bool cont = true;
                if (notFoundColumns.Count > 0)
                {
                    SelectDuplicateColumns form = new SelectDuplicateColumns(notFoundColumns.ToArray(), DataHelper.getHeadersOfDataTable(data));
                    if( cont = (form.ShowDialog() == DialogResult.OK))
                    {
                        string[] from = form.Table.Rows.Cast<DataRow>().Select(row => row.ItemArray[0].ToString()).ToArray();
                        string[] to = form.Table.Rows.Cast<DataRow>().Select(row => row.ItemArray[1].ToString()).ToArray();
                        for(int i = 0; i < from.Length; i++)
                        {
                            proc.renameHeaders(from[i], to[i]);
                        }
                    }
                }
                if(cont)
                {
                    proc.doWork(data, out string sortingOrder, null, null, null);
                    assignDataSource(data);
                    addDataSourceAddColumn(column);
                }
            }
        }


        private void dBASEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgTable.DataSource != null)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = $"DBASE Dateien ({dbfExt})|{dbfExt}|Alle Dateien (*.*)|*.*";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    pgbLoading.BeginInvoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Marquee; }));
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        ExportHelper.exportDbase(Path.GetFileNameWithoutExtension(saveFileDialog1.FileName), getDataSource(true), Path.GetDirectoryName(saveFileDialog1.FileName));
                        pgbLoading.BeginInvoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Blocks; }));
                    }).Start();
                }
            }
        }

        private void dgTable_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            ViewHelper.addNumerationToDataGridView(sender, e, Font);
        }

        private void rückgängigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable oldTable = getDataSource();
            pgbLoading.Style = ProgressBarStyle.Marquee;

            new Thread(() =>
            {
                DataTable newTable = historyHelper.goBack(oldTable, getSorting());
                dgTable.Invoke(new MethodInvoker(() =>
                {
                    takeOverHistory(newTable, historyHelper.OrderString);
                }));
                pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Blocks; }));
            }).Start();
            
            
        }

        private void takeOverHistory(DataTable table, string orderString)
        {
            setSorting(orderString);
            assignDataSource(table);
            setRowCount();
        }

        private void wiederholenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable oldTable = getDataSource();
            pgbLoading.Style = ProgressBarStyle.Marquee;

            new Thread(() =>
            {
                DataTable newTable = historyHelper.repeat(getDataSource(), getSorting());
                dgTable.Invoke(new MethodInvoker(() =>
                {
                    takeOverHistory(newTable, historyHelper.OrderString);
                }));
                pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Blocks; }));
            }).Start();
        }

        private void dgTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            addDataSourceCellChanged(tableValueBefore, e.ColumnIndex, rowBefore);
        }

        private void dgTable_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            tableValueBefore = dgTable[e.ColumnIndex, e.RowIndex].Value.ToString();
            rowBefore = e.RowIndex >= 0 && e.RowIndex < sourceTable.Rows.Count ? getDataTableRowIndexOfDataGridView(e.RowIndex) : 0;
        }

        private void dgTable_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            setRowCount();
        }

        private void tabellenZusammenfügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(null, null, ImportState.Append);
        }

        private void verwaltungToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Administration form = new Administration(dgTable.DataSource != null ? DataHelper.getHeadersOfDataTable(getDataSource()) : new object[0], contextGlobal);
            form.FormClosed += new FormClosedEventHandler(administrationFormClosed);
            form.Show();
        }

        private void administrationFormClosed(object sender, FormClosedEventArgs e)
        {
            Administration admin = ((Administration)sender);
            loadProcedures(admin.Procedures);
            loadWorkflows(admin.Workflows);
            loadTolerances(admin.Tolerances);
            loadCases(admin.Cases);
        }

        private void cSVToolStripMenuItem_Click(object sender, EventArgs e, DataTable dt = null)
        {
            if (dgTable.DataSource != null)
            {
                DataTable table = dt ?? getDataSource(true);
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = $"CSV Dateien ({csvExt})|{csvExt}|Alle Dateien (*.*)|*.*";
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    pgbLoading.Style = ProgressBarStyle.Marquee;
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        ExportHelper.exportCsv(table, Path.GetDirectoryName(saveFileDialog1.FileName),  Path.GetFileNameWithoutExtension(saveFileDialog1.FileName));
                        pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Blocks; }));
                    }).Start();
                }
            }
        }

        private void postwurfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Formula formula = new Formula(FormulaState.Export, DataHelper.getHeadersOfDataTable(getDataSource()));
            formula.Text = "Bitte Spalten angeben";
            formula.FormClosed += new FormClosedEventHandler(saveCustomClosed);
            formula.Show();
        }

        private void saveCustomClosed(object sender, FormClosedEventArgs e)
        {
            if (((Formula)sender).DialogResult == DialogResult.OK)
            {
                DataTable table = getDataSource(true);
                HashSet<int> columns = new HashSet<int>(DataHelper.getHeaderIndices(table, ((Formula)sender).getSelectedHeaders()));

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
            ExportCustom formula = new ExportCustom(DataHelper.getHeadersOfDataTable(getDataSource()), true);
            formula.FormClosed += new FormClosedEventHandler(saveExportClosed);
            formula.Show();
        }

        private void saveExportClosed(object sender, FormClosedEventArgs e)
        {
            DataTable table = getDataSource(true);
            ExportCustom export = (ExportCustom)sender;
            if (export.DialogResult == DialogResult.OK)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    if (!table.Rows[i].ItemArray[export.getColumnIndex()].ToString().Equals(export.getValue()))
                    {
                        table.Rows.RemoveAt(i);
                        i--;
                    }
                }
                cSVToolStripMenuItem_Click(null, null, table);
            }
        }

        private void zählenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportCustom form = new ExportCustom(DataHelper.getHeadersOfDataTable(getDataSource()), false);
            form.FormClosed += new FormClosedEventHandler(CountContendClosed);
            form.Show();
        }

        private void CountContendClosed(object sender, FormClosedEventArgs e)
        {
            ExportCustom export = (ExportCustom)sender;
            if (export.DialogResult == DialogResult.OK)
            {
                DataTable table = ViewHelper.getSortedView($"[{export.getSelectedValue()}] asc", getDataSource()).ToTable();

                Dictionary<string, int> pair = new Dictionary<string, int>();
                foreach (DataRow row in table.Rows)
                {
                    string item = row.ItemArray[export.getColumnIndex()].ToString();
                    if (pair.ContainsKey(item))
                    {
                        pair[item] = pair[item] + 1;
                    }
                    else
                    {
                        pair.Add(item, 1);
                    }
                }

                Form1 form = new Form1(DataHelper.DictionaryToDataTable(pair, export.getSelectedValue()));
                form.Show();
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
                    dgTable.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = SortOrder.None;
                    resetSort(col);
                }
                else
                {
                    
                    bool asc = col.HeaderCell.SortGlyphDirection == SortOrder.Ascending;

                    setSorting($"[{col.Name}] {(asc ? "DESC" : "ASC")}");
                    assignDataSource();

                    dgTable.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = asc ? SortOrder.Descending : SortOrder.Ascending;
                }
            }
        }

        private void resetSort(DataGridViewColumn col)
        {
            setSorting(string.Empty);
            assignDataSource();
        }

        private void zeileLöschenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow oldRow = ((DataRowView)dgTable.Rows[selectedRow].DataBoundItem).Row;
            int tableIndex = ((DataView)dgTable.DataSource).Table.Rows.IndexOf(oldRow);

            object[] oldContent = oldRow.ItemArray.Clone() as object[];
            dgTable.Rows.RemoveAt(selectedRow);

            addDataSourceDeleteRow(oldContent, tableIndex);

            setRowCount();
        }

        private void zeileEinfügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable table = getDataSource();
            DataRow row = table.NewRow();
            int tableRowIndex = string.IsNullOrEmpty(SortingOrder) ? getDataTableRowIndexOfDataGridView(selectedRow) : table.Rows.Count;
            table.Rows.InsertAt(row, tableRowIndex);
            assignDataSource(table);

            addDataSourceAddRow(tableRowIndex);
        }

        private void tabelleHinzufügenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(null, null, ImportState.Append);
        }

        private void mergeTables(DataTable table)
        {
            DataTable originalTable = getDataSource();
            int ColumnIndexNew = originalTable.Columns.Count;
            int RowIndexNew = originalTable.Rows.Count;
            DataHelper.concatTables(originalTable, table);

            assignDataSource(originalTable);
            addDataSourceAddColumnAndRows(ColumnIndexNew, RowIndexNew);
        }

        private int getDataTableRowIndexOfDataGridView(int rowIndex)
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
            UpLowCaseForm form = new UpLowCaseForm(DataHelper.getHeadersOfDataTable(getDataSource()));
            if (form.ShowDialog() == DialogResult.OK) {
                List<WorkProc> list = new List<WorkProc>();
                list.Add(new ProcUpLowCase(form.getColumns(), form.allColumns(), form.getOption()));
                Work workflow = new Work(string.Empty, list, 0);
                workflow_Click(null, null, workflow);
            }
        }

        private void sortierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortForm form = new SortForm(DataHelper.getHeadersOfDataTable(getDataSource()), SortingOrder);
            if(form.ShowDialog() == DialogResult.OK)
            {
                setSorting(form.SortString);
                assignDataSource();
            }
        }
    }
}
