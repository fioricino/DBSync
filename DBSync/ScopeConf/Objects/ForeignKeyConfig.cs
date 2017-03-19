using System;
using System.Xml.Serialization;

namespace DBSync.ScopeConf.Objects
{
    [Serializable]
    public class ForeignKeyConfig
    {
        [XmlAttribute]
        public string Table { get; set; }

        [XmlAttribute]
        public string Column { get; set; }

        [XmlAttribute]
        public string ReferencedTable { get; set; }

        [XmlAttribute]
        public string ReferencedColumn { get; set; }
    }
}
