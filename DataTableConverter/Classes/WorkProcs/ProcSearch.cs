using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DataTableConverter.Classes.WorkProcs
{
    [Serializable()]
    class ProcSearch : WorkProc
    {
        internal static readonly string ClassName = "Suchen";
        public string SearchText;
        public string Header;
        public int From, To;
        public bool TotalSearch;
        public string Shortcut;

        public ProcSearch(int ordinal, int id, string name) : base(ordinal, id, name) { }

        public ProcSearch(string searchText, string header, int from, int to, string newColumn, bool totalSearch)
        {
            SearchText = searchText;
            Header = header;
            From = from;
            To = to;
            NewColumn = newColumn;
            TotalSearch = totalSearch;
        }

        public ProcSearch(string searchText, string header, string newColumn, bool totalSearch, string shortcut)
        {
            SearchText = searchText;
            Header = header;
            TotalSearch = totalSearch;
            Shortcut = shortcut;
            NewColumn = newColumn;
        }

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName)
        {
            string alias = Header;
            if (PrepareSingle(ref alias, invokeForm, tableName, out string destination) && alias != null && From <= To)
            {
                invokeForm.DatabaseHelper.SearchAndShortcut(Header, destination, TotalSearch, SearchText, Shortcut, From, To, sortingOrder, orderType, tableName);
            }
        }

        public override string[] GetHeaders()
        {
            return new string[] { Header };
        }

        public override void RemoveHeader(string colName)
        {
            if (colName == Header)
            {
                Header = string.Empty;
            }
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            if (oldName == Header)
            {
                Header = newName;
            }
        }
    }
}
