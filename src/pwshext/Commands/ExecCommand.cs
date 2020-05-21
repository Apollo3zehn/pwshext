﻿using FluentFTP;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System.IO;
using System.Management.Automation;

namespace OneDas.Hdf.VdsTool.Commands
{
    public class ExecCommand
    {
        #region Fields

        private string _scriptFilePath;
        private ILogger _logger;

        #endregion

        public ExecCommand(string scriptFilePath, ILogger logger)
        {
            _scriptFilePath = scriptFilePath;
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

                var vdsToolLogger = new PwshLogger(_logger);
                _logger.LogInformation($"Executing script '{_scriptFilePath}'.");

                ps.Runspace.SessionStateProxy.SetVariable("logger", vdsToolLogger);
                ps.Runspace.SessionStateProxy.SetVariable("scriptRoot", Path.GetDirectoryName(_scriptFilePath));

                ps.AddScript(File.ReadAllText(_scriptFilePath))
                  .Invoke();
            }
        }
    }
}
