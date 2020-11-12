using System;
using System.Linq;
using TauCode.Db.Model;
using TauCode.Extensions;
using TauCode.Parsing;
using TauCode.Parsing.Building;
using TauCode.Parsing.Lexing;
using TauCode.Parsing.Nodes;
using TauCode.Parsing.TinyLisp;
using TauCode.Parsing.Tokens;

namespace TauCode.Db.SQLite.Parsing
{
    public class SQLiteParser
    {
        public static SQLiteParser Instance { get; } = new SQLiteParser();

        private readonly INode _root;
        private readonly IParser _parser;

        private SQLiteParser()
        {
            _root = this.BuildRoot();
            _parser = new Parser
            {
                Root = _root,
            };
        }

        private INode BuildRoot()
        {
            var nodeFactory = new SQLiteNodeFactory();
            var input = this.GetType().Assembly.GetResourceText("grammar.lisp", true);
            ILexer lexer = new TinyLispLexer();
            var tokens = lexer.Lexize(input);

            var reader = new TinyLispPseudoReader();
            var list = reader.Read(tokens);

            ITreeBuilder builder = new TreeBuilder();
            var root = builder.Build(nodeFactory, list);

            this.ChargeRoot(root);

            return root;
        }

        private void ChargeRoot(INode root)
        {
            var allSqlNodes = root.FetchTree();

            var exactTextNodes = allSqlNodes
                .Where(x => x is ExactTextNode)
                .Cast<ExactTextNode>()
                .ToList();

            var reservedWords = exactTextNodes
                .Select(x => x.ExactText)
                .Distinct()
                .Select(x => x.ToUpperInvariant())
                .ToHashSet();

            var identifiersAsWords = allSqlNodes
                .Where(x =>
                    x is TextNode textNode &&
                    x.Name.EndsWith("-name", StringComparison.InvariantCultureIgnoreCase))
                .Cast<TextNode>()
                .ToList();

            #region assign job to nodes

            // table
            var createTable = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "do-create-table", StringComparison.InvariantCultureIgnoreCase));
            createTable.Action = (node, token, accumulator) =>
            {
                var tableMold = new TableMold();
                accumulator.AddResult(tableMold);
            };

