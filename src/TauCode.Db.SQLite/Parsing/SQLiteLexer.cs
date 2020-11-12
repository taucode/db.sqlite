using TauCode.Db.SQLite.Parsing.TokenProducers;
using TauCode.Extensions;
using TauCode.Parsing.Lexing;
using TauCode.Parsing.Lexing.StandardProducers;

namespace TauCode.Db.SQLite.Parsing
{
    public class SQLiteLexer : LexerBase
    {
        protected override ITokenProducer[] CreateProducers()
        {
            return new ITokenProducer[]
            {
                new WhiteSpaceProducer(),
                new WordProducer(),
                new SqlPunctuationProducer(),
                new IntegerProducer(IsAcceptableIntegerTerminator),
                new SqlStringProducer(),
                new SqlIdentifierProducer(),
            };
        }

        private bool IsAcceptableIntegerTerminator(char c)
        {
            if (LexingHelper.IsInlineWhiteSpaceOrCaretControl(c))
            {
                return true;
            }

            if (c.IsIn('(', ')', ','))
            {
                return true;
            }

            return false;
        }
    }
}
