ASSEMBLY_INFO = 'src/RestEase/Properties/AssemblyInfo.cs'
PROJECT_JSON = './src/RestEase/project.json'
GITLINK_REMOTE = 'https://github.com/canton7/RestEase'
DOTNET = %q{dotnet}
CD = %q{cd}

desc "Create NuGet package"
task :package do
  local_hash = `git rev-parse HEAD`.chomp
  sh "NuGet/GitLink.exe . -c \"Release 4.0\" -s #{local_hash} -u #{GITLINK_REMOTE} -f src/RestEase.sln -ignore RestEaseUnitTests -ignore RestEaseUnitTests.Net40"
  sh "NuGet/GitLink.exe . -c \"Release 4.5\" -s #{local_hash} -u #{GITLINK_REMOTE} -f src/RestEase.sln -ignore RestEaseUnitTests -ignore RestEaseUnitTests.Net40"
  sh DOTNET, 'restore'
  Dir.chdir(File.dirname(PROJECT_JSON)) do
    sh DOTNET, 'pack', "--configuration=Release", "--output=../.."
  end
end

desc "Bump version number"
task :version, [:version] do |t, args|
  parts = args[:version].split('.')
  parts << '0' if parts.length == 3
  version = parts.join('.')

  content = IO.read(ASSEMBLY_INFO)
  content[/^\[assembly: AssemblyVersion\(\"(.+?)\"\)\]/, 1] = version
  content[/^\[assembly: AssemblyFileVersion\(\"(.+?)\"\)\]/, 1] = version
  File.open(ASSEMBLY_INFO, 'w'){ |f| f.write(content) }

  content = IO.read(PROJECT_JSON)
  content[/\"version\":\s*\"(.+?)\"/, 1] = args[:version]
  File.open(PROJECT_JSON, 'w'){ |f| f.write(content) }
end

desc "Build the project for release"
task :build do
  sh DOTNET, 'restore'
  Dir.chdir(File.dirname(PROJECT_JSON)) do
    sh DOTNET, 'build', "--configuration=Release"
  end
end
