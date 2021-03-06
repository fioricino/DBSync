﻿using System;
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

namespace DBSync
{
    public class DBAnalyzer
    {
        private string permissionsSqlFileName;
        private string uniqueColumnsFileName;
        private string notNullColumnsFileName;
        private string scriptFilesFolder;
        private ObservableCollection<TableInfo> levelInfo = new ObservableCollection<TableInfo>();
        private string connectionString;
        private ScopeConfiguration scopes;

        public DBAnalyzer(string permissionsSqlFileName ,
        string uniqueColumnsFileName,
        string notNullColumnsFileName,
        string scriptFilesFolder,
		string scopeConfigurationSource,
            string connectionString)
        {
            Errors = new List<string>();
            this.permissionsSqlFileName = permissionsSqlFileName;
            this.uniqueColumnsFileName = uniqueColumnsFileName;
            this.notNullColumnsFileName = notNullColumnsFileName;
            this.scriptFilesFolder = scriptFilesFolder;
            this.connectionString = connectionString;

            InitScopes(scopeConfigurationSource);

            InitLevelInfo();

            FindPossibleErrors();
        }

        public List<string> Errors { get; private set; }
        
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
            var rootAliases = scope.RootAliases;

            var tableGraph = GetDigraph(scope.Aliases);


            var routes = GetAllDirectedRoutes(tableGraph, rootAliases);

            foreach (var route in routes)
            {
                var sgTable = route.Start;
                //var syncTable = scope.Tables.First(table => table.Name == sgTable.Name);

                string fromClause;

                var sbuilder = new StringBuilder();

                //Print first table
                sbuilder.Append(route.Directed[0].Alias);

                sbuilder.AppendLine();

                for (var i = 0; i < route.Directed.Count - 1; i++)
                {
                    var key = route.Directed[i];

                    AppendJoin(sbuilder, key);
                }

                var lastKey = route.Directed[route.Directed.Count - 1];

                var filterColumnInfo = route.RootTable.FilterColumns.SingleOrDefault(c => c.IsReferenced);
                var goesFromRoot = 
                    //lastKey.Column.Table.IsRoot
                    //||
                                   filterColumnInfo != null && lastKey.ReferencedColumn != filterColumnInfo.ColumnName;

                var isSelf = lastKey.Alias.IsRoot;

                if (goesFromRoot && !isSelf
                    //lastKey.Column.Table != lastKey.ReferencedColumn.Table
                    /*Special case for print rotTble it self*/)
                {
                    AppendJoin(sbuilder, lastKey);
                }
                fromClause = sbuilder.ToString().Trim(' ', '\t', '\n', '\r');

                //string filterColumnName = route.RootTable.GetFilterColumnName();
                //if (!String.IsNullOrEmpty(filterColumnName))
                //{
                //    whereClauses.Add(GetWhereClause(useRootTable ? route.RootTable.Name : lastKey.Column.Table.Name, useRootTable ? filterColumnName : lastKey.Column.Name, route.RootTable.FilterClause));
                //}

                var whereClauses =
                    route.AllTables.SelectMany(t => t.FilterColumns)
                        //.Union(
                        //    route.AllTables.Where(t => !t.IgnoreParentFilter).SelectMany(t => t.Table.FilterColumns)
                                .Union(sgTable.Table.Scope.FilterColumns)
                                .Distinct()
                        .Select(
                            filter =>
                                !goesFromRoot && !isSelf && filter.IsReferenced
                                    ? GetWhereClause(lastKey.Alias.NameOrAlias, lastKey.Column, filter.FilterClause, filter.IsSkippedOnDelete)
                                    : GetNonReferencedWhereClause(filter.Table.NameOrAlias, filter))
                        .ToList();

                //if (table != route.RootTable && !String.IsNullOrEmpty(table.FilterColumnName))
                    //{
                    //    whereClauses.Add(GetWhereClause(table.Name, table.FilterColumnName, table.FilterClause));
                    //}


                sgTable.Scripts.Add(new SyncScript(fromClause, whereClauses));
            }
        }

        private WhereClause GetWhereClause(string tableName, string columnName, string clause, bool isSkippedOnDelete)
        {
            return new WhereClause(clause.Replace("#COLUMN#", String.Format("[{0}].[{1}]", tableName, columnName)), isSkippedOnDelete);
        }

