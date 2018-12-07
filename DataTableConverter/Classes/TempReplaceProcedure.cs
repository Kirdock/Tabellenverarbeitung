using System.Collections.Generic;

namespace DataTableConverter.Classes
{
    class TempReplaceProcedure
    {
        public string[] Columns { get; set; }
        public WorkProc workProc { get; set; }

        public TempReplaceProcedure(string[] columns, WorkProc wp)
        {
            Columns = columns;
            workProc = wp;
        }
    }
}
