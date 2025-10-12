# STROOP (Refactored)
*SuperMario64 Technical Runtime Observer and Object Processor*  

_This is a fork from [SM64-TAS-ABC/STROOP](https://github.com/SM64-TAS-ABC/STROOP) designed to be faster, less memory intensive and more extensible. The feature set and code structure may differ severely from the baseline._

  STROOP is a diagnostic tool for Super Mario 64 that displays and allows for simple editing of various game values and information. It can connect to a running emulator and update values in real time. Some core features include views of loaded/unloaded objects, Mario structure variables, camera + HUD values, an overhead map display, and many more.

## Why should I use this over the main version?

This reworked version aims to reduce the memory load and improve the real-time performance of STROOP by reevaluating the underlying structure of what STROOP does and what I believe it is meant to be. By generalizing some of its core features and entirely refactoring the map tab, this version is able to handle complex TASing workloads even on relatively weak machines.  
You can even create sick ghost comparisons like [this one](https://youtu.be/5mdgjsFqN2I?feature=shared&t=15) with relative ease.

## Downloading STROOP

If you wish to stay connected with the most recent developments, I recommend that you build STROOP yourself as described under the [Building](#Building) section.  
You can download (somewhat) recent binaries of the [Refactor here](https://github.com/chaosBrick/STROOP/releases).  
If this updated feature set doesn't suit your needs, you can of course still get the releases from the [original main branch](https://github.com/SM64-TAS-ABC/STROOP/releases/tag/vDev).  

## Requirements

  As of the current build, STROOP has the following system requirements:
  * Windows 10 / Windows 8.1 / Windows 8 / Windows 7 64-bit or 32-bit
  * OpenGL 3.2 or greater
  * .NET Framework 4.8 (See [.NET Framework System Requirements](https://msdn.microsoft.com/en-us/library/8z6watww(v=vs.110).aspx) for more information)
  * [Mupen](https://repack.skazzy3.com/) is recommended for TASing. (Nemu64 and some other emulators may work, but that's a bit of a shot in the dark)
  * 64 Marios (Must be super)
  * Marios must be American, Japanese or PAL
 
## Building

Requirements:
  * Visual Studio *(2017 or above recommended)*
  * OpenTK 3.0.1 *(or higher, will likely work)*
  
OpenTK is a prerequisite for building STROOP. This is easiest installed by using the NuGet package manager. STROOP can be easily built from the source code by opening up the solution file in Visual Studio and performing a build.  

Yes, I fully expect you to know your way around git and Github for now. Clone the repository and double click the file and build and whatnot, it's not that difficult, but I'm aware that this is really incomplete right now. Also, this is just copy-pasted from the main repository, so don't blame me for being unspecific!

## Status

The repository is currently a hot mess and I shall update it to reflect a more structured approach to develop this version of STROOP in the coming weeks (as of 2024/08/18).  
This refactor is ongoing and has diverged severely from the main branch in order to rework the structure of the project and implement new complex features, such as a fully redesigned (3D) map tab and the "ghost hack" used to display and compare runs within the game, as well as a work-in-progress brute-forcing Tab.  
Features from the main branch are partially missing or work in different ways.

I hope to eventually get STROOP back onto a single track again, whether that be the main branch or this fork remains to be seen however. If you are capable and willing to take a heavy load, I'd invite you to discuss details with me on how that should happen on this barebones [Discord](https://discord.gg/QdcwCgXn) server (will adjust as the needs arise).

## Contributing

I'd love to develop this version of STROOP into something that everyone involved with SM64 TASing and beyond can get great value from.  
If you are a user of this STROOP version in any capacity, all of your suggestions for improvements will be appreciated (and ideally eventually implemented).
You may submit your feedback either via an [issue](../../issues) or just speak your mind in this barebones [Discord](https://discord.gg/YHgau6tg2d) server.

If you choose to contribute code via a [pull request](../../pulls), please make an effort to keep your changes free of noise, especially regarding code formatting.  
While I do not enforce any specific style and do not intend to do so, the result of `dotnet format` should be taken as a baseline.  
**Please don't let your IDE automatically break long lines in edited files!**  
These line breaks usually don't facilitate readability at all, and the never-ending war about the "optimal maximum line length" makes it so the noise from added or removed line breaks remains constant in every pull request when different developers with different settings work on the same files.  
Particularly, JetBrains Rider has a setting to `Wrap long lines` in the `Editor->Code Style->C#` section, **which I have turned off to prevent it from introducing meaningless line breaks.**  
If you feel the need to break long lines for better readability, please do so manually with intent.
