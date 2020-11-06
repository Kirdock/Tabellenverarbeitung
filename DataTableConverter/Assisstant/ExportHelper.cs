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
using Microsoft.Office.Interop.Excel;

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

        internal static int Save(string directory, string fileName, string oldFileExtension, int encoding, int format, Form invokeForm, System.Action updateLoadingBar = null, string tableName = "main", string orderColumnName = null)
        {
            SQLiteCommand command = orderColumnName == string.Empty ? DatabaseHelper.GetDataCommand(tableName) : DatabaseHelper.GetDataCommand(tableName, orderColumnName);
            return Save(directory, fileName, oldFileExtension, encoding, format, invokeForm, command, updateLoadingBar, tableName);
        }

        internal static int Save(string directory, string fileName, string oldFileExtension, int encoding, int format, Form invokeForm, SQLiteCommand command, System.Action updateLoadingBar = null, string tableName = "main")
        {
            int rowCount = 0;
            if (command == null)
            {
                command = DatabaseHelper.GetDataCommand(tableName);
            }

            switch (format)
            {
                //CSV
                case 0:
                    rowCount = ExportCsv(directory, fileName, encoding, command, invokeForm, updateLoadingBar);
                    break;

                //Dbase
                case 1:
                    rowCount = ExportDbase(tableName, directory, fileName, command, invokeForm);
                    break;

                //Excel
                case 2:
                    rowCount = ExportExcel(directory, fileName, oldFileExtension, command, invokeForm);
                    break;
            }
            return rowCount;
        }

        private static int ExportDbase(string tableName, string directory, string fileName, SQLiteCommand command, Form invokeForm)
        {
            int offset = 0;
            List<string> duplicates = new List<string>();
            string[] headers = DatabaseHelper.GetSortedColumnsAsAlias(tableName).ToArray();
            for (int i = 1; i < headers.Length; i++)
            {
                string header = headers[i].Length > DbaseMaxHeaderLength ? headers[i].Substring(0, DbaseMaxHeaderLength) : headers[i];
                string headerBefore = headers[i - 1].Length > DbaseMaxHeaderLength ? headers[i - 1].Substring(0, DbaseMaxHeaderLength) : headers[i - 1];
                if (header == headerBefore)
                {
                    duplicates.Add($"\"{headers[i - 1]}\" und \"{headers[i]}\"");
                }
            }

            if (duplicates.Count > 0)
            {
                MessageHandler.MessagesOK(invokeForm, MessageBoxIcon.Warning, "Aufgrund der Kürzung von Spaltennamen durch DBASE gibt es Duplikate: \n" + string.Join(" ,\n", duplicates));
                return 0;
            }

            
            if (fileName.Length > DbaseMaxFileLength)
            {
                fileName = fileName.Substring(0, DbaseMaxFileLength);
            }
            string path = Path.Combine(Path.GetDirectoryName(directory), "temp");
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, invokeForm);
                return 0;
            }
            string fullpath = Path.Combine(path, fileName + ".DBF");
            string fullPathOriginal = Path.Combine(directory, fileName + ".DBF");

            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }

            int[] max = DatabaseHelper.GetMaxColumnLength(tableName);
            if (max.Sum() <= DbaseMaxRecordCharacterLength)
            {
                string[] columns = DatabaseHelper.GetSortedColumnsAsAlias(tableName).ToArray();
                string query = string.Empty;
                try
                {
                    CreateTable(columns, max, path, fileName, ref query);
                }
                catch (Exception ex)
                {
                    DeleteDirectory(path);
                    ErrorHelper.LogMessage($"{ex.ToString() + Environment.NewLine} query:{query};   path: {path}; fileName: {fileName}; headers:[{string.Join("; ", columns)}]", invokeForm);
                    return 0;
                }

                try
                {
                    #region Adjust Header. Update number of records
                    using (FileStream stream = new FileStream(fullpath, FileMode.Open))
                    {
                        byte[] bytes = new byte[1] { 0x1A };
                        stream.Position = stream.Length - 1;

                        if (stream.ReadByte() == bytes[0])
                        {
                            stream.Position--;
                        }

                        bool running = true;
                        while (running)
                        {
                            command.Parameters["$offset"].Value = offset;

                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.HasRows)
                                {
                                    running = false;
                                }
                                else
                                {
                                    int y = 0;
                                    for (; reader.Read(); y++)
                                    {
                                        StringBuilder builder = new StringBuilder(" ");
                                        for (int i = 0; i < columns.Length; i++)
                                        {
                                            string temp = reader.GetString(i);
                                            builder.Append(temp.Length > DbaseMaxCharacterLength ? temp.Substring(0, DbaseMaxCharacterLength) : temp.PadRight(max[i]));
                                        }
                                        stream.Write(DbaseEncoding.GetBytes(builder.ToString()), 0, builder.Length);
                                    }
                                    offset += y;
                                }
                            }
                        }

                        stream.Write(bytes, 0, bytes.Length);

                        byte[] records = BitConverter.GetBytes(offset);
                        stream.Position = 4;
                        stream.Write(records, 0, records.Length);
                        #endregion
                    }
                    if (File.Exists(fullPathOriginal))
                    {
                        File.Delete(fullPathOriginal);
                    }
                    File.Move(fullpath, fullPathOriginal);
                }
                catch (Exception ex)
                {
                    ErrorHelper.LogMessage(ex, invokeForm);
                    offset = 0;
                }
                finally
                {
                    DeleteDirectory(path);
                }
            }
            else
            {
                DeleteDirectory(path);
                invokeForm.MessagesOK(MessageBoxIcon.Warning, $"Die maximal unterstützte Zeilenlänge von {DbaseMaxRecordCharacterLength + 1:n0} Zeichen wurde überschritten!\nDie Datei kann nicht erstellt werden");
                offset = 0;
            }
            return offset;
        }

        internal static int ExportCsv(string directory, string fileName, int encoding, SQLiteCommand command, Form invokeForm, System.Action updateLoadingBar = null)
        {
            int offset = 0;
            string path = Path.Combine(directory,fileName+ ".csv");

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
                        bool running = true;
                        while (running)
                        {
                            command.Parameters["$offset"].Value = offset;

                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.HasRows)
                                {
                                    running = false;
                                }
                                else
                                {
                                    int rowCount = 0;
                                    if (offset == 0) //write header
                                    {
                                        for (var i = 0; i < reader.FieldCount - 1; i++)
                                        {
                                            writer.Write(reader.GetName(i));
                                            writer.Write(CSVSeparator);
                                        }
                                        writer.Write(reader.GetName(reader.FieldCount - 1));
                                        writer.Write(writer.NewLine);
                                    }

                                    for (; reader.Read(); rowCount++)
                                    {

                                        for (int i = 0; i < reader.FieldCount - 1; i++)
                                        {
                                            writer.Write(reader.GetString(i));
                                            writer.Write(CSVSeparator);
                                        }
                                        writer.Write(reader.GetString(reader.FieldCount - 1));
                                        writer.Write(writer.NewLine);
                                        updateLoadingBar?.Invoke();
                                    }
                                    running = rowCount < Properties.Settings.Default.MaxRows;
                                    offset += rowCount;
                                }
                            }
                        }
                    }
                }
            }
            command.Dispose();
            return offset;
        }

        private static int ExportExcel(string directory, string fileName, string oldFileExtension, SQLiteCommand command, Form invokeForm)
        {
            int offset = 0;
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

                

                bool running = true;
                string[] columnNames = new string[0];
                
                while (running)
                {
                    command.Parameters["$offset"].Value = offset;

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (offset == 0) //write header
                        {
                            columnNames = new string[reader.FieldCount];
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                columnNames[i] = reader.GetName(i);
                            }
                            InsertHeadersToExcel(columnNames, worksheet);
                        }

                        if (reader.HasRows)
                        {
                            int rowCount = 0;
                            object[,] data = new object[(int)Properties.Settings.Default.MaxRows, columnNames.Length];
                            for (; reader.Read(); rowCount++)
                            {
                                object[] row = new object[columnNames.Length];
                                for (int y = 0; y < columnNames.Length; y++)
                                {
                                    data[rowCount, y] = reader.GetString(y);
                                }
                            }

                            InsertRowsSkeleton(worksheet, rowCount, columnNames.Length, offset);
                            InsertRowsToExcel(worksheet, data, offset + 2, rowCount - 1, columnNames.Length); //+2 because index starts at 1 and header is first

                            offset += rowCount;
                            running = rowCount < Properties.Settings.Default.MaxRows;
                        }
                        else
                        {
                            running = false;
                        }
                    }
                }
                command.Dispose();

                SaveExcelFile(directory, fileName, oldFileExtension, workbook, invokeForm);
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex, invokeForm);
                offset = 0;
            }
            finally
            {
                workbook?.Close(false, Type.Missing, Type.Missing);
                excel?.Quit();
                


                // Release our resources.
                if(workbook != null) Marshal.ReleaseComObject(workbook);
                if(workbooks != null) Marshal.ReleaseComObject(workbooks);
                if (excel != null)
                {
                    Marshal.ReleaseComObject(excel);
                    Marshal.FinalReleaseComObject(excel);
                }
            }

            return offset;
        }

        private static void SaveExcelFile(string directory, string fileName, string oldFileExtension, Workbook workbook, Form invokeForm)
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

        internal static string ExportExcel(System.Data.DataTable dt, string directory, string filename, Form mainForm)
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

        private static Microsoft.Office.Interop.Excel.ListObject InsertHeadersToExcel(System.Data.DataTable table, Microsoft.Office.Interop.Excel.Worksheet worksheet)
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

        internal static bool ExportDbase(string originalFileName, System.Data.DataTable dataTable, string originalPath, Form mainForm, System.Action updateLoadingBar = null)
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

        private static void CreateTable(System.Data.DataTable table, int[] max, string path, string filename, ref string query)
        {
            CreateTable(table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray(), max, path, filename, ref query);
        }

        private static void CreateTable(string[] columns, int[] max, string path, string filename, ref string query)
        {
            query = CreateQuery(columns, filename, max);

            OleDbConnection con = new OleDbConnection(GetConnection(path));
            OleDbCommand cmd = new OleDbCommand(query, con);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
            cmd.Dispose();
        }

        private static string CreateQuery(string[] columns, string filename, int[] max)
        {
            StringBuilder csb = new StringBuilder($"create table [{filename}] (");
            for (int i = 0; i < columns.Length; i++)
            {
                csb.Append($"[{columns[i]}] varchar({max[i]}),");
            }

            csb[csb.Length - 1] = ')';
            return csb.ToString();
        }

        private static int[] MaxLengthOfColumns(System.Data.DataTable dataTable)
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
        internal static void ExportTableWithColumnCondition(IEnumerable<ExportCustomItem> items, string filePath, System.Action stopLoadingBar, System.Action saveFinished, int codePage, Form mainForm, string continuedNumberColumn, string tableName = "main")
        {
            new Thread(() =>
            {

                foreach (ExportCustomItem item in items)
                {
                    Dictionary<string, string[]> dict = new Dictionary<string, string[]>();
                    IEnumerable<string> itemValues = item.CheckedAllValues ? item.AllValues : item.SelectedValues;

                    foreach (string value in itemValues)
                    {
                        string newTable = Guid.NewGuid().ToString();
                        DatabaseHelper.CreateTable(DatabaseHelper.GetSortedColumnsAsAlias(tableName).ToArray(), newTable);

                        dict.Add(value, new string[] { newTable, $"{item.Name}_{value}" });
                    }

                    DatabaseHelper.SplitTableOnRowValue(dict,item.Column);

                    foreach (string[] tableInfo in dict.Values.Distinct())
                    {
                        Save(Path.GetDirectoryName(filePath), tableInfo[1], Path.GetExtension(filePath), codePage, item.Format, mainForm, null, tableInfo[0], continuedNumberColumn);
                    }
                }
                stopLoadingBar.Invoke();
                saveFinished.Invoke();
            }).Start();
            
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
        internal static string ExportCount(string columnName, int count, bool showFromTo, OrderType orderType, string tableName = "main")
        {
            string newTable = Guid.NewGuid().ToString();

            if (count == 0)
            {
                Dictionary<string, int> pair = DatabaseHelper.GroupCountOfColumn(columnName, tableName);
                DatabaseHelper.DictionaryToDataTable(pair, columnName, showFromTo, newTable, DatabaseHelper.GetRowCount(tableName));
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
