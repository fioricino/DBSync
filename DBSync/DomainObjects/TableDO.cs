using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSync.Interfaces;

namespace DBSync.DomainObjects
{
    public class TableDO
    {
        public TableDO(string name, IEnumerable<ColumnDO> columns, ScopeDO scope, List<AliasDO> aliases, bool isRoot, ISqlTable sqlTable)
        {
            Name = name;
            Columns = columns;
            Scope = scope;
            Aliases = aliases;
            IsRoot = isRoot;
            SqlTable = sqlTable;

            foreach (var alias in Aliases)
            {
                alias.Table = this;
            }

            foreach (var column in columns)
            {
                column.Table = this;
            }
        }

        public string Name { get; private set; }

        public IEnumerable<ColumnDO> Columns { get; private set; }

        public ScopeDO Scope { get; private set; }

        public List<AliasDO> Aliases { get; private set; }

        public bool IsRoot { get; private set; }

        public ISqlTable SqlTable { get; private set; }

    }
}
