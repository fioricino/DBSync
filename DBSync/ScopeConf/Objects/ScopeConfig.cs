using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DBSync.ScopeConf.Objects
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

        public List<FilterColumnConfig> FilterColumns { get; set; } = new List<FilterColumnConfig>();

        public List<ForeignKeyConfig> ReversedForeignKeys { get; set; } = new List<ForeignKeyConfig>();

        public List<ForeignKeyConfig> ArtificialForeignKeys { get; set; } = new List<ForeignKeyConfig>();

        public List<ForeignKeyConfig> IgnoredForeignKeys { get; set; }
            
        public List<TableConfig> Tables { get; set; } = new List<TableConfig>();

    }
}
