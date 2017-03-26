using System.Collections.Generic;

namespace DBSyncNew.Database.Interfaces
{
    public interface IDBTable
    {
         string Name { get; }

        List<IDBColumn> Columns { get; } 
    }
}