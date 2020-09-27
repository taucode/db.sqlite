using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using TauCode.Db.Data;
using TauCode.Db.DbValueConverters;
using TauCode.Db.Exceptions;

namespace TauCode.Db.SQLite.Tests
{
    [TestFixture]
    public class SQLiteCruderTests : TestBase
    {
        private class WrongData
        {
        }

        public enum Town : sbyte
        {
            Kiev = 1,
            Tropea = 2,
        }

        public enum UserRole
        {
            Admin = 1,
            Developer
        }


        private IDbCruder _cruder;

        [SetUp]
        public void SetUp()
        {
            _cruder = this.DbInspector.Factory.CreateCruder(this.Connection, null);
        }

        [Test]
        public void InsertRow_ValidRow_Inserts()
        {
            // Arrange
            var id = new Guid("bf7f7507-4b08-48f0-9bbe-d061acdbca31");

            var language = new
            {
                id,
                code = "it",
                name = "Italian",
            };

            // Act
            _cruder.InsertRow("language", language);

            // Assert
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText = @"SELECT [id], [code], [name] FROM [language] WHERE [id] = @p_id";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "p_id";
                parameter.Value = id.ToString();
                command.Parameters.Add(parameter);
                var row = DbTools.GetCommandRows(command).Single();

                Assert.That(row.id, Is.EqualTo(id));
                Assert.That(row.code, Is.EqualTo("it"));
                Assert.That(row.name, Is.EqualTo("Italian"));
            }
        }

