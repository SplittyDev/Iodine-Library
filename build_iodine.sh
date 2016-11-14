#!/usr/bin/env bash
cd src/Iodine
nuget restore
xbuild /p:Configuration=Minimal
