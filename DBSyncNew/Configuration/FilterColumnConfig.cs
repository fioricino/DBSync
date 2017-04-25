using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DBSyncNew.Configuration
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
