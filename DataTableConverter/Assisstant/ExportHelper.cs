using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using DataTableConverter.Extensions;
using DataTableConverter.View;
using Microsoft.Office.Interop.Excel;
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

namespace DataTableConverter
{
    internal enum SaveFormat { CSV = 0, DBASE = 1, EXCEL = 2 };
    class ExportHelper
    {
        [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true)]
        public static extern int AddFontResource([In][MarshalAs(UnmanagedType.LPWStr)]
                                         string lpFileName);

        internal static readonly string ProjectName = "Tabellenkonvertierung";
        internal static string ProjectPath
        {
            get
            {
                if (Properties.Settings.Default.SettingPath == string.Empty || !Directory.Exists(Properties.Settings.Default.SettingPath))
                {
                    Properties.Settings.Default.SettingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ProjectName);
                    Properties.Settings.Default.Save();
                }
                return Properties.Settings.Default.SettingPath;
            }
        }

        internal static string ProjectPresets => Path.Combine(ProjectPath, "Vorlagen");
        internal static string ProjectProcedures => Path.Combine(ProjectPath, "Funktionen.bin");
        internal static string ProjectTolerance => Path.Combine(ProjectPath, "Toleranzen.bin");
        internal static string ProjectCases => Path.Combine(ProjectPath, "Fälle.bin");
        internal static string ProjectWorkflows => Path.Combine(ProjectPath, "Arbeitsabläufe.bin");
        internal static string WorkflowPath => Path.Combine(ProjectPath, "Arbeitsabläufe");
        internal static string ProcedurePath => Path.Combine(ProjectPath, "Suchen & Ersetzen");
        internal static string ProjectHeaderPresets => Path.Combine(ProjectPath, "Vorlagen Überschriften");
        private readonly string CSVSeparator = ";";
        private readonly Encoding DbaseEncoding = Encoding.GetEncoding(850); //858; 850; "ISO-8859-1"; 866
        internal readonly int DbaseMaxFileLength = 8;
        private readonly int DbaseMaxHeaderLength = 10;
        private readonly int DbaseMaxCharacterLength = 254;
        private readonly int DbaseMaxRecordCharacterLength = 3999;
        private readonly string FontFileName = "seguisym.ttf";
        private readonly DatabaseHelper DatabaseHelper;


        internal ExportHelper(DatabaseHelper databaseHelper)
        {
            DatabaseHelper = databaseHelper;
        }

        internal void CheckRequired()
        {
            CheckFolders();
            CheckFont();
        }

        private void CheckFolders()
        {
            if (!Directory.Exists(ProjectPath))
            {
                Directory.CreateDirectory(ProjectPath);
            }
            if (!Directory.Exists(ProjectPresets))
            {
                Directory.CreateDirectory(ProjectPresets);
            }
            if (!Directory.Exists(ProjectHeaderPresets))
            {
                Directory.CreateDirectory(ProjectHeaderPresets);
            }
            if (!Directory.Exists(WorkflowPath))
            {
                Directory.CreateDirectory(WorkflowPath);
            }
            if (!Directory.Exists(ProcedurePath))
            {
                Directory.CreateDirectory(ProcedurePath);
            }
        }

        private void CheckFont()
        {
            string fontName = "Segoe UI Symbol";
            float fontSize = 12;

            using (System.Drawing.Font fontTester = new System.Drawing.Font(
                   fontName,
                   fontSize,
                   System.Drawing.FontStyle.Regular,
                   System.Drawing.GraphicsUnit.Pixel))
            {
                if (fontTester.Name != fontName)
                {
                    string filePath = Path.Combine(UpdateHelper.GetCurrentDirectory(), FontFileName);
                    if (File.Exists(filePath))
                    {
                        AddFontResource(filePath);
                    }
                }
            }
        }

        internal bool SaveProcedures(List<Proc> procedures, Form mainForm)
        {
            bool error = false;
            List<string> files = new List<string>(GetProcedures());
            foreach (Proc proc in procedures.Where(proc => proc.Name != null))
            {
                try
                {
                    string filename = RemoveSpecialCharacters(proc.Name);
                    string path = Path.Combine(ProcedurePath, filename + ".bin");
                    using (Stream stream = File.Open(path, FileMode.Create))
                    {
                        BinaryFormatter bin = new BinaryFormatter();
                        bin.Serialize(stream, proc);
                        files.Remove(path);
                    }
                }
                catch (Exception ex)
                {
                    error = true;
                    ErrorHelper.LogMessage(ex, mainForm, false);
                }
            }
            if (!error)
            {
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
            return error;
        }

        internal string RemoveSpecialCharacters(string text)
        {
            return text.Replace("?", "？")
                       .Replace(" : ", "：")
                       .Replace(":", "：")
                       .Replace("/", " ∕ ")
                       .Replace("\\", "＼")
                       .Replace("\"", "''")
                       .Replace(">", "＞")
                       .Replace("<", "＜")
                       .Replace("*", "＊");
        }

        internal bool SaveWorkflows(List<Work> workflows, Form mainForm)
        {
            bool error = false;
            List<string> files = new List<string>(GetWorkflows());
            foreach (Work work in workflows.Where(work => work.Name != null))
            {
                try
                {
                    string filename = RemoveSpecialCharacters(work.Name);
                    string path = Path.Combine(WorkflowPath, filename + ".bin");
                    using (Stream stream = File.Open(path, FileMode.Create))
                    {
                        BinaryFormatter bin = new BinaryFormatter();
                        bin.Serialize(stream, work);
                        files.Remove(path);
                    }
                }
                catch (Exception ex)
                {
                    error = true;
                    ErrorHelper.LogMessage(ex, mainForm, false);
                }
            }
            if (!error)
            {
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
            return error;
        }

        internal string[] GetWorkflows()
        {
            return Directory.GetFiles(WorkflowPath, "*.bin");
        }

        internal string[] GetProcedures()
        {
            return Directory.GetFiles(ProcedurePath, "*.bin");
        }

        internal bool SaveTextImportTemplate(TextImportTemplate template, string path)
        {
            bool error = false;
            try
            {
                using (Stream stream = File.Open(path, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, template);
                }
            }
            catch (IOException)
            {
                error = true;
            }
            return error;
        }

        internal bool SaveTolerances(List<Tolerance> tolerances)
        {
            bool error = false;
            try
            {
                using (Stream stream = File.Open(ProjectTolerance, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, tolerances);
                }
            }
            catch (IOException)
            {
                error = true;
            }
            return error;
        }

        internal bool SaveCases(List<Case> cases)
        {
            bool error = false;
            try
            {
                using (Stream stream = File.Open(ProjectCases, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, cases);
                }
            }
            catch (IOException)
            {
                error = true;
            }
            return error;
        }

        internal int Save(string directory, string fileName, string oldFileExtension, int encoding, SaveFormat format, string order, OrderType orderType, Form invokeForm, string tableName, System.Action updateLoadingBar = null, string orderColumnName = null)
        {
            SQLiteCommand command = orderColumnName == string.Empty ? DatabaseHelper.GetDataCommand(tableName, order, orderType) : DatabaseHelper.GetDataCommand(tableName, order, orderType, orderColumnName);
            return Save(directory, fileName, oldFileExtension, encoding, format, order, orderType, invokeForm, tableName, command, updateLoadingBar);
        }

        internal int Save(string directory, string fileName, string oldFileExtension, int encoding, SaveFormat format, string order, OrderType orderType, Form invokeForm, string tableName, SQLiteCommand command, System.Action updateLoadingBar = null)
        {
            int rowCount = 0;
            if (command == null)
            {
                command = DatabaseHelper.GetDataCommand(tableName, order, orderType);
            }

            switch (format)
            {
                //CSV
                case SaveFormat.CSV:
                    rowCount = ExportCsv(directory, fileName, encoding, command, invokeForm, tableName, updateLoadingBar);
                    break;

                //Dbase
                case SaveFormat.DBASE:
                    rowCount = ExportDbase(tableName, directory, fileName, command, invokeForm);
                    break;

                //Excel
                case SaveFormat.EXCEL:
                    rowCount = ExportExcel(directory, fileName, oldFileExtension, command, invokeForm, tableName, updateLoadingBar);
                    break;
            }
            return rowCount;
        }

        private int ExportDbase(string tableName, string directory, string fullFileName, SQLiteCommand command, Form invokeForm)
        {
            int rowCount = 0;
            List<string> duplicates = new List<string>();
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                string[] aliases = new string[reader.FieldCount];
                string fileName = fullFileName;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    aliases[i] = reader.GetName(i);
                }
                string[] sortedHeaders = aliases.OrderBy(h => h, System.StringComparer.OrdinalIgnoreCase).ToArray();
                for (int i = 1; i < sortedHeaders.Length; i++)
                {
                    string originalHeader = sortedHeaders[i];
                    string originalHeaderBefore = sortedHeaders[i - 1];
                    string header = originalHeader.Length > DbaseMaxHeaderLength ? originalHeader.Substring(0, DbaseMaxHeaderLength) : originalHeader;
                    string headerBefore = originalHeaderBefore.Length > DbaseMaxHeaderLength ? originalHeaderBefore.Substring(0, DbaseMaxHeaderLength) : originalHeaderBefore;
                    if (header == headerBefore)
                    {
                        duplicates.Add($"\"{originalHeaderBefore}\" und \"{originalHeader}\"");
                    }
                }

                if (duplicates.Count > 0)
                {
                    MessageHandler.MessagesOK(invokeForm, MessageBoxIcon.Warning, "Aufgrund der Kürzung von Spaltennamen durch DBASE gibt es Duplikate: \n" + string.Join(" ,\n", duplicates));
                    return 0;
                }


                if (fullFileName.Length > DbaseMaxFileLength)
                {
                    fileName = fileName.Substring(0, DbaseMaxFileLength);
                }
                string tempDirectoryPath = Path.Combine(Path.GetDirectoryName(directory), "temp");
                try
                {
                    Directory.CreateDirectory(tempDirectoryPath);
                }
                catch (Exception ex)
                {
                    ErrorHelper.LogMessage(ex, invokeForm);
                    return 0;
                }
                string fullPathTemp = Path.Combine(tempDirectoryPath, fileName + ".DBF");
                string fullPathOriginal = Path.Combine(directory, fullFileName + ".DBF");

                if (File.Exists(fullPathTemp))
                {
                    File.Delete(fullPathTemp);
                }

                int[] max = DatabaseHelper.GetMaxColumnLength(aliases, tableName);
                for (int i = 0; i < max.Length; i++)
                {
                    if (max[i] > DbaseMaxCharacterLength)
                    {
                        max[i] = DbaseMaxCharacterLength;
                    }
                }

                if (max.Sum() <= DbaseMaxRecordCharacterLength)
                {
                    string query = string.Empty;
                    try
                    {
                        CreateTable(aliases, max, tempDirectoryPath, fileName, ref query);
                        if (File.Exists(fullPathTemp))
                        {
                            #region Adjust Header. Update number of records
                            using (FileStream stream = new FileStream(fullPathTemp, FileMode.OpenOrCreate))
                            {
                                byte[] bytes = new byte[1] { 0x1A };
                                stream.Position = stream.Length - 1;

                                if (stream.ReadByte() == bytes[0])
                                {
                                    stream.Position--;
                                }


                                for (; reader.Read(); rowCount++)
                                {
                                    StringBuilder builder = new StringBuilder(" ");
                                    for (int i = 0; i < aliases.Length; i++)
                                    {
                                        string temp = reader.GetValue(i).ToString();
                                        builder.Append(temp.Length > DbaseMaxCharacterLength ? temp.Substring(0, DbaseMaxCharacterLength) : temp.PadRight(max[i]));
                                    }
                                    stream.Write(DbaseEncoding.GetBytes(builder.ToString()), 0, builder.Length);
                                }

                                stream.Write(bytes, 0, bytes.Length);

                                byte[] records = BitConverter.GetBytes(rowCount);
                                stream.Position = 4;
                                stream.Write(records, 0, records.Length);
                                #endregion
                            }
                            if (File.Exists(fullPathOriginal))
                            {
                                File.Delete(fullPathOriginal);
                            }
                            try
                            {
                                File.Move(fullPathTemp, fullPathOriginal);
                            }
                            catch (FileNotFoundException)
                            {
                                MessageHandler.MessagesOK(invokeForm, MessageBoxIcon.Error, $"Die Datei {fullPathTemp} konnte nicht erstellt werden.\nDieses Problem ist höchstwahrscheinlich aufgrund eines Anti-Virenprogrammes aufgetreten.");
                            }
                        }
                        else
                        {
                            MessageHandler.MessagesOK(invokeForm, MessageBoxIcon.Error, $"Die Datei {fullPathTemp} konnte nicht erstellt werden.\nDieses Problem ist höchstwahrscheinlich aufgrund eines Anti-Virenprogrammes aufgetreten.");
                        }

                    }
                    catch (Exception ex)
                    {
                        ErrorHelper.LogMessage($"{ex.ToString() + Environment.NewLine} query:{query};   path: {tempDirectoryPath}; fileName: {fileName}; headers:[{string.Join("; ", aliases)}]", invokeForm);
                        return 0;
                    }
                }
                else
                {
                    invokeForm.MessagesOK(MessageBoxIcon.Warning, $"Die maximal unterstützte Zeilenlänge von {DbaseMaxRecordCharacterLength + 1:n0} Zeichen wurde überschritten!\nDie Datei kann nicht erstellt werden");
                    rowCount = 0;
                }
                if (Directory.Exists(tempDirectoryPath))
                {
                    DeleteDirectory(tempDirectoryPath, invokeForm);
                }
                else
                {
                    invokeForm.MessagesOK(MessageBoxIcon.Warning, $"Der temporär angelegte Ordner kann nicht wieder gelöscht werden, da er nicht gefunden werden kann");
                }
            }
            return rowCount;
        }


        /// <summary>
        /// Depth-first recursive delete, with handling for descendant 
        /// directories open in Windows Explorer.
        /// </summary>
        private void DeleteDirectory(string path, Form mainForm)
        {

            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory, mainForm);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, mainForm, true);
            }
        }

        private void CreateTable(string[] columns, int[] max, string path, string filename, ref string query)
        {
            using (OleDbConnection con = new OleDbConnection(GetConnection(path)))
            {
                con.Open();
                using (OleDbCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = CreateQuery(columns, filename, max);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string CreateQuery(string[] columns, string filename, int[] max)
        {
            StringBuilder csb = new StringBuilder($"create table [{filename}] (");
            for (int i = 0; i < columns.Length; i++)
            {
                csb.Append($"[{columns[i]}] varchar({max[i]}),");
            }

            csb[csb.Length - 1] = ')';
            return csb.ToString();
        }

        private string GetConnection(string path)
        {
            return $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={path};Extended Properties=dBase IV";
        }

        internal int ExportCsv(string directory, string fileName, int encoding, SQLiteCommand command, Form invokeForm, string tableName, System.Action updateLoadingBar = null)
        {
            int rowCount = 0;
            string path = Path.Combine(directory, fileName + ".csv");

            if (encoding == 0)
            {
                SelectEncoding form = new SelectEncoding();
                DialogResult result = DialogResult.Cancel;
                invokeForm.Invoke(new MethodInvoker(() =>
                {
                    result = form.ShowDialog(invokeForm);
                }));
                if (result == DialogResult.OK)
                {
                    encoding = form.FileEncoding;
                }
            }
            if (encoding != 0)
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(fileStream, Encoding.GetEncoding(encoding)))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                for (var i = 0; i < reader.FieldCount - 1; i++)
                                {
                                    writer.Write(reader.GetName(i));
                                    writer.Write(CSVSeparator);
                                }
                                writer.Write(reader.GetName(reader.FieldCount - 1));
                                writer.Write(writer.NewLine);

                                for (; reader.Read(); rowCount++)
                                {

                                    for (int i = 0; i < reader.FieldCount - 1; i++)
                                    {
                                        writer.Write(reader.GetValue(i).ToString());
                                        writer.Write(CSVSeparator);
                                    }
                                    writer.Write(reader.GetValue(reader.FieldCount - 1).ToString());
                                    writer.Write(writer.NewLine);
                                    updateLoadingBar?.Invoke();
                                }
                            }
                        }
                    }
                }
            }
            return rowCount;
        }

        private int ExportExcel(string directory, string fileName, string oldFileExtension, SQLiteCommand command, Form invokeForm, string tableName, System.Action updateLoadingBar)
        {
            int rowCount = 0;
            Workbooks workbooks = null;
            Workbook workbook = null;
            Microsoft.Office.Interop.Excel.Application excel = null;
            try
            {
                string workSheetName = "Tabelle 1";

                excel = new Microsoft.Office.Interop.Excel.Application
                {
                    DisplayAlerts = false,
                    Visible = false,
                    ScreenUpdating = false,
                    SheetsInNewWorkbook = 1
                };

                workbooks = excel.Workbooks;
                workbook = workbooks.Add(Type.Missing);

                Sheets worksheets = workbook.Sheets;
                Worksheet worksheet = (Worksheet)worksheets[1];

                excel.Calculation = XlCalculation.xlCalculationManual;
                worksheet.Name = workSheetName;

                int maxRows = DatabaseHelper.GetRowCount(tableName);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) //write header
                    {
                        string[] aliases = new string[reader.FieldCount];
                        int maxRowsPerExecution = ImportHelper.MaxCellsPerIteration / reader.FieldCount; // about 50000 cells per iteration
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            aliases[i] = reader.GetName(i);
                        }
                        InsertHeadersToExcel(aliases, worksheet);
                        InsertRowsSkeleton(worksheet, maxRows, aliases.Length);
                        int rowStart = 2; //+2 because index starts at 1 and header is first
                        while (reader.HasRows)
                        {
                            int max = rowCount + maxRowsPerExecution > maxRows ? maxRows - rowCount : maxRowsPerExecution;
                            int newRows;
                            object[,] data = new object[max, aliases.Length];
                            for (newRows = 0; newRows < maxRowsPerExecution && reader.Read(); rowCount++, newRows++)
                            {
                                object[] row = new object[aliases.Length];
                                for (int y = 0; y < aliases.Length; y++)
                                {
                                    data[newRows, y] = reader.GetValue(y).ToString();
                                }
                                updateLoadingBar?.Invoke();
                            }

                            try
                            {
                                InsertRowsToExcel(worksheet, data, rowStart, newRows - 1, aliases.Length);
                            }
                            catch (Exception ex)
                            {
                                ErrorHelper.LogMessage($"Error while inserting rows to Excel{Environment.NewLine}rowStart: {rowStart}; count: {newRows - 1}; columnCount: {aliases.Length}; maxRows: {maxRows}; max: {max}", invokeForm, false);
                                throw ex;
                            }
                            rowStart += newRows;
                        }
                    }
                }

                SaveExcelFile(directory, fileName, oldFileExtension, workbook, invokeForm);
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, invokeForm);
                rowCount = 0;
            }
            finally
            {
                workbook?.Close(false, Type.Missing, Type.Missing);
                excel?.Quit();

                // Release our resources.
                if (workbook != null) Marshal.ReleaseComObject(workbook);
                if (workbooks != null) Marshal.ReleaseComObject(workbooks);
                if (excel != null)
                {
                    Marshal.ReleaseComObject(excel);
                    Marshal.FinalReleaseComObject(excel);
                }
            }

            return rowCount;
        }

        private void SaveExcelFile(string directory, string fileName, string oldFileExtension, Workbook workbook, Form invokeForm)
        {
            string saveName = fileName + ".xls";
            XlFileFormat fileFormat = XlFileFormat.xlWorkbookNormal;
            if (oldFileExtension != ".xls")
            {
                saveName += "x";
                fileFormat = XlFileFormat.xlOpenXMLWorkbook;
            }
            string path = Path.Combine(directory, saveName);
            try
            {
                workbook.SaveAs(path, fileFormat, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            }
            catch (Exception ex)
            {
                invokeForm.MessagesOK(MessageBoxIcon.Warning, "Die Datei konnte nicht gespeichert werden! Wird die Datei gerade verwendet?");
                ErrorHelper.LogMessage(ex, invokeForm, false);
            }
        }

        private ListObject InsertHeadersToExcel(string[] columns, Worksheet worksheet)
        {
            // Insert column headers.
            object[,] data = new object[1, columns.Length];
            for (int column = 0; column < columns.Length; column++)
            {
                data[0, column] = columns[column];
            }

            Range beginWrite = (Range)worksheet.Cells[1, 1];
            Range endWrite = (Range)worksheet.Cells[1, columns.Length];
            Range sheetData = worksheet.Range[beginWrite, endWrite];
            sheetData.NumberFormat = "@";
            sheetData.Value2 = data;

            worksheet.Select();
            return sheetData.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange,
                                                   sheetData,
                                                   Type.Missing,
                                                   XlYesNoGuess.xlYes,
                                                   Type.Missing);
        }

        private void InsertRowsToExcel(Worksheet worksheet, object[,] data, int rowStart, int rowCount, int columnCount)
        {
            Range beginWrite = (Range)worksheet.Cells[rowStart, 1];
            Range endWrite = (Range)worksheet.Cells[rowStart + rowCount, columnCount];
            Range range = worksheet.Range[beginWrite, endWrite];
            range.NumberFormat = "@";
            range.Value2 = data;
        }

        private void InsertRowsSkeleton(Worksheet worksheet, int rowCount, int columnCount)
        {
            Range beginWrite = (Range)worksheet.Cells[2, 1];
            Range endWrite = (Range)worksheet.Cells[rowCount + 2, columnCount];
            Range addNewRows = worksheet.Range[beginWrite, endWrite];
            addNewRows.Insert(XlInsertShiftDirection.xlShiftDown, XlInsertFormatOrigin.xlFormatFromLeftOrAbove);
        }


        /// <summary>
        /// Split table depending on row values. Each row with a specific value goes into another table
        /// </summary>
        /// <param name="items"></param>
        /// <param name="filePath"></param>
        /// <param name="stopLoadingBar"></param>
        /// <param name="saveFinished"></param>
        /// <param name="codePage"></param>
        /// <param name="mainForm"></param>
        /// <param name="continuedNumberColumn"></param>
        /// <param name="tableName"></param>
        internal void ExportTableWithColumnCondition(IEnumerable<ExportCustomItem> items, string filePath, int codePage, string order, OrderType orderType, Form mainForm, string continuedNumberColumn, string tableName, Action<string> setStatus)
        {
            foreach (ExportCustomItem item in items)
            {
                Dictionary<string, string[]> dict = new Dictionary<string, string[]>();
                string fileExtension = GetFileExtension(item.Format);

                if (item.CheckedAllValues)
                {
                    foreach (string value in item.AllValues)
                    {
                        setStatus($"Die Datei {item.Name}_{value}.{fileExtension} wird vorbereitet");
                        string newTable = Guid.NewGuid().ToString();
                        DatabaseHelper.CreateTable(DatabaseHelper.GetSortedColumnsAsAlias(tableName).ToArray(), newTable);
                        dict.Add(value, new string[] { newTable, $"{item.Name}_{value}" });
                    }
                }
                else
                {
                    string newTable = Guid.NewGuid().ToString();
                    DatabaseHelper.CreateTable(DatabaseHelper.GetSortedColumnsAsAlias(tableName).ToArray(), newTable);
                    foreach (string value in item.SelectedValues)
                    {
                        setStatus($"Die Datei {item.Name}.{fileExtension} wird vorbereitet");
                        if (!dict.ContainsKey(value))
                        {
                            dict.Add(value, new string[] { newTable, item.Name });
                        }
                    }
                }

                setStatus($"Die Datei wird vorbereitet");
                DatabaseHelper.SplitTableOnRowValue(dict, item.Column, tableName);

                foreach (string[] tableInfo in dict.Values.GroupBy(info => info[0]).Select(group => group.First()))
                {
                    string newTable = tableInfo[0];
                    string fileName = tableInfo[1];
                    setStatus($"Die Datei {fileName}.{fileExtension} wird gespeichert");
                    Save(Path.GetDirectoryName(filePath), fileName, Path.GetExtension(filePath), codePage, (SaveFormat)item.Format, order, orderType, mainForm, newTable, null, continuedNumberColumn);
                    DatabaseHelper.Delete(newTable);
                }
            }
        }

        private string GetFileExtension(int format)
        {
            string result;
            switch (format)
            {
                case 0:
                    result = "csv";
                    break;
                case 1:
                    result = "DBASE";
                    break;
                default:
                    result = "xlsx";
                    break;
            }
            return result;
        }

        /// <summary>
        /// Export either group and count of column or {count} rows per distinct value in column
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="count"></param>
        /// <param name="showFromTo"></param>
        /// <param name="tableName"></param>
        /// <param name="orderType"></param>
        /// <returns></returns>
        internal string ExportCount(string columnName, int count, bool showFromTo, OrderType orderType, string tableName)
        {
            string newTable = Guid.NewGuid().ToString();

            if (count == 0)
            {
                Dictionary<string, long> pair = DatabaseHelper.GroupCountOfColumn(columnName, tableName);
                DatabaseHelper.DictionaryToTable(pair, columnName, showFromTo, newTable, DatabaseHelper.GetRowCount(tableName));
            }
            else
            {
                DatabaseHelper.CreateTable(DatabaseHelper.GetSortedColumnsAsAlias(tableName), newTable);
                DatabaseHelper.InsertDataPerColumnValue(columnName, orderType, count, tableName, newTable);
            }
            return newTable;
        }
    }
}
