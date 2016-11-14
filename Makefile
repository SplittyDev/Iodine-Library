.PHONY: all clean library

all: library

library:
	cd ./src/Iodine && nuget restore
	xbuild ./src/Iodine/Iodine.sln /p:Configuration=Release /p:DefineConstants="MINIMAL" /t:Build "/p:Mono=true;BaseConfiguration=Release"