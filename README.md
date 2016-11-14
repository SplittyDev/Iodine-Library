# Iodine Library for [iosh][iosh]
Upstream repository: [Iodine][iodine]

Iodine is dynamically typed multi-paradigm programming language written in C#.   
The syntax of the Iodine is derived from several languages including Python, C#, and F#.

This repository is a stripped-down version of the original Iodine repository,   
modified to be easily embeddable and to work better with [iosh][iosh].

An up-to-date version of this library always ships with the latest version of iosh.

## Building
There should be no reason for you to build this version of Iodine yourself,   
since it has been designed specifically for the purpose of using it within iosh.   

If you're desperately wanting to build this library yourself,   
you can do so using msbuild (Windows) or xbuild (most unixoids).

On WSL and most unixoids the library can also be built by running ```make```.

## License
This repository in its entirety is licensed under the BSD 3-Clause License.

[iodine]: https://github.com/IodineLang/Iodine
[iosh]: https://github.com/SplittyDev/iosh