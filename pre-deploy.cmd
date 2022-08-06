dotnet restore

dotnet build --configuration Debug
dotnet build --configuration Release

dotnet test -c Debug .\test\TauCode.Db.SQLite.Tests\TauCode.Db.SQLite.Tests.csproj
dotnet test -c Release .\test\TauCode.Db.SQLite.Tests\TauCode.Db.SQLite.Tests.csproj

nuget pack nuget\TauCode.Db.SQLite.nuspec