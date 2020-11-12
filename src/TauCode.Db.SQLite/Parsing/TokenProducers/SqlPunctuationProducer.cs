using TauCode.Extensions;
using TauCode.Parsing;
using TauCode.Parsing.Lexing;
using TauCode.Parsing.Tokens;

namespace TauCode.Db.SQLite.Parsing.TokenProducers
{
    public class SqlPunctuationProducer : ITokenProducer
    {
        public LexingContext Context { get; set; }

        public IToken Produce()
        {
            var context = this.Context;
            var text = context.Text;

            var c = text[context.Index];

            if (c.IsIn('(', ')', ','))
            {
                var index = context.Index;
                var position = new Position(context.Line, context.Column);
                context.AdvanceByChar();
                return new PunctuationToken(this.Context.Text[index], position, 1);
            }

            return null;
        }
    }
}
