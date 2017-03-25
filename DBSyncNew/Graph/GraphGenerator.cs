using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBSyncNew.Graph
{
    public class GraphGenerator<TVertex, TEdge>
        where TVertex : VertexSource<TVertex, TEdge>
        where TEdge : EdgeSource<TVertex>

    {
        //TODO performance
        public List<Vertex<TVertex, TEdge>> GenerateGraph(IEnumerable<TVertex> vertices)
        {
            var verticesList = vertices.ToList();
            var dict = verticesList.Select(table => new Vertex<TVertex, TEdge>(table)).ToList();
            foreach (var vertex in dict)
            {
                foreach (
                    var key in
                        vertex.Value.OutgoingEdges.Where(
                            c => verticesList.Contains(c.End, EqualityComparer<TVertex>.Default)
                                 && verticesList.Contains(c.Start, EqualityComparer<TVertex>.Default)))
                {
                    var start = dict.Single(v => EqualityComparer<TVertex>.Default.Equals(v.Value, key.Start));
                    var end = dict.Single(v => EqualityComparer<TVertex>.Default.Equals(v.Value, key.End));

                    AddEdge(end, start, key);
                }
            }
            return dict;
        }


        private void AddEdge(Vertex<TVertex, TEdge> end, Vertex<TVertex, TEdge> start, TEdge value)
        {
            var edge = new Edge<TVertex, TEdge>(start, end, value);
            if (!start.Edges.Any(e => e.Start == start && e.End == end && EqualityComparer<TEdge>.Default.Equals(e.Value, value)))
            {
                start.Edges.Add(edge);
            }
            if (!end.Edges.Any(e => e.Start == start && e.End == end && EqualityComparer<TEdge>.Default.Equals(e.Value, value)))
            {
                end.Edges.Add(edge);
            }
        }
    }
}
