using System;
using NUnit.Framework;
using TauCode.Db.Exceptions;
using TauCode.Extensions;

namespace TauCode.Db.SQLite.Tests.DbMigrator
{
    [TestFixture]
    public class SQLiteJsonMigratorTests : TestBase
    {
        #region Constructor

        [Test]
        public void Constructor_ValidArguments_RunsOk()
        {
            // Arrange

            // Act
            var migrator = new SQLiteJsonMigrator(
                this.Connection,
                () => "{}",
                () => "{}");

            // Assert
            Assert.That(migrator.Connection, Is.SameAs(this.Connection));
            Assert.That(migrator.Factory, Is.SameAs(SQLiteUtilityFactory.Instance));
            Assert.That(migrator.SchemaName, Is.EqualTo(null));
        }

        [Test]
        public void Constructor_ConnectionIsNull_ThrowsArgumentNullException()
        {
            // Arrange

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => new SQLiteJsonMigrator(
                null,
                () => "{}",
                () => "{}"));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("connection"));
        }

        [Test]
        public void Constructor_ConnectionIsNotOpen_ThrowsArgumentException()
        {
            // Arrange
            using var connection = TestHelper.CreateConnection(false, false);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => new SQLiteJsonMigrator(
                connection,
                () => "{}",
                () => "{}"));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("connection"));
            Assert.That(ex.Message, Does.StartWith("Connection should be opened."));
        }

        [Test]
        public void Constructor_MetadataJsonGetterIsNull_ThrowsArgumentNullException()
        {
            // Arrange

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => new SQLiteJsonMigrator(
                this.Connection,
                null,
                () => "{}"));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("metadataJsonGetter"));
        }

        [Test]
        public void Constructor_DataJsonGetterIsNull_ThrowsArgumentNullException()
        {
            // Arrange

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => new SQLiteJsonMigrator(
                this.Connection,
                () => "{}",
                null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("dataJsonGetter"));
        }

        #endregion

        #region Migrate

        [Test]
        public void Migrate_ValidInput_RunsOk()
        {
            // Arrange
            var migrator = new SQLiteJsonMigrator(
                this.Connection,
                () => this.GetType().Assembly.GetResourceText("MigrateMetadataInput.json", true),
                () => this.GetType().Assembly.GetResourceText("MigrateDataCustomInput.json", true),
                x => x != "WorkInfo",
                (tableMold, row) =>
                {
                    if (tableMold.Name == "Person")
                    {
                        var birthday = (string)row.GetValue("Birthday");
                        var birthdayDateTime = DateTime.Parse(birthday.Substring("Month_".Length));
                        row.SetValue("Birthday", birthdayDateTime);

                        var genderString = (string)row.GetValue("Gender");
                        row.SetValue("Gender", (byte)genderString.ToEnum<Gender>());
                    }

                    return row;
                });

            // Act
            migrator.Migrate();

            // Assert
            IDbSchemaExplorer schemaExplorer = new SQLiteSchemaExplorer(this.Connection);

            #region metadata

            var scriptBuilder = new SQLiteScriptBuilder();
            var tableMolds = schemaExplorer.GetTables(null, true, true, true, true, true);
            var script = scriptBuilder.BuildCreateAllTablesScript(tableMolds);
            var expectedScript = this.GetType().Assembly.GetResourceText("MigratedDbCustomOutput.sql", true);

            Assert.That(script, Is.EqualTo(expectedScript));

            #endregion

            #region data

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
            Assert.That(harveyPhoto1["Content"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey1.png", true)));
            Assert.That(harveyPhoto1["ContentThumbnail"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey1Thumb.png", true)));
            Assert.That(harveyPhoto1["TakenAt"], Is.EqualTo(DateTime.Parse("1997-12-12T11:12:13")));
            Assert.That(harveyPhoto1["ValidUntil"], Is.EqualTo(DateTime.Parse("1998-12-12")));

            var harveyPhoto2 = TestHelper.LoadRow(this.Connection, "Photo", "PH-2");
            Assert.That(harveyPhoto2["Id"], Is.EqualTo("PH-2"));
            Assert.That(harveyPhoto2["PersonDataId"], Is.EqualTo(101));
            Assert.That(harveyPhoto2["Content"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey2.png", true)));
            Assert.That(harveyPhoto2["ContentThumbnail"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey2Thumb.png", true)));
            Assert.That(harveyPhoto2["TakenAt"], Is.EqualTo(DateTime.Parse("1991-01-01T02:16:17")));
            Assert.That(harveyPhoto2["ValidUntil"], Is.EqualTo(DateTime.Parse("1993-09-09")));

            var mariaPhoto1 = TestHelper.LoadRow(this.Connection, "Photo", "PM-1");
            Assert.That(mariaPhoto1["Id"], Is.EqualTo("PM-1"));
            Assert.That(mariaPhoto1["PersonDataId"], Is.EqualTo(201));
            Assert.That(mariaPhoto1["Content"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria1.png", true)));
            Assert.That(mariaPhoto1["ContentThumbnail"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria1Thumb.png", true)));
            Assert.That(mariaPhoto1["TakenAt"], Is.EqualTo(DateTime.Parse("1998-04-05T08:09:22")));
            Assert.That(mariaPhoto1["ValidUntil"], Is.EqualTo(DateTime.Parse("1999-04-05")));

            var mariaPhoto2 = TestHelper.LoadRow(this.Connection, "Photo", "PM-2");
            Assert.That(mariaPhoto2["Id"], Is.EqualTo("PM-2"));
            Assert.That(mariaPhoto2["PersonDataId"], Is.EqualTo(201));
            Assert.That(mariaPhoto2["Content"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria2.png", true)));
            Assert.That(mariaPhoto2["ContentThumbnail"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria2Thumb.png", true)));
            Assert.That(mariaPhoto2["TakenAt"], Is.EqualTo(DateTime.Parse("2001-06-01T11:12:19")));
            Assert.That(mariaPhoto2["ValidUntil"], Is.EqualTo(DateTime.Parse("2002-07-07")));

            #endregion

            #region WorkInfo

            // no such table in this test (WorkInfo)

            #endregion

            #endregion
        }

        [Test]
        public void Migrate_TablePredicateIsNull_MigratesAll()
        {
            // Arrange
            var migrator = new SQLiteJsonMigrator(
                this.Connection,
                () => this.GetType().Assembly.GetResourceText("MigrateMetadataInput.json", true),
                () => this.GetType().Assembly.GetResourceText("MigrateDataInput.json", true));

            // Act
            migrator.Migrate();

            // Assert
            IDbSchemaExplorer schemaExplorer = new SQLiteSchemaExplorer(this.Connection);

            #region metadata

            var scriptBuilder = new SQLiteScriptBuilder();
            var tableMolds = schemaExplorer.GetTables(null, true, true, true, true, true);
            var script = scriptBuilder.BuildCreateAllTablesScript(tableMolds);

            var expectedScript = this.GetType().Assembly.GetResourceText("MigratedDbOutput.sql", true);

            Assert.That(script, Is.EqualTo(expectedScript));

            #endregion

            #region data

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
            Assert.That(harveyPhoto1["Content"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey1.png", true)));
            Assert.That(harveyPhoto1["ContentThumbnail"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey1Thumb.png", true)));
            Assert.That(harveyPhoto1["TakenAt"], Is.EqualTo(DateTime.Parse("1997-12-12T11:12:13")));
            Assert.That(harveyPhoto1["ValidUntil"], Is.EqualTo(DateTime.Parse("1998-12-12")));

            var harveyPhoto2 = TestHelper.LoadRow(this.Connection, "Photo", "PH-2");
            Assert.That(harveyPhoto2["Id"], Is.EqualTo("PH-2"));
            Assert.That(harveyPhoto2["PersonDataId"], Is.EqualTo(101));
            Assert.That(harveyPhoto2["Content"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey2.png", true)));
            Assert.That(harveyPhoto2["ContentThumbnail"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicHarvey2Thumb.png", true)));
            Assert.That(harveyPhoto2["TakenAt"], Is.EqualTo(DateTime.Parse("1991-01-01T02:16:17")));
            Assert.That(harveyPhoto2["ValidUntil"], Is.EqualTo(DateTime.Parse("1993-09-09")));

            var mariaPhoto1 = TestHelper.LoadRow(this.Connection, "Photo", "PM-1");
            Assert.That(mariaPhoto1["Id"], Is.EqualTo("PM-1"));
            Assert.That(mariaPhoto1["PersonDataId"], Is.EqualTo(201));
            Assert.That(mariaPhoto1["Content"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria1.png", true)));
            Assert.That(mariaPhoto1["ContentThumbnail"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria1Thumb.png", true)));
            Assert.That(mariaPhoto1["TakenAt"], Is.EqualTo(DateTime.Parse("1998-04-05T08:09:22")));
            Assert.That(mariaPhoto1["ValidUntil"], Is.EqualTo(DateTime.Parse("1999-04-05")));

            var mariaPhoto2 = TestHelper.LoadRow(this.Connection,  "Photo", "PM-2");
            Assert.That(mariaPhoto2["Id"], Is.EqualTo("PM-2"));
            Assert.That(mariaPhoto2["PersonDataId"], Is.EqualTo(201));
            Assert.That(mariaPhoto2["Content"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria2.png", true)));
            Assert.That(mariaPhoto2["ContentThumbnail"],
                Is.EqualTo(this.GetType().Assembly.GetResourceBytes("PicMaria2Thumb.png", true)));
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

            #endregion
        }

        [Test]
        public void Migrate_MetadataJsonGetterReturnsNull_ThrowsTauDbException()
        {
            // Arrange
            var migrator = new SQLiteJsonMigrator(
                this.Connection,
                () => null,
                () => "{}");

            // Act
            var ex = Assert.Throws<TauDbException>(() => migrator.Migrate());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("'MetadataJsonGetter' returned null."));
        }

        [Test]
        public void Migrate_DataJsonGetterReturnsNull_ThrowsTauDbException()
        {
            // Arrange
            var migrator = new SQLiteJsonMigrator(
                this.Connection,
                () => "{}",
                () => null);

            // Act
            var ex = Assert.Throws<TauDbException>(() => migrator.Migrate());

            // Assert
            Assert.That(ex, Has.Message.EqualTo("'DataJsonGetter' returned null."));
        }

        #endregion
    }
}
