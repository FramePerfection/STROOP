# STROOP (Refactored)
*SuperMario64 Technical Runtime Observer and Object Processor*

_This is a fork from [SM64-TAS-ABC/STROOP](https://github.com/SM64-TAS-ABC/STROOP) designed to be faster, less memory intensive and more extensible. The feature set and code structure may differ severely from the baseline._

  STROOP is a diagnostic tool for Super Mario 64 that displays and allows for simple editing of various game values and information. It can connect to a running emulator and update values in real time. Some core features include views of loaded/unloaded objects, Mario structure variables, camera + HUD values, an overhead map display, and many more.

## Why should I use this over the main version?

This reworked version aims to reduce the memory load and improve the real-time performance of STROOP by reevaluating the underlying structure of what STROOP does and what I believe it is meant to be. By generalizing some of its core features and entirely refactoring the map tab, this version is able to handle complex TASing workloads even on relatively weak machines.<br>
You can even create sick ghost comparisons like [this one](https://youtu.be/5mdgjsFqN2I?feature=shared&t=15) with relative ease.

## Downloading STROOP

You can download the latest binaries of this Refactor from the [releases section](https://github.com/FramePerfection/STROOP/releases).
If you wish to stay connected with the most recent developments, I recommend that you build STROOP yourself as described under the [Building](#Building) section.

If this updated feature set doesn't suit your needs, you can of course still get the releases from the [original main branch](https://github.com/SM64-TAS-ABC/STROOP/releases/tag/vDev).

## Requirements

  As of the current build, STROOP has the following system requirements:
  * Windows 11 / Windows 10 / Windows 8.1 / Windows 8 / Windows 7 64-bit or 32-bit
  * OpenGL 3.2 or greater
  * [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
  * [Mupen](https://mupen64.com/) is recommended for TASing. (Nemu64 and some other emulators may work, but that's a bit of a shot in the dark)
  * 64 Marios (Must be super)
  * Marios must be American, Japanese or PAL

## Building

Requirements:
  * A [dotnet 8.0 (or higher) SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) in any capacity
  * An internet connection for the [NuGet](https://nuget.org) package dependencies, such as OpenTK.

A simple `dotnet build` in the repository root directory will create a debug build.<br>
A `Release` configuration is also present for non-debugging purposes and is used for the binary releases in this repository.

## Status

As of 2025/22/12 and version 0.7.0, [this branching model](https://nvie.com/posts/a-successful-git-branching-model/) shall be employed to facilitate orderly releases.

This refactor is ongoing and has diverged severely from the main repository in order to:
- rework the structure of the project while minimizing dependencies and "noise" in the code for better performance, and, eventually, crossplatform support
- implement new complex features, such as a fully redesigned (3D) map tab and the "ghost hack" used to display and compare runs within the game, as well as a work-in-progress brute-forcing Tab.

Features from the main branch are partially missing or work in different ways.

I hope to eventually get STROOP back onto a single track again, whether that be the main branch or this fork remains to be seen however. If you are capable and willing to take a heavy load, I'd invite you to discuss details with me on how that should happen on this barebones [Discord](https://discord.gg/QdcwCgXn) server (will adjust as the needs arise).

## Contributing

I'd love to develop this version of STROOP into something that everyone involved with SM64 TASing and beyond can get great value from.<br>
If you are a user of this STROOP version in any capacity, all of your suggestions for improvements will be appreciated (and ideally eventually implemented).<br>
You may submit your feedback either via an [issue](../../issues) or just speak your mind in this barebones [Discord](https://discord.gg/YHgau6tg2d) server.

If you choose to contribute code via a [pull request](../../pulls), please make an effort to keep your changes free of noise, especially regarding code formatting.<br>
While I do not enforce any specific style and do not intend to do so, the result of `dotnet format` should be taken as a baseline.
**Please don't let your IDE automatically break long lines in edited files!**<br>
These line breaks usually don't facilitate readability at all, and the never-ending war about the "optimal maximum line length" makes it so the noise from added or removed line breaks remains constant in every pull request when different developers with different settings work on the same files.<br>
Particularly, JetBrains Rider has a setting to `Wrap long lines` in the `Editor->Code Style->C#` section, **which I have turned off to prevent it from introducing meaningless line breaks.**<br>
If you feel the need to break long lines for better readability, please do so manually with intent.
