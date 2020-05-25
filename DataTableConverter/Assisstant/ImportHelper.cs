using DataTableConverter.Classes;
using DataTableConverter.Extensions;
using DataTableConverter.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace DataTableConverter.Assisstant
{
    class ImportHelper
    {

        internal static readonly string ProjectWorkflows = ExportHelper.ProjectWorkflows;
        internal static readonly string ProjectProcedures = ExportHelper.ProjectProcedures;
        internal static readonly string ProjectTolerances = ExportHelper.ProjectTolerance;
        internal static readonly string ProjectCases = ExportHelper.ProjectCases;
        internal static readonly string TextExt = "*.txt";
        internal static readonly string AccessExt = "*.accdb;*.accde;*.accdt;*.accdr;*.mdb";
        internal static readonly string DbfExt = "*.dbf";
        internal static readonly string CsvExt = "*.csv";
        internal static readonly string ExcelExt = "*.xlsx;*.xlsm;*.xlsb;*.xltx;*.xltm;*.xls;*.xlt;*.xls;*.xml;*.xml;*.xlam;*.xla;*.xlw;*.xlr;";
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)]
        string path,
            [MarshalAs(UnmanagedType.LPTStr)]
        StringBuilder shortPath,
            int shortPathLength);

        internal class KeyVal : IEquatable<KeyVal>
        {
            internal string Key;
            internal int Val;
            internal KeyVal(string key, int val)
            {
                Key = key;
                Val = val;
            }

            public bool Equals(KeyVal other)
            {
                return Key == other.Key && Val == other.Val;
            }

            public override string ToString()
            {
                return Key.ToString();
            }
        }

        internal static DataTable ImportFile(string file, bool multipleFiles, Dictionary<string, ImportSettings> fileImportSettings, ContextMenuStrip ctxRow, ProgressBar progressBar, Form mainForm, ref int fileEncoding, ImportSettings settings = null)
        {
            string filename = Path.GetFileName(file);
            string extension = Path.GetExtension(file).ToLower();
            DataTable table = null;
            

            if (extension == ".dbf")
            {
                table = OpenDBF(file, progressBar, mainForm);
            }
            else if (extension != string.Empty && AccessExt.Contains(extension))
            {
                table = OpenMSAccess(file, progressBar, mainForm);
            }
            else if (extension != string.Empty && ExcelExt.Contains(extension))
            {
                table = OpenExcel(file, progressBar, mainForm);
            }
            else
            {
                if (fileImportSettings != null && fileImportSettings.ContainsKey(extension) || settings != null)
                {
                    settings = settings ?? fileImportSettings[extension];

                    if (settings.Values != null)
                    {
                        table = OpenTextFixed(file, settings.Values, settings.Headers, settings.CodePage,false, progressBar, mainForm);
                    }
                    else if (settings.Separators.Count > 0)
                    {
                        table = OpenText(file, settings.Separators, settings.CodePage, settings.ContainsHeaders, settings.Headers.ToArray(), false, progressBar, mainForm);
                    }
                    else
                    {
                        table = OpenTextBetween(file, settings.CodePage, settings.TextBegin, settings.TextEnd, settings.ContainsHeaders, settings.Headers.ToArray(), false, progressBar, mainForm);
                    }

                }
                else
                {
                    TextFormat form = new TextFormat(file, multipleFiles, ctxRow);
                    DialogResult result = DialogResult.Cancel;
                    mainForm.Invoke(new MethodInvoker(() =>
                    {
                        result = form.ShowDialog(mainForm);
                    }));

                    if (result == DialogResult.OK)
                    {
                        if (form.TakeOver && fileImportSettings != null)
                        {
                            fileImportSettings.Add(extension, form.ImportSettings);
                        }
                        fileEncoding = form.ImportSettings.CodePage;
                        return ImportFile(file, multipleFiles, fileImportSettings, ctxRow, progressBar, mainForm, ref fileEncoding, form.ImportSettings);
                    }
                }
            }
            if(table != null)
            {
                CheckDataTableColumnHeader(table);
                table.RemoveEmptyRows();
                
                if (Properties.Settings.Default.TrimImport)
                {
                    table.Trim();
                }
            }
            return table;
        }

        internal static OpenFileDialog GetOpenFileDialog(bool status)
        {
            return new OpenFileDialog
            {
                Filter = $"Text-, CSV-, DBASE und Excel-Dateien ({CsvExt}, {TextExt}, {AccessExt}, {DbfExt}*.xl*)|{CsvExt};{TextExt}; {ExcelExt}; {DbfExt}; {AccessExt}"
                            + $"|Textdateien ({TextExt})|{TextExt}"
                            + $"|Access-Dateien ({AccessExt})|{AccessExt}"
                            + $"|CSV-Dateien ({CsvExt})|{CsvExt}"
                            + $"|dBase-Dateien ({DbfExt})|{DbfExt}"
                            + $"|Excel-Dateien (*.xl*)|{ExcelExt}"
                            + "|Alle Dateien (*.*)|*.*",
                RestoreDirectory = true,
                Multiselect = status
            };
        }

        internal static string[] LoadPresetsByName()
        {
            return Directory.GetFiles(ExportHelper.ProjectPresets, "*.bin")
                                         .Select(Path.GetFileNameWithoutExtension)
                                         .ToArray();
        }

        internal static string[] LoadHeaderPresetsByName()
        {
            return Directory.GetFiles(ExportHelper.ProjectHeaderPresets, "*.bin")
                                         .Select(Path.GetFileNameWithoutExtension)
                                         .ToArray();
        }

        internal static KeyVal[] LoadAllPresetsByName()
        {

            List<KeyVal> presets = new List<KeyVal>();
            foreach(string name in LoadHeaderPresetsByName())
            {
                presets.Add(new KeyVal(name,0));
            }
            foreach (string name in LoadPresetsByName())
            {
                presets.Add(new KeyVal(name, 1));
            }

            return presets.AsEnumerable().OrderBy(name => name.Key, new NaturalStringComparer(SortOrder.Ascending)).ThenBy(name => name.Val).ToArray();
        }

        internal static DataTable OpenText(string path, List<string> separators, int codePage, bool containsHeaders, object[] headers, bool isPreview, ProgressBar progressBar, Form mainForm)
        {
            DataTable dt = new DataTable();

            try
            {
                int skip = 0;
                if (containsHeaders)
                {
                    skip = 1;
                    IEnumerable<string> list = File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(1)
                    .SelectMany(x => x.Split(separators.ToArray(), StringSplitOptions.None))
                    .Select(ln => ln.Trim()).ToList();

                    foreach (string column in list)
                    {
                        dt.TryAddColumn((Properties.Settings.Default.ImportHeaderUpperCase ? column.ToUpper() : column).Trim());
                    }
                }
                else
                {
                    foreach (string column in headers)
                    {
                        dt.TryAddColumn(column);
                    }
                }


                if (isPreview)
                {
                    InsertTextIntoDataTable(File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(4), dt, skip, separators, null, mainForm);
                }
                else
                {
                    InsertTextIntoDataTable(File.ReadLines(path, Encoding.GetEncoding(codePage)), dt, skip, separators, progressBar, mainForm);
                }

                //File.ReadLines doesn't read all lines, it returns a IEnumerable, and lines are lazy evaluated,
                //  so just the first line will be loaded two times.
            }
            catch (IOException)
            {
                mainForm.MessagesOK(MessageBoxIcon.Warning, "Die Datei wird zurzeit von einem anderen Programm benutzt und kann nicht geöffnet werden.");
            }
            catch (ArgumentException)
            {
                mainForm.MessagesOK(MessageBoxIcon.Warning, "Die Zeile hat mehr Spalten als erlaubt");
                dt = null;
            }
            return dt;
        }

        private static void InsertTextIntoDataTable(IEnumerable<string> enumerable, DataTable dt, int skip, List<string> separators, ProgressBar progressBar, Form mainForm)
        {
            IEnumerable<string[]> enumerableArray = enumerable.Skip(skip)
                    .Select(x => x.Split(separators.ToArray(), StringSplitOptions.None));

            progressBar?.StartLoadingBar(enumerableArray.Count(), mainForm);
            enumerableArray.ToList()
                    .ForEach(line =>
                    {
                        var temp = line.Select(ln => ln.ToString().Trim()).ToArray();
                        int count = temp.Count();
                        while (count > dt.Columns.Count)
                        {
                            dt.TryAddColumn("Spalte" + dt.Columns.Count);
                        }
                        dt.Rows.Add(temp);
                        progressBar?.UpdateLoadingBar(mainForm);
                    });
        }

        internal static DataTable OpenTextBetween(string path, int codePage, string begin, string end, bool containsHeaders, object[] headers, bool isPreview, ProgressBar progressBar, Form mainForm)
        {
            DataTable dt = new DataTable();
            try
            {
                int skip = 0;
                if (containsHeaders)
                {
                    skip = 1;
                    string headerLine = File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(1).ToArray()[0].ToString();
                    string[] headerRow = createRow(headerLine, begin, end);

                    foreach (string field in headerRow)
                    {
                        dt.TryAddColumn((Properties.Settings.Default.ImportHeaderUpperCase ? field.ToUpper() : field).Trim());
                    }
                }
                else
                {
                    foreach (string column in headers)
                    {
                        dt.TryAddColumn(column);
                    }
                }

                string[] lines = new string[0];
                if (isPreview)
                {
                    lines = File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(4).Skip(skip).ToArray();
                }
                else
                {
                    var list = File.ReadLines(path, Encoding.GetEncoding(codePage)).Skip(skip);
                    lines = File.ReadLines(path, Encoding.GetEncoding(codePage)).Skip(skip).ToArray();
                }
                progressBar?.StartLoadingBar(lines.Length, mainForm);

                foreach(string line in lines)
                {
                    string[] row = createRow(line, begin, end);
                    while(row.Length > dt.Columns.Count)
                    {
                        dt.TryAddColumn("Spalte" + dt.Columns.Count);
                    }
                    progressBar?.UpdateLoadingBar(mainForm);
                    dt.Rows.Add(row);
                }
            }
            catch (IOException)
            {
                mainForm.MessagesOK(MessageBoxIcon.Warning, "Die Datei wird zurzeit von einem anderen Programm benutzt und kann nicht geöffnet werden.");
            }

            return dt;


            string[] createRow(string line, string beginText, string endText)
            {
                List<string> row = new List<string>();
                int lineLength = line.Length;
                int beginLength = beginText.Length;
                int endLength = endText.Length;
                bool finish = false;

                int pointer = 0;
                while (!finish)
                {
                    int indexBegin = line.IndexOf(beginText,pointer);
                    if(indexBegin != -1)
                    {
                        indexBegin += beginLength;

                        int indexEnd = line.IndexOf(endText, indexBegin);
                        if (indexEnd != -1)
                        {
                            row.Add(line.Substring(indexBegin, indexEnd - indexBegin).Trim());
                            pointer = indexEnd + endLength;
                        }
                        else
                        {
                            finish = true;
                        }
                    }
                    else
                    {
                        finish = true;
                    }

                }
                return row.ToArray();
            }
        }

        private static int RenameColumn(DataTable dt, string column, int counter)
        {
            try
            {
                dt.Columns[column].ColumnName = $"{column}{counter}";
                return counter;
            }
            catch (DuplicateNameException)
            {
                counter++;
                return RenameColumn(dt, column, counter);
            }
        }

        internal static DataTable OpenDBF(string path, ProgressBar progressBar, Form mainForm)
        {
            DataTable data = new DataTable();
            string directory = ToShortPathName(Path.GetDirectoryName(path));
            string shortPath = GetShortFileName(path);
            string constr = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={directory};Extended Properties=\"dBASE IV;CharacterSet={Encoding.Default.CodePage};\"";
            OleDbConnection con = new OleDbConnection(constr);

            var sql = $@"select * from [{ shortPath}]";
            con.Open();
            progressBar?.StartLoadingBar(GetDataReaderRowCount(con,shortPath, mainForm), mainForm);

            DataRowChangeEventHandler handler = (sender, e) => FillDataTableNewRow(e, progressBar, mainForm);
            data.RowChanged += handler;
            OleDbDataAdapter da = new OleDbDataAdapter(new OleDbCommand(sql, con));
            da.Fill(data);
            da.Dispose();
            data.RowChanged -= handler;
            data = data.Columns.Cast<DataColumn>().All(col => col.DataType == typeof(string)) ? data : data.SetColumnsTypeStringWithContainingData();
            data.AdjustDBASEImport();
            return data;
        }

        private static void FillDataTableNewRow(DataRowChangeEventArgs e, ProgressBar progressBar, Form mainForm)
        {
            if (e.Action == DataRowAction.Add)
            {
                progressBar?.UpdateLoadingBar(mainForm);
            }
        }

        private static int GetDataReaderRowCount(OleDbConnection con, string path, Form mainForm)
        {
            int count = 0;
            try
            {
                OleDbCommand cmd = new OleDbCommand($"SELECT count(*) FROM [{path}]", con);
                count = (int)cmd.ExecuteScalar();
                cmd.Dispose();
            }
            catch(Exception ex)
            {
                ErrorHelper.LogMessage(ex, mainForm, false);
            }
            
            return count;
        }

        private static int GetDataReaderTablesRowCount(OleDbConnection con, DataTable tables, Form mainForm)
        {
            int count = 0;
            foreach(DataRow row in tables.Rows)
            {
                if (row["TABLE_TYPE"].ToString() == "TABLE")
                {
                    string tableName = row["TABLE_NAME"].ToString();
                    count += GetDataReaderRowCount(con, tableName, mainForm);
                }
            }
            return count;
        }

        internal static DataTable OpenMSAccess(string path, ProgressBar progressBar, Form mainForm)
        {
            DataTable table = new DataTable();

            try
            {
                OleDbConnection con = new OleDbConnection
                {
                    ConnectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};"
                };
                con.Open();
                DataTable tables = con.GetSchema("Tables");
                progressBar?.StartLoadingBar(GetDataReaderTablesRowCount(con, tables, mainForm), mainForm);
                
                foreach (DataRow row in tables.Rows)
                {
                    if (row["TABLE_TYPE"].ToString() == "TABLE")
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        using (OleDbDataAdapter dbAdapter = new OleDbDataAdapter($"Select * from [{tableName}]", con))
                        {
                            string fileName = Path.GetFileName(path) + "; " + tableName;
                            DataTable temp = new DataTable();
                            temp.RowChanged += (sender, e) => FillDataTableNewRow(e, progressBar, mainForm);
                            dbAdapter.Fill(temp);
                            temp = temp.Columns.Cast<DataColumn>().All(col => col.DataType == typeof(string)) ? temp : temp.SetColumnsTypeStringWithContainingData();
                            table.ConcatTable(temp, fileName, fileName);
                        }
                    }
                }
                con.Close();
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, mainForm);
            }
            return table;
        }

        private static void CheckDataTableColumnHeader(DataTable table)
        {
            if (Properties.Settings.Default.ImportHeaderUpperCase)
            {
                foreach (DataColumn col in table.Columns)
                {
                    col.ColumnName = col.ColumnName.ToUpper();
                }
            }
        }

        #region Excel Import
        internal static DataTable OpenExcel(string path, ProgressBar progressBar, Form mainForm)
        {
            DataTable data = new DataTable();
            
            Microsoft.Office.Interop.Excel.Application objXL = null;
            Microsoft.Office.Interop.Excel.Workbook objWB = null;
            string clipboardBefore = string.Empty;
            try
            {
                clipboardBefore = Clipboard.GetText();
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, mainForm, false);
            }
            Clipboard.Clear();

            try
            {
                objXL = new Microsoft.Office.Interop.Excel.Application
                {
                    DisplayAlerts = false
                };
                bool hasPassword = false;
                string password = string.Empty;
                do
                {
                    try
                    {
                        objWB = objXL.Workbooks.Open(Filename: path, ReadOnly: true, Password: password);
                        hasPassword = false;
                    }
                    catch (COMException ex)
                    {
                        if (hasPassword = (ex.ErrorCode == -2146827284))
                        {
                            password = Microsoft.VisualBasic.Interaction.InputBox("Bitte Password eingeben", "Datei durch Passwort geschützt", string.Empty);
                            if (string.IsNullOrWhiteSpace(password))
                            {
                                return data;
                            }
                        }
                    }
                } while (hasPassword);

                string[] selectedSheets = SelectExcelSheets(objWB.Worksheets.Cast<Microsoft.Office.Interop.Excel.Worksheet>().Select(x => x.Name).ToArray(), mainForm);
                
                bool fileNameColumn;
                if(fileNameColumn = (data.Columns.IndexOf(Extensions.DataTableExtensions.FileName) == -1 && selectedSheets.Length > 1))
                {
                    data.Columns.Add(Extensions.DataTableExtensions.FileName, typeof(string));
                }

                RemoveTabsInCellsOfExcel(objXL);

                foreach (string sheetName in selectedSheets)
                {
                    Microsoft.Office.Interop.Excel.Worksheet objSHT = objWB.Worksheets[sheetName];
                    if (objSHT.AutoFilter != null)
                    {
                        objSHT.AutoFilter.ShowAllData();
                    }
                    int rows = objSHT.UsedRange.Rows.Count;
                    int cols = objSHT.UsedRange.Columns.Count;
                    while (cols > 0 && objSHT.Cells[1, cols].Text == string.Empty)
                    {
                        cols--;
                    }
                    if (cols == 0)
                    {
                        continue;
                    }

                    progressBar?.StartLoadingBar(rows, mainForm);

                    RangeToDataTable(objSHT, objXL, rows, cols, data, fileNameColumn ? Path.GetFileName(path) + "; " + sheetName : null, progressBar, mainForm);
                    Marshal.ReleaseComObject(objSHT);
                }
                if (fileNameColumn)
                {
                    data.Columns[Extensions.DataTableExtensions.FileName].SetOrdinal(data.Columns.Count - 1);
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, mainForm);
            }
            finally
            {
                if (objWB != null)
                {
                    objWB.Close(false);
                    Marshal.ReleaseComObject(objWB);
                }
                if (objXL != null)
                {
                    objXL.DisplayAlerts = true;
                    objXL.CutCopyMode = 0;
                    objXL.Quit();
                    Marshal.ReleaseComObject(objXL);
                    Marshal.FinalReleaseComObject(objXL);
                }

                if (!string.IsNullOrEmpty(clipboardBefore))
                {
                    //idk why but the Clipboard text is not set when I do it immediately
                    Thread thread = new Thread(() =>
                    {
                        Thread.Sleep(100);
                        Clipboard.SetText(clipboardBefore);
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
            }
            return data;
        }

        private static void RangeToDataTable(Microsoft.Office.Interop.Excel.Worksheet objSHT, Microsoft.Office.Interop.Excel.Application objXL, int rows, int cols, DataTable table, string fileName, ProgressBar progressBar, Form mainForm)
        {
            List<string> headers = SetHeaderOfExcel(table, objSHT, cols);
            Clipboard.Clear();
            objXL.CutCopyMode = 0;
            int rowRange = 50000;
            for(int i = 2; i <= rows; i++)
            {
                Microsoft.Office.Interop.Excel.Range c1 = objSHT.Cells[i, 1];
                int rowCount = i + rowRange;
                Microsoft.Office.Interop.Excel.Range c2 = objSHT.Cells[rowCount > rows ? rows : rowCount, cols];
                Microsoft.Office.Interop.Excel.Range range = objSHT.get_Range(c1, c2);
                range.Copy();
                IDataObject data = Clipboard.GetDataObject();
                string content = (string)data.GetData(DataFormats.UnicodeText);
                if(content != null)
                {
                    GetDataOfString(content, table, fileName, headers, progressBar, mainForm);
                }
                i = rowCount;
            }
        }

        private static void RemoveTabsInCellsOfExcel(Microsoft.Office.Interop.Excel.Application objXL)
        {
            objXL.Cells.Replace(What: "\t", Replacement: string.Empty, LookAt: Microsoft.Office.Interop.Excel.XlLookAt.xlPart,
                    SearchOrder: Microsoft.Office.Interop.Excel.XlSearchOrder.xlByRows
                    , MatchCase: false, SearchFormat: false, ReplaceFormat: false);
        }

        private static List<string> SetHeaderOfExcel(DataTable table, Microsoft.Office.Interop.Excel.Worksheet objSHT, int cols)
        {
            Microsoft.Office.Interop.Excel.Range c1 = objSHT.Cells[1, 1];
            Microsoft.Office.Interop.Excel.Range c2 = objSHT.Cells[1, cols];
            Microsoft.Office.Interop.Excel.Range range = objSHT.get_Range(c1, c2);
            range.Copy();
            IDataObject data = Clipboard.GetDataObject();
            string content = (string)data.GetData(DataFormats.UnicodeText);
            return GetHeadersOfContent(content, table);
        }

        private static void GetDataOfString(string content, DataTable table, string fileName, List<string> headers, ProgressBar progressBar, Form mainForm)
        {
            int maxLength = content.Length;
            StringBuilder cellBuilder = new StringBuilder();
            Dictionary<string,string> cells = new Dictionary<string, string>();
            
            int headerCounter = 0;
            DataRow row = table.NewRow();
            bool generatedMulti = false;
            
            for (int i = 0; i < maxLength; i++)
            {
                if (content[i] == '\r' && (i + 1) < maxLength && content[i + 1] == '\n') // new row
                {
                    progressBar?.UpdateLoadingBar(mainForm);
                    if (!generatedMulti)
                    {
                        SetContentRowValue(row, headers[headerCounter], cellBuilder);
                    }
                    generatedMulti = false;
                    headerCounter = 0;

                    AddContentDataRow(row, table, fileName);

                    row = table.NewRow();
                    
                    i++;
                    if ((i + 1) < maxLength && content[i + 1] == '\"') //beginning of cell that has text wrappings
                    {
                        i++;
                        generatedMulti = true;
                        GenerateMultiCell(content, table, row, headers[headerCounter], ref i);
                    }
                    else
                    {
                        cells.Clear();
                    }
                    
                }
                else if (content[i] == '\t') // new column
                {
                    if (!generatedMulti)
                    {
                        SetContentRowValue(row, headers[headerCounter], cellBuilder);
                    }
                    generatedMulti = false;
                    headerCounter++;

                    if ((i + 1) < maxLength && content[i + 1] == '\"') //beginning of cell that has text wrappings
                    {
                        i++;
                        generatedMulti = true;
                        GenerateMultiCell(content, table, row, headers[headerCounter], ref i);
                    }
                }
                else
                {
                    cellBuilder.Append(content[i]);
                }
            }
            SetContentRowValue(row, headers[headerCounter], cellBuilder);
            AddContentDataRow(row, table, fileName);
        }

        private static void AddContentDataRow(DataRow row, DataTable table, string fileName)
        {
            if (row.ItemArray.Any(cell => !string.IsNullOrWhiteSpace(cell.ToString())))
            {
                if (fileName != null)
                {
                    row[Extensions.DataTableExtensions.FileName] = fileName;
                }
                table.Rows.Add(row);
            }
        }

        private static void SetContentRowValue(DataRow row, string column, StringBuilder builder)
        {
            row[column] = builder.ToString();
            builder.Clear();
        }

        private static List<string> GetHeadersOfContent(string content, DataTable table)
        {
            List<string> headers = new List<string>();
            StringBuilder header = new StringBuilder();
            for(int i=0; i < content.Length; i++)
            {
                if(content[i] == '\r' && content[i+1] == '\n')
                {
                    AddHeaderOfContent(table, headers, header);
                    break;
                }
                else if(content[i] == '\t')
                {
                    AddHeaderOfContent(table, headers, header);
                }
                else
                {
                    header.Append(content[i]);
                }
            }
            return headers;
        }

        private static void AddHeaderOfContent(DataTable table, List<string> headers, StringBuilder header)
        {
            string headerString = header.ToString();
            int counter = 0;
            if (string.IsNullOrWhiteSpace(headerString))
            {
                headerString = "Spalte";
                counter = 1;
            }
            string newColumn = TryAddColumn(headers, headerString, counter);
            if (!table.Columns.Contains(newColumn))
            {
                table.Columns.Add(newColumn, typeof(string));
            }
            header.Clear();
        }

        private static string TryAddColumn(List<string> headers, string headerName, int counter = 0)
        {
            string result;
            string name = counter == 0 ? headerName : headerName + counter;
            if (headers.Contains(name))
            {
                counter++;
                result = TryAddColumn(headers, headerName, counter);
            }
            else
            {
                result = name;
                headers.Add(name);
            }
            return result;
        }

        private static void GenerateMultiCell(string content, DataTable table, DataRow row, string header, ref int i)
        {
            ++i;
            int multiCellCount = 0;
            StringBuilder cell = new StringBuilder();
            bool newLine;
            for (;i < content.Length; ++i)
            {
                if (EndOfMultiCell(content, i, out bool isNotMultiCell))
                {
                    if (isNotMultiCell)
                    {
                        i--;
                        cell = new StringBuilder("\"").Append(cell);
                    }
                    else if(multiCellCount == 0)
                    {
                        cell = new StringBuilder("\"").Append(cell).Append('\"');
                    }
                    AddMultiCellColumn(header, multiCellCount, table, row, cell);
                    break;
                }
                else if((newLine = (content[i] == '\r')) || content[i] == '\n')
                {
                    AddMultiCellColumn(header, multiCellCount, table, row,cell);

                    if (newLine)
                    {
                        ++i;
                    }
                    multiCellCount++;
                }
                else if (content[i] != '\"' || content[i-1] != '\"' || content[i + 1] == '\"') //when there is a " in a multiCell, then Excel writes \"\"
                {
                    cell.Append(content[i]);
                }
            }
        }

        private static void AddMultiCellColumn(string header, int count, DataTable table, DataRow row, StringBuilder text)
        {
            string result = text.ToString();
            text.Clear();
            if (result != string.Empty)
            {
                string multiHeader = count == 0 ? header : header + count;
                if (!table.Columns.Contains(multiHeader))
                {
                    int ordinal = table.Columns.IndexOf(header) + count;
                    table.Columns.Add(multiHeader, typeof(string)).SetOrdinal(ordinal >= table.Columns.Count ? table.Columns.Count-1 : ordinal);
                }
                row[multiHeader] = result;
            }
        }

        private static bool EndOfMultiCell(string content, int i, out bool isNotMultiCell)
        {
            int nextIndex = i + 1;
            return (isNotMultiCell = content[i] == '\t') || content[i] == '\"' && (nextIndex == content.Length || (nextIndex < content.Length && (content[nextIndex] == '\r' || content[nextIndex] == '\t')));
        }
        #endregion

        internal static DataTable OpenTextFixed(string path, List<int> config, List<string> header, int encoding, bool isPreview, ProgressBar progressBar, Form mainForm)
        {
            DataTable dt = new DataTable();
            if (header == null || config == null || header.Count == 0 || config.Count == 0)
            {
                return dt;
            }
            try
            {
                
                header.ForEach(x => dt.TryAddColumn(x.ToString()));

                StreamReader stream = new StreamReader(path, Encoding.GetEncoding(encoding));
                FileInfo info = new FileInfo(path);
                progressBar?.StartLoadingBar((int) (info.Length/config.Sum()), mainForm);


                while (!stream.EndOfStream && (!isPreview || dt.Rows.Count < 3))
                {
                    string[] itemArray = new string[header.Count];

                    for (int i = 0; i < config.Count && !stream.EndOfStream; i++)
                    {
                        char[] body = new char[config[i]];
                        
                        for(int index = 0; index < config[i] && stream.Peek() != -1; index++)
                        {
                            body[index] = (char)stream.Read();
                        }

                        itemArray[i] = new string(body);
                    }
                    progressBar?.UpdateLoadingBar(mainForm);
                    dt.Rows.Add(itemArray);

                    int charCode;
                    while ((charCode = stream.Peek()) == '\r' || charCode == '\n')
                    {
                        stream.Read(); //stream.BaseStream.seek(2,SeekOrigin.Current) does not work; \r\n is still being read even if stream.BaseStream.Position is changed
                    }
                }
                stream.Close();
            }
            catch (IOException)
            {
                mainForm.MessagesOK(MessageBoxIcon.Warning, "Die Datei wird zurzeit von einem anderen Programm verwendet und kann nicht geöffnet werden.");
            }
            return dt;
        }

        internal static List<Proc> LoadProcedures(Form mainForm)
        {
            List<Proc> data = new List<Proc>();
            if (File.Exists(ProjectProcedures))
            {
                try
                {
                    using (Stream stream = File.Open(ProjectProcedures, FileMode.Open))
                    {
                        BinaryFormatter bin = new BinaryFormatter();

                        data = (List<Proc>)bin.Deserialize(stream);
                    }
                }
                catch (IOException)
                {
                }
                if (!ExportHelper.SaveProcedures(data, mainForm))
                {
                    File.Delete(ProjectProcedures);
                }
            }
            else
            {
                string[] files = ExportHelper.GetProcedures();
                bool error = false;
                foreach (string file in files)
                {
                    try
                    {
                        using (Stream stream = File.Open(file, FileMode.Open))
                        {
                            BinaryFormatter bin = new BinaryFormatter();
                            if (bin.Deserialize(stream) is Proc proc)
                            {
                                data.Add(proc);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        error = true;
                        ErrorHelper.LogMessage(ex, mainForm, false);
                    }
                }
                if (error)
                {
                    mainForm.MessagesOK(MessageBoxIcon.Error, "Es konnten nicht alle Arbeitsabläufe geladen werden");
                }
            }
            return data;
        }

        internal static List<Work> LoadWorkflows(Form mainForm)
        {
            List<Work> data = new List<Work>();
            if (File.Exists(ProjectWorkflows))
            {
                try
                {
                    using (Stream stream = File.Open(ProjectWorkflows, FileMode.Open))
                    {
                        BinaryFormatter bin = new BinaryFormatter();

                        data = (List<Work>)bin.Deserialize(stream);
                    }
                }
                catch (IOException)
                {
                }
                if (!ExportHelper.SaveWorkflows(data, mainForm))
                {
                    File.Delete(ProjectWorkflows);
                }
            }
            else
            {
                string[] files = ExportHelper.GetWorkflows();
                bool error = false;
                foreach(string file in files)
                {
                    try
                    {
                        using (Stream stream = File.Open(file, FileMode.Open))
                        {
                            BinaryFormatter bin = new BinaryFormatter();
                            if (bin.Deserialize(stream) is Work work)
                            {
                                data.Add(work);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        error = true;
                        ErrorHelper.LogMessage(ex, mainForm, false);
                    }
                }
                if (error)
                {
                    mainForm.MessagesOK(MessageBoxIcon.Error, "Es konnten nicht alle Arbeitsabläufe geladen werden");
                }
            }
            return data;
        }

        internal static Work LoadWorkflow(string path)
        {
            Work result = null;
            using (Stream stream = File.Open(path, FileMode.Open))
            {
                BinaryFormatter bin = new BinaryFormatter();
                result = bin.Deserialize(stream) as Work;
            }
            return result;
        }

        internal static Proc LoadProcedure(string path)
        {
            Proc result = null;
            using(Stream stream = File.Open(path, FileMode.Open))
            {
                BinaryFormatter bin = new BinaryFormatter();
                result = bin.Deserialize(stream) as Proc;
            }
            return result;
        }

        internal static List<Tolerance> LoadTolerances()
        {
            List<Tolerance> data = new List<Tolerance>();
            if (File.Exists(ProjectTolerances))
            {
                try
                {
                    using (Stream stream = File.Open(ProjectTolerances, FileMode.Open))
                    {
                        BinaryFormatter bin = new BinaryFormatter();

                        data = (List<Tolerance>)bin.Deserialize(stream);
                    }
                }
                catch (IOException)
                {
                }
            }
            return data;
        }

        internal static List<Case> LoadCases()
        {
            List<Case> data = new List<Case>();
            if (File.Exists(ProjectTolerances))
            {
                try
                {
                    using (Stream stream = File.Open(ProjectCases, FileMode.Open))
                    {
                        BinaryFormatter bin = new BinaryFormatter();

                        data = (List<Case>)bin.Deserialize(stream);
                    }
                }
                catch (IOException)
                {
                }
            }
            return data;
        }

        internal static TextImportTemplate LoadTextImportTemplate(string path)
        {
            TextImportTemplate data = null;
            if (File.Exists(path))
            {
                try
                {
                    using (Stream stream = File.Open(path, FileMode.Open))
                    {
                        BinaryFormatter bin = new BinaryFormatter();

                        data = (TextImportTemplate)bin.Deserialize(stream);
                    }
                }
                catch (IOException)
                {
                }
            }
            return data;
        }

        private static string[] SelectExcelSheets(string[] sheets, Form mainForm)
        {
            string[] checkedSheets = new string[0];
            if (sheets.Length == 1)
            {
                return sheets;
            }
            else
            {
                ExcelSheets form = new ExcelSheets(sheets);
                DialogResult result = DialogResult.Cancel;
                mainForm.Invoke(new MethodInvoker(() =>
                {
                    result = form.ShowDialog(mainForm);
                }));
                if (result == DialogResult.OK)
                {
                    checkedSheets = form.GetSheets();
                }
                form.Dispose();
                return checkedSheets;
            }
        }

        internal static ImportSettings GenerateSettingsThroughPreset(int presetType, string settingPreset)
        {
            ImportSettings setting = null;
            if (presetType == 0)
            {
                TextImportTemplate template = LoadTextImportTemplate(Path.Combine(ExportHelper.ProjectHeaderPresets, $"{settingPreset}.bin"));
                if (template != null)
                {
                    switch (template.SelectedSeparated)
                    {
                        case TextImportTemplate.SelectedSeparatedState.Between:
                            setting = new ImportSettings(template.Encoding, template.BeginSeparator, template.EndSeparator, template.ContainsHeaders, template.Table.ColumnValues(0));
                            break;

                        case TextImportTemplate.SelectedSeparatedState.Tab:
                            setting = new ImportSettings(new List<string> { "\t" }, template.Encoding, template.ContainsHeaders, template.Table.ColumnValues(0));
                            break;

                        case TextImportTemplate.SelectedSeparatedState.TabCharacter:
                        default:
                            setting = new ImportSettings(template.Separators, template.Encoding, template.ContainsHeaders, template.Table.ColumnValues(0));
                            break;
                    }
                }
            }
            else
            {
                TextImportTemplate template = LoadTextImportTemplate(Path.Combine(ExportHelper.ProjectPresets, $"{settingPreset}.bin"));
                if (template != null)
                {
                    setting = new ImportSettings(template.Table.ColumnValuesAsInt(0).ToList(), template.Table.ColumnValuesAsString(1).ToList(), template.Encoding);
                }
            }
            return setting;
        }

        public static string ToShortPathName(string longName)
        {
            int bufferSize = 256;
            // don´t allocate stringbuilder here but outside of the function for fast access
            StringBuilder shortNameBuffer = new StringBuilder(bufferSize);
            GetShortPathName(longName, shortNameBuffer, bufferSize);
            return shortNameBuffer.ToString();
        }

        internal static string GetShortFileName(string path)
        {
            StringBuilder temp = new StringBuilder(255);

            GetShortPathName(path, temp, 255);

            return temp.ToString().Split('\\').Last().ToLower();
        }
        private string LongFileName(string shortName)
        {
            return new FileInfo(shortName).FullName;
        }

    }
}
