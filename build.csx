#!/usr/bin/env dotnet-script

#r "nuget: SimpleTasks, 0.9.4"

using SimpleTasks;
using static SimpleTasks.SimpleTask;

#nullable enable

string restEaseDir = "src/RestEase";
string httpClientFactoryDir = "src/RestEase.HttpClientFactory";
string sourceGeneratorDir = "src/RestEase.SourceGenerator";

string testsDir = "src/RestEase.UnitTests";
string httpClientFactoryTestsDir = "src/RestEase.HttpClientFactory.UnitTests";
string sourceGeneratorTestsDir = "src/RestEase.SourceGenerator.UnitTests";

CreateTask("build").Run((string versionOpt, string configurationOpt, bool updateCompatSuppression) =>
{
    // We can't have separate build and package steps due to https://github.com/dotnet/sdk/issues/24943
    // We can't run package validation on every build, as it needs a version higher than the baseline (so not 0.0.0)
    // We therefore package on every build (<GeneratePackageOnBuild>true</GeneratePackageOnBuild>), and only turn on package validation when we
    // specify a version.
    string flags = $"--configuration={configurationOpt ?? "Release"} -p:VersionPrefix=\"{versionOpt ?? "0.0.0"}\"";

    string validationFlags = "";
    if (versionOpt != null)
    {
        flags += " -p:GeneratePackageOnBuild=true";
        validationFlags = "-p:EnablePackageValidation=true";
        if (updateCompatSuppression)
        {
            validationFlags += " -p:GenerateCompatibilitySuppressionFile=true";
        }
    }
    else if (updateCompatSuppression)
    {
        throw new Exception("--updateCompatSuppression requires --version");
    }
    
    Command.Run("dotnet", $"build {flags} {validationFlags} \"{restEaseDir}\"");
    Command.Run("dotnet", $"build {flags} {validationFlags} \"{httpClientFactoryDir}\"");
    Command.Run("dotnet", $"build {flags} \"{sourceGeneratorDir}\"");
});

CreateTask("test").Run(() =>
{
    Command.Run("dotnet", $"test \"{testsDir}\"");
    Command.Run("dotnet", $"test \"{httpClientFactoryTestsDir}\"");
    Command.Run("dotnet", $"test \"{sourceGeneratorTestsDir}\"");
});

return InvokeTask(Args);
