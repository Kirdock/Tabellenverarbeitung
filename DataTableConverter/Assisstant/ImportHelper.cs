using DataTableConverter.Classes;
using DataTableConverter.Extensions;
using DataTableConverter.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
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
    class ImportHelper : MessageHandler
    {

        internal static readonly string ProjectWorkflows = ExportHelper.ProjectWorkflows;
        internal static readonly string ProjectProcedures = ExportHelper.ProjectProcedures;
        internal static readonly string ProjectTolerances = ExportHelper.ProjectTolerance;
        internal static readonly string ProjectCases = ExportHelper.ProjectCases;
        internal static readonly string TextExt = "*.txt";
        internal static readonly string AccessExt = "*.accdb;*.accde;*.accdt;*.accdr;*.mdb";
        internal static readonly string DbfExt = "*.dbf";
        internal static readonly string CsvExt = "*.csv";
        internal static readonly string ExcelExt = "*.xlsx;*.xlsm;*.xlsb;*.xltx;*.xltm;*.xls;*.xlt;*.xls;*.xml;*.xml;*.xlam;*.xla;*.xlw;*.xlr;";


        internal static DataTable ImportFile(string file, Form1 mainform, bool multipleFiles, Dictionary<string, ImportSettings> fileImportSettings, ContextMenuStrip ctxRow)
        {
            string filename = Path.GetFileName(file);
            string extension = Path.GetExtension(file).ToLower();
            DataTable table = null;

            if (extension == ".dbf")
            {
                table = OpenDBF(file);
            }
            else if (extension != string.Empty && AccessExt.Contains(extension))
            {
                table = OpenMSAccess(file);
            }
            else if (extension != string.Empty && ExcelExt.Contains(extension))
            {
                table = OpenExcel(file, mainform);
            }
            else
            {
                if (fileImportSettings != null && fileImportSettings.TryGetValue(extension, out ImportSettings settings))
                {
                    if (settings.Values != null)
                    {
                        table = OpenTextFixed(file, settings.Values, settings.Headers, settings.CodePage);
                    }
                    else if (settings.Separator != null)
                    {
                        table = OpenText(file, settings.Separator, settings.CodePage, settings.ContainsHeaders, settings.Headers.ToArray());
                    }
                    else
                    {
                        table = OpenTextBetween(file, settings.CodePage, settings.TextBegin, settings.TextEnd, settings.ContainsHeaders, settings.Headers.ToArray());
                    }

                }
                else
                {
                    TextFormat form = new TextFormat(file, multipleFiles, ctxRow);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        if (form.TakeOver && fileImportSettings != null)
                        {
                            fileImportSettings.Add(extension, form.ImportSettings);
                        }
                        table = form.DataTable;
                    }
                }
            }
            if(table != null)
            {
                CheckDataTableColumnHeader(table);
                table = table.RemoveEmptyRows();
                table.RemoveNull();
            }            
            return table;
        }

        internal static OpenFileDialog GetOpenFileDialog(bool status)
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
                Multiselect = status
            };
        }

        internal static DataTable OpenText(string path, string separator, int codePage, bool containsHeaders, object[] headers, bool isPreview = false)
        {
            DataTable dt = new DataTable();

            try
            {
                int skip = 0;
                if (containsHeaders)
                {
                    skip = 1;
                    List<string> list = File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(1)
                    .SelectMany(x => x.Split(new string[] { separator }, StringSplitOptions.None))
                    .Select(ln => ln.Trim()).ToList();

                    foreach (string column in list)
                    {
                        dt.TryAddColumn((Properties.Settings.Default.ImportHeaderUpperCase ? column.ToUpper() : column).Trim());
                    }
                }
                else
                {
                    foreach (string column in headers)
                    {
                        dt.TryAddColumn(column);
                    }
                }


                if (isPreview)
                {
                    InsertTextIntoDataTable(File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(4), dt, skip, separator);
                }
                else
                {
                    InsertTextIntoDataTable(File.ReadLines(path, Encoding.GetEncoding(codePage)), dt, skip, separator);
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

        private static void InsertTextIntoDataTable(IEnumerable<string> enumerable, DataTable dt, int skip, string separator)
        {
            enumerable.Skip(skip)
                    .Select(x => x.Split(new string[] { separator }, StringSplitOptions.None))
                    .ToList()
                    .ForEach(line =>
                    {
                        var temp = line.Select(ln => ln.ToString().Trim()).ToArray();
                        int count = temp.Count();
                        while (count > dt.Columns.Count)
                        {
                            dt.TryAddColumn("Spalte" + dt.Columns.Count);
                        }
                        dt.Rows.Add(temp);
                    });
        }

        internal static DataTable OpenTextBetween(string path, int codePage, string begin, string end, bool containsHeaders, object[] headers, bool isPreview = false)
        {
            DataTable dt = new DataTable();
            try
            {
                int skip = 0;
                if (containsHeaders)
                {
                    skip = 1;
                    string headerLine = File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(1).ToArray()[0].ToString();
                    string[] headerRow = createRow(headerLine, begin, end);

                    foreach (string field in headerRow)
                    {
                        dt.TryAddColumn((Properties.Settings.Default.ImportHeaderUpperCase ? field.ToUpper() : field).Trim());
                    }
                }
                else
                {
                    foreach (string column in headers)
                    {
                        dt.TryAddColumn(column);
                    }
                }

                string[] lines = new string[0];
                if (isPreview)
                {
                    lines = File.ReadLines(path, Encoding.GetEncoding(codePage)).Take(4).Skip(skip).ToArray();
                }
                else
                {
                    lines = File.ReadLines(path, Encoding.GetEncoding(codePage)).Skip(skip).ToArray();
                }

                foreach(string line in lines)
                {
                    string[] row = createRow(line, begin, end);
                    while(row.Length > dt.Columns.Count)
                    {
                        dt.TryAddColumn("Spalte" + dt.Columns.Count);
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

        private static int RenameColumn(DataTable dt, string column, int counter)
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

        internal static DataTable OpenDBF(string path)
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
            
            DataTable table = new DataTable();

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
                            temp.SetColumnsTypeString();
                            table.ConcatTable(temp, fileName, fileName);
                        }
                    }
                }
                Con.Close();
            }
            catch (Exception ex)
            {
                ErrorHelper.LogMessage(ex);
            }
            return table;
        }

        private static void CheckDataTableColumnHeader(DataTable table)
        {
            if (Properties.Settings.Default.ImportHeaderUpperCase)
            {
                foreach (DataColumn col in table.Columns)
                {
                    col.ColumnName = col.ColumnName.ToUpper();
                }
            }
        }

        internal static DataTable OpenExcel(string path, Form1 mainform)
        {
            DataTable data = new DataTable();
            
            Microsoft.Office.Interop.Excel.Application objXL = null;
            Microsoft.Office.Interop.Excel.Workbook objWB = null;
            string clipboardBefore = Clipboard.GetText();
            Clipboard.Clear();

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

                string[] selectedSheets = SelectExcelSheets(objWB.Worksheets.Cast<Microsoft.Office.Interop.Excel.Worksheet>().Select(x => x.Name).ToArray());
                
                bool fileNameColumn;
                if(fileNameColumn = (data.Columns.IndexOf(Extensions.DataTableExtensions.FileName) == -1 && selectedSheets.Length > 1))
                {
                    data.Columns.Add(Extensions.DataTableExtensions.FileName, typeof(string));
                }
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

                    Microsoft.Office.Interop.Excel.Range c1 = objSHT.Cells[1, 1];
                    Microsoft.Office.Interop.Excel.Range c2 = objSHT.Cells[rows, cols];
                    Microsoft.Office.Interop.Excel.Range range = objSHT.get_Range(c1, c2);
                    
                    RangeToDataTable(range, data, selectedSheets.Length > 1 ? Path.GetFileName(path) + "; " + sheetName : null);
                    Marshal.ReleaseComObject(objSHT);
                }
                if (fileNameColumn)
                {
                    data.Columns[Extensions.DataTableExtensions.FileName].SetOrdinal(data.Columns.Count - 1);
                }
                objXL.CutCopyMode = 0;
                objWB.Close();
                objXL.Quit();
                Marshal.ReleaseComObject(objWB);
                Marshal.ReleaseComObject(objXL);
                Marshal.FinalReleaseComObject(objXL);
            }
            catch (Exception ex)
            {
                objWB?.Close();
                objXL?.Quit();
                ErrorHelper.LogMessage(ex);
            }
            finally
            {
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

            return data;
        }

        private static void RangeToDataTable(Microsoft.Office.Interop.Excel.Range range, DataTable table, string fileName)
        {
            range.Copy();
            IDataObject data = Clipboard.GetDataObject();
            string content = (string)data.GetData(DataFormats.Text);
            string[] stringRows = content.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string[] headers = stringRows.First().Split('\t');
            int[] columns = new int[headers.Length];
            //header
            for(int i = 0; i < headers.Length; i++)
            {
                int index;
                if((index = table.Columns.IndexOf(headers[i])) == -1)
                {
                    index = table.Columns.Count;
                    table.Columns.Add(headers[i], typeof(string));
                }
                columns[i] = index;
            }

            //content
            foreach (string stringRow in stringRows.Skip(1))
            {
                var values = stringRow.Split('\t');
                DataRow dr = table.NewRow();
                for (int i = 0; i < values.Length; i++)
                {

                    string[] subValues = values[i].Split('\n') ?? new string[0];
                    if (subValues.Length < 2)
                    {
                        dr[columns[i]] = values[i];
                    }
                    else
                    {
                        #region split \n into several columns

                        int[] newColIndizes = new int[subValues.Length];
                        string colName = table.Columns[columns[i]].ColumnName;

                        newColIndizes[0] = columns[i];
                        for (int col = 1; col < subValues.Length; col++)
                        {
                            string newColName = colName + col;
                            int index = table.Columns.IndexOf(newColName);
                            if (index == -1)
                            {
                                newColIndizes[col] = table.Columns.Count;
                                table.Columns.Add(newColName, typeof(string));
                            }
                            else
                            {
                                newColIndizes[col] = index;
                            }
                        }

                        //format is "myText\nSecondText" with quotation marks
                        subValues[0] = subValues[0].TrimStart('"');
                        subValues[subValues.Length - 1] = subValues[subValues.Length - 1].TrimEnd('"');

                        for (int index = 0; index < subValues.Length; index++)
                        {
                            dr[newColIndizes[index]] = subValues[index];
                        }
                        #endregion
                    }
                }
                if (dr.ItemArray.Any(value => !string.IsNullOrWhiteSpace(value?.ToString())))
                {
                    if (fileName != null)
                    {
                        dr[Extensions.DataTableExtensions.FileName] = fileName;
                    }
                    table.Rows.Add(dr);
                }
            }

            Clipboard.Clear();
        }

        internal static DataTable OpenTextFixed(string path, List<int> config, List<string> header, int encoding, bool isPreview = false)
        {
            DataTable dt = new DataTable();
            if (header == null || config == null || header.Count == 0 || config.Count == 0)
            {
                return dt;
            }
            try
            {
                header.ForEach(x => dt.TryAddColumn(x.ToString()));

                StreamReader stream = new StreamReader(path, Encoding.GetEncoding(encoding));
                
                
                while (!stream.EndOfStream && (!isPreview || dt.Rows.Count < 3))
                {
                    string[] itemArray = new string[header.Count];

                    for (int i = 0; i < config.Count && !stream.EndOfStream; i++)
                    {
                        char[] body = new char[config[i]];
                        
                        for(int index = 0; index < config[i] && stream.Peek() != -1; index++)
                        {
                            body[index] = (char)stream.Read();
                        }

                        itemArray[i] = new string(body);
                    }
                    
                    dt.Rows.Add(itemArray);

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
                MessagesOK(MessageBoxIcon.Warning, "Die Datei wird zurzeit von einem anderen Programm verwendet und kann nicht geöffnet werden.");
            }
            return dt;
        }

        internal static List<Proc> LoadProcedures()
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

        internal static List<Work> LoadWorkflows()
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

        internal static List<Tolerance> LoadTolerances()
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

        internal static List<Case> LoadCases()
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

        private static string[] SelectExcelSheets(string[] sheets)
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
