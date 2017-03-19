using System;
using System.Xml.Serialization;

namespace DBSync.DomainObjects
{
    public class FilterColumnDO
    {
        public ColumnDO Column { get; set; }

        public string FilterClause { get; set; }

        public bool IsReferenced { get; set; }
    }
}
