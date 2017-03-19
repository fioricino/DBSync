using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBSync.MsSql.Objects
{
    public class MsSqlTable
    {
        public string Name { get; set; }

        public List<MsSqlColumn> Columns { get; set; } = new List<MsSqlColumn>();
    }
}
