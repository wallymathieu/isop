require 'visual_studio_files.rb'
require 'albacore'

require 'rbconfig'
require_relative './src/.nuget/nuget'
#http://stackoverflow.com/questions/11784109/detecting-operating-systems-in-ruby

$dir = File.join(File.dirname(__FILE__),'src')

def with_mono_properties msb
  solution_dir = File.join(File.dirname(__FILE__),'src')
  nuget_tools_path = File.join(solution_dir, '.nuget')
  msb.prop :SolutionDir, solution_dir
  msb.prop :NuGetToolsPath, nuget_tools_path
  msb.prop :NuGetExePath, File.join(nuget_tools_path, 'NuGet.exe')
  msb.prop :PackagesDir, File.join(solution_dir, 'packages')
end

desc "Install missing NuGet packages."
task :install_packages do
  if NuGet::os == :windows
    sln =File.join($dir, "Isop.Wpf.sln")
  else
    sln =File.join($dir, "Isop.sln")
  end
  NuGet::exec("restore #{sln}")
end

desc "build"
build :build => [:install_packages] do |msb|
  msb.prop :configuration, :Debug
  msb.prop :platform, "Mixed Platforms"
  if NuGet::os != :windows
    with_mono_properties msb
  end
  msb.target = :Rebuild
  msb.be_quiet
  msb.nologo
  if NuGet::os == :windows
    msb.sln =File.join($dir, "Isop.Wpf.sln")
  else
    msb.sln =File.join($dir, "Isop.sln")
  end
end

build :build_release => [:install_packages] do |msb|
  msb.prop :configuration, :Release
  msb.prop :platform, "Mixed Platforms"
  if NuGet::os != :windows
    with_mono_properties msb
  end
  msb.target = :Rebuild
  msb.be_quiet
  msb.nologo
  if NuGet::os == :windows
    msb.sln =File.join($dir, "Isop.Wpf.sln")
  else
    msb.sln =File.join($dir, "Isop.sln")
  end
end


task :default => ['build']

desc "test using nunit console"
test_runner :test => [:build] do |nunit|
  nunit.exe = NuGet::nunit_path
  files = [File.join($dir,"Tests/bin/Debug/Tests.dll"), File.join($dir,"Isop.Client.Tests/bin/Debug/Isop.Client.Tests.dll")]
  if NuGet::os == :windows
    files.push File.join($dir,"Isop.Wpf.Tests/bin/Debug/Isop.Wpf.Tests.dll")
  end
  nunit.files = files 
end

desc "copy example cli to wpf and cli folder"
task :copy_cli => :build do
  cp File.join($dir,"Example/bin/Debug/Example.exe"), File.join($dir,"Isop.Server/bin/Debug")
  cp File.join($dir,"Example/bin/Debug/Example.exe"), File.join($dir,"Isop.Auto.Cli/bin/Debug")
end

desc "create the core nuget package"
task :core_nugetpack => [:build_release] do |nuget|
  cd File.join($dir, "Isop") do
    NuGet::exec "pack Isop.nuspec"
  end
end

desc "create the core nuget package"
task :core_nugetpush do |nuget|
  cd File.join($dir, "Isop") do
    NuGet::exec "push "+Dir.glob("Isop.*.nupkg").last
  end
end

desc "build nuget packages"
build :build_nuget_packages => [:install_packages] do |msb|
  msb.prop :configuration, :Release
  msb.prop :platform, "Mixed Platforms"
  if NuGet::os != :windows
    with_mono_properties msb
  end
  msb.prop :RunOctoPack, true
  msb.target = :Build
  msb.be_quiet
  msb.nologo
  if NuGet::os == :windows
    msb.sln =File.join($dir, "Isop.Wpf.sln")
  else
    msb.sln =File.join($dir, "Isop.sln")
  end
end
