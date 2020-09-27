using NUnit.Framework;

namespace TauCode.Db.SQLite.Tests
{
    [TestFixture]
    public class SQLiteScriptBuilderTests : TestBase
    {
        private IDbScriptBuilder _scriptBuilder;

        [SetUp]
        public void SetUp()
        {
            _scriptBuilder = this.DbInspector.Factory.CreateScriptBuilder(null);
        }

        [Test]
        public void BuildCreateTableScript_ValidArgument_CreatesScript()
        {
            // Arrange
            var table = this.DbInspector
                .Factory
                .CreateTableInspector(this.Connection, null, "fragment")
                .GetTable();

            // Act
            var sql = _scriptBuilder.BuildCreateTableScript(table, true);

            // Assert
            var expectedSql = @"CREATE TABLE [fragment](
    [id] UNIQUEIDENTIFIER NOT NULL,
    [note_translation_id] UNIQUEIDENTIFIER NOT NULL,
    [sub_type_id] UNIQUEIDENTIFIER NOT NULL,
    [code] TEXT NULL,
    [order] INTEGER NOT NULL,
    [content] TEXT NOT NULL,
    CONSTRAINT [PK_fragment] PRIMARY KEY([id] ASC),
    CONSTRAINT [FK_fragment_noteTranslation] FOREIGN KEY([note_translation_id]) REFERENCES [note_translation]([id]),
    CONSTRAINT [FK_fragment_fragmentSubType] FOREIGN KEY([sub_type_id]) REFERENCES [fragment_sub_type]([id]))";

            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        protected override void ExecuteDbCreationScript()
        {
            var migrator = new TestMigrator(this.ConnectionString, this.GetType().Assembly);
            migrator.Migrate();
        }
    }
}
