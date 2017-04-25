using System;
using System.Collections.Generic;
using System.Linq;

namespace DBSyncNew.Graph
{
    public sealed class Vertex<TVertex, TEdge> 
        //TODO refactor equality comparison
        : IEquatable<Vertex<TVertex, TEdge>>
        where TVertex : IEquatable<TVertex>
    {
        public Vertex(TVertex value, bool isReferenced)
        {
            Value = value;
            Edges = new List<Edge<TVertex, TEdge>>();
            IsReferenced = isReferenced;
        }

        public TVertex Value { get; }

        public List<Edge<TVertex, TEdge>> Edges { get; }

        public bool IsReferenced { get; }

        public bool Equals(Vertex<TVertex, TEdge> other)
        {
            return other != null && EqualityComparer<TVertex>.Default.Equals(other.Value, Value);
        }

        public override string ToString()
        {
            int outgoingEdgesCount = Edges.Count(edge => edge.Start.Equals(this));
            int incomingEdgesCount = Edges.Count(edge => edge.End.Equals(this));
            return $"{Value}; Edges: {Edges.Count}, Outgoing: {outgoingEdgesCount}, Incoming: {incomingEdgesCount}";
        }
    }
}
