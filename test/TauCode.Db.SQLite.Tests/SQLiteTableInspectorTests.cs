using System.Linq;
using NUnit.Framework;
using TauCode.Db.Exceptions;

namespace TauCode.Db.SQLite.Tests
{
    [TestFixture]
    public class SQLiteTableInspectorTests : TestBase
    {
        private IDbTableInspector _tableInspector;

        [SetUp]
        public void SetUp()
        {
            _tableInspector = new SQLiteTableInspector(this.Connection, "foo");
        }

        [Test]
        public void GetTable_ValidInput_ProducesExpectedResult()
        {
            // Arrange

            // Act
            var table = _tableInspector.GetTable();

            // Assert
            var column = table.Columns.Single(x => x.Name == "my_string");
            Assert.That(column.Type.Name, Is.EqualTo("TEXT"));

            column = table.Columns.Single(x => x.Name == "my_ansi_string");
            Assert.That(column.Type.Name, Is.EqualTo("TEXT"));

            column = table.Columns.Single(x => x.Name == "my_binary");
            Assert.That(column.Type.Name, Is.EqualTo("BLOB"));

            column = table.Columns.Single(x => x.Name == "my_bool");
            Assert.That(column.Type.Name, Is.EqualTo("INTEGER"));

            column = table.Columns.Single(x => x.Name == "my_byte");
            Assert.That(column.Type.Name, Is.EqualTo("INTEGER"));

            column = table.Columns.Single(x => x.Name == "my_currency");
            Assert.That(column.Type.Name, Is.EqualTo("NUMERIC"));

            column = table.Columns.Single(x => x.Name == "my_datetime");
            Assert.That(column.Type.Name, Is.EqualTo("DATETIME"));

            column = table.Columns.Single(x => x.Name == "my_decimal");
            Assert.That(column.Type.Name, Is.EqualTo("NUMERIC"));

            column = table.Columns.Single(x => x.Name == "my_double");
            Assert.That(column.Type.Name, Is.EqualTo("NUMERIC"));

            column = table.Columns.Single(x => x.Name == "my_fixed_string");
            Assert.That(column.Type.Name, Is.EqualTo("TEXT"));

            column = table.Columns.Single(x => x.Name == "my_fixed_ansi_string");
            Assert.That(column.Type.Name, Is.EqualTo("TEXT"));

            column = table.Columns.Single(x => x.Name == "my_float");
            Assert.That(column.Type.Name, Is.EqualTo("NUMERIC"));

            column = table.Columns.Single(x => x.Name == "my_int16");
            Assert.That(column.Type.Name, Is.EqualTo("INTEGER"));

            column = table.Columns.Single(x => x.Name == "my_int32");
            Assert.That(column.Type.Name, Is.EqualTo("INTEGER"));

            column = table.Columns.Single(x => x.Name == "my_int64");
            Assert.That(column.Type.Name, Is.EqualTo("INTEGER"));
        }

        [Test]
        public void GetTable_NonExistingTable_ThrowsDbException()
        {
            // Arrange

            // Act
            var ex = Assert.Throws<TauDbException>(() =>
            {
                var tableInspector = this.DbInspector.Factory.CreateTableInspector(this.Connection, null, "non_existing_table");
                tableInspector.GetTable();
            });

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Table 'non_existing_table' not found."));
        }

        protected override void ExecuteDbCreationScript()
        {
            var migrator = new TestMigrator(this.ConnectionString, this.GetType().Assembly);
            migrator.Migrate();
        }
    }
}
