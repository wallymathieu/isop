
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
    nunit.command = Dir.glob(File.join(dir,"packages/NUnit.Runners.*/Tools/nunit-console.exe")).first
    nunit.assemblies File.join(dir,"Tests/bin/Debug/Tests.dll"), File.join(dir,"Isop.Wpf.Tests/bin/Debug/Isop.Wpf.Tests.dll")
  end
  desc "copy example cli to wpf and cli folder"
  task :copy_cli => :build do
    cp File.join(dir,"Example.Cli/bin/Debug/Example.Cli.dll"), File.join(dir,"Isop.Wpf/bin/Debug")
    cp File.join(dir,"Example.Cli/bin/Debug/Example.Cli.dll"), File.join(dir,"Isop.Auto.Cli/bin/Debug")
  end

  task :core_copy_to_nuspec => [:build] do
    output_directory_lib = File.join(dir,"nuget/Isop/lib/40/")
    mkdir_p output_directory_lib
    ['Isop'].each{ |project|
      cp Dir.glob("./#{project}/bin/Debug/*.dll"), output_directory_lib
    }
    
  end
  task :runners_copy_to_nuspec => [:build] do
    output_directory_lib = File.join(dir,"nuget/Isop.Runners/tools/")
    mkdir_p output_directory_lib
    ['Isop.Wpf', 'Isop.Auto.Cli'].each{ |project|
      cp Dir.glob("./#{project}/bin/Debug/*.exe"), output_directory_lib
    }
  end

  desc "create the nuget package"
  task :nugetpack => [:core_nugetpack, :runners_nugetpack]

  task :core_nugetpack => [:core_copy_to_nuspec] do |nuget|
    cd File.join(dir,"nuget/Isop") do
      sh "..\\..\\.nuget\\NuGet.exe pack Isop.nuspec"
    end
  end

  task :runners_nugetpack => [:runners_copy_to_nuspec] do |nuget|
    cd File.join(dir,"nuget/Isop.Runners") do
      sh "..\\..\\.nuget\\NuGet.exe pack Isop.Runners.nuspec"
    end
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
