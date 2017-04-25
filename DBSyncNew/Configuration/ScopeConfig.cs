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

        //TODO use this
        [XmlAttribute]
        public string FilterColumnName { get; set; }

        [XmlAttribute]
        public string FilterClause { get; set; }

        [XmlAttribute]
        public MetaDataGenerationPattern MetaDataGenerationPattern { get; set; }

        public List<FilterColumnConfig> FilterColumns { get; set; } = new List<FilterColumnConfig>();

        public List<ForeignKeyConfig> ReversedForeignKeys { get; set; } = new List<ForeignKeyConfig>();

        //TODO skip if duplicated
        public List<ForeignKeyConfig> ArtificialForeignKeys { get; set; } = new List<ForeignKeyConfig>();

        public List<ForeignKeyConfig> IgnoredForeignKeys { get; set; } = new List<ForeignKeyConfig>();

        public List<TableConfig> Tables { get; set; } = new List<TableConfig>();
    }
}
