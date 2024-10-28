namespace TauCode.Db.SQLite;

public static class SQLiteExtensions
{
    public static void DropSchema(this SQLiteExplorer explorer, string schemaName, bool forceDropSchemaTables = false)
    {
        if (forceDropSchemaTables)
        {
            var tableNames = explorer.GetTableNames(schemaName);
            if (tableNames.Any())
            {
                throw new NotImplementedException();
            }
        }

        explorer.GetOpenConnection().ExecuteSql(@$"DROP SCHEMA {schemaName}");
    }

    public static void ProcessJson(this SqlInstructionProcessor processor, string json)
    {
        // todo checks

        var instructionReader = new JsonInstructionReader();
        using var jsonTextReader = new StringReader(json);

        var instructions = instructionReader.ReadInstructions(jsonTextReader);

        processor.Process(instructions);
    }
}
