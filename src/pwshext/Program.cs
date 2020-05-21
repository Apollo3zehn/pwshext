using Microsoft.Extensions.Logging;
using OneDas.Hdf.VdsTool.Commands;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OneDas.Hdf.VdsTool
{
    class Program
    {
        #region Fields

        private static ILoggerFactory _loggerFactory;

        #endregion

        #region Methods

        private static async Task<int> Main(string[] args)
        {
            Console.Title = "Powershell Tool";

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            // paths
            var appdataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pwshext");
            Directory.CreateDirectory(appdataFolderPath);

            var logFolderPath = Path.Combine(appdataFolderPath, "LOGS");
            Directory.CreateDirectory(logFolderPath);

            // configure logging
            var template = "{Timestamp:yyyy-MM-ddTHH:mm:ss} {context} [{Level:u3}] ({SourceContext}) {Message}{NewLine}{Exception}";

            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddFile(Path.Combine(logFolderPath, "VdsTool-{Date}.txt"), outputTemplate: template);
            });

            // configure CLI
            var rootCommand = new RootCommand("Powershell Tool");

            rootCommand.AddCommand(Program.PrepareExecCommand());

            return await rootCommand.InvokeAsync(args);
        }

        #endregion

        #region Commands

        private static Command PrepareExecCommand()
        {
            var command = new Command("exec", "Runs the provided Powershell script")
            {
                new Option("--script-path", "The location of the powershell script")
                {
                    Argument = new Argument<string>(),
                    Required = true
                },
                new Option("--transaction-id", "Log messages are tagged with the transaction identifier")
                {
                    Argument = new Argument<string>(),
                    Required = true
                }
            };

            command.Handler = CommandHandler.Create((string scriptPath, string transactionId) =>
            {
                var logger = _loggerFactory.CreateLogger($"EXEC ({transactionId})");

                try
                {
                    new ExecCommand(scriptPath, logger).Run();
                    logger.LogInformation($"Execution of the 'pwsh' command finished successfully (path: '{scriptPath}').");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Execution of the 'pwsh' command failed (path: '{scriptPath}'). Error message: '{ex.Message}'.");
                    return 1;
                }

                return 0;
            });

            return command;
        }

        #endregion
    }
}
