using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Resources;
using DBSyncOld.DataSets;

namespace DBSyncOld
{
    public static class SyncMetaDataHelper
    {
        public const string SelectMetaDataFileTemplate = "{0}SelectMetaData.sql";
        public const string DeleteMetaDataFileTemplate = "{0}DeleteMetaData.sql";
        public const string DataSetPostfix = "DS";

        private static string GetResource(string resourceName)
        {
            var assembly = typeof(DBAnalyzer).Assembly;
            var ns = typeof(DBAnalyzer).Namespace;
            var fullName = String.Format("{0}.{1}", ns, resourceName); 

            using (var stream = assembly.GetManifestResourceStream(fullName))
            {
                if (stream == null)
                {
                    throw new MissingManifestResourceException(fullName);
                }
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static string GetDataSetName(ScopeType scope)
        {
            return String.Concat(scope, DataSetPostfix);
        }

        public static SyncDataContainer CreaSyncDataContainerForScope(ScopeType scope, params string[] queryParam)
        {
            DataSet dataSet = GetEmptyDataSetForScope(scope);

            string selectMetaDataTemplate = GetSqlMetaDataForScope(SelectMetaDataFileTemplate, scope, queryParam);

            return  new SyncDataContainer(dataSet, selectMetaDataTemplate);
        }

        public static string GetDeleteMetaDataForScope(ScopeType scope, params string[] queryParam)
        {
            return GetSqlMetaDataForScope(DeleteMetaDataFileTemplate, scope, queryParam);
        }

        public static SyncDataDto ToSyncContainer(this SyncDataContainer dataContainer)
        {
            var recordNumber = dataContainer.SyncDataSet.Tables.Cast<DataTable>().Sum(t => t.Rows.Count);

            var data = DatasetSerializer.SerializeDataSet(dataContainer.SyncDataSet);

            return new SyncDataDto(data, recordNumber);
        }

        static DataSet GetEmptyDataSetForScope(ScopeType scope)
        {
            switch (scope)
            {
                case ScopeType.Expenses:
                    return new ExpensesDS();
                case ScopeType.Order:
                    return new OrderDS();
                case ScopeType.Cash:
                    return new CashDS();
                case ScopeType.Protocol:
                    return new ProtocolDS();
                case ScopeType.Login:
                    return new LoginDS();
                case ScopeType.Core:
                    return new CoreDS();
                case ScopeType.ExpensesAll:
                    return new ExpensesAllDS();
                default:
                    throw new ArgumentOutOfRangeException("scope");
            }
        }

        static string GetSqlMetaDataForScope(string flieNameTemplate, ScopeType scope, params string[] queryParam)
        {

            var name = String.Format(flieNameTemplate, scope);

            var selectStatementTemplate =  GetResource(name);
            
            string selectStatement = queryParam.Length > 0 ? string.Format(selectStatementTemplate, queryParam) : selectStatementTemplate;

            return selectStatement;
        }
      
        public static string GetSPName(this DataTable dataTable)
        {
            return GetSPName(dataTable.TableName);
        }

        public static string GetSPName(string tableName)
        {
            return String.Format("SYNC_MERGE_{0}", tableName);
        }

        public static string GetTVPName(this DataTable dataTable)
        {
            return GetTVPName(dataTable.TableName);
        }

        public static string GetTVPName(string tableName)
        {
            return String.Format("TVP_{0}", tableName);
        }

    }
}
