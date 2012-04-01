require 'fileutils'
# the code is mostly from fluentmigrator
# https://github.com/schambers/fluentmigrator
#
ISOP_VERSION = "1.2.0.0"
def to_nuget_version(v)
	v[1] + v[3]
end

def copy_files(from, to, filename, extensions)
  extensions.each do |ext|
    FileUtils.cp "#{from}#{filename}.#{ext}", "#{to}#{filename}.#{ext}"
  end
end

def prepare_lib
  output_directory_lib = "./nuget/Isop/lib/40"
  FileUtils.mkdir_p output_directory_lib
  ['Isop','Isop.Auto.Cli','Isop.Wpf'].each{ |project|
    cp Dir.glob("./#{project}/bin/Debug/*.*"), output_directory_lib
  } 
end

namespace :nuget do
  task :clean do
    FileUtils.rm_rf './nuget/Isop/lib/'
  end
  
  desc "create the Isop nuspec file"
  nuspec :create_spec do |nuspec|

     nuspec.id = "Isop"
     nuspec.version = ISOP_VERSION
     nuspec.authors = "Oskar Gewalli"
     nuspec.owners = "Oskar Gewalli"
     nuspec.description = "Isop is a library to help simplify and structure command line apps. The Isop dll is used if you want to have your own console application. The Isop.Cli.exe and Isop.Wpf.exe if you dont care about implementing a 'Main' method."
     nuspec.title = "Isop"
     nuspec.language = "en-US"
     nuspec.projectUrl = "https://github.com/wallymathieu/isop"
     nuspec.working_directory = "nuget/Isop"
     nuspec.output_file = "Isop.nuspec"
  end
  
  task :prepare_package => ['ms:build', :create_spec, :clean] do
     prepare_lib 
  end

  task :package => :prepare_package do
    nuget_pack('nuget/Isop/', 'nuget/Isop/Isop.nuspec')
  end
  
  def nuget_pack(base_folder, nuspec_path)
    cmd = Exec.new  
    output = 'nuget_package/'
    cmd.command = '.nuget/NuGet.exe'
    cmd.parameters = "pack #{nuspec_path} -basepath #{base_folder} -outputdirectory #{output}"
    cmd.execute
  end

end
