using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DBSyncNew.Configuration
{
    [Serializable]
    public class ScopesConfig
    {
        public List<ScopeConfig> Scopes { get; set; }
    }
}
