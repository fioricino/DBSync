using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DBSync.ScopeConf.Objects
{
    [Serializable]
    public class TableConfig
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public bool IsRoot { get; set; }
        
        [XmlAttribute]
        public bool IgnoreKey { get; set; }

        [XmlAttribute]
        public bool KeepRowVersion { get; set; }

        [XmlAttribute]
        public bool IsSkippedOnDelete { get; set; }
        
        [XmlAttribute]
        public SyncConflictResolutionPolicy ConflictResolutionPolicy { get; set; }

        public List<FilterColumnConfig> FilterColumns { get; set; } = new List<FilterColumnConfig>();

        public List<AliasConfig> Aliases { get; set; } = new List<AliasConfig>();
        
    }
}
