using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Configuration;

namespace DBSyncNew.SchemaObjects
{
    public class FilterColumnObj
    {
        private FilterColumnObj(Builder builder)
        {
            ColumnName = builder.FilterColumnConfig.ColumnName;
            FilterClause = builder.FilterColumnConfig.FilterClause;
            IsReferenced = builder.FilterColumnConfig.IsReferenced;
            IsSkippedOnDelete = builder.FilterColumnConfig.IsSkippedOnDelete;
            Table = builder.TableOrAlias;
        }

        public string ColumnName { get; }

        public string FilterClause { get; }

        public bool IsReferenced { get; }

        public bool IsSkippedOnDelete { get; }

        //TODO don't use if belongs to scope
        public ITableOrAlias Table { get; }

        public class Builder
        {
            //TODO dont't pass non-constructed objects
            public Builder(FilterColumnConfig filterColumnConfig, ITableOrAlias tableOrAlias)
            {
                FilterColumnConfig = filterColumnConfig;
                TableOrAlias = tableOrAlias;
            }

            public FilterColumnConfig FilterColumnConfig { get; }

            public ITableOrAlias TableOrAlias { get; }

            public FilterColumnObj Build()
            {
                return new FilterColumnObj(this);
            }
        }
    }
}
