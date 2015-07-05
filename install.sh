#! /bin/bash

if [ "$(id -u)" != "0" ]; then
	echo "WARNING: Not running as root, default installation directory WILL FAIL!"
fi

prefix="/usr/lib/iodine"

if [ "$#" -ge 1 ]; then
	prefix=$1
fi

echo "Using prefix $prefix"

if [ -d "$prefix" ]; then
	cp ./bin/iodine.exe $prefix/iodine.exe
	cp -r ./bin/modules $prefix/modules
	cp -r ./bin/extensions $prefix/extensions
	cat ./iodine_run.sh > /usr/bin/iodine
	chmod a+x /usr/bin/iodine
	cat "export IODINE_PATH=$prefix" > /etc/profile.d/iodine.sh
fi
