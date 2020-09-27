using System.Collections.Generic;
using TauCode.Db.SQLite.Parsing.TextClasses;
using TauCode.Parsing;
using TauCode.Parsing.Building;
using TauCode.Parsing.TextClasses;

namespace TauCode.Db.SQLite.Parsing
{
    public class SQLiteNodeFactory : NodeFactoryBase
    {
        public SQLiteNodeFactory()
            : base(
                "SQLite Nodes Family",
                new List<ITextClass>
                {
                    WordTextClass.Instance,
                    StringTextClass.Instance,
                    SqlIdentifierTextClass.Instance,
                },
                false)
        {
        }
    }
}