using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace DBSyncNew
{
    [Serializable]
    public class ScopeConfiguration
    {
        public ScopeConfiguration()
        {
            Scopes = new List<ScopeInfo>();
        }

        public List<ScopeInfo> Scopes { get; set; }

        [XmlAttribute]
        SyncConflictResolutionPolicy ConflictResolutionPolicy { get; set; }

        public void SetRelations()
        {
            foreach (var scope in Scopes)
            {
                scope.SetRelations();
            }
        }

        public IEnumerable<TableInfo> Findtable(string name)
        {
            return Scopes.SelectMany(s => s.Tables).Where(t => t.Name == name || t.Aliases.Any(a => a.Name == name));
        }
    }
}
