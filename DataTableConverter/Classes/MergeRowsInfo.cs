using System.Collections.Generic;
using System.Data;

namespace DataTableConverter.Classes
{
    class MergeRowsInfo
    {
        internal Dictionary<string, decimal> CountDict;
        internal int RowsMerged = 0;
        internal DataRow Row;

        public MergeRowsInfo(DataRow row)
        {
            CountDict = new Dictionary<string, decimal>();
            Row = row;
        }
    }
}
