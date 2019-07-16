﻿using DataTableConverter.Assisstant;
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

namespace DataTableConverter
{
    class ExportHelper : MessageHandler
    {
        [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true)]
        public static extern int AddFontResource([In][MarshalAs(UnmanagedType.LPWStr)]
                                         string lpFileName);

        internal static readonly string ProjectName = "Tabellenkonvertierung";
        internal static readonly string ProjectPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),ProjectName);
        internal static readonly string ProjectPresets = Path.Combine(ProjectPath,"Vorlagen");
        internal static readonly string ProjectProcedures = Path.Combine(ProjectPath,"Funktionen.bin");
        internal static readonly string ProjectTolerance = Path.Combine(ProjectPath,"Toleranzen.bin");
        internal static readonly string ProjectCases = Path.Combine(ProjectPath,"Fälle.bin");
        internal static readonly string ProjectWorkflows = Path.Combine(ProjectPath,"Arbeitsabläufe.bin");
        internal static readonly string ProjectHeaderPresets = Path.Combine(ProjectPath, "Vorlagen Überschriften");
        private static readonly string CSVSeparator = ";";
        private static readonly Encoding DbaseEncoding = Encoding.GetEncoding(850); //858; 850; "ISO-8859-1"; 866
        internal static readonly int DbaseMaxFileLength = 8;
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

        internal static bool SaveWorkflows(List<Work> workflows)
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

        internal static void ExportCsv(DataTable dt, string directory, string filename)
        {
            var writer = File.CreateText(Path.Combine(directory, filename + ".csv"));
            
            writer.WriteLine(string.Join(CSVSeparator, dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName)));
            foreach (DataRow row in dt.Rows)
            {
                writer.WriteLine(string.Join(CSVSeparator, row.ItemArray.Select(field => field.ToString())));
            }
            writer.Close();
            
        }

        internal static string ExportExcel(DataTable dt, string directory, string filename)
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
                    MessagesOK(MessageBoxIcon.Warning, "Die Datei konnte nicht gespeichert werden! Wird die Datei gerade verwendet?");
                    ErrorHelper.LogMessage(ex,false);
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
                ErrorHelper.LogMessage(ex);
            }
            return path;
        }

        internal static void ExportDbase(string originalFileName, DataTable dataTable, string originalPath)
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


            int[] max = MaxLengthOfColumns(dataTable);
            try
            {
                CreateTable(dataTable, max, path, fileName);
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage($"{ex.ToString() + Environment.NewLine}    path: {path}; fileName: {fileName}; headers:[{string.Join("; ",dataTable.HeadersOfDataTableAsString())}]");
                return;
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
                
                if(stream.ReadByte() == bytes[0])
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
                        builder.Append(temp.PadRight(max[y]));
                    }
                    stream.Write(DbaseEncoding.GetBytes(builder.ToString()), 0, builder.Length);
                }

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

        private static void CreateTable(DataTable table, int[] max, string path, string filename)
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
            return max;
        }

        private static string GetConnection(string path)
        {
            return $@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={path};Extended Properties=dBase IV";
        }
    }
}
