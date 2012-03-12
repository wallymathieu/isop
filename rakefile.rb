require 'albacore'

task :default => ['ms:build']
namespace :ms do
  desc "build using msbuild"
  msbuild :build do |msb|
    msb.properties :configuration => :Debug
    msb.targets :Clean, :Rebuild
    msb.verbosity = 'quiet'
    msb.solution = "Isop.Wpf.sln"
  end
  desc "test using nunit console"
  nunit :test => :build do |nunit|
    nunit.command = "packages/NUnit.2.5.9.10348/Tools/nunit-console.exe"
    nunit.assemblies "Tests/bin/Debug/Tests.dll", "Isop.Wpf.Tests/bin/Debug/Isop.Wpf.Tests.dll"
  end
  desc "copy example cli to wpf and cli folder"
  task :copy_cli => :build do
    cp "Example.Cli/bin/Debug/Example.Cli.dll", "Isop.Wpf/bin/Debug"
    cp "Example.Cli/bin/Debug/Example.Cli.dll", "Isop.Auto.Cli/bin/Debug"
  end
end


namespace :mono do
  desc "build isop on mono"
  xbuild :build do |msb|
    msb.properties :configuration => :Debug
    msb.targets :rebuild
    msb.verbosity = 'quiet'
    msb.solution = "Isop.sln"
  end

  task :test => :build do
    # does not work for some reason 
    command = "nunit-console"
    assemblies = "Tests.dll"
    cd "Tests/bin/Debug" do
      sh "#{command} #{assemblies}"
    end
  end
  desc "copy example cli to cli folder"
  task :copy_cli => :build do
    cp "Example.Cli/bin/Debug/Example.Cli.dll", "Isop.Auto.Cli/bin/Debug"
  end
end

