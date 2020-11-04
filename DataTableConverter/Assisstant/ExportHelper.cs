using DataTableConverter.Assisstant;
using DataTableConverter.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using DataTableConverter.Extensions;
using System.Threading;
using DataTableConverter.View;
using System.Data.SQLite;

namespace DataTableConverter
{
    class ExportHelper
    {
        [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true)]
        public static extern int AddFontResource([In][MarshalAs(UnmanagedType.LPWStr)]
                                         string lpFileName);

        internal static readonly string ProjectName = "Tabellenkonvertierung";
        internal static string ProjectPath { get
            {
                return Properties.Settings.Default.SettingPath == string.Empty ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ProjectName) : Properties.Settings.Default.SettingPath;
            } }
        
        internal static string ProjectPresets => Path.Combine(ProjectPath,"Vorlagen");
        internal static string ProjectProcedures => Path.Combine(ProjectPath,"Funktionen.bin");
        internal static string ProjectTolerance => Path.Combine(ProjectPath,"Toleranzen.bin");
        internal static string ProjectCases => Path.Combine(ProjectPath,"Fälle.bin");
        internal static string ProjectWorkflows => Path.Combine(ProjectPath,"Arbeitsabläufe.bin");
        internal static string WorkflowPath => Path.Combine(ProjectPath, "Arbeitsabläufe");
        internal static string ProcedurePath => Path.Combine(ProjectPath, "Suchen & Ersetzen");
        internal static string ProjectHeaderPresets => Path.Combine(ProjectPath, "Vorlagen Überschriften");
        private static readonly string CSVSeparator = ";";
        private static readonly Encoding DbaseEncoding = Encoding.GetEncoding(850); //858; 850; "ISO-8859-1"; 866
        internal static readonly int DbaseMaxFileLength = 8;
        private static readonly int DbaseMaxHeaderLength = 10;
        private static readonly int DbaseMaxCharacterLength = 254;
        private static readonly int DbaseMaxRecordCharacterLength = 3999;
        private static readonly string FontFileName = "seguisym.ttf";


        internal static void CheckRequired()
        {
            CheckFolders();
            CheckFont();
        }

        private static void CheckFolders()
        {
            if (!Directory.Exists(ProjectPath))
            {
                Directory.CreateDirectory(ProjectPath);
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

        private static void CheckFont()
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
        
        internal static bool SaveProcedures(List<Proc> procedures, Form mainForm)
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

        internal static string RemoveSpecialCharacters(string text)
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

        internal static bool SaveWorkflows(List<Work> workflows, Form mainForm)
        {
            bool error = false;
            List<string> files = new List<string>(GetWorkflows());
            foreach (Work work in workflows.Where(work => work.Name != null))
            {
                try
                {
                    string filename = RemoveSpecialCharacters(work.Name);
                    string path = Path.Combine(WorkflowPath, filename+".bin");
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
                foreach(string file in files)
                {
                    File.Delete(file);
                }
            }
            return error;
        }

        internal static string[] GetWorkflows()
        {
            return Directory.GetFiles(WorkflowPath, "*.bin");
        }

        internal static string[] GetProcedures()
        {
            return Directory.GetFiles(ProcedurePath, "*.bin");
        }

        internal static bool SaveTextImportTemplate(TextImportTemplate template, string path)
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

        internal static bool SaveTolerances(List<Tolerance> tolerances)
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

        internal static bool SaveCases(List<Case> cases)
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

        internal static int Save(string filePath, int encoding, int format, Form1 invokeForm, SQLiteCommand command)
        {
            int rowCount = 0;
            switch (format)
            {
                //CSV
                case 0:
                    {
                        rowCount = ExportCsv(tableName, path, filePath, encoding, invokeForm);
                    }
                    break;

                //Dbase
                case 1:
                    {
                        rowCount = ExportDbase(filePath, tableName, path, invokeForm);
                    }
                    break;

                //Excel
                case 2:
                    {
                        rowCount = ExportExcel(filePath, command, invokeForm);
                    }
                    break;
            }
            return rowCount;
        }

        internal static void ExportCsv(DataTable dt, string directory, string filename, int codePage, Form mainForm, Action updateLoadingBar = null)
        {
            string path = Path.Combine(directory, filename + ".csv");
            StreamWriter writer;
            FileStream fileStream = null;
            if (codePage == 0)
            {
                SelectEncoding form = new SelectEncoding();
                DialogResult result = DialogResult.Cancel;
                mainForm.Invoke(new MethodInvoker(() =>
                {
                    result = form.ShowDialog(mainForm);
                }));
                if(result == DialogResult.OK)
                {
                    codePage = form.FileEncoding;
                }
            }
            if (codePage != 0)
            {
                fileStream = new FileStream(path, FileMode.Create);
                writer = new StreamWriter(fileStream, Encoding.GetEncoding(codePage));

                writer.WriteLine(string.Join(CSVSeparator, dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName)));
                foreach (DataRow row in dt.Rows)
                {
                    writer.WriteLine(string.Join(CSVSeparator, row.ItemArray.Select(field => field.ToString())));
                    updateLoadingBar?.Invoke();
                }
                writer.Close();
                fileStream.Close();
            }
        }


        private static int ExportExcel(string filePath, SQLiteCommand command, Form1 invokeForm)
        {
            int offset = 0;
            bool running = true;

            try
            {
                string workSheetName = "Tabelle 1";

                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application
                {
                    DisplayAlerts = false,
                    Visible = false,
                    ScreenUpdating = false,
                    SheetsInNewWorkbook = 1
                };

                Microsoft.Office.Interop.Excel.Workbooks workbooks = excel.Workbooks;
                Microsoft.Office.Interop.Excel.Workbook workbook = workbooks.Add(Type.Missing);

                Microsoft.Office.Interop.Excel.Sheets worksheets = workbook.Sheets;
                Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worksheets[1];

                excel.Calculation = Microsoft.Office.Interop.Excel.XlCalculation.xlCalculationManual;
                worksheet.Name = workSheetName;

                InsertHeadersToExcel(DatabaseHelper.GetSortedColumnsAsAlias().ToArray(), worksheet);

                while(running)
                {
                    command.Parameters["$offset"].Value = offset;
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table); //instead of fill we could execute Reader and create the needed object[,] here
                        if (table.Rows.Count == 0)
                        {
                            running = false;
                        }
                        else
                        {
                            int columns = table.Columns.Count;

                            InsertRowsSkeleton(worksheet, table.Rows.Count, columns, offset);
                            InsertRowsToExcel(worksheet, table.ToObjectArray(), offset + 2, table.Rows.Count - 1, columns); //+2 because index starts at 1 and header is first

                            offset += table.Rows.Count;
                            running = table.Rows.Count < Properties.Settings.Default.MaxRows;
                            
                            table.Dispose();
                        }
                    }
                }


            }
            catch
            {

            }
            
            return offset;
        }

        internal static string ExportExcel(DataTable dt, string directory, string filename, Form mainForm)
        {
            string path = null;
            int rowRange = 10000;
            try
            {
                string workSheetName = "Tabelle 1";

                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application
                {
                    DisplayAlerts = false,
                    Visible = false,
                    ScreenUpdating = false,
                    SheetsInNewWorkbook = 1
                };

                Microsoft.Office.Interop.Excel.Workbooks workbooks = excel.Workbooks;
                Microsoft.Office.Interop.Excel.Workbook workbook = workbooks.Add(Type.Missing);

                Microsoft.Office.Interop.Excel.Sheets worksheets = workbook.Sheets;
                Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)worksheets[1];

                excel.Calculation = Microsoft.Office.Interop.Excel.XlCalculation.xlCalculationManual;
                worksheet.Name = workSheetName;

                
                int columns = dt.Columns.Count;
                // Add the +1 to allow room for column headers.


                int rowCount = dt.Rows.Count;
                InsertHeadersToExcel(dt, worksheet);
                InsertRowsSkeleton(worksheet, rowCount, columns);
                
                int rowStart = 2;
                for (int step = 0; step < rowCount; step += rowRange)
                {
                    int rows = Math.Min(rowRange, rowCount-step);
                    object[,] data = new object[rows, columns];
                    for (int srcRow = step, dstRow = 0; dstRow < rows; srcRow++, dstRow++)
                    {
                        for (int column = 0; column < columns; column++)
                        {
                            data[dstRow, column] = dt.Rows[srcRow][column];
                        }
                    }
                    InsertRowsToExcel(worksheet, data, rowStart, rows - 1, columns);
                    rowStart += rows;
                }

                string saveName = Path.GetFileNameWithoutExtension(filename)+".xls";
                Microsoft.Office.Interop.Excel.XlFileFormat fileFormat = Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal;
                if (Path.GetExtension(filename) != ".xls")
                {
                    saveName += "x";
                    fileFormat = Microsoft.Office.Interop.Excel.XlFileFormat.xlOpenXMLWorkbook;
                }
                path = Path.Combine(directory, saveName);
                try
                {
                    workbook.SaveAs(path, fileFormat, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                }
                catch(Exception ex)
                {
                    mainForm.MessagesOK(MessageBoxIcon.Warning, "Die Datei konnte nicht gespeichert werden! Wird die Datei gerade verwendet?");
                    ErrorHelper.LogMessage(ex,mainForm, false);
                }
                workbook.Close(false, Type.Missing, Type.Missing);
                excel.Quit();

                // Release our resources.
                Marshal.ReleaseComObject(workbook);
                Marshal.ReleaseComObject(workbooks);
                Marshal.ReleaseComObject(excel);
                Marshal.FinalReleaseComObject(excel);
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, mainForm);
            }
            return path;
        }

        private static Microsoft.Office.Interop.Excel.ListObject InsertHeadersToExcel(DataTable table, Microsoft.Office.Interop.Excel.Worksheet worksheet)
        {
            return InsertHeadersToExcel(table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray(), worksheet);
        }

        private static Microsoft.Office.Interop.Excel.ListObject InsertHeadersToExcel(string[] columns, Microsoft.Office.Interop.Excel.Worksheet worksheet)
        {
            // Insert column headers.
            object[,] data = new object[1, columns.Length];
            for (int column = 0; column < columns.Length; column++)
            {
                data[0, column] = columns[column];
            }

            Microsoft.Office.Interop.Excel.Range beginWrite = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, 1];
            Microsoft.Office.Interop.Excel.Range endWrite = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, columns.Length];
            Microsoft.Office.Interop.Excel.Range sheetData = worksheet.Range[beginWrite, endWrite];
            sheetData.NumberFormat = "@";
            sheetData.Value2 = data;

            worksheet.Select();
            return sheetData.Worksheet.ListObjects.Add(Microsoft.Office.Interop.Excel.XlListObjectSourceType.xlSrcRange,
                                                   sheetData,
                                                   Type.Missing,
                                                   Microsoft.Office.Interop.Excel.XlYesNoGuess.xlYes,
                                                   Type.Missing);
        }

        private static void InsertRowsToExcel( Microsoft.Office.Interop.Excel.Worksheet worksheet, object[,] data, int rowStart, int rowCount, int columnCount)
        {
            Microsoft.Office.Interop.Excel.Range beginWrite = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[rowStart, 1];
            Microsoft.Office.Interop.Excel.Range endWrite = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[rowStart + rowCount, columnCount];
            Microsoft.Office.Interop.Excel.Range range = worksheet.Range[beginWrite, endWrite];
            range.NumberFormat = "@";
            range.Value2 = data;
        }

        private static void InsertRowsSkeleton(Microsoft.Office.Interop.Excel.Worksheet worksheet, int rowCount, int columnCount, int rowOffset = 0)
        {
            Microsoft.Office.Interop.Excel.Range beginWrite = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[2 + rowOffset, 1];
            Microsoft.Office.Interop.Excel.Range endWrite = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[rowCount, columnCount];
            Microsoft.Office.Interop.Excel.Range addNewRows = worksheet.Range[beginWrite, endWrite];
            addNewRows.Insert(Microsoft.Office.Interop.Excel.XlInsertShiftDirection.xlShiftDown, Microsoft.Office.Interop.Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove);
        }

        internal static bool ExportDbase(string originalFileName, DataTable dataTable, string originalPath, Form mainForm, Action updateLoadingBar = null)
        {
            bool saved = false;

            List<string> duplicates = new List<string>();
            string[] headers = dataTable.HeadersOfDataTableAsString().OrderBy(header => header).ToArray();
            for (int i = 1; i < headers.Length; i++)
            {
                string header = headers[i].Length > DbaseMaxHeaderLength ? headers[i].Substring(0, DbaseMaxHeaderLength) : headers[i];
                string headerBefore = headers[i-1].Length > DbaseMaxHeaderLength ? headers[i-1].Substring(0, DbaseMaxHeaderLength) : headers[i-1];
                if (header == headerBefore)
                {
                    duplicates.Add($"\"{headers[i-1]}\" und \"{headers[i]}\"");
                }
            }

            if(duplicates.Count > 0)
            {
                MessageHandler.MessagesOK(mainForm, MessageBoxIcon.Warning, "Aufgrund der Kürzung von Spaltennamen durch DBASE gibt es Duplikate: \n"+string.Join(" ,\n",duplicates));
                return saved;
            }
            
            string fileName = originalFileName.ToUpper();
            if (fileName.Length > DbaseMaxFileLength)
            {
                fileName = fileName.Substring(0, DbaseMaxFileLength);
            }
            string path = Path.Combine(originalPath, "temp");
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, mainForm);
                return saved;
            }
            string fullpath = Path.Combine(path,fileName+".DBF");
            string fullPathOriginal = Path.Combine(originalPath, originalFileName + ".DBF");

            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }


            int[] max = MaxLengthOfColumns(dataTable);
            if (max.Sum() <= DbaseMaxRecordCharacterLength)
            {
                string query = string.Empty;
                try
                {
                    CreateTable(dataTable, max, path, fileName, ref query);
                }
                catch (Exception ex)
                {
                    DeleteDirectory(path);
                    ErrorHelper.LogMessage($"{ex.ToString() + Environment.NewLine} query:{query};   path: {path}; fileName: {fileName}; headers:[{string.Join("; ", dataTable.HeadersOfDataTableAsString())}]", mainForm);
                    return saved;
                }

                try
                {
                    #region Adjust Header. Update number of records

                    FileStream stream = new FileStream(fullpath, FileMode.Open);
                    byte[] records = BitConverter.GetBytes(dataTable.Rows.Count);
                    byte[] bytes = new byte[1] { 0x1A };

                    stream.Position = 4;
                    stream.Write(records, 0, records.Length);

                    stream.Position = stream.Length - 1;

                    if (stream.ReadByte() == bytes[0])
                    {
                        stream.Position--;
                    }

                    foreach (DataRow row in dataTable.Rows)
                    {
                        StringBuilder builder = new StringBuilder(" ");

                        for (int y = 0; y < dataTable.Columns.Count; y++)
                        {
                            string temp = y >= row.ItemArray.Length ? string.Empty : row[y].ToString();
                            builder.Append(temp.Length > DbaseMaxCharacterLength ? temp.Substring(0, DbaseMaxCharacterLength) : temp.PadRight(max[y]));
                        }
                        stream.Write(DbaseEncoding.GetBytes(builder.ToString()), 0, builder.Length);
                        updateLoadingBar?.Invoke();
                    }

                    stream.Write(bytes, 0, bytes.Length);
                    stream.Close();
                    if (File.Exists(fullPathOriginal))
                    {
                        File.Delete(fullPathOriginal);
                    }
                    File.Move(fullpath, fullPathOriginal);
                    saved = true;
                    #endregion
                }
                catch (Exception ex)
                {
                    ErrorHelper.LogMessage(ex, mainForm);
                }
                finally
                {
                    DeleteDirectory(path);
                }
            }
            else
            {
                DeleteDirectory(path);
                mainForm.MessagesOK(MessageBoxIcon.Warning, $"Die maximal unterstützte Zeilenlänge von {DbaseMaxRecordCharacterLength + 1:n0} Zeichen wurde überschritten!\nDie Datei kann nicht erstellt werden");
            }
            return saved;
        }

        /// <summary>
        /// Depth-first recursive delete, with handling for descendant 
        /// directories open in Windows Explorer.
        /// </summary>
        private static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
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
        }

        private static void CreateTable(DataTable table, int[] max, string path, string filename, ref string query)
        {
            query = CreateQuery(table, filename, max);
            
            OleDbConnection con = new OleDbConnection(GetConnection(path));
            OleDbCommand cmd = new OleDbCommand(query, con);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
            cmd.Dispose();
        }

        private static string CreateQuery(DataTable table, string filename, int[] max)
        {
            StringBuilder csb = new StringBuilder($"create table [{filename}] (");
            for (int i = 0; i < table.Columns.Count; i++)
            {
                csb.Append($"[{table.Columns[i].ColumnName}] varchar({max[i]}),");
            }

            csb[csb.Length - 1] = ')';
            return csb.ToString();
        }

        private static int[] MaxLengthOfColumns(DataTable dataTable)
        {
            int[] max = new int[dataTable.Columns.Count];
            for(int i = 0; i < max.Length; i++)
            {
                max[i] = 1;
            }
            foreach (DataRow row in dataTable.Rows)
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    int length = row.ItemArray[i].ToString().Length;
                    if (length > max[i])
                    {
                        max[i] = length;
                    }
                }
            }
            for (int i = 0; i < max.Length; i++)
            {
                if (max[i] > DbaseMaxCharacterLength)
                {
                    max[i] = DbaseMaxCharacterLength;
                }
            }
            return max;
        }

        private static string GetConnection(string path)
        {
            return $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={path};Extended Properties=dBase IV";
        }

        internal static void ExportTableWithColumnCondition(DataTable originalTable, IEnumerable<ExportCustomItem> items, string filePath, Action stopLoadingBar, Action saveFinished, int codePage, Form mainForm, string continuedNumberColumn)
        {
            new Thread(() =>
            {

                foreach (ExportCustomItem item in items)
                {
                    Dictionary<string, DataTable> Dict = new Dictionary<string, DataTable>();
                    DataTable tableSkeleton = originalTable.Clone();
                    if (item.CheckedAllValues)
                    {
                        foreach (string value in item.AllValues)
                        {
                            DataTable table = tableSkeleton.Copy();
                            table.TableName = $"{item.Name}_{value}";
                            Dict.Add(value, table);
                        }
                    }
                    else
                    {

                        DataTable temp = tableSkeleton.Copy();
                        temp.TableName = item.Name;
                        foreach (string value in item.SelectedValues)
                        {
                            if (!Dict.TryGetValue(value, out DataTable tables))
                            {   
                                Dict.Add(value, temp);
                            }
                        }
                    }
                    foreach(DataRow row in originalTable.Rows)
                    {
                        if (Dict.TryGetValue(row[item.Column].ToString(), out DataTable table))
                        {
                            table.ImportRow(row);
                        }
                    }

                    foreach (DataTable table in Dict.Values.Distinct())
                    {
                        if(continuedNumberColumn != string.Empty)
                        {
                            string col = table.TryAddColumn(continuedNumberColumn);
                            table.Columns[col].SetOrdinal(0);
                            for(int i = 0; i < table.Rows.Count; i++)
                            {
                                table.Rows[i][col] = (i + 1).ToString();
                            }
                        }
                        string FileName = table.TableName;
                        string path = Path.GetDirectoryName(filePath);
                        switch (item.Format)
                        {
                            //CSV
                            case 0:
                                {
                                    ExportCsv(table, path, FileName, codePage, mainForm);
                                }
                                break;

                            //Dbase
                            case 1:
                                {
                                    ExportDbase(FileName, table, path, mainForm);
                                }
                                break;

                            //Excel
                            case 2:
                                {
                                    ExportExcel(table, path, FileName, mainForm);
                                }
                                break;
                        }
                    }
                }
                stopLoadingBar.Invoke();
                saveFinished.Invoke();
            }).Start();
            
        }

        internal static DataTable ExportCount(string selectedValue, int count, bool showFromTo, DataTable oldTable, OrderType orderType)
        {
            //Select selectedValue, count(selectedValue) from main group by selectedValue;
            DataTable table = oldTable.GetSortedView($"[{selectedValue}] asc", orderType, -1).ToTable();
            int columnIndex = table.Columns.IndexOf(selectedValue);
            DataTable newTable = new DataTable();

            if (count == 0)
            {
                Dictionary<string, int> pair = table.GroupCountOfColumn(columnIndex);

                newTable = DataHelper.DictionaryToDataTable(pair, selectedValue, showFromTo);
                newTable.Rows.Add(new string[] { "Gesamt", table.Rows.Count.ToString() });
            }
            else
            {
                Dictionary<string, int> pair = new Dictionary<string, int>();
                foreach (string col in table.HeadersOfDataTable())
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
                    else if (!contains)
                    {
                        pair.Add(item, 1);
                        newTable.Rows.Add(row.ItemArray);
                    }
                }
            }
            return newTable;
        }
    }
}
