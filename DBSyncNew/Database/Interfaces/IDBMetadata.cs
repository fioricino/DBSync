using System;
using System.Collections;
using System.Collections.Generic;

namespace DBSyncNew.Database.Interfaces
{
    public interface IDBMetadata : IDictionary<string, List<IDBColumn>>
    {
         
    }
}