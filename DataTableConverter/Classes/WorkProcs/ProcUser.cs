using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcUser : WorkProc
    {
        internal static readonly string ClassName = "Suchen & Ersetzen";
        internal static readonly string ContainsDataFormat = "[befüllt]";
        public bool IsSystem;
        public Proc Procedure;
        public override string[] GetHeaders()
        {
            return RemoveEmptyHeaders(Columns.AsEnumerable().Select(dr => dr.ItemArray.Length > 0 ? dr.ItemArray[0].ToString() : null));
        }

        public ProcUser(int ordinal, int id, string name) : base(ordinal, id, name) { }

        public ProcUser(string[] columns, string header, bool copyOldColumn)
        {
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            foreach (string col in columns)
            {
                Columns.Rows.Add(col);
            }
            NewColumn = header;
            CopyOldColumn = copyOldColumn;
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            foreach (DataRow row in Columns.Rows)
            {
                if (row.ItemArray[0].ToString() == oldName)
                {
                    row.SetField(0, newName);
                }
            }
        }

        public override void RemoveHeader(string colName)
        {
            Columns = Columns.AsEnumerable().Where(row => row[0].ToString() != colName).ToTable(Columns);
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName)
        {
            procedure = IsSystem ? Procedure : procedure;
            if (PrepareMultiple(GetHeaders(), invokeForm, tableName, out string[] sourceColumns, out string[] destinationColumns))
            {
                IEnumerable<DataRow> replaces = procedure.Replace.AsEnumerable().Where(row => !string.IsNullOrEmpty(row[0]?.ToString()) || !string.IsNullOrEmpty(row[1]?.ToString()));
                IEnumerable<DataRow> replaceWithoutEmpty = replaces.Where(replace => replace[0].ToString() != string.Empty && replace[0].ToString() != ContainsDataFormat);
                IEnumerable<DataRow> replaceWithEmpty = replaces.Where(replace => replace[0].ToString() == string.Empty && replace[1].ToString().Length > 0);
                DataRow replaceWhole = replaces.FirstOrDefault(replace => replace[0].ToString() == ContainsDataFormat);
                string replaceWholeText = replaceWhole?[1].ToString() ?? string.Empty;
                bool containsReplaceWhole = replaceWhole != null;
                bool containsEmpty = replaceWithEmpty.Count() > 0;
                string replaceEmptyString = containsEmpty ? replaceWithEmpty.First()[1].ToString() : string.Empty;

                invokeForm.DatabaseHelper.SearchAndReplace(sourceColumns, destinationColumns, procedure.CheckTotal, procedure.CheckWord, procedure.LeaveEmpty, replaceEmptyString, replaceWholeText, containsEmpty, containsReplaceWhole, replaceWithoutEmpty, tableName);
            }
        }
    }
}
