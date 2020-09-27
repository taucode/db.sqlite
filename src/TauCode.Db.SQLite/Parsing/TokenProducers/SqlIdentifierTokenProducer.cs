using System.Collections.Generic;
using System.Linq;
using TauCode.Db.SQLite.Parsing.TextClasses;
using TauCode.Parsing;
using TauCode.Parsing.Exceptions;
using TauCode.Parsing.Lexing;
using TauCode.Parsing.TextDecorations;
using TauCode.Parsing.Tokens;

namespace TauCode.Db.SQLite.Parsing.TokenProducers
{
    public class SqlIdentifierTokenProducer : ITokenProducer
    {
        private static Dictionary<char, char> Delimiters { get; }
        private static HashSet<char> OpeningDelimiters { get; }
        private static HashSet<char> ClosingDelimiters { get; }
        private static Dictionary<char, char> ReverseDelimiters { get; }

        static SqlIdentifierTokenProducer()
        {
            Delimiters = new[]
            {
                "[]",
                "\"\"",
                "``",
            }
                .ToDictionary(x => x[0], x => x[1]);

            OpeningDelimiters = new HashSet<char>(Delimiters.Keys);
            ClosingDelimiters = new HashSet<char>(Delimiters.Values);

            ReverseDelimiters = Delimiters
                .ToDictionary(x => x.Value, x => x.Key);

        }

        public LexingContext Context { get; set; }

        public IToken Produce()
        {
            var context = this.Context;
            var text = context.Text;
            var length = text.Length;

            var c = text[context.Index];

            if (OpeningDelimiters.Contains(c) ||
                c == '_' ||
                LexingHelper.IsLatinLetter(c))
            {
                char? openingDelimiter = OpeningDelimiters.Contains(c) ? c : (char?)null;

                var initialIndex = context.Index;
                var index = initialIndex + 1;
                
                while (true)
                {
                    if (index == length)
                    {
                        if (openingDelimiter.HasValue)
                        {
                            var delta = index - initialIndex;
                            var column = context.Column + delta;

                            this.ThrowUnclosedIdentifierException(context.Line, column);
                        }
                        break;
                    }

                    c = text[index];

                    if (c == '_' || LexingHelper.IsLatinLetter(c) || LexingHelper.IsDigit(c))
                    {
                        index++;
                        continue;
                    }

                    if (ClosingDelimiters.Contains(c))
                    {
                        if (index - initialIndex > 2)
                        {
                            if (openingDelimiter.HasValue)
                            {
                                if (openingDelimiter.Value == ReverseDelimiters[c])
                                {
                                    index++;

                                    var delta = index - initialIndex;
                                    var column = context.Column + delta;

                                    var str = text.Substring(initialIndex + 1, delta - 2);
                                    var position = new Position(context.Line, context.Column);
                                    context.Advance(delta, 0, column);
                                    return new TextToken(
                                        SqlIdentifierTextClass.Instance,
                                        NoneTextDecoration.Instance,
                                        str,
                                        position,
                                        delta);
                                }
                                else
                                {
                                    var delta = index - initialIndex;
                                    var column = context.Column + delta;

                                    this.ThrowUnclosedIdentifierException(context.Line, column);
                                }
                            }
                            else
                            {
                                var delta = index - initialIndex;
                                var column = context.Column + delta;

                                throw new LexingException($"Unexpected delimiter: '{c}'.", new Position(context.Line, column));
                            }
                        }
                        else
                        {
                            var delta = index - initialIndex;
                            var column = context.Column + delta;
                            throw new LexingException($"Unexpected delimiter: '{c}'.", new Position(context.Line, column));
                        }
                    }
                }
            }

            return null;
        }

        private void ThrowUnclosedIdentifierException(int line, int column)
        {
            throw new LexingException("Unclosed identifier.", new Position(line, column));
        }
    }
}
