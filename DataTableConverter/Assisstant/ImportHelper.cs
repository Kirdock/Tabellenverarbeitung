using DataTableConverter.Classes;
using DataTableConverter.Classes.WorkProcs;
using DataTableConverter.Extensions;
using DataTableConverter.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

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
        internal static readonly int MaxCellsPerIteration = 50000;
        private readonly DatabaseHelper DatabaseHelper;
        private readonly ExportHelper ExportHelper;
        private static readonly XNamespace ExcelNamespace = "urn:schemas-microsoft-com:office:spreadsheet";
        private static readonly XName RowExcelNamespace = ExcelNamespace + "Row";
        private static readonly XName CellExcelNamespace = ExcelNamespace + "Cell";
        private static readonly XName IndexNamexpace = ExcelNamespace + "Index";

        [DllImport("kernel32.dll", EntryPoint = "GetShortPathName", CharSet = CharSet.Auto)]
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

        internal string ImportFile(string file, bool multipleFiles, Dictionary<string, ImportSettings> fileImportSettings, ContextMenuStrip ctxRow, ProgressBar progressBar, Form mainForm, ref int fileEncoding, ref string password, ImportSettings settings = null)
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
                OpenExcel(file, progressBar, mainForm, tableName, ref password);
            }
            else
            {
                if (fileImportSettings != null && fileImportSettings.ContainsKey(extension) || settings != null)
                {
                    settings = settings ?? fileImportSettings[extension];

                    if (settings.Values != null)
                    {
                        OpenTextFixed(tableName, file, settings.Values, settings.Headers, settings.CodePage, false, settings.HasRowBreaks, progressBar, mainForm);
                    }
                    else if (settings.Separators?.Count > 0)
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
                    TextFormat form = new TextFormat(DatabaseHelper, this, ExportHelper, tableName, file, multipleFiles, ctxRow, fileImportSettings.Count != 0);
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
                        return ImportFile(file, multipleFiles, fileImportSettings, ctxRow, progressBar, mainForm, ref fileEncoding, ref password, form.ImportSettings);
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
                Multiselect = multiselect,
                CheckFileExists = false,
                CheckPathExists = false
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
            foreach (string name in LoadHeaderPresetsByName())
            {
                presets.Add(new KeyVal(name, 0));
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
                Func<string, string> importOperation = DatabaseHelper.ImportOperation();
                List<string> newHeaders;
                if (containsHeaders)
                {
                    skip = 1;
                    newHeaders = File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(1)
                        .SelectMany(x => x.Split(separators.ToArray(), StringSplitOptions.None))
                        .Select(column => importOperation(column)).ToList();
                    var indexItem = newHeaders.Select((item, index) => new { item, index });
                    for (int i = 0; i < newHeaders.Count; ++i)
                    {
                        var duplicates = indexItem.Where(element => element.index != i && element.item.Equals(newHeaders[i], StringComparison.OrdinalIgnoreCase)).ToArray();
                        for(int y = 0; y < duplicates.Length; ++y)
                        {
                            int counter = y + 2;
                            string newValue;
                            do
                            {
                                newValue = duplicates[y].item + counter;
                                ++counter;
                            } while (newHeaders.Contains(newValue));

                            newHeaders[duplicates[y].index] = newValue;
                        }
                    }

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
            Func<string, string> trimOperation = GetTrimOperation();
            IEnumerable<string[]> enumerableArray = enumerable.Skip(skip)
                    .Select(x => x.Split(separators.ToArray(), StringSplitOptions.None));

            progressBar?.StartLoadingBar(enumerableArray.Count(), mainForm);
            SQLiteCommand insertCommand = null;
            foreach (string[] line in enumerableArray)
            {
                string[] values = line.Select(ln => trimOperation(ln)).ToArray();
                bool addedColumns = values.Length > headers.Count;
                while (values.Length > headers.Count)
                {
                    string colName;
                    int counter = 2;
                    do
                    {
                        colName = "Spalte" + counter;
                        ++counter;
                    } while (headers.Contains(colName));

                    colName = DatabaseHelper.AddColumn(tableName, colName);
                    headers.Add(colName);
                    insertCommand = null;
                }

                insertCommand = DatabaseHelper.InsertRow(headers, values, tableName, insertCommand);
                if (addedColumns)
                {
                    insertCommand = null;
                }
                progressBar?.UpdateLoadingBar(mainForm);
            }
        }

        internal void OpenTextBetween(string tableName, string path, int codePage, string begin, string end, bool containsHeaders, object[] headers, bool isPreview, ProgressBar progressBar, Form mainForm)
        {
            Func<string, string> trimOperation = GetTrimOperation();
            try
            {
                int skip = 0;
                List<string> newHeaders;
                if (containsHeaders)
                {
                    skip = 1;
                    string headerLine = File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(1).ToArray()[0].ToString();
                    newHeaders = createRow(headerLine, begin, end).Select(field => trimOperation(field)).ToList();
                    DatabaseHelper.CreateTable(newHeaders, tableName);
                }
                else
                {
                    newHeaders = headers.Cast<string>().ToList();
                    DatabaseHelper.CreateTable(newHeaders, tableName);
                }
                if (newHeaders.Count != 0)
                {
                    SQLiteCommand insertCommand = null;
                    IEnumerable<string> eLines = File.ReadLines(path, Encoding.GetEncoding(codePage));
                    if (isPreview)
                    {
                        eLines = eLines.Take(PreviewRows);
                    }

                    string[] lines = eLines.Skip(skip).ToArray();
                    progressBar?.StartLoadingBar(lines.Length, mainForm);

                    foreach (string line in lines)
                    {
                        string[] values = createRow(line, begin, end);
                        while (values.Length > newHeaders.Count)
                        {
                            string colName = "Spalte" + newHeaders.Count;
                            colName = DatabaseHelper.AddColumn(tableName, colName);
                            newHeaders.Add(colName);
                        }
                        progressBar?.UpdateLoadingBar(mainForm);
                        insertCommand = DatabaseHelper.InsertRow(values.Length < newHeaders.Count ? newHeaders.Take(values.Length) : newHeaders, values, tableName, insertCommand);
                    }
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
                    int indexBegin = line.IndexOf(beginText, pointer);
                    if (indexBegin != -1)
                    {
                        indexBegin += beginLength;

                        int indexEnd = line.IndexOf(endText, indexBegin);
                        if (indexEnd != -1)
                        {
                            row.Add(trimOperation(line.Substring(indexBegin, indexEnd - indexBegin)));
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

        internal bool OpenTextFixed(string tableName, string path, List<int> config, List<string> header, int encoding, bool isPreview, bool hasRowBreaks, ProgressBar progressBar, Form mainForm)
        {
            if (header == null || config == null || header.Count == 0 || config.Count == 0)
            {
                return false;
            }
            try
            {
                DatabaseHelper.CreateTable(header, tableName);
                Func<string, string> trimOperation = GetTrimOperation();
                SQLiteCommand insertCommand = null;
                if (hasRowBreaks)
                {
                    IEnumerable<string> eLines = File.ReadLines(path, Encoding.GetEncoding(encoding));
                    if (isPreview)
                    {
                        eLines = eLines.Take(3);
                    }
                    progressBar?.StartLoadingBar(eLines.Count(), mainForm);
                    int countBefore = 0;
                    foreach (string line in eLines)
                    {
                        Dictionary<string, string> row = new Dictionary<string, string>();
                        int index = 0;
                        for (int i = 0; i < config.Count; i++)
                        {
                            int length = config[i];
                            if (index + config[i] > line.Length)
                            {
                                row.Add(header[i], trimOperation(line.Substring(index, line.Length - index)));
                                i = config.Count;
                            }
                            else
                            {
                                row.Add(header[i], trimOperation(line.Substring(index, length)));
                                index += config[i];
                            }
                        }
                        progressBar?.UpdateLoadingBar(mainForm);
                        if(row.Count != countBefore)
                        {
                            insertCommand = null;
                        }
                        insertCommand = DatabaseHelper.InsertRow(row, tableName, insertCommand);
                        countBefore = row.Count;
                    }
                }
                else
                {
                    using (StreamReader stream = new StreamReader(path, Encoding.GetEncoding(encoding)))
                    {
                        FileInfo info = new FileInfo(path);
                        progressBar?.StartLoadingBar((int)(info.Length / config.Sum()), mainForm);

                        long rowCount = 0;
                        while (!stream.EndOfStream && (!isPreview || rowCount < 3))
                        {
                            Dictionary<string, string> row = new Dictionary<string, string>();

                            for (int i = 0; i < config.Count && !stream.EndOfStream; i++)
                            {
                                char[] body = new char[config[i]];

                                for (int index = 0; index < config[i] && stream.Peek() != -1; index++)
                                {
                                    body[index] = (char)stream.Read();
                                }
                                row.Add(header[i], trimOperation(new string(body)));
                            }
                            progressBar?.UpdateLoadingBar(mainForm);
                            insertCommand = DatabaseHelper.InsertRow(row, tableName, insertCommand);
                            rowCount++;
                        }
                    }
                }
            }
            catch (IOException)
            {
                mainForm.MessagesOK(MessageBoxIcon.Warning, "Die Datei wird zurzeit von einem anderen Programm verwendet und kann nicht geöffnet werden.");
            }
            return true;
        }

        internal void OpenDBF(string path, ProgressBar progressBar, Form mainForm, string tableName)
        {
            string directory = ToShortPathName(Path.GetDirectoryName(path));
            string shortFileName = GetShortFileName(path);
            string shortPath = Path.Combine(directory, shortFileName);
            Func<string, string> trimOperation = GetTrimOperation();
            if (File.Exists(shortPath))
            {
                string constr = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={directory};Extended Properties=\"dBASE IV;CharacterSet={Encoding.Default.CodePage};\"";
                using (OleDbConnection con = new OleDbConnection(constr))
                {
                    con.Open();
                    progressBar?.StartLoadingBar(GetDataReaderRowCount(con, shortFileName, mainForm), mainForm);

                    using (OleDbCommand command = con.CreateCommand())
                    {
                        command.CommandText = $"select * from [{shortFileName}]";
                        SQLiteCommand insertCommand = null;
                        using (OleDbDataReader reader = command.ExecuteReader())
                        {
                            string[] columnNames = new string[reader.FieldCount];
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                columnNames[i] = reader.GetName(i).Trim();
                            }
                            DatabaseHelper.CreateTable(columnNames, tableName);
                            while (reader.Read())
                            {
                                object[] values = new object[reader.FieldCount];
                                for (int i = 0; i < reader.FieldCount; ++i)
                                {
                                    values[i] = trimOperation(reader.GetValue(i).ToString().Replace("\n", string.Empty).TrimEnd()); //remove new lines in dbase
                                }
                                insertCommand = DatabaseHelper.InsertRow(columnNames, values, tableName, insertCommand);
                                progressBar?.UpdateLoadingBar(mainForm);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageHandler.MessagesOK(mainForm, MessageBoxIcon.Error, $"Die Datei {shortPath} konnte nicht gefunden werden");
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
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, mainForm, false);
            }

            return count;
        }

        private int GetDataReaderTablesRowCount(OleDbConnection con, DataTable tables, Form mainForm)
        {
            int count = 0;
            foreach (DataRow row in tables.Rows)
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
            Func<string, string> trimOperation = GetTrimOperation();
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
                                for (int i = 0; i < reader.FieldCount; i++)
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
                                else if (newColumnNames.Count != 0)
                                {
                                    foreach (string columnName in newColumnNames)
                                    {
                                        DatabaseHelper.AddColumnFixedAlias(columnName, tableName);
                                    }
                                }

                                SQLiteCommand liteCommand = null;
                                while (reader.Read())
                                {
                                    object[] values = new object[reader.FieldCount];
                                    for (int i = 0; i < reader.FieldCount; ++i)
                                    {
                                        values[0] = trimOperation(reader.GetValue(i).ToString());
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

        private Func<string, string> GetTrimOperation()
        {
            return Properties.Settings.Default.TrimImport ? (Func<string, string>)(value => ProcTrim.Trim(value)) : value => value;
        }

        #region Excel Import
        internal void OpenExcel(string path, ProgressBar progressBar, Form mainForm, string tableName, ref string password)
        {
            Microsoft.Office.Interop.Excel.Application objXL = null;
            Microsoft.Office.Interop.Excel.Workbook objWB = null;
            Func<string, string> trimOperation = GetTrimOperation();
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

                string[] sheets = objWB.Worksheets.Cast<Microsoft.Office.Interop.Excel.Worksheet>().Select(x => x.Name).ToArray();
                int[] selectedSheetsIndizes = SelectItems(sheets, mainForm);
                string[] selectedSheets = selectedSheetsIndizes.Select(index => sheets[index]).ToArray();
                List<string> headers = new List<string>();
                if (selectedSheets.Length > 1)
                {
                    headers.Add(Extensions.DataTableExtensions.FileName);
                }

                DatabaseHelper.CreateTable(headers.ToArray(), tableName);

                foreach (string sheetName in selectedSheets)
                {
                    Microsoft.Office.Interop.Excel.Worksheet objSHT = objWB.Worksheets[sheetName];
                    int rows = objSHT.Rows.Find("*", System.Reflection.Missing.Value,
                                                   System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                                                   Microsoft.Office.Interop.Excel.XlSearchOrder.xlByRows, Microsoft.Office.Interop.Excel.XlSearchDirection.xlPrevious,
                                                   false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Row;
                    //int rows = objSHT.UsedRange.Rows.Count;
                    int cols = objSHT.Cells.Find("*", System.Reflection.Missing.Value,
                                                   System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                                                   Microsoft.Office.Interop.Excel.XlSearchOrder.xlByColumns, Microsoft.Office.Interop.Excel.XlSearchDirection.xlPrevious,
                                                   false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Column;
                    
                    if (cols == 0)
                    {
                        continue;
                    }

                    if (Properties.Settings.Default.UnhideRows)
                    {
                        objSHT.Rows.EntireRow.Hidden = false;
                    }

                    if (Properties.Settings.Default.UnhideColumns)
                    {
                        objSHT.Columns.EntireColumn.Hidden = false;
                    }

                    List<string> newHeaders = SetHeaderOfExcel(objSHT, cols);

                    foreach(string header in newHeaders)
                    {
                        if (!headers.Contains(header, System.StringComparer.OrdinalIgnoreCase))
                        {
                            headers.Add(header);
                            DatabaseHelper.AddColumn(tableName, header);
                        }
                    }

                    progressBar?.StartLoadingBar(rows, mainForm);

                    RangeToDataTable(objSHT, objXL, rows, cols, tableName, selectedSheets.Length != 1 ? Path.GetFileName(path) + "; " + sheetName : null, newHeaders, trimOperation, progressBar, mainForm);
                    Marshal.ReleaseComObject(objSHT);
                }
                if (selectedSheets.Length > 1)
                {
                    DatabaseHelper.MoveColumnToIndex(headers.Count - 1, headers.First(), tableName);
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

        private void RangeToDataTable(Microsoft.Office.Interop.Excel.Worksheet objSHT, Microsoft.Office.Interop.Excel.Application objXL, int rows, int cols, string tableName, string fileName, List<string> newHeaders, Func<string,string> trimOperation, ProgressBar progressBar, Form mainForm)
        {
            Clipboard.Clear();
            objXL.CutCopyMode = 0;
            int rowRange = MaxCellsPerIteration / cols; // about 50000 cells per iteration
            SQLiteCommand insertCommand = null;
            int maxAttempts = 5;
            for (int i = 2; i <= rows; i += rowRange + 1)
            {
                Microsoft.Office.Interop.Excel.Range c1 = objSHT.Cells[i, 1];
                int rowCount = i + rowRange;
                int endRow = rowCount > rows ? rows : rowCount;
                Microsoft.Office.Interop.Excel.Range c2 = objSHT.Cells[endRow, cols];
                Microsoft.Office.Interop.Excel.Range range = objSHT.get_Range(c1, c2);
                int attempts = 0;
                object content = null;

                while (content == null && attempts < maxAttempts)
                {
                    range.Copy();
                    content = Clipboard.GetDataObject().GetData("XML Spreadsheet");
                    attempts++;
                }
                if(attempts == maxAttempts && content == null)
                {
                    ErrorHelper.LogMessage($"Kopieren der Zeilen {i} bis {endRow} {maxAttempts} mal fehlgeschlagen\nZeilen werden übersprungen", mainForm);
                }
                else
                {
                    insertCommand = ClipboardToDatabase((MemoryStream)content, newHeaders.ToArray(), fileName, tableName, progressBar, mainForm, insertCommand, trimOperation);
                }
            }
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

        public static XDocument ForwardToXDocument(XmlDocument xmlDocument)
        {
            using (XmlNodeReader xmlNodeReader = new XmlNodeReader(xmlDocument))
            {
                xmlNodeReader.MoveToContent();
                return XDocument.Load(xmlNodeReader);
            }
        }

        private SQLiteCommand ClipboardToDatabase(MemoryStream content, string[] headers, string fileName, string tableName, ProgressBar progressBar, Form invokeForm, SQLiteCommand insertCommand, Func<string, string> trimOperation)
        {
            StreamReader streamReader = new StreamReader(content);
            streamReader.BaseStream.SetLength(streamReader.BaseStream.Length - 1);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(streamReader.ReadToEnd());
            
            List<XElement> linqRows = ForwardToXDocument(xmlDocument).Descendants(RowExcelNamespace).ToList();

            foreach (XElement rowElement in linqRows)
            {
                int index = 0;
                
                Dictionary<string, string> row = new Dictionary<string, string>(); //column, value pair
                for (int i = 0; i < headers.Length; ++i)
                {
                    row.Add(headers[i], string.Empty);
                }
                foreach (XElement cell in rowElement.Descendants(CellExcelNamespace))
                {
                    // emtpy columns are ignored in rowElement.Descendants, that's why we adjust the index
                    XAttribute indexAttribute = cell.Attribute(IndexNamexpace);
                    if (indexAttribute != null)
                    {
                        int.TryParse(indexAttribute.Value, out index);
                        index--;
                    }

                    string val = trimOperation(cell.Value.Replace("\t", string.Empty)); // remove tabs
                    string[] multiCells = val.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    if (multiCells.Length > 1)
                    {
                        for(int i = 0; i < multiCells.Length; ++i)
                        {
                            AddMultiCellColumn(headers[index], i, tableName, row, multiCells[i]);
                        }
                        insertCommand = null;
                    }
                    else
                    {
                        row[headers[index]] = val;
                        
                    }
                    index++;
                }
                
                insertCommand = AddContentDataRow(row, fileName, tableName, insertCommand);
                progressBar?.UpdateLoadingBar(invokeForm);
            };
            return insertCommand;
        }

        private SQLiteCommand AddContentDataRow(Dictionary<string, string> row, string fileName, string tableName, SQLiteCommand command)
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

        private void SetContentRowValue(Dictionary<string, string> row, string column, StringBuilder builder, Func<string, string> trimOperation)
        {
            row.Add(column, trimOperation(builder.ToString()));
            builder.Clear();
        }

        private List<string> GetHeadersOfContent(string content)
        {
            List<string> headers = new List<string>();
            StringBuilder header = new StringBuilder();
            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '\r' && content[i + 1] == '\n')
                {
                    AddHeaderOfContent(headers, header);
                    break;
                }
                else if (content[i] == '\t')
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

        private void AddMultiCellColumn(string header, int count, string tableName, Dictionary<string, string> row, string result)
        {
            string multiHeader = count == 0 ? header : header + count;
            string latestHeader = multiHeader;
            if(count != 0)
            {
                int countBefore = count - 1;
                latestHeader = header + (countBefore == 0 ? string.Empty : countBefore.ToString());
            }

            if (!DatabaseHelper.ContainsAlias(tableName, multiHeader))
            {
                multiHeader = DatabaseHelper.AddColumn(tableName, multiHeader);
                DatabaseHelper.MoveColumnToIndex(latestHeader, multiHeader, tableName);
            }
            row.Add(multiHeader, result);
        }
        #endregion

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
                foreach (string file in files)
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
            using (Stream stream = File.Open(path, FileMode.Open))
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

        private int[] SelectItems(string[] sheets, Form mainForm, string headerText = null)
        {
            int[] checkedSheets = new int[0];
            if (sheets.Length == 1)
            {
                return new int[] { 0 };
            }
            else
            {
                using (ExcelSheets form = new ExcelSheets(sheets, headerText))
                {
                    DialogResult result = DialogResult.Cancel;
                    mainForm.Invoke(new MethodInvoker(() =>
                    {
                        result = form.ShowDialog(mainForm);
                    }));
                    if (result == DialogResult.OK)
                    {
                        checkedSheets = form.GetSheets();
                    }
                }
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
                    setting = new ImportSettings(template.Table.ColumnValuesAsInt(0).ToList(), template.Table.ColumnValuesAsString(1).ToList(), template.Encoding, template.HasRowBreak);
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
