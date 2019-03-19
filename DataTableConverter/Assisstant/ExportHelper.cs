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

namespace DataTableConverter
{
    class ExportHelper : MessageHandler
    {
        internal static readonly string ProjectName = "Tabellenkonvertierung";
        internal static readonly string ProjectPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),ProjectName);
        internal static readonly string ProjectPresets = Path.Combine(ProjectPath,"Vorlagen");
        internal static readonly string ProjectProcedures = Path.Combine(ProjectPath,"Funktionen.bin");
        internal static readonly string ProjectTolerance = Path.Combine(ProjectPath,"Toleranzen.bin");
        internal static readonly string ProjectCases = Path.Combine(ProjectPath,"Fälle.bin");
        internal static readonly string ProjectWorkflows = Path.Combine(ProjectPath,"Arbeitsabläufe.bin");
        private static readonly string CSVSeparator = ";";
        private static readonly Encoding DbaseEncoding = Encoding.GetEncoding(850); //858; 850; "ISO-8859-1"; 866
        internal static readonly int DbaseMaxFileLength = 8;


        internal static void checkFolders()
        {
            if (!Directory.Exists(ProjectPath))
            {
                Directory.CreateDirectory(ProjectPath);
                Directory.CreateDirectory(ProjectPresets);
            }
        }
        
        internal static bool saveProcedures(List<Proc> procedures)
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

        internal static bool saveWorkflows(List<Work> workflows)
        {
            bool error = false;
            try
            {
                using (Stream stream = File.Open(ProjectWorkflows, FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, workflows);
                }
            }
            catch (IOException)
            {
                error = true;
            }
            return error;
        }

        internal static bool saveTolerances(List<Tolerance> tolerances)
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

        internal static bool saveCases(List<Case> cases)
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

        internal static void exportCsv(DataTable dt, string directory, string filename)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            sb.AppendLine(string.Join(CSVSeparator, columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(CSVSeparator, fields));
            }
            File.WriteAllText(Path.Combine(directory,filename+".csv"), sb.ToString());
        }

        internal static void exportExcel(DataTable dt, string directory, string filename)
        {
            try
            {
                string workSheetName = "Tabelle 1";

                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application
                {
                    DisplayAlerts = false,
                    Visible = false,
                    ScreenUpdating = false
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
                sheetData.Value2 = data;

                worksheet.Select();
                sheetData.Worksheet.ListObjects.Add(Microsoft.Office.Interop.Excel.XlListObjectSourceType.xlSrcRange,
                                                   sheetData,
                                                   System.Type.Missing,
                                                   Microsoft.Office.Interop.Excel.XlYesNoGuess.xlYes,
                                                   System.Type.Missing).Name = workSheetName;

                workbook.SaveAs(Path.Combine(directory, filename + ".xls"), Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
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
                ErrorHelper.LogMessage(ex);
            }
        }

        internal static void exportDbase(string originalFileName, DataTable dataTable, string originalPath)
        {

            string fileName = originalFileName.ToUpper();
            if (fileName.Length > DbaseMaxFileLength)
            {
                fileName = fileName.Substring(0, DbaseMaxFileLength);
            }
            string path = Path.Combine(originalPath, "temp");
            Directory.CreateDirectory(path);
            string fullpath = Path.Combine(path,fileName+".DBF");
            string fullPathOriginal = Path.Combine(originalPath, originalFileName + ".DBF");

            if (File.Exists(fullPathOriginal))
            {
                File.Delete(fullPathOriginal);
            }


            int[] max = getMaxLengthOfColumns(dataTable);
            createTable(dataTable, max, path, fileName);

            try
            {
                #region Adjust Header. Update number of records

                FileStream stream = new FileStream(fullpath, FileMode.Open);
                byte[] records = BitConverter.GetBytes(dataTable.Rows.Count);
                byte[] bytes = new byte[1] { 0x1A };

                string text = joinTable(dataTable, max);
                stream.Position = 4;
                stream.Write(records, 0, records.Length);

                stream.Position = stream.Length - 1;
                
                if(stream.ReadByte() == bytes[0])
                {
                    stream.Position--;
                }
                
                stream.Write(DbaseEncoding.GetBytes(text), 0, text.Length);
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
                File.Move(fullpath, fullPathOriginal);
                Directory.Delete(path);
                #endregion
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex);
            }

        }

        private static void createTable(DataTable table, int[] max, string path, string filename)
        {
            StringBuilder csb = new StringBuilder($"create table [{filename}] (");
            for (int i = 0; i < table.Columns.Count; i++)
            {
                csb.Append($"[{table.Columns[i].ColumnName}] varchar({max[i]}),");
            }

            csb[csb.Length - 1] = ')';



            OleDbConnection con = new OleDbConnection(GetConnection(path));
            OleDbCommand cmd = new OleDbCommand
            {
                Connection = con
            };
            con.Open();
            cmd.CommandText = csb.ToString();
            cmd.ExecuteNonQuery();
            con.Close();
        }

        private static string joinTable(DataTable table, int[] max)
        {
            StringBuilder builder = new StringBuilder();
            foreach(DataRow row in table.Rows)
            {
                builder.Append(" ");
                for (int y = 0; y < table.Columns.Count; y++)
                {
                    string text = y >= row.ItemArray.Length ? string.Empty : row[y].ToString();
                    builder.Append(text.PadRight(max[y]));
                }
            }
            return builder.ToString();
        }

        private static int[] getMaxLengthOfColumns(DataTable dataTable)
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
            return max;
        }

        private static string GetConnection(string path)
        {
            return $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={path};Extended Properties=dBase IV";
        }
    }
}
