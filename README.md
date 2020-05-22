# pwshext

[![AppVeyor](https://ci.appveyor.com/api/projects/status/github/apollo3zehn/pwshext?svg=true)](https://ci.appveyor.com/project/Apollo3zehn/pwshext)
[![NuGet](https://img.shields.io/nuget/vpre/pwshext.svg)](https://www.nuget.org/packages?q=pwshext)

# Installation
```pwsh
dotnet tool install --global --version 7.0.100-preview.1.final pwshext
```

# Sample usage

```pwsh
pwshext.exe exec --script .\sample.ps1 --id MyId
```

Or with logging into the current folder: 
```
pwshext.exe exec --script .\sample.ps1 --id MyId --log-folder .
```

# Passing arguments
tbd

# Connect to a FTP/FTPS server (FluentFTP):

```pwsh
$logger.LogInformation('Welcome to the extended Poweshell runtime.')
$ErrorActionPreference = "Stop"

# general settings
$sourceDir      = '/raw'
$days           = 30

# FTP settings
$hostName       = 'ftp.testserver.org'
$port           = 21
$userName       = 'username'
$password       = 'password'

# start
$ftpClient      = New-Object -TypeName FluentFTP.FtpClient `
                             -ArgumentList $hostName, $port, $username, $password

$ftpClient.Connect()

$logger.LogInformation("Connected to ftp://$($userName)@$($hostName):$($port).")
```

* See also https://github.com/robinrodricks/FluentFTP for a detailed description of the library.

The log output is then:

```
2020-05-22T16:50:16  [INF] (MyId) Executing script 'C:\Users\<username>\Desktop\sample.ps1'.
2020-05-22T16:50:17  [INF] (MyId) Welcome to the extended Poweshell runtime.
2020-05-22T16:50:17  [INF] (MyId) Connected to ftp://username@ftp.testserver.org:21.
2020-05-22T16:50:17  [INF] (MyId) Execution of the 'exec' command finished successfully (path: 'C:\Users\<username>\Desktop\sample.ps1').
```

# Connect to an SFTP server (SSH.NET)

tbd

* See also https://github.com/sshnet/SSH.NET for a detailed description of the library.
