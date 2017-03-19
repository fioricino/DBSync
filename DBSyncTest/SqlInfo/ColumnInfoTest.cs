using System;
using DBSync;
using DBSync.SqlInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DBSyncTest
{
    [TestClass]
    public class ColumnInfoTest
    {
        private ColumnInfo columnInfo;

        [TestInitialize]
        public void Init()
        {
            columnInfo = new ColumnInfo(new TableInfo())
            {
                Name = "Column"
            };
        }

        [TestMethod]
        public void TestIsGuid()
        {
            columnInfo.DataType = "uniqueidentifier";
            Assert.IsTrue(columnInfo.IsGuid);
        }

        [TestMethod]
        public void TestNoGuid()
        {
            columnInfo.DataType = "bigint";
            Assert.IsFalse(columnInfo.IsGuid);
        }

        [TestMethod]
        public void TestQuotedName()
        {
            Assert.AreEqual($"[{columnInfo.Name}]", columnInfo.QuotedName);
        }

        [TestMethod]
        public void TestToSqlStringNullableNoModifiers()
        {
            columnInfo.IsNullable = true;
            columnInfo.DataType = "bigint";

            Assert.AreEqual($"[{columnInfo.Name}] {columnInfo.DataType}  NULL",
                columnInfo.ToSqlString());
        }

        [TestMethod]
        public void TestToSqlStringNotNullableDecimal()
        {
            columnInfo.IsNullable = false;
            columnInfo.DataType = "decimal";
            columnInfo.Scale = 2;
            columnInfo.Precision = 10;

            Assert.AreEqual($"[{columnInfo.Name}] {columnInfo.DataType} (10,2) NOT NULL",
                columnInfo.ToSqlString());
        }

        [TestMethod]
        public void TestToSqlStringNotNullableVarchar()
        {
            columnInfo.IsNullable = false;
            columnInfo.DataType = "varchar";
            columnInfo.MaxLength = -1;

            Assert.AreEqual($"[{columnInfo.Name}] {columnInfo.DataType} (max) NOT NULL",
                columnInfo.ToSqlString());
        }
    }
}
