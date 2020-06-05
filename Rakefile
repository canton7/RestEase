require 'json'

RESTEASE_SLN = 'src/RestEase.sln'
RESTEASE_DIR = 'src/RestEase'
SOURCE_GENERATOR_DIR = 'src/RestEase.SourceGenerator'
TESTS_DIR = 'src/RestEase.UnitTests'
SOURCE_GENERATOR_TESTS_DIR = 'src/RestEase.SourceGenerator.UnitTests'

RESTEASE_CSPROJ = File.join(RESTEASE_DIR, 'RestEase.csproj')
SOURCE_GENERATOR_CSPROJ = File.join(SOURCE_GENERATOR_DIR, 'RestEase.SourceGenerator.csproj')
SOURCE_GENERATOR_NUSPEC = File.join(SOURCE_GENERATOR_DIR, 'RestEase.SourceGenerator.nuspec')
NUGET_DIR = File.join(File.dirname(__FILE__), 'NuGet')

desc "Restore NuGet packages"
task :restore do
  sh 'dotnet', 'restore', RESTEASE_SLN
end

desc "Create NuGet package"
task :package do
  sh 'dotnet', 'pack', '--no-build', '--configuration=Release', "--output=#{NUGET_DIR}", '--include-symbols', RESTEASE_DIR
  sh 'dotnet', 'pack', '--no-build', '--configuration=Release', "--output=#{NUGET_DIR}", SOURCE_GENERATOR_DIR
end

desc "Bump version number"
task :version, [:version] do |t, args|
  version = args[:version]

  content = IO.read(RESTEASE_CSPROJ)
  content[/<Version>(.+?)<\/Version>/, 1] = version
  File.open(RESTEASE_CSPROJ, 'w'){ |f| f.write(content) }

  sg_content = IO.read(SOURCE_GENERATOR_CSPROJ)
  sg_content[/<Version>(.+?)<\/Version>/, 1] = version
  File.open(SOURCE_GENERATOR_CSPROJ, 'w'){ |f| f.write(sg_content) }

  sg_nuspec_content = IO.read(SOURCE_GENERATOR_NUSPEC)
  sg_nuspec_content[/<version>(.+?)<\/version>/, 1] = version
  # Be conservative and say we're not compatible with 2.0+
  sg_nuspec_content[/<dependency id="RestEase" version="(.+?)"/, 1] = "[#{version},2.0)"
  File.open(SOURCE_GENERATOR_NUSPEC, 'w'){ |f| f.write(sg_nuspec_content) }
end

desc "Build the project for release"
task :build do
  sh 'dotnet', 'build', '--configuration=Release', '/p:ContinuousIntegrationBuild=true', RESTEASE_DIR
  sh 'dotnet', 'build', '--configuration=Release', SOURCE_GENERATOR_DIR
end

desc "Run tests"
task :test do
  # AppVeyor doesn't have the VS SDK, so it can't build the Visx if we ask it to test the sln
  Dir.chdir(TESTS_DIR) do
    sh 'dotnet', 'test'
  end
  Dir.chdir(SOURCE_GENERATOR_TESTS_DIR) do
    sh 'dotnet', 'test'
  end
end
