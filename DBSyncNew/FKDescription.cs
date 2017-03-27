using System;
using System.Xml.Serialization;

namespace DBSyncNew
{
    public class FKDescription
    {
        public string Table { get; set; }

        public string Column { get; set; }

        public string ReferencedTable { get; set; }

        public string ReferencedColumn { get; set; }
    }
}
