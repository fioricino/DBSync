using System;
using System.Collections.Generic;

namespace DBSyncNew.Graph
{
    public abstract class VertexSource<TVertex, TEdge> : IEquatable<TVertex> where TEdge : EdgeSource<TVertex> 
        where TVertex : IEquatable<TVertex>
    {
         public abstract IEnumerable<TEdge> OutgoingEdges { get; }
        public abstract bool Equals(TVertex other);

        public abstract bool IsReferenced { get; }
    }
}