using FluentFTP;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace pwshext.Commands
{
    public class ExecCommand
    {
        #region Fields

        private string _scriptFilePath;
        private string[] _args;
        private ILogger _logger;

        #endregion

        public ExecCommand(string scriptFilePath, string[] args, ILogger logger)
        {
            _scriptFilePath = scriptFilePath;
            _args = args;
            _logger = logger;
        }

        public void Run()
        {
            using (PowerShell ps = PowerShell.Create())
            {
                // ensure FluentFTP lib is loaded
                _ = new FtpClient();

                // ensure SSH.NET is loaded
                _ = new PasswordAuthenticationMethod("Test", "User");

                // create listener
                ps.Streams.Verbose.DataAdded += (sender, e) => _logger.LogTrace(ps.Streams.Verbose[e.Index].Message);
                ps.Streams.Debug.DataAdded += (sender, e) => _logger.LogDebug(ps.Streams.Debug[e.Index].Message);
                ps.Streams.Information.DataAdded += (sender, e) => _logger.LogInformation(ps.Streams.Information[e.Index].MessageData.ToString());
                ps.Streams.Warning.DataAdded += (sender, e) => _logger.LogWarning(ps.Streams.Warning[e.Index].Message);
                ps.Streams.Error.DataAdded += (sender, e) => _logger.LogError(ps.Streams.Error[e.Index].Exception.Message);

                ps.Runspace.SessionStateProxy.SetVariable("scriptRoot", Path.GetDirectoryName(_scriptFilePath));

                ps.AddScript(File.ReadAllText(_scriptFilePath))
                    .AddParameters(this.PrepareArgs(_args))
                    .Invoke();
            }
        }

        private Dictionary<string, string> PrepareArgs(string[] args)
        {
            var result = new Dictionary<string, string>();

            foreach (var arg in args)
            {
                try
                {
                    var argsSplitted = arg
                        .Split('=', 2)
                        .Select(value => value.Trim())
                        .ToArray();

                    result[argsSplitted[0]] = argsSplitted[1];
                }
                catch
                {
                    //
                }
            }

            return result;
        }
    }
}
