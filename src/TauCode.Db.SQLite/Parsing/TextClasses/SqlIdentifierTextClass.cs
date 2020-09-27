using TauCode.Parsing;
using TauCode.Parsing.TextClasses;

namespace TauCode.Db.SQLite.Parsing.TextClasses
{
    [TextClass("identifier")]
    public class SqlIdentifierTextClass : TextClassBase
    {
        public static SqlIdentifierTextClass Instance { get; } = new SqlIdentifierTextClass();

        private SqlIdentifierTextClass()
        {
        }

        protected override string TryConvertFromImpl(string text, ITextClass anotherClass)
        {
            if (
                anotherClass is WordTextClass &&
                !SQLiteParsingHelper.IsReservedWord(text))
            {
                return text;
            }

            return null;
        }
    }
}
