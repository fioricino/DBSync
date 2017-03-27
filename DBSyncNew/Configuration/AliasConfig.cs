using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DBSyncNew.Configuration
{
    [Serializable]
    public class AliasConfig
    {
        [XmlAttribute]
        public string Name { get; set; }

        public List<FilterColumnConfig> FilterColumns { get; } = new List<FilterColumnConfig>();

        [XmlAttribute]
        public bool IsRoot { get; set; }
    }
}
