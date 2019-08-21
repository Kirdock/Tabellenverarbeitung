﻿using DataTableConverter.Assisstant;
using DataTableConverter.Extensions;
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
    class ProcCount : WorkProc
    {
        internal static readonly string ClassName = "Zählen";
        public string Column;
        public int Count;
        public bool ShowFromTo;
        public bool CountChecked;

        public ProcCount(int ordinal, int id, string name) : base(ordinal, id, name) { }

        public override void doWork(DataTable table, ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filePath, ContextMenuStrip ctxRow, OrderType orderType, Form invokeForm, out int[] newOrderIndices)
        {
            newOrderIndices = new int[0];
            DataTable newTable = ExportHelper.ExportCount(Column, CountChecked ? Count : 0, ShowFromTo, table, orderType);
            invokeForm.BeginInvoke(new MethodInvoker(() =>
            {
                new Form1(newTable).Show();
            }));
        }

        public override string[] GetHeaders()
        {
            return new string[] { Column };
        }

        public override void removeHeader(string colName)
        {
            if(Column == colName)
            {
                Column = string.Empty;
            }
        }

        public override void renameHeaders(string oldName, string newName)
        {
            if(Column == oldName)
            {
                Column = newName;
            }
        }
    }
}
