require 'json'

RESTEASE_DIR = 'src/RestEase'
TESTS_DIR = 'src/RestEaseUnitTests'

ASSEMBLY_INFO = File.join(RESTEASE_DIR, 'Properties/AssemblyInfo.cs')
RESTEASE_JSON = File.join(RESTEASE_DIR, 'project.json')

desc "Create NuGet package"
task :package do
  sh 'dotnet', 'pack', '--no-build', '--configuration=Release', '--output=NuGet', RESTEASE_DIR
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

  content = JSON.parse(File.read(RESTEASE_JSON))
  content['version'] = args[:version]
  File.open(RESTEASE_JSON, 'w'){ |f| f.write(JSON.pretty_generate(content)) }
end

desc "Build the project for release"
task :build do
  sh 'dotnet', 'build', '--configuration=Release', RESTEASE_DIR
end

desc "Run tests"
task :test do
  sh 'dotnet', 'test', TESTS_DIR
end
