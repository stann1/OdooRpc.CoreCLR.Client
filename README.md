# OdooRpc.CoreCLR.Client

Simple [Odoo JSON-RPC](https://www.odoo.com/documentation/9.0/api_integration.html) Client for [.Net Core 3.1](https://www.microsoft.com/net/core).

Inspired by https://github.com/saidimu/odoo and https://github.com/osiell/odoorpc.

### Installation

*TODO*

### Build From Source

To build the library from source, please do:

```Shells
# Clone repository
git clone https://github.com/vmlf01/OdooRpc.CoreCLR.Client.git
cd OdooRpc.CoreCLR.Client

# Restore NuGet packages
dotnet restore

# Build NuGet package
dotnet pack src/OdooRpc.CoreCLR.Client --output build
```

You can also run the tests by doing:

```Shell
# Run tests from repository root
dotnet test tests/OdooRpc.CoreCLR.Client.Tests
```

### Publish NuGet package to repository

You will need nuget.exe on your path and set the NuGet API Key before you can push a package to the NuGet repository.

```Shell
nuget.exe setApiKey 76d7xxxx-xxxx-xxxx-xxxx-eabb8b0cxxxx
nuget.exe push .\build\OdooRpc.CoreCLR.Client.V3.2.2.0.nupkg
```

### Samples
All updated to work with dotnetcore 3.1

1. update the appsettings.json file (note: enter the host without protocol prefix, and set the use of ssl)
2. Alternative 1, create an appsettings.Development.json file and optionally other env settings files, which you don't want source controlled.
Then run the sample project with `DOTNETCORE_ENVIRONMENT=Development dotnet run`
3. Alernative 2, use env variables instead of appsetting values when starting the sample project. Example:
`OdooConnection__Host=my.odoohost.com OdooConnection__Password=mypassword dotnet run`

The above commands are for UNIX, if you are on windows, command-line env vars are set like `set ENV_VAR=value && dotnet run`
