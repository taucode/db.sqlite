using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TauCode.Db.DbValueConverters;
using TauCode.Db.Exceptions;
using TauCode.Extensions;

namespace TauCode.Db.SQLite.Tests.DbSerializer
{
    [TestFixture]
    public class SQLiteSerializerTests : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            var sql = this.GetType().Assembly.GetResourceText("CreatePersonDb.sql", true);
            this.Connection.ExecuteCommentedScript(sql);

            sql = this.GetType().Assembly.GetResourceText("SeedPersonDb.sql", true);
            this.Connection.ExecuteCommentedScript(sql);
        }

        #region Constructor

        [Test]
        public void Constructor_ValidArguments_RunsOk()
        {
            // Arrange

            // Act
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);

            // Assert
            Assert.That(serializer.Connection, Is.SameAs(this.Connection));
            Assert.That(serializer.Factory, Is.SameAs(SQLiteUtilityFactory.Instance));
            Assert.That(serializer.SchemaName, Is.EqualTo(null));
            Assert.That(serializer.Cruder, Is.TypeOf<SQLiteCruder>());
        }

        [Test]
        public void Constructor_ConnectionIsNull_ThrowsArgumentNullException()
        {
            // Arrange

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => new SQLiteSerializer(null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("connection"));
        }

        [Test]
        public void Constructor_ConnectionIsNotOpen_ThrowsArgumentException()
        {
            // Arrange
            using var connection = TestHelper.CreateConnection(false, false);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => new SQLiteSerializer(connection));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("connection"));
            Assert.That(ex.Message, Does.StartWith("Connection should be opened."));
        }

        #endregion

        #region SerializeTableData

        [Test]
        public void SerializeTableData_ValidArguments_RunsOk()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);
            serializer.JsonSerializerSettings.Formatting = Formatting.Indented;

            // Act
            var json = serializer.SerializeTableData("Person");

            // Assert
            var expectedJson = this.GetType().Assembly.GetResourceText("SerializeTableResult.json", true);

            Assert.That(json, Is.EqualTo(expectedJson));
        }

        [Test]
        public void SerializeTableData_TableNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => serializer.SerializeTableData(null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        [Test]
        public void SerializeTableData_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => serializer.SerializeTableData("bad_table"));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Table 'bad_table' does not exist."));
        }

        #endregion

        #region SerializeDbData

        [Test]
        public void SerializeDbData_ValidArguments_RunsOk()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);
            serializer.JsonSerializerSettings.Converters = new List<JsonConverter>
            {
                new StringEnumConverter(namingStrategy:new DefaultNamingStrategy()),
            };
            serializer.JsonSerializerSettings.Formatting = Formatting.Indented;

            // Act
            serializer.Cruder.GetTableValuesConverter("Person").SetColumnConverter("Gender", new EnumValueConverter<Gender>(DbType.Byte));
            serializer.Cruder.GetTableValuesConverter("WorkInfo").SetColumnConverter("PositionCode", new EnumValueConverter<WorkPosition>(DbType.AnsiString));

            var json = serializer.SerializeDbData(x => x != "Photo");

            // Assert
            var expectedJson = this.GetType().Assembly.GetResourceText("SerializeDbCustomResult.json", true);

            Assert.That(json, Is.EqualTo(expectedJson));
        }

        [Test]
        public void SerializeDbData_TableNamePredicateIsNull_SerializesDbData()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);
            serializer.JsonSerializerSettings.Converters = new List<JsonConverter>
            {
                new StringEnumConverter(namingStrategy:new DefaultNamingStrategy()),
            };
            serializer.JsonSerializerSettings.Formatting = Formatting.Indented;

            // Act
            var json = serializer.SerializeDbData();

            // Assert
            var expectedJson = this.GetType().Assembly.GetResourceText("SerializeDbResult.json", true);

            Assert.That(json, Is.EqualTo(expectedJson));
        }

        [Test]
        public void SerializeDbData_TableNamePredicateIsFalser_ReturnsEmptyArray()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);

            // Act
            var json = serializer.SerializeDbData(x => false);

            // Assert
            Assert.That(json, Is.EqualTo("{}"));
        }

        #endregion

        #region DeserializeTableData

        [Test]
        public void DeserializeTableData_ValidArguments_RunsOk()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);
            var json = this.GetType().Assembly.GetResourceText("DeserializeTableInput.json", true);

            this.Connection.ExecuteSingleSql("DELETE FROM [Photo]");
            this.Connection.ExecuteSingleSql("DELETE FROM [WorkInfo]");
            this.Connection.ExecuteSingleSql("DELETE FROM [PersonData]");
            this.Connection.ExecuteSingleSql("DELETE FROM [Person]");

            // Act
            serializer.DeserializeTableData(
                "Person",
                json,
                (tableMold, row) =>
                {
                    if (tableMold.Name == "Person")
                    {
                        var birthday = (string)row.GetValue("Birthday");
                        var birthdayDateTime = DateTime.Parse(birthday.Substring("Month_".Length));
                        row.SetValue("Birthday", birthdayDateTime);
                    }

                    return row;
                });

            // Assert
            IDbCruder cruder = new SQLiteCruder(this.Connection);
            var persons = cruder.GetAllRows("Person");

            Assert.That(persons, Has.Count.EqualTo(2));

            var wolf = persons.Single(x => x.Id.Equals(1));
            Assert.That(wolf.Tag, Is.EqualTo(new Guid("df601c43-fb4c-4a4d-ab05-e6bf5cfa68d1")));
            Assert.That(wolf.IsChecked, Is.EqualTo(1));
            Assert.That(wolf.Birthday, Is.EqualTo(DateTime.Parse("1939-05-13")));
            Assert.That(wolf.FirstName, Is.EqualTo("Harvey"));
            Assert.That(wolf.LastName, Is.EqualTo("Keitel"));
            Assert.That(wolf.Initials, Is.EqualTo("HK"));
            Assert.That(wolf.Gender, Is.EqualTo(100));

            var maria = persons.Single(x => x.Id.Equals(2));
            Assert.That(maria.Tag, Is.EqualTo(new Guid("374d413a-6287-448d-a4c1-918067c2312c")));
            Assert.That(maria.IsChecked, Is.Null);
            Assert.That(maria.Birthday, Is.EqualTo(DateTime.Parse("1965-08-19")));
            Assert.That(maria.FirstName, Is.EqualTo("Maria"));
            Assert.That(maria.LastName, Is.EqualTo("Medeiros"));
            Assert.That(maria.Initials, Is.EqualTo("MM"));
            Assert.That(maria.Gender, Is.EqualTo(200));
        }

        [Test]
        public void DeserializeTableData_TableNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);
            var json = this.GetType().Assembly.GetResourceText("DeserializeTableInput.json", true);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => serializer.DeserializeTableData(null, json));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        [Test]
        public void DeserializeTableData_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => serializer.DeserializeTableData("bad_table", "[]"));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Table 'bad_table' does not exist."));
        }

        [Test]
        public void DeserializeTableData_JsonIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => serializer.DeserializeTableData("Person", null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("json"));
        }

        [Test]
        public void DeserializeTableData_JsonContainsBadData_ThrowsTauDbException()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);
            var json = this.GetType().Assembly.GetResourceText("DeserializeTableBadInput.json", true);

            // Act
            var ex = Assert.Throws<TauDbException>(() => serializer.DeserializeTableData("Person", json));

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Object representing row #0 is invalid. Property 'Id' is not a JValue."));
        }

        #endregion

        #region DeserializeDbData

        [Test]
        public void DeserializeDbData_ValidArguments_RunsOk()
        {
            // Arrange
            IDbInspector dbInspector = new SQLiteInspector(this.Connection);
            dbInspector.DeleteDataFromAllTables();

            IDbSerializer serializer = new SQLiteSerializer(this.Connection);
            var json = this.GetType().Assembly.GetResourceText("DeserializeDbInput.json", true);

            // Act
            serializer.DeserializeDbData(
                json,
                x => x != "WorkInfo",
                (tableMold, row) =>
                {
                    if (tableMold.Name == "Person")
                    {
                        var birthday = (string)row.GetValue("Birthday");
                        var birthdayDateTime = DateTime.Parse(birthday.Substring("Month_".Length));
                        row.SetValue("Birthday", birthdayDateTime);
                    }

                    return row;
                });

            // Assert

            #region Person

            Assert.That(TestHelper.GetTableRowCount(this.Connection, "Person"), Is.EqualTo(2));

            var harvey = TestHelper.LoadRow(this.Connection, "Person", 1);
            Assert.That(harvey["Id"], Is.EqualTo(1));
            Assert.That(harvey["Tag"], Is.EqualTo(new Guid("df601c43-fb4c-4a4d-ab05-e6bf5cfa68d1")));
            Assert.That(harvey["IsChecked"], Is.EqualTo(1));
            Assert.That(harvey["Birthday"], Is.EqualTo(DateTime.Parse("1939-05-13")));
            Assert.That(harvey["FirstName"], Is.EqualTo("Harvey"));
            Assert.That(harvey["LastName"], Is.EqualTo("Keitel"));
            Assert.That(harvey["Initials"], Is.EqualTo("HK"));
            Assert.That(harvey["Gender"], Is.EqualTo(100));

            var maria = TestHelper.LoadRow(this.Connection, "Person", 2);
            Assert.That(maria["Id"], Is.EqualTo(2));
            Assert.That(maria["Tag"], Is.EqualTo(new Guid("374d413a-6287-448d-a4c1-918067c2312c")));
            Assert.That(maria["IsChecked"], Is.EqualTo(null));
            Assert.That(maria["Birthday"], Is.EqualTo(DateTime.Parse("1965-08-19")));
            Assert.That(maria["FirstName"], Is.EqualTo("Maria"));
            Assert.That(maria["LastName"], Is.EqualTo("Medeiros"));
            Assert.That(maria["Initials"], Is.EqualTo("MM"));
            Assert.That(maria["Gender"], Is.EqualTo(200));

            #endregion

            #region PersonData

            Assert.That(TestHelper.GetTableRowCount(this.Connection, "PersonData"), Is.EqualTo(2));

            var harveyData = TestHelper.LoadRow(this.Connection, "PersonData", 101);
            Assert.That(harveyData["Id"], Is.EqualTo(101));
            Assert.That(harveyData["PersonId"], Is.EqualTo(1));
            Assert.That(harveyData["BestAge"], Is.EqualTo(42));
            Assert.That(harveyData["Hash"], Is.EqualTo(791888333));
            Assert.That(harveyData["Height"], Is.EqualTo(175.5));
            Assert.That(harveyData["Weight"], Is.EqualTo(68.9));
            Assert.That(harveyData["UpdatedAt"], Is.EqualTo(DateTime.Parse("1996-11-02T11:12:13")));
            Assert.That(harveyData["Signature"], Is.EqualTo(new byte[] { 0xde, 0xfe, 0xca, 0x77 }));

            var mariaData = TestHelper.LoadRow(this.Connection, "PersonData", 201);
            Assert.That(mariaData["Id"], Is.EqualTo(201));
            Assert.That(mariaData["PersonId"], Is.EqualTo(2));
            Assert.That(mariaData["BestAge"], Is.EqualTo(26));
            Assert.That(mariaData["Hash"], Is.EqualTo(901014134123412));
            Assert.That(mariaData["Height"], Is.EqualTo(168.5));
            Assert.That(mariaData["Weight"], Is.EqualTo(54.7));
            Assert.That(mariaData["UpdatedAt"], Is.EqualTo(DateTime.Parse("1994-01-11T16:01:02")));
            Assert.That(mariaData["Signature"], Is.EqualTo(new byte[] { 0x15, 0x99, 0xaa, 0xbb }));

            #endregion

            #region Photo

            Assert.That(TestHelper.GetTableRowCount(this.Connection, "Photo"), Is.EqualTo(4));

            var harveyPhoto1 = TestHelper.LoadRow(this.Connection, "Photo", "PH-1");
            Assert.That(harveyPhoto1["Id"], Is.EqualTo("PH-1"));
            Assert.That(harveyPhoto1["PersonDataId"], Is.EqualTo(101));
            Assert.That(harveyPhoto1["Content"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey1.png", true)));
            Assert.That(harveyPhoto1["ContentThumbnail"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey1Thumb.png", true)));
            Assert.That(harveyPhoto1["TakenAt"], Is.EqualTo(DateTime.Parse("1997-12-12T11:12:13")));
            Assert.That(harveyPhoto1["ValidUntil"], Is.EqualTo(DateTime.Parse("1998-12-12")));

            var harveyPhoto2 = TestHelper.LoadRow(this.Connection, "Photo", "PH-2");
            Assert.That(harveyPhoto2["Id"], Is.EqualTo("PH-2"));
            Assert.That(harveyPhoto2["PersonDataId"], Is.EqualTo(101));
            Assert.That(harveyPhoto2["Content"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey2.png", true)));
            Assert.That(harveyPhoto2["ContentThumbnail"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey2Thumb.png", true)));
            Assert.That(harveyPhoto2["TakenAt"], Is.EqualTo(DateTime.Parse("1991-01-01T02:16:17")));
            Assert.That(harveyPhoto2["ValidUntil"], Is.EqualTo(DateTime.Parse("1993-09-09")));

            var mariaPhoto1 = TestHelper.LoadRow(this.Connection, "Photo", "PM-1");
            Assert.That(mariaPhoto1["Id"], Is.EqualTo("PM-1"));
            Assert.That(mariaPhoto1["PersonDataId"], Is.EqualTo(201));
            Assert.That(mariaPhoto1["Content"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria1.png", true)));
            Assert.That(mariaPhoto1["ContentThumbnail"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria1Thumb.png", true)));
            Assert.That(mariaPhoto1["TakenAt"], Is.EqualTo(DateTime.Parse("1998-04-05T08:09:22")));
            Assert.That(mariaPhoto1["ValidUntil"], Is.EqualTo(DateTime.Parse("1999-04-05")));

            var mariaPhoto2 = TestHelper.LoadRow(this.Connection, "Photo", "PM-2");
            Assert.That(mariaPhoto2["Id"], Is.EqualTo("PM-2"));
            Assert.That(mariaPhoto2["PersonDataId"], Is.EqualTo(201));
            Assert.That(mariaPhoto2["Content"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria2.png", true)));
            Assert.That(mariaPhoto2["ContentThumbnail"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria2Thumb.png", true)));
            Assert.That(mariaPhoto2["TakenAt"], Is.EqualTo(DateTime.Parse("2001-06-01T11:12:19")));
            Assert.That(mariaPhoto2["ValidUntil"], Is.EqualTo(DateTime.Parse("2002-07-07")));

            #endregion

            #region WorkInfo

            Assert.That(TestHelper.GetTableRowCount(this.Connection, "WorkInfo"), Is.EqualTo(0));

            #endregion
        }

        [Test]
        public void DeserializeDbData_JsonIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => serializer.DeserializeDbData(null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("json"));
        }

        [Test]
        public void DeserializeDbData_TablePredicateIsNull_DeserializesAll()
        {
            // Arrange
            IDbInspector dbInspector = new SQLiteInspector(this.Connection);
            dbInspector.DeleteDataFromAllTables();

            IDbSerializer serializer = new SQLiteSerializer(this.Connection);
            var json = this.GetType().Assembly.GetResourceText("DeserializeDbInput.json", true);

            // Act
            serializer.DeserializeDbData(
                json,
                null,
                (tableMold, row) =>
                {
                    if (tableMold.Name == "Person")
                    {
                        var birthday = (string)row.GetValue("Birthday");
                        var birthdayDateTime = DateTime.Parse(birthday.Substring("Month_".Length));
                        row.SetValue("Birthday", birthdayDateTime);
                    }

                    return row;
                });

            // Assert

            #region Person

            Assert.That(TestHelper.GetTableRowCount(this.Connection, "Person"), Is.EqualTo(2));

            var harvey = TestHelper.LoadRow(this.Connection, "Person", 1);
            Assert.That(harvey["Id"], Is.EqualTo(1));
            Assert.That(harvey["Tag"], Is.EqualTo(new Guid("df601c43-fb4c-4a4d-ab05-e6bf5cfa68d1")));
            Assert.That(harvey["IsChecked"], Is.EqualTo(1));
            Assert.That(harvey["Birthday"], Is.EqualTo(DateTime.Parse("1939-05-13")));
            Assert.That(harvey["FirstName"], Is.EqualTo("Harvey"));
            Assert.That(harvey["LastName"], Is.EqualTo("Keitel"));
            Assert.That(harvey["Initials"], Is.EqualTo("HK"));
            Assert.That(harvey["Gender"], Is.EqualTo(100));

            var maria = TestHelper.LoadRow(this.Connection, "Person", 2);
            Assert.That(maria["Id"], Is.EqualTo(2));
            Assert.That(maria["Tag"], Is.EqualTo(new Guid("374d413a-6287-448d-a4c1-918067c2312c")));
            Assert.That(maria["IsChecked"], Is.EqualTo(null));
            Assert.That(maria["Birthday"], Is.EqualTo(DateTime.Parse("1965-08-19")));
            Assert.That(maria["FirstName"], Is.EqualTo("Maria"));
            Assert.That(maria["LastName"], Is.EqualTo("Medeiros"));
            Assert.That(maria["Initials"], Is.EqualTo("MM"));
            Assert.That(maria["Gender"], Is.EqualTo(200));

            #endregion

            #region PersonData

            Assert.That(TestHelper.GetTableRowCount(this.Connection, "PersonData"), Is.EqualTo(2));

            var harveyData = TestHelper.LoadRow(this.Connection, "PersonData", 101);
            Assert.That(harveyData["Id"], Is.EqualTo(101));
            Assert.That(harveyData["PersonId"], Is.EqualTo(1));
            Assert.That(harveyData["BestAge"], Is.EqualTo(42));
            Assert.That(harveyData["Hash"], Is.EqualTo(791888333));
            Assert.That(harveyData["Height"], Is.EqualTo(175.5));
            Assert.That(harveyData["Weight"], Is.EqualTo(68.9));
            Assert.That(harveyData["UpdatedAt"], Is.EqualTo(DateTime.Parse("1996-11-02T11:12:13")));
            Assert.That(harveyData["Signature"], Is.EqualTo(new byte[] { 0xde, 0xfe, 0xca, 0x77 }));

            var mariaData = TestHelper.LoadRow(this.Connection, "PersonData", 201);
            Assert.That(mariaData["Id"], Is.EqualTo(201));
            Assert.That(mariaData["PersonId"], Is.EqualTo(2));
            Assert.That(mariaData["BestAge"], Is.EqualTo(26));
            Assert.That(mariaData["Hash"], Is.EqualTo(901014134123412));
            Assert.That(mariaData["Height"], Is.EqualTo(168.5));
            Assert.That(mariaData["Weight"], Is.EqualTo(54.7));
            Assert.That(mariaData["UpdatedAt"], Is.EqualTo(DateTime.Parse("1994-01-11T16:01:02")));
            Assert.That(mariaData["Signature"], Is.EqualTo(new byte[] { 0x15, 0x99, 0xaa, 0xbb }));

            #endregion

            #region Photo

            Assert.That(TestHelper.GetTableRowCount(this.Connection, "Photo"), Is.EqualTo(4));

            var harveyPhoto1 = TestHelper.LoadRow(this.Connection, "Photo", "PH-1");
            Assert.That(harveyPhoto1["Id"], Is.EqualTo("PH-1"));
            Assert.That(harveyPhoto1["PersonDataId"], Is.EqualTo(101));
            Assert.That(harveyPhoto1["Content"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey1.png", true)));
            Assert.That(harveyPhoto1["ContentThumbnail"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey1Thumb.png", true)));
            Assert.That(harveyPhoto1["TakenAt"], Is.EqualTo(DateTime.Parse("1997-12-12T11:12:13")));
            Assert.That(harveyPhoto1["ValidUntil"], Is.EqualTo(DateTime.Parse("1998-12-12")));

            var harveyPhoto2 = TestHelper.LoadRow(this.Connection, "Photo", "PH-2");
            Assert.That(harveyPhoto2["Id"], Is.EqualTo("PH-2"));
            Assert.That(harveyPhoto2["PersonDataId"], Is.EqualTo(101));
            Assert.That(harveyPhoto2["Content"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey2.png", true)));
            Assert.That(harveyPhoto2["ContentThumbnail"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey2Thumb.png", true)));
            Assert.That(harveyPhoto2["TakenAt"], Is.EqualTo(DateTime.Parse("1991-01-01T02:16:17")));
            Assert.That(harveyPhoto2["ValidUntil"], Is.EqualTo(DateTime.Parse("1993-09-09")));

            var mariaPhoto1 = TestHelper.LoadRow(this.Connection, "Photo", "PM-1");
            Assert.That(mariaPhoto1["Id"], Is.EqualTo("PM-1"));
            Assert.That(mariaPhoto1["PersonDataId"], Is.EqualTo(201));
            Assert.That(mariaPhoto1["Content"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria1.png", true)));
            Assert.That(mariaPhoto1["ContentThumbnail"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria1Thumb.png", true)));
            Assert.That(mariaPhoto1["TakenAt"], Is.EqualTo(DateTime.Parse("1998-04-05T08:09:22")));
            Assert.That(mariaPhoto1["ValidUntil"], Is.EqualTo(DateTime.Parse("1999-04-05")));

            var mariaPhoto2 = TestHelper.LoadRow(this.Connection, "Photo", "PM-2");
            Assert.That(mariaPhoto2["Id"], Is.EqualTo("PM-2"));
            Assert.That(mariaPhoto2["PersonDataId"], Is.EqualTo(201));
            Assert.That(mariaPhoto2["Content"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria2.png", true)));
            Assert.That(mariaPhoto2["ContentThumbnail"], Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria2Thumb.png", true)));
            Assert.That(mariaPhoto2["TakenAt"], Is.EqualTo(DateTime.Parse("2001-06-01T11:12:19")));
            Assert.That(mariaPhoto2["ValidUntil"], Is.EqualTo(DateTime.Parse("2002-07-07")));

            #endregion

            #region WorkInfo

            Assert.That(TestHelper.GetTableRowCount(this.Connection, "WorkInfo"), Is.EqualTo(2));

            var harveyWorkInfo = TestHelper.LoadRow(this.Connection, "WorkInfo", 1001);
            Assert.That(harveyWorkInfo["Id"], Is.EqualTo(1001));
            Assert.That(harveyWorkInfo["PersonId"], Is.EqualTo(1));
            Assert.That(harveyWorkInfo["PositionCode"], Is.EqualTo("Fixer"));
            Assert.That(harveyWorkInfo["PositionDescription"], Is.EqualTo("Человек, решающий пробемы"));
            Assert.That(harveyWorkInfo["PositionDescriptionEn"], Is.EqualTo("Man who fixes problems"));
            Assert.That(harveyWorkInfo["HiredOn"], Is.EqualTo(DateTime.Parse("1990-02-07T11:12:44")));

            var dateTime = (DateTime)harveyWorkInfo["WorkStartDayTime"];
            var time = dateTime.TimeOfDay;
            Assert.That(time, Is.EqualTo(TimeSpan.Parse("07:11:22")));

            Assert.That(harveyWorkInfo["Salary"], Is.EqualTo(20100.20m));
            Assert.That(harveyWorkInfo["Bonus"], Is.EqualTo(10500.70m));
            Assert.That(harveyWorkInfo["OvertimeCoef"], Is.EqualTo(1.2).Within(0.00001));
            Assert.That(harveyWorkInfo["WeekendCoef"], Is.EqualTo(3.7));
            Assert.That(harveyWorkInfo["Url"], Is.EqualTo("https://example.com/wolf"));

            var mariaWorkInfo = TestHelper.LoadRow(this.Connection, "WorkInfo", 2001);
            Assert.That(mariaWorkInfo["Id"], Is.EqualTo(2001));
            Assert.That(mariaWorkInfo["PersonId"], Is.EqualTo(2));
            Assert.That(mariaWorkInfo["PositionCode"], Is.EqualTo("Lover"));
            Assert.That(mariaWorkInfo["PositionDescription"], Is.EqualTo("Девушка, любящая Бутча"));
            Assert.That(mariaWorkInfo["PositionDescriptionEn"], Is.EqualTo("The girl who loves Butch"));
            Assert.That(mariaWorkInfo["HiredOn"], Is.EqualTo(DateTime.Parse("1989-08-11T01:08:05")));

            dateTime = (DateTime)mariaWorkInfo["WorkStartDayTime"];
            time = dateTime.TimeOfDay;
            Assert.That(time, Is.EqualTo(TimeSpan.Parse("01:44:33")));

            Assert.That(mariaWorkInfo["Salary"], Is.EqualTo(700.10m));
            Assert.That(mariaWorkInfo["Bonus"], Is.EqualTo(80.33m));
            Assert.That(mariaWorkInfo["OvertimeCoef"], Is.EqualTo(1.7).Within(0.00001));
            Assert.That(mariaWorkInfo["WeekendCoef"], Is.EqualTo(2.6));
            Assert.That(mariaWorkInfo["Url"], Is.EqualTo("https://example.com/fabienne"));

            #endregion
        }

        [Test]
        public void DeserializeDbData_TablePredicateReturnsUnknownTable_ThrowsTauDbException()
        {
            // Arrange
            IDbInspector dbInspector = new SQLiteInspector(this.Connection);
            dbInspector.DeleteDataFromAllTables();

            IDbSerializer serializer = new SQLiteSerializer(this.Connection);
            var json = this.GetType().Assembly.GetResourceText("DeserializeDbBadInput.json", true);

            // Act
            var ex = Assert.Throws<TauDbException>(() => serializer.DeserializeDbData(json));

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Table 'BadTable' does not exist."));
        }

        #endregion

        #region SerializeTableMetadata

        [Test]
        public void SerializeTableMetadata_ValidArguments_RunsOk()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);
            serializer.JsonSerializerSettings.Formatting = Formatting.Indented;
            serializer.JsonSerializerSettings.Converters = new JsonConverter[]
            {
                new StringEnumConverter(),
            };

            // Act
            var json = serializer.SerializeTableMetadata("Person");

            // Assert
            var expectedJson = this.GetType().Assembly.GetResourceText("SerializeTableMetadataResult.json", true);
            Assert.That(json, Is.EqualTo(expectedJson));
        }

        [Test]
        public void SerializeTableMetadata_TableNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => serializer.SerializeTableMetadata(null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        [Test]
        public void SerializeTableMetadata_TableDoesNotExist_ThrowsTauDbException()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);

            // Act
            var ex = Assert.Throws<TauDbException>(() => serializer.SerializeTableMetadata("bad_table"));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Table 'bad_table' does not exist."));
        }

        #endregion

        #region SerializeDbMetadata

        [Test]
        public void SerializeDbMetadata_ValidArguments_RunsOk()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);
            serializer.JsonSerializerSettings.Formatting = Formatting.Indented;
            serializer.JsonSerializerSettings.Converters = new JsonConverter[]
            {
                new StringEnumConverter(),
            };

            // Act
            var json = serializer.SerializeDbMetadata(x => x != "WorkInfo");

            // Assert
            var expectedJson = this.GetType().Assembly.GetResourceText("SerializeDbMetadataCustomResult.json", true);
            Assert.That(json, Is.EqualTo(expectedJson));
        }

        [Test]
        public void SerializeDbMetadata_PredicateIsNull_SerializesAll()
        {
            // Arrange
            IDbSerializer serializer = new SQLiteSerializer(this.Connection);
            serializer.JsonSerializerSettings.Formatting = Formatting.Indented;
            serializer.JsonSerializerSettings.Converters = new JsonConverter[]
            {
                new StringEnumConverter(),
            };

            // Act
            var json = serializer.SerializeDbMetadata(null);

            // Assert
            var expectedJson = this.GetType().Assembly.GetResourceText("SerializeDbMetadataResult.json", true);
            Assert.That(json, Is.EqualTo(expectedJson));
        }

        #endregion
    }
}
