using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableConverter.Classes
{
    class NotFoundHeaders
    {
        public List<string> Headers { get; set; }
        public WorkProc Wp { get; set; }
        

        public NotFoundHeaders(List<string> headers, WorkProc wp)
        {
            Headers = headers;
            Wp = wp;
            
        }
    }
}
