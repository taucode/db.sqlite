using System;
using System.IO;
using System.Text;
using TauCode.Extensions;

namespace TauCode.Db.SQLite.Tests
{
    internal static class TestHelper
    {
        internal const string NonExistingGuidString = "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee";
        internal static readonly Guid NonExistingGuid = new Guid(NonExistingGuidString);

        internal static string GetResourceText(string fileName) =>
            typeof(TestHelper).Assembly.GetResourceText(fileName, true);

        internal static void WriteDiff(string actual, string expected, string directory, string fileExtension, string reminder)
        {
            if (reminder != "to" + "do")
            {
                throw new InvalidOperationException("don't forget this call with mark!");
            }

            fileExtension = fileExtension.Replace(".", "");

            var actualFileName = $"0-actual.{fileExtension}";
            var expectedFileName = $"1-expected.{fileExtension}";

            var actualFilePath = Path.Combine(directory, actualFileName);
            var expectedFilePath = Path.Combine(directory, expectedFileName);

            File.WriteAllText(actualFilePath, actual, Encoding.UTF8);
            File.WriteAllText(expectedFilePath, expected, Encoding.UTF8);
        }
    }
}
