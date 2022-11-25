# Playstation Macro

This project is a fork of the awesome [PS4Macro](https://github.com/komefai/PS4Macro), forked to work for PS5 too, along with all the new control sensors and buttons that PS5 controller has.

Automation utility for PS4 and PS5 Remote Play written in C#.

🔔 **Download latest version [here](https://github.com/komefai/PlaystationMacro/releases)!**

## How to use

To record, click on `RECORD` button (Ctrl+R) to arm recording then press `PLAY` to start recording controls. The red text on the bottom right indicates the number of frames recorded. You can stop recording by clicking on `RECORD` button (Ctrl+R) again. The macro will then play the controls in a loop.

## Scripting

C# scripting support has been introduced in version 0.3.0 and later. This allows us to create custom behaviors beyond repeating macros with an easy-to-use API. The API also includes wrapped convenience functions such as pressing buttons, timing, and taking a screenshot from PS4 Remote Play. 

See the [scripting video tutorial](https://youtu.be/daCb97rbimA) to get started or see [the wiki](https://github.com/komefai/PlaystationMacro/wiki) for full documentation, examples, and other information.

NOTE: The script have to include a reference to `PlaystationMacroAPI.dll` to interface with PlaystationMacro. At the moment the scripts has to be compiled into a DLL file to be able to open with PS4 Macro.

##### Basic Example Script

This example script will press DPad up and wait one second, follow by pressing square. The loop repeats every 800ms.

```csharp
using PlaystationMacroAPI;

public class Script : ScriptBase
{
    /* Constructor */
    public Script()
    {
        Config.Name = "Example Script";
        Config.LoopDelay = 800;
    }

    // Called when the user pressed play
    public override void Start()
    {
        base.Start();
    }

    // Called every interval set by LoopDelay
    public override void Update()
    {
        Press(new DualShockState() { DPad_Up = true });
        Sleep(1000);
        Press(new DualShockState() { Square = true });
    }
}
```

#### List of Scripts

- [Keyboard Remapping Utility](https://github.com/komefai/PlaystationMacro.Remote)
- [Marvel Heroes Omega Bot](https://github.com/komefai/PlaystationMacro.MarvelHeroesOmega)
- [PES2018 Bot (Simulator Mode)](https://github.com/leguims/PlaystationMacro.PES2018Lite) by [leguims](https://github.com/leguims)

---

## Credits

- [EasyHook](https://easyhook.github.io/)
- [Mono.Options](https://www.nuget.org/packages/Mono.Options/)
- [PS4Macro](https://github.com/komefai/PS4Macro)
