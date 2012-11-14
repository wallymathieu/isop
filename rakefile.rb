
require 'albacore'

task :default => ['isop:ms:build']
namespace :isop do
namespace :ms do
  dir = File.dirname(__FILE__)
  desc "build using msbuild"
  msbuild :build do |msb|
    msb.properties :configuration => :Debug
    msb.targets :Clean, :Rebuild
    msb.verbosity = 'quiet'
    msb.solution =File.join(dir, "Isop.Wpf.sln")
  end
  desc "test using nunit console"
  nunit :test => :build do |nunit|
    nunit.command = File.join(dir,"packages/NUnit.2.5.9.10348/Tools/nunit-console.exe")
    nunit.assemblies File.join(dir,"Tests/bin/Debug/Tests.dll"), File.join(dir,"Isop.Wpf.Tests/bin/Debug/Isop.Wpf.Tests.dll")
  end
  desc "copy example cli to wpf and cli folder"
  task :copy_cli => :build do
    cp File.join(dir,"Example.Cli/bin/Debug/Example.Cli.dll"), File.join(dir,"Isop.Wpf/bin/Debug")
    cp File.join(dir,"Example.Cli/bin/Debug/Example.Cli.dll"), File.join(dir,"Isop.Auto.Cli/bin/Debug")
  end
end



namespace :mono do
  dir = File.dirname(__FILE__)
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
end
