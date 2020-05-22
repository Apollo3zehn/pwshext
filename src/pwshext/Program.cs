using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        #region Methods

        internal static async Task<int> Main(string[] args)
        {
            Console.Title = "Powershell Tool";

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            // paths
            var appdataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pwshext");
            Directory.CreateDirectory(appdataFolderPath);

            var logFolderPath = Path.Combine(appdataFolderPath, "LOGS");
            Directory.CreateDirectory(logFolderPath);

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
                new Option<FileInfo>("--script", "The location of the powershell script")
                {
                    Required = true
                },
                new Option<string>("--id", "Log messages are tagged with the identifier")
                {
                    Required = true
                },
                new Option<string>("--log-folder", "The parent folder of the log files")
                {
                    Required = false
                },
                new Option("--log-level", "The log level. Options are: Trace, Debug, Information, Warning, Error or Critical.")
                {
                    Argument = new Argument<LogLevel>(() => LogLevel.Information),
                    Required = false
                },
                new Option("--arg", @"An argument for the function or script in the form of --arg argname=argvalue or --arg ""argname=argvalue with space"" Repeat this for every argument to append.")
                {
                    Argument = new Argument<string>() { Arity = ArgumentArity.ZeroOrMore }
                }
            };

            command.Handler = CommandHandler.Create((FileInfo script, string id, string logFolder, LogLevel logLevel, string[] arg) =>
            {
                // configure logging
                if (!string.IsNullOrWhiteSpace(logFolder))
                    Directory.CreateDirectory(logFolder);

                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.SetMinimumLevel(logLevel);
                    builder.AddConsole();
                });

                /* add serilog through ILoggerFactory instead of ILoggingBuilder to dispose the logger along with the ILoggerFactory (found in source code) */
                if (!string.IsNullOrWhiteSpace(logFolder))
                {
                    var template = "{Timestamp:yyyy-MM-ddTHH:mm:ss} {context} [{Level:u3}] ({SourceContext}) {Message}{NewLine}{Exception}";
                    loggerFactory.AddFile(Path.Combine(logFolder, "pwshext-{Date}.txt"), minimumLevel: logLevel, outputTemplate: template);
                }

                var logger = loggerFactory?.CreateLogger(id) ?? NullLogger.Instance;

                try
                {
                    var args = arg;
                    new ExecCommand(script.FullName, args ?? new string[0], logger).Run();
                    logger.LogInformation($"Execution of the 'exec' command finished successfully (path: '{script.FullName}').");
                }
                catch (Exception ex)
                {
                    logger.LogError($"Execution of the 'exec' command failed (path: '{script.FullName}'). Error message: '{ex.Message}'.");
                    return 1;
                }

                return 0;
            });

            return command;
        }

        #endregion
    }
}
