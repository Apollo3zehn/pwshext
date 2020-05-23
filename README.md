# pwshext

[![AppVeyor](https://ci.appveyor.com/api/projects/status/github/apollo3zehn/pwshext?svg=true)](https://ci.appveyor.com/project/Apollo3zehn/pwshext)
[![NuGet](https://img.shields.io/nuget/vpre/pwshext.svg)](https://www.nuget.org/packages?q=pwshext)

Powershell Extended aims to bring .NET logging and FTP access functionality. Therefore it has an integrated [Console logger](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1#console), [File logger](https://github.com/serilog/serilog), [FTP(s) client](https://github.com/robinrodricks/FluentFTP) and [SFTP client](https://github.com/sshnet/SSH.NET).

# Versioning
Powershell extended uses the official Powershell version (e.g. 7.0.1) and extendes it with its own version (7.0.100).

# Installation
First make sure you have the .NET Core 3.1 SDK installed. you can find it [here](https://dotnet.microsoft.com/download/dotnet-core/3.1).

Once it is available, run the following command to install the global powershell tool:

```ps
dotnet tool install --global pwshext
```

# Passing arguments

To execute a simple script, just type:
```ps
pwshext exec sample.ps1
```

If you want to pass arguments to the script, you need to add the `--arg` argument for each argument to pass.

```ps
pwshext exec --arg arg1=value1 --arg "arg2=value 2 with space" sample.ps1
```

Make sure your script accepts the desired arguments:

```ps
Param (
    [string]$arg1,
    [string]$arg2
)

Write-Information $arg1;
Write-Information $arg2;
```

# Aliases

There are also aliases to shorten the commands:

`ps --l MyLoggerName --ll Trace script.ps1`

Please see the help (`pwshext exec -h`) for more information.

# Logging

## Console vs. File Logger
By default, all output is printed to the console similar to a standard powershell application. However, you may prefer structured logging. To get this, you need to give the logger a name and add the `--console-logger` option:
```ps
pwshext exec --logger MyLoggerName --console-logger sample.ps1
```

Alternatively, if you need a file logger (using serilog), do the following:

```ps
pwshext exec --logger MyLoggerName --file-logger <parent-folder-path> sample.ps1
```

## Writing Messages
To actually log anything, you can use the standard Powershell `Write-XXX` methods:

```ps
# if you want verbose messages to be displayed
$VerbosePreference = 'Continue'

# if you want debug messages to be displayed
$DebugPreference = 'Continue'

Write-Verbose 'Verbose message'
Write-Debug 'Debug message'
Write-Information 'Information message'
Write-Warning 'Warning message'
Write-Error 'Error message'

# you can also use `Write-Host`, which is similar to Write-Information
Write-Host 'Host message'
```

## Unstructured output

With a simple `pwshext exec sample.ps1` the console output looks like:

```
Information message
Warning message
Error message
Host message
```

You may wonder why the `Verbose` and `Debug` messages are missing although `$VerbosePreference` and `$DebugPreference` are set.

`pwshext` uses the .NET logging framework internally (even if you do not add the `--logger` option), which applies a minimum log level filter (default: `Information`). Set it to `Trace` to see all messages. With `pwshext exec --log-level Trace sample.ps1`, the output becomes:

```
Verbose message
Debug message
Information message
Warning message
Error message
Host message
```

## Structured output (console)

With the console logger enabled as shown above (`--logger MyLoggerName --console-logger`), the output gets structured:
```
trce: MyLoggerName[0]
      Verbose message
dbug: MyLoggerName[0]
      Debug message
info: MyLoggerName[0]
      Information message
warn: MyLoggerName[0]
      Warning message
fail: MyLoggerName[0]
      Error message
info: MyLoggerName[0]
      Host message
```

## Structured output (file)

If enabled, the file logger will format the output like this:

```ps
2020-05-23T16:18:28 [VRB] (MyLoggerName) Verbose message
2020-05-23T16:18:28 [DBG] (MyLoggerName) Debug message
2020-05-23T16:18:28 [INF] (MyLoggerName) Information message
2020-05-23T16:18:28 [WRN] (MyLoggerName) Warning message
2020-05-23T16:18:28 [ERR] (MyLoggerName) Error message
2020-05-23T16:18:28 [INF] (MyLoggerName) Host message
```

If you do not like the file output template, you can use your own. E.g:
`ps --logger MyLoggerName --file-logger . --log-level Trace --log-template "{Timestamp:HH:mm} [{Level:u3}] ({SourceContext}) {Message}{NewLine}" script.ps1`

With the new template the file output becomes shorter:
```
16:18 [VRB] (MyLoggerName) Verbose message
16:18 [DBG] (MyLoggerName) Debug message
16:18 [INF] (MyLoggerName) Information message
16:18 [WRN] (MyLoggerName) Warning message
16:18 [ERR] (MyLoggerName) Error message
16:18 [INF] (MyLoggerName) Host message
```

Please see this [Link](https://github.com/serilog/serilog/wiki/Formatting-Output) for an explanation of the serilog format.

# Connect to a FTP(S) server with FluentFTP:

```ps
# FTP settings
$hostName       = 'ftp.testserver.org'
$port           = 21
$userName       = 'username'
$password       = 'password'

# start
$ftpClient      = New-Object -TypeName FluentFTP.FtpClient `
                             -ArgumentList $hostName, $port, $username, $password

$ftpClient.Connect()

$Write-Information "Connected to ftp://$($userName)@$($hostName):$($port)."

# use e.g.:
# $ftpClient.GetListing($sourceDir, $searchOption) | Where-Object ...
# $ftpClient.DownloadFile($targetFile, $sourceFile)
```

> See also https://github.com/robinrodricks/FluentFTP/wiki/Quick-Start-Example for a detailed description of the library.

# Connect to a SFTP server with SSH.NET

```ps
# FTP settings
$hostName       = 'ftp.testserver.org'
$port           = 21
$userName       = 'username'
$password       = 'password'

# start
$ftpClient      = New-Object -TypeName Renci.SshNet.SftpClient `
                             -ArgumentList $hostName, $port, $username, $password

$ftpClient.Connect()

$Write-Information "Connected to ftp://$($userName)@$($hostName):$($port)."

# use e.g.:
# $ftpClient.ListDirectory($sourceDir) | Where-Object ...
# $ftpClient.DownloadFile($sourceFile, $stream)
```

> See also https://github.com/sshnet/SSH.NET for a description of the library. It is not as detailed as that of `FluentFTP` but the client works similar.
