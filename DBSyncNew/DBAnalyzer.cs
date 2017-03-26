using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using DBSyncNew.Database;
using DBSyncNew.Database.Interfaces;
using DBSyncNew.Database.MsSql;
using DBSyncNew.Graph;
using DBSyncNew.Scripts;

namespace DBSyncNew
{
    public class DBAnalyzer
    {
        private ObservableCollection<TableInfo> levelInfo = new ObservableCollection<TableInfo>();
        private string connectionString;
        private ScopeConfiguration scopes;
        private GraphGenerator<AliasInfo, ForeignKeyAliasInfo> graphGenerator = new GraphGenerator<AliasInfo, ForeignKeyAliasInfo>(); 
        private GraphTraverser<AliasInfo, ForeignKeyAliasInfo> graphTraverser = new  GraphTraverser<AliasInfo, ForeignKeyAliasInfo>();
        private ScriptGenerator scriptGenerator = new ScriptGenerator();

        public DBAnalyzer(
		string scopeConfigurationSource,
            string connectionString)
        {
            //todo move
            DBAdapterFactory.RegisterAdapter("mssql", cs => new MsSqlDBAdapter(cs));
            Errors = new List<string>();
            this.connectionString = connectionString;

            scopes = ReadScopesFromXml(scopeConfigurationSource);

            InitLevelInfo();

            FindPossibleErrors();
        }

        public List<string> Errors { get; private set; }

        private void InitLevelInfo()
        {
            var dbAdapter = DBAdapterFactory.GetDBAdapter("mssql", connectionString);

            var dbMetadata = dbAdapter.GetDbMetadata();

            foreach (var dbTable in dbMetadata)
            {
                var tableName = dbTable.Key;

                var tableInfo = scopes.Findtable(tableName).ToList();

                foreach (var info in tableInfo)
                {
                    InitColumnsFromDB(dbTable.Value, info);

                    levelInfo.Add(info);
                }

                //TODO WARN
                //if (!tableInfo.Any())
                //{
                //    //table is present in DB, b
                //    tableInfo.Add(new TableInfo() { Scope = scopes.Scopes.Single(s => s.ScopeType == ScopeType.None), Name = tableName });
                //}
            }

            InitRelationsFromDB(dbMetadata);

            InitArtificialRelations();

            foreach (var tableInfo in levelInfo)
            {
                tableInfo.Level = CalculateLevel(tableInfo, 0);
            }

            foreach (var scope in scopes.Scopes)
            {
                scope.Tables.Clear();
                scope.Tables.AddRange(levelInfo.Where(t => t.ScopeType == scope.ScopeType));
            }
        }

        private void InitArtificialRelations()
        {
            foreach (var scope in scopes.Scopes)
            {
                foreach (var aritificalForeignKey in scope.ArtificialForeignKeys)
                {
                    var column = scope.Tables.Single(t => t.Name == aritificalForeignKey.Table)
                        .Columns.Single(c => c.Name == aritificalForeignKey.Column);
                    var referencedColumn = scope.Tables.Single(t => t.Name == aritificalForeignKey.ReferencedTable)
                        .Columns.Single(c => c.Name == aritificalForeignKey.ReferencedColumn);

                    AddFkRelation(column, referencedColumn);
                }
            }
        }

        //TODO performance
        private void InitRelationsFromDB(IDBMetadata dbMetadata)
        {
            foreach (var table in dbMetadata)
            {
                foreach (var dbColumn in table.Value.Where(c => c.ReferencedColumn != null))
                {
                    var tableInfos = scopes.Findtable(dbColumn.TableName).ToList();
                    var referencedTableInfos = scopes.Findtable(dbColumn.ReferencedTable).ToList();

                    foreach (var tableInfo in tableInfos)
                    {
                        var column = tableInfo.Columns.Single(c => c.Name == dbColumn.Name);

                        //TODO WARN
                        foreach (
                            var referencedTableInfo in
                                referencedTableInfos.Where(r => r.ScopeType == tableInfo.ScopeType || r.ScopeType == ScopeType.Core)
                            )
                        {
                            var referencedColumn = referencedTableInfo.Columns.Single(c => c.Name == dbColumn.ReferencedColumn);
                            AddFkRelation(column, referencedColumn);
                        }
                    }
                }
            }
        }

