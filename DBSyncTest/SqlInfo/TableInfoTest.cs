using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBSync;
using DBSync.SqlInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBSyncTest.SqlInfo
{
    [TestClass]
    class TableInfoTest
    {
        private TableInfo tableInfo;

        [TestInitialize]
        public void Init()
        {
            tableInfo = new TableInfo()
            {
                Name = "Table"
            };
        }

        [TestMethod]
        public void TestNameForTempTable()
        {
            Assert.AreEqual($"@{tableInfo.Name}_FOR_DELETE", tableInfo.NameForTempTable);
        }

        [TestMethod]
        public void TestParentInfos()
        {
            var parent1 = new TableInfo() {Name = "Parent1"};
            var parent2 = new TableInfo() {Name = "Parent2"};
            tableInfo.Columns = new List<ColumnInfo>()
            {
                new ColumnInfo(tableInfo),
                new ColumnInfo(tableInfo)
                {
                    ReferencedColumn = new ColumnInfo(parent1)
                },
                  new ColumnInfo(tableInfo)
                {
                    ReferencedColumn = new ColumnInfo(parent2)
                }
            };

            Assert.AreEqual(2, tableInfo.ParentInfos.Count());
            Assert.IsTrue(tableInfo.ParentInfos.Contains(parent1));
            Assert.IsTrue(tableInfo.ParentInfos.Contains(parent2));
        }

        [TestMethod]
        public void TestFKColumns()
        {
            var fkColumn = new ColumnInfo(tableInfo)
            {
                ReferencedColumn = new ColumnInfo(new TableInfo())
            };
            tableInfo.Columns = new List<ColumnInfo>()
            {
                new ColumnInfo(tableInfo),
                fkColumn
            };
            Assert.AreEqual(1, tableInfo.FKColumns.Count());
            Assert.IsTrue(tableInfo.FKColumns.Contains(fkColumn));
        }

        [TestMethod]
        public void TestIsCoreData()
        {
            tableInfo.Scope = new ScopeInfo() {ScopeType = ScopeType.Core};
            Assert.IsTrue(tableInfo.IsCoreData);
        }

        [TestMethod]
        public void TestNoCoreData()
        {
            tableInfo.Scope = new ScopeInfo() { ScopeType = ScopeType.Order };
            Assert.IsFalse(tableInfo.IsCoreData);
        }

        [TestMethod]
        public void TestIsGuid()
        {
            tableInfo.Columns = new List<ColumnInfo>()
            {
                new ColumnInfo(tableInfo) {DataType = "bigint"},
                new ColumnInfo(tableInfo) {DataType = "uniqueidentifier"}
            };
            Assert.IsTrue(tableInfo.IsGuid);
        }

        [TestMethod]
        public void TestNoGuid()
        {
            tableInfo.Columns = new List<ColumnInfo>()
            {
                new ColumnInfo(tableInfo) {DataType = "bigint"},
                new ColumnInfo(tableInfo) {DataType = "varchar"}
            };
            Assert.IsFalse(tableInfo.IsGuid);
        }

        [TestMethod]
        public void TestPKColumns()
        {
            var pk1 = new ColumnInfo(tableInfo) {IsPk = true};
            var pk2 = new ColumnInfo(tableInfo) {IsPk = true};
            tableInfo.Columns = new List<ColumnInfo>()
            {
                new ColumnInfo(tableInfo) {IsPk = false},
                pk1,
                pk2
            };

            Assert.AreEqual(2, tableInfo.PKColumns.Count());
            Assert.IsTrue(tableInfo.PKColumns.Contains(pk1));
            Assert.IsTrue(tableInfo.PKColumns.Contains(pk2));
        }

        [TestMethod]
        public void TestNotPKColumns()
        {
            var pk = new ColumnInfo(tableInfo) { IsPk = true };
            var sync = new ColumnInfo(tableInfo);
            var nonSync = new ColumnInfo(tableInfo) {IsReadOnly = true};
            tableInfo.Columns = new List<ColumnInfo>()
            {
               pk, sync, nonSync
            };

            Assert.AreEqual(1, tableInfo.NotPKColumns.Count());
            Assert.IsTrue(tableInfo.NotPKColumns.Contains(sync));
        }

        [TestMethod]
        public void TestSyncColumns()
        {
            var pk = new ColumnInfo(tableInfo) { IsPk = true };
            var sync = new ColumnInfo(tableInfo);
            var nonSync = new ColumnInfo(tableInfo) { IsReadOnly = true };
            tableInfo.Columns = new List<ColumnInfo>()
            {
               pk, sync, nonSync
            };

            Assert.AreEqual(2, tableInfo.SyncColumns.Count());
            Assert.IsTrue(tableInfo.SyncColumns.Contains(sync));
            Assert.IsTrue(tableInfo.SyncColumns.Contains(pk));
        }

        [TestMethod]
        public void TestPossibleWrongKeyGuid()
        {
            tableInfo.Scope = new ScopeInfo() {ScopeType = ScopeType.Core};
            tableInfo.Columns.Add(new ColumnInfo(tableInfo) {IsPk = true, DataType = "uniqueidentifier"});
            Assert.IsTrue(tableInfo.PossibleWrongKey);
        }

        [TestMethod]
        public void TestNoWrongKeyGuid()
        {
            tableInfo.Scope = new ScopeInfo() { ScopeType = ScopeType.Order };
            tableInfo.Columns.Add(new ColumnInfo(tableInfo) { IsPk = true, DataType = "uniqueidentifier" });
            Assert.IsTrue(tableInfo.PossibleWrongKey);
        }

        [TestMethod]
        public void TestNoWrongKeyNoGuid()
        {
            tableInfo.Scope = new ScopeInfo() { ScopeType = ScopeType.Core };
            tableInfo.Columns.Add(new ColumnInfo(tableInfo) { IsPk = true, DataType = "bigint" });
            Assert.IsTrue(tableInfo.PossibleWrongKey);
        }

        [TestMethod]
        public void TestPossibleWrongConflictResolution()
        {
            tableInfo.ConflictResolutionPolicy = SyncConflictResolutionPolicy.LastChangeDate;
            tableInfo.Columns.Add(new ColumnInfo(tableInfo) {Name = "ID"});
            Assert.IsTrue(tableInfo.PossibleWrongConflictResolution);
        }

        [TestMethod]
        public void TestNoWrongConflictResolution()
        {
            tableInfo.ConflictResolutionPolicy = SyncConflictResolutionPolicy.LastChangeDate;
            tableInfo.Columns.Add(new ColumnInfo(tableInfo) { Name = "ID" });
            tableInfo.Columns.Add(new ColumnInfo(tableInfo) { Name = "CHANGE_DATE" });
            Assert.IsTrue(tableInfo.PossibleWrongConflictResolution);
        }

        [TestMethod]
        public void TestSetRelations()
        {
            tableInfo.FilterColumns.Add(new FilterColumnInfo());
            tableInfo.FilterColumns.Add(new FilterColumnInfo());
            tableInfo.Aliases.Add(new AliasInfo() {FilterColumns = new List<FilterColumnInfo>() {new FilterColumnInfo()} });
            tableInfo.SetRelations();

            foreach (var filterColumnInfo in tableInfo.FilterColumns)
            {
                Assert.AreEqual(tableInfo, filterColumnInfo.Table);
            }

            Assert.AreEqual(2, tableInfo.Aliases.Count);
            Assert.IsTrue(tableInfo.Aliases.Any(a => a.IsRoot && a.FilterColumns == tableInfo.FilterColumns));
            foreach (var aliasInfo in tableInfo.Aliases)
            {
                Assert.AreEqual(tableInfo, aliasInfo.Table);
                foreach (var filterColumnInfo in aliasInfo.FilterColumns)
                {
                    Assert.AreEqual(tableInfo, filterColumnInfo.Table);
                }
            }
        }
    }
}
