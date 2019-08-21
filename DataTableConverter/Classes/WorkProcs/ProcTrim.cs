using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcTrim : WorkProc
    {
        internal static readonly string ClassName = "Trim";
        public string Characters;
        public bool DeleteDouble;
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

        public ProcTrim(string characters, TrimType type, bool deleteDouoble)
        {
            Characters = characters;
            Type = type;
            DeleteDouble = deleteDouoble;
        }

        public override string[] GetHeaders()
        {
            return new string[0];
        }

        public override void renameHeaders(string oldName, string newName)
        {
            return;
        }

        public override void removeHeader(string colName)
        {
            return;
        }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form invokeForm)
        {
            char[] charArray = Characters.ToCharArray();

            if (DeleteDouble)
            {
                foreach (DataRow row in table.Rows)
                {
                    foreach (char c in charArray)
                    {
                        Regex regex = new Regex("[" + c + "]{2,}", RegexOptions.None);
                        for (int i = 0; i < row.ItemArray.Length; i++)
                        {
                            row[i] = regex.Replace(GetTrimmed(row[i].ToString(), charArray), c.ToString());
                        }
                    }
                }
            }
            else
            {
                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < row.ItemArray.Length; i++)
                    {
                        row[i] = GetTrimmed(row[i].ToString(), charArray);
                    }
                }
            }

            foreach (DataColumn col in table.Columns)
            {
                col.ColumnName = GetTrimmed(col.ColumnName, charArray);
            }
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