        private static void InitColumnsFromDB(List<IDBColumn> dbColumns, TableInfo info)
        {
            foreach (var dbColumn in dbColumns)
            {
                var columnInfo = new ColumnInfo(info)
                {
                    Name = dbColumn.Name,
                    IsPk = dbColumn.IsPrimaryKey,
                    IsNullable = dbColumn.IsNullable,
                    DataType = dbColumn.DataType,
                    IsReadOnly = dbColumn.IsReadOnly,
                    Precision = dbColumn.Precision,
                    Scale = dbColumn.Scale,
                    MaxLength = dbColumn.MaxLength
                };
                info.Columns.Add(columnInfo);
            }
        }

   

        private static DataTable ReadTableDataFromDB(SqlConnection sqlConnection)
        {
            DataTable mdt;
            var tableCommand =
                new SqlCommand(@"SELECT distinct t.name TABLE_NAME
FROM sys.tables t

WHERE t.schema_id = 1
AND t.type = 'U'", sqlConnection);

            var tableReader = tableCommand.ExecuteReader();
            mdt = new DataTable();
            mdt.Load(tableReader);
            return mdt;
        }


        public string GenerateSqlMetaData()
        {
            return GenerateTuevTablesScript();
        }

        /// <summary>
        /// used in MetaDataGenerator.tt
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public string GenerateDataSetMetaData(ScopeType scope)
        {
            var tables = scopes.Scopes.Single(s => s.ScopeType == scope).OrderedTables.ToList();

            using (var connection = new SqlConnection(connectionString))
            {
                var command = String.Join("; ", tables.Select(t => String.Format("SELECT * FROM {0}", t.Name)));
                var adapter = new SqlDataAdapter(new SqlCommand(command, connection));
                var ds = new DataSet(SyncMetaDataHelper.GetDataSetName(scope));

                adapter.TableMappings.AddRange(Enumerable.Range(0, tables.Count)
                    .Select(
                        number =>
                            new DataTableMapping(
                                String.Format("Table{0}", number == 0 ? "" : number.ToString()), tables[number].Name)).ToArray());
                
                adapter.FillSchema(ds, SchemaType.Mapped);
                foreach (DataTable table in ds.Tables)
                {
                    var tableInfo = tables.Single(t => t.Name == table.TableName);
                    if (table.Columns.Contains("ROW_VERSION") && !tableInfo.KeepRowVersion)
                    {
                        table.Columns.Remove("ROW_VERSION");
                    }
                    foreach (DataColumn column in table.Columns)
                    {
                        column.AutoIncrement = false;
                        column.ReadOnly = false;
                    }
                }
                using (var writer = new StringWriter())
                {
                    ds.WriteXmlSchema(writer);
                    return writer.ToString();
                }
            }
        }

        public SelectMetaDataGenerationPattern GetMetaDataGenerationPattern(ScopeType scopeType)
        {
            return scopes.Scopes.Single(s => s.ScopeType == scopeType).MetaDataGenerationPattern;
        }

        public Tuple<string, string> GenerateDataSetSelectMetaData(ScopeType scopeType)
        {
            var dict = GenerateTableSelectMetaData(scopeType);

            var selectScript = String.Join("\n", dict.Select(d => d.SelectScript));
            dict.Reverse();
            var sb = new StringBuilder();
            sb.AppendLine(String.Join("\n", dict.Select(d => d.CreateTableScript)));
            sb.AppendLine();
            sb.AppendLine(String.Join("\n", dict.Select(d => d.SelectIdscript)));
            sb.AppendLine();
            sb.AppendLine(String.Join("\n", dict.Select(d => d.DeleteScript)));

            return new Tuple<string, string>(selectScript, sb.ToString());
        }

        public List<SyncScripts> GenerateTableSelectMetaData(ScopeType scopeType)
        {
            var scope = scopes.Scopes.Single(s => s.ScopeType == scopeType);

            if (scope.HasRoot)
            {
                GenerateRootedScopeSelectScripts(scope);
            }
            else
            {
                GenerateNonRootedScopeSelectScripts(scope);
            }
           
            var result = new List<SyncScripts>();

            foreach (var table in scope.OrderedTables)
            {
                var scripts = new SyncScripts
                              {
                                  TableName = table.Name,
                                  SelectScript = GenerateSelectScript(table, scope.HasRoot, false),
                                  CreateTableScript = table.IsSkippedOnDelete ? String.Empty : GenerateCreateTableScript(table),
                                  SelectIdscript = table.IsSkippedOnDelete ? String.Empty : GenerateSelectScript(table, scope.HasRoot, true),
                                  DeleteScript = table.IsSkippedOnDelete ? String.Empty : GenerateDeleteScript(table)
                              };

                result.Add(scripts);
            }

            return result;
        }


        private string GenerateCreateTableScript(TableInfo table)
        {
            return String.Format("DECLARE {0} TABLE({1})", table.NameForTempTable,
                        String.Join(", ",
                            table.PKColumns.Select(c => String.Format("{0} {1}", c.Name, c.DataType))));
        }

        private string GenerateDeleteScript(TableInfo table)
        {
            return String.Format("DELETE {0} FROM {0} tab JOIN {1} idlist ON {2}", table.Name, table.NameForTempTable,
                        String.Join(" AND ",
                            table.PKColumns.Select(c => String.Format("tab.{0} = idlist.{0}", c.Name))));
        }

        private string GenerateSelectScript(TableInfo table, bool useDistinct, bool isForDelete)
        {
            var sb = new StringBuilder();

            if (isForDelete)
            {
                sb.AppendLine(String.Format("INSERT INTO @{0}_FOR_DELETE", table.Name));
            }

            for (int i = 0; i < table.Aliases.Count; i++)
            {
                var alias = table.Aliases[i];

                var selectedColumns = isForDelete ? String.Join(", ", alias.Table.PKColumns.Select(c => 
                    String.Format("{0}.{1}", alias.NameOrAlias, c.Name))) : String.Format("{0}.*", alias.NameOrAlias);

                if (alias.Scripts.Count == 0)
                {
                    throw new InvalidOperationException(String.Format("Table {0} has no scripts.", alias.NameOrAlias));
                }

                for (int index = 0; index < alias.Scripts.Count; index++)
                {
                    var script = alias.Scripts[index];
                    sb.AppendFormat("SELECT {0} {1} ", useDistinct ? "DISTINCT" : "", selectedColumns);

                    sb.AppendLine();
                    sb.AppendLine("FROM " + script.FromCause);
                    var whereClauses = script.WhereCause.Where(c => !isForDelete || !c.IsSkippedOnDelete)
                            .Select(wc => wc.Clause)
                            .ToList();
                    if (whereClauses.Count > 0)
                    {
                        sb.AppendLine("WHERE ");
                        sb.AppendLine(String.Join("\nAND ", whereClauses));

                        if (index < alias.Scripts.Count - 1)
                        {
                            sb.AppendLine();

                            sb.AppendLine("UNION");
                            sb.AppendLine();
                        }
                    }
                }

                if (i < table.Aliases.Count - 1)
                {
                    sb.AppendLine();
                    sb.AppendLine("UNION");
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine(";");
                }
            }
            return sb.ToString();
        }

        private void GenerateNonRootedScopeSelectScripts(ScopeInfo scope)
        {
            foreach (var table in scope.Tables)
            {
                foreach (var alias in table.Aliases)
                {
                    string fromClause = alias.NameOrAlias;

                    var whereClauses = table.FilterColumns.Union(scope.FilterColumns).Select(filter => GetNonReferencedWhereClause(alias.NameOrAlias, filter)).ToList();

                    alias.Scripts.Add(new SyncScript(fromClause, whereClauses));
                }
            }
        }

        private WhereClause GetNonReferencedWhereClause(string tableName, FilterColumnInfo filter)
        {
            return GetWhereClause(tableName, filter.ColumnName, filter.FilterClause, filter.IsSkippedOnDelete);
        }

        private void GenerateRootedScopeSelectScripts(ScopeInfo scope)
        {
            List<Vertex<AliasInfo, ForeignKeyAliasInfo>> tableGraph = graphGenerator.GenerateGraph(scope.Aliases);

            IEnumerable<AliasInfo> rootAliases = scope.RootAliases;

            List<SGRoute<AliasInfo, ForeignKeyAliasInfo>> routes = graphTraverser.GetAllDirectedRoutes(tableGraph, rootAliases.Select(r => tableGraph.Single(t => 
            EqualityComparer<AliasInfo>.Default.Equals(t.Value, r))));

            foreach (var route in routes)
            {
                var sgTable = route.Start;
                //var syncTable = scope.Tables.First(table => table.Name == sgTable.Name);

                string fromClause;

                var sbuilder = new StringBuilder();

                //Print first table
                sbuilder.Append(route.Directed[0].Start.Value);

                sbuilder.AppendLine();

                for (var i = 0; i < route.Directed.Count - 1; i++)
                {
                    var key = route.Directed[i];

                    scriptGenerator.AppendJoin(sbuilder, key.Value);
                }

                var lastKey = route.Directed[route.Directed.Count - 1];

                var filterColumnInfo = route.RootTable.Value.FilterColumns.SingleOrDefault(c => c.IsReferenced);
                var goesFromRoot = 
                                   filterColumnInfo != null && lastKey.Value.ReferencedColumn != filterColumnInfo.ColumnName;

                var isSelf = lastKey.Start.Value.IsRoot;

                if (goesFromRoot && !isSelf
                    //lastKey.Column.Table != lastKey.ReferencedColumn.Table
                    /*Special case for print rotTble it self*/)
                {
                    scriptGenerator.AppendJoin(sbuilder, lastKey.Value);
                }
                fromClause = sbuilder.ToString().Trim(' ', '\t', '\n', '\r');

                //string filterColumnName = route.RootTable.GetFilterColumnName();
                //if (!String.IsNullOrEmpty(filterColumnName))
                //{
                //    whereClauses.Add(GetWhereClause(useRootTable ? route.RootTable.Name : lastKey.Column.Table.Name, useRootTable ? filterColumnName : lastKey.Column.Name, route.RootTable.FilterClause));
                //}

                var whereClauses =
                    route.AllVertices.SelectMany(t => t.Value.FilterColumns)
                        //.Union(
                        //    route.AllVertices.Where(t => !t.IgnoreParentFilter).SelectMany(t => t.Table.FilterColumns)
                                .Union(sgTable.Value.Table.Scope.FilterColumns)
                                .Distinct()
                        .Select(
                            filter =>
                                !goesFromRoot && !isSelf && filter.IsReferenced
                                    ? GetWhereClause(lastKey.Start.Value.NameOrAlias, lastKey.Value.Column, filter.FilterClause, filter.IsSkippedOnDelete)
                                    : GetNonReferencedWhereClause(filter.Table.NameOrAlias, filter))
                        .ToList();

                //if (table != route.RootTable && !String.IsNullOrEmpty(table.FilterColumnName))
                    //{
                    //    whereClauses.Add(GetWhereClause(table.Name, table.FilterColumnName, table.FilterClause));
                    //}


                sgTable.Value.Scripts.Add(new SyncScript(fromClause, whereClauses));
            }
        }

        private WhereClause GetWhereClause(string tableName, string columnName, string clause, bool isSkippedOnDelete)
        {
            return new WhereClause(clause.Replace("#COLUMN#", String.Format("[{0}].[{1}]", tableName, columnName)), isSkippedOnDelete);
        }

        public string GenerateInsertMetaData(ScopeType scope)
        {
            var result = new StringBuilder();
            var tables = scopes.Scopes.Single(s => s.ScopeType == scope).OrderedTables;
            foreach (var table in tables)
            {
                result.AppendLine(GenerateTableInsertMetaData(table));
            }
            return result.ToString();
        }

        private string GenerateTableInsertMetaData(TableInfo table)
        {
            var result = new StringBuilder();

            result.AppendLine(table.GetDropSPStatement());
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine(table.GetDropTVPStatement());
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine(table.GetTVPStatement());
            result.AppendLine("GO");
            result.AppendLine();

            result.AppendLine(table.GetSPStatement());
            result.AppendLine("GO");
            result.AppendLine();

            return result.ToString();
        }


        private ScopeConfiguration ReadScopesFromXml(string scopeConfigurationSource)
        {
            ScopeConfiguration result;
            var serializer = new XmlSerializer(typeof(ScopeConfiguration));

            using (var file = File.OpenRead(scopeConfigurationSource))
            {
                result = (ScopeConfiguration)serializer.Deserialize(file);
            }
            result.SetRelations();
            if (result.Scopes.All(s => s.ScopeType != ScopeType.None))
            {
                result.Scopes.Add(new ScopeInfo() {ScopeType = ScopeType.None});
            }

            return result;
        }

   

        private static void AddFkRelation(ColumnInfo column, ColumnInfo referencedColumn)
        {
            column.ReferencedColumn = referencedColumn;
            var fk = new ForeignKeyInfo()
                     {
                         Column = column,
                         ReferencedColumn = referencedColumn,
                     };
            column.Table.ForeignKeys.Add(fk);
            column.ReferencedColumn.Table.ForeignKeys.Add(fk);

            foreach (var alias in column.Table.Aliases)
            {
                foreach (var refAlias in referencedColumn.Table.Aliases)
                {
                    var direction = column.Table.Scope.GetForeignKeyDirection(column.Name, alias.NameOrAlias,
                        referencedColumn.Name, refAlias.NameOrAlias);
                    var aliasFk = new ForeignKeyAliasInfo()
                    {
                        Column = fk.Column.Name,
                        ReferencedColumn = fk.ReferencedColumn.Name,
                        Alias = alias,
                        ReferencedAlias = refAlias,
                        Direction = direction
                    };
                    refAlias.ForeignKeys.Add(aliasFk);
                    alias.ForeignKeys.Add(aliasFk);
                }
            }
        }

        private int CalculateLevel(TableInfo tableInfo, int stackLevel)
        {
            var maxLevel = stackLevel;
            foreach (var parentInfo in tableInfo.ParentInfos)
            {
                var level = CalculateLevel(parentInfo, stackLevel + 1);
                maxLevel = Math.Max(level, maxLevel);
            }
            return maxLevel;
        }

        private void FindPossibleErrors()
        {
            foreach (var info in levelInfo)
            {
                foreach (var parentInfo in info.ParentInfos)
                {
                    if (parentInfo.ScopeType != ScopeType.Core && parentInfo.ScopeType != info.ScopeType)
                    {
                        throw new Exception(String.Format("Table {0} in scope {1} has dependency on table {2} in scope {3}.",
                            info.Name, info.ScopeType, parentInfo.Name, parentInfo.ScopeType));
                    }
                }

                if (info.PossibleWrongKey)
                {
                    throw new Exception(String.Format("Table {0} in scope {1} has PK of type {2} GUID", info.Name, info.ScopeType, 
                        info.IsGuid ? "" : "other than"));
                }

                if (info.PossibleWrongConflictResolution)
                {
                    throw new Exception(String.Format("Table {0} in scope {1} has invalid conflict resolution policy.", info.Name, info.ScopeType));
                }
            }
        }

        private string GenerateTuevTablesScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine("USE TUEV_SUED");
            sb.AppendLine("GO");
            sb.AppendLine();
            sb.AppendLine("print '---- SYS_TUEV_TABLES script started -----'");
            sb.AppendLine();
            sb.AppendLine("DELETE FROM [dbo].[SYS_TUEV_TABLES]");
            sb.AppendLine("SET IDENTITY_INSERT dbo.SYS_TUEV_TABLES ON");
            sb.AppendLine();
            var orderedTables = levelInfo.OrderBy(t => t.Name).ToList();
            for (int index = 0; index < orderedTables.Count; index++)
            {
                TableInfo tableLevelInfo = orderedTables[index];
                string insertCommandText = string.Format(
                    @"INSERT INTO [dbo].[SYS_TUEV_TABLES] ([ID], [TABLE_NAME], [SCOPE_TYPE], [LEVEL], [IS_DATE_FILTERED]) VALUES({0},'{1}',{2},{3},{4});",
                    index, tableLevelInfo.Name, (int)tableLevelInfo.ScopeType, tableLevelInfo.Level, 0);
                sb.AppendLine(insertCommandText);
            }
            sb.AppendLine();
            sb.AppendLine("SET IDENTITY_INSERT dbo.SYS_TUEV_TABLES OFF");
            return sb.ToString();
        }
    }
}
