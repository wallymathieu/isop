 #!/usr/bin/env bash

 source /usr/local/rvm/scripts/rvm

 rvm use --install $1
 rvm --default use $1
 shift

 if (( $# ))
 then gem install $@
 fi