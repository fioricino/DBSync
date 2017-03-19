using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBSync.DomainObjects
{
    public class AliasDO
    {
        public AliasDO(string name, TableDO table)
        {
            Name = name;
            Table = table;
        }

        public string Name { get; private set; }

        public TableDO Table { get; set; }

        public List<ColumnDO> FilterColumns { get; } = new List<ColumnDO>(); 
    }
}
