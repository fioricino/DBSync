using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBSyncNew.Database.Interfaces
{
    public interface IDBAdapter
    {
        IDBMetadata GetDbMetadata();
    }
}
