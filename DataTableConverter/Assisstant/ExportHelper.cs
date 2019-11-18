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
        internal static string ProjectHeaderPresets => Path.Combine(ProjectPath, "Vorlagen Überschriften");
        private static readonly string CSVSeparator = ";";
        private static readonly Encoding DbaseEncoding = Encoding.GetEncoding(850); //858; 850; "ISO-8859-1"; 866
        internal static readonly int DbaseMaxFileLength = 8;
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
        
        internal static bool SaveProcedures(List<Proc> procedures)
        {
            bool error = false;
            try
            {
                using (Stream stream = File.Open(ProjectProcedures, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, procedures);
                }
            }
            catch (IOException)
            {
                error = true;
            }
            return error;
        }

        internal static bool SaveWorkflows(List<Work> workflows, Form mainForm)
        {
            bool error = false;
            List<string> files = new List<string>(GetWorkflows());
            foreach (Work work in workflows.Where(work => work.Name != null))
            {
                try
                {
                    string path = Path.Combine(WorkflowPath, work.Name+".bin");
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
            return Directory.GetFiles(WorkflowPath, "*.bin"); ;
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

        internal static void ExportCsv(DataTable dt, string directory, string filename, Action updateLoadingBar = null)
        {
            StreamWriter writer = File.CreateText(Path.Combine(directory, filename + ".csv"));
            
            writer.WriteLine(string.Join(CSVSeparator, dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName)));
            foreach (DataRow row in dt.Rows)
            {
                writer.WriteLine(string.Join(CSVSeparator, row.ItemArray.Select(field => field.ToString())));
                updateLoadingBar?.Invoke();
            }
            writer.Close();
            
        }

        internal static string ExportExcel(DataTable dt, string directory, string filename, Form mainForm)
        {
            string path = null;
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

                int rows = dt.Rows.Count;
                int columns = dt.Columns.Count;
                // Add the +1 to allow room for column headers.
                var data = new object[rows + 1, columns];

                // Insert column headers.
                for (var column = 0; column < columns; column++)
                {
                    data[0, column] = dt.Columns[column].ColumnName;
                }

                // Insert the provided records.
                for (var row = 0; row < rows; row++)
                {
                    for (var column = 0; column < columns; column++)
                    {
                        data[row + 1, column] = dt.Rows[row][column];
                    }
                }

                // Write this data to the excel worksheet.
                Microsoft.Office.Interop.Excel.Range beginWrite = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, 1];
                Microsoft.Office.Interop.Excel.Range endWrite = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[rows + 1, columns];
                Microsoft.Office.Interop.Excel.Range sheetData = worksheet.Range[beginWrite, endWrite];
                sheetData.NumberFormat = "@";
                sheetData.Value2 = data;

                worksheet.Select();
                sheetData.Worksheet.ListObjects.Add(Microsoft.Office.Interop.Excel.XlListObjectSourceType.xlSrcRange,
                                                   sheetData,
                                                   System.Type.Missing,
                                                   Microsoft.Office.Interop.Excel.XlYesNoGuess.xlYes,
                                                   System.Type.Missing).Name = workSheetName;
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
                    workbook.SaveAs(Path.Combine(directory, saveName), fileFormat, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
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

        internal static bool ExportDbase(string originalFileName, DataTable dataTable, string originalPath, Form mainForm, Action updateLoadingBar = null)
        {
            bool saved = false;
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
                        StringBuilder builder = new StringBuilder();
                        builder.Append(" ");
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

        internal static void ExportTableWithColumnCondition(DataTable originalTable, IEnumerable<ExportCustomItem> items, string filePath, Action stopLoadingBar, Action saveFinished, Form mainForm)
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
                        string FileName = table.TableName;
                        string path = Path.GetDirectoryName(filePath);
                        switch (item.Format)
                        {
                            //CSV
                            case 0:
                                {
                                    ExportCsv(table, path, FileName);
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
            DataTable table = oldTable.GetSortedView($"[{selectedValue}] asc", orderType).ToTable();
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
