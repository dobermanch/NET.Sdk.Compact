# MSBuild Extensions

The project SDK for building .NET 3.5 Compact Framework projects in the Visual Studio 2017+ or by the MSBuild 15+.  

## Build

To build a package localy use latest version of [.NET Core SDK](https://dotnet.microsoft.com/download) or MSBuild.  

```cmd
dotnet pack .\NET.Sdk.Compact.csproj
```

## Usage

To start using it

- Publish the `NET.Sdk.Compact` and `NETCompact.Library` nuget packages to yours NuGet repository
- Create new console app, and update the project file to start using new SDK

``` xml
<Project Sdk="NET.Sdk.Compact">
    
</Project>
```

### Useful Links

[Use MSBuild project SDKs](https://docs.microsoft.com/en-us/visualstudio/msbuild/how-to-use-project-sdk?view=vs-2017)  
[global.json overview](https://docs.microsoft.com/en-us/dotnet/core/tools/global-json)  
[Target frameworks](https://docs.microsoft.com/en-us/dotnet/standard/frameworks)  
[Additions to the csproj format for .NET Core](https://docs.microsoft.com/en-us/dotnet/core/tools/csproj)  
[MSBuild Binary and Structured Log Viewer](http://msbuildlog.com)  
[MSBuild project file schema reference](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-project-file-schema-reference?view=vs-2017)  
[Customize your build](https://docs.microsoft.com/en-us/visualstudio/msbuild/customize-your-build?view=vs-2017)  
[Packages, metapackages and frameworks](https://docs.microsoft.com/en-us/dotnet/core/packages)  