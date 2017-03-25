using System;
using System.Collections.Generic;
using DBSyncOld;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBSyncTest
{
    [TestClass]
    public class UnitTest1
    {
        private static DBSyncOld.DBAnalyzer dbAnalyzerOld;
        private static DBSyncNew.DBAnalyzer dbAnalyzerNew;

        [ClassInitialize]
        public static void InitClass(TestContext context)
        {
            dbAnalyzerOld = new DBSyncOld.DBAnalyzer("", "", "", "", "Resources/scopes.xml", "Server=(local); Initial catalog=TUEV_SUED; Integrated Security=true");
            dbAnalyzerNew = new DBSyncNew.DBAnalyzer("", "", "", "", "Resources/scopes.xml", "Server=(local); Initial catalog=TUEV_SUED; Integrated Security=true");
        }

  
        [TestMethod]
        public void TestGenerateDataSetMetaData()
        {
            foreach (DBSyncOld.ScopeType scopeType in Enum.GetValues(typeof (DBSyncOld.ScopeType)))
            {
                if (scopeType == ScopeType.Historical) continue;
                
                string dataSetMetadataOld = dbAnalyzerOld.GenerateDataSetMetaData(scopeType);
                var scopeTypeNew = (DBSyncNew.ScopeType) Enum.Parse(typeof (DBSyncNew.ScopeType), scopeType.ToString());
                string dataSetMetadataNew = dbAnalyzerNew.GenerateDataSetMetaData(scopeTypeNew);

                Assert.AreEqual(dataSetMetadataOld, dataSetMetadataNew);
            }
        }

        [TestMethod]
        public void TestGenerateDataSetSelectMetaData()
        {
            foreach (DBSyncOld.ScopeType scopeType in Enum.GetValues(typeof(DBSyncOld.ScopeType)))
            {
                if (scopeType == ScopeType.Historical) continue;

                Tuple<string, string> dataSetMetadataOld = dbAnalyzerOld.GenerateDataSetSelectMetaData(scopeType);
                var scopeTypeNew = (DBSyncNew.ScopeType)Enum.Parse(typeof(DBSyncNew.ScopeType), scopeType.ToString());
                Tuple<string, string> dataSetMetadataNew = dbAnalyzerNew.GenerateDataSetSelectMetaData(scopeTypeNew);

                Assert.AreEqual(dataSetMetadataOld, dataSetMetadataNew);
            }
        }

        [TestMethod]
        public void TestGenerateInsertMetaData()
        {
            foreach (DBSyncOld.ScopeType scopeType in Enum.GetValues(typeof(DBSyncOld.ScopeType)))
            {
                if (scopeType == ScopeType.Historical) continue;

                string dataSetMetadataOld = dbAnalyzerOld.GenerateInsertMetaData(scopeType);
                var scopeTypeNew = (DBSyncNew.ScopeType)Enum.Parse(typeof(DBSyncNew.ScopeType), scopeType.ToString());
                string dataSetMetadataNew = dbAnalyzerNew.GenerateInsertMetaData(scopeTypeNew);

                Assert.AreEqual(dataSetMetadataOld, dataSetMetadataNew);
            }
        }

        [TestMethod]
        public void TestGenerateSqlMetaData()
        {
                string dataSetMetadataOld = dbAnalyzerOld.GenerateSqlMetaData();
                string dataSetMetadataNew = dbAnalyzerNew.GenerateSqlMetaData();

                Assert.AreEqual(dataSetMetadataOld, dataSetMetadataNew);
        }
    }
}
