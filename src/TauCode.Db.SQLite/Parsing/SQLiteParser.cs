using TauCode.Data.Graphs;
using TauCode.Db.Model;
using TauCode.Extensions;
using TauCode.Parsing;
using TauCode.Parsing.Graphs.Building.Impl;
using TauCode.Parsing.Nodes;
using TauCode.Parsing.Tokens;

namespace TauCode.Db.SQLite.Parsing;

internal class SQLiteParser
{
    internal static SQLiteParser Instance { get; } = new SQLiteParser();

    private readonly IParser _parser;
    private readonly ILexer _lexer;

    private SQLiteParser()
    {
        var reader = new SQLiteGraphScriptReader();
        var script = this.GetType().Assembly.GetResourceText("grammar.lisp", true);
        var groupMold = reader.ReadScript(script.AsMemory());
        var vertexFactory = new SQLiteVertexFactory();
        var graphBuilder = new GraphBuilder(vertexFactory);

        var graph = graphBuilder.Build(groupMold);

        this.SetUpActions(graph);

        _parser = new Parser
        {
            Root = (IdleNode)graph.Single(x => x.Name == "root-node"),
            AllowsMultipleExecutions = true,
        };

        _lexer = new SQLiteLexer();
    }

    private void SetUpActions(IGraph graph)
    {
        var actionNodes = graph
            .Where(x => x is ActionNode)
            .Cast<ActionNode>()
            .ToList();

        // create
        var createNode = actionNodes.Single(x => x.Name == "create");
        createNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // create table
        var createTableNode = actionNodes.Single(x => x.Name == "do-create-table");
        createTableNode.Action = (node, context) =>
        {
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;

            var tableMold = new TableMold();
            parsingResult.Clauses.Add(tableMold);

            parsingResult.IncreaseVersion();
        };

        // table name
        var tableNameNode = actionNodes.Single(x => x.Name == "table-name");
        tableNameNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            string tableName;

            if (token is SqlIdentifierToken sqlIdentifierToken)
            {
                tableName = sqlIdentifierToken.Value.Value;
            }
            else
            {
                tableName = token.ToString();
            }

            var parsingResult = (SQLiteParsingResult)context.ParsingResult;

            var tableMold = parsingResult.GetLastClause<TableMold>();
            tableMold.Name = tableName;

            parsingResult.IncreaseVersion();
        };

        // table opening
        var tableOpeningNode = actionNodes.Single(x => x.Name == "table-opening");
        tableOpeningNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // column-name
        var columnNameNode = actionNodes.Single(x => x.Name == "column-name");
        columnNameNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();

            string columnName;

            if (token is SqlIdentifierToken sqlIdentifierToken)
            {
                columnName = sqlIdentifierToken.Value.Value;
            }
            else
            {
                columnName = token.ToString();
            }

            var columnMold = new ColumnMold
            {
                Name = columnName,
            };

