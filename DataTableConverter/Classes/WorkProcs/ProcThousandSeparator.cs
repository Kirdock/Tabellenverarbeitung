﻿using DataTableConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    class ProcThousandSeparator : WorkProc
    {
        internal static readonly string ClassName = "Tausender Trennzeichen";

        public override string[] GetHeaders()
        {
            return RemoveEmptyHeaders(Columns.AsEnumerable().Select(dr => dr.ItemArray.FirstOrDefault()?.ToString()));
        }

        public ProcThousandSeparator(int ordinal, int id, string name) : base(ordinal, id, name) { }

        public ProcThousandSeparator(string[] columns, string newColumn, bool copyOldColumn)
        {
            NewColumn = newColumn;
            Columns = new DataTable { TableName = "Columnnames" };
            Columns.Columns.Add("Spalten", typeof(string));
            foreach (string col in columns)
            {
                Columns.Rows.Add(col);
            }
            CopyOldColumn = copyOldColumn;
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            foreach (DataRow row in Columns.Rows)
            {
                if (row[0].ToString() == oldName)
                {
                    row[0] = newName;
                }
            }
        }

        public override void RemoveHeader(string colName)
        {
            Columns = Columns.AsEnumerable().Where(row => row[0].ToString() != colName).ToTable(Columns);
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName)
        {
            string[] columns = GetHeaders();
            if (PrepareMultiple(columns, invokeForm, tableName, out string[] sourceColumns, out string[] destinationColumns))
            {
                invokeForm.DatabaseHelper.ThousandSeparator(sourceColumns, destinationColumns, tableName);
            }
        }
    }
}
