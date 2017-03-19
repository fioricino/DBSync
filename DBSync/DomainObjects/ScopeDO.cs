using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSync.ScopeConf.Objects;

namespace DBSync.DomainObjects
{
    public class ScopeDO
    {
        public ScopeType ScopeType { get; set; }

        public List<TableDO> Tables { get; } = new List<TableDO>();

    }
}
