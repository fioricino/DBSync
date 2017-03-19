using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSync.Interfaces;

namespace DBSync.DomainObjects
{
    public class ColumnDO
    {
        public string Name { get; set; }

        public ISqlColumn Column { get; set; }

        public TableDO Table { get; set; }
    }
}
