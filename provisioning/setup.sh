#!/usr/bin/env bash

source /usr/local/rvm/scripts/rvm

cd /vagrant/

bundle install

echo cd \/vagrant > /home/vagrant/.bashrc