        public string GenerateSqlMetaData()
        {
            var result = new StringBuilder();

            result.Append(GenerateTuevTablesScript());

            return result.ToString();
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


        private static void AppendJoin(StringBuilder sbuilder, ForeignKeyAliasInfo key)
        {
            sbuilder.Append(" JOIN ");
            sbuilder.Append(key.ReferencedAlias.NameWithAlias);
            sbuilder.Append(" ON ");
            sbuilder.Append(string.Format("{0}.{1} = {2}.{3}",
                key.Alias.NameOrAlias,
                key.Column,
                key.ReferencedAlias.NameOrAlias,
                key.ReferencedColumn));
            sbuilder.AppendLine();
        }

        private List<SGRoute> GetAllDirectedRoutes(List<Vertex<AliasInfo, ForeignKeyAliasInfo>> graph, IEnumerable<AliasInfo> rootTables)
        {
            var result = new List<SGRoute>();
            foreach (var rootTable in rootTables)
            {
                var rootVertex = graph.First(vertex => vertex.Value == rootTable);
                var vertexRoutes = GetAllDirectedRoutesInternal(rootVertex);
                result.AddRange(vertexRoutes.Select(list => SGRoute.Create(rootTable, list.Select(edge => edge.Value).ToList())).ToList());
                //Add itself route
                
                //var fakeColumn = new ColumnInfo(rootTable) {};
                var column = rootTable.Table.FilterColumns.FirstOrDefault(c => c.IsReferenced);
                //if (column != null)
                //{
                //    fakeColumn.Name = column.ColumnName;
                //}
                var fakeFK = new ForeignKeyAliasInfo() { Alias = rootTable, ReferencedAlias = rootTable};
                if (column != null)
                {
                    fakeFK.Column = fakeFK.ReferencedColumn = column.ColumnName;
                }
                //, Column = column.ColumnName, ReferencedColumn = column.ColumnName};
                result.Add(new SGRoute(new List<ForeignKeyAliasInfo> { fakeFK }, rootTable));
            }

            return result;
        }

        private static List<List<Edge<AliasInfo, ForeignKeyAliasInfo>>> GetAllDirectedRoutesInternal(Vertex<AliasInfo, ForeignKeyAliasInfo> vertex)
        {
            var result = new List<List<Edge<AliasInfo, ForeignKeyAliasInfo>>>();

            //var vertexWithRef = vertexesWithRoundReferences.FirstOrDefault(vRef => vRef.Vertex == vertex);
            //if (vertexWithRef != null)
            //{
            //    vertexWithRef.ProcessedOrder = vertexesWithRoundReferences.Max(vRef => vRef.ProcessedOrder.HasValue ? vRef.ProcessedOrder : 0) + 1;
            //}
            foreach (var edge in vertex.Edges)
            {
                Vertex<AliasInfo, ForeignKeyAliasInfo> nextVertex = null;
                if (edge.Value.Direction == ForeignKeyDirection.Direct && vertex != edge.End)
                {
                    //Direct edge
                    nextVertex = edge.End;
                }
                else if (edge.Value.Direction == ForeignKeyDirection.Reversed && vertex != edge.Start)
                {
                    //Referenced edge
                    nextVertex = edge.Start;
                }
                if (nextVertex == null)
                {
                    //ignored edge
                    continue;
                }
                //var nextVertexRef = vertexesWithRoundReferences.FirstOrDefault(vRef => vRef.Vertex == nextVertex);
                //if (nextVertexRef != null && (vertexWithRef == null || nextVertexRef.ProcessedOrder < vertexWithRef.ProcessedOrder))
                //{
                //    //NOTE: Ignore this edge. It will be handle during handle vertex with round references with less process order
                //    continue;
                //}

                result.Add(new List<Edge<AliasInfo, ForeignKeyAliasInfo>> { edge });
                var nextRoutes = GetAllDirectedRoutesInternal(nextVertex).ToList();
                foreach (var nextRoute in nextRoutes)
                {
                    nextRoute.Insert(0, edge);
                    result.Add(nextRoute);
                }
            }

            return result;
        }

        private List<Vertex<AliasInfo, ForeignKeyAliasInfo>> GetDigraph(IEnumerable<AliasInfo> tables)
        {
            var dict = tables.ToDictionary(table => table.NameOrAlias, table => new Vertex<AliasInfo, ForeignKeyAliasInfo>(table));
            foreach (var vertex in dict.Values)
            {
                foreach (var key in vertex.Value.ForeignKeys.Where(c => tables.Contains(c.ReferencedAlias) && tables.Contains(c.Alias)))
                {
                    var start = dict.Values.Single(v => v.Value == key.Alias);
                    //.Direction == ForeignKeyDirection.Direct ? dict.Values.Single(v => v.Value == key.Column.Table) : dict.Values.Single(v => v.Value == key.ReferencedColumn.Table);
                    var end = dict.Values.Single(v => v.Value == key.ReferencedAlias);
                    
                        //key.Direction == ForeignKeyDirection.Direct ? dict.Values.Single(v => v.Value == key.ReferencedColumn.Table) : dict.Values.Single(v => v.Value == key.Column.Table);
                    AddEdge(end, start, key);
                }
            }
            return dict.Values.ToList();
        }

        private static void AddEdge(Vertex<AliasInfo, ForeignKeyAliasInfo> end, Vertex<AliasInfo, ForeignKeyAliasInfo> start, ForeignKeyAliasInfo value)
        {
            var edge = new Edge<AliasInfo, ForeignKeyAliasInfo>(start, end, value);
            if (!start.Edges.Any(e => e.Start == start && e.End == end && e.Value == value))
            {
                start.Edges.Add(edge);
            }
            if (!end.Edges.Any(e => e.Start == start && e.End == end && e.Value == value))
            {
                end.Edges.Add(edge);
            }
        }

        private TableInfo[] GetMetadataTables()
        {
            var tableNames = levelInfo.Where(x => x.IsTableMetadata).ToArray();

            return tableNames.ToArray();
        }

        private string GetPermissionScript()
        {
            var fileText = GetScriptFileText(permissionsSqlFileName);
            var db = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
            return String.Format(fileText, db);
        }

        private string GetUniqueColumnsScript()
        {
            return GetScriptFileText(uniqueColumnsFileName);
        }

        private string GetNotNullColumnsScript()
        {
            return GetScriptFileText(notNullColumnsFileName);
        }

        private string GetScriptFileText(string fileName)
        {
            string folder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = System.IO.Path.Combine(folder, scriptFilesFolder, fileName);

            if (!File.Exists(filePath))
            {
                throw new ApplicationException(string.Format("File not found:\n{0}", filePath));
            }

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        private void InitScopes(string scopeConfigurationSource)
        {
            var serializer = new XmlSerializer(typeof(ScopeConfiguration));

            using (var file = File.OpenRead(scopeConfigurationSource))
            {
                scopes = (ScopeConfiguration)serializer.Deserialize(file);
            }
            scopes.SetRelations();
            if (scopes.Scopes.All(s => s.ScopeType != ScopeType.None))
            {
                scopes.Scopes.Add(new ScopeInfo() {ScopeType = ScopeType.None});
            }
        }

        private void InitLevelInfo()
        {
            DataTable mdt;
            DataTable mdc;
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                var tableCommand =
                    new SqlCommand(@"SELECT distinct t.name TABLE_NAME
FROM sys.tables t

WHERE t.schema_id = 1
AND t.type = 'U'", sqlConnection);

                //black magic
                var columnCommand = new SqlCommand(@"SELECT DISTINCT 
c.name COLUMN_NAME, 
tp.name COLUMN_TYPE,
COLUMNPROPERTY(c.object_id, c.name, 'charmaxlen') MAX_LENGTH,
c.precision COLUMN_PRECISION,
c.scale COLUMN_SCALE,
c.is_nullable IS_NULLABLE,
CONVERT(BIT, CASE WHEN (c.is_identity = 1 OR tp.name = 'timestamp') THEN 1 ELSE 0 END) IS_READONLY,
CONVERT(BIT, COALESCE(ix.is_primary_key, 0)) IS_PK,
t.name TABLE_NAME,
tt.name REFERENCED_TABLE,
cc.NAME REFERENCED_COLUMN,
c.column_id ORDINAL_NUMBER

FROM sys.columns c
LEFT JOIN sys.types tp
ON c.system_type_id = tp.system_type_id
AND c.user_type_id = tp.user_type_id
LEFT JOIN
(SELECT ic.column_id, ic.object_id, is_primary_key 
FROM sys.index_columns ic
JOIN sys.indexes i
ON i.index_id = ic.index_id
AND i.object_id = ic.object_id
AND i.is_primary_key = 1) ix
ON c.column_id = ix.column_id
AND c.object_id = ix.object_id

LEFT JOIN sys.tables t
ON t.object_id = c.object_id
LEFT JOIN sys.foreign_key_columns f
ON f.parent_object_id = c.object_id
AND f.parent_column_id = c.column_id
LEFT JOIN sys.tables tt
ON f.referenced_object_id = tt.object_id
LEFT JOIN sys.columns cc
ON f.referenced_object_id = cc.object_id
AND f.referenced_column_id = cc.column_id
WHERE t.schema_id = 1
AND t.type = 'U'
ORDER BY TABLE_NAME, ORDINAL_NUMBER
                                                                ", sqlConnection);

                sqlConnection.Open();
                var tableReader = tableCommand.ExecuteReader();
                mdt = new DataTable();
                mdt.Load(tableReader);

                var columnReader = columnCommand.ExecuteReader();
                mdc = new DataTable();
                mdc.Load(columnReader);
            }
	     
		        foreach (DataRow tableRow in mdt.Rows)
		        {
			        //init base table data
			        var tableName = tableRow["TABLE_NAME"].ToString();

                    var tableInfo = scopes.Findtable(tableName).ToList();

		            if (!tableInfo.Any())
		            {
		                tableInfo.Add(new TableInfo() {Scope = scopes.Scopes.Single(s => s.ScopeType == ScopeType.None), Name = tableName});
		            }

		            foreach (var info in tableInfo)
		            {
                        levelInfo.Add(info);
		            }
                }
	       
	       
	        //init columns
            foreach (var tableInfo in levelInfo)
            {
                var columnRows = mdc.Rows.Cast<DataRow>().Where(row => row["TABLE_NAME"].ToString() == tableInfo.Name);

                foreach (DataRow columnRow in columnRows)
                {
                    var columnInfo = new ColumnInfo(tableInfo)
                    {
                        Name = columnRow.Field<string>("COLUMN_NAME"),
                        IsPk = columnRow.Field<bool>("IS_PK"),
                        IsNullable = columnRow.Field<bool>("IS_NULLABLE"),
                        DataType = columnRow.Field<string>("COLUMN_TYPE"),
                        IsReadOnly = columnRow.Field<bool>("IS_READONLY"),
                        Precision = columnRow.Field<byte>("COLUMN_PRECISION"),
                        Scale = columnRow.Field<byte>("COLUMN_SCALE"),
                        MaxLength = columnRow.Field<int?>("MAX_LENGTH"),
                    };
                    tableInfo.Columns.Add(columnInfo);
                }
            }

            //init relations
            var allColumns = levelInfo.SelectMany(l => l.Columns).ToList();
            foreach (var row in mdc.Rows.Cast<DataRow>().Where(row => !(row["REFERENCED_COLUMN"] is DBNull)))
            {
                var columnName = row["COLUMN_NAME"].ToString();
                var tableName = row["TABLE_NAME"].ToString();
                var referencedColumnName = row["REFERENCED_COLUMN"].ToString();
                var referencedTableName = row["REFERENCED_TABLE"].ToString();

                if (!scopes.Findtable(tableName).Any() || !scopes.Findtable(referencedTableName).Any())
                {
                    continue;
                }

                var columns = allColumns.Where(c => c.Name == columnName && c.Table.Name == tableName);
                foreach (var column in columns)
                {
                    var referencedColumn = allColumns.Single(c => c.Name == referencedColumnName && c.Table.Name == referencedTableName
                        && (c.Table.Scope.ScopeType == column.Table.Scope.ScopeType || c.Table.Scope.ScopeType == ScopeType.Core));


                    AddFkRelation(column, referencedColumn);

                   
                }
               
            }
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

            //calculate levels
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
            for (int index = 0; index < levelInfo.Count; index++)
            {
                TableInfo tableLevelInfo = levelInfo[index];
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
