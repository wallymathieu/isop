# -*- mode: ruby -*-
# vi: set ft=ruby :

# Vagrantfile API/syntax version. Don't touch unless you know what you're doing!
VAGRANTFILE_API_VERSION = "2"

Vagrant.configure(VAGRANTFILE_API_VERSION) do |config|
  config.vm.box = "ubuntu/trusty64"
  config.vm.box_url = "https://vagrantcloud.com/ubuntu/boxes/trusty64"
  config.vm.provision :shell, :path => "provisioning/install-rvm.sh",  :args => "stable"
  config.vm.provision :shell, :path => "provisioning/install-ruby.sh", :args => "2.1.2"
  config.vm.provision :shell, :path => "provisioning/install-mono.sh"
  config.vm.provision :shell, :path => "provisioning/setup.sh"

end
