using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSyncNew.SchemaObjects;

namespace DBSyncNew.Graph
{
    public class GraphTraverser<TVertex, TEdge> where TVertex : IEquatable<TVertex> where TEdge : IReversable<TEdge>
        //TODO remove
        , new()
    {
        public List<SGRoute<TVertex, TEdge>> GetAllDirectedRoutes(List<Vertex<TVertex, TEdge>> graph, IEnumerable<Vertex<TVertex, TEdge>> rootTables)
        {
            var result = new List<SGRoute<TVertex, TEdge>>();
            foreach (var rootTable in rootTables)
            {
                var rootVertex = graph.First(vertex => vertex.Equals(rootTable));
                var vertexRoutes = GetAllDirectedRoutesInternal(rootVertex);
                result.AddRange(vertexRoutes.Select(list => SGRoute<TVertex, TEdge>.Create(rootTable, list)).ToList());
                //Add itself route

                if (rootTable.IsReferenced)
                {
                    
                }
                //var column = rootTable.Table.FilterColumns.FirstOrDefault(c => c.IsReferenced);
           
                var fakeFK = new Edge<TVertex, TEdge>(rootTable, rootTable, new TEdge(), ForeignKeyDirection.Direct);
                //if (column != null)
                //{
                //    fakeFK.Column = fakeFK.ReferencedColumn = column.ColumnName;
                //}
                result.Add(new SGRoute<TVertex, TEdge>(new List<Edge<TVertex, TEdge>> { fakeFK }, rootTable));
            }

            return result;
        }

        private static List<List<Edge<TVertex, TEdge>>> GetAllDirectedRoutesInternal(Vertex<TVertex, TEdge> vertex)
        {
            var result = new List<List<Edge<TVertex, TEdge>>>();

            //var vertexWithRef = vertexesWithRoundReferences.FirstOrDefault(vRef => vRef.Vertex == vertex);
            //if (vertexWithRef != null)
            //{
            //    vertexWithRef.ProcessedOrder = vertexesWithRoundReferences.Max(vRef => vRef.ProcessedOrder.HasValue ? vRef.ProcessedOrder : 0) + 1;
            //}
            foreach (var edge in vertex.Edges)
            {
                Vertex<TVertex, TEdge> nextVertex = null;
                if (edge.Direction == ForeignKeyDirection.Direct && !EqualityComparer<Vertex<TVertex, TEdge>>.Default.Equals(vertex, edge.End))
                {
                    //Direct edge
                    nextVertex = edge.End;
                }
                else if (edge.Direction == ForeignKeyDirection.Reversed && !EqualityComparer<Vertex<TVertex, TEdge>>.Default.Equals(vertex, edge.Start))
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

                result.Add(new List<Edge<TVertex, TEdge>> { edge });
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
