using ClosedXML.Excel;
using DataTableConverter.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;

namespace DataTableConverter
{
    class ExportHelper : MessageHandler
    {
        internal static readonly string ProjectName = "Tabellenkonvertierung";
        internal static readonly string ProjectPath = $@"{ Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) }\{ProjectName}";
        internal static readonly string ProjectPresets = $@"{ProjectPath}\Vorlagen";
        internal static readonly string ProjectProcedures = $@"{ProjectPath}\Funktionen.bin";
        internal static readonly string ProjectTolerance = $@"{ProjectPath}\Toleranzen.bin";
        internal static readonly string ProjectCases = $@"{ProjectPath}\Fälle.bin";
        internal static readonly string ProjectWorkflows = $@"{ProjectPath}\Arbeitsabläufe.bin";
        private static readonly Encoding DbaseEncoding = Encoding.GetEncoding(858);


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
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }
            File.WriteAllText($@"{directory}\{filename}.csv", sb.ToString());
        }

        internal static void exportExcel(DataGridView dgTable)
        {
            if (dgTable.DataSource != null)
            {
                copyAlltoClipboard(out IDataObject clipboardBefore, dgTable);
                Microsoft.Office.Interop.Excel.Application xlexcel;
                Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
                Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet;
                object misValue = Missing.Value;

                xlexcel = new Microsoft.Office.Interop.Excel.Application();
                xlexcel.Visible = true;
                xlWorkBook = xlexcel.Workbooks.Add(misValue);
                xlWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
                Microsoft.Office.Interop.Excel.Range CR = (Microsoft.Office.Interop.Excel.Range)xlWorkSheet.Cells[1, 1];
                CR.Select();
                xlWorkSheet.PasteSpecial(CR, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, true);

                Clipboard.Clear();
                Clipboard.SetDataObject(clipboardBefore, true);
            }
        }

        internal static void exportExcel(DataTable dt, string directory, string filename)
        {
            try
            {
                XLWorkbook wb = new XLWorkbook();
                wb.Worksheets.Add(dt, "Tabelle1");
                wb.SaveAs($@"{directory}\{filename}.xlsx");
            }
            catch(Exception ex)
            {
                MessagesOK(MessageBoxIcon.Error, "Es ist ein Fehler beim Speichern aufgetreten\nFehler:" + ex.Message);
            }
        }

        private static void copyAlltoClipboard(out IDataObject clipboardBefore, DataGridView dgTable)
        {
            clipboardBefore = Clipboard.GetDataObject();
            dgTable.SelectAll();
            DataObject dataObj = dgTable.GetClipboardContent();
            if (dataObj != null)
            {
                Clipboard.SetDataObject(dataObj);
            }
            dgTable.ClearSelection();
        }

        internal static void exportDbase(string fileName, DataTable dataTable, string path)
        {
            int maxFileLength = 8;
            fileName = fileName.ToUpper();
            if(fileName.Length > maxFileLength)
            {
                fileName = fileName.Substring(0, maxFileLength);
            }

            string fullpath = $@"{path}\{fileName}.DBF";

            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }

            
            int[] max = getMaxLengthOfColumns(dataTable);
            createTable(dataTable, max, path, fileName);
            
            #region oldInsert
            //DataRow row = dataTable.Rows[0];
            //cmd.Parameters.Clear();
            //StringBuilder query = new StringBuilder($"insert into {fileName} values(");

            //for (int i = 0; i < row.ItemArray.Length; i++)
            //{
            //    string id = "@" + i;
            //    query.Append(id + ",");
            //    cmd.Parameters.AddWithValue(id, row.ItemArray[i].ToString());
            //}

            //query[query.Length - 1] = ')';
            //cmd.CommandText = query.ToString();

            //cmd.ExecuteNonQuery();
            #endregion


            try
            {
                #region Adjust Header. Update number of records

                FileStream stream = new FileStream(fullpath, FileMode.Open);
                byte[] records = BitConverter.GetBytes(dataTable.Rows.Count);
                byte[] bytes = new byte[1];
                bytes[0] = 0x1A;

                string text = joinTable(dataTable, max);
                stream.Position = 4;
                stream.Write(records, 0, records.Length);

                stream.Position = stream.Length;
                
                if(stream.ReadByte() == bytes[0])
                {
                    stream.Position--;
                }
                
                stream.Write(DbaseEncoding.GetBytes(text), 0, text.Length);
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();

                #endregion
            }
            catch (Exception ex)
            {
                MessagesOK(MessageBoxIcon.Error, ex.Message);
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
            //return $"Provider=vfpoledb.1;Data Source={path};Collating Sequence = machine; ";
        }
    }
}
