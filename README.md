# Full Queue

Queue up several orders in the countdown at the start of a game and on expeditions, the same way you can at home.

At home, Survival Log lets you line up orders: walk there, pick that up, cook this. In the countdown before the disaster and on expeditions it only keeps one, and every new order throws away the ones still waiting. Full Queue lets your orders queue up there too, so a new order joins the end of the queue like it does at home.

## Features

- **A full queue at the start of a game**: in the countdown before the disaster, new orders join the queue instead of replacing it.
- **A full queue on expeditions**: on an expedition, new orders join the queue instead of replacing it.
- **Nothing else about orders changes**: a new order waits its turn at the end of the queue, and that is the only difference.
- **See every waiting order**: the queue bar at the bottom of the screen shows all your waiting orders instead of stopping at 5. With more than 5 it keeps its size and scrolls with the mouse wheel, and a small number at each edge says how many orders are out of sight on that side. Click any order to cancel it, the same as the first 5. With 5 or fewer orders the bar looks the same as without the mod. This works everywhere the queue bar shows up, home included.
- **A queue bar in the countdown**: the game has no queue bar in the countdown before the disaster, so the mod adds one in the bottom left corner. The buttons there move up to make room for it. Like the normal queue bar, it starts with a big box showing what your character is doing right now, with a ring showing how far along it is. When your character is storing items, the box also shows how many are put away. Click an order or the big box to cancel it. The countdown queue bar goes away when the disaster starts and the normal one takes over.
- **Each place has its own switch**: you can turn the queue off for the countdown or for expeditions if you'd rather give one order at a time there.
- **A log file**: the mod writes what it did to `BepInEx\full-queue.log`.

There are no new keys or buttons. Scroll the queue bar with the mouse wheel while the pointer is over it.

Works in English and Chinese. The mod doesn't add any words to the game's screens. The queue bar shows the game's order names in the language you play in, and the out-of-sight counts are plain numbers such as `+3`. The settings file explains each setting in English and Simplified Chinese.

Full Queue adds no data to your save and does not edit game files. You can remove it at any time.

## Known issues and limits

- When the countdown ends and the disaster hits, the game drops any orders still waiting. That happens without the mod too.
- When an expedition ends, any orders still waiting are dropped. They don't come home with you.
- The mod doesn't change how orders queue at home, because the game already lets you queue orders there.
- The mod doesn't change how orders queue in the tutorial.
- If you turn `ScrollQueue` off, the queue bar only shows the first 5 waiting orders and the countdown has no queue bar, the same as without the mod. The orders past the fifth are still in the queue and run in turn.

## Requirements

- Survival Log 1.1.20719. The mod is built for this version, and a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game, version `6.0.0-be.788`.
- [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.20.0 or newer. Full Queue doesn't load without it. It applies settings you change on the Mod Manager's Mods screen without restarting the game.
- Optional: [Mod Manager](https://www.nexusmods.com/survivallog/mods/73). With it, you can change every setting in game on its Mods screen and see what each one does, without opening the config file.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12). If you haven't installed any other mods before, start the game once so BepInEx finishes setting up, then quit.
2. Install Exiled's Toolkit. Download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/61), and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Download `FullQueue-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-FullQueue/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/32).
4. Extract the zip into the game folder (the folder with `SurvivalLog.exe`). It holds a `BepInEx` folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
   `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\FullQueue.dll`

## Uninstall

Delete `FullQueue.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.fullqueue.cfg` and `BepInEx\full-queue.log`.

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.fullqueue.cfg`, which is created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. If you change them on the Mods screen, QueueInPrologue applies the next time you load a game and QueueOnExpeditions applies from your next expedition. Enabled and ScrollQueue take effect after you restart the game. See [CONFIG.md](CONFIG.md) for every setting.

## Troubleshooting

Built against BepInEx `6.0.0-be.788`.

Check `BepInEx\LogOutput.log` for a line starting `===== Full Queue`. It says whether the mod is on, whether the queue is on for the start of a game and for expeditions, and whether the queue bar shows every order. The next lines should say the hooks went in. The one for the queue bar starts `hooked Reducer_Web_CoreUI1.RA_UpdateActionList`. Once you're in a game, the line `page script sent to the web UI` means the scrolling queue bar is ready.

Each time you start a game, load a save or start an expedition, the log gets one more line saying where you are and what the mod did, for example `expedition: new orders now join the queue instead of replacing it`.

The same lines are in `BepInEx\full-queue.log`, which survives restarts.

If the queue bar still shows only 5 orders, look for a line starting `could not` or `failed` that mentions the queue bar. If that part of the mod can't start, the queue bar stays the same as without the mod.

If you can still only give one order at a time, look in the log for a line starting `could not` or `failed`. If the mod can't make its change, you get one order at a time like without the mod.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log` and `BepInEx\full-queue.log`.

To reach me directly, message `exiled.dev` on Discord. You can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx and the game's IL2CPP interop assemblies, so you need a game install where BepInEx is set up and the game has been started once. Those assemblies come from the game and aren't part of this repo. You also need the .NET 8 SDK.

```
dotnet build src/FullQueue -c Release -p:GameDir="C:\path\to\Survival Log"
```

`GameDir` is the game folder. The default in the project file is a local path you will almost certainly need to override. Compiling also needs Exiled's Toolkit 0.20.0 or newer: point `-p:ToolkitDll="C:\path\to\ExiledToolkit.dll"` at its DLL. The output DLL is at `src\FullQueue\bin\Release\FullQueue.dll`.

The unit tests check where the character is, which places get the queue, and how the orders past the fifth are written for the queue bar. They also run the queue bar's page script against a stand-in for the game's page. None of them need the game:

```
dotnet test tests/FullQueue.Tests
```

## Changelog

Every version's changes are in [CHANGELOG.md](CHANGELOG.md).

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
