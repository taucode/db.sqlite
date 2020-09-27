using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace TauCode.Db.SQLite.Tests
{
    [TestFixture]
    public abstract class TestBase
    {
        protected IDbInspector DbInspector;
        protected IDbConnection Connection;
        protected string ConnectionString;

        protected string TempDbFilePath;

        protected virtual void OneTimeSetUpImpl()
        {
            var tuple = SQLiteTools.CreateSQLiteDatabase();
            this.TempDbFilePath = tuple.Item1;
            this.ConnectionString = tuple.Item2;

            this.Connection = new SQLiteConnection(this.ConnectionString);
            this.Connection.Open();
            ((SQLiteConnection)this.Connection).BoostSQLiteInsertions();

            this.DbInspector = new SQLiteInspector(Connection);

            this.ExecuteDbCreationScript();
        }

        protected abstract void ExecuteDbCreationScript();

        protected virtual void OneTimeTearDownImpl()
        {
            this.Connection.Dispose();
            try
            {
                File.Delete(TempDbFilePath);
            }
            catch
            {
                // dismiss
            }
        }

        protected virtual void SetUpImpl()
        {
            this.DbInspector.DeleteDataFromAllTables();
        }

        protected virtual void TearDownImpl()
        {
        }

        [OneTimeSetUp]
        public void OneTimeSetUpBase()
        {
            this.OneTimeSetUpImpl();
        }

        [OneTimeTearDown]
        public void OneTimeTearDownBase()
        {
            this.OneTimeTearDownImpl();
        }

        [SetUp]
        public void SetUpBase()
        {
            this.SetUpImpl();
        }

        [TearDown]
        public void TearDownBase()
        {
            this.TearDownImpl();
        }

        protected dynamic GetRow(string tableName, object id)
        {
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText = $@"SELECT * FROM [{tableName}] WHERE [id] = @p_id";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "p_id";
                parameter.Value = id;
                command.Parameters.Add(parameter);
                var row = DbTools.GetCommandRows(command).SingleOrDefault();
                return row;
            }
        }

        protected IList<dynamic> GetRows(string tableName)
        {
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText = $@"SELECT * FROM [{tableName}]";
                var rows = DbTools.GetCommandRows(command);
                return rows;
            }
        }
    }
}
