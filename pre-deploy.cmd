dotnet restore

dotnet build TauCode.Db.SQLite.sln -c Debug
dotnet build TauCode.Db.SQLite.sln -c Release

dotnet test TauCode.Db.SQLite.sln -c Debug
dotnet test TauCode.Db.SQLite.sln -c Release

nuget pack nuget\TauCode.Db.SQLite.nuspec