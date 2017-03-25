using System;

namespace DBSyncNew.Graph
{
    public sealed class Edge<TVertex, TEdge> where TVertex : IEquatable<TVertex>
    {
        public Edge(Vertex<TVertex, TEdge> start, Vertex<TVertex, TEdge> end, TEdge value)
        {
            Start = start;
            End = end;
            Value = value;
        }

        public Vertex<TVertex, TEdge> Start { get; }

        public Vertex<TVertex, TEdge> End { get; }

        public TEdge Value { get; private set; }

        public override string ToString()
        {
            return $"{Start.Value} -> {End.Value}";
        }
    }
}
