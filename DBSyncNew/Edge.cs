namespace DBSyncNew
{
    public sealed class Edge<TV, TE>
    {
        public Edge(Vertex<TV, TE> start, Vertex<TV, TE> end, TE value)
        {
            Start = start;
            End = end;
            Value = value;
        }
        #region Public properties
        public Vertex<TV, TE> Start { get; private set; }
        public Vertex<TV, TE> End { get; private set; }
        public TE Value { get; private set; }
        #endregion
        #region Public methods
        public override string ToString()
        {
            return string.Format("{0} -> {1}", Start.Value, End.Value);
        }
        #endregion
    }
}
