using System;
using System.Xml.Serialization;

namespace DBSync.ScopeConf.Objects
{
    [Serializable]
    public class FilterColumnConfig
    {
        [XmlAttribute]
        public string ColumnName { get; set; }

        [XmlAttribute]
        public string FilterClause { get; set; }

        [XmlAttribute]
        public bool IsReferenced { get; set; }

        [XmlAttribute]
        public bool IsSkippedOnDelete { get; set; }
    }
}
