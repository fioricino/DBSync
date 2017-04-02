using System.Collections.Generic;

namespace DBSyncNew.Database.Read.Interfaces
{
    public interface IDatabase
    {
        IDBTables Tables { get; }

        List<IDBForeignKey> ForeignKeys { get; }
    }
}