            var tableName = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "table-name", StringComparison.InvariantCultureIgnoreCase));
            tableName.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                tableMold.Name = ((TextToken)token).Text;
            };

            var columnName = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "column-name", StringComparison.InvariantCultureIgnoreCase));
            columnName.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var columnMold = new ColumnMold
                {
                    Name = ((TextToken)token).Text,
                };
                tableMold.Columns.Add(columnMold);
            };

            var typeName = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "type-name", StringComparison.InvariantCultureIgnoreCase));
            typeName.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var columnMold = tableMold.Columns.Last();
                columnMold.Type.Name = ((TextToken)token).Text;
            };

            var precision = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "precision", StringComparison.InvariantCultureIgnoreCase));
            precision.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var columnMold = tableMold.Columns.Last();
                columnMold.Type.Precision = ((IntegerToken)token).Value.ToInt32();
            };

            var scale = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "scale", StringComparison.InvariantCultureIgnoreCase));
            scale.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var columnMold = tableMold.Columns.Last();
                columnMold.Type.Scale = ((IntegerToken)token).Value.ToInt32();
            };

            var nullToken = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "null", StringComparison.InvariantCultureIgnoreCase));
            nullToken.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var columnMold = tableMold.Columns.Last();
                columnMold.IsNullable = true;
            };

            var notNullToken = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "not-null", StringComparison.InvariantCultureIgnoreCase));
            notNullToken.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var columnMold = tableMold.Columns.Last();
                columnMold.IsNullable = false;
            };

            var inlinePrimaryKey = (ActionNode)allSqlNodes.Single(x =>
                string.Equals(x.Name, "inline-primary-key", StringComparison.InvariantCultureIgnoreCase));

            inlinePrimaryKey.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var columnMold = tableMold.Columns.Last();
                columnMold.Properties["#is_explicit_primary_key"] = "true";
            };

            var autoincrement = (ActionNode)allSqlNodes.Single(x =>
                string.Equals(x.Name, "autoincrement", StringComparison.InvariantCultureIgnoreCase));
            autoincrement.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var columnMold = tableMold.Columns.Last();
                columnMold.Identity = new ColumnIdentityMold("1", "1");
            };

            var defaultNull = (ActionNode)allSqlNodes.Single(x =>
                string.Equals(x.Name, "default-null", StringComparison.InvariantCultureIgnoreCase));
            defaultNull.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var columnMold = tableMold.Columns.Last();
                columnMold.Default = "NULL";
            };

            var defaultInteger = (ActionNode)allSqlNodes.Single(x =>
                string.Equals(x.Name, "default-integer", StringComparison.InvariantCultureIgnoreCase));
            defaultInteger.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var columnMold = tableMold.Columns.Last();
                columnMold.Default = ((IntegerToken)token).Value;
            };

            var defaultString = (ActionNode)allSqlNodes.Single(x =>
                string.Equals(x.Name, "default-string", StringComparison.InvariantCultureIgnoreCase));
            defaultString.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var columnMold = tableMold.Columns.Last();
                columnMold.Default = $"'{((TextToken)token).Text}'";
            };

            var constraintName = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "constraint-name", StringComparison.InvariantCultureIgnoreCase));
            constraintName.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                tableMold.SetLastConstraintName(((TextToken)token).Text);
            };

            var pk = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "do-primary-key", StringComparison.InvariantCultureIgnoreCase));
            pk.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                tableMold.PrimaryKey = new PrimaryKeyMold
                {
                    Name = tableMold.GetLastConstraintName(),
                };
            };

            var pkColumnName = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "pk-column-name", StringComparison.InvariantCultureIgnoreCase));
            pkColumnName.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var primaryKey = tableMold.PrimaryKey;
                primaryKey.Columns.Add(((TextToken)token).Text);
            };

            var pkColumnAscOrDesc = (ActionNode)allSqlNodes.Single(x =>
                string.Equals(x.Name, "pk-asc-or-desc", StringComparison.InvariantCultureIgnoreCase));

            var fk = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "do-foreign-key", StringComparison.InvariantCultureIgnoreCase));
            fk.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var foreignKey = new ForeignKeyMold
                {
                    Name = tableMold.GetLastConstraintName(),
                };
                tableMold.ForeignKeys.Add(foreignKey);
            };

            var fkTableName = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "fk-referenced-table-name", StringComparison.InvariantCultureIgnoreCase));
            fkTableName.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var foreignKey = tableMold.ForeignKeys.Last();
                var foreignKeyTableName = ((TextToken)token).Text;
                foreignKey.ReferencedTableName = foreignKeyTableName;
            };

            var fkColumnName = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "fk-column-name", StringComparison.InvariantCultureIgnoreCase));
            fkColumnName.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var foreignKey = tableMold.ForeignKeys.Last();
                var foreignKeyColumnName = ((TextToken)token).Text;
                foreignKey.ColumnNames.Add(foreignKeyColumnName);
            };

            var fkReferencedColumnName = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "fk-referenced-column-name", StringComparison.InvariantCultureIgnoreCase));
            fkReferencedColumnName.Action = (node, token, accumulator) =>
            {
                var tableMold = accumulator.GetLastResult<TableMold>();
                var foreignKey = tableMold.ForeignKeys.Last();
                var foreignKeyReferencedColumnName = ((TextToken)token).Text;
                foreignKey.ReferencedColumnNames.Add(foreignKeyReferencedColumnName);
            };

            // index
            var createUniqueIndex = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "do-create-unique-index", StringComparison.InvariantCultureIgnoreCase));
            createUniqueIndex.Action = (node, token, accumulator) =>
            {
                var index = new IndexMold
                {
                    IsUnique = true,
                };
                index.SetIsCreationFinalized(false);
                accumulator.AddResult(index);
            };

            var createIndex = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "do-create-index", StringComparison.InvariantCultureIgnoreCase));
            createIndex.Action = (node, token, accumulator) =>
            {
                bool brandNewIndex;

                if (accumulator.Count == 0)
                {
                    brandNewIndex = true;
                }
                else
                {
                    var result = accumulator.Last();
                    if (result is IndexMold indexMold)
                    {
                        brandNewIndex = indexMold.GetIsCreationFinalized();
                    }
                    else
                    {
                        brandNewIndex = true;
                    }
                }

                if (brandNewIndex)
                {
                    var newIndex = new IndexMold();
                    newIndex.SetIsCreationFinalized(true);
                    accumulator.AddResult(newIndex);
                }
                else
                {
                    var existingIndexMold = accumulator.GetLastResult<IndexMold>();
                    existingIndexMold.SetIsCreationFinalized(true);
                }
            };

            var indexName = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "index-name", StringComparison.InvariantCultureIgnoreCase));
            indexName.Action = (node, token, accumulator) =>
            {
                var index = accumulator.GetLastResult<IndexMold>();
                index.Name = ((TextToken)token).Text;
            };

            var indexTableName = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "index-table-name", StringComparison.InvariantCultureIgnoreCase));
            indexTableName.Action = (node, token, accumulator) =>
            {
                var index = accumulator.GetLastResult<IndexMold>();
                index.TableName = ((TextToken)token).Text;
            };

            var indexColumnName = (ActionNode)allSqlNodes.Single(x =>
               string.Equals(x.Name, "index-column-name", StringComparison.InvariantCultureIgnoreCase));
            indexColumnName.Action = (node, token, accumulator) =>
            {
                var index = accumulator.GetLastResult<IndexMold>();
                var columnMold = new IndexColumnMold
                {
                    Name = ((TextToken)token).Text,
                };
                index.Columns.Add(columnMold);
            };

            var indexColumnAscOrDesc = (ActionNode)allSqlNodes.Single(x =>
                string.Equals(x.Name, "index-column-asc-or-desc", StringComparison.InvariantCultureIgnoreCase));
            indexColumnAscOrDesc.Action = (node, token, accumulator) =>
            {
                var index = accumulator.GetLastResult<IndexMold>();
                var columnInfo = index.Columns.Last();

                var ascOrDesc = ((TextToken)token).Text.ToLowerInvariant();
                columnInfo.SortDirection = SQLiteParsingHelper.SqlToSortDirection(ascOrDesc);
            };

            #endregion

            foreach (var identifiersAsWord in identifiersAsWords)
            {
                identifiersAsWord.AdditionalChecker = (token, accumulator) =>
                {
                    var wordToken = ((TextToken)token).Text.ToUpperInvariant();
                    return !reservedWords.Contains(wordToken);
                };
            }
        }

        public object[] Parse(string sql)
        {
            ILexer lexer = new SQLiteLexer();
            var tokens = lexer.Lexize(sql);

            var results = _parser.Parse(tokens);
            return results;
        }
    }
}
