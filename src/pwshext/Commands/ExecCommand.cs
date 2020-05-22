using FluentFTP;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace OneDas.Hdf.VdsTool.Commands
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

                var logger = new PwshLogger(_logger);
                _logger.LogInformation($"Executing script '{_scriptFilePath}'.");

                ps.Runspace.SessionStateProxy.SetVariable("logger", logger);
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
