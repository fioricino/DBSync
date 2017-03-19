using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSync.DomainObjects;
using DBSync.MsSql.Objects;
using DBSync.ScopeConf.Objects;

namespace DBSync.DOBuilder
{
    public class ScopeDOBuilder
    {
        public AllScopesDO BuildAllScopesDO(AllScopesConfig allScopesConfig, List<MsSqlTable> sqlTables)
        {
            AllScopesDO result = new AllScopesDO();
            result.scopes.Add(new ScopeDO() {ScopeType = ScopeType.None});
            
        } 
    }
}
