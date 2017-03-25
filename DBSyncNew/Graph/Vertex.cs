using System;
using System.Collections.Generic;
using System.Linq;

namespace DBSyncNew.Graph
{
    public sealed class Vertex<TVertex, TEdge>
        where TVertex : IEquatable<TVertex>
    {
        public Vertex(TVertex value)
        {
            Value = value;
            Edges = new List<Edge<TVertex, TEdge>>();
        }

        public TVertex Value { get; }

        public List<Edge<TVertex, TEdge>> Edges { get; }

        public override string ToString()
        {
            int outgoingEdgesCount = Edges.Count(edge => edge.Start.Equals(this));
            int incomingEdgesCount = Edges.Count(edge => edge.End.Equals(this));
            return $"{Value}; Edges: {Edges.Count}, Outgoing: {outgoingEdgesCount}, Incoming: {incomingEdgesCount}";
        }
    }
}
