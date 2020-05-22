using OneDas.Hdf.VdsTool;
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
        public async Task CanExecuteScript()
        {
            // Arrange

            /* script */
            var scriptContent = @"
$logger.LogTrace(""Trace"");
$logger.LogDebug(""Debug"");
$logger.LogInformation(""Information"");
$logger.LogWarning(""Warning"");
$logger.LogError(""Error"");
$logger.LogCritical(""Critical"");
";

            var tmpScriptFilePath = Path.GetTempFileName();
            File.WriteAllText(tmpScriptFilePath, scriptContent);

            /* logging */
            var logFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(logFolderPath);

            /* args */
            var args = $"exec --script {tmpScriptFilePath} --id MyId --log-folder {logFolderPath} --log-level Trace"
                .Split(" ");

            try
            {
                // Act
                await Program.Main(args);

                // Assert
                var files = Directory.EnumerateFiles(logFolderPath);
                Assert.True(files.Any());

                var actual = File.ReadAllLines(files.First())
                    .Skip(1)
                    .Take(6)
                    .Select(value => value.Substring(21))
                    .ToArray();

                var expected = new string[]
                {
                    "[VRB] (MyId) Trace",
                    "[DBG] (MyId) Debug",
                    "[INF] (MyId) Information",
                    "[WRN] (MyId) Warning",
                    "[ERR] (MyId) Error",
                    "[FTL] (MyId) Critical"
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

$logger.LogInformation($param1);
$logger.LogInformation($param2);
$logger.LogInformation($param3);
$logger.LogInformation($param4);
";

            var tmpScriptFilePath = Path.GetTempFileName();
            File.WriteAllText(tmpScriptFilePath, scriptContent);

            /* logging */
            var logFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(logFolderPath);

            /* args */
            var args = $"exec --script {tmpScriptFilePath} --id MyId --log-folder {logFolderPath} --arg param1=value1 param2=value2 --arg param3=with_=_sign. --arg param4=Greetings"
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
                    .Skip(1)
                    .Take(4)
                    .Select(value => value.Substring(21))
                    .ToArray();

                var expected = new string[]
                {
                    "[INF] (MyId) value1",
                    "[INF] (MyId) value2",
                    "[INF] (MyId) with = sign.",
                    "[INF] (MyId) Greetings"
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
        public async Task CanCallScriptWithFunction()
        {
            // Arrange

            /* script */
            var scriptContent = @"
Param (
    [string]$param
)

function Invoke([string]$message)
{
    $logger.LogInformation($param);
}

Invoke($param)
";

            var tmpScriptFilePath = Path.GetTempFileName();
            File.WriteAllText(tmpScriptFilePath, scriptContent);

            /* logging */
            var logFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(logFolderPath);

            /* args */
            var args = $"exec --script {tmpScriptFilePath} --id MyId --log-folder {logFolderPath} --arg param=MyMessage"
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
                    .Skip(1)
                    .Take(1)
                    .Select(value => value.Substring(21))
                    .ToArray();

                var expected = new string[]
                {
                    "[INF] (MyId) MyMessage"
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

$logger.LogInformation($ftpClient.GetType());

$ftpClient      = New-Object -TypeName Renci.SshNet.PasswordAuthenticationMethod `
                             -ArgumentList UserName, Password;

$logger.LogInformation($ftpClient.GetType());
";

            var tmpScriptFilePath = Path.GetTempFileName();
            File.WriteAllText(tmpScriptFilePath, scriptContent);

            /* logging */
            var logFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(logFolderPath);

            /* args */
            var args = $"exec --script {tmpScriptFilePath} --id MyId --log-folder {logFolderPath} --arg param=MyMessage"
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
                    .Skip(1)
                    .Take(2)
                    .Select(value => value.Substring(21))
                    .ToArray();

                var expected = new string[]
                {
                    "[INF] (MyId) FluentFTP.FtpClient",
                    "[INF] (MyId) Renci.SshNet.PasswordAuthenticationMethod"
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