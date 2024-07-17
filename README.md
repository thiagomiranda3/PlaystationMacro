# IMPORTANT

The new PS Remote Play has an anti hook protection that makes this program useless. The only workaround so far to continue using it is by downloading an older version of PSRemotePlay:

https://ps5-remote-play.uptodown.com/windows/descargar/81670958

Download it and when it ask for update to new version, click No. You will have to link it to your PS4/PS5 everytime, but at least the macro will work on this version.

As soon as I can find a way to bypass this anti hook protection, I will update this program, but so far, I can't do much about it unfortunately.

# Playstation Macro

This project is a fork of the awesome [PS4Macro](https://github.com/komefai/PS4Macro), forked to work for PS5 too, along with all the new control sensors and buttons that PS5 controller has.

Automation utility for PS4 and PS5 Remote Play written in C#.

🔔 **Download latest version [here](https://github.com/thiagomiranda3/PlaystationMacro/releases/download/0.5.2/PlaystationMacro.zip)!**

## How to use

To record, click on `RECORD` button (Ctrl+R) to arm recording then press `PLAY` to start recording controls. The red text on the bottom right indicates the number of frames recorded. You can stop recording by clicking on `RECORD` button (Ctrl+R) again. The macro will then play the controls in a loop.

## Scripting

C# scripting support has been introduced in version 0.3.0 and later. This allows us to create custom behaviors beyond repeating macros with an easy-to-use API. The API also includes wrapped convenience functions such as pressing buttons, timing, and taking a screenshot from PS4 Remote Play. 

See the [scripting video tutorial](https://youtu.be/daCb97rbimA) to get started or see [the wiki](https://github.com/komefai/PlaystationMacro/wiki) for full documentation, examples, and other information.

NOTE: The script have to include a reference to `PlaystationMacroAPI.dll` to interface with PlaystationMacro. At the moment the scripts has to be compiled into a DLL file to be able to open with PS4 Macro.

## Credits

- [EasyHook](https://easyhook.github.io/)
- [Mono.Options](https://www.nuget.org/packages/Mono.Options/)
- [PS4Macro](https://github.com/komefai/PS4Macro)
