require './visual_studio.rb'
require 'albacore'

task :default => ['isop:ms:build']
task :nugetpack => ['isop:ms:nugetpack']

namespace :isop do

namespace :ms do
  dir = File.join(File.dirname(__FILE__),'src')
  nugetfolder = File.join(File.dirname(__FILE__),'nuget')
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
    output_directory_lib = File.join(nugetfolder,"Isop/lib/40/")
    mkdir_p output_directory_lib
    ['Isop'].each{ |project|
      cp Dir.glob("./#{project}/bin/Debug/*.dll"), output_directory_lib
    }
    
  end
  task :runners_copy_to_nuspec => [:build] do
    output_directory_lib = File.join(nugetfolder,"Isop.Runners/tools/")
    mkdir_p output_directory_lib
    ['Isop.Wpf', 'Isop.Auto.Cli'].each{ |project|
      cp Dir.glob("./#{project}/bin/Debug/*.exe"), output_directory_lib
    }
  end

  desc "create the nuget package"
  task :nugetpack => [:core_nugetpack, :runners_nugetpack]

  task :core_nugetpack => [:core_copy_to_nuspec] do |nuget|
    cd File.join(nugetfolder,"Isop") do
      sh "..\\..\\src\\.nuget\\NuGet.exe pack Isop.nuspec"
    end
  end

  task :runners_nugetpack => [:runners_copy_to_nuspec] do |nuget|
    cd File.join(nugetfolder,"Isop.Runners") do
      sh "..\\..\\src\\.nuget\\NuGet.exe pack Isop.Runners.nuspec"
    end
  end
end

desc "regenerate links in isop cli"
task :regen_cli do
  isop = VisualStudio::CsProj.new(File.open(File.join('Isop','Isop.csproj'), "r").read)
  isop_files = isop.files.select do |file|
    file.type=='Compile' && !file.file.end_with?('AssemblyInfo.cs')
  end
    
  cli = VisualStudio::CsProj.new(File.open(File.join('Isop.Auto.Cli','Isop.Cli.csproj'), "r").read)
  cli.clear_links
  isop_files.each do |file|
    opts = file.to_opts
    opts[:file] = "..\\Isop\\#{file.file}"
    opts[:link] = "Isop\\#{file.file}"
    cli.add(VisualStudio::FileReference.new(opts))
  end
  File.open(File.join('Isop.Auto.Cli','Isop.Cli.csproj'), "w") do |f|
    cli.write f
  end
end

desc "regenerate links in isop wpf"
task :regen_wpf do
  isop = VisualStudio::CsProj.new(File.open(File.join('Isop','Isop.csproj'), "r").read)
  isop_files = isop.files.select do |file|
    file.type=='Compile' && !file.file.end_with?('AssemblyInfo.cs')
  end
  wpfcontrols = VisualStudio::CsProj.new(File.open(File.join('Isop.WpfControls','Isop.WpfControls.csproj'), "r").read)
  wpfcontrol_files = wpfcontrols.files.select do |file|
    !file.file.end_with?('AssemblyInfo.cs')
  end
  
  wpf = VisualStudio::CsProj.new(File.open(File.join('Isop.Wpf','Isop.Wpf.csproj'), "r").read)
  wpf.clear_links
  isop_files.each do |file|
    opts = file.to_opts
    opts[:file] = "..\\Isop\\#{file.file}"
    opts[:link] = "Isop\\#{file.file}"
    wpf.add(VisualStudio::FileReference.new(opts))
  end
  wpfcontrol_files.each do |file|
    opts = file.to_opts
    opts[:file] = "..\\Isop.WpfControls\\#{file.file}"
    opts[:link] = "WpfControls\\#{file.file}"
    wpf.add(VisualStudio::FileReference.new(opts))
  end

  File.open(File.join('Isop.Wpf','Isop.Wpf.csproj'), "w") do |f|
    wpf.write f
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
