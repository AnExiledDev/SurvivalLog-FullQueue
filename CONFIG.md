# Full Queue settings

Settings live in `BepInEx\config\com.dev.survivallog.fullqueue.cfg`, created the first time the game starts with the mod. Edit the file while the game is closed; changes apply at the next game start. Delete the file to get the defaults back.

## General

| Setting | Default | What it does |
| --- | --- | --- |
| `Enabled` | `true` | Turns the whole mod on or off. When `true`, your character can queue several orders in the countdown before the disaster and on expeditions, as at home, and the queue bar shows every waiting order. When `false`, the game keeps one order at a time there and its own queue bar. |
| `QueueInPrologue` | `true` | When `true`, you can queue several orders in the countdown before the disaster at the start of a game. When `false`, each new order there replaces the ones still waiting, as in the game. |
| `QueueOnExpeditions` | `true` | When `true`, you can queue several orders on expeditions. When `false`, each new order there replaces the ones still waiting, as in the game. |
| `ScrollQueue` | `true` | When `true`, the queue bar at the bottom of the screen shows every waiting order. With more than 5 it keeps its size, scrolls with the mouse wheel, and a small number at its edge says how many orders are out of sight. When `false`, the bar shows only the first 5, as in the game. |

The tutorial is always left as the game has it, whatever these say.

There are no keys or buttons to set. The queue bar scrolls with the mouse wheel while the pointer is over it.
