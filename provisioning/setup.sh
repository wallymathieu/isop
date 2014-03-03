#!/usr/bin/env bash

source /usr/local/rvm/scripts/rvm

cd /vagrant/
rvm use 2.1.0
bundle install

echo cd \/vagrant > /home/vagrant/.bashrc