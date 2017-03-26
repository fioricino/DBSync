using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Database.Interfaces;

namespace DBSyncNew.Database.MsSql
{
    public class MsSqlTableAdapter : IDBTable
    {
        public MsSqlTableAdapter(IGrouping<string, DataRow> grouping)
        {
            Name = grouping.Key;
            foreach (var row in grouping)
            {
                Columns.Add(new MsSqlColumnAdapter(row));
            }
        }

        public string Name { get; }
        public List<IDBColumn> Columns { get; } = new List<IDBColumn>();
    }
}
