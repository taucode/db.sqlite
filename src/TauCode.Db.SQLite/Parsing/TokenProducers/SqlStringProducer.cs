using System.Text;
using TauCode.Parsing;
using TauCode.Parsing.Lexing;
using TauCode.Parsing.TextClasses;
using TauCode.Parsing.TextDecorations;
using TauCode.Parsing.Tokens;

namespace TauCode.Db.SQLite.Parsing.TokenProducers
{
    public class SqlStringProducer : ITokenProducer
    {
        public IToken Produce()
        {
            var context = this.Context;
            var text = context.Text;
            var length = text.Length;

            var c = text[context.Index];

            if (c == '\'')
            {
                var initialIndex = context.Index;
                var initialLine = context.Line;

                var index = initialIndex + 1; // skip '''

                int delta;
                var sb = new StringBuilder();

                while (true)
                {
                    if (index == length)
                    {
                        var column = context.Column + (index - initialIndex);
                        throw LexingHelper.CreateUnclosedStringException(new Position(
                            initialLine,
                            column));
                    }

                    c = text[index];

                    if (LexingHelper.IsCaretControl(c))
                    {
                        var column = context.Column + (index - initialIndex);
                        throw LexingHelper.CreateNewLineInStringException(new Position(initialLine, column));
                    }

                    index++;

                    if (c == '\'')
                    {
                        break;
                    }

                    sb.Append(c);
                }

                delta = index - initialIndex;
                var str = sb.ToString();


                var token = new TextToken(
                    StringTextClass.Instance,
                    SingleQuoteTextDecoration.Instance,
                    str,
                    new Position(context.Line, context.Column),
                    delta);

                context.Advance(delta, 0, context.Column + delta);
                return token;
            }

            return null;
        }

        public LexingContext Context { get; set; }
    }
}
