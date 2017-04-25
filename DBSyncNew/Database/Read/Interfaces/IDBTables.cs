using System.Collections.Generic;

namespace DBSyncNew.Database.Read.Interfaces
{
    public interface IDBTables : IDictionary<string, List<IDBColumn>>
    {
         
    }
}