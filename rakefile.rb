require 'visual_studio_files.rb'
require 'albacore'

require 'rbconfig'
require_relative './src/.nuget/nuget'
#http://stackoverflow.com/questions/11784109/detecting-operating-systems-in-ruby

$dir = File.join(File.dirname(__FILE__),'src')
$nugetfolder = File.join(File.dirname(__FILE__),'nuget')

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
  package_paths = FileList["src/**/packages.config"]+["src/.nuget/packages.config"]

  package_paths.each.each do |filepath|
      NuGet::exec("i #{filepath} -o ./src/packages")
  end
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

task :default => ['build']

desc "test using nunit console"
test_runner :test => [:build] do |nunit|
  nunit.exe = NuGet::nunit_path
  files = [File.join($dir,"Tests/bin/Debug/Tests.dll")]
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

task :core_copy_to_nuspec => [:build] do
  output_directory_lib = File.join($nugetfolder,"Isop/lib/40/")
  mkdir_p output_directory_lib
  ['Isop'].each{ |project|
    cp Dir.glob(File.join($dir,"#{project}/bin/Debug/*.dll")), output_directory_lib
  }
  
end
task :runners_copy_to_nuspec => [:build] do
  output_directory_lib = File.join($nugetfolder,"Isop.Runners/tools/")
  mkdir_p output_directory_lib
  ['Isop.Wpf', 'Isop.Auto.Cli'].each{ |project|
    cp Dir.glob(File.join($dir,"#{project}/bin/Debug/*.exe")), output_directory_lib
  }
end

desc "create the nuget package"
task :nugetpack => [:core_nugetpack, :runners_nugetpack]

task :core_nugetpack => [:core_copy_to_nuspec] do |nuget|
  cd File.join($nugetfolder,"Isop") do
    NuGet::exec "pack Isop.nuspec"
  end
end

task :runners_nugetpack => [:runners_copy_to_nuspec] do |nuget|
  cd File.join($nugetfolder,"Isop.Runners") do
    NuGet::exec "pack Isop.Runners.nuspec"
  end
end

desc "regenerate links in isop cli"
task :regen_cli do
  isop = VisualStudioFiles::CsProj.new(File.open(File.join($dir,'Isop','Isop.csproj'), "r").read)
  isop_files = isop.files.select do |file|
    file.type=='Compile' && !file.file.end_with?('AssemblyInfo.cs')
  end
  
  cli = VisualStudioFiles::CsProj.new(File.open(File.join($dir,'Isop.Auto.Cli','Isop.Cli.csproj'), "r").read)
  cli.clear_links
  isop_files.each do |file|
    hash = file.to_hash
    hash[:file] = "..\\Isop\\#{file.file}"
    hash[:link] = "Isop\\#{file.file}"
    cli.add(hash)
  end
  File.open(File.join($dir,'Isop.Auto.Cli','Isop.Cli.csproj'), "w") do |f|
    cli.write f
  end
end
