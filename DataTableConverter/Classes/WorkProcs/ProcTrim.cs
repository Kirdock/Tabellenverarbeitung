using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataTableConverter.Extensions;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcTrim : WorkProc
    {
        internal static readonly string ClassName = "Trim";
        public string Characters;
        public bool DeleteDouble;
        public bool AllColumns = true;

        public enum TrimType { Start, End, Both};
        public TrimType Type;

        public ProcTrim()
        {
            SetDefault();
        }
        public ProcTrim(int ordinal, int id, string name) : base(ordinal, id, name)
        {
            SetDefault();
        }

        private void SetDefault()
        {
            Type = TrimType.Both;
            Characters = " ";
            DeleteDouble = true;
        }

        public ProcTrim(string characters, TrimType type, bool deleteDouble, bool allColumns, string[] columns)
        {
            Characters = characters;
            Type = type;
            DeleteDouble = deleteDouble;
            AllColumns = allColumns;
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            foreach (string col in columns)
            {
                Columns.Rows.Add(col);
            }
        }

        public override string[] GetHeaders()
        {
            return AllColumns ? new string[0] : RemoveEmptyHeaders(Columns.ColumnValuesAsString(0));
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            if (!AllColumns)
            {
                foreach (DataRow row in Columns.Rows)
                {
                    if (row.ItemArray[0].ToString() == oldName)
                    {
                        row.SetField(0, newName);
                    }
                }
            }
        }

        public override void RemoveHeader(string colName)
        {
            if (!AllColumns)
            {
                Columns = Columns.AsEnumerable().Where(row => row[0].ToString() != colName).ToTable(Columns);
            }
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName = "main")
        {
            //could use own SQL function here because of regex
            if (Characters.Contains(" "))
            {
                Characters += (char)160;
            }
            char[] charArray = Characters.ToCharArray();
            string[] columns = AllColumns ? table.HeadersOfDataTableAsString() : GetHeaders();

            if (DeleteDouble)
            {
                foreach (DataRow row in table.Rows)
                {
                    foreach (char c in charArray)
                    {
                        Regex regex = new Regex("[" + c + "]{2,}", RegexOptions.None);
                        foreach (string col in columns)
                        {
                            row[col] = regex.Replace(GetTrimmed(row[col].ToString(), charArray), c.ToString());
                        }
                    }
                }
            }
            else
            {
                foreach (DataRow row in table.Rows)
                {
                    foreach (string col in columns)
                    {
                        row[col] = GetTrimmed(row[col].ToString(), charArray);
                    }
                }
            }

            foreach (DataColumn col in table.Columns)
            {
                col.ColumnName = GetTrimmed(col.ColumnName, charArray);
            }
        }

        /// <summary>
        /// trims leading and ending spaces including double appearences
        /// </summary>
        /// <param name="value"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        internal static string Trim(string value)
        {
            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
            return regex.Replace(value.Trim(), " ");
        }

        private string GetTrimmed(string text, char[] charArray)
        {
            string result;
            switch (Type)
            {
                case TrimType.Start:
                    result = text.TrimStart(charArray);
                    break;

                case TrimType.End:
                    result = text.TrimEnd(charArray);
                    break;

                case TrimType.Both:
                default:
                    result = text.Trim(charArray);
                    break;
            }
            return result;
        }
    }
}
