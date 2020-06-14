#!/usr/bin/env dotnet-script

#r "nuget: SimpleTasks, 0.9.1"
#r "nuget: SimpleExec, 6.2.0"

using SimpleExec;
using static SimpleTasks.SimpleTask;

string restEaseDir = "src/RestEase";
string sourceGeneratorDir = "src/RestEase.SourceGenerator";

string testsDir = "src/RestEase.UnitTests";
string sourceGeneratorTestesDir = "src/RestEase.SourceGenerator.UnitTests";

string nugetDir = "NuGet";

CreateTask("build").Run((string versionOpt, string configurationOpt) =>
{
	string version = versionOpt ?? "0.0.0";
	string configuration = configurationOpt ?? "Release";
	Command.Run("dotnet", $"build --configuration={configuration} -p:ContinuousIntegrationBuild=true -p:Version=\"{version}\" \"{restEaseDir}\"");
	Command.Run("dotnet", $"build --configuration={configuration} -p:Version=\"{version}\" \"{sourceGeneratorDir}\"");
});

CreateTask("package").Run((string version) =>
{
	Command.Run("dotnet", $"pack --configuration=Release -p:Version=\"{version}\" --output=\"{nugetDir}\" --include-symbols \"{restEaseDir}\"");
	Command.Run("dotnet", $"pack --configuration=Release -p:Version=\"{version}\" --output=\"{nugetDir}\" \"{sourceGeneratorDir}\"");
});

CreateTask("test").Run(() =>
{
	Command.Run("dotnet", $"test \"{testsDir}\"");
	Command.Run("dotnet", $"test \"{sourceGeneratorTestesDir}\"");
});

return InvokeTask(Args);
