using DataTableConverter.Classes;
using DataTableConverter.Extensions;
using DataTableConverter.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
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

        internal readonly string ProjectWorkflows;
        internal readonly string ProjectProcedures;
        internal readonly string ProjectTolerances;
        internal readonly string ProjectCases;
        internal readonly int PreviewRows = 4;
        internal readonly string TextExt = "*.txt";
        internal readonly string AccessExt = "*.accdb;*.accde;*.accdt;*.accdr;*.mdb";
        internal readonly string DbfExt = "*.dbf";
        internal readonly string CsvExt = "*.csv";
        internal readonly string ExcelExt = "*.xlsx;*.xlsm;*.xlsb;*.xltx;*.xltm;*.xls;*.xlt;*.xls;*.xml;*.xml;*.xlam;*.xla;*.xlw;*.xlr;";
        private readonly DatabaseHelper DatabaseHelper;
        private readonly ExportHelper ExportHelper;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)]
        string path,
            [MarshalAs(UnmanagedType.LPTStr)]
        StringBuilder shortPath,
            int shortPathLength);

        internal ImportHelper(ExportHelper exportHelper, DatabaseHelper databaseHelper)
        {
            ExportHelper = exportHelper;
            DatabaseHelper = databaseHelper;
            ProjectWorkflows = ExportHelper.ProjectWorkflows;
            ProjectProcedures = ExportHelper.ProjectProcedures;
            ProjectTolerances = ExportHelper.ProjectTolerance;
            ProjectCases = ExportHelper.ProjectCases;
    }

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

        internal string ImportFile(string file, bool multipleFiles, Dictionary<string, ImportSettings> fileImportSettings, ContextMenuStrip ctxRow, ProgressBar progressBar, Form mainForm, ref int fileEncoding, ImportSettings settings = null)
        {
            string filename = Path.GetFileName(file);
            string extension = Path.GetExtension(file).ToLower();
            string tableName = Guid.NewGuid().ToString();

            if (extension == ".dbf")
            {
                OpenDBF(file, progressBar, mainForm, tableName);
            }
            else if (extension != string.Empty && AccessExt.Contains(extension))
            {
                OpenMSAccess(file, progressBar, mainForm, ref tableName);
            }
            else if (extension != string.Empty && ExcelExt.Contains(extension))
            {
                OpenExcel(file, progressBar, mainForm, tableName);
            }
            else
            {
                if (fileImportSettings != null && fileImportSettings.ContainsKey(extension) || settings != null)
                {
                    settings = settings ?? fileImportSettings[extension];

                    if (settings.Values != null)
                    {
                        OpenTextFixed(tableName, file, settings.Values, settings.Headers, settings.CodePage,false, progressBar, mainForm);
                    }
                    else if (settings.Separators.Count > 0)
                    {
                        OpenText(tableName, file, settings.Separators, settings.CodePage, settings.ContainsHeaders, settings.Headers.ToArray(), false, progressBar, mainForm);
                    }
                    else
                    {
                        OpenTextBetween(tableName, file, settings.CodePage, settings.TextBegin, settings.TextEnd, settings.ContainsHeaders, settings.Headers.ToArray(), false, progressBar, mainForm);
                    }
                }
                else
                {
                    TextFormat form = new TextFormat(DatabaseHelper, this, ExportHelper, tableName, file, multipleFiles, ctxRow);
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
                    else
                    {
                        DatabaseHelper.Delete(tableName, true);
                        tableName = null;
                    }
                }
            }
            return tableName;
        }

        internal OpenFileDialog GetOpenFileDialog(bool multiselect)
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
                Multiselect = multiselect
            };
        }

        internal string[] LoadPresetsByName()
        {
            return Directory.GetFiles(ExportHelper.ProjectPresets, "*.bin")
                                         .Select(Path.GetFileNameWithoutExtension)
                                         .ToArray();
        }

        internal string[] LoadHeaderPresetsByName()
        {
            return Directory.GetFiles(ExportHelper.ProjectHeaderPresets, "*.bin")
                                         .Select(Path.GetFileNameWithoutExtension)
                                         .ToArray();
        }

        internal KeyVal[] LoadAllPresetsByName()
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

        internal void OpenText(string tableName, string path, List<string> separators, int codePage, bool containsHeaders, object[] headers, bool isPreview, ProgressBar progressBar, Form mainForm)
        {

            try
            {
                int skip = 0;
                List<string> newHeaders;
                if (containsHeaders)
                {
                    skip = 1;
                    newHeaders = File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(1)
                        .SelectMany(x => x.Split(separators.ToArray(), StringSplitOptions.None))
                        .Select(column => (Properties.Settings.Default.ImportHeaderUpperCase ? column.ToUpper() : column).Trim()).ToList();

                    DatabaseHelper.CreateTable(newHeaders, tableName);
                }
                else
                {
                    newHeaders = headers.Cast<string>().ToList();
                    DatabaseHelper.CreateTable(newHeaders, tableName);
                }

                IEnumerable<string> eLines = File.ReadLines(path, Encoding.GetEncoding(codePage));
                if (isPreview)
                {
                    eLines = eLines.Take(PreviewRows);
                }

                InsertTextIntoDataTable(eLines, tableName, skip, separators, newHeaders, progressBar, mainForm);

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
            }
        }

        private void InsertTextIntoDataTable(IEnumerable<string> enumerable, string tableName, int skip, List<string> separators, List<string> headers, ProgressBar progressBar, Form mainForm)
        {
            IEnumerable<string[]> enumerableArray = enumerable.Skip(skip)
                    .Select(x => x.Split(separators.ToArray(), StringSplitOptions.None));

            progressBar?.StartLoadingBar(enumerableArray.Count(), mainForm);
            foreach(string[] line in enumerableArray)
            {
                string[] values = line.Select(ln => ln.Trim()).ToArray();

                while (values.Length > headers.Count)
                {
                    string colName = "Spalte" + headers.Count;
                    
                    DatabaseHelper.AddColumn(tableName, colName); //have to check if exists
                    //colName = DatabaseHelper.AddColumnsWithAdditionalIfExists()  //adjust it for this case
                    headers.Add(colName);
                }
                DatabaseHelper.InsertRow(headers, values, tableName);
                progressBar?.UpdateLoadingBar(mainForm);
            }
        }

        internal void OpenTextBetween(string tableName, string path, int codePage, string begin, string end, bool containsHeaders, object[] headers, bool isPreview, ProgressBar progressBar, Form mainForm)
        {
            try
            {
                int skip = 0;
                List<string> newHeaders;
                if (containsHeaders)
                {
                    skip = 1;
                    string headerLine = File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(1).ToArray()[0].ToString();
                    newHeaders = createRow(headerLine, begin, end).Select(field => (Properties.Settings.Default.ImportHeaderUpperCase ? field.ToUpper() : field).Trim()).ToList();
                    DatabaseHelper.CreateTable(newHeaders, tableName);
                }
                else
                {
                    newHeaders = headers.Cast<string>().ToList();
                    DatabaseHelper.CreateTable(newHeaders, tableName);
                }

                IEnumerable<string> eLines = File.ReadLines(path, Encoding.GetEncoding(codePage));
                if (isPreview)
                {
                    eLines = eLines.Take(PreviewRows);
                }

                string[] lines = eLines.Skip(skip).ToArray();
                progressBar?.StartLoadingBar(lines.Length, mainForm);

                foreach(string line in lines)
                {
                    string[] values = createRow(line, begin, end);
                    while(values.Length > newHeaders.Count)
                    {
                        string colName = "Spalte" + newHeaders.Count;
                        DatabaseHelper.AddColumn(tableName, colName);
                        newHeaders.Add(colName);
                    }
                    progressBar?.UpdateLoadingBar(mainForm);
                    DatabaseHelper.InsertRow(newHeaders, values, tableName);
                }
            }
            catch (IOException)
            {
                mainForm.MessagesOK(MessageBoxIcon.Warning, "Die Datei wird zurzeit von einem anderen Programm benutzt und kann nicht geöffnet werden.");
            }


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

        private int RenameColumn(DataTable dt, string column, int counter)
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

        internal void OpenDBF(string path, ProgressBar progressBar, Form mainForm, string tableName)
        {
            string directory = ToShortPathName(Path.GetDirectoryName(path));
            string shortPath = GetShortFileName(path);
            string constr = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={directory};Extended Properties=\"dBASE IV;CharacterSet={Encoding.Default.CodePage};\"";
            using (OleDbConnection con = new OleDbConnection(constr))
            {
                con.Open();
                progressBar?.StartLoadingBar(GetDataReaderRowCount(con, shortPath, mainForm), mainForm);

                using (OleDbCommand command = con.CreateCommand())
                {
                    command.CommandText = $@"select * from [{ shortPath}]";
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        string[] columnNames = new string[reader.FieldCount];
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            columnNames[i] = reader.GetName(i);
                        }
                        DatabaseHelper.CreateTable(columnNames, tableName);
                        while (reader.Read())
                        {
                            object[] values = new object[reader.FieldCount];
                            for (int i = 0; i < reader.FieldCount; ++i)
                            {
                                values[i] = reader.GetString(i).Replace("\n", string.Empty); //remove new lines in dbase
                            }
                            DatabaseHelper.InsertRow(columnNames, values, tableName);
                        }
                    }
                }
            }
        }

        private void FillDataTableNewRow(DataRowChangeEventArgs e, ProgressBar progressBar, Form mainForm)
        {
            if (e.Action == DataRowAction.Add)
            {
                progressBar?.UpdateLoadingBar(mainForm);
            }
        }

        private int GetDataReaderRowCount(OleDbConnection con, string path, Form mainForm)
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

        private int GetDataReaderTablesRowCount(OleDbConnection con, DataTable tables, Form mainForm)
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

        internal string OpenMSAccess(string path, ProgressBar progressBar, Form mainForm, ref string tableName)
        {
            tableName = Guid.NewGuid().ToString();
            try
            {
                OleDbConnection con = new OleDbConnection
                {
                    ConnectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};"
                };
                con.Open();
                DataTable tables = con.GetSchema("Tables");
                progressBar?.StartLoadingBar(GetDataReaderTablesRowCount(con, tables, mainForm), mainForm);
                bool tableCreated = false;
                List<string> allColumnNames = new List<string>();
                
                foreach (DataRow row in tables.Rows)
                {
                    if (row["TABLE_TYPE"].ToString() == "TABLE")
                    {
                        string accessTableName = row["TABLE_NAME"].ToString();
                        using (OleDbCommand command = con.CreateCommand())
                        {
                            command.CommandText = $"Select * from [{accessTableName}]";
                            using (OleDbDataReader reader = command.ExecuteReader())
                            {
                                List<string> newColumnNames = new List<string>();
                                string[] columnNames = new string[reader.FieldCount];
                                for(int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    if (!allColumnNames.Any(col => col.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        newColumnNames.Add(columnName);
                                    }
                                    columnNames[i] = columnName;
                                }
                                if (!tableCreated)
                                {
                                    DatabaseHelper.CreateTable(newColumnNames, tableName);
                                    tableCreated = true;
                                }
                                else if(newColumnNames.Count != 0)
                                {
                                    foreach(string columnName in newColumnNames)
                                    {
                                        DatabaseHelper.AddColumnFixedAlias(columnName, tableName);
                                    }
                                }

                                SQLiteCommand liteCommand = null;
                                while (reader.Read())
                                {
                                    object[] values = new object[reader.FieldCount];
                                    for(int i = 0; i < reader.FieldCount; ++i)
                                    {
                                        values[0] = reader.GetString(i);
                                    }
                                    liteCommand = DatabaseHelper.InsertRow(columnNames, values, tableName, liteCommand);
                                    progressBar?.UpdateLoadingBar(mainForm);
                                }
                            }
                        }
                    }
                }
                con.Close();
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, mainForm);
                DatabaseHelper.Delete(tableName, true);
                tableName = null;
            }
            return tableName;
        }

        private void CheckDataTableColumnHeader(DataTable table)
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
        internal void OpenExcel(string path, ProgressBar progressBar, Form mainForm, string tableName)
        {
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
                                return;
                            }
                        }
                    }
                } while (hasPassword);

                string[] selectedSheets = SelectExcelSheets(objWB.Worksheets.Cast<Microsoft.Office.Interop.Excel.Worksheet>().Select(x => x.Name).ToArray(), mainForm);
                
                bool fileNameColumn;
                List<string> headers = new List<string>();
                if(fileNameColumn = (selectedSheets.Length > 1))
                {
                    headers.Add(Extensions.DataTableExtensions.FileName);
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
                    int cols = objSHT.Cells.Find("*", System.Reflection.Missing.Value,
                                                   System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                                                   Microsoft.Office.Interop.Excel.XlSearchOrder.xlByColumns, Microsoft.Office.Interop.Excel.XlSearchDirection.xlPrevious,
                                                   false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Column;
                    if (cols == 0)
                    {
                        continue;
                    }

                    progressBar?.StartLoadingBar(rows, mainForm);
                    UnhideRowsAndColumns(objSHT);
                    RangeToDataTable(objSHT, objXL, rows, cols, tableName, fileNameColumn ? Path.GetFileName(path) + "; " + sheetName : null, progressBar, mainForm);
                    Marshal.ReleaseComObject(objSHT);
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
        }

        private void UnhideRowsAndColumns(Microsoft.Office.Interop.Excel.Worksheet objSHT)
        {
            objSHT.Columns.EntireColumn.Hidden = false;
            objSHT.Rows.EntireRow.Hidden = false;
        }

        private void RangeToDataTable(Microsoft.Office.Interop.Excel.Worksheet objSHT, Microsoft.Office.Interop.Excel.Application objXL, int rows, int cols, string tableName, string fileName, ProgressBar progressBar, Form mainForm)
        {
            
            List<string> headers = SetHeaderOfExcel(objSHT, cols);
            DatabaseHelper.CreateTable(headers.ToArray(), tableName);
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
                if (content != null)
                {
                    GetDataOfString(content, tableName, fileName, headers, progressBar, mainForm);
                }
                i = rowCount;
            }
        }

        private void RemoveTabsInCellsOfExcel(Microsoft.Office.Interop.Excel.Application objXL)
        {
            objXL.Cells.Replace(What: "\t", Replacement: string.Empty, LookAt: Microsoft.Office.Interop.Excel.XlLookAt.xlPart,
                    SearchOrder: Microsoft.Office.Interop.Excel.XlSearchOrder.xlByRows
                    , MatchCase: false, SearchFormat: false, ReplaceFormat: false);
        }

        private List<string> SetHeaderOfExcel(Microsoft.Office.Interop.Excel.Worksheet objSHT, int cols)
        {
            Microsoft.Office.Interop.Excel.Range c1 = objSHT.Cells[1, 1];
            Microsoft.Office.Interop.Excel.Range c2 = objSHT.Cells[1, cols];
            Microsoft.Office.Interop.Excel.Range range = objSHT.get_Range(c1, c2);
            range.Copy();
            IDataObject data = Clipboard.GetDataObject();
            string content = (string)data.GetData(DataFormats.UnicodeText);
            return GetHeadersOfContent(content);
        }

        private void GetDataOfString(string content, string tableName, string fileName, List<string> headers, ProgressBar progressBar, Form mainForm)
        {
            int maxLength = content.Length;
            StringBuilder cellBuilder = new StringBuilder();
            Dictionary<string,string> cells = new Dictionary<string, string>();
            
            int headerCounter = 0;
            Dictionary<string, string> row = new Dictionary<string, string>(); //column, value pair
            bool generatedMulti = false;
            SQLiteCommand insertCommand = null;
            
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

                    insertCommand = AddContentDataRow(row, fileName, tableName, insertCommand);

                    row.Clear();
                    
                    i++;
                    if ((i + 1) < maxLength && content[i + 1] == '\"') //beginning of cell that has text wrappings
                    {
                        i++;
                        generatedMulti = true;
                        bool addedColumns = GenerateMultiCell(content, tableName, row, headers[headerCounter], ref i);
                        if (addedColumns)
                        {
                            insertCommand = null;
                        }
                    }
                    else
                    {
                        cells.Clear();
                    }
                    
                }
                else if (content[i] == '\t') // new column
                {
                    if (!generatedMulti && cellBuilder.Length > 0)
                    {
                        SetContentRowValue(row, headers[headerCounter], cellBuilder);
                    }
                    generatedMulti = false;
                    headerCounter++;

                    if ((i + 1) < maxLength && content[i + 1] == '\"') //beginning of cell that has text wrappings
                    {
                        i++;
                        generatedMulti = true;
                        bool addedColumns = GenerateMultiCell(content, tableName, row, headers[headerCounter], ref i);
                        if (addedColumns)
                        {
                            insertCommand = null;
                        }
                    }
                }
                else
                {
                    cellBuilder.Append(content[i]);
                }
            }
            SetContentRowValue(row, headers[headerCounter], cellBuilder);
            AddContentDataRow(row, tableName, fileName, insertCommand);
        }

        private SQLiteCommand AddContentDataRow(Dictionary<string,string> row, string fileName, string tableName, SQLiteCommand command)
        {
            if (row.Values.Any(value => !string.IsNullOrWhiteSpace(value.ToString()))) //Request: delete empty rows
            {
                if (fileName != null)
                {
                    row.Add(Extensions.DataTableExtensions.FileName, fileName);
                }
                command = DatabaseHelper.InsertRow(row, tableName, command);
            }
            return command;
        }

        private void SetContentRowValue(Dictionary<string, string> row, string column, StringBuilder builder)
        {
            row.Add(column, builder.ToString());
            builder.Clear();
        }

        private List<string> GetHeadersOfContent(string content)
        {
            List<string> headers = new List<string>();
            StringBuilder header = new StringBuilder();
            for(int i=0; i < content.Length; i++)
            {
                if(content[i] == '\r' && content[i+1] == '\n')
                {
                    AddHeaderOfContent(headers, header);
                    break;
                }
                else if(content[i] == '\t')
                {
                    AddHeaderOfContent(headers, header);
                }
                else
                {
                    header.Append(content[i]);
                }
            }
            return headers;
        }

        private void AddHeaderOfContent(List<string> headers, StringBuilder header)
        {
            string headerString = header.ToString();
            int counter = 0;
            if (string.IsNullOrWhiteSpace(headerString))
            {
                headerString = "Spalte";
                counter = 1;
            }
            TryAddColumn(headers, headerString, counter);
            header.Clear();
        }

        private string TryAddColumn(List<string> headers, string headerName, int counter = 0)
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

        private bool GenerateMultiCell(string content, string tableName, Dictionary<string, string> row, string header, ref int i)
        {
            ++i;
            int multiCellCount = 0;
            StringBuilder cell = new StringBuilder();
            bool newLine;
            bool addedColumn = false;
            for (;i < content.Length; ++i)
            {
                if (EndOfMultiCell(content, i, out bool isNotMultiCell))
                {
                    if (isNotMultiCell)
                    {
                        i--;
                        cell = new StringBuilder("\"").Append(cell);
                    }
                    else if(multiCellCount == 0 && cell.Length > 0)
                    {
                        cell = new StringBuilder("\"").Append(cell).Append('\"');
                    }
                    addedColumn |= AddMultiCellColumn(header, multiCellCount, tableName, row, cell);
                    break;
                }
                else if((newLine = (content[i] == '\r')) || content[i] == '\n')
                {
                    if (cell.Length != 0)
                    {
                        addedColumn |= AddMultiCellColumn(header, multiCellCount, tableName, row, cell);

                        if (newLine)
                        {
                            ++i;
                        }
                        multiCellCount++;
                    }
                }
                else if (content[i] != '\"' || content[i-1] != '\"' || content[i + 1] == '\"') //when there is a " in a multiCell, then Excel writes \"\"
                {
                    cell.Append(content[i]);
                }
            }
            return addedColumn;
        }

        private bool AddMultiCellColumn(string header, int count, string tableName, Dictionary<string, string> row, StringBuilder text)
        {
            string result = text.ToString();
            bool addedCollumn = false;
            text.Clear();
            if (result != string.Empty)
            {
                string multiHeader = count == 0 ? header : header + count;
                if (!DatabaseHelper.ContainsAlias(tableName, multiHeader))
                {
                    DatabaseHelper.AddColumn(tableName, multiHeader);
                    addedCollumn = true;
                }
                row.Add(multiHeader, result);
            }
            return addedCollumn;
        }

        private bool EndOfMultiCell(string content, int i, out bool isNotMultiCell)
        {
            int nextIndex = i + 1;
            return (isNotMultiCell = content[i] == '\t') || content[i] == '\"' && (nextIndex == content.Length || (nextIndex < content.Length && (content[nextIndex] == '\r' || content[nextIndex] == '\t')));
        }
        #endregion

        internal void OpenTextFixed(string tableName, string path, List<int> config, List<string> header, int encoding, bool isPreview, ProgressBar progressBar, Form mainForm)
        {
            if (header == null || config == null || header.Count == 0 || config.Count == 0)
            {
                return;
            }
            try
            {
                DatabaseHelper.CreateTable(header.ToArray(), tableName);

                

                StreamReader stream = new StreamReader(path, Encoding.GetEncoding(encoding));
                FileInfo info = new FileInfo(path);
                progressBar?.StartLoadingBar((int) (info.Length/config.Sum()), mainForm);

                long rowCount = 0;
                while (!stream.EndOfStream && (!isPreview || rowCount < 3))
                {
                    Dictionary<string, string> row = new Dictionary<string, string>();
                    //string[] itemArray = new string[header.Count];

                    for (int i = 0; i < config.Count && !stream.EndOfStream; i++)
                    {
                        char[] body = new char[config[i]];
                        
                        for(int index = 0; index < config[i] && stream.Peek() != -1; index++)
                        {
                            body[index] = (char)stream.Read();
                        }
                        row.Add(header[i], new string(body));
                    }
                    progressBar?.UpdateLoadingBar(mainForm);
                    DatabaseHelper.InsertRow(row, tableName);
                    rowCount++;
                    
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
        }

        internal List<Proc> LoadProcedures(Form mainForm)
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

        internal List<Work> LoadWorkflows(Form mainForm)
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

        internal Work LoadWorkflow(string path)
        {
            Work result = null;
            using (Stream stream = File.Open(path, FileMode.Open))
            {
                BinaryFormatter bin = new BinaryFormatter();
                result = bin.Deserialize(stream) as Work;
            }
            return result;
        }

        internal Proc LoadProcedure(string path)
        {
            Proc result = null;
            using(Stream stream = File.Open(path, FileMode.Open))
            {
                BinaryFormatter bin = new BinaryFormatter();
                result = bin.Deserialize(stream) as Proc;
            }
            return result;
        }

        internal List<Tolerance> LoadTolerances()
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

        internal List<Case> LoadCases()
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

        internal TextImportTemplate LoadTextImportTemplate(string path)
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

        private string[] SelectExcelSheets(string[] sheets, Form mainForm)
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

        internal ImportSettings GenerateSettingsThroughPreset(int presetType, string settingPreset)
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

        public string ToShortPathName(string longName)
        {
            int bufferSize = 256;
            // don´t allocate stringbuilder here but outside of the function for fast access
            StringBuilder shortNameBuffer = new StringBuilder(bufferSize);
            GetShortPathName(longName, shortNameBuffer, bufferSize);
            return shortNameBuffer.ToString();
        }

        internal string GetShortFileName(string path)
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
