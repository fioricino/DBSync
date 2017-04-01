using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Database.Interfaces;

namespace DBSyncNew.Database.MsSql
{
    public class MsSqlTables : Dictionary<string, List<IDBColumn>>, IDBTables
    {
        public MsSqlTables(DataTable dataTable) : base(dataTable.Rows.Cast<DataRow>()
            .GroupBy(row => row.Field<string>("TABLE_NAME"))
            .ToDictionary(t => t.Key, t => t.Select( tt => (IDBColumn)new MsSqlColumn(tt)).ToList()))
        {
            
        }
    }
}
