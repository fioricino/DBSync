using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBSync.DomainObjects
{
    public class AllScopesDO
    {
        public List<ScopeDO> scopes { get; } = new List<ScopeDO>();

        public Dictionary<string, TableDO> AllTables { get;  }= new Dictionary<string, TableDO>(); 
    }
}
