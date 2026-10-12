# Full Queue settings

Settings live in `BepInEx\config\com.dev.survivallog.fullqueue.cfg`, created the first time the game starts with the mod. Edit the file while the game is closed; changes apply at the next game start. Delete the file to get the defaults back.

## General

| Setting | Default | What it does |
| --- | --- | --- |
| `Enabled` | `true` | Let your character queue several orders in the countdown before the disaster and on expeditions, as at home, and show every waiting order in the queue bar. `false` turns the whole mod off: the game's one order at a time, and the game's queue bar. |
| `QueueInPrologue` | `true` | Queue orders in the countdown before the disaster at the start of a game. `false` keeps the game's one order at a time there. |
| `QueueOnExpeditions` | `true` | Queue orders on expeditions. `false` keeps the game's one order at a time there. |
| `ScrollQueue` | `true` | Show every waiting order in the queue bar at the bottom of the screen. With more than 5 the bar keeps its size and scrolls with the mouse wheel, and a small number at its edge says how many orders are out of sight. `false` leaves the game's bar, which shows the first 5. |

The tutorial is always left as the game has it, whatever these say.

There are no keys or buttons to set. The queue bar scrolls with the mouse wheel while the pointer is over it.
