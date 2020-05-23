using Microsoft.Extensions.Logging;
using pwshext.Commands;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace pwshext
{
    class Program
    {
        #region Methods

        internal static async Task<int> Main(string[] args)
        {
            Console.Title = "Powershell Extended";
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            // configure CLI
            var rootCommand = new RootCommand("Powershell Extended");
            rootCommand.AddCommand(Program.PrepareExecCommand());

            return await rootCommand.InvokeAsync(args);
        }

        #endregion

        #region Commands

        private static Command PrepareExecCommand()
        {
            var defaultTemplate = "{Timestamp:yyyy-MM-ddTHH:mm:ss} [{Level:u3}] ({SourceContext}) {Message}{NewLine}";

            var command = new Command("exec", "Runs the provided Powershell script")
            {
                new Option(new string[] { "--logger", "-l" }, "Enable logging using the provided logger name.")
                {
                    Argument = new Argument<string>("logger name", () => "default"),
                },
                new Option(new string[] { "--console-logger", "-cl" }, "Enable the console logger.")
                {
                    //
                },
                new Option(new string[] { "--file-logger", "-fl" }, "Enable the serilog file logger.")
                {
                    Argument = new Argument<string>("parent folder path"),
                },
                new Option(new string[] { "--log-level", "-ll" }, "The minimum log level. The default is 'Information'.")
                {
                    Argument = new Argument<LogLevel>(() => LogLevel.Information),
                },
                new Option(new string[] { "--log-template", "-lt" }, $"The serilog log template. The default is {defaultTemplate}.")
                {
                    Argument = new Argument<string>("template", () => defaultTemplate),
                },
                new Option(new string[] { "--arg", "-a" }, @"An argument for the script to be called. Use --arg argname=argvalue or --arg ""argname=argvalue with space"". Repeat this for each argument.")
                {
                    Argument = new Argument<string[]>(),
                }
            };

            command.AddArgument(new Argument<string>("script-path"));

            command.Handler = CommandHandler.Create((string logger, bool consoleLogger, string fileLogger, LogLevel logLevel, string logTemplate, string[] arg, string scriptPath) =>
            {
                // logger factory
                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.SetMinimumLevel(logLevel);

                    // add console logger
                    if (consoleLogger)
                        builder.AddConsole();
                });

                // add file logger
                if (!string.IsNullOrWhiteSpace(fileLogger))
                {
                    /* add serilog through ILoggerFactory instead of ILoggingBuilder to dispose the logger along with the ILoggerFactory (found in source code) */
                    Directory.CreateDirectory(fileLogger);
                    loggerFactory.AddFile(Path.Combine(fileLogger, "pwshext-{Date}.txt"), minimumLevel: logLevel, outputTemplate: logTemplate);
                }

                // add default logger
                if (!consoleLogger)
                    loggerFactory.AddProvider(new DefaultLoggerProvider());

                // create logger
                var loggerInstance = loggerFactory.CreateLogger(logger);

                try
                {
                    var args = arg;
                    new ExecCommand(scriptPath, args ?? new string[0], loggerInstance).Run();
                }
                catch (Exception ex)
                {
                    loggerInstance.LogError(ex.Message);
                    return 1;
                }

                return 0;
            });

            return command;
        }

        #endregion
    }
}
