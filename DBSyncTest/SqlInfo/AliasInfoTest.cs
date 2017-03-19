using System;
using DBSync;
using DBSync.SqlInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBSyncTest
{
    [TestClass]
    public class AliasInfoTest
    {
        private AliasInfo aliasInfo;

        [TestInitialize]
        public void Init()
        {
            aliasInfo = new AliasInfo()
            {
                Table = new TableInfo()
                {
                    Name = "Table"
                }
            };

        }

        [TestMethod]
        public void TestNameOrAliasNoAlias()
        {
            Assert.AreEqual(aliasInfo.Table.Name, aliasInfo.NameOrAlias);
          
        }

        [TestMethod]
        public void TestNameOrAliasHasAlias()
        {
            aliasInfo.Name = "Alias";
            Assert.AreEqual(aliasInfo.Name, aliasInfo.NameOrAlias);
        }

        [TestMethod]
        public void TestNameWithAliasNoAlias()
        {
            Assert.AreEqual(aliasInfo.Table.Name, aliasInfo.NameWithAlias);
        }

        [TestMethod]
        public void TestNameWithAliasHasAlias()
        {
            aliasInfo.Name = "Alias";
            Assert.AreEqual(String.Format("{0} AS {1}", aliasInfo.Table.Name, aliasInfo.Name), aliasInfo.NameWithAlias);
        }

        [TestMethod]
        public void TestSetRelations()
        {
            for (int i = 0; i < 5; i++)
            {
                aliasInfo.FilterColumns.Add(new FilterColumnInfo());
            }

            aliasInfo.SetRelations();
            foreach (var filterColumnInfo in aliasInfo.FilterColumns)
            {
                Assert.AreEqual(aliasInfo, filterColumnInfo.Table);
            }
        }
    }
}
