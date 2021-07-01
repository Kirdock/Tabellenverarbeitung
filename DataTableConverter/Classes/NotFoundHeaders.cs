using System.Collections.Generic;

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
