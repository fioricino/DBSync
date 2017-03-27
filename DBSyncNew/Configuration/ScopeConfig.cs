using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DBSyncNew.Configuration
{
    [Serializable]
    public class ScopeConfig
    {
        [XmlAttribute]
        public ScopeType ScopeType { get; set; }

        [XmlAttribute]
        public string FilterColumnName { get; set; }

        [XmlAttribute]
        public string FilterClause { get; set; }

        [XmlAttribute]
        public SelectMetaDataGenerationPattern MetaDataGenerationPattern { get; set; }

        public List<FilterColumnConfig> FilterColumns { get; set; }

        public List<ForeignKeyConfig> ReversedForeignKeys { get; set; }

        public List<ForeignKeyConfig> ArtificialForeignKeys { get; set; }

        public List<ForeignKeyConfig> IgnoredForeignKeys { get; set; }

        public List<TableConfig> Tables { get; set; }
    }
}
