namespace DBSyncNew.Database.Interfaces
{
    public interface IDBColumn
    {
        string Name { get; }

        bool IsPrimaryKey { get; }

        bool IsNullable { get; }

        string DataType { get; }

        bool IsReadOnly { get; }

        byte Precision { get; }

        byte Scale { get; }

        int? MaxLength { get; }
        string TableName { get; }
        string ReferencedColumn { get; }
        string ReferencedTable { get; }
    }
}