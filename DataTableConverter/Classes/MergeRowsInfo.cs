using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    class MergeRowsInfo
    {
        internal Dictionary<string,float> CountDict;
        internal int RowsMerged = 0;
        internal DataRow Row;

        public MergeRowsInfo(DataRow row)
        {
            CountDict = new Dictionary<string, float>();
            Row = row;
        }
    }
}
