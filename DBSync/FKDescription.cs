using System;
using System.Xml.Serialization;

namespace DBSyncOld
{
    [Serializable]
    public class FKDescription
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
