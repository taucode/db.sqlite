using NUnit.Framework;
using System;


namespace TauCode.Db.SQLite.Tests.DbInspector
{
    [TestFixture]
    public class SQLiteInspectorTests : TestBase
    {
        #region Constructor

        /// <summary>
        /// Creates SqlInspector with valid connection and existing schema
        /// </summary>
        [Test]
        public void Constructor_ValidArguments_RunsOk()
        {
            // Arrange

            // Act
            IDbInspector inspector = new SQLiteInspector(this.Connection);

            // Assert
            Assert.That(inspector.Connection, Is.SameAs(this.Connection));
            Assert.That(inspector.Factory, Is.SameAs(SQLiteUtilityFactory.Instance));

            Assert.That(inspector.SchemaName, Is.EqualTo(null));
        }

        [Test]
        public void Constructor_SchemaIsNull_RunsOkAndSchemaIsNull()
        {
            // Arrange

            // Act
            IDbInspector inspector = new SQLiteInspector(this.Connection);

            // Assert
            Assert.That(inspector.Connection, Is.SameAs(this.Connection));
            Assert.That(inspector.Factory, Is.SameAs(SQLiteUtilityFactory.Instance));

            Assert.That(inspector.SchemaName, Is.EqualTo(null));
        }

        [Test]
        public void Constructor_ConnectionIsNull_ThrowsArgumentNullException()
        {
            // Arrange

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => new SQLiteInspector(null));
            
            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("connection"));
        }

        [Test]
        public void Constructor_ConnectionIsNotOpen_ThrowsArgumentException()
        {
            // Arrange
            using var connection = TestHelper.CreateConnection(false, false);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => new SQLiteInspector(connection));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("Connection should be opened."));
            Assert.That(ex.ParamName, Is.EqualTo("connection"));
        }

        #endregion

        #region GetSchemaNames

        [Test]
        public void GetSchemaNames_NoArguments_ReturnsSchemaNames()
        {
            // Arrange
            IDbInspector inspector = new SQLiteInspector(this.Connection);

            // Act
            var schemaNames = inspector.GetSchemaNames();

            // Assert
            Assert.That(schemaNames, Is.Empty);
        }

        #endregion

        #region GetTableNames

        [Test]
        public void GetTableNames_NoArguments_ReturnsTableNames()
        {
            // Arrange
            this.Connection.ExecuteSingleSql(@"
CREATE TABLE [tab2]([id] int PRIMARY KEY)
");

            this.Connection.ExecuteSingleSql(@"
CREATE TABLE [tab1]([id] int PRIMARY KEY)
");

            this.Connection.ExecuteSingleSql(@"
CREATE TABLE [tab3]([id] int PRIMARY KEY)
");

            IDbInspector inspector = new SQLiteInspector(this.Connection);

            // Act
            var tableNames = inspector.GetTableNames();

            // Assert
            Assert.That(
                tableNames,
                Is.EqualTo(new[]
                {
                    "tab1",
                    "tab2",
                    "tab3",
                }));
        }

        #endregion
    }
}
