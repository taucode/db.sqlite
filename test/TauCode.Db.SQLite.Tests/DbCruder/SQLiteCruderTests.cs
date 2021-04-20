using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TauCode.Db.Data;
using TauCode.Db.DbValueConverters;
using TauCode.Db.Exceptions;
using TauCode.Db.Model;
using TauCode.Extensions;

namespace TauCode.Db.SQLite.Tests.DbCruder
{
    [TestFixture]
    public class SQLiteCruderTests : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            var sql = this.GetType().Assembly.GetResourceText("crebase.sql", true);
            this.Connection.ExecuteCommentedScript(sql);
        }

        private void CreateSuperTable()
        {
            var sql = this.GetType().Assembly.GetResourceText("SuperTable.sql", true);
            this.Connection.ExecuteSingleSql(sql);
        }

        private SuperTableRowDto CreateSuperTableRowDto()
        {
            return new SuperTableRowDto
            {
                Id = 17,
                TheGuid = new Guid("8e816a5f-b97c-43df-95e9-4fbfe7172dd0"),
                TheBigInt = 3891231123,
                TheDecimal = 11.2m,
                TheReal = 15.99,
                TheDateTime = DateTime.Parse("2011-11-12T10:10:10"),
                TheTime = TimeSpan.Parse("03:03:03"),
                TheText = "Андрей Коваленко",
                TheBlob = new byte[] { 0x10, 0x20, 0x33 },

                NotExisting = 777,
            };
        }

        private void InsertSuperTableRow()
        {
            using var command = this.Connection.CreateCommand();
            command.CommandText = @"
INSERT INTO [SuperTable](
    [Id],
    [TheGuid],
    [TheBigInt],
    [TheDecimal],
    [TheReal],
    [TheDateTime],
    [TheTime],
    [TheText],
    [TheBlob])
VALUES(
    @p_id,
    @p_theGuid,
    @p_theBigInt,
    @p_theDecimal,
    @p_theReal,
    @p_theDateTime,
    @p_theTime,
    @p_theText,
    @p_theBlob)
";

            var row = this.CreateSuperTableRowDto();

            command.Parameters.AddWithValue("@p_id", row.Id);
            command.Parameters.AddWithValue("@p_theGuid", row.TheGuid);
            command.Parameters.AddWithValue("@p_theBigInt", row.TheBigInt);
            command.Parameters.AddWithValue("@p_theDecimal", row.TheDecimal);
            command.Parameters.AddWithValue("@p_theReal", row.TheReal);
            command.Parameters.AddWithValue("@p_theDateTime", row.TheDateTime);
            command.Parameters.AddWithValue("@p_theTime", row.TheTime);
            command.Parameters.AddWithValue("@p_theText", row.TheText);
            command.Parameters.AddWithValue("@p_theBlob", row.TheBlob);

            command.ExecuteNonQuery();
        }

        private void CreateMediumTable()
        {
            var sql = @"
CREATE TABLE [MediumTable](
    [Id] integer NOT NULL PRIMARY KEY,

    [TheInt] integer NULL DEFAULT 1599,
    [TheNVarChar] text NULL DEFAULT 'Semmi')
";

            this.Connection.ExecuteSingleSql(sql);
        }

        private void CreateSmallTable()
        {
            var sql = @"
CREATE TABLE [SmallTable](
    [Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,

    [TheInt] integer NULL DEFAULT 1599,
    [TheNVarChar] text NULL DEFAULT 'Semmi')
";

            this.Connection.ExecuteSingleSql(sql);
        }

        #region Constructor

        [Test]
        public void Constructor_ValidArguments_RunsOk()
        {
            // Arrange

            // Act
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Assert
            Assert.That(cruder.Connection, Is.SameAs(this.Connection));
            Assert.That(cruder.Factory, Is.SameAs(SQLiteUtilityFactory.Instance));
            Assert.That(cruder.SchemaName, Is.EqualTo(null));
            Assert.That(cruder.ScriptBuilder, Is.TypeOf<SQLiteScriptBuilder>());
            Assert.That(cruder.BeforeInsertRow, Is.Null);
            Assert.That(cruder.AfterInsertRow, Is.Null);
        }

        [Test]
        public void Constructor_ConnectionIsNull_ThrowsArgumentNullException()
        {
            // Arrange

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => new SQLiteCruder(null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("connection"));
        }

        [Test]
        public void Constructor_ConnectionIsNotOpen_ThrowsArgumentException()
        {
            // Arrange
            using var connection = TestHelper.CreateConnection(false, false);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => new SQLiteCruder(connection));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("connection"));
            Assert.That(ex.Message, Does.StartWith("Connection should be opened."));
        }

        #endregion

        #region GetTableValuesConverter

        [Test]
        public void GetTableValuesConverter_ValidArgument_ReturnsProperConverter()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var converter = cruder.GetTableValuesConverter("PersonData");

            // Assert
            var dbValueConverter = converter.GetColumnConverter("Id");
            Assert.That(dbValueConverter, Is.TypeOf<GuidValueConverter>());
        }

        [Test]
        public void GetTableValuesConverter_ArgumentIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => cruder.GetTableValuesConverter(null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        [Test]
        public void GetTableValuesConverter_NotExistingTable_ThrowsTauDbException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => cruder.GetTableValuesConverter("bad_table"));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Table 'bad_table' does not exist."));
        }

        #endregion

        #region ResetTables

        [Test]
        public void ResetTables_NoArguments_RunsOk()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);
            cruder.GetTableValuesConverter("PersonData").SetColumnConverter("Id", new StringValueConverter());
            var oldDbValueConverter = cruder.GetTableValuesConverter("PersonData").GetColumnConverter("Id");

            // Act
            cruder.ResetTables();
            var resetDbValueConverter = cruder.GetTableValuesConverter("PersonData").GetColumnConverter("Id");

            // Assert
            Assert.That(oldDbValueConverter, Is.TypeOf<StringValueConverter>());
            Assert.That(resetDbValueConverter, Is.TypeOf<GuidValueConverter>());
        }

        #endregion

        #region InsertRow

        [Test]
        public void InsertRow_ValidArguments_InsertsRow()
        {
            // Arrange
            var row1 = new Dictionary<string, object>
            {
                {"Id", new Guid("a776fd76-f2a8-4e09-9e69-b6d08e96c075")},
                {"PersonId", 101},
                {"Weight", 69.2m},
                {"PersonMetaKey", (short) 12},
                {"IQ", 101.6m},
                {"Temper", (short) 4},
                {"PersonOrdNumber", (byte) 3},
                {"MetricB", -3},
                {"MetricA", 177},
                {"NotExisting", 11},
            };

            var row2 = new DynamicRow();
            row2.SetProperty("Id", new Guid("a776fd76-f2a8-4e09-9e69-b6d08e96c075"));
            row2.SetProperty("PersonId", 101);
            row2.SetProperty("Weight", 69.2m);
            row2.SetProperty("PersonMetaKey", (short)12);
            row2.SetProperty("IQ", 101.6m);
            row2.SetProperty("Temper", (short)4);
            row2.SetProperty("PersonOrdNumber", (byte)3);
            row2.SetProperty("MetricB", -3);
            row2.SetProperty("MetricA", 177);
            row2.SetProperty("NotExisting", 11);

            var row3 = new
            {
                Id = new Guid("a776fd76-f2a8-4e09-9e69-b6d08e96c075"),
                PersonId = 101,
                Weight = 69.2m,
                PersonMetaKey = (short)12,
                IQ = 101.6m,
                Temper = (short)4,
                PersonOrdNumber = (byte)3,
                MetricB = -3,
                MetricA = 177,
                NotExisting = 11,
            };

            var row4 = new HealthInfoDto
            {
                Id = new Guid("a776fd76-f2a8-4e09-9e69-b6d08e96c075"),
                PersonId = 101,
                Weight = 69.2m,
                PersonMetaKey = 12,
                IQ = 101.6m,
                Temper = 4,
                PersonOrdNumber = 3,
                MetricB = -3,
                MetricA = 177,
                NotExisting = 11,
            };

            object[] rows =
            {
                row1,
                row2,
                row3,
                row4,
            };

            IReadOnlyDictionary<string, object>[] loadedRows = new IReadOnlyDictionary<string, object>[rows.Length];

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            for (var i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                cruder.InsertRow("HealthInfo", row, x => x != "NotExisting");
                var loadedRow = TestHelper.LoadRow(
                    this.Connection,
                    "HealthInfo",
                    new Guid("a776fd76-f2a8-4e09-9e69-b6d08e96c075"));

                loadedRows[i] = loadedRow;

                this.Connection.ExecuteSingleSql("DELETE FROM [HealthInfo]");
            }

            // Assert
            for (var i = 0; i < loadedRows.Length; i++)
            {
                var originalRow = rows[i];
                var cleanOriginalRow = new DynamicRow(originalRow);
                cleanOriginalRow.RemoveProperty("NotExisting");

                var originalRowJson = JsonConvert.SerializeObject(cleanOriginalRow);
                var loadedJson = JsonConvert.SerializeObject(loadedRows[i]);

                Assert.That(loadedJson, Is.EqualTo(originalRowJson));
            }
        }

        [Test]
        public void InsertRow_AllDataTypes_RunsOk()
        {
            // Arrange
            this.CreateSuperTable();

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            var superRow = this.CreateSuperTableRowDto();
            var dynamicRow = new DynamicRow(superRow);
            dynamicRow.RemoveProperty("NotExisting");

            dynamic row = dynamicRow;

            // Act
            cruder.InsertRow("SuperTable", row, (Func<string, bool>)(x => true));

            // Assert
            var loadedRow = TestHelper.LoadRow(this.Connection, "SuperTable", 17);

            Assert.That(loadedRow["Id"], Is.EqualTo(17));
            Assert.That(loadedRow["Id"], Is.TypeOf<long>());

            Assert.That(loadedRow["TheGuid"], Is.EqualTo(new Guid("8e816a5f-b97c-43df-95e9-4fbfe7172dd0")));
            Assert.That(loadedRow["TheGuid"], Is.TypeOf<Guid>());

            Assert.That(loadedRow["TheBigInt"], Is.EqualTo(3891231123));
            Assert.That(loadedRow["TheBigInt"], Is.TypeOf<long>());

            Assert.That(loadedRow["TheDecimal"], Is.EqualTo(11.2m));
            Assert.That(loadedRow["TheDecimal"], Is.TypeOf<decimal>());

            Assert.That(loadedRow["TheReal"], Is.EqualTo(15.99));
            Assert.That(loadedRow["TheReal"], Is.TypeOf<double>());

            Assert.That(loadedRow["TheDateTime"], Is.EqualTo(DateTime.Parse("2011-11-12T10:10:10")));
            Assert.That(loadedRow["TheDateTime"], Is.TypeOf<DateTime>());

            var dateTime = (DateTime)loadedRow["TheTime"];
            Assert.That(dateTime.TimeOfDay, Is.EqualTo(TimeSpan.Parse("03:03:03")));
            Assert.That(loadedRow["TheTime"], Is.TypeOf<DateTime>());

            Assert.That(loadedRow["TheText"], Is.EqualTo("Андрей Коваленко"));
            Assert.That(loadedRow["TheText"], Is.TypeOf<string>());

            Assert.That(loadedRow["TheBlob"], Is.EqualTo(new byte[] { 0x10, 0x20, 0x33 }));
            Assert.That(loadedRow["TheBlob"], Is.TypeOf<byte[]>());
        }

        [Test]
        public void InsertRow_RowIsEmptyAndSelectorIsFalser_InsertsDefaultValues()
        {
            // Arrange
            var row1 = new Dictionary<string, object>();
            var row2 = new DynamicRow();
            var row3 = new { };

            object[] rows =
            {
                row1,
                row2,
                row3,
            };

            var insertedRows = new IReadOnlyDictionary<string, object>[rows.Length];

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            using var command = this.Connection.CreateCommand();

            for (var i = 0; i < rows.Length; i++)
            {
                var row = rows[i];

                var createTableSql = @"
CREATE TABLE [MyTab](
    [Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
    [Length] integer NULL DEFAULT NULL,
    [Name] text DEFAULT 'Polly')
";
                command.CommandText = createTableSql;
                command.ExecuteNonQuery();

                cruder.InsertRow("MyTab", row, x => false);
                var insertedRow = TestHelper.LoadRow(this.Connection, "MyTab", 1);
                insertedRows[i] = insertedRow;

                this.Connection.ExecuteSingleSql("DROP TABLE [MyTab]");
            }

            // Assert
            var json = JsonConvert.SerializeObject(
                new
                {
                    Id = 1,
                    Length = (int?)null,
                    Name = "Polly",
                },
                Formatting.Indented);

            foreach (var insertedRow in insertedRows)
            {
                var insertedJson = JsonConvert.SerializeObject(insertedRow, Formatting.Indented);
                Assert.That(insertedJson, Is.EqualTo(json));
            }
        }

        [Test]
        public void InsertRow_RowHasUnknownPropertiesAndSelectorIsFalser_InsertsDefaultValues()
        {
            // Arrange
            this.CreateSmallTable();

            var row1 = new Dictionary<string, object>
            {
                {"NonExisting", 777},
            };

            var row2 = new DynamicRow();
            row2.SetProperty("NonExisting", 777);

            var row3 = new
            {
                NonExisting = 777,
            };

            var row4 = new DummyDto
            {
                NonExisting = 777,
            };

            object[] rows =
            {
                row1,
                row2,
                row3,
                row4,
            };

            IReadOnlyDictionary<string, object>[] insertedRows = new IReadOnlyDictionary<string, object>[rows.Length];
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            for (var i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                cruder.InsertRow("SmallTable", row, x => false);

                var lastIdentity = (int)this.Connection.GetLastIdentity();

                var insertedRow = TestHelper.LoadRow(
                    this.Connection,
                    "SmallTable",
                    lastIdentity);

                insertedRows[i] = insertedRow;

                this.Connection.ExecuteSingleSql("DELETE FROM [SmallTable]");
            }

            // Assert
            foreach (var insertedRow in insertedRows)
            {
                Assert.That(insertedRow["TheInt"], Is.EqualTo(1599));
                Assert.That(insertedRow["TheNVarChar"], Is.EqualTo("Semmi"));
            }
        }

        [Test]
        public void InsertRow_NoColumnForSelectedProperty_ThrowsTauDbException()
        {
            // Arrange
            this.CreateSmallTable();

            var row = new
            {
                TheInt = 1,
                TheNVarChar = "Polina",
                NotExisting = 100,
            };

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => cruder.InsertRow("SmallTable", row));

            // Assert
            Assert.That(ex, Has.Message.EqualTo($"Column 'NotExisting' not found in table 'SmallTable'."));
        }

        [Test]
        public void InsertRow_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => cruder.InsertRow("bad_table", new object()));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Table 'bad_table' does not exist."));
        }

        [Test]
        public void InsertRow_TableNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => cruder.InsertRow(null, new object(), x => true));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        [Test]
        public void InsertRow_RowIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => cruder.InsertRow("HealthInfo", null, x => true));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("row"));
        }

        [Test]
        public void InsertRow_RowContainsDBNullValue_ThrowsTauDbException()
        {
            // Arrange
            this.CreateSuperTable();
            IDbCruder cruder = new SQLiteCruder(this.Connection);
            var row = new
            {
                TheGuid = DBNull.Value,
            };

            // Act
            var ex = Assert.Throws<TauDbException>(() => cruder.InsertRow("SuperTable", row, x => x == "TheGuid"));

            // Assert
            Assert.That(ex,
                Has.Message.EqualTo(
                    "Failed to apply value to DB command. See inner exception for details. Table: 'SuperTable', column: 'TheGuid', value: 'System.DBNull'."));
        }

        #endregion

        #region InsertRows

        [Test]
        public void InsertRows_ValidArguments_InsertsRows()
        {
            // Arrange
            var row1 = new Dictionary<string, object>
            {
                {"Id", new Guid("11111111-1111-1111-1111-111111111111")},
                {"PersonId", 101},
                {"Weight", 69.2m},
                {"PersonMetaKey", (short) 12},
                {"IQ", 101.6m},
                {"Temper", (short) 4},
                {"PersonOrdNumber", (byte) 3},
                {"MetricB", -3},
                {"MetricA", 177},
                {"NotExisting", 7},
            };

            var row2 = new DynamicRow();
            row2.SetProperty("Id", new Guid("22222222-2222-2222-2222-222222222222"));
            row2.SetProperty("PersonId", 101);
            row2.SetProperty("Weight", 69.2m);
            row2.SetProperty("PersonMetaKey", (short)12);
            row2.SetProperty("IQ", 101.6m);
            row2.SetProperty("Temper", (short)4);
            row2.SetProperty("PersonOrdNumber", (byte)3);
            row2.SetProperty("MetricB", -3);
            row2.SetProperty("MetricA", 177);
            row2.SetProperty("NotExisting", 7);

            var row3 = new
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                PersonId = 101,
                Weight = 69.2m,
                PersonMetaKey = (short)12,
                IQ = 101.6m,
                Temper = (short)4,
                PersonOrdNumber = (byte)3,
                MetricB = -3,
                MetricA = 177,
                NotExisting = 7,
            };

            var row4 = new HealthInfoDto
            {
                Id = new Guid("44444444-4444-4444-4444-444444444444"),
                PersonId = 101,
                Weight = 69.2m,
                PersonMetaKey = 12,
                IQ = 101.6m,
                Temper = 4,
                PersonOrdNumber = 3,
                MetricB = -3,
                MetricA = 177,
                NotExisting = 7,
            };

            object[] rows =
            {
                row1,
                row2,
                row3,
                row4,
            };

            //this.Connection.ExecuteSingleSql("ALTER TABLE [HealthInfo] DROP CONSTRAINT [FK_healthInfo_Person]");

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            cruder.InsertRows("HealthInfo", rows, x => x != "NotExisting");

            using var command = this.Connection.CreateCommand();
            command.CommandText = @"
SELECT
    *
FROM
    [HealthInfo]
ORDER BY
    [Id]
";
            var loadedRows = DbTools.GetCommandRows(command);
            Assert.That(loadedRows, Has.Count.EqualTo(4));

            for (var i = 0; i < loadedRows.Count; i++)
            {
                var cleanOriginalRow = new DynamicRow(rows[i]);
                cleanOriginalRow.RemoveProperty("NotExisting");

                var json = JsonConvert.SerializeObject(cleanOriginalRow, Formatting.Indented);
                var loadedJson = JsonConvert.SerializeObject(loadedRows[i], Formatting.Indented);

                Assert.That(json, Is.EqualTo(loadedJson));
            }
        }

        [Test]
        public void InsertRows_RowsAreEmptyAndSelectorIsFalser_InsertsDefaultValues()
        {
            // Arrange
            this.CreateSmallTable();

            var row1 = new Dictionary<string, object>();
            var row2 = new DynamicRow();
            var row3 = new object();

            var rows = new[]
            {
                row1,
                row2,
                row3,
            };

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            cruder.InsertRows("SmallTable", rows, x => false);

            // Assert
            using var command = this.Connection.CreateCommand();
            command.CommandText = @"
SELECT
    *
FROM
    [SmallTable]
ORDER BY
    [Id]
";
            var loadedRows = DbTools.GetCommandRows(command);
            Assert.That(loadedRows, Has.Count.EqualTo(3));

            foreach (var loadedRow in loadedRows)
            {
                Assert.That(loadedRow.TheInt, Is.EqualTo(1599));
                Assert.That(loadedRow.TheNVarChar, Is.EqualTo("Semmi"));
            }
        }

        [Test]
        public void InsertRows_PropertySelectorProducesNoProperties_InsertsDefaultValues()
        {
            // Arrange
            this.CreateSmallTable();

            var row1 = new
            {
                TheInt = 77,
                TheNVarChar = "abc",
            };

            var row2 = new
            {
                TheInt = 88,
                TheNVarChar = "def",
            };

            var rows = new[] { row1, row2 };

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            cruder.InsertRows("SmallTable", rows, x => false);

            // Assert
            using var command = this.Connection.CreateCommand();
            command.CommandText = @"
SELECT
    *
FROM
    [SmallTable]
ORDER BY
    [Id]
";

            var loadedRows = DbTools.GetCommandRows(command);
            Assert.That(loadedRows, Has.Count.EqualTo(2));

            foreach (var loadedRow in loadedRows)
            {
                Assert.That(loadedRow.TheInt, Is.EqualTo(1599));
                Assert.That(loadedRow.TheNVarChar, Is.EqualTo("Semmi"));
            }
        }

        [Test]
        public void InsertRows_PropertySelectorIsNull_UsesAllColumns()
        {
            // Arrange
            this.CreateSmallTable();

            var row1 = new
            {
                TheInt = 77,
                TheNVarChar = "abc",
            };

            var row2 = new
            {
                TheInt = 88,
                TheNVarChar = "def",
            };

            var rows = new[] { row1, row2 };

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            cruder.InsertRows("SmallTable", rows);

            // Assert
            using var command = this.Connection.CreateCommand();
            command.CommandText = @"
SELECT
    *
FROM
    [SmallTable]
ORDER BY
    [Id]
";

            var loadedRows = DbTools.GetCommandRows(command);
            Assert.That(loadedRows, Has.Count.EqualTo(2));

            var loadedRow = loadedRows[0];
            Assert.That(loadedRow.TheInt, Is.EqualTo(77));
            Assert.That(loadedRow.TheNVarChar, Is.EqualTo("abc"));

            loadedRow = loadedRows[1];
            Assert.That(loadedRow.TheInt, Is.EqualTo(88));
            Assert.That(loadedRow.TheNVarChar, Is.EqualTo("def"));
        }

        [Test]
        public void InsertRows_NoColumnForSelectedProperty_ThrowsTauDbException()
        {
            // Arrange
            this.CreateSmallTable();

            var row1 = new
            {
                TheInt = 77,
                TheNVarChar = "abc",
                NotExisting = 2,
            };

            var row2 = new
            {
                TheInt = 88,
                TheNVarChar = "def",
                NotExisting = 1,
            };

            var rows = new[] { row1, row2 };

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => cruder.InsertRows("SmallTable", rows, x => true));

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Column 'NotExisting' not found in table 'SmallTable'."));
        }

        [Test]
        public void InsertRows_NextRowSignatureDiffersFromPrevious_ThrowsArgumentException()
        {
            // Arrange
            this.CreateSmallTable();

            var row1 = new
            {
                TheInt = 77,
            };

            var row2 = new
            {
                TheNVarChar = "def",
            };

            var rows = new object[] { row1, row2 };

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => cruder.InsertRows("SmallTable", rows, x => true));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("'values' does not contain property representing column 'TheInt' of table 'SmallTable'."));
        }

        [Test]
        public void InsertRows_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => cruder.InsertRows("bad_table", new object[] { }));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Table 'bad_table' does not exist."));
        }

        [Test]
        public void InsertRows_TableNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => cruder.InsertRows(null, new object[] { }));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        [Test]
        public void InsertRows_RowsIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => cruder.InsertRows("HealthInfo", null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("rows"));
        }

        [Test]
        public void InsertRows_RowsContainNull_ThrowsArgumentException()
        {
            // Arrange
            this.CreateSmallTable();

            var rows = new[]
            {
                new object(),
                null,
            };

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => cruder.InsertRows("SmallTable", rows));

            // Assert
            Assert.That(ex, Has.Message.StartWith("'rows' must not contain nulls."));
            Assert.That(ex.ParamName, Is.EqualTo("rows"));
        }

        [Test]
        public void InsertRows_RowContainsDBNullValue_ThrowsTauDbException()
        {
            // Arrange
            this.CreateSmallTable();

            var rows = new object[]
            {
                new
                {
                    TheInt = 10,
                },
                new
                {
                    TheInt = DBNull.Value,
                },
            };

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => cruder.InsertRows("SmallTable", rows));

            // Assert
            Assert.That(ex,
                Has.Message.StartWith(
                    "Failed to apply value to DB command. See inner exception for details. Table: 'SmallTable', column: 'TheInt', value: 'System.DBNull'."));
        }

        #endregion

        #region RowInsertedCallback

        [Test]
        public void RowInsertedCallback_SetToSomeValue_KeepsThatValue()
        {
            // Arrange
            this.CreateSmallTable();

            IDbCruder cruder = new SQLiteCruder(this.Connection);
            var sb1 = new StringBuilder();

            // Act
            Func<TableMold, object, int, object> callback = (tableMold, row, index) =>
            {
                sb1.Append($"Table name: {tableMold.Name}; index: {index}");
                return row;
            };

            cruder.BeforeInsertRow = callback;

            cruder.InsertRow("SmallTable", new object());
            var callback1 = cruder.BeforeInsertRow;

            cruder.BeforeInsertRow = null;
            var callback2 = cruder.BeforeInsertRow;

            // Assert
            var s = sb1.ToString();
            Assert.That(s, Is.EqualTo("Table name: SmallTable; index: 0"));

            Assert.That(callback1, Is.SameAs(callback));
            Assert.That(callback2, Is.Null);
        }

        [Test]
        public void RowInsertedCallback_SetToNonNull_IsCalledWhenInsertRowIsCalled()
        {
            // Arrange
            this.CreateSmallTable();

            IDbCruder cruder = new SQLiteCruder(this.Connection);
            var sb1 = new StringBuilder();

            // Act
            cruder.BeforeInsertRow = (table, row, index) =>
            {
                sb1.Append($"Before insertion. Table name: {table.Name}; index: {index}. ");
                return row;
            };

            cruder.AfterInsertRow = (table, row, index) =>
            {
                sb1.Append($"After insertion. Table name: {table.Name}; index: {index}.");
            };

            cruder.InsertRow("SmallTable", new object());

            // Assert
            var s = sb1.ToString();
            Assert.That(s, Is.EqualTo("Before insertion. Table name: SmallTable; index: 0. After insertion. Table name: SmallTable; index: 0."));
        }

        [Test]
        public void RowInsertedCallback_SetToNonNull_IsCalledWhenInsertRowsIsCalled()
        {
            // Arrange
            this.CreateSmallTable();

            IDbCruder cruder = new SQLiteCruder(this.Connection);
            var sb1 = new StringBuilder();

            // Act
            cruder.BeforeInsertRow = (table, row, index) =>
            {
                sb1.AppendLine($"Table name: {table.Name}; index: {index}; int: {((dynamic)row).TheInt}");
                return row;
            };

            cruder.InsertRows(
                "SmallTable",
                new object[]
                {
                    new
                    {
                        TheInt = 11,
                    },
                    new
                    {
                        TheInt = 22,
                    },
                });

            // Assert
            var s = sb1.ToString();
            Assert.That(s, Is.EqualTo(@"Table name: SmallTable; index: 0; int: 11
Table name: SmallTable; index: 1; int: 22
"));
        }

        #endregion

        #region GetRow

        [Test]
        public void GetRow_ValidArguments_ReturnsRow()
        {
            // Arrange
            this.CreateSuperTable();

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            var superRow = this.CreateSuperTableRowDto();
            var dynamicRow = new DynamicRow(superRow);
            dynamicRow.RemoveProperty("NotExisting");

            dynamic row = dynamicRow;

            cruder.InsertRow("SuperTable", row, (Func<string, bool>)(x => true)); // InsertRow is ut'ed already :)

            // Act
            var loadedRow = ((DynamicRow)cruder.GetRow("SuperTable", 17, x => x.Contains("Time"))).ToDictionary();

            // Assert
            Assert.That(loadedRow, Has.Count.EqualTo(2));

            Assert.That(loadedRow["TheDateTime"], Is.EqualTo(DateTime.Parse("2011-11-12T10:10:10")));
            Assert.That(loadedRow["TheDateTime"], Is.TypeOf<DateTime>());

            Assert.That(loadedRow["TheTime"], Is.EqualTo(TimeSpan.Parse("03:03:03")));
            Assert.That(loadedRow["TheTime"], Is.TypeOf<TimeSpan>());
        }

        [Test]
        public void GetRow_AllDataTypes_RunsOk()
        {
            // Arrange
            this.CreateSuperTable();

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            var superRow = this.CreateSuperTableRowDto();
            var dynamicRow = new DynamicRow(superRow);
            dynamicRow.RemoveProperty("NotExisting");

            dynamic row = dynamicRow;

            cruder.InsertRow("SuperTable", row, (Func<string, bool>)(x => true)); // InsertRow is ut'ed already :)

            // Act
            var loadedRow = ((DynamicRow)cruder.GetRow("SuperTable", 17)).ToDictionary();

            // Assert
            Assert.That(loadedRow["Id"], Is.EqualTo(17));
            Assert.That(loadedRow["Id"], Is.TypeOf<long>());

            Assert.That(loadedRow["TheGuid"], Is.EqualTo(new Guid("8e816a5f-b97c-43df-95e9-4fbfe7172dd0")));
            Assert.That(loadedRow["TheGuid"], Is.TypeOf<Guid>());

            Assert.That(loadedRow["TheBigInt"], Is.EqualTo(3891231123));
            Assert.That(loadedRow["TheBigInt"], Is.TypeOf<long>());

            Assert.That(loadedRow["TheDecimal"], Is.EqualTo(11.2m));
            Assert.That(loadedRow["TheDecimal"], Is.TypeOf<decimal>());

            Assert.That(loadedRow["TheReal"], Is.EqualTo(15.99));
            Assert.That(loadedRow["TheReal"], Is.TypeOf<double>());

            Assert.That(loadedRow["TheDateTime"], Is.EqualTo(DateTime.Parse("2011-11-12T10:10:10")));
            Assert.That(loadedRow["TheDateTime"], Is.TypeOf<DateTime>());

            Assert.That(loadedRow["TheTime"], Is.EqualTo(TimeSpan.Parse("03:03:03")));
            Assert.That(loadedRow["TheTime"], Is.TypeOf<TimeSpan>());

            Assert.That(loadedRow["TheText"], Is.EqualTo("Андрей Коваленко"));
            Assert.That(loadedRow["TheText"], Is.TypeOf<string>());

            Assert.That(loadedRow["TheBlob"], Is.EqualTo(new byte[] { 0x10, 0x20, 0x33 }));
            Assert.That(loadedRow["TheBlob"], Is.TypeOf<byte[]>());
        }

        [Test]
        public void GetRow_SelectorIsTruer_DeliversAllColumns()
        {
            // Arrange
            this.CreateSuperTable();

            IDbCruder cruder = new SQLiteCruder(this.Connection);
            var strongTypedRow = this.CreateSuperTableRowDto();
            var dynamicRow = new DynamicRow(strongTypedRow);
            dynamicRow.RemoveProperty("NotExisting");

            dynamic row = dynamicRow;

            cruder.InsertRow("SuperTable", row, (Func<string, bool>)(x => true)); // InsertRow is ut'ed already :)

            // Act
            var loadedRow = ((DynamicRow)cruder.GetRow("SuperTable", 17, x => true)).ToDictionary();

            // Assert
            Assert.That(loadedRow["Id"], Is.EqualTo(17));
            Assert.That(loadedRow["Id"], Is.TypeOf<long>());

            Assert.That(loadedRow["TheGuid"], Is.EqualTo(new Guid("8e816a5f-b97c-43df-95e9-4fbfe7172dd0")));
            Assert.That(loadedRow["TheGuid"], Is.TypeOf<Guid>());

            Assert.That(loadedRow["TheBigInt"], Is.EqualTo(3891231123));
            Assert.That(loadedRow["TheBigInt"], Is.TypeOf<long>());

            Assert.That(loadedRow["TheDecimal"], Is.EqualTo(11.2m));
            Assert.That(loadedRow["TheDecimal"], Is.TypeOf<decimal>());

            Assert.That(loadedRow["TheReal"], Is.EqualTo(15.99));
            Assert.That(loadedRow["TheReal"], Is.TypeOf<double>());

            Assert.That(loadedRow["TheDateTime"], Is.EqualTo(DateTime.Parse("2011-11-12T10:10:10")));
            Assert.That(loadedRow["TheDateTime"], Is.TypeOf<DateTime>());

            Assert.That(loadedRow["TheTime"], Is.EqualTo(TimeSpan.Parse("03:03:03")));
            Assert.That(loadedRow["TheTime"], Is.TypeOf<TimeSpan>());

            Assert.That(loadedRow["TheText"], Is.EqualTo("Андрей Коваленко"));
            Assert.That(loadedRow["TheText"], Is.TypeOf<string>());

            Assert.That(loadedRow["TheBlob"], Is.EqualTo(new byte[] { 0x10, 0x20, 0x33 }));
            Assert.That(loadedRow["TheBlob"], Is.TypeOf<byte[]>());
        }

        [Test]
        public void GetRow_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => cruder.GetRow("bad_table", 1));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Table 'bad_table' does not exist."));
        }

        [Test]
        public void GetRow_TableNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => cruder.GetRow(null, 1));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        [Test]
        public void GetRow_IdIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => cruder.GetRow("some_table", null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("id"));
        }

        [Test]
        public void GetRow_TableHasNoPrimaryKey_ThrowsArgumentException()

        {
            // Arrange
            this.Connection.ExecuteSingleSql("CREATE TABLE [dummy](Foo integer)"); // no PK
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentException>((() => cruder.GetRow("dummy", 1)));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("Table 'dummy' does not have a primary key."));
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        [Test]
        public void GetRow_TablePrimaryKeyIsMultiColumn_ThrowsArgumentException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentException>((() => cruder.GetRow("Person", "the_id")));

            // Assert
            Assert.That(ex,
                Has.Message.StartsWith("Failed to retrieve single primary key column name for the table 'Person'."));
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        [Test]
        public void GetRow_IdNotFound_ReturnsNull()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);
            const int nonExistingId = 133;

            // Act
            var row = cruder.GetRow("NumericData", nonExistingId);

            // Assert
            Assert.That(row, Is.Null);
        }

        [Test]
        public void GetRow_SelectorIsFalser_ThrowsArgumentException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => cruder.GetRow("NumericData", 111, x => false));

            // Assert
            Assert.That(ex, Has.Message.StartWith("No columns were selected."));
            Assert.That(ex.ParamName, Is.EqualTo("columnSelector"));
        }

        #endregion

        #region GetAllRows

        [Test]
        public void GetAllRows_ValidArguments_ReturnsRows()
        {
            // Arrange
            var insertSql = this.GetType().Assembly.GetResourceText("InsertRows.sql", true);
            this.Connection.ExecuteCommentedScript(insertSql);

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var rows = cruder.GetAllRows("DateData", x => x == "Moment");

            // Assert
            var row = (DynamicRow)rows[0];
            Assert.That(row.GetDynamicMemberNames().Count(), Is.EqualTo(1));
            Assert.That(row.GetProperty("Moment"), Is.EqualTo(DateTime.Parse("2020-01-01T05:05:05")));

            row = rows[1];
            Assert.That(row.GetDynamicMemberNames().Count(), Is.EqualTo(1));
            Assert.That(row.GetProperty("Moment"), Is.EqualTo(DateTime.Parse("2020-02-02T06:06:06")));
        }

        [Test]
        public void GetAllRows_SelectorIsTruer_DeliversAllColumns()
        {
            // Arrange
            var insertSql = this.GetType().Assembly.GetResourceText("InsertRows.sql", true);
            this.Connection.ExecuteCommentedScript(insertSql);

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var rows = cruder.GetAllRows("DateData", x => true);

            // Assert
            var row = rows[0];
            Assert.That(row.Id, Is.EqualTo(new Guid("11111111-1111-1111-1111-111111111111")));
            Assert.That(row.Moment, Is.EqualTo(DateTime.Parse("2020-01-01T05:05:05")));

            row = rows[1];
            Assert.That(row.Id, Is.EqualTo(new Guid("22222222-2222-2222-2222-222222222222")));
            Assert.That(row.Moment, Is.EqualTo(DateTime.Parse("2020-02-02T06:06:06")));
        }

        [Test]
        public void GetAllRows_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => cruder.GetAllRows("bad_table"));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Table 'bad_table' does not exist."));
        }

        [Test]
        public void GetAllRows_TableNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => cruder.GetAllRows(null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        [Test]
        public void GetAllRows_SelectorIsFalser_ThrowsArgumentException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => cruder.GetAllRows("HealthInfo", x => false));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("No columns were selected."));
            Assert.That(ex.ParamName, Is.EqualTo("columnSelector"));
        }

        #endregion

        #region UpdateRow

        [Test]
        public void UpdateRow_ValidArguments_UpdatesRow()
        {
            // Arrange
            var id = 17;

            var update1 = new Dictionary<string, object>
            {
                {"Id", id},
                {"TheDateTime", DateTime.Parse("1978-07-05T08:08:08")},
                {"TheTime", TimeSpan.Parse("11:11:11")},
                {"NotExisting", 777},
            };

            var update2 = new DynamicRow();
            update2.SetProperty("Id", id);
            update2.SetProperty("TheDateTime", DateTime.Parse("1978-07-05T08:08:08"));
            update2.SetProperty("TheTime", TimeSpan.Parse("11:11:11"));
            update2.SetProperty("NotExisting", 777);

            var update3 = new
            {
                Id = id,
                TheDateTime = DateTime.Parse("1978-07-05T08:08:08"),
                TheTime = TimeSpan.Parse("11:11:11"),
                NotExisting = 777,
            };

            var update4 = new SuperTableRowDto
            {
                Id = id,
                TheDateTime = DateTime.Parse("1978-07-05T08:08:08"),
                TheTime = TimeSpan.Parse("11:11:11"),
                NotExisting = 777,
            };

            var updates = new object[]
            {
                update1,
                update2,
                update3,
                update4,
            };

            var loadedRows = new IReadOnlyDictionary<string, object>[updates.Length];

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            for (var i = 0; i < updates.Length; i++)
            {
                this.CreateSuperTable();
                this.InsertSuperTableRow();

                cruder.UpdateRow(
                    "SuperTable",
                    updates[i],
                    x => x.Contains("Time") || x == "Id");

                var loadedRow = TestHelper.LoadRow(this.Connection, "SuperTable", 17);
                loadedRows[i] = loadedRow;

                this.Connection.ExecuteSingleSql("DROP TABLE [SuperTable]");
            }

            for (var i = 0; i < loadedRows.Length; i++)
            {
                var loadedRow = loadedRows[i];

                Assert.That(loadedRow["Id"], Is.EqualTo(17));
                Assert.That(loadedRow["Id"], Is.TypeOf<long>());

                Assert.That(loadedRow["TheGuid"], Is.EqualTo(new Guid("8e816a5f-b97c-43df-95e9-4fbfe7172dd0")));
                Assert.That(loadedRow["TheGuid"], Is.TypeOf<Guid>());

                Assert.That(loadedRow["TheBigInt"], Is.EqualTo(3891231123));
                Assert.That(loadedRow["TheBigInt"], Is.TypeOf<long>());

                Assert.That(loadedRow["TheDecimal"], Is.EqualTo(11.2m));
                Assert.That(loadedRow["TheDecimal"], Is.TypeOf<decimal>());

                Assert.That(loadedRow["TheReal"], Is.EqualTo(15.99));
                Assert.That(loadedRow["TheReal"], Is.TypeOf<double>());

                Assert.That(loadedRow["TheDateTime"], Is.EqualTo(DateTime.Parse("1978-07-05T08:08:08")));
                Assert.That(loadedRow["TheDateTime"], Is.TypeOf<DateTime>());

                var dateTime = (DateTime)loadedRow["TheTime"];
                Assert.That(dateTime.TimeOfDay, Is.EqualTo(TimeSpan.Parse("11:11:11")));
                Assert.That(loadedRow["TheTime"], Is.TypeOf<DateTime>());

                Assert.That(loadedRow["TheText"], Is.EqualTo("Андрей Коваленко"));
                Assert.That(loadedRow["TheText"], Is.TypeOf<string>());

                Assert.That(loadedRow["TheBlob"], Is.EqualTo(new byte[] { 0x10, 0x20, 0x33 }));
                Assert.That(loadedRow["TheBlob"], Is.TypeOf<byte[]>());
            }
        }

        [Test]
        public void UpdateRow_AllDataTypes_RunsOk()
        {
            // Arrange
            this.CreateSuperTable();
            this.InsertSuperTableRow();

            var updateDto = new SuperTableRowDto
            {
                Id = 17,
                TheGuid = new Guid("22222222-2222-2222-2222-222222222222"),
                TheBigInt = 178811,
                TheDecimal = 99.13m,
                TheReal = 2.69,
                TheDateTime = DateTime.Parse("2001-01-01T17:18:19"),
                TheTime = TimeSpan.Parse("07:07:07"),
                TheText = "Whats up",
                TheBlob = new byte[] { 0xda, 0x77, 0x88 },
            };

            var updateDynamic = new DynamicRow(updateDto);
            updateDynamic.RemoveProperty("NotExisting");

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var updated = cruder.UpdateRow("SuperTable", updateDynamic, x => x != "NotExisting");

            // Assert
            Assert.That(updated, Is.True);

            var loadedRow = TestHelper.LoadRow(this.Connection, "SuperTable", 17);

            Assert.That(loadedRow["Id"], Is.EqualTo(17));
            Assert.That(loadedRow["Id"], Is.TypeOf<long>());

            Assert.That(loadedRow["TheGuid"], Is.EqualTo(new Guid("22222222-2222-2222-2222-222222222222")));
            Assert.That(loadedRow["TheGuid"], Is.TypeOf<Guid>());

            Assert.That(loadedRow["TheBigInt"], Is.EqualTo(178811));
            Assert.That(loadedRow["TheBigInt"], Is.TypeOf<long>());

            Assert.That(loadedRow["TheDecimal"], Is.EqualTo(99.13m));
            Assert.That(loadedRow["TheDecimal"], Is.TypeOf<decimal>());

            Assert.That(loadedRow["TheReal"], Is.EqualTo(2.69));
            Assert.That(loadedRow["TheReal"], Is.TypeOf<double>());

            Assert.That(loadedRow["TheDateTime"], Is.EqualTo(DateTime.Parse("2001-01-01T17:18:19")));
            Assert.That(loadedRow["TheDateTime"], Is.TypeOf<DateTime>());

            var dateTime = (DateTime)loadedRow["TheTime"];
            Assert.That(dateTime.TimeOfDay, Is.EqualTo(TimeSpan.Parse("07:07:07")));
            Assert.That(loadedRow["TheTime"], Is.TypeOf<DateTime>());

            Assert.That(loadedRow["TheText"], Is.EqualTo("Whats up"));
            Assert.That(loadedRow["TheText"], Is.TypeOf<string>());

            Assert.That(loadedRow["TheBlob"], Is.EqualTo(new byte[] { 0xda, 0x77, 0x88 }));
            Assert.That(loadedRow["TheBlob"], Is.TypeOf<byte[]>());
        }

        [Test]
        public void UpdateRow_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => cruder.UpdateRow("bad_table", new { Id = 1, Name = 2 }));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Table 'bad_table' does not exist."));
        }

        [Test]
        public void UpdateRow_TableNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() =>
                cruder.UpdateRow(null, new { Id = 1, Name = 2 }, x => true));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        [Test]
        public void UpdateRow_RowUpdateIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() =>
                cruder.UpdateRow("SuperTable", null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("rowUpdate"));
        }

        [Test]
        public void UpdateRow_PropertySelectorIsNull_UsesAllProperties()
        {
            // Arrange
            this.CreateSuperTable();
            this.InsertSuperTableRow();

            var updateDto = new SuperTableRowDto
            {
                Id = 17,
                TheGuid = new Guid("22222222-2222-2222-2222-222222222222"),
                TheBigInt = 178811,
                TheDecimal = 99.13m,
                TheReal = 2.69,
                TheDateTime = DateTime.Parse("2001-01-01T17:18:19"),
                TheTime = TimeSpan.Parse("07:07:07"),
                TheText = "Whats up",
                TheBlob = new byte[] { 0xda, 0x77, 0x88 },
                NotExisting = 777,
            };

            var update = new DynamicRow(updateDto);
            update.RemoveProperty("NotExisting");

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var updated = cruder.UpdateRow("SuperTable", update, null);

            // Assert
            Assert.That(updated, Is.True);

            var loadedRow = TestHelper.LoadRow(this.Connection, "SuperTable", 17);

            Assert.That(loadedRow["Id"], Is.EqualTo(17));
            Assert.That(loadedRow["Id"], Is.TypeOf<long>());

            Assert.That(loadedRow["TheGuid"], Is.EqualTo(new Guid("22222222-2222-2222-2222-222222222222")));
            Assert.That(loadedRow["TheGuid"], Is.TypeOf<Guid>());

            Assert.That(loadedRow["TheBigInt"], Is.EqualTo(178811));
            Assert.That(loadedRow["TheBigInt"], Is.TypeOf<long>());

            Assert.That(loadedRow["TheDecimal"], Is.EqualTo(99.13m));
            Assert.That(loadedRow["TheDecimal"], Is.TypeOf<decimal>());

            Assert.That(loadedRow["TheReal"], Is.EqualTo(2.69));
            Assert.That(loadedRow["TheReal"], Is.TypeOf<double>());

            Assert.That(loadedRow["TheDateTime"], Is.EqualTo(DateTime.Parse("2001-01-01T17:18:19")));
            Assert.That(loadedRow["TheDateTime"], Is.TypeOf<DateTime>());

            var dateTime = (DateTime)loadedRow["TheTime"];
            Assert.That(dateTime.TimeOfDay, Is.EqualTo(TimeSpan.Parse("07:07:07")));
            Assert.That(loadedRow["TheTime"], Is.TypeOf<DateTime>());

            Assert.That(loadedRow["TheText"], Is.EqualTo("Whats up"));
            Assert.That(loadedRow["TheText"], Is.TypeOf<string>());

            Assert.That(loadedRow["TheBlob"], Is.EqualTo(new byte[] { 0xda, 0x77, 0x88 }));
            Assert.That(loadedRow["TheBlob"], Is.TypeOf<byte[]>());
        }

        [Test]
        public void UpdateRow_PropertySelectorDoesNotContainPkColumn_ThrowsArgumentException()
        {
            // Arrange
            this.CreateSuperTable();
            this.InsertSuperTableRow();

            var update = new
            {
                TheGuid = new Guid("22222222-2222-2222-2222-222222222222"),
            };

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => cruder.UpdateRow("SuperTable", update));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("'rowUpdate' does not contain primary key value."));
            Assert.That(ex.ParamName, Is.EqualTo("rowUpdate"));
        }

        [Test]
        public void UpdateRow_PropertySelectorContainsOnlyPkColumn_ThrowsArgumentException()
        {
            // Arrange
            this.CreateSuperTable();
            this.InsertSuperTableRow();

            var update = new
            {
                Id = 1,
            };

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => cruder.UpdateRow("SuperTable", update));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("'rowUpdate' has no columns to update."));
            Assert.That(ex.ParamName, Is.EqualTo("rowUpdate"));
        }

        [Test]
        public void UpdateRow_IdIsNull_ThrowsArgumentException()
        {
            // Arrange
            this.CreateSuperTable();
            this.InsertSuperTableRow();

            var update = new
            {
                Id = (object)null,
                TheGuid = Guid.NewGuid(),
            };

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => cruder.UpdateRow("SuperTable", update));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("Primary key column value must not be null."));
            Assert.That(ex.ParamName, Is.EqualTo("rowUpdate"));
        }

        [Test]
        public void UpdateRow_NoColumnForSelectedProperty_ThrowsTauDbException()
        {
            // Arrange
            this.CreateSuperTable();
            this.InsertSuperTableRow();

            var update = new
            {
                Id = 1,
                NotExisting = 7,
            };

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => cruder.UpdateRow("SuperTable", update));

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Column 'NotExisting' not found in table 'SuperTable'."));
        }

        [Test]
        public void UpdateRow_TableHasNoPrimaryKey_ThrowsArgumentException()

        {
            // Arrange
            this.Connection.ExecuteSingleSql("CREATE TABLE [dummy](Foo integer)"); // no PK
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentException>((() => cruder.UpdateRow("dummy", new { Foo = 1 })));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("Table 'dummy' does not have a primary key."));
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        [Test]
        public void UpdateRow_TablePrimaryKeyIsMultiColumn_ThrowsArgumentException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentException>((() => cruder.UpdateRow("Person", new { Key = 3 })));

            // Assert
            Assert.That(ex,
                Has.Message.StartsWith("Failed to retrieve single primary key column name for the table 'Person'."));
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        #endregion

        #region DeleteRow

        [Test]
        public void DeleteRow_ValidArguments_DeletesRowAndReturnsTrue()
        {
            // Arrange
            this.CreateMediumTable();
            const int id = 1;
            this.Connection.ExecuteSingleSql($"INSERT INTO [MediumTable]([Id]) VALUES ({id})");

            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var deleted = cruder.DeleteRow("MediumTable", id);

            // Assert
            var deletedRow = TestHelper.LoadRow(this.Connection, "MediumTable", id);

            Assert.That(deleted, Is.True);
            Assert.That(deletedRow, Is.Null);
        }

        [Test]
        public void DeleteRow_IdNotFound_ReturnsFalse()
        {
            // Arrange
            this.CreateMediumTable();
            IDbCruder cruder = new SQLiteCruder(this.Connection);
            var notExistingId = 11;

            // Act
            var deleted = cruder.DeleteRow("MediumTable", notExistingId);

            // Assert
            Assert.That(deleted, Is.False);
        }

        [Test]
        public void DeleteRow_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => cruder.DeleteRow("bad_table", 17));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Table 'bad_table' does not exist."));
        }

        [Test]
        public void DeleteRow_TableNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => cruder.DeleteRow(null, 11));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        [Test]
        public void DeleteRow_IdIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => cruder.DeleteRow("MediumTable", null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("id"));
        }

        [Test]
        public void DeleteRow_TableHasNoPrimaryKey_ThrowsArgumentException()
        {
            // Arrange
            this.Connection.ExecuteSingleSql("CREATE TABLE [dummy](Foo integer)"); // no PK
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>((() => cruder.DeleteRow("dummy", 1)));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("Table 'dummy' does not have a primary key."));
        }

        [Test]
        public void DeleteRow_PrimaryKeyIsMultiColumn_ThrowsArgumentException()
        {
            // Arrange
            IDbCruder cruder = new SQLiteCruder(this.Connection);

            // Act

            var ex = Assert.Throws<ArgumentException>((() => cruder.DeleteRow("Person", "the_id")));

            // Assert
            Assert.That(
                ex,
                Has.Message.StartsWith("Failed to retrieve single primary key column name for the table 'Person'."));
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        #endregion
    }
}