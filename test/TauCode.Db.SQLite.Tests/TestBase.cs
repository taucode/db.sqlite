using NUnit.Framework;
using System.Data.SQLite;

namespace TauCode.Db.SQLite.Tests;

[TestFixture]
public abstract class TestBase
{
    protected SQLiteConnection Connection { get; private set; }

    [OneTimeSetUp]
    public void OneTimeSetUpBase()
    {
        this.Connection = TestHelper.CreateConnection();
    }

    [OneTimeTearDown]
    public void OneTimeTearDownBase()
    {
        this.Connection.Dispose();
        this.Connection = null;
    }

    [SetUp]
    public void SetUpBase()
    {
        this.Connection.PurgeDatabase();
    }
}