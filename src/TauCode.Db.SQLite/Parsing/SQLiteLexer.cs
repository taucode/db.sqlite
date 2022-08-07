using TauCode.Data.Text;
using TauCode.Extensions;
using TauCode.Parsing;
using TauCode.Parsing.TokenProducers;

namespace TauCode.Db.SQLite.Parsing;

internal class SQLiteLexer : Lexer
{
    internal SQLiteLexer()
    {
        Producers = new ILexicalTokenProducer[]
        {
            new WhiteSpaceProducer(),
            new Int32Producer(IntegerTerminator),
            new WordProducer(WordTerminator),
            new PunctuationProducer(
                new char[]
                {
                    '(',
                    ')',
                    ',',
                },
                (input, index) => true),
            new SqlIdentifierProducer(
                SQLiteParsingHelper.IsReservedWord,
                WordTerminator)
            {
                Delimiter =
                    SqlIdentifierDelimiter.None |
                    SqlIdentifierDelimiter.Brackets |
                    SqlIdentifierDelimiter.DoubleQuotes |
                    0,
            },
            new SqlStringProducer(WordTerminator),
        };
    }

    private static bool WordTerminator(ReadOnlySpan<char> input, int index)
    {
        var c = input[index];
        var result = c.IsIn(' ', '\t', '\r', '\n', ',', '(', ')'); // todo: hashset
        return result;
    }

    private static bool IntegerTerminator(ReadOnlySpan<char> input, int index)
    {
        var c = input[index];
        var result = c.IsIn(' ', '\t', '\r', '\n', ',', '(', ')'); // todo: hashset
        return result;
    }
}