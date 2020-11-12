using System;
using System.Collections.Generic;
using System.Linq;
using TauCode.Db.Model;
using TauCode.Extensions;
using TauCode.Parsing;
using TauCode.Parsing.Building;
using TauCode.Parsing.Lexing;
using TauCode.Parsing.Nodes;
using TauCode.Parsing.TinyLisp;

namespace TauCode.Db.SQLite.Parsing
{
    internal static class SQLiteParsingHelper
    {
        private static readonly ILexer TheLexer = new TinyLispLexer();
        private static readonly HashSet<string> ReservedWordsHashSet;

        public static HashSet<string> ReservedWords = ReservedWordsHashSet ??= CreateReservedWords();

        private static HashSet<string> CreateReservedWords()
        {
            var grammar = typeof(SQLiteParsingHelper).Assembly.GetResourceText("grammar.lisp", true);
            var tokens = TheLexer.Lexize(grammar);

            var reader = new TinyLispPseudoReader();
            var form = reader.Read(tokens);

            var nodeFactory = new SQLiteNodeFactory();
            var builder = new TreeBuilder();
            var root = builder.Build(nodeFactory, form);
            var nodes = root.FetchTree();

            var words = new List<string>();

            words.AddRange(nodes
                .Where(x => x is ExactTextNode)
                .Cast<ExactTextNode>()
                .Select(x => x.ExactText.ToLowerInvariant()));

            words.AddRange(nodes
                .Where(x => x is MultiTextNode)
                .Cast<MultiTextNode>()
                .SelectMany(x => x.Texts.Select(y => y.ToLowerInvariant())));

            return new HashSet<string>(words);
        }

        public static bool IsReservedWord(string text) => ReservedWords.Contains(text.ToLowerInvariant());

        public static SortDirection SqlToSortDirection(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            switch (sql.ToLowerInvariant())
            {
                case "asc":
                    return SortDirection.Ascending;

                case "desc":
                    return SortDirection.Descending;

                default:
                    throw new ArgumentException($"Unknown sort direction: '{sql}'.", nameof(sql));
            }
        }

        internal static void SetLastConstraintName(this TableMold tableMold, string value)
        {
            tableMold.Properties["#last-constraint-name"] = value;
        }

        internal static string GetLastConstraintName(this TableMold tableMold)
        {
            return tableMold.Properties["#last-constraint-name"];
        }

        internal static void SetIsCreationFinalized(this IndexMold indexMold, bool value)
        {
            indexMold.Properties["#is-creation-finalized"] = value.ToString();
        }

        internal static bool GetIsCreationFinalized(this IndexMold indexMold)
        {
            return bool.Parse(indexMold.Properties["#is-creation-finalized"]);
        }
    }
}
