require 'visual_studio_files.rb'
require 'albacore'

require 'rbconfig'
require 'nuget_helper'

$dir = File.join(File.dirname(__FILE__),'src')
$sln = File.join($dir, "Isop.sln")

desc "Install missing NuGet packages."
task :restore do
  NugetHelper.exec("restore #{$sln}")
end

desc "build"
build :build => [:restore] do |msb|
  msb.prop :configuration, :Debug
  msb.prop :platform, "Mixed Platforms"
  msb.target = :Rebuild
  msb.be_quiet
  msb.nologo
  msb.sln = $sln
end

build :build_release => [:restore] do |msb|
  msb.prop :configuration, :Release
  msb.prop :platform, "Mixed Platforms"
  msb.target = :Rebuild
  msb.be_quiet
  msb.nologo
  msb.sln = $sln
end

task :default => ['build']

desc "test using nunit console"
test_runner :test => [:build] do |nunit|
  nunit.exe = NugetHelper.nunit_path
  files = Dir.glob(File.join($dir, "*Tests", "**", "bin", "Debug", "*Tests.dll"))
  nunit.files = files 
end

desc "copy example cli to wpf and cli folder"
task :copy_cli => :build do
  cp File.join($dir,"Example/bin/Debug/Example.exe"), File.join($dir,"Isop.Auto.Cli/bin/Debug")
end

desc "create the nuget packages"
task :nugetpack => [:build_release, :core_nugetpack, :cli_nugetpack]

task :core_nugetpack => [:build_release] do |nuget|
  cd File.join($dir, "Isop") do
    NugetHelper::exec "pack Isop.csproj"
  end
end

task :cli_nugetpack => [:build_release] do |nuget|
  cd File.join($dir, "Isop.Auto.Cli") do
    NugetHelper::exec "pack Isop.Cli.csproj"
  end
end

task :core_nugetpush do |nuget|
  cd File.join($dir, "Isop") do
    latest = NugetHelper.last_version(Dir.glob("Isop.*.nupkg"))
    NugetHelper::exec("push #{latest}")
  end
end
