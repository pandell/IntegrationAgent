# Pandell Integration Agent

A tool for running code from specified NuGet package. It's auto-updating and installs all related package dependencies. 
It could be used as deployment tool in environments with limited control. 

## How it works

First IntegrationAgent check the remote repositories for its own package and update self if newer found.
Then it finds the latest version of the package it needs to run.
If the package is not in the local packages repository then it downloads and install the package with all its dependencies.
Finally it finds the execution point of the target assembly and pass the token parameter to it.
It's a package responsibility to validate and parse the token.

## Installation and Run

Copy the executable to a desired folder and give the user who will execute it write permissions to this folder.
Start by calling the executable:
```
IntegrationAgent.exe
```

## Configuration and Usage

The IntegrationAgent will look for JSON configuration file with the same name and folder from where it's running.
An example configuration file.
```json
{
	"package": "Package.Name",
	"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ",
	"repositoryUsername": "nuget-user",
	"repositoryPassword": "password",
	"repository": "http://host/NugetServer/nuget"
}
``` 
Parameters could be also supplied as command line arguments to the executable (command line arguments have precedence).
The `package` and `token` are obviously needed to locate the package and pass a string token argument to the execution delegate.

## Execution delegate

IntegrationAgent uses MEF to locate the execution delegate.
Define the execution point as follow in some class from the target assembly.
```csharp
[Export("IntegrationAgentMain")]
public Action<string, Func<TraceLevel, string, bool>> Run 
{
	get { return Export.DoExport; }
}
```

## License

MIT © [Pandell Technology](http://pandell.com/)