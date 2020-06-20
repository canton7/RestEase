#!/usr/bin/env dotnet-script

#r "nuget: SimpleTasks, 0.9.1"
#r "nuget: SimpleExec, 6.2.0"

using SimpleExec;
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
    Command.Run("dotnet", $"build {flags} -p:ContinuousIntegrationBuild=true \"{restEaseDir}\"");
    Command.Run("dotnet", $"build {flags} -p:ContinuousIntegrationBuild=true \"{httpClientFactoryDir}\"");
    Command.Run("dotnet", $"build {flags} \"{sourceGeneratorDir}\"");
});

CreateTask("package").DependsOn("build").Run((string version, string configurationOpt) =>
{
    var flags = CommonFlags(version, configurationOpt) + $" --no-build --output=\"{nugetDir}\"";
    Command.Run("dotnet", $"pack {flags} --include-symbols \"{restEaseDir}\"");
    Command.Run("dotnet", $"pack {flags} --include-symbols \"{httpClientFactoryDir}\"");
    Command.Run("dotnet", $"pack {flags} \"{sourceGeneratorDir}\"");
});

string CommonFlags(string? version, string? configuration) =>
    $"--configuration={configuration ?? "Release"} -p:Version=\"{version ?? "0.0.0"}\"";

CreateTask("test").Run(() =>
{
    Command.Run("dotnet", $"test \"{testsDir}\"");
    Command.Run("dotnet", $"test \"{sourceGeneratorTestsDir}\"");
});

return InvokeTask(Args);
