using TauCode.Db.Model;
using TauCode.Parsing;

// todo regions
// todo unused methods
namespace TauCode.Db.SQLite.Parsing;

internal class SQLiteParsingResult : IParsingResult
{
    public List<object> Clauses { get; } = new List<object>();

    public void AddCreateClausePlaceholder()
    {
        this.Clauses.Add("CREATE");
    }

    public void ReplaceCreateClausePlaceholderWithCreateTableInfo()
    {
        var last = this.Clauses.Last();
        if ((string)last != "CREATE")
        {
            throw new NotImplementedException();
        }

        this.Clauses[^1] = new TableMold();
    }

    public void ReplaceCreateClausePlaceholderWithCreateIndexInfo()
    {
        var last = this.Clauses.Last();
        if ((string)last != "CREATE")
        {
            throw new NotImplementedException();
        }

        this.Clauses[^1] = new IndexMold();
    }

    public T GetLastClause<T>()
    {
        var lastClause = this.Clauses.Last();
        return (T)lastClause;
    }

    public int Version { get; private set; }

    public void IncreaseVersion()
    {
        this.Version++;
    }
}