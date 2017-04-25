namespace DBSyncNew.Database.Read.Interfaces
{
    public interface IDBForeignKey
    {
        string Table { get; }
        string Column { get; }
        string ReferencedTable { get; }
        string ReferencedColumn { get; }
    }
}