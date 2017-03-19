namespace DBSync
{
    public sealed class Edge<TVertex, TEdge>
    {
        public Edge(Vertex<TVertex, TEdge> start, Vertex<TVertex, TEdge> end, TEdge value)
        {
            Start = start;
            End = end;
            Value = value;
        }
        #region Public properties
        public Vertex<TVertex, TEdge> Start { get; private set; }
        public Vertex<TVertex, TEdge> End { get; private set; }
        public TEdge Value { get; private set; }
        #endregion
        #region Public methods
        public override string ToString()
        {
            return string.Format("{0} -> {1}", Start.Value, End.Value);
        }
        #endregion
    }
}
