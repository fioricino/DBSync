using System.Collections.Generic;
using DBSync.SqlInfo;

namespace DBSync
{
    public sealed class SyncScript
    {
      
        public SyncScript(string fromCause, List<WhereClause> whereCause)
        {
            FromCause = fromCause;
            WhereCause = whereCause;
        }

        public string FromCause { get; private set; }
        public List<WhereClause> WhereCause { get; private set; }


    }
}
