using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DBSync.ScopeConf.Objects
{
    [Serializable]
    public class AliasConfig
    {
        [XmlAttribute]
        public string Name { get; set; }

        public List<FilterColumnConfig> FilterColumns { get; set; } = new List<FilterColumnConfig>();

        [XmlAttribute]
        public bool IsRoot { get; set; }
    }
}
