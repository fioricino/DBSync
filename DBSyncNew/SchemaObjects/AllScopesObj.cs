using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.Configuration;
using DBSyncNew.Database.Read.Interfaces;

namespace DBSyncNew.SchemaObjects
{
    public class AllScopesObj
    {
        private AllScopesObj(Builder builder)
        {
            Scopes = builder.ScopesConfig.Scopes.Select(s => new ScopeObj.Builder(s, builder.Database.Tables).Build()).ToList();
            foreach (var dbForeignKey in builder.Database.ForeignKeys)
            {
                var tables = findTables(dbForeignKey.Table);
                var refTables = findTables(dbForeignKey.ReferencedTable);
                foreach (var table in tables)
                {
                    var column = table.Columns.Single(c => c.Name == dbForeignKey.Column);
                    foreach (var refTable in refTables.Where(t => t.Scope.ScopeType == ScopeType.Core || t.Scope.ScopeType == table.Scope.ScopeType))
                    {
                        //TODO move
                        var refColumn = refTable.Columns.Single(c => c.Name == dbForeignKey.ReferencedColumn);
                        column.ReferencedColumnObj = refColumn;
                        foreach (var alias in table.Aliases)
                        {
                            foreach (var refAlias in refTable.Aliases)
                            {
                                var fk = new ForeignKeyObj.DBBuilder(dbForeignKey, table.Scope.ReversedForeignKeys.Any(rfk => 
                                rfk.Table == alias.NameOrAlias && rfk.ReferencedTable == refAlias.NameOrAlias && rfk.Column == dbForeignKey.Column
                                && rfk.ReferencedColumn == dbForeignKey.ReferencedColumn) ? ForeignKeyDirection.Reversed :
                                table.Scope.IgnoredForeignKeys.Any(rfk =>
                                rfk.Table == alias.NameOrAlias && rfk.ReferencedTable == refAlias.NameOrAlias && rfk.Column == dbForeignKey.Column
                                && rfk.ReferencedColumn == dbForeignKey.ReferencedColumn) ? ForeignKeyDirection.Ignored
                                : ForeignKeyDirection.Direct, alias, refAlias
                                ).Build();
                                alias.ForeignKeys.Add(fk);
                                refAlias.ForeignKeys.Add(fk);
                            }
                        }
                    }
                }
            }
            foreach (var table in Scopes.SelectMany(s => s.Tables))
            {
                table.Level = CalculateLevel(table, 0);
            }
        }

        private int CalculateLevel(TableObj tableInfo, int stackLevel)
        {
            var maxLevel = stackLevel;
            foreach (var parentInfo in tableInfo.ParentInfos)
            {
                var level = CalculateLevel(parentInfo, stackLevel + 1);
                maxLevel = Math.Max(level, maxLevel);
            }
            return maxLevel;
        }

        // TODO performance
        private List<TableObj> findTables(string tableName)
        {
            return Scopes.SelectMany(s => s.Tables).Where(a => a.Name == tableName).ToList();
        }

        public List<ScopeObj> Scopes { get; }

        public class Builder
        {
            public Builder(ScopesConfig scopeConfig, IDatabase database)
            {
                this.ScopesConfig = scopeConfig;
                this.Database = database;
            }

            public IDatabase Database { get; }

            public ScopesConfig ScopesConfig { get; }

            public AllScopesObj Build()
            {
                return new AllScopesObj(this);
            }
        }
    }
}
