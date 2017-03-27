using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DBSyncNew.Configuration
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
