﻿using DataTableConverter.Assisstant;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    internal class ProcTrim : WorkProc
    {
        internal static readonly string ClassName = "Trim";

        public ProcTrim() { }
        public ProcTrim(int ordinal, int id, string name) : base(ordinal, id, name) { }

        public override string[] getHeaders()
        {
            return new string[0];
        }

        public override void renameHeaders(string oldName, string newName)
        {
            return;
        }

        public override void doWork(DataTable table, out string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure)
        {
            sortingOrder = string.Empty;
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    row.SetField(i, row.ItemArray[i].ToString().Trim());
                }
            }
            foreach (DataColumn col in table.Columns)
            {
                col.ColumnName = col.ColumnName.Trim();
            }
        }
    }
}