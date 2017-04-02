namespace DBSyncNew
{
    public interface ITableOrAlias
    {
        string NameOrAlias { get; } 
        int Level { get; }
    }
}