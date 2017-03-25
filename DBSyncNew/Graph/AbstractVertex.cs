using System;
using System.Collections.Generic;

namespace DBSyncNew.Graph
{
    public abstract class AbstractVertex<TVertex, TEdge> : IEquatable<TVertex> where TEdge : AbstractEdge<TVertex> 
        where TVertex : IEquatable<TVertex>
    {
         public abstract IEnumerable<TEdge> OutgoingEdges { get; }
        public abstract bool Equals(TVertex other);
    }
}