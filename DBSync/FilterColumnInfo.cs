using System;
using System.Xml.Serialization;

namespace DBSyncOld
{
    [Serializable]
    public class FilterColumnInfo
    {
        [XmlAttribute]
        public string ColumnName { get; set; }

        [XmlAttribute]
        public string FilterClause { get; set; }

        [XmlAttribute]
        public bool IsReferenced { get; set; }

        [XmlAttribute]
        public bool IsSkippedOnDelete { get; set; }

        [XmlIgnore]
        public ITableOrAlias Table { get; set; }
    }
}
