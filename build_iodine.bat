@echo off
set msbuild="C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe"
cd src\Iodine
nuget restore
%msbuild% /p:Configuration=Minimal /p:outdir="..\..\bin\" Iodine.sln
pause >nul