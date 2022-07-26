using System;
using System.Linq;
using NUnit.Framework;
using TauCode.Db.Exceptions;
using TauCode.Db.Model;
using TauCode.Extensions;

namespace TauCode.Db.SQLite.Tests.DbTableInspector
{
    [TestFixture]
    public class SQLiteTableInspectorTests : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            var sql = this.GetType().Assembly.GetResourceText("crebase.sql", true);
            this.Connection.ExecuteCommentedScript(sql);
        }

        private void AssertColumn(
            ColumnMold actualColumnMold,
            string expectedColumnName,
            DbTypeMoldInfo expectedType,
            bool expectedIsNullable,
            ColumnIdentityMoldInfo expectedIdentity,
            string expectedDefault)
        {
            Assert.That(actualColumnMold.Name, Is.EqualTo(expectedColumnName));

            Assert.That(actualColumnMold.Type.Name, Is.EqualTo(expectedType.Name));
            Assert.That(actualColumnMold.Type.Size, Is.EqualTo(expectedType.Size));
            Assert.That(actualColumnMold.Type.Precision, Is.EqualTo(expectedType.Precision));
            Assert.That(actualColumnMold.Type.Scale, Is.EqualTo(expectedType.Scale));

            Assert.That(actualColumnMold.IsNullable, Is.EqualTo(expectedIsNullable));

            if (actualColumnMold.Identity == null)
            {
                Assert.That(expectedIdentity, Is.Null);
            }
            else
            {
                Assert.That(expectedIdentity, Is.Not.Null);
                Assert.That(actualColumnMold.Identity.Seed, Is.EqualTo(expectedIdentity.Seed));
                Assert.That(actualColumnMold.Identity.Increment, Is.EqualTo(expectedIdentity.Increment));
            }

            Assert.That(actualColumnMold.Default, Is.EqualTo(expectedDefault));
        }

        #region Constructor

        [Test]
        public void Constructor_ValidArguments_RunsOk()
        {
            // Arrange

            // Act
            IDbTableInspector inspector = new SQLiteTableInspector(this.Connection, "tab1");

            // Assert
            Assert.That(inspector.Connection, Is.SameAs(this.Connection));
            Assert.That(inspector.Factory, Is.SameAs(SQLiteUtilityFactory.Instance));

            Assert.That(inspector.SchemaName, Is.EqualTo(null));
            Assert.That(inspector.TableName, Is.EqualTo("tab1"));
        }

        [Test]
        public void Constructor_ConnectionIsNull_ThrowsArgumentNullException()
        {
            // Arrange

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => new SQLiteTableInspector(null, "tab1"));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("connection"));
        }

        [Test]
        public void Constructor_ConnectionIsNotOpen_ThrowsArgumentException()
        {
            // Arrange
            using var connection = TestHelper.CreateConnection(false, false);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => new SQLiteTableInspector(connection, "tab1"));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("Connection should be opened."));
            Assert.That(ex.ParamName, Is.EqualTo("connection"));
        }
        
        [Test]
        public void Constructor_TableNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => new SQLiteTableInspector(this.Connection, null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        #endregion

        #region GetColumns

        [Test]
        public void GetColumns_ValidInput_ThrowsNotSupportedException()
        {
            // Arrange
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "Person");

            // Act
            var ex = Assert.Throws<NotSupportedException>(() => tableInspector.GetColumns());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Use method 'GetTable'."));
        }

        [Test]
        public void GetColumns_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "bad_table");

            // Act
            var ex = Assert.Throws<NotSupportedException>(() => tableInspector.GetColumns());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Use method 'GetTable'."));
        }

        #endregion

        #region GetPrimaryKey

        [Test]
        public void GetPrimaryKey_ValidInput_ThrowsNotSupportedException()
        {
            // Arrange
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "Person");

            // Act
            var ex = Assert.Throws<NotSupportedException>(() => tableInspector.GetPrimaryKey());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Use method 'GetTable'."));
        }

        [Test]
        public void GetPrimaryKey_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbTableInspector inspector = new SQLiteTableInspector(this.Connection, "bad_table");

            // Act
            var ex = Assert.Throws<NotSupportedException>(() => inspector.GetPrimaryKey());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Use method 'GetTable'."));
        }

        #endregion

        #region GetForeignKeys

        [Test]
        public void GetForeignKeys_ValidInput_ThrowsNotSupportedException()
        {
            // Arrange
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");

            // Act
            var ex = Assert.Throws<NotSupportedException>(() => tableInspector.GetForeignKeys());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Use method 'GetTable'."));
        }

        [Test]
        public void GetForeignKeys_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbTableInspector inspector = new SQLiteTableInspector(this.Connection, "bad_table");

            // Act
            var ex = Assert.Throws<NotSupportedException>(() => inspector.GetForeignKeys());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Use method 'GetTable'."));
        }

        #endregion

        #region GetIndexes

        [Test]
        public void GetIndexes_ValidInput_ReturnsIndexes()
        {
            // Arrange
            IDbTableInspector inspector1 = new SQLiteTableInspector(this.Connection, "Person");
            IDbTableInspector inspector2 = new SQLiteTableInspector(this.Connection, "WorkInfo");
            IDbTableInspector inspector3 = new SQLiteTableInspector(this.Connection, "HealthInfo");

            // Act
            var indexes1 = inspector1.GetIndexes();
            var indexes2 = inspector2.GetIndexes();
            var indexes3 = inspector3.GetIndexes();

            // Assert

            // Person
            Assert.That(indexes1, Is.Empty);

            // WorkInfo
            Assert.That(indexes2, Has.Count.EqualTo(1));

            // index: UX_workInfo_Hash
            var index = indexes2[0];
            Assert.That(index.Name, Is.EqualTo("UX_workInfo_Hash"));
            Assert.That(index.TableName, Is.EqualTo("WorkInfo"));
            Assert.That(index.IsUnique, Is.True);
            Assert.That(index.Columns, Has.Count.EqualTo(1));

            var column = index.Columns.Single();
            Assert.That(column.Name, Is.EqualTo("Hash"));
            Assert.That(column.SortDirection, Is.EqualTo(SortDirection.Ascending));

            // HealthInfo
            Assert.That(indexes3, Has.Count.EqualTo(1));

            // index: IX_healthInfo_metricAmetricB
            index = indexes3[0];
            Assert.That(index.Name, Is.EqualTo("IX_healthInfo_metricAmetricB"));
            Assert.That(index.TableName, Is.EqualTo("HealthInfo"));
            Assert.That(index.IsUnique, Is.False);
            Assert.That(index.Columns, Has.Count.EqualTo(2));

            column = index.Columns[0];
            Assert.That(column.Name, Is.EqualTo("MetricA"));
            Assert.That(column.SortDirection, Is.EqualTo(SortDirection.Ascending));

            column = index.Columns[1];
            Assert.That(column.Name, Is.EqualTo("MetricB"));
            Assert.That(column.SortDirection, Is.EqualTo(SortDirection.Descending));
        }

        [Test]
        public void GetIndexes_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbTableInspector inspector = new SQLiteTableInspector(this.Connection, "bad_table");

            // Act
            var ex = Assert.Throws<NotSupportedException>(() => inspector.GetForeignKeys());

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Use method 'GetTable'."));
        }

        #endregion

        #region GetTable

        [Test]
        public void GetTable_ValidInput_ReturnsTable()
        {
            // Arrange
            IDbTableInspector inspector = new SQLiteTableInspector(this.Connection, "HealthInfo");

            // Act
            var table = inspector.GetTable();

            // Assert

            Assert.That(table.Name, Is.EqualTo("HealthInfo"));

            #region primary keys

            var primaryKey = table.PrimaryKey;

            Assert.That(primaryKey.Name, Is.EqualTo("PK_healthInfo"));

            Assert.That(primaryKey.Columns, Has.Count.EqualTo(1));

            var column = primaryKey.Columns.Single();
            Assert.That(column, Is.EqualTo("Id"));

            #endregion

            #region columns

            var columns = table.Columns;
            Assert.That(columns, Has.Count.EqualTo(9));

            this.AssertColumn(columns[0], "Id", new DbTypeMoldInfo("uniqueidentifier"), false, null, null);
            this.AssertColumn(columns[1], "PersonId", new DbTypeMoldInfo("integer"), false, null, null);
            this.AssertColumn(
                columns[2],
                "Weight",
                new DbTypeMoldInfo("numeric"),
                false,
                null,
                null);
            this.AssertColumn(columns[3], "PersonMetaKey", new DbTypeMoldInfo("integer"), false, null, null);
            this.AssertColumn(columns[4], "IQ", new DbTypeMoldInfo("numeric"), true, null, null);
            this.AssertColumn(columns[5], "Temper", new DbTypeMoldInfo("integer"), true, null, null);
            this.AssertColumn(columns[6], "PersonOrdNumber", new DbTypeMoldInfo("integer"), false, null, null);
            this.AssertColumn(columns[7], "MetricB", new DbTypeMoldInfo("integer"), true, null, null);
            this.AssertColumn(columns[8], "MetricA", new DbTypeMoldInfo("integer"), true, null, null);

            #endregion

            #region foreign keys

            var foreignKeys = table.ForeignKeys;

            Assert.That(foreignKeys, Has.Count.EqualTo(1));
            var fk = foreignKeys.Single();

            Assert.That(fk.Name, Is.EqualTo("FK_healthInfo_Person"));
            CollectionAssert.AreEqual(
                new string[]
                {
                    "PersonId",
                    "PersonMetaKey",
                    "PersonOrdNumber",
                },
                fk.ColumnNames);

            Assert.That(fk.ReferencedTableName, Is.EqualTo("Person"));
            CollectionAssert.AreEqual(
                new string[]
                {
                    "Id",
                    "MetaKey",
                    "OrdNumber",
                },
                fk.ReferencedColumnNames);

            #endregion

            #region indexes

            var indexes = table.Indexes;

            Assert.That(indexes, Has.Count.EqualTo(1));

            // index: IX_healthInfo_metricAmetricB
            var index = indexes[0];
            Assert.That(index.Name, Is.EqualTo("IX_healthInfo_metricAmetricB"));
            Assert.That(index.TableName, Is.EqualTo("HealthInfo"));
            Assert.That(index.IsUnique, Is.False);
            Assert.That(index.Columns, Has.Count.EqualTo(2));

            var indexColumn = index.Columns[0];
            Assert.That(indexColumn.Name, Is.EqualTo("MetricA"));
            Assert.That(indexColumn.SortDirection, Is.EqualTo(SortDirection.Ascending));

            indexColumn = index.Columns[1];
            Assert.That(indexColumn.Name, Is.EqualTo("MetricB"));
            Assert.That(indexColumn.SortDirection, Is.EqualTo(SortDirection.Descending));

            #endregion
        }

        [Test]
        public void GetTable_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbTableInspector inspector = new SQLiteTableInspector(this.Connection, "bad_table");

            // Act
            var ex = Assert.Throws<TauDbException>(() => inspector.GetTable());

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Table 'bad_table' does not exist."));
        }

        #endregion
    }
}
