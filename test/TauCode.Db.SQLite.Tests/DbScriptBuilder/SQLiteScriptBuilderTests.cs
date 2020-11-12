using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TauCode.Db.Exceptions;
using TauCode.Db.Model;
using TauCode.Extensions;

namespace TauCode.Db.SQLite.Tests.DbScriptBuilder
{
    [TestFixture]
    public class SQLiteScriptBuilderTests : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            var sql = this.GetType().Assembly.GetResourceText("crebase.sql", true);
            this.Connection.ExecuteCommentedScript(sql);
        }

        private void AssertCorruptedTableAction(Action<TableMold> action)
        {
            // Arrange
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "HealthInfo");
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            var table = tableInspector.GetTable();

            // Act

            // corrupted: table name is null
            var table1 = (TableMold)table.Clone();
            table1.Name = null;
            var ex1 = Assert.Throws<ArgumentException>(() => action(table1));

            // corrupted: columns is null
            var table2 = (TableMold)table.Clone();
            table2.Columns = null;
            var ex2 = Assert.Throws<ArgumentException>(() => action(table2));

            // corrupted: column is null
            var table3 = (TableMold)table.Clone();
            table3.Columns[0] = null;
            var ex3 = Assert.Throws<ArgumentException>(() => action(table3));

            // corrupted: columns is empty
            var table4 = (TableMold)table.Clone();
            table4.Columns.Clear();
            var ex4 = Assert.Throws<ArgumentException>(() => action(table4));

            // corrupted: column name is null
            var table5 = (TableMold)table.Clone();
            table5.Columns[0].Name = null;
            var ex5 = Assert.Throws<ArgumentException>(() => action(table5));

            // corrupted: column type is null
            var table6 = (TableMold)table.Clone();
            table6.Columns[0].Type = null;
            var ex6 = Assert.Throws<ArgumentException>(() => action(table6));

            // corrupted: type name is null
            var table7 = (TableMold)table.Clone();
            table7.Columns[0].Type.Name = null;
            var ex7 = Assert.Throws<ArgumentException>(() => action(table7));

            // corrupted: only type scale is provided
            var table8 = (TableMold)table.Clone();
            table8.Columns[0].Type.Size = null;
            table8.Columns[0].Type.Precision = null;
            table8.Columns[0].Type.Scale = 1;
            var ex8 = Assert.Throws<ArgumentException>(() => action(table8));

            // corrupted: type size and scale are provided
            var table9 = (TableMold)table.Clone();
            table9.Columns[0].Type.Size = 1;
            table9.Columns[0].Type.Precision = null;
            table9.Columns[0].Type.Scale = 1;
            var ex9 = Assert.Throws<ArgumentException>(() => action(table9));

            // corrupted: type size and precision are provided
            var table10 = (TableMold)table.Clone();
            table10.Columns[0].Type.Size = 1;
            table10.Columns[0].Type.Precision = null;
            table10.Columns[0].Type.Scale = 1;
            var ex10 = Assert.Throws<ArgumentException>(() => action(table10));

            // corrupted: type size, precision and scale are provided
            var table11 = (TableMold)table.Clone();
            table11.Columns[0].Type.Size = 1;
            table11.Columns[0].Type.Precision = null;
            table11.Columns[0].Type.Scale = 1;
            var ex11 = Assert.Throws<ArgumentException>(() => action(table11));

            // corrupted: identity seed is null
            var table12 = (TableMold)table.Clone();
            table12.Columns[0].Identity = new ColumnIdentityMold
            {
                Seed = 1.ToString(),
                Increment = 1.ToString(),
            };
            table12.Columns[0].Identity.Seed = null;
            var ex12 = Assert.Throws<ArgumentException>(() => action(table12));

            // corrupted: identity increment is null
            var table13 = (TableMold)table.Clone();
            table13.Columns[0].Identity = new ColumnIdentityMold
            {
                Seed = 1.ToString(),
                Increment = 1.ToString(),
            };
            table13.Columns[0].Identity.Increment = null;
            var ex13 = Assert.Throws<ArgumentException>(() => action(table13));

            // corrupted: pk name is null
            var table14 = (TableMold)table.Clone();
            table14.PrimaryKey.Name = null;
            var ex14 = Assert.Throws<ArgumentException>(() => action(table14));

            // corrupted: pk columns is null
            var table15 = (TableMold)table.Clone();
            table15.PrimaryKey.Columns = null;
            var ex15 = Assert.Throws<ArgumentException>(() => action(table15));

            // corrupted: pk columns is empty
            var table16 = (TableMold)table.Clone();
            table16.PrimaryKey.Columns.Clear();
            var ex16 = Assert.Throws<ArgumentException>(() => action(table16));

            // corrupted: pk columns contain nulls
            var table17 = (TableMold)table.Clone();
            table17.PrimaryKey.Columns[0] = null;
            var ex17 = Assert.Throws<ArgumentException>(() => action(table17));

            // corrupted: fks is null
            var table18 = (TableMold)table.Clone();
            table18.ForeignKeys = null;
            var ex18 = Assert.Throws<ArgumentException>(() => action(table18));

            // corrupted: fks contains nulls
            var table19 = (TableMold)table.Clone();
            table19.ForeignKeys[0] = null;
            var ex19 = Assert.Throws<ArgumentException>(() => action(table19));

            // corrupted: fk name is null
            var table20 = (TableMold)table.Clone();
            table20.ForeignKeys[0].Name = null;
            var ex20 = Assert.Throws<ArgumentException>(() => action(table20));

            // corrupted: fk column names is null
            var table21 = (TableMold)table.Clone();
            table21.ForeignKeys[0].ColumnNames = null;
            var ex21 = Assert.Throws<ArgumentException>(() => action(table21));

            // corrupted: fk column names is empty
            var table22 = (TableMold)table.Clone();
            table22.ForeignKeys[0].ColumnNames.Clear();
            var ex22 = Assert.Throws<ArgumentException>(() => action(table22));

            // corrupted: fk column name is null
            var table23 = (TableMold)table.Clone();
            table23.ForeignKeys[0].ColumnNames[0] = null;
            var ex23 = Assert.Throws<ArgumentException>(() => action(table23));

            // corrupted: fk referenced table name is null
            var table24 = (TableMold)table.Clone();
            table24.ForeignKeys[0].ReferencedTableName = null;
            var ex24 = Assert.Throws<ArgumentException>(() => action(table24));

            // corrupted: fk referenced column names is null
            var table25 = (TableMold)table.Clone();
            table25.ForeignKeys[0].ReferencedColumnNames = null;
            var ex25 = Assert.Throws<ArgumentException>(() => action(table25));

            // corrupted: fk referenced column names contains null
            var table26 = (TableMold)table.Clone();
            table26.ForeignKeys[0].ReferencedColumnNames[0] = null;
            var ex26 = Assert.Throws<ArgumentException>(() => action(table26));

            // corrupted: fk column names and referenced column names have different count
            var table27 = (TableMold)table.Clone();
            table27.ForeignKeys[0].ReferencedColumnNames.Add("extra_column");
            var ex27 = Assert.Throws<ArgumentException>(() => action(table27));

            // corrupted: indexes is null
            var table28 = (TableMold)table.Clone();
            table28.Indexes = null;
            var ex28 = Assert.Throws<ArgumentException>(() => action(table28));

            // corrupted: indexes contain null
            var table29 = (TableMold)table.Clone();
            table29.Indexes[0] = null;
            var ex29 = Assert.Throws<ArgumentException>(() => action(table29));

            // corrupted: index name is null
            var table30 = (TableMold)table.Clone();
            table30.Indexes[0].Name = null;
            var ex30 = Assert.Throws<ArgumentException>(() => action(table30));

            // corrupted: index table name is null
            var table31 = (TableMold)table.Clone();
            table31.Indexes[0].TableName = null;
            var ex31 = Assert.Throws<ArgumentException>(() => action(table31));

            // corrupted: index columns is null
            var table32 = (TableMold)table.Clone();
            table32.Indexes[0].Columns = null;
            var ex32 = Assert.Throws<ArgumentException>(() => action(table32));

            // corrupted: index columns is empty
            var table33 = (TableMold)table.Clone();
            table33.Indexes[0].Columns.Clear();
            var ex33 = Assert.Throws<ArgumentException>(() => action(table33));

            // corrupted: index columns contains null
            var table34 = (TableMold)table.Clone();
            table34.Indexes[0].Columns[0] = null;
            var ex34 = Assert.Throws<ArgumentException>(() => action(table34));

            // corrupted: index column name is null
            var table35 = (TableMold)table.Clone();
            table35.Indexes[0].Columns[0].Name = null;
            var ex35 = Assert.Throws<ArgumentException>(() => action(table35));

            // Assert
            Assert.That(ex1, Has.Message.StartsWith("Table name cannot be null."));
            Assert.That(ex2, Has.Message.StartsWith("Table columns cannot be null."));
            Assert.That(ex3, Has.Message.StartsWith("Table columns cannot contain nulls."));
            Assert.That(ex4, Has.Message.StartsWith("Table columns cannot be empty."));
            Assert.That(ex5, Has.Message.StartsWith("Column name cannot be null."));
            Assert.That(ex6, Has.Message.StartsWith("Column type cannot be null."));
            Assert.That(ex7, Has.Message.StartsWith("Type name cannot be null."));
            Assert.That(ex8, Has.Message.StartsWith("If type scale is provided, precision must be provided as well."));
            Assert.That(
                ex9,
                Has.Message.StartsWith("If type size is provided, neither precision nor scale cannot be provided."));
            Assert.That(
                ex10,
                Has.Message.StartsWith("If type size is provided, neither precision nor scale cannot be provided."));
            Assert.That(
                ex11,
                Has.Message.StartsWith("If type size is provided, neither precision nor scale cannot be provided."));
            Assert.That(ex12, Has.Message.StartsWith("Identity seed cannot be null."));
            Assert.That(ex13, Has.Message.StartsWith("Identity increment cannot be null."));
            Assert.That(ex14, Has.Message.StartsWith("Primary key's name cannot be null."));
            Assert.That(ex15, Has.Message.StartsWith("Primary key's columns cannot be null."));
            Assert.That(ex16, Has.Message.StartsWith("Primary key's columns cannot be empty."));
            Assert.That(ex17, Has.Message.StartsWith("Primary key's columns cannot contain nulls."));
            Assert.That(ex18, Has.Message.StartsWith("Table foreign keys list cannot be null."));
            Assert.That(ex19, Has.Message.StartsWith("Table foreign keys cannot contain nulls."));
            Assert.That(ex20, Has.Message.StartsWith("Foreign key name cannot be null."));
            Assert.That(ex21, Has.Message.StartsWith("Foreign key column names collection cannot be null."));
            Assert.That(ex22, Has.Message.StartsWith("Foreign key column names collection cannot be empty."));
            Assert.That(ex23, Has.Message.StartsWith("Foreign key column names cannot contain nulls."));
            Assert.That(ex24, Has.Message.StartsWith("Foreign key's referenced table name cannot be null."));
            Assert.That(ex25, Has.Message.StartsWith("Foreign key referenced column names collection cannot be null."));
            Assert.That(ex26, Has.Message.StartsWith("Foreign key referenced column names cannot contain nulls."));
            Assert.That(
                ex27,
                Has.Message.StartsWith("Foreign key's column name count does not match referenced column name count."));
            Assert.That(ex28, Has.Message.StartsWith("Table indexes list cannot be null."));
            Assert.That(ex29, Has.Message.StartsWith("Table indexes cannot contain nulls."));
            Assert.That(ex30, Has.Message.StartsWith("Index name cannot be null."));
            Assert.That(ex31, Has.Message.StartsWith("Index table name cannot be null."));
            Assert.That(ex32, Has.Message.StartsWith("Index columns cannot be null."));
            Assert.That(ex33, Has.Message.StartsWith("Index columns cannot be empty."));
            Assert.That(ex34, Has.Message.StartsWith("Index columns cannot contain nulls."));
            Assert.That(ex35, Has.Message.StartsWith("Index column name cannot be null."));

            #region check arg name

            ArgumentException[] exceptions = new[]
            {
                ex1,
                ex2,
                ex3,
                ex4,
                ex5,
                ex6,
                ex7,
                ex8,
                ex9,
                ex10,
                ex11,
                ex12,
                ex13,
                ex14,
                ex15,
                ex16,
                ex17,
                ex18,
                ex19,
                ex20,
                ex21,
                ex22,
                ex23,
                ex24,
                ex25,
                ex26,
                ex27,
                ex28,
                ex29,
                ex30,
                ex31,
                ex32,
                ex33,
                ex34,
                ex35,
            };

            Assert.That(exceptions.All(x => x.ParamName == "table"), Is.True);

            #endregion
        }

        #region Constructor

        [Test]
        public void Constructor_NoArguments_RunsOk()
        {
            // Arrange

            // Act
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            // Assert
            Assert.That(scriptBuilder.Connection, Is.Null);
            Assert.That(scriptBuilder.Factory, Is.EqualTo(SQLiteUtilityFactory.Instance));
            Assert.That(scriptBuilder.SchemaName, Is.EqualTo(null));
            Assert.That(scriptBuilder.CurrentOpeningIdentifierDelimiter, Is.EqualTo('['));
        }

        #endregion

        #region CurrentOpeningIdentifierDelimiter

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        [TestCase(null)]
        public void CurrentOpeningIdentifierDelimiter_SetValidValue_ChangesValue(char? openingDelimiter)
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            // Act
            scriptBuilder.CurrentOpeningIdentifierDelimiter = openingDelimiter;

            // Assert
            Assert.That(scriptBuilder.CurrentOpeningIdentifierDelimiter, Is.EqualTo(openingDelimiter));
        }

        [Test]
        public void CurrentOpeningIdentifierDelimiter_SetInvalidValidValue_ThrowsTauDbException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            // Act
            var ex = Assert.Throws<TauDbException>(() => scriptBuilder.CurrentOpeningIdentifierDelimiter = '`');

            // Assert
            Assert.That(ex, Has.Message.EqualTo("Invalid opening identifier delimiter: '`'."));
        }

        #endregion

        #region BuildCreateTableScript

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildCreateTableScript_IncludeConstraints_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "TaxInfo");
            var table = tableInspector.GetTable();

            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();
            scriptBuilder.CurrentOpeningIdentifierDelimiter = delimiter;

            string scriptName = "BuildCreateTableScript_Brackets.sql";

            if (delimiter == '"')
            {
                scriptName = "BuildCreateTableScript_DoubleQuotes.sql";
            }

            // Act
            var sql = scriptBuilder.BuildCreateTableScript(table, true);

            var expectedSql = this.GetType().Assembly.GetResourceText(scriptName, true);

            this.Connection.DropTable("TaxInfo");
            this.Connection.ExecuteSingleSql(sql);

            IDbTableInspector tableInspector2 = new SQLiteTableInspector(this.Connection, "TaxInfo");
            var table2 = tableInspector2.GetTable();

            var json = JsonConvert.SerializeObject(table);
            var json2 = JsonConvert.SerializeObject(table2);

            // Assert
            Assert.That(sql, Is.EqualTo(expectedSql));
            Assert.That(json, Is.EqualTo(json2));
        }

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildCreateTableScript_DoNotIncludeConstraints_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "TaxInfo");
            var table = tableInspector.GetTable();

            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();
            scriptBuilder.CurrentOpeningIdentifierDelimiter = delimiter;

            string scriptName = "BuildCreateTableScript_NoConstraints_Brackets.sql";

            if (delimiter == '"')
            {
                scriptName = "BuildCreateTableScript_NoConstraints_DoubleQuotes.sql";
            }

            // Act
            var sql = scriptBuilder.BuildCreateTableScript(table, false);

            var expectedSql = this.GetType().Assembly.GetResourceText(scriptName, true);

            this.Connection.DropTable("TaxInfo");
            this.Connection.ExecuteSingleSql(sql);

            // Assert
            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        [Test]
        public void BuildCreateTableScript_TableMoldIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => scriptBuilder.BuildCreateTableScript(null, true));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("table"));
        }

        [Test]
        public void BuildCreateTableScript_TableMoldIsCorrupted_ThrowsArgumentException()
        {
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            this.AssertCorruptedTableAction(x => scriptBuilder.BuildCreateTableScript(x, true));
        }

        #endregion

        #region BuildCreateIndexScript

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildCreateIndexScript_UniqueIndex_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "WorkInfo");
            var table = tableInspector.GetTable();
            var index = table.Indexes.Single(x => x.Name == "UX_workInfo_Hash");

            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder()
            {
                CurrentOpeningIdentifierDelimiter = delimiter,
            };

            // Act
            var sql = scriptBuilder.BuildCreateIndexScript(index);

            // Assert
            var expectedSql = this.GetType()
                .Assembly
                .GetResourceText("BuildCreateIndexScript_UniqueIndex_Brackets.sql", true);

            if (delimiter == '"')
            {
                expectedSql = this.GetType()
                    .Assembly
                    .GetResourceText("BuildCreateIndexScript_UniqueIndex_DoubleQuotes.sql", true);
            }

            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildCreateIndexScript_NonUniqueIndex_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "HealthInfo");
            var table = tableInspector.GetTable();
            var index = table.Indexes.Single(x => x.Name == "IX_healthInfo_metricAmetricB");

            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder()
            {
                CurrentOpeningIdentifierDelimiter = delimiter,
            };

            // Act
            var sql = scriptBuilder.BuildCreateIndexScript(index);

            // Assert
            var expectedSql = this.GetType()
                .Assembly
                .GetResourceText("BuildCreateIndexScript_NonUniqueIndex_Brackets.sql", true);

            if (delimiter == '"')
            {
                expectedSql = this.GetType()
                    .Assembly
                    .GetResourceText("BuildCreateIndexScript_NonUniqueIndex_DoubleQuotes.sql", true);
            }

            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        [Test]
        public void BuildCreateIndexScript_IndexIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => scriptBuilder.BuildCreateIndexScript(null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("index"));
        }

        [Test]
        public void BuildCreateIndexScript_IndexIsCorrupted_ThrowsArgumentException()
        {
            // Arrange
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "HealthInfo");
            var table = tableInspector.GetTable();
            var index = table.Indexes.Single(x => x.Name == "IX_healthInfo_metricAmetricB");

            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            // Act
            // corrupted: index name is null
            var index1 = (IndexMold)index.Clone();
            index1.Name = null;
            var ex1 = Assert.Throws<ArgumentException>(() => scriptBuilder.BuildCreateIndexScript(index1));

            // corrupted: index index name is null
            var index2 = (IndexMold)index.Clone();
            index2.TableName = null;
            var ex2 = Assert.Throws<ArgumentException>(() => scriptBuilder.BuildCreateIndexScript(index2));

            // corrupted: index columns is null
            var index3 = (IndexMold)index.Clone();
            index3.Columns = null;
            var ex3 = Assert.Throws<ArgumentException>(() => scriptBuilder.BuildCreateIndexScript(index3));

            // corrupted: index columns is empty
            var index4 = (IndexMold)index.Clone();
            index4.Columns.Clear();
            var ex4 = Assert.Throws<ArgumentException>(() => scriptBuilder.BuildCreateIndexScript(index4));

            // corrupted: index columns contains null
            var index5 = (IndexMold)index.Clone();
            index5.Columns[0] = null;
            var ex5 = Assert.Throws<ArgumentException>(() => scriptBuilder.BuildCreateIndexScript(index5));

            // corrupted: index column name is null
            var index6 = (IndexMold)index.Clone();
            index6.Columns[0].Name = null;
            var ex6 = Assert.Throws<ArgumentException>(() => scriptBuilder.BuildCreateIndexScript(index6));

            // Assert
            Assert.That(ex1, Has.Message.StartsWith("Index name cannot be null."));
            Assert.That(ex2, Has.Message.StartsWith("Index table name cannot be null."));
            Assert.That(ex3, Has.Message.StartsWith("Index columns cannot be null."));
            Assert.That(ex4, Has.Message.StartsWith("Index columns cannot be empty."));
            Assert.That(ex5, Has.Message.StartsWith("Index columns cannot contain nulls."));
            Assert.That(ex6, Has.Message.StartsWith("Index column name cannot be null."));
        }

        #endregion

        #region BuildDropTableScript

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildDropTableScript_ValidArgument_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder()
            {
                CurrentOpeningIdentifierDelimiter = delimiter,
            };

            // Act
            var sql = scriptBuilder.BuildDropTableScript("MyTable");

            // Assert
            var expectedSql = "DROP TABLE [MyTable]";

            if (delimiter == '"')
            {
                expectedSql = expectedSql
                    .Replace('[', '"')
                    .Replace(']', '"');
            }

            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        [Test]
        public void BuildDropTableScript_TableNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => scriptBuilder.BuildDropTableScript(null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        #endregion

        #region BuildInsertScript

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildInsertScript_ValidArguments_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder()
            {
                CurrentOpeningIdentifierDelimiter = delimiter,
            };

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "Person");
            var table = tableInspector.GetTable();

            var columnToParameterMappings = new Dictionary<string, string>
            {
                {"MetaKey", "p_metaKey"},
                {"OrdNumber", "p_ordNumber"},
                {"Id", "p_id"},
                {"FirstName", "p_firstName"},
                {"LastName", "p_lastName"},
                {"Birthday", "p_birthday"},
                {"Gender", "p_gender"},
                {"Initials", "p_initials"},
            };

            // Act
            var sql = scriptBuilder.BuildInsertScript(table, columnToParameterMappings);

            // Assert
            var expectedSql = this.GetType()
                .Assembly
                .GetResourceText("BuildInsertScript_AllColumns_Brackets.sql", true);
            if (delimiter == '"')
            {
                expectedSql = this.GetType()
                    .Assembly
                    .GetResourceText("BuildInsertScript_AllColumns_DoubleQuotes.sql", true);
            }

            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildInsertScript_ColumnToParameterMappingsIsEmpty_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder()
            {
                CurrentOpeningIdentifierDelimiter = delimiter,
            };

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "Person");
            var table = tableInspector.GetTable();

            var columnToParameterMappings = new Dictionary<string, string>();

            // Act
            var sql = scriptBuilder.BuildInsertScript(table, columnToParameterMappings);

            // Assert
            var expectedSql = "INSERT INTO [Person] DEFAULT VALUES";
            if (delimiter == '"')
            {
                expectedSql = expectedSql
                    .Replace('[', '"')
                    .Replace(']', '"');
            }

            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildInsertScript_ColumnToParameterMappingsContainsBadColumns_ThrowsArgumentException(
            char delimiter)
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder()
            {
                CurrentOpeningIdentifierDelimiter = delimiter,
            };

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "Person");
            var table = tableInspector.GetTable();

            var columnToParameterMappings = new Dictionary<string, string>
            {
                {"MetaKey", "p_metaKey"},
                {"OrdNumber", "p_ordNumber"},
                {"Id", "p_id"},
                {"FirstName", "p_firstName"},
                {"LastName", "p_lastName"},
                {"Birthday", "p_birthday"},
                {"Gender", "p_gender"},
                {"Initials", "p_initials"},
                {"BadColumn", "p_badColumn"},
            };

            // Act
            var ex = Assert.Throws<ArgumentException>(() =>
                scriptBuilder.BuildInsertScript(table, columnToParameterMappings));

            // Assert
            Assert.That(ex, Has.Message.StartsWith($"Invalid column: 'BadColumn'."));
            Assert.That(ex.ParamName, Is.EqualTo(nameof(columnToParameterMappings)));
        }

        [Test]
        public void BuildInsertScript_TableMoldIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() =>
                scriptBuilder.BuildInsertScript(null, new Dictionary<string, string>()));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("table"));
        }

        [Test]
        public void BuildInsertScript_TableMoldIsCorrupted_ThrowsArgumentException()
        {
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            var columnToParameterMappings = new Dictionary<string, string>();

            this.AssertCorruptedTableAction(x => scriptBuilder.BuildInsertScript(x, columnToParameterMappings));
        }

        [Test]
        public void BuildInsertScript_ColumnToParameterMappingsIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "Person");
            var table = tableInspector.GetTable();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => scriptBuilder.BuildInsertScript(table, null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("columnToParameterMappings"));
        }

        [Test]
        public void BuildInsertScript_ColumnToParameterMappingsIsCorrupted_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "Person");
            var table = tableInspector.GetTable();

            var columnToParameterMappings = new Dictionary<string, string>
            {
                {"MetaKey", "p_metaKey"},
                {"OrdNumber", "p_ordNumber"},
                {"Id", "p_id"},
                {"FirstName", "p_firstName"},
                {"LastName", "p_lastName"},
                {"Birthday", "p_birthday"},
                {"Gender", "p_gender"},
                {"Initials", "p_initials"},
            };

            // Act
            var columnToParameterMappings1 = columnToParameterMappings.ToDictionary(
                x => x.Key,
                x => x.Value);
            columnToParameterMappings1["Id"] = null;

            var ex = Assert.Throws<ArgumentException>(() =>
                scriptBuilder.BuildInsertScript(table, columnToParameterMappings1));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("columnToParameterMappings"));
            Assert.That(ex, Has.Message.StartsWith("'columnToParameterMappings' cannot contain null values."));
        }

        #endregion

        #region BuildUpdateScript

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildUpdateScript_ValidArguments_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder()
            {
                CurrentOpeningIdentifierDelimiter = delimiter,
            };

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");
            var table = tableInspector.GetTable();

            var columnToParameterMappings = new Dictionary<string, string>
            {
                {"Height", "p_height"},
                {"Photo", "p_photo"},
                {"EnglishDescription", "p_englishDescription"},
                {"UnicodeDescription", "p_unicodeDescription"},
                {"PersonMetaKey", "p_personMetaKey"},
                {"PersonOrdNumber", "p_personOrdNumber"},
                {"PersonId", "p_personId"},

                {"Id", "p_id"},
            };

            // Act
            var sql = scriptBuilder.BuildUpdateScript(table, columnToParameterMappings);

            // Assert
            var expectedSql = this.GetType().Assembly
                .GetResourceText("BuildUpdateScript_AllColumns_Brackets.sql", true);
            if (delimiter == '"')
            {
                expectedSql = this.GetType().Assembly
                    .GetResourceText("BuildUpdateScript_AllColumns_DoubleQuotes.sql", true);
            }

            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        [Test]
        public void BuildUpdateScript_MappingsIncomplete_ThrowsArgumentException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");
            var table = tableInspector.GetTable();

            var columnToParameterMappings = new Dictionary<string, string>
            {
                {"Height", "p_height"},
                {"Photo", "p_photo"},
                {"EnglishDescription", "p_englishDescription"},
                {"UnicodeDescription", "p_unicodeDescription"},
                {"PersonMetaKey", "p_personMetaKey"},
                {"PersonOrdNumber", "p_personOrdNumber"},
                {"PersonId", "p_personId"},

                {"Id", "p_id"},
            };

            // Act

            // no pk column
            var columnToParameterMappings1 = columnToParameterMappings.ToDictionary(
                x => x.Key,
                x => x.Value);
            columnToParameterMappings1.Remove("Id");

            var ex1 = Assert.Throws<ArgumentException>((() =>
                scriptBuilder.BuildUpdateScript(table, columnToParameterMappings1)));

            // pk column only
            var columnToParameterMappings2 = columnToParameterMappings.ToDictionary(
                x => x.Key,
                x => x.Value);
            foreach (var columnName in columnToParameterMappings2.Keys.Except(new[] { "Id" }))
            {
                columnToParameterMappings2.Remove(columnName);
            }

            var ex2 = Assert.Throws<ArgumentException>((() =>
                scriptBuilder.BuildUpdateScript(table, columnToParameterMappings2)));


            // Assert
            Assert.That(ex1,
                Has.Message.StartsWith("'columnToParameterMappings' must contain primary key column mapping."));
            Assert.That(ex1.ParamName, Is.EqualTo("columnToParameterMappings"));

            Assert.That(ex2,
                Has.Message.StartsWith(
                    "'columnToParameterMappings' must contain at least one column mapping besides primary key column."));
            Assert.That(ex2.ParamName, Is.EqualTo("columnToParameterMappings"));
        }

        [Test]
        public void BuildUpdateScript_TableDoesNotContainPrimaryKey_ThrowsArgumentException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            this.Connection.ExecuteSingleSql("CREATE TABLE [dummy](Foo int)"); // no PK
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "dummy");
            var table = tableInspector.GetTable();

            var columnToParameterMappings = new Dictionary<string, string>
            {
                {"Foo", "p_foo"},
            };

            // Act

            var ex = Assert.Throws<ArgumentException>((() =>
                scriptBuilder.BuildUpdateScript(table, columnToParameterMappings)));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("Table 'dummy' does not have a primary key."));
            Assert.That(ex.ParamName, Is.EqualTo("table"));
        }

        [Test]
        public void BuildUpdateScript_PrimaryKeyIsMultiColumn_ThrowsArgumentException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "Person");
            var table = tableInspector.GetTable();

            var columnToParameterMappings = new Dictionary<string, string>
            {
                {"FirstName", "p_firstName"},

                {"MetaKey", "p_metaKey"},
                {"OrdNumber", "p_ordNumber"},
                {"Id", "p_id"},
            };

            // Act

            var ex = Assert.Throws<ArgumentException>((() =>
                scriptBuilder.BuildUpdateScript(table, columnToParameterMappings)));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("Failed to retrieve single primary key column name for the table 'Person'."));
            Assert.That(ex.ParamName, Is.EqualTo("table"));
        }

        [Test]
        public void BuildUpdateScript_TableIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            var columnToParameterMappings = new Dictionary<string, string>
            {
                {"FirstName", "p_firstName"},

                {"MetaKey", "p_metaKey"},
                {"OrdNumber", "p_ordNumber"},
                {"Id", "p_id"},
            };

            // Act

            var ex = Assert.Throws<ArgumentNullException>((() =>
                scriptBuilder.BuildUpdateScript(null, columnToParameterMappings)));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("table"));
        }

        [Test]
        public void BuildUpdateScript_TableIsCorrupted_ThrowsArgumentNullException()
        {
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            var columnToParameterMappings = new Dictionary<string, string>
            {
                {"Id", "p_id"},

                {"Weight", "p_weight"},
            };

            this.AssertCorruptedTableAction(mold => scriptBuilder.BuildUpdateScript(mold, columnToParameterMappings));
        }

        [Test]
        public void BuildUpdateScript_ColumnToParameterMappingsIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");
            var table = tableInspector.GetTable();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => scriptBuilder.BuildUpdateScript(table, null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("columnToParameterMappings"));
        }

        [Test]
        public void BuildUpdateScript_ColumnToParameterMappingsIsCorrupted_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");
            var table = tableInspector.GetTable();

            var columnToParameterMappings = new Dictionary<string, string>
            {
                {"Height", "p_height"},
                {"Photo", "p_photo"},
                {"EnglishDescription", "p_englishDescription"},
                {"UnicodeDescription", "p_unicodeDescription"},
                {"PersonMetaKey", "p_personMetaKey"},
                {"PersonOrdNumber", "p_personOrdNumber"},
                {"PersonId", "p_personId"},

                {"Id", "p_id"},
            };

            // Act
            var columnToParameterMappings1 = columnToParameterMappings.ToDictionary(
                x => x.Key,
                x => x.Value);
            columnToParameterMappings1["PersonMetaKey"] = null;

            var ex = Assert.Throws<ArgumentException>(() =>
                scriptBuilder.BuildUpdateScript(table, columnToParameterMappings1));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("columnToParameterMappings"));
            Assert.That(ex, Has.Message.StartsWith("'columnToParameterMappings' cannot contain null values."));
        }

        #endregion

        #region BuildSelectByPrimaryKeyScript

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildSelectByPrimaryKeyScript_ValidArguments_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder()
            {
                CurrentOpeningIdentifierDelimiter = delimiter,
            };

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");
            var table = tableInspector.GetTable();

            var columns = new[]
            {
                "Height",
                "EnglishDescription",
                "UnicodeDescription",
                "PersonMetaKey",
                "PersonOrdNumber",
                "PersonId",
            }.ToHashSet();

            bool Selector(string x) => columns.Contains(x);

            // Act
            var sql = scriptBuilder.BuildSelectByPrimaryKeyScript(table, "p_id", Selector);

            // Assert
            var expectedSql = this.GetType()
                .Assembly
                .GetResourceText("BuildSelectByPrimaryKeyScript_Brackets.sql", true);

            if (delimiter == '"')
            {
                expectedSql = this.GetType()
                    .Assembly
                    .GetResourceText("BuildSelectByPrimaryKeyScript_DoubleQuotes.sql", true);
            }

            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildSelectByPrimaryKeyScript_ColumnSelectorIsNull_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder()
            {
                CurrentOpeningIdentifierDelimiter = delimiter,
            };

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");
            var table = tableInspector.GetTable();

            // Act
            var sql = scriptBuilder.BuildSelectByPrimaryKeyScript(table, "p_id", null);

            // Assert
            var expectedSql = this.GetType()
                .Assembly
                .GetResourceText("BuildSelectByPrimaryKeyScript_AllColumns_Brackets.sql", true);

            if (delimiter == '"')
            {
                expectedSql = this.GetType()
                    .Assembly
                    .GetResourceText("BuildSelectByPrimaryKeyScript_AllColumns_DoubleQuotes.sql", true);
            }

            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        [Test]
        public void BuildSelectByPrimaryKeyScript_TableDoesNotContainPrimaryKey_ThrowsArgumentException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            this.Connection.ExecuteSingleSql("CREATE TABLE [dummy](Foo int)"); // no PK
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "dummy");
            var table = tableInspector.GetTable();

            // Act

            var ex = Assert.Throws<ArgumentException>((() =>
                scriptBuilder.BuildSelectByPrimaryKeyScript(table, "p_id", x => true)));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("Table 'dummy' does not have a primary key."));
            Assert.That(ex.ParamName, Is.EqualTo("table"));
        }

        [Test]
        public void BuildSelectByPrimaryKeyScript_PrimaryKeyIsMultiColumn_ThrowsArgumentException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "Person");
            var table = tableInspector.GetTable();

            // Act

            var ex = Assert.Throws<ArgumentException>((() =>
                scriptBuilder.BuildSelectByPrimaryKeyScript(table, "p_id", x => true)));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("Failed to retrieve single primary key column name for the table 'Person'."));
            Assert.That(ex.ParamName, Is.EqualTo("table"));
        }

        [Test]
        public void BuildSelectByPrimaryKeyScript_PrimaryKeyParameterNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");
            var table = tableInspector.GetTable();

            var columns = new[]
            {
                "Height",
                "EnglishDescription",
                "UnicodeDescription",
                "PersonMetaKey",
                "PersonOrdNumber",
                "PersonId",
            }.ToHashSet();

            bool Selector(string x) => columns.Contains(x);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() =>
                scriptBuilder.BuildSelectByPrimaryKeyScript(table, null, Selector));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("pkParameterName"));
        }

        [Test]
        public void BuildSelectByPrimaryKeyScript_NoColumnsSelected_ThrowsArgumentException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");
            var table = tableInspector.GetTable();

            // Act
            var ex = Assert.Throws<ArgumentException>(() =>
                scriptBuilder.BuildSelectByPrimaryKeyScript(table, "p_id", x => false));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("columnSelector"));
            Assert.That(ex, Has.Message.StartsWith("No columns were selected."));
        }

        [Test]
        public void BuildSelectByPrimaryKeyScript_TableIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            // Act
            var ex = Assert.Throws<ArgumentNullException>((() =>
                scriptBuilder.BuildSelectByPrimaryKeyScript(null, "p_id", null)));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("table"));
        }

        [Test]
        public void BuildSelectByPrimaryKeyScript_TableIsCorrupted_ThrowsArgumentNullException()
        {
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            this.AssertCorruptedTableAction(mold => scriptBuilder.BuildSelectByPrimaryKeyScript(mold, "p_id", null));
        }

        #endregion

        #region BuildSelectAllScript

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildSelectAllScript_ValidArguments_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder()
            {
                CurrentOpeningIdentifierDelimiter = delimiter,
            };

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");
            var table = tableInspector.GetTable();

            var columns = new[]
            {
                "Height",
                "EnglishDescription",
                "UnicodeDescription",
                "PersonMetaKey",
                "PersonOrdNumber",
                "PersonId",
            }.ToHashSet();

            bool Selector(string x) => columns.Contains(x);

            // Act
            var sql = scriptBuilder.BuildSelectAllScript(table, Selector);

            // Assert
            var expectedSql = this.GetType()
                .Assembly
                .GetResourceText("BuildSelectAllScript_Brackets.sql", true);

            if (delimiter == '"')
            {
                expectedSql = this.GetType()
                    .Assembly
                    .GetResourceText("BuildSelectAllScript_DoubleQuotes.sql", true);
            }

            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildSelectAllScript_ColumnSelectorIsNull_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder()
            {
                CurrentOpeningIdentifierDelimiter = delimiter,
            };

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");
            var table = tableInspector.GetTable();

            // Act
            var sql = scriptBuilder.BuildSelectAllScript(table, null);

            // Assert
            var expectedSql = this.GetType()
                .Assembly
                .GetResourceText("BuildSelectAllScript_AllColumns_Brackets.sql", true);

            if (delimiter == '"')
            {
                expectedSql = this.GetType()
                    .Assembly
                    .GetResourceText("BuildSelectAllScript_AllColumns_DoubleQuotes.sql", true);
            }

            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        [Test]
        public void BuildSelectAllScript_NoColumnsSelected_ThrowsArgumentException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");
            var table = tableInspector.GetTable();

            // Act
            var ex = Assert.Throws<ArgumentException>(() =>
                scriptBuilder.BuildSelectAllScript(table, x => false));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("columnSelector"));
            Assert.That(ex, Has.Message.StartsWith("No columns were selected."));
        }

        [Test]
        public void BuildSelectAllScript_TableIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            // Act
            var ex = Assert.Throws<ArgumentNullException>((() =>
                scriptBuilder.BuildSelectAllScript(null, null)));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("table"));
        }

        [Test]
        public void BuildSelectAllScript_TableIsCorrupted_ThrowsArgumentNullException()
        {
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            this.AssertCorruptedTableAction(mold => scriptBuilder.BuildSelectAllScript(mold, null));
        }

        #endregion

        #region BuildDeleteByPrimaryKeyScript

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildDeleteByPrimaryKeyScript_ValidArguments_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder()
            {
                CurrentOpeningIdentifierDelimiter = delimiter,
            };

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");
            var table = tableInspector.GetTable();
            
            // Act
            var sql = scriptBuilder.BuildDeleteByPrimaryKeyScript(table, "p_id");

            // Assert
            var expectedSql = "DELETE FROM [PersonData] WHERE [Id] = @p_id";

            if (delimiter == '"')
            {
                expectedSql = expectedSql
                    .Replace('[', '"')
                    .Replace(']', '"');
            }

            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        [Test]
        public void BuildDeleteByPrimaryKeyScript_TableHasNoPrimaryKey_ThrowsArgumentException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            this.Connection.ExecuteSingleSql("CREATE TABLE [dummy](Foo int)"); // no PK
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "dummy");
            var table = tableInspector.GetTable();

            // Act

            var ex = Assert.Throws<ArgumentException>((() =>
                scriptBuilder.BuildDeleteByPrimaryKeyScript(table, "p_id")));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("Table 'dummy' does not have a primary key."));
            Assert.That(ex.ParamName, Is.EqualTo("table"));
        }

        [Test]
        public void BuildDeleteByPrimaryKeyScript_PrimaryKeyIsMultiColumn_ThrowsArgumentException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();
            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "Person");
            var table = tableInspector.GetTable();

            // Act

            var ex = Assert.Throws<ArgumentException>((() =>
                scriptBuilder.BuildDeleteByPrimaryKeyScript(table, "p_id")));

            // Assert
            Assert.That(ex, Has.Message.StartsWith("Failed to retrieve single primary key column name for the table 'Person'."));
            Assert.That(ex.ParamName, Is.EqualTo("table"));
        }

        [Test]
        public void BuildDeleteByPrimaryKeyScript_PrimaryKeyParameterNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            IDbTableInspector tableInspector = new SQLiteTableInspector(this.Connection, "PersonData");
            var table = tableInspector.GetTable();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() =>
                scriptBuilder.BuildDeleteByPrimaryKeyScript(table, null));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("pkColumnParameterName"));
        }

        [Test]
        public void BuildDeleteByPrimaryKeyScript_TableIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            // Act
            var ex = Assert.Throws<ArgumentNullException>((() =>
                scriptBuilder.BuildDeleteByPrimaryKeyScript(null, "p_id")));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("table"));
        }

        [Test]
        public void BuildDeleteByPrimaryKeyScript_TableIsCorrupted_ThrowsArgumentNullException()
        {
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            this.AssertCorruptedTableAction(mold => scriptBuilder.BuildDeleteByPrimaryKeyScript(mold, "p_id"));
        }

        #endregion

        #region BuildDeleteScript

        [Test]
        [TestCase('[')]
        [TestCase('"')]
        public void BuildDeleteScript_ValidArgument_ReturnsValidScript(char delimiter)
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder()
            {
                CurrentOpeningIdentifierDelimiter = delimiter,
            };

            var tableName = "PersonData";

            // Act
            var sql = scriptBuilder.BuildDeleteScript(tableName);

            // Assert
            var expectedSql = "DELETE FROM [PersonData]";

            if (delimiter == '"')
            {
                expectedSql = expectedSql
                    .Replace('[', '"')
                    .Replace(']', '"');
            }

            Assert.That(sql, Is.EqualTo(expectedSql));
        }

        [Test]
        public void BuildDeleteScript_TableNameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            IDbScriptBuilder scriptBuilder = new SQLiteScriptBuilder();

            // Act
            var ex = Assert.Throws<ArgumentNullException>((() =>
                scriptBuilder.BuildDeleteScript(null)));

            // Assert
            Assert.That(ex.ParamName, Is.EqualTo("tableName"));
        }

        #endregion
    }
}
