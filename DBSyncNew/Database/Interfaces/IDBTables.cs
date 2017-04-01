using System;
using System.Collections;
using System.Collections.Generic;

namespace DBSyncNew.Database.Interfaces
{
    public interface IDBTables : IDictionary<string, List<IDBColumn>>
    {
         
    }
}