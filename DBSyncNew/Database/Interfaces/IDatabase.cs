using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBSyncNew.Database.Interfaces
{
    public interface IDatabase
    {
        IDBTables Tables { get; }

        List<IDBForeignKey> ForeignKeys { get; }
    }
}
