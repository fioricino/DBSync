using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBSync.MsSql.Objects
{
    public class MsSqlColumn
    {
        public string Name { get; set; }
        public bool IsPk { get; set; }
        public bool IsNullable { get; set; }
        public string DataType { get; set; }
        public bool IsReadOnly { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int? MaxLength { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }
        public int OrdinalNumber { get; set; }
    }
}