            tableMold.Columns.Add(columnMold);
            parsingResult.IncreaseVersion();
        };

        // type-name
        var typeNameNode = actionNodes.Single(x => x.Name == "type-name");
        typeNameNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();
            var columnMold = tableMold.Columns[^1];
            columnMold.Type.Name = token.ToString();

            parsingResult.IncreaseVersion();
        };

        // not
        var notNode = actionNodes.Single(x => x.Name == "not");
        notNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // not null
        var notNullNode = actionNodes.Single(x => x.Name == "not-null");
        notNullNode.Action = (node, context) =>
        {
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();
            var columnMold = tableMold.Columns[^1];
            columnMold.IsNullable = false;

            context.ParsingResult.IncreaseVersion();
        };

        // primary
        var primaryNode = actionNodes.Single(x => x.Name == "inline-primary");
        primaryNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // primary key
        var primaryKeyNode = actionNodes.Single(x => x.Name == "inline-primary-key");
        primaryKeyNode.Action = (node, context) =>
        {
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();
            var columnMold = tableMold.Columns.Last();
            columnMold.Properties["#is_explicit_primary_key"] = "true";

            context.ParsingResult.IncreaseVersion();
        };

        // comma after column def
        var commaAfterColumnDef = actionNodes.Single(x => x.Name == "comma-after-column-def");
        commaAfterColumnDef.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // null
        var nullNode = actionNodes.Single(x => x.Name == "null");
        nullNode.Action = (node, context) =>
        {
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();
            var columnMold = tableMold.Columns[^1];
            columnMold.IsNullable = true;

            context.ParsingResult.IncreaseVersion();
        };

        // autoincrement
        var autoincrementNode = actionNodes.Single(x => x.Name == "autoincrement");
        autoincrementNode.Action = (node, context) =>
        {
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();
            var columnMold = tableMold.Columns[^1];
            columnMold.Identity = new ColumnIdentityMold("1", "1");

            context.ParsingResult.IncreaseVersion();
        };

        // default
        var defaultNode = actionNodes.Single(x => x.Name == "default");
        defaultNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // default null
        var defaultNullNode = actionNodes.Single(x => x.Name == "default-null");
        defaultNullNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();
            var columnMold = tableMold.Columns[^1];

            columnMold.Default = token.ToString();

            context.ParsingResult.IncreaseVersion();
        };

        // default integer
        var defaultIntegerNode = actionNodes.Single(x => x.Name == "default-integer");
        defaultIntegerNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();
            var columnMold = tableMold.Columns[^1];

            columnMold.Default = token.ToString();

            context.ParsingResult.IncreaseVersion();
        };

        // default string
        var defaultStringNode = actionNodes.Single(x => x.Name == "default-string");
        defaultStringNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();
            var columnMold = tableMold.Columns[^1];

            columnMold.Default = token.ToString();

            context.ParsingResult.IncreaseVersion();
        };

        // table closing
        var tableClosingNode = actionNodes.Single(x => x.Name == "table-closing");
        tableClosingNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // constraint
        var constraintNode = actionNodes.Single(x => x.Name == "constraint");
        constraintNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // constraint name
        var constraintNameNode = actionNodes.Single(x => x.Name == "constraint-name");
        constraintNameNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();

            string constraintName;

            if (token is SqlIdentifierToken sqlIdentifierToken)
            {
                constraintName = sqlIdentifierToken.Value.Value;
            }
            else
            {
                constraintName = token.ToString();
            }

            tableMold.SetLastConstraintName(constraintName);

            context.ParsingResult.IncreaseVersion();
        };

        // do primary
        var doPrimaryNode = actionNodes.Single(x => x.Name == "do-primary");
        doPrimaryNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // do primary key
        var doPrimaryKeyNode = actionNodes.Single(x => x.Name == "do-primary-key");
        doPrimaryKeyNode.Action = (node, context) =>
        {
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();

            tableMold.PrimaryKey = new PrimaryKeyMold
            {
                Name = tableMold.GetLastConstraintName(),
            };

            context.ParsingResult.IncreaseVersion();
        };

        // pk-opening
        var pkOpeningNode = actionNodes.Single(x => x.Name == "pk-opening");
        pkOpeningNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // pk column name
        var pkColumnNameNode = actionNodes.Single(x => x.Name == "pk-column-name");
        pkColumnNameNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();
            var primaryKey = tableMold.PrimaryKey;

            string columnName;

            if (token is SqlIdentifierToken sqlIdentifierToken)
            {
                columnName = sqlIdentifierToken.Value.Value;
            }
            else
            {
                columnName = token.ToString();
            }

            primaryKey.Columns.Add(columnName);

            context.ParsingResult.IncreaseVersion();
        };

        // comma more pk columns
        var commaMorePkColumnsNode = actionNodes.Single(x => x.Name == "comma-more-pk-columns");
        commaMorePkColumnsNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // pk closing
        var pkClosingNode = actionNodes.Single(x => x.Name == "pk-closing");
        pkClosingNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // comma after constraint
        var commaAfterConstraintNode = actionNodes.Single(x => x.Name == "comma-after-constraint");
        commaAfterConstraintNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // do foreign
        var doForeignNode = actionNodes.Single(x => x.Name == "do-foreign");
        doForeignNode.Action = (node, context) =>
        {
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();
            var foreignKey = new ForeignKeyMold
            {
                Name = tableMold.GetLastConstraintName(),
            };

            tableMold.ForeignKeys.Add(foreignKey);

            context.ParsingResult.IncreaseVersion();
        };

        // do foreign key
        var doForeignKeyNode = actionNodes.Single(x => x.Name == "do-foreign-key");
        doForeignKeyNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // fk opening
        var doFkOpeningNode = actionNodes.Single(x => x.Name == "fk-opening");
        doFkOpeningNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // fk column name
        var fkColumnNameNode = actionNodes.Single(x => x.Name == "fk-column-name");
        fkColumnNameNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();
            var foreignKey = tableMold.ForeignKeys[^1];

            string fkColumnName;

            if (token is SqlIdentifierToken sqlIdentifierToken)
            {
                fkColumnName = sqlIdentifierToken.Value.Value;
            }
            else
            {
                fkColumnName = token.ToString();
            }
            foreignKey.ColumnNames.Add(fkColumnName);

            context.ParsingResult.IncreaseVersion();
        };

        // comma after fk column name
        var commaAfterFkColumnNameNode = actionNodes.Single(x => x.Name == "comma-after-fk-column-name");
        commaAfterFkColumnNameNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // fk closing
        var fkClosingNode = actionNodes.Single(x => x.Name == "fk-closing");
        fkClosingNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // fk references
        var fkReferencesNode = actionNodes.Single(x => x.Name == "fk-references");
        fkReferencesNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // fk referenced table name
        var fkReferencedTableNameNode = actionNodes.Single(x => x.Name == "fk-referenced-table-name");
        fkReferencedTableNameNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();
            var foreignKey = tableMold.ForeignKeys[^1];

            string fkReferencedTableName;

            if (token is SqlIdentifierToken sqlIdentifierToken)
            {
                fkReferencedTableName = sqlIdentifierToken.Value.Value;
            }
            else
            {
                fkReferencedTableName = token.ToString();
            }

            foreignKey.ReferencedTableName = fkReferencedTableName;

            context.ParsingResult.IncreaseVersion();
        };

        // fk referenced opening
        var fkReferencedOpeningNode = actionNodes.Single(x => x.Name == "fk-referenced-opening");
        fkReferencedOpeningNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // fk referenced column name
        var fkReferencedColumnNameNode = actionNodes.Single(x => x.Name == "fk-referenced-column-name");
        fkReferencedColumnNameNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var tableMold = parsingResult.GetLastClause<TableMold>();
            var foreignKey = tableMold.ForeignKeys[^1];

            string fkReferencedColumnName;

            if (token is SqlIdentifierToken sqlIdentifierToken)
            {
                fkReferencedColumnName = sqlIdentifierToken.Value.Value;
            }
            else
            {
                fkReferencedColumnName = token.ToString();
            }

            foreignKey.ReferencedColumnNames.Add(fkReferencedColumnName);

            context.ParsingResult.IncreaseVersion();
        };

        // comma after referenced column name
        var commaAfterReferencedColumnNameNode = actionNodes.Single(x => x.Name == "comma-after-referenced-column-name");
        commaAfterReferencedColumnNameNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // fk referenced closing
        var fkReferencedClosingNode = actionNodes.Single(x => x.Name == "fk-referenced-closing");
        fkReferencedClosingNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // do create unique
        var doCreateUniqueNode = actionNodes.Single(x => x.Name == "do-create-unique");
        doCreateUniqueNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // do create unique index
        var doCreateUniqueNodeIndex = actionNodes.Single(x => x.Name == "do-create-unique-index");
        doCreateUniqueNodeIndex.Action = (node, context) =>
        {
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;

            var index = new IndexMold
            {
                IsUnique = true,
            };

            parsingResult.Clauses.Add(index);

            context.ParsingResult.IncreaseVersion();
        };

        // do create non unique index
        var doCreateNonUniqueIndexNode = actionNodes.Single(x => x.Name == "do-create-non-unique-index");
        doCreateNonUniqueIndexNode.Action = (node, context) =>
        {
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;

            var index = new IndexMold
            {
                IsUnique = false,
            };

            parsingResult.Clauses.Add(index);

            context.ParsingResult.IncreaseVersion();
        };

        // index name
        var indexNameNode = actionNodes.Single(x => x.Name == "index-name");
        indexNameNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var index = parsingResult.GetLastClause<IndexMold>();

            string indexName;

            if (token is SqlIdentifierToken sqlIdentifierToken)
            {
                indexName = sqlIdentifierToken.Value.Value;
            }
            else
            {
                indexName = token.ToString();
            }

            index.Name = indexName;

            context.ParsingResult.IncreaseVersion();
        };

        // index on
        var indexOnNode = actionNodes.Single(x => x.Name == "index-on");
        indexOnNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // index table name
        var indexTableNameNode = actionNodes.Single(x => x.Name == "index-table-name");
        indexTableNameNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var index = parsingResult.GetLastClause<IndexMold>();

            string indexTableName;

            if (token is SqlIdentifierToken sqlIdentifierToken)
            {
                indexTableName = sqlIdentifierToken.Value.Value;
            }
            else
            {
                indexTableName = token.ToString();
            }

            index.TableName = indexTableName;

            context.ParsingResult.IncreaseVersion();
        };

        // index columns opening
        var indexColumnsOpeningNode = actionNodes.Single(x => x.Name == "index-columns-opening");
        indexColumnsOpeningNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // index column name
        var indexColumnNameNode = actionNodes.Single(x => x.Name == "index-column-name");
        indexColumnNameNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var index = parsingResult.GetLastClause<IndexMold>();

            string indexColumnName;

            if (token is SqlIdentifierToken sqlIdentifierToken)
            {
                indexColumnName = sqlIdentifierToken.Value.Value;
            }
            else
            {
                indexColumnName = token.ToString();
            }

            var indexColumnMold = new IndexColumnMold
            {
                Name = indexColumnName,
            };

            index.Columns.Add(indexColumnMold);

            context.ParsingResult.IncreaseVersion();
        };

        // index column asc or desc
        var indexColumnAscOrDescNode = actionNodes.Single(x => x.Name == "index-column-asc-or-desc");
        indexColumnAscOrDescNode.Action = (node, context) =>
        {
            var token = context.GetCurrentToken();
            var parsingResult = (SQLiteParsingResult)context.ParsingResult;
            var index = parsingResult.GetLastClause<IndexMold>();

            var indexColumn = index.Columns[^1];
            var ascOrDesc = token.ToString().ToLowerInvariant();
            indexColumn.SortDirection = SQLiteParsingHelper.SqlToSortDirection(ascOrDesc);

            context.ParsingResult.IncreaseVersion();
        };

        // comma after index column name
        var commaAfterIndexColumnNameNode = actionNodes.Single(x => x.Name == "comma-after-index-column-name");
        commaAfterIndexColumnNameNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };

        // index columns closing
        var indexColumnsClosingNode = actionNodes.Single(x => x.Name == "index-columns-closing");
        indexColumnsClosingNode.Action = (node, context) =>
        {
            // todo: don't need. use IdleAction.
            context.ParsingResult.IncreaseVersion();
        };
    }

    internal IList<object> Parse(string sql)
    {
        if (sql == null)
        {
            throw new ArgumentNullException(nameof(sql));
        }

        var tokens = _lexer.Tokenize(sql.AsMemory());
        var parsingResult = new SQLiteParsingResult();
        _parser.Parse(tokens, parsingResult);

        return parsingResult.Clauses;
    }
}