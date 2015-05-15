desc "Create NuGet package"
task :package do
  sh "NuGet/nuget.exe pack NuGet/RestEase.nuspec"
  sh "NuGet/GitLink.exe . -u https://github.com/canton7/RestEase -f src/RestEase.sln -ignore RestEaseUnitTests"
end