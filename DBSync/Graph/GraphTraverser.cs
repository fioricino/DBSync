using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBSync.Graph
{
    public static class GraphTraverser
    {
        public static void GenerateNonRootedScopeSelectScripts(ScopeInfo scope)
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

        public static void GenerateRootedScopeSelectScripts(ScopeInfo scope)
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

        private static List<Vertex<AliasInfo, ForeignKeyAliasInfo>> GetDigraph(IEnumerable<AliasInfo> tables)
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

        private static List<SGRoute> GetAllDirectedRoutes(List<Vertex<AliasInfo, ForeignKeyAliasInfo>> graph, IEnumerable<AliasInfo> rootTables)
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
                var fakeFK = new ForeignKeyAliasInfo() { Alias = rootTable, ReferencedAlias = rootTable };
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

        private static WhereClause GetNonReferencedWhereClause(string tableName, FilterColumnInfo filter)
        {
            return GetWhereClause(tableName, filter.ColumnName, filter.FilterClause, filter.IsSkippedOnDelete);
        }



        private static WhereClause GetWhereClause(string tableName, string columnName, string clause, bool isSkippedOnDelete)
        {
            return new WhereClause(clause.Replace("#COLUMN#", String.Format("[{0}].[{1}]", tableName, columnName)), isSkippedOnDelete);
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
    }
}
