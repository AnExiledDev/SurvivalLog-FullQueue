# Full Queue

Queue up several orders in the countdown at the start of a game and on expeditions, the same way you can at home.

At home, Survival Log lets you line up orders: walk there, pick that up, cook this. In the countdown before the disaster and on expeditions it only keeps one. Every new order throws away the ones still waiting. Full Queue lets your orders queue up there too, so a new order joins the end of the queue like it does at home.

## Features

- **A full queue at the start of a game**: in the countdown before the disaster, new orders join the queue instead of replacing it.
- **A full queue on expeditions**: the same on an expedition.
- **Only one thing changes**: a new order joins the end of the queue instead of replacing it. Nothing else about orders is changed.
- **See every waiting order**: the queue bar at the bottom of the screen shows all your waiting orders, not just the first 5. With more than 5 it keeps its size and scrolls with the mouse wheel, and a small number at its edge says how many orders are out of sight on that side. Click any of them to cancel it, as with the first 5. With 5 or fewer the bar looks just as it always did. It works wherever the game shows the bar.
- **A queue bar in the countdown**: the game shows no queue bar in the countdown before the disaster. The mod adds one in the bottom left corner, under the buttons there, which move up to make room. Like the bar after the disaster, it starts with a big box for what your character is doing now. Click an order, or the big box, to cancel it. It goes away when the disaster starts and the game's own bar takes over.
- **Each place has its own switch**: turn the queue off for the start of a game or for expeditions if you prefer the game's way there.
- **A log file**: what the mod did is written to `BepInEx\full-queue.log`.

There are no new keys or buttons. Scroll the queue bar with the mouse wheel while the pointer is over it.

Works in English and Chinese. The mod adds no words of its own to the game's screens: the queue bar shows the game's own order names in the language you play in, and the out-of-sight counts are plain numbers such as `+3`. The settings file explains each setting in English and Simplified Chinese.

Full Queue adds no data to your save and does not edit game files. You can remove it at any time.

## Known issues and limits

- When the countdown ends and the disaster hits, the game drops any orders still waiting. It did that before too.
- When an expedition ends, any orders still waiting are dropped. They do not come home with you.
- The game lets orders queue at home on its own, so the mod leaves home alone.
- The tutorial is left as the game has it.
- If you turn `ScrollQueue` off, the queue bar shows the first 5 waiting orders as in the game. The rest still wait their turn.

## Requirements

- Survival Log 1.1.20151. Built for this version; a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game, version `6.0.0-be.788`.
- Optional: [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.4.0 or newer. With it, settings changed on the Mod Manager's Mods screen take effect without restarting the game. Without it, they take effect at the next game start.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Optional, to change settings without a restart: install Exiled's Toolkit. Download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/61), and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Download `FullQueue-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-FullQueue/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/32).
4. Extract the zip into the game folder (the folder with `SurvivalLog.exe`). It holds a `BepInEx` folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
   `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\FullQueue.dll`

## Uninstall

Delete `FullQueue.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.fullqueue.cfg` and `BepInEx\full-queue.log`.

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.fullqueue.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. Changed on the Mods screen, QueueInPrologue takes effect the next time a game loads and QueueOnExpeditions from the next expedition; Enabled and ScrollQueue take effect at the next game start. See [CONFIG.md](CONFIG.md) for every setting. Without Exiled's Toolkit installed, changes made on the Mods screen take effect at the next game start instead.

## Troubleshooting

Built against BepInEx `6.0.0-be.788`.

Check `BepInEx\LogOutput.log` for a line starting `===== Full Queue`. It says whether the mod is on, whether the queue is on for the start of a game and for expeditions, and whether the queue bar shows every order. The next lines should say the hooks went in; the queue bar's starts `hooked Reducer_Web_CoreUI1.RA_UpdateActionList`. Once you are in a game, a line `page script sent to the web UI` says the scrolling bar is ready.

Each time you start a game, load a save or start an expedition, the log gets one more line saying where you are and what the mod did, for example `expedition: new orders now join the queue instead of replacing it`.

The same lines are in `BepInEx\full-queue.log`, which survives restarts.

If the queue bar still shows only 5 orders, look for a line starting `could not` or `failed` that mentions the queue bar. When that part cannot do its job, the bar stays as the game draws it.

If you still can only give one order at a time, look in the log for a line starting `could not` or `failed`. When the mod cannot do its part, the game keeps its one order at a time.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log` and `BepInEx\full-queue.log`.

To reach me directly, message `exiled.dev` on Discord; you can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx and the game's IL2CPP interop assemblies, so a game install with BepInEx set up and started once is required. Those assemblies come from the game and are not part of this repo. The .NET 8 SDK is required.

```
dotnet build src/FullQueue -c Release -p:GameDir="C:\path\to\Survival Log"
```

`GameDir` is the game folder. The default in the project file is a local path you will almost certainly need to override. The output DLL is at `src\FullQueue\bin\Release\FullQueue.dll`.

The unit tests cover the pure rules (where the character is, which places get the queue, and how the orders past the fifth are written for the queue bar), and run the queue bar's page script against a stand-in for the game's page. They need nothing from the game:

```
dotnet test tests/FullQueue.Tests
```

## Changelog

- **0.2.1**: Clearer setting descriptions in the settings file, now also in Simplified Chinese. No change to what the mod does.
- **0.2.0**: The queue bar now shows every waiting order, not just the first 5. With more than 5 it keeps its size and scrolls with the mouse wheel, and a small number shows how many orders are out of sight. Orders past the fifth can be cancelled by clicking them. New setting `ScrollQueue` turns this off.
- **0.1.0**: First release. Orders queue up in the countdown before the disaster and on expeditions, as at home.

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