        [Test]
        public void InsertRow_RowWithEnums_Inserts()
        {
            // Arrange
            var hoo = new DynamicRow(new
            {
                id = 1,
                name = "Vasya",
                enum_int32 = (ushort)1,
                enum_string = UserRole.Admin,
            });

            // Act
            _cruder.GetTableValuesConverter("hoo").SetColumnConverter(
                "enum_int32",
                new EnumValueConverter<Town>(DbType.Int64));

            _cruder.GetTableValuesConverter("hoo").SetColumnConverter(
                "enum_string",
                new EnumValueConverter<UserRole>(DbType.String));

            _cruder.InsertRow("hoo", hoo);

            // Assert
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText = @"SELECT [id], [name], [enum_int32], [enum_string] FROM [hoo] WHERE [id] = 1";

                var row = DbTools.GetCommandRows(command).Single();

                Assert.That(row.id, Is.EqualTo(1));
                Assert.That(row.name, Is.EqualTo("Vasya"));
                Assert.That(row.enum_int32, Is.EqualTo(1));
                Assert.That(row.enum_string, Is.EqualTo(UserRole.Admin.ToString()));
            }
        }


        [Test]
        public void InsertRow_WrongColumnValue_ThrowsTauCodeDbException()
        {
            // Arrange
            var id = new Guid("bf7f7507-4b08-48f0-9bbe-d061acdbca31");

            var language = new
            {
                id,
                code = new WrongData(),
                name = "Italian",
            };

            // Act
            var ex = Assert.Throws<TauDbException>(() => _cruder.InsertRow("language", language));

            // Assert
            Assert.That(ex.Message, Does.StartWith("Could not transform value"));
        }

        [Test]
        public void InsertRow_NonExistingColumnNames_ThrowsTauCodeDbException()
        {
            // Arrange
            var id = new Guid("bf7f7507-4b08-48f0-9bbe-d061acdbca31");

            var language = new
            {
                id,
                wrong_column_name = "it",
                name = "Italian",
            };

            // Act
            var ex = Assert.Throws<TauDbException>(() => _cruder.InsertRow("language", language));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Column not found: 'wrong_column_name'."));
        }

        [Test]
        public void UpdateRow_ValidData_UpdatesAndReturnsTrue()
        {
            // Arrange
            var id = new Guid("bf7f7507-4b08-48f0-9bbe-d061acdbca31");

            var language = new
            {
                id,
                code = "it",
                name = "Italian",
            };

            _cruder.InsertRow("language", language);

            // Act
            var updated = _cruder.UpdateRow(
                "language",
                new
                {
                    name = "Duzhe Italian!"
                },
                id);

            // Assert
            Assert.That(updated, Is.True);

            var row = this.GetRow("language", id.ToString());
            Assert.That(row.id, Is.EqualTo(id));
            Assert.That(row.code, Is.EqualTo("it"));
            Assert.That(row.name, Is.EqualTo("Duzhe Italian!"));
        }

        [Test]
        public void UpdateRow_NonExistingId_DoesNothingAndReturnsFalse()
        {
            // Arrange
            var id = TestHelper.NonExistingGuid;

            // Act
            var updated = _cruder.UpdateRow("language", new { name = "Duzhe Italian!" }, id);

            // Assert
            Assert.That(updated, Is.False);
        }

        [Test]
        public void UpdateRow_NonExistingColumnNames_ThrowsTauCodeDbException()
        {
            // Arrange
            var id = new Guid("bf7f7507-4b08-48f0-9bbe-d061acdbca31");

            var language = new
            {
                id,
                code = "it",
                name = "Italian",
            };

            _cruder.InsertRow("language", language);

            // Act
            var ex = Assert.Throws<TauDbException>(() => _cruder.UpdateRow(
                "language",
                new
                {
                    wrong_name = "Duzhe Italian!"
                },
                id));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Column not found: 'wrong_name'."));
        }

        [Test]
        public void UpdateRow_PrimaryKeyColumnIncluded_ThrowsTauCodeDbException()
        {
            // Arrange
            var id = new Guid("bf7f7507-4b08-48f0-9bbe-d061acdbca31");

            var language = new
            {
                id,
                code = "it",
                name = "Italian",
            };

            _cruder.InsertRow("language", language);

            // Act
            var ex = Assert.Throws<TauDbException>(() => _cruder.UpdateRow(
                "language",
                new
                {
                    id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    name = "Duzhe Italian!"
                },
                id));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Update object must not contain ID column."));
        }

        [Test]
        public void UpdateRow_WrongColumnValue_ThrowsTauCodeDbException()
        {
            // Arrange
            var id = new Guid("bf7f7507-4b08-48f0-9bbe-d061acdbca31");

            var language = new
            {
                id,
                code = "it",
                name = "Italian",
            };

            _cruder.InsertRow("language", language);

            // Act
            var ex = Assert.Throws<TauDbException>(() => _cruder.UpdateRow(
                "language",
                new
                {
                    name = new WrongData(),
                },
                id));

            // Assert
            Assert.That(ex.Message, Does.StartWith("Could not transform value"));
        }

        [Test]
        public void GetRow_ValidId_ReturnsRow()
        {
            // Arrange
            this.Connection.ExecuteSingleSql(@"
INSERT INTO [language](
    [id],
    [code],
    [name])
VALUES(
    '4fca7968-49cf-4c52-b793-e4b27087251b',
    'en',
    'English')");

            var id = new Guid("4fca7968-49cf-4c52-b793-e4b27087251b");

            // Act
            var row = _cruder.GetRow("language", id);

            // Assert
            Assert.That(row.id, Is.EqualTo(id));
            Assert.That(row.code, Is.EqualTo("en"));
            Assert.That(row.name, Is.EqualTo("English"));
        }

        [Test]
        public void GetRow_ColumnValueIsNull_ReturnsRowWithNull()
        {
            // Arrange
            this.Connection.ExecuteSingleSql(@"
INSERT INTO [hoo](
    [id],
    [name],
    [enum_string])
VALUES(
    11,    
    null,
    'Developer')");

            _cruder.GetTableValuesConverter("hoo").SetColumnConverter(
                "enum_string",
                new EnumValueConverter<UserRole>(DbType.String));

            // Act
            var row = _cruder.GetRow("hoo", 11);

            // Assert
            Assert.That(row.id, Is.EqualTo(11));
            Assert.That(row.name, Is.Null);
            Assert.That(row.enum_string, Is.EqualTo(UserRole.Developer));
        }

        [Test]
        public void DeleteRow_ValidId_DeletesRow()
        {
            // Arrange
            this.Connection.ExecuteSingleSql(@"
INSERT INTO [language](
    [id],
    [code],
    [name])
VALUES(
    '4fca7968-49cf-4c52-b793-e4b27087251b',
    'en',
    'English')");

            var id = new Guid("4fca7968-49cf-4c52-b793-e4b27087251b");

            // Act
            var deleted = _cruder.DeleteRow("language", id);

            // Assert
            Assert.That(deleted, Is.True);

            var deletedRow = this.GetRow("language", id);
            Assert.That(deletedRow, Is.Null);
        }

        protected override void ExecuteDbCreationScript()
        {
            var migrator = new TestMigrator(this.ConnectionString, this.GetType().Assembly);
            migrator.Migrate();
        }
    }
}
