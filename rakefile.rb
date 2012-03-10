require 'albacore'

task :default => [:build]

msbuild :build do |msb|
  msb.properties :configuration => :Debug
  msb.targets :Clean, :Rebuild
  msb.verbosity = 'quiet'
  msb.solution = "Isop.sln"
end

nunit :test => :build do |nunit|
  nunit.command = "packages/NUnit.2.5.9.10348/Tools/nunit-console.exe"
  nunit.assemblies "Tests/bin/Debug/Tests.dll"
end

