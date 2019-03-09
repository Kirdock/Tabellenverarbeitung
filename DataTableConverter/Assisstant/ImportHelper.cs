using DataTableConverter.Classes;
using DataTableConverter.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DataTableConverter.Assisstant
{
    class ImportHelper : MessageHandler
    {

        internal static readonly string ProjectWorkflows = ExportHelper.ProjectWorkflows;
        internal static readonly string ProjectProcedures = ExportHelper.ProjectProcedures;
        internal static readonly string ProjectTolerances = ExportHelper.ProjectTolerance;
        internal static readonly string ProjectCases = ExportHelper.ProjectCases;

        internal static DataTable openText(string path, string separator, int codePage, bool isPreview = false)
        {
            DataTable dt = new DataTable();

            try
            {
                //File.ReadLines(path).Take(1)
                //.SelectMany(x => x.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                //.ToList()
                //.ForEach(x => dt.Columns.Add(x.Trim()));

                List<string> list = File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(1)
                .SelectMany(x => x.Split(new string[] { separator }, StringSplitOptions.None))
                .Select(ln => ln.Trim()).ToList();

                foreach (string column in list)
                {
                    DataHelper.addColumn(column.Trim(), dt);
                }


                if (isPreview)
                {
                    File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(4).Skip(1)
                    .Select(x => x.Split(new string[] { separator }, StringSplitOptions.None))
                    .ToList()
                    .ForEach(line => dt.Rows.Add(line.Select(ln => ln.ToString().Trim()).ToArray()));
                }
                else
                {
                    File.ReadLines(path, Encoding.GetEncoding(codePage)).Skip(1)
                    .Select(x => x.Split(new string[] { separator }, StringSplitOptions.None))
                    .ToList()
                    .ForEach(line => dt.Rows.Add(line.Select(ln => ln.ToString().Trim()).ToArray()));
                }

                //File.ReadLines doesn't read all lines, it returns a IEnumerable, and lines are lazy evaluated,
                //  so just the first line will be loaded two times.
            }
            catch (IOException)
            {
                MessagesOK(MessageBoxIcon.Warning, "Die Datei wird zurzeit von einem anderen Programm benutzt und kann nicht geöffnet werden.");
            }
            catch (ArgumentException)
            {
                MessagesOK(MessageBoxIcon.Warning, "Die Zeile hat mehr Spalten als erlaubt");
                dt = null;
            }
            return dt;
        }

        internal static DataTable openTextBetween(string path, int codePage, string begin, string end, bool isPreview = false)
        {
            DataTable dt = new DataTable();
            //bool moreColumnsInRow = false;
            try
            {
                string headerLine = File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(1).ToArray()[0].ToString();
                string[] headerRow = createRow(headerLine, begin, end);
                int columnCount = headerRow.Length;

                foreach (string field in headerRow)
                {
                    DataHelper.addColumn(field, dt);
                }

                string[] lines = new string[0];
                if (isPreview)
                {
                    lines = File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(4).Skip(1).ToArray();
                }
                else
                {
                    lines = File.ReadLines(path, Encoding.GetEncoding(codePage)).Skip(1).ToArray();
                }

                foreach(string line in lines)
                {
                    string[] row = createRow(line, begin, end);
                    if(row.Length > columnCount)
                    {
                        row = row.Take(columnCount).ToArray();
                        //moreColumnsInRow = true;
                    }
                    dt.Rows.Add(row);
                }
            }
            catch (IOException)
            {
                MessagesOK(MessageBoxIcon.Warning, "Die Datei wird zurzeit von einem anderen Programm benutzt und kann nicht geöffnet werden.");
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

        private static int renameColumn(DataTable dt, string column, int counter)
        {
            try
            {
                dt.Columns[column].ColumnName = $"{column}{counter}";
                return counter;
            }
            catch (DuplicateNameException)
            {
                counter++;
                return renameColumn(dt, column, counter);
            }
        }

        internal static DataTable openDBF(string path)
        {
            DataTable data = new DataTable();
            string directory = Path.GetDirectoryName(path);
            string shortPath = GetShortFileName(path);
            string constr = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={directory};Extended Properties=\"dBASE IV;CharacterSet={Encoding.Default.CodePage};\"";
            OleDbConnection con = new OleDbConnection(constr);

            var sql = $@"select * from [{ shortPath}]";
            OleDbCommand cmd = new OleDbCommand(sql, con);
            con.Open();
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            da.Fill(data);
            return data;
        }

        internal static DataTable OpenMSAccess(string path)
        {
            OleDbConnection Con = new OleDbConnection {
                ConnectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};"
            };
            
            DataTable Table = new DataTable();

            try
            {
                Con.Open();
                DataTable Tables = Con.GetSchema("Tables");
                foreach (DataRow row in Tables.Rows)
                {
                    if (row["TABLE_TYPE"].ToString() == "TABLE")
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        using (OleDbDataAdapter dbAdapter = new OleDbDataAdapter($"Select * from [{tableName}]", Con))
                        {
                            string fileName = Path.GetFileName(path) + "; " + tableName;
                            DataTable temp = new DataTable();
                            dbAdapter.Fill(temp);
                            DataHelper.concatTables(Table, temp, fileName, fileName);
                        }
                    }
                }
                Con.Close();
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex);
            }
            return Table;
        }

        internal static DataTable openExcel(string path, Form1 mainform)
        {
            DataTable data = new DataTable();

            #region old without password
            //string constr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Extended Properties='Excel 12.0 XML;HDR=YES;IMEX=1';";
            //string sheetName = null;
            //OleDbConnection con = new OleDbConnection(constr);
            //con.Open();
            //DataTable dataSchema = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

            //if (dataSchema == null)
            //{
            //    MessagesOK(MessageBoxIcon.Warning, "Die Datei ist leer!");
            //}
            //else
            //{
            //    Regex r = new Regex(@"^\'.*?\'$", RegexOptions.IgnoreCase);

            //    string[] sheets = dataSchema.Rows.Cast<DataRow>()
            //        .Select(row => row["TABLE_NAME"].ToString())
            //        .Where(sheet =>sheet.EndsWith("$") || sheet.EndsWith("$'"))
            //        .Select(sheet => {
            //            int offset = r.Match(sheet).Success ? 1 : 0;
            //            return sheet.Substring(offset, sheet.Length - 1 - offset * 2);
            //        }).ToArray();

            //    if (sheets.Length == 1)
            //    {
            //        sheetName = sheets[0];
            //    }
            //    else
            //    {
            //        ExcelSheets form = new ExcelSheets(sheets);
            //        if (form.ShowDialog() == DialogResult.OK)
            //        {
            //            sheetName = form.getSheet();
            //        }
            //    }
            //    if (sheetName != null)
            //    {
            //        OleDbCommand oconn = new OleDbCommand($"Select * From [{sheetName}$]", con);
            //        OleDbDataAdapter sda = new OleDbDataAdapter(oconn);
            //        sda.Fill(data);
            //    }
            //}
            //con.Close();
            #endregion

            
            Microsoft.Office.Interop.Excel.Application objXL = null;
            Microsoft.Office.Interop.Excel.Workbook objWB = null;

            try
            {
                objXL = new Microsoft.Office.Interop.Excel.Application();

                bool hasPassword = false;
                string password = string.Empty;
                do
                {
                    try
                    {
                        objWB = objXL.Workbooks.Open(Filename: path, ReadOnly: true, Password: password);
                        hasPassword = false;
                    }
                    catch (System.Runtime.InteropServices.COMException ex)
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
                
                string[] selectedSheets = (string[])mainform.Invoke(
                    new Func<string[]>(() =>
                    selectExcelSheets(objWB.Worksheets.Cast<Microsoft.Office.Interop.Excel.Worksheet>().Select(x => x.Name).ToArray())
                    )
                );
                
                foreach (string sheetName in selectedSheets)
                {
                    Microsoft.Office.Interop.Excel.Worksheet objSHT = objWB.Worksheets[sheetName];
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

                    int[] columns = new int[cols];
                    int noofrow = 1;


                    //Get ColumnName from first Row
                    for (int c = 1; c <= cols; c++)
                    {
                        string colname = objSHT.Cells[1, c].Text;
                        if (!data.Columns.Contains(colname))
                        {
                            data.Columns.Add(colname);
                        }
                        columns[c - 1] = data.Columns.IndexOf(colname);
                        noofrow = 2;
                    }
                    //END

                    Microsoft.Office.Interop.Excel.Range c1 = objSHT.Cells[noofrow, 1];
                    Microsoft.Office.Interop.Excel.Range c2 = objSHT.Cells[rows, cols];
                    Microsoft.Office.Interop.Excel.Range range = objSHT.get_Range(c1, c2);

                    object[,] values = (object[,])range.Value2;


                    for (int i = 1; i <= values.GetLength(0); i++)
                    {
                        DataRow dr = data.NewRow();
                        for (int j = 1; j <= values.GetLength(1); j++)
                        {
                            dr[columns[j - 1]] = values[i, j];
                        }
                        data.Rows.Add(dr);
                    }
                }
                objWB.Close();
                objXL.Quit();
            }
            catch (Exception ex)
            {
                objWB.Close();
                objXL.Quit();
                MessagesOK(MessageBoxIcon.Error, $"Es ist ein Fehler aufgetreten!\nMöglicherweise Falsches Passwort?\nFehler:{ex.Message}");
            }

            return data;
        }

        internal static DataTable openTextFixed(string data, string path, List<int> config, List<string> header, bool isPreview = false)
        {
            DataTable dt = new DataTable();
            if (header == null || config == null || header.Count == 0 || config.Count == 0)
            {
                return dt;
            }
            try
            {
                header.ForEach(x => dt.Columns.Add(x.ToString()));

                int startindex = 0;
                while (startindex < data.Length && (isPreview && dt.Rows.Count < 3 || !isPreview))
                {
                    List<string> row = new List<string>();

                    foreach (int line in config)
                    {
                        row.Add(data.Substring(startindex, line));
                        startindex += line;
                    }
                    dt.Rows.Add(row.ToArray());
                }


                //File.ReadLines doesn't read all lines, it returns a IEnumerable, and lines are lazy evaluated,
                //  so just the first line will be loaded two times.
            }
            catch (IOException)
            {
                MessagesOK(MessageBoxIcon.Warning, "Die Datei wird zurzeit von einem anderen Programm benutzt und kann nicht geöffnet werden.");
            }
            return dt;
        }

        internal static List<Proc> loadProcedures()
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
            }
            return data;
        }

        internal static List<Work> loadWorkflows()
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
            }
            return data;
        }

        internal static List<Tolerance> loadTolerances()
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

        internal static List<Case> loadCases()
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

        private static string[] selectExcelSheets(string[] sheets)
        {
            string[] checkedSheets = new string[0];
            if (sheets.Length == 1)
            {
                return sheets;
            }
            else
            {
                ExcelSheets form = new ExcelSheets(sheets);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    checkedSheets = form.GetSheets();
                }
                return checkedSheets;
            }
        }

        internal static string GetShortFileName(string path)
        {
            StringBuilder temp = new StringBuilder(255);

            int n = GetShortPathName(path, temp, 255);

            return ((temp.ToString().Split('\\')).Last()).ToLower();
        }
        private string LongFileName(string shortName)
        {
            return new FileInfo(shortName).FullName;
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern int GetShortPathName(
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
        string path,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPTStr)]
        StringBuilder shortPath,
            int shortPathLength);

    }
}
