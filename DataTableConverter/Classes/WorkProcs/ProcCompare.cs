﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    class ProcCompare : WorkProc
    {
        internal static readonly string ClassName = "Spalten vergleichen";
        public string SourceColumn;
        public string CompareColumn;

        public ProcCompare(int ordinal, int id, string name) : base(ordinal, id, name) { }

        internal ProcCompare(string newColumn, bool oldColumn, string sourceColumn, string compareColumn)
        {
            NewColumn = newColumn;
            CopyOldColumn = oldColumn;
            SourceColumn = sourceColumn;
            CompareColumn = compareColumn;
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName)
        {
            if (string.IsNullOrWhiteSpace(SourceColumn) || string.IsNullOrWhiteSpace(CompareColumn))
            {
                return;
            }

            if (PrepareSingle(ref SourceColumn, invokeForm, tableName, out string destinationColumn))
            {
                invokeForm.DatabaseHelper.EmptyColumnByCondition(SourceColumn, destinationColumn, CompareColumn, tableName);
            }
        }

        public override string[] GetHeaders()
        {
            return new string[] { SourceColumn, CompareColumn };
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            if (SourceColumn == oldName)
            {
                SourceColumn = newName;
            }
            if (CompareColumn == oldName)
            {
                CompareColumn = newName;
            }
        }

        public override void RemoveHeader(string colName)
        {
            if (CompareColumn == colName)
            {
                CompareColumn = null;
            }
            if (SourceColumn == colName)
            {
                SourceColumn = null;
            }
        }

    }
}
