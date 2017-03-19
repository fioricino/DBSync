using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DBSync.ScopeConf.Objects
{
    [Serializable]
    public class AllScopesConfig
    {
        public List<ScopeConfig> Scopes { get; set; } = new List<ScopeConfig>();

        [XmlAttribute]
        SyncConflictResolutionPolicy ConflictResolutionPolicy { get; set; }
    }
}
