using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBSyncNew.Graph
{
    public class GraphTraverser
    {
        public List<SGRoute> GetAllDirectedRoutes(List<Vertex<AliasInfo, ForeignKeyAliasInfo>> graph, IEnumerable<AliasInfo> rootTables)
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
    }
}
