ASSEMBLY_INFO = 'src/RestEase/Properties/AssemblyInfo.cs'
NUSPEC = 'NuGet/RestEase.nuspec'
CSPROJ = 'src/RestEase/RestEase.csproj'
MSBUILD = %q{C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe}

desc "Create NuGet package"
task :package => :build do
  sh "NuGet/GitLink.exe . -u https://github.com/canton7/RestEase -f src/RestEase.sln -ignore RestEaseUnitTests"
  Dir.chdir('NuGet') do
    sh "nuget.exe pack RestEase.nuspec"
  end
end

desc "Bump version number"
task :version, [:version] do |t, args|
  content = IO.read(ASSEMBLY_INFO)
  content[/\[assembly: AssemblyVersion\(\"(.+?).0\"\)\]/, 1] = args[:version]
  content[/\[assembly: AssemblyFileVersion\(\"(.+?).0\"\)\]/, 1] = args[:version]
  File.open(ASSEMBLY_INFO, 'w'){ |f| f.write(content) }

  content = IO.read(NUSPEC)
  content[/<version>(.+?)<\/version>/, 1] = args[:version]
  File.open(NUSPEC, 'w'){ |f| f.write(content) }
end

desc "Build the project for release"
task :build do
  sh MSBUILD, CSPROJ, "/t:Clean;Rebuild", "/p:Configuration=Release", "/verbosity:normal"
end