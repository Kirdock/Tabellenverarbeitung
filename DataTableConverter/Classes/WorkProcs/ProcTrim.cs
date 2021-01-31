using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcTrim : WorkProc
    {
        internal static readonly string ClassName = "Trim";
        public string Characters;
        public bool DeleteDouble;
        public bool AllColumns = true;

        public enum TrimType { Start, End, Both };
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
            invokeForm.DatabaseHelper.Trim(Characters, AllColumns ? null : GetHeaders(), DeleteDouble, Type, tableName);
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


    }
}
