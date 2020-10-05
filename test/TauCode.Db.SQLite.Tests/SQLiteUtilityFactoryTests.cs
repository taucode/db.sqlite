using NUnit.Framework;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace TauCode.Db.SQLite.Tests
{
    [TestFixture]
    public class SQLiteUtilityFactoryTests
    {
        [Test]
        public void Members_DifferentArguments_HaveExpectedProps()
        {
            // Arrange
            IDbUtilityFactory utilityFactory = SQLiteUtilityFactory.Instance;

            // get SQLite stuff loaded.
            using (new SQLiteConnection())
            {   
            }

            // Act
            IDbConnection connection = new SQLiteConnection();
            var tuple = SQLiteTools.CreateSQLiteDatabase();
            var filePath = tuple.Item1;
            var connectionString = tuple.Item2;
            connection.ConnectionString = connectionString;
            connection.Open();

            IDbDialect dialect = utilityFactory.GetDialect();

            IDbScriptBuilder scriptBuilder = utilityFactory.CreateScriptBuilder(null);

            IDbInspector dbInspector = utilityFactory.CreateInspector(connection, null);

            IDbTableInspector tableInspector = utilityFactory.CreateTableInspector(connection, null, "language");

            IDbCruder cruder = utilityFactory.CreateCruder(connection, null);

            IDbSerializer dbSerializer = utilityFactory.CreateSerializer(connection, null);

            // Assert
            Assert.That(connection, Is.TypeOf<SQLiteConnection>());
            Assert.That(dialect, Is.SameAs(SQLiteDialect.Instance));

            Assert.That(scriptBuilder, Is.TypeOf<SQLiteScriptBuilder>());
            Assert.That(scriptBuilder.CurrentOpeningIdentifierDelimiter, Is.EqualTo('['));

            Assert.That(dbInspector, Is.TypeOf<SQLiteInspector>());
            Assert.That(tableInspector, Is.TypeOf<SQLiteTableInspector>());
            Assert.That(cruder, Is.TypeOf<SQLiteCruder>());
            Assert.That(dbSerializer, Is.TypeOf<SQLiteSerializer>());

            // Finalize
            connection.Dispose();
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // dismiss
            }
        }
    }
}
