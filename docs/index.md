# What is STROOP

STROOP is an advanced memory viewer and manipulation program for Super Mario 64 that aids in analyising complex scenarios, TASing, hacking and recording footage of the game. It was originally created by Denebou and Pannenkoek in 2016 in an effort to advance the A-Button Challenge, and has since developed into a general-purpose tool for all sorts of things that are related to playing, hacking and TASing Super Mario 64.  
The original repository for STROOP can be found [here](https://github.com/SM64-TAS-ABC/STROOP).  
SuperM's documentation writeup for that version can be found [here](TODO: link once it's available lol).

I (FramePerfection) have since created a fork from this development in 2021 to create a less resource intensive, more flexible and feature rich version of STROOP. This is the version that this documentation will be based on, however I will attempt to point out differences to the original version where it's appropriate.  
The repository for this version can be found [here](https://github.com/frameperfection/STROOP).

The name "STROOP" is derived as a backronym from **S**M64 **T**echnical **R**untime **O**bserver and **O**bject **P**rocessor.


## Getting started

STROOP is typically used and works best in conjunction with [mupen64](https://github.com/mupen64/mupen64-rr-lua).  
If you don't have mupen set up, the recommend way is to download [Skazzy's Repack](http://repack.skazzy3.com), which includes a recent stable build of mupen along with several plugins, and thus can run out of the box.  
You will, of course, also need a ROM of Super Mario 64 (or a ROM hack thereof, if that's your interest), preferrably either the NTSC or US version, as those are best supported by STROOP.

To install STROOP, head to the releases page on GitHub ([here](https://github.com/FramePerfection/STROOP/releases/) for my refactor, and [here](https://github.com/SM64-TAS-ABC/STROOP/releases) for the original repository), then download the `STROOP.zip` file (or similarly named file) from the `Assets` section of the latest release.  
Extract the downloaded archive in a location of your liking, and then simply run STROOP.exe.

### Connecting STROOP to mupen

To do its work, STROOP must usually be able to read the memory of a running Super Mario 64 instance (there is a [feature to work based on a savestate instead](TODO: link)). After starting up, STROOP will list all processes that it has detected to be running Super Mario 64, and let you connect STROOP to one of them with the click of a button.

If this list is empty for you, even after hitting `Refresh` after you have launched Super Mario 64, it means STROOP is unable to detect the running Super Mario 64 instance. This can happen when the auto-detection of the RAM start for the emulated N64's memory fails. (Note that this feature is unavailable or works differently in the original STROOP.)  
To fix this issue, select `Utilities->Show RAM start...` and click `Copy STROOP config line` , then open the `./Config/Config.xml` file and paste that line into the `<Emulators>` section.  
After restarting STROOP, it should then be able to detect your emulator.  

There are some more things to note about this:

- If you don't have the "Show RAM start" option, install a recent version of mupen from [Skazzy's Repack](http://repack.skazzy3.com) - seriously!
- The `autoDetect` attribute included in the STROOP config line actually does nothing - the correct name is `autoDetectRAMStart`, and it is `true` by default, rendering the attribute doubly useless. I deeply regret this design decision, as it effectively leads to the program doing guess work, with no clear indication of how to turn it off, and I hope to right this mistake in a future version.
- In some situations, the auto-detection algorithm may come across a region of memory that appears to be the emulated N64's memory, but actually isn't, such as a region of a savestate or the like. When this happens, STROOP will not appear to update its information as expected. When this happens, adding the STROOP config line as described above and setting `autoDetectRAMStart` to `false` will solve the problem.

## Overview and Structure

So you've installed STROOP, connected it to mupen, and are finally greeted with STROOP's _real_ user interface and your first instinct might be to just hit the `Disconnect` button in the top left from the informational overload, but that's probably not what you had in mind.  
So, what now? Well, that depends heavily on what you want to do...

STROOP's main interface consists of two key elements: The [object list](TODO: link) at the bottom, and the [tabs](TODO: link) at the top.  
The tabs are designed to present information and controls for certain aspects of the game or workflows you may want to follow. Over time, you'll want to make yourself familiar with the different individual tabs that suit your needs and deeply understand their purpose, interactions between one another and the object list.

By default, the object list and the "left panel" of each tab are always visible, but you can change this appearance with the layout buttons in the top right corner near the gear icon. However, because objects are relevant in a great number of contexts, and because many STROOP tabs aren't designed to work well with any but the default layout, messing around with this can hardly ever be recommended.

As a general note, it is worth mentioning that right clicking just about anything in STROOP will reveal a lot of hidden, yet at times very useful context menus - even for typically non-function elements like labels or elements that serve a different function by nature such as buttons or drop-down boxes.