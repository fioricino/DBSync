using System;
using System.Xml.Serialization;

namespace DBSyncNew
{
    public class FilterColumnInfo
    {
        public string ColumnName { get; set; }

        public string FilterClause { get; set; }

        public bool IsReferenced { get; set; }

        public bool IsSkippedOnDelete { get; set; }

        public ITableOrAlias Table { get; set; }
    }
}
