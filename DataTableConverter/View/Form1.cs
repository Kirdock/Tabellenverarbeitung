using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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
        private Dictionary<string, SortOrder> DataGridOrders;
        private string tempOrder;

        internal Form1(DataTable table = null)
        {
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


        private void assignDataSource(DataTable table)
        {
            DataView view = table.DefaultView;
            view.Sort = tempOrder ?? getSorting();
            tempOrder = null;

            saveDataGridSortMode();

            dgTable.DataSource = null;
            dgTable.DataSource = view;

            restoreDataGridSortMode();
        }

        private void saveDataGridSortMode()
        {
            DataGridOrders = new Dictionary<string, SortOrder>();
            foreach (DataGridViewColumn col in dgTable.Columns)
            {
                DataGridOrders.Add(col.HeaderText, col.HeaderCell.SortGlyphDirection);
            }
        }

        private void restoreDataGridSortMode()
        {

            for (int i = 0; i < dgTable.Columns.Count; i++)
            {
                string key = dgTable.Columns[i].HeaderText;
                dgTable.Columns[i].SortMode = DataGridViewColumnSortMode.Programmatic;
                if (DataGridOrders.ContainsKey(key))
                {
                    dgTable.Columns[i].HeaderCell.SortGlyphDirection = DataGridOrders[key];
                }
            }
            DataGridOrders = null;
        }

        private void assignDataSourceColumnChange(DataTable table, string column, string newColumn = null)
        {
            DataView view = table.DefaultView;
            string adjustSort = getSorting();
            saveDataGridSortMode();
            if (newColumn == null)
            {
                int indexFrom = adjustSort.IndexOf($"[{column}]");
                if (indexFrom != -1)
                {
                    int indexTo = adjustSort.IndexOf(",", column.Length + 2 + indexFrom);
                    adjustSort = adjustSort.Remove(indexFrom, indexTo == -1 ? adjustSort.Length : (indexTo - indexFrom + 2)); //+2 weil nach "," noch ein Leerzeichen ist
                }
            }
            else
            {
                adjustSort = adjustSort.Replace($"[{column}]", $"[{newColumn}]");
            }
            view.Sort = adjustSort;
            setDataView(view);
            restoreDataGridSortMode();
        }

        private string getSorting()
        {
            return dgTable.DataSource == null ? string.Empty : getDataView().Sort;
        }

        private DataTable getDataSource(bool withSort = false)
        {
            if (dgTable.DataSource == null)
            {
                return null;
            }
            else
            {
                DataView view = getDataView();
                return withSort ? view.ToTable() : view.Table.Copy();
            }
        }

        private DataView getDataView()
        {
            dgTable.BindingContext[dgTable.DataSource].EndCurrentEdit();
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
            List <string> headers = DataHelper.getHeadersToLower(originalTable);
            bool contains = true;
            List<string> columns = new List<string>();
            string[] caseColumns = caseColumnsWorkflow ?? cas.getColumnsAsArray();
            caseColumns = caseColumns.Select(cs => cs.ToLower()).ToArray();
            for (int i = 0; i < caseColumns.Length; i++)
            {
                if (!headers.Contains(caseColumns[i]))
                {
                    contains = false;
                    break;
                }
                else
                {
                    columns.Add(originalTable.Columns[i].ColumnName);
                }
            }

            if (contains)
            {
                duplicateClosed(null, null, cas, originalTable, columns.ToArray(), caseColumnsWorkflow != null);
            }
            else
            {
                SelectDuplicateColumns form = new SelectDuplicateColumns(cas.getColumnsAsArray(), DataHelper.getHeadersOfDataTable(originalTable));
                form.FormClosed += (sender2, e2) => duplicateClosed(sender2, e2, cas, originalTable, null, caseColumnsWorkflow != null);
                form.Show();
            }
        }

        private void duplicateClosed (object sender, FormClosedEventArgs e, Case cas, DataTable originalTable, string[] Col, bool startedThroughWorkflow)
        {
            Hashtable hTable = new Hashtable();
            ArrayList duplicateList = new ArrayList();

            string[] columns = Col ?? ((SelectDuplicateColumns)sender).Table.Rows.Cast<DataRow>().Select(row => row.ItemArray[1].ToString()).ToArray();
            int[] subStringBegin = cas.getBeginSubstring();
            int[] subStringEnd = cas.getEndSubstring();
            DataTable oldTable = originalTable.Copy();
            int lastIndex = originalTable.Columns.IndexOf("Duplikat");
            bool columnAdded;
            if (columnAdded = lastIndex == -1)
            {
                lastIndex = originalTable.Columns.Count;
                DataHelper.addColumn("Duplikat", originalTable);

            }
            


            for (int index = 0; index < originalTable.Rows.Count; index++)
            {
                string identifier = getColumnsAsObjectArray(originalTable.Rows[index], columns,subStringBegin, subStringEnd);
                
                if (hTable.Contains(identifier))
                {
                    originalTable.Rows[(int)hTable[identifier]].SetField(lastIndex, cas.Shortcut);
                    originalTable.Rows[index].SetField(lastIndex, cas.Shortcut);
                }
                else
                {
                    hTable.Add(identifier, index);
                }
            }
            if (!startedThroughWorkflow)
            {
                if (columnAdded)
                {
                    addDataSourceAddColumn(lastIndex);
                }
                else
                {
                    addDataSourceColumnValuesChanged(oldTable, originalTable, lastIndex);
                }
                assignDataSource(originalTable);
            }
        }

        private string getColumnsAsObjectArray(DataRow row, string[] columns, int[] subStringBegin, int[] subStringEnd)
        {
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < columns.Length; i++)
            {
                #region Set Tolerances
                StringBuilder result = new StringBuilder(row[columns[i]].ToString().ToLower());
                foreach (Tolerance tol in tolerances)
                {
                    foreach (string t in tol.getColumnsAsArrayToLower())
                    {
                        result.Replace(t, tol.Name);
                    }
                }
                #endregion

                string resultString = result.ToString();

                #region Set Substring
                int begin = subStringBegin[i];
                int end = subStringEnd[i];
                if ( begin != 0 && end != 0 && end >= begin)
                {
                    if (begin - 1 > resultString.Length)
                    {
                        resultString = string.Empty;
                    }
                    else
                    {
                        int count = end - begin + 1;
                        if (begin + count > resultString.Length)
                        {
                            count = resultString.Length - begin + 1;
                        }
                        resultString = resultString.Substring(begin - 1, count);
                    }
                }
                #endregion

                res.Append("|").Append(resultString);
            }
            return res.ToString();
        }


        private void workflow_Click(object sender, EventArgs e, Work workflow)
        {
            List<NotFoundHeaders> notFound = new List<NotFoundHeaders>();
            
            List<TempReplaceProcedure> tempReplace = new List<TempReplaceProcedure>();
            List<string> headers = DataHelper.getHeadersToLower(getDataSource());
            DataTable table = getDataSource();
            foreach (WorkProc wp in workflow.Procedures)
            {
                List<string> notFoundColumns = new List<string>();
                string[] wpHeaders = new string[0];
                switch (wp.Type) {
                    case ProcedureState.Order:
                    case ProcedureState.User:
                        checkHeaders(headers, notFoundColumns, out string[] col, wp.Columns.Rows.Cast<DataRow>().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null).ToArray());
                        wpHeaders = col;
                        if (!string.IsNullOrWhiteSpace(wp.NewColumn))
                        {
                            headers.Add(wp.NewColumn.ToLower());
                        }
                        break;

                    case ProcedureState.Duplicate:
                        checkHeaders(headers, notFoundColumns, out string[] col2, wp.DuplicateColumns);
                        wpHeaders = col2;
                        headers.Add("duplikat");
                        break;

                    case ProcedureState.Merge:
                        checkMergeHeaders(headers, wp.Formula, notFoundColumns, out string[] col3);
                        wpHeaders = col3;
                        headers.Add(wp.NewColumn.ToLower());
                        break;

                    //Bei Trim ist nichts
                }
                wp.Headers = wpHeaders;
                if(notFoundColumns.Count > 0)
                {
                    notFound.Add(new NotFoundHeaders(notFoundColumns, wp, tempReplace.Count));
                }
                tempReplace.Add(new TempReplaceProcedure(wpHeaders, wp));
            }
            if (notFound.Count == 0)
            {
                replaceThroughTemp(tempReplace);
            }
            else
            {
                //New Form
                //NotFoundHeader = newHeader (combobox)
                //DialogResult res = MessageHandler.MessagesYesNo(MessageBoxIcon.Warning, $"Es wurden folgende Spaltennamen nicht gefunden: {notFoundColumns}\nTrotzdem ausführen?");
                //if(res == DialogResult.Yes)
                //{
                //    replaceThroughTemp(tempReplace);
                //}
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
                        for(int y = 0; y < nf.Wp.Headers.Length; y++)
                        {
                            for (int i = 0; i < from.Length; i++)
                            {
                                if (nf.Wp.Headers[y] == from[i] && nf.Headers.Contains(from[i])) //Kann sein, dass eine Spalte hinzugefügt wird und sie bei manchen Valid und bei manchen inValid ist, je nachdem wann sie ausgeführt werden
                                {

                                    switch (nf.Wp.Type)
                                    {
                                        case ProcedureState.Order:
                                        case ProcedureState.User:
                                            foreach(DataRow row in nf.Wp.Columns.Rows)
                                            {
                                                if(row.ItemArray[0].ToString() == from[i])
                                                {
                                                    row.SetField(0, to[i]);
                                                }
                                            }
                                            break;

                                        case ProcedureState.Duplicate:
                                            for(int x = 0; x < nf.Wp.DuplicateColumns.Length; x++)
                                            {
                                                if (nf.Wp.DuplicateColumns[x] == from[i])
                                                {
                                                    nf.Wp.DuplicateColumns[x] = to[i];
                                                }
                                            }
                                            break;

                                        case ProcedureState.Merge:
                                            nf.Wp.Formula = nf.Wp.Formula.Replace($"[{from[i]}]", $"[{to[i]}]");
                                            break;


                                    }


                                    nf.Wp.Headers[y] = to[i];
                                }
                            }
                        }
                        //tempReplace[nf.Index].workProc = nf.Wp; //brauch ich das? hab ja ne Referenz...
                    }
                    replaceThroughTemp(tempReplace);
                }
            }
        }

        private void checkHeaders(List<string> tableHeader, List<string> notFoundColumns, out string[] columns, string[] headers)
        {
            columns = new string[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                string headerText = headers[i];

                if (headerText == null)
                {
                    continue;
                }
                columns[i] = headerText;
                if (!tableHeader.Contains(headerText.ToLower()))
                {
                    notFoundColumns.Add(headerText);
                }
            }
        }

        private void replaceThroughTemp(List<TempReplaceProcedure> temp)
        {
            DataTable table = getDataSource();
            pgbLoading.Style = ProgressBarStyle.Marquee;
            new Thread(() =>
            {
                foreach (TempReplaceProcedure t in temp)
                {
                    TempReplaceProcedure temporary = t;
                    replaceProcedure(table, null, temporary.Columns, null, temporary.workProc);
                }
                pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Blocks; }));
                dgTable.Invoke(new MethodInvoker(() => {
                    addDataSourceValueChange(getDataSource(), table);
                }));
            }).Start();
        }

        private int getProcedureThroughId(int id)
        {
            int index = -1;
            try
            {
                index = procedures.FindIndex(p => p.Id == id);
            }
            catch { }
            return index;
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
                    //addDataSourceAddColumn(oldCount + i);  //sollte noch geändert werden
                }
                if (newColumn)
                {
                    DataHelper.addColumn(form.getOrderColumnName(), sourceTable);
                    //addDataSourceAddColumn(newColumnIndex); //sollte noch geändert werden
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
                        if (source.ItemArray[oldIndex].Equals(row.ItemArray[mergeIndex]))
                        {
                            for (int i = 0; i < columns.Length; i++)
                            {
                                source.SetField(oldCount + i, row.ItemArray[i]);
                            }
                            if (newColumn)
                            {
                                source.SetField(newColumnIndex, y.ToString());
                            }
                            hs.Add(rowIndex);
                            break;
                        }
                    }
                }
                #endregion

                dgTable.Invoke(new MethodInvoker(() => { addDataSourceValueChange(tableOld, sourceTable); }));
                pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Blocks; }));
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
                new Thread(() => trimDataTable(getDataSource(), true)).Start();
            }
        }

        private void procedure_Click(object sender, EventArgs e, Proc procedure)
        {
            Formula formula = new Formula(FormulaState.Procedure, DataHelper.getHeadersOfDataTable(getDataSource()));
            formula.Text = "Bitte Spalten angeben";
            formula.FormClosed += (sender2, e2) => procedureClosed(sender2, e2, procedure);
            formula.Show();
        }

        private void replaceProcedure(DataTable table, Proc procedure, string[] columns, string columnHeader, WorkProc wp = null)
        {
            bool intoNewCol = false;
            int lastCol = table.Columns.Count;
            ProcedureState type = wp == null ? ProcedureState.User : wp.Type;

            switch (type) {
                case ProcedureState.Trim:
                    trimDataTable(table, false);
                    break;

                case ProcedureState.Order:
                    tempOrder = buildOrder(wp.Columns);
                    break;

                case ProcedureState.Merge:
                    mergeColumns(wp.NewColumn,wp.Formula, table, columns);
                    break;

                case ProcedureState.Duplicate:
                    Case cas = getCaseThroughId(wp.ProcedureId);
                    string[] tempCol = cas.getColumnsAsArray();
                    if (tempCol.Length < wp.DuplicateColumns.Length)
                    {
                        wp.DuplicateColumns = wp.DuplicateColumns.Take(tempCol.Length).ToArray();
                    }
                    else if(tempCol.Length > wp.DuplicateColumns.Length)
                    {
                        List<string> list = new List<string>();
                        list.AddRange(wp.DuplicateColumns);
                        list.AddRange(tempCol.Skip(wp.DuplicateColumns.Length).ToArray());
                        wp.DuplicateColumns = list.ToArray();
                    }
                    
                    case_Click(null, null, cas, wp.DuplicateColumns, table);
                    break;

                case ProcedureState.User:
                    if(procedure == null)
                    {
                        procedure = procedures[getProcedureThroughId(wp.ProcedureId)];
                    }
                    DataTable replaces = procedure.Replace;
                    if (!string.IsNullOrWhiteSpace(columnHeader))
                    {
                        table.Columns.Add(columnHeader);
                        intoNewCol = true;
                    }
                    List<int> headerIndices = DataHelper.getHeaderIndices(table, columns);
                    foreach (DataRow rep in replaces.Rows)
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            for (int i = 0; i < row.ItemArray.Length; i++)
                            {
                                
                                if ((columns == null || headerIndices.Contains(i)) && rep.ItemArray[0].ToString().Length > 0)
                                {
                                    int index = intoNewCol ? lastCol : i;
                                    row.SetField(index, row.ItemArray[i].ToString().Replace(rep.ItemArray[0].ToString(), rep.ItemArray[1].ToString()));
                                }
                            }
                        }
                    }
                    break;
            }
            if (wp == null) //Started through Workflow
            {
                if (intoNewCol)
                {
                    addDataSourceAddColumn(lastCol);
                    assignDataSource(table);
                }
                else
                {
                    addDataSourceValueChange(getDataSource(), table);
                }
            }
        }

        private string buildOrder(DataTable table)
        {
            StringBuilder builder = new StringBuilder();
            
            foreach(DataRow row in table.Rows)
            {
                object col = row[0];
                bool orderDESC = string.IsNullOrWhiteSpace(row[1]?.ToString()) ? false : (bool)row[1];
                builder.Append("[").Append(col.ToString()).Append("] ").Append(orderDESC ? "DESC" : "ASC").Append(", ");
            }
            string result = builder.ToString();
            if(result.Length > 2)
            {
                result = result.Substring(0, builder.Length - 2);
            }
            return result;
        }


        private Case getCaseThroughId(int id)
        {
            return cases[cases.FindIndex(cs => cs.Id == id)];
        }

        private void procedureClosed(object sender, FormClosedEventArgs e, Proc procedure)
        {
            if (((Formula)sender).DialogResult == DialogResult.OK)
            {
                string[] columns = ((Formula)sender).getSelectedHeaders();
                
                if (columns.Length > 0)
                {
                    replaceProcedure(getDataSource(),procedure, columns, ((Formula)sender).getHeaderName());
                }
            }
        }

        private void trimDataTable(DataTable dt, bool addHistory)
        {
            Thread.CurrentThread.IsBackground = true;
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    row.SetField(i, row.ItemArray[i].ToString().Trim());
                }
            }
            foreach(DataColumn col in dt.Columns)
            {
                col.ColumnName = col.ColumnName.Trim();
            }
            if (addHistory)
            {
                dgTable.Invoke(new MethodInvoker(() =>
                {
                    addDataSourceValueChange(getDataSource(), dt);
                }));
                pgbLoading.Invoke(new MethodInvoker(() => { pgbLoading.Style = ProgressBarStyle.Blocks; }));
            }
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
                string formu = formula.getFormula();
                checkMergeHeaders(DataHelper.getHeadersToLower(data), formu, notFoundColumns, out string[] headers);
                if (notFoundColumns.Count > 0)
                {
                    SelectDuplicateColumns form = new SelectDuplicateColumns(notFoundColumns.ToArray(), DataHelper.getHeadersOfDataTable(data));
                    if(form.ShowDialog() == DialogResult.OK)
                    {
                        string[] from = form.Table.Rows.Cast<DataRow>().Select(row => row.ItemArray[0].ToString()).ToArray();
                        string[] to = form.Table.Rows.Cast<DataRow>().Select(row => row.ItemArray[1].ToString()).ToArray();
                        for(int i = 0; i < from.Length; i++)
                        {
                            formu.Replace($"[{from[i]}]", $"[{to[i]}]");
                        }


                        int column = mergeColumns(formula.getHeaderName(), formu, data, headers);
                        assignDataSource(data);
                        addDataSourceAddColumn(column);
                    }
                }
                else
                {
                    int column = mergeColumns(formula.getHeaderName(), formu, data, headers);
                    assignDataSource(data);
                    addDataSourceAddColumn(column);
                }
            }
        }

        private void checkMergeHeaders(List<string> tableHeaders, string form, List<string> notFoundColumns, out string[] headers)
        {
            string regularExpressionPattern = @"\[(.*?)\]";
            Regex re = new Regex(regularExpressionPattern);

            MatchCollection matches = re.Matches(form);
            //string []columns = new string[matches.Count];
            headers = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                headers[i] = matches[i].Value.Substring(1, matches[i].Value.Length - 2);
            }
            
            checkHeaders(tableHeaders, notFoundColumns, out string[] columns, headers);
        }

        private int mergeColumns(string headerName, string form, DataTable data, string[] headers)
        {
            int column = data.Columns.Count;
            DataHelper.addColumn(headerName, data);

            headers = headers.Select(h => h.ToLower()).ToArray();
            
            List<string> tableHeaders = DataHelper.getHeadersToLower(data);
            

            for (int rowIndex = 0; rowIndex < data.Rows.Count; rowIndex++)
            {
                DataRow row = data.Rows[rowIndex];
                string format = "";

                int counter = headers.Length - 1;

                bool skipWhenEmpty = false;
                for (int i = form.Length - 1; i >= 0; i--)
                {
                    string header = headers[counter];
                    int index = tableHeaders.IndexOf(header.ToLower());
                    char c = form[i];

                    if (c != ']')
                    {
                        if (skipWhenEmpty)
                        {
                            continue;
                        }
                        else
                        {
                            format = c + format;
                        }
                    }
                    else
                    {
                        skipWhenEmpty = string.IsNullOrWhiteSpace(row[index]?.ToString());
                        if (!skipWhenEmpty)
                        {
                            format = row[index] + format;
                        }

                        i -= (header.Length + 1);
                        counter--;
                    }
                }

                data.Rows[rowIndex].SetField(column, format);
            }
            return column;
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
            DataTable table = historyHelper.goBack(getDataSource(), getSorting());
            takeOverHistory(table, historyHelper.OrderString);
        }

        private void takeOverHistory(DataTable table, string orderString)
        {
            tempOrder = orderString;
            assignDataSource(table);
            setRowCount();
        }

        private void wiederholenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable table = historyHelper.repeat(getDataSource(), getSorting());
            takeOverHistory(table, historyHelper.OrderString);
        }

        private void dgTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            addDataSourceCellChanged(tableValueBefore, e.ColumnIndex, getDataTableRowIndexOfDataGridView(e.RowIndex));
        }

        private void dgTable_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            tableValueBefore = dgTable[e.ColumnIndex, e.RowIndex].Value.ToString();
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
                DataTable table = getDataSource();
                DataView view = table.DefaultView;
                view.Sort = $"[{export.getSelectedValue()}] asc";
                table = view.ToTable();
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
                    resetSort(col);
                }
                else
                {
                    
                    bool asc = col.HeaderCell.SortGlyphDirection == SortOrder.Ascending;
                    DataView view = getDataView();

                    view.Sort = $"[{col.Name}] {(asc ? "DESC" : "ASC")}";
                    col.HeaderCell.SortGlyphDirection = asc ? SortOrder.Descending : SortOrder.Ascending;
                }
            }
        }

        private void resetSort(DataGridViewColumn col)
        {
            getDataView().Sort = string.Empty;
            col.HeaderCell.SortGlyphDirection = SortOrder.None;
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
            
            int tableRowIndex = getDataTableRowIndexOfDataGridView(selectedRow);
            DataTable table = getDataSource();
            DataRow row = table.NewRow();
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
            return ((DataView)dgTable.DataSource).Table.Rows.IndexOf(oldRow);
        }

        private void überschriftenEinlesenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(null, null, ImportState.Header);
        }

        private void sortierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine(getDataView().Sort);
            SortForm form = new SortForm(DataHelper.getHeadersOfDataTable(getDataSource()), getDataView().Sort);
            if(form.ShowDialog() == DialogResult.OK)
            {
                DataView view = getDataView();
                view.Sort = form.SortString;
                setDataView(view);
            }
        }

        private void setDataView(DataView view)
        {
            List<SortOrder> orders = new List<SortOrder>();
            foreach (DataGridViewColumn col in dgTable.Columns)
            {
                orders.Add(col.HeaderCell.SortGlyphDirection);
            }
            dgTable.DataSource = null;
            dgTable.DataSource = view;
            dgTable.Columns.Cast<DataGridViewColumn>().ToList().ForEach(column => {
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
            });
        }
    }
}
