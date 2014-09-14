require 'visual_studio_files.rb'
require 'albacore'

require 'rbconfig'
#http://stackoverflow.com/questions/11784109/detecting-operating-systems-in-ruby
def os
  @os ||= (
    host_os = RbConfig::CONFIG['host_os']
    case host_os
    when /mswin|msys|mingw|cygwin|bccwin|wince|emc/
      :windows
    when /darwin|mac os/
      :macosx
    when /linux/
      :linux
    when /solaris|bsd/
      :unix
    else
      raise Error::WebDriverError, "unknown os: #{host_os.inspect}"
    end
  )
end

def nuget_exec(parameters)

  command = File.join(File.dirname(__FILE__), "src",".nuget","NuGet.exe")
  if os == :windows
    sh "#{command} #{parameters}"
  else
    sh "mono --runtime=v4.0.30319 #{command} #{parameters} "
  end
end

def nunit_cmd()
  cmds = Dir.glob(File.join(File.dirname(__FILE__),"src","packages","NUnit.Runners.*","tools","nunit-console.exe"))
  if cmds.any?
    if os != :windows
      command = "mono --runtime=v4.0.30319 #{cmds.first}"
    else
      command = cmds.first
    end
  else
    raise "Could not find nunit runner!"
  end
  return command
  
end

def nunit_exec(dir, tlib)
    command = nunit_cmd()
    assemblies= "#{tlib}.dll"
    cd dir do
      sh "#{command} #{assemblies}" do  |ok, res|
        if !ok
          abort 'Nunit failed!'
        end
      end
    end
end

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
      nuget_exec("i #{filepath} -o ./src/packages -source http://www.nuget.org/api/v2/")
  end
end

desc "build"
build :build do |msb|
  msb.prop :configuration, :Debug
  msb.prop :platform, "Mixed Platforms"
  if os != :windows
    with_mono_properties msb
  end
  msb.target = :Rebuild
  msb.be_quiet
  msb.nologo
  if os == :windows
    msb.sln =File.join($dir, "Isop.Wpf.sln")
  else
    msb.sln =File.join($dir, "Isop.sln")
  end
end

task :default => ['build']

desc "test using nunit console"
test_runner :test => [:build, :install_packages] do |nunit|
  command = Dir.glob(File.join($dir,"packages/NUnit.Runners.*/Tools/nunit-console.exe")).first
  puts command
  nunit.exe = command
  files = [File.join($dir,"Tests/bin/Debug/Tests.dll")]
  if os == :windows
    files.push File.join($dir,"Isop.Wpf.Tests/bin/Debug/Isop.Wpf.Tests.dll")
  end
  nunit.files = files 
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
    nuget_exec "pack Isop.nuspec"
  end
end

task :runners_nugetpack => [:runners_copy_to_nuspec] do |nuget|
  cd File.join($nugetfolder,"Isop.Runners") do
    nuget_exec "pack Isop.Runners.nuspec"
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
