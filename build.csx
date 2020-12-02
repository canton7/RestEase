#!/usr/bin/env dotnet-script

#r "nuget: SimpleTasks, 0.9.4"

using SimpleTasks;
using static SimpleTasks.SimpleTask;

#nullable enable

string restEaseDir = "src/RestEase";
string sourceGeneratorDir = "src/RestEase.SourceGenerator";
string httpClientFactoryDir = "src/RestEase.HttpClientFactory";

string testsDir = "src/RestEase.UnitTests";
string sourceGeneratorTestsDir = "src/RestEase.SourceGenerator.UnitTests";

string nugetDir = "NuGet";

CreateTask("build").Run((string versionOpt, string configurationOpt) =>
{
    var flags = CommonFlags(versionOpt, configurationOpt);
    Command.Run("dotnet", $"build {flags} \"{restEaseDir}\"");
    Command.Run("dotnet", $"build {flags} \"{httpClientFactoryDir}\"");
    Command.Run("dotnet", $"build {flags} -p:VersionSuffix=\"preview\" \"{sourceGeneratorDir}\"");
});

CreateTask("package").DependsOn("build").Run((string version, string configurationOpt) =>
{
    var flags = CommonFlags(version, configurationOpt) + $" --no-build --output=\"{nugetDir}\"";
    Command.Run("dotnet", $"pack {flags} \"{restEaseDir}\"");
    Command.Run("dotnet", $"pack {flags} \"{httpClientFactoryDir}\"");
    Command.Run("dotnet", $"pack {flags} \"{sourceGeneratorDir}\"");
});

string CommonFlags(string? version, string? configuration) =>
    $"--configuration={configuration ?? "Release"} -p:VersionPrefix=\"{version ?? "0.0.0"}\"";

CreateTask("test").Run(() =>
{
    Command.Run("dotnet", $"test \"{testsDir}\"");
    Command.Run("dotnet", $"test \"{sourceGeneratorTestsDir}\"");
});

return InvokeTask(Args);
