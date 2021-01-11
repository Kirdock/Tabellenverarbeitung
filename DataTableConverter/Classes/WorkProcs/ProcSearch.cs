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
    class ProcSearch : WorkProc
    {
        internal static readonly string ClassName = "Suchen";
        public string SearchText;
        public string Header;
        public int From, To;
        public bool TotalSearch;
        public string Shortcut;

        public ProcSearch(int ordinal, int id, string name) : base(ordinal, id, name){}

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

        public override void DoWork(ref string sortingOrder, Case duplicateCase, List<Tolerance> tolerances, Proc procedure, string filename, ContextMenuStrip ctxRow, OrderType orderType, Form1 invokeForm, string tableName = "main")
        {
            string column = invokeForm.DatabaseHelper.GetColumnName(Header, tableName);
            if (column != null && From <= To )
            {
                if (!string.IsNullOrWhiteSpace(NewColumn))
                {
                    string col = invokeForm.DatabaseHelper.GetColumnName(NewColumn, tableName) ?? invokeForm.DatabaseHelper.AddColumnWithAdditionalIfExists(NewColumn);
                    invokeForm.DatabaseHelper.SearchAndShortcut(Header, col, TotalSearch, SearchText, Shortcut, From, To, sortingOrder, orderType, tableName);
                }
            }
        }

        public override string[] GetHeaders()
        {
            return new string[] { Header };
        }

        public override void RemoveHeader(string colName)
        {
            if(colName == Header)
            {
                Header = string.Empty;
            }
        }

        public override void RenameHeaders(string oldName, string newName)
        {
            if(oldName == Header)
            {
                Header = newName;
            }
        }
    }
}
