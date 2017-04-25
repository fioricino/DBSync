using System.Collections.Generic;

namespace DBSyncNew
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
