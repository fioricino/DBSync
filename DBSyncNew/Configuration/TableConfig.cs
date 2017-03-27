using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DBSyncNew.Configuration
{
    [Serializable]
    public class TableConfig
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public bool IsSkippedOnDelete { get; set; }

        [XmlAttribute]
        public SyncConflictResolutionPolicy ConflictResolutionPolicy { get; set; }

        public List<FilterColumnConfig> FilterColumns { get; } = new List<FilterColumnConfig>();

        public List<AliasConfig> Aliases { get; } = new List<AliasConfig>();
    }
}
