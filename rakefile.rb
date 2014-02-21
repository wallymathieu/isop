require 'visual_studio_files.rb'
require 'albacore'

task :default => ['ms:build']
task :nugetpack => ['ms:nugetpack']

$dir = File.join(File.dirname(__FILE__),'src')
$nugetfolder = File.join(File.dirname(__FILE__),'nuget')

namespace :ms do
  desc "build using msbuild"
  msbuild :build do |msb|
    msb.properties :configuration => :Debug
    msb.targets :Clean, :Rebuild
    msb.verbosity = 'quiet'
    msb.solution =File.join($dir, "Isop.Wpf.sln")
  end
  desc "test using nunit console"
  nunit :test => :build do |nunit|
    nunit.command = Dir.glob(File.join($dir,"packages/NUnit.Runners.*/Tools/nunit-console.exe")).first
    nunit.assemblies File.join($dir,"Tests/bin/Debug/Tests.dll"), File.join($dir,"Isop.Wpf.Tests/bin/Debug/Isop.Wpf.Tests.dll")
  end
  desc "copy example cli to wpf and cli folder"
  task :copy_cli => :build do
    cp File.join($dir,"Example.Cli/bin/Debug/Example.Cli.dll"), File.join($dir,"Isop.Wpf/bin/Debug")
    cp File.join($dir,"Example.Cli/bin/Debug/Example.Cli.dll"), File.join($dir,"Isop.Auto.Cli/bin/Debug")
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
      sh "..\\..\\src\\.nuget\\NuGet.exe pack Isop.nuspec"
    end
  end

  task :runners_nugetpack => [:runners_copy_to_nuspec] do |nuget|
    cd File.join($nugetfolder,"Isop.Runners") do
      sh "..\\..\\src\\.nuget\\NuGet.exe pack Isop.Runners.nuspec"
    end
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

desc "regenerate links in isop wpf"
task :regen_wpf do
  isop = VisualStudioFiles::CsProj.new(File.open(File.join($dir,'Isop','Isop.csproj'), "r").read)
  isop_files = isop.files.select do |file|
    file.type=='Compile' && !file.file.end_with?('AssemblyInfo.cs')
  end
  wpfcontrols = VisualStudioFiles::CsProj.new(File.open(File.join($dir,'Isop.WpfControls','Isop.WpfControls.csproj'), "r").read)
  wpfcontrol_files = wpfcontrols.files.select do |file|
    !file.file.end_with?('AssemblyInfo.cs')
  end
  
  wpf = VisualStudioFiles::CsProj.new(File.open(File.join($dir,'Isop.Wpf','Isop.Wpf.csproj'), "r").read)
  wpf.clear_links
  isop_files.each do |file|
    hash = file.to_hash
    hash[:file] = "..\\Isop\\#{file.file}"
    hash[:link] = "Isop\\#{file.file}"
    wpf.add(hash)
  end
  wpfcontrol_files.each do |file|
    hash = file.to_hash
    hash[:file] = "..\\Isop.WpfControls\\#{file.file}"
    hash[:link] = "WpfControls\\#{file.file}"
    wpf.add(hash)
  end

  File.open(File.join($dir,'Isop.Wpf','Isop.Wpf.csproj'), "w") do |f|
    wpf.write f
  end
end


namespace :mono do
  desc "build isop on mono"
  xbuild :build do |msb|
    solution_dir = File.join(File.dirname(__FILE__),'src')
    nuget_tools_path = File.join(solution_dir, '.nuget')
    msb.properties :configuration => :Debug, 
      :SolutionDir => solution_dir,
      :NuGetToolsPath => nuget_tools_path,
      :NuGetExePath => File.join(nuget_tools_path, 'NuGet.exe'),
      :PackagesDir => File.join(solution_dir, 'packages')
    msb.targets :rebuild
    msb.verbosity = 'quiet'
    msb.solution = File.join('.','src',"Isop.sln")
  end

  desc "test with nunit"
  task :test => :build do
    # does not work for some reason 
    command = "nunit-console4"
    assemblies = "Tests.dll"
    cd "src/Tests/bin/Debug" do
      sh "#{command} #{assemblies}"
    end
  end

  desc "copy example cli to cli folder"
  task :copy_cli => :build do
    cp "Example.Cli/bin/Debug/Example.Cli.dll", "Isop.Auto.Cli/bin/Debug"
  end

  desc "Install missing NuGet packages."
  task :install_packages do |cmd|
    FileList["src/**/packages.config"].each do |filepath|
      sh "mono --runtime=v4.0.30319 ./src/.nuget/NuGet.exe i #{filepath} -o ./src/packages"
    end
  end

end
