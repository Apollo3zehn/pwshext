using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace pwshext.Tests
{
    public class ExecTests
    {
        [Fact]
        public async Task CanUseLogging()
        {
            // Arrange

            /* script */
            var scriptContent = @"
$VerbosePreference  = ""Continue""
$DebugPreference    = ""Continue""

Write-Verbose       ""Write-Verbose""
Write-Debug         ""Write-Debug""
Write-Information   ""Write-Information""
Write-Warning       ""Write-Warning""
Write-Error         ""Write-Error""

Write-Host          ""Write-Host""
";

            var tmpScriptFilePath = Path.GetTempFileName();
            File.WriteAllText(tmpScriptFilePath, scriptContent);

            /* logging */
            var logFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(logFolderPath);

            /* args */
            var args = $"exec --logger MyLogger --file-logger {logFolderPath} --log-level Trace {tmpScriptFilePath}"
                .Split(" ");

            try
            {
                // Act
                await Program.Main(args);

                // Assert
                var files = Directory.EnumerateFiles(logFolderPath);
                Assert.True(files.Any());

                var actual = File.ReadAllLines(files.First())
                    .Select(value => value.Substring(20))
                    .ToArray();

                var expected = new string[]
                {
                    "[VRB] (MyLogger) Write-Verbose",
                    "[DBG] (MyLogger) Write-Debug",
                    "[INF] (MyLogger) Write-Information",
                    "[WRN] (MyLogger) Write-Warning",
                    "[ERR] (MyLogger) Write-Error",
                    "[INF] (MyLogger) Write-Host",
                };

                Assert.True(actual.SequenceEqual(expected));
            }
            finally
            {
                if (File.Exists(tmpScriptFilePath))
                    File.Delete(tmpScriptFilePath);

                if (Directory.Exists(logFolderPath))
                    Directory.Delete(logFolderPath, recursive: true);
            }
        }

        [Fact]
        public async Task CanUseLogLevels()
        {
            // Arrange

            /* script */
            var scriptContent = @"
$VerbosePreference  = ""Continue""
$DebugPreference    = ""Continue""

Write-Verbose       ""Write-Verbose""
Write-Debug         ""Write-Debug""
Write-Information   ""Write-Information""
Write-Warning       ""Write-Warning""
Write-Error         ""Write-Error""
";

            var tmpScriptFilePath = Path.GetTempFileName();
            File.WriteAllText(tmpScriptFilePath, scriptContent);

            /* logging */
            var logFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(logFolderPath);

            /* args */
            var args = $"exec --logger MyLogger --file-logger {logFolderPath} --log-level Warning {tmpScriptFilePath}"
                .Split(" ");

            try
            {
                // Act
                await Program.Main(args);

                // Assert
                var files = Directory.EnumerateFiles(logFolderPath);
                Assert.True(files.Any());

                var actual = File.ReadAllLines(files.First())
                    .Select(value => value.Substring(20))
                    .ToArray();

                var expected = new string[]
                {
                    "[WRN] (MyLogger) Write-Warning",
                    "[ERR] (MyLogger) Write-Error"
                };

                Assert.True(actual.SequenceEqual(expected));
            }
            finally
            {
                if (File.Exists(tmpScriptFilePath))
                    File.Delete(tmpScriptFilePath);

                if (Directory.Exists(logFolderPath))
                    Directory.Delete(logFolderPath, recursive: true);
            }
        }

        [Fact]
        public async Task CanExecuteScriptWithParams()
        {
            // Arrange

            /* script */
            var scriptContent = @"
Param (
    [string]$param1,
    [string]$param2,
    [string]$param3,
    [string]$param4
)

Write-Information $param1;
Write-Information $param2;
Write-Information $param3;
Write-Information $param4;
";

            var tmpScriptFilePath = Path.GetTempFileName();
            File.WriteAllText(tmpScriptFilePath, scriptContent);

            /* logging */
            var logFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(logFolderPath);

            /* args */
            var args = $"exec --logger MyLogger --file-logger {logFolderPath} --arg param1=value1 param2=value2 --arg param3=with_=_sign. --arg param4=Greetings {tmpScriptFilePath}"
                .Split(" ")
                .Select(value => value.Replace('_', ' '))
                .ToArray();

            try
            {
                // Act
                await Program.Main(args);

                // Assert
                var files = Directory.EnumerateFiles(logFolderPath);
                Assert.True(files.Any());

                var actual = File.ReadAllLines(files.First())
                    .Select(value => value.Substring(20))
                    .ToArray();

                var expected = new string[]
                {
                    "[INF] (MyLogger) value1",
                    "[INF] (MyLogger) value2",
                    "[INF] (MyLogger) with = sign.",
                    "[INF] (MyLogger) Greetings"
                };

                Assert.True(actual.SequenceEqual(expected));
            }
            finally
            {
                if (File.Exists(tmpScriptFilePath))
                    File.Delete(tmpScriptFilePath);

                if (Directory.Exists(logFolderPath))
                    Directory.Delete(logFolderPath, recursive: true);
            }
        }

        [Fact]
        public async Task CanExecuteScriptWithFunction()
        {
            // Arrange

            /* script */
            var scriptContent = @"
Param (
    [string]$param
)

function Invoke([string]$message)
{
    Write-Information $message;
}

Invoke($param)
";

            var tmpScriptFilePath = Path.GetTempFileName();
            File.WriteAllText(tmpScriptFilePath, scriptContent);

            /* logging */
            var logFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(logFolderPath);

            /* args */
            var args = $"exec --logger MyLogger --file-logger {logFolderPath} --arg param=MyMessage {tmpScriptFilePath}"
                .Split(" ")
                .Select(value => value.Replace('_', ' '))
                .ToArray();

            try
            {
                // Act
                await Program.Main(args);

                // Assert
                var files = Directory.EnumerateFiles(logFolderPath);
                Assert.True(files.Any());

                var actual = File.ReadAllLines(files.First())
                    .Select(value => value.Substring(20))
                    .ToArray();

                var expected = new string[]
                {
                    "[INF] (MyLogger) MyMessage"
                };

                Assert.True(actual.SequenceEqual(expected));
            }
            finally
            {
                if (File.Exists(tmpScriptFilePath))
                    File.Delete(tmpScriptFilePath);

                if (Directory.Exists(logFolderPath))
                    Directory.Delete(logFolderPath, recursive: true);
            }
        }

        [Fact]
        public async Task CanAccessFTPClients()
        {
            // Arrange

            /* script */
            var scriptContent = @"
$ftpClient      = New-Object -TypeName FluentFTP.FtpClient

Write-Information $ftpClient.GetType();

$ftpClient      = New-Object -TypeName Renci.SshNet.PasswordAuthenticationMethod `
                             -ArgumentList UserName, Password;

Write-Information $ftpClient.GetType();
";

            var tmpScriptFilePath = Path.GetTempFileName();
            File.WriteAllText(tmpScriptFilePath, scriptContent);

            /* logging */
            var logFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(logFolderPath);

            /* args */
            var args = $"exec --logger MyLogger --file-logger {logFolderPath} {tmpScriptFilePath}"
                .Split(" ")
                .Select(value => value.Replace('_', ' '))
                .ToArray();

            try
            {
                // Act
                await Program.Main(args);

                // Assert
                var files = Directory.EnumerateFiles(logFolderPath);
                Assert.True(files.Any());

                var actual = File.ReadAllLines(files.First())
                    .Select(value => value.Substring(20))
                    .ToArray();

                var expected = new string[]
                {
                    "[INF] (MyLogger) FluentFTP.FtpClient",
                    "[INF] (MyLogger) Renci.SshNet.PasswordAuthenticationMethod"
                };

                Assert.True(actual.SequenceEqual(expected));
            }
            finally
            {
                if (File.Exists(tmpScriptFilePath))
                    File.Delete(tmpScriptFilePath);

                if (Directory.Exists(logFolderPath))
                    Directory.Delete(logFolderPath, recursive: true);
            }
        }
    }
};