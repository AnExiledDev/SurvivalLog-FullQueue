# Full Queue settings

Settings live in `BepInEx\config\com.dev.survivallog.fullqueue.cfg`, which is created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, or edit the file while the game is closed. If you change them on the Mods screen, QueueInPrologue applies the next time you load a game and QueueOnExpeditions applies from your next expedition. Enabled and ScrollQueue take effect after you restart the game. Delete the file to get the defaults back.

## General

| Setting | Default | What it does |
| --- | --- | --- |
| `Enabled` | `true` | Turns the whole mod on or off. When `true`, your character can queue several orders in the countdown before the disaster and on expeditions, the same way you can at home, and the queue bar shows every waiting order. When `false`, you get one order at a time there and the normal queue bar. |
| `QueueInPrologue` | `true` | When `true`, you can queue several orders in the countdown before the disaster at the start of a game. When `false`, each new order there replaces the ones still waiting, the same as without the mod. |
| `QueueOnExpeditions` | `true` | When `true`, you can queue several orders on expeditions. When `false`, each new order there replaces the ones still waiting, the same as without the mod. |
| `ScrollQueue` | `true` | When `true`, the queue bar at the bottom of the screen shows every waiting order. With more than 5 it keeps its size and scrolls with the mouse wheel, and a small number at its edge says how many orders are out of sight. The game has no queue bar in the countdown before the disaster, so the mod adds one in the bottom left corner. When `false`, the bar shows only the first 5 and the countdown has no queue bar, the same as without the mod. |

The mod doesn't change how orders queue in the tutorial, whatever these settings are.

There are no keys or buttons to set. Scroll the queue bar with the mouse wheel while the pointer is over it.
