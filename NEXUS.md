Name: Full Queue
Summary: Queue up several orders in the countdown at the start of a game and on expeditions, the same way you can at home, instead of one at a time. 开局的灾变倒计时和外出探索中也能像在家一样连续下达多个操作排进队列，不再一次只能一个。

Description (BBCode):
At home, Survival Log lets you line up orders: walk there, pick that up, cook this. In the countdown before the disaster and on expeditions it only keeps one, and every new order throws away the ones still waiting. Full Queue lets your orders queue up there too, so a new order joins the end of the queue like it does at home.

[size=5]Main features[/size]
[list]
[*][b]A full queue at the start of a game[/b]: in the countdown before the disaster, new orders join the queue instead of replacing it.[/*]
[*][b]A full queue on expeditions[/b]: the same on an expedition.[/*]
[*][b]Only one thing changes[/b]: a new order joins the end of the queue instead of replacing it. Nothing else about orders is changed.[/*]
[*][b]See every waiting order[/b]: the queue bar at the bottom of the screen shows all your waiting orders, not just the first 5. With more than 5 it keeps its size and scrolls with the mouse wheel, and a small number at its edge says how many orders are out of sight. Click any of them to cancel it.[/*]
[*][b]Each place has its own switch[/b]: turn the queue off for the start of a game or for expeditions if you prefer the game's way there.[/*]
[/list]
No new keys or buttons. Full Queue adds no data to your save and does not edit game files. You can remove it at any time.

Works in English and Chinese.

[size=5]Known issues[/size]
[list]
[*]When the countdown ends and the disaster hits, the game drops any orders still waiting, as it did before.[/*]
[*]When an expedition ends, any orders still waiting are dropped. They do not come home with you.[/*]
[*]The tutorial is left as the game has it. Its guided steps may expect one order at a time, so the mod stays out.[/*]
[*]In the countdown before the disaster the queue bar you see at home may not show, so you may not see your waiting orders there. Showing, reordering or cancelling queued orders in the countdown is untested.[/*]
[*]The expedition part is the least tested. If you still can only give one order at a time on an expedition, please tell me and include [font=Courier New]BepInEx&#92;full-queue.log[/font].[/*]
[/list]
If something odd happens with orders at the start of a game or on an expedition, try it with Enabled set to false to see whether the mod caused it, and please tell me either way (see Feedback below).

[size=5]Requirements[/size]
[list]
[*]Survival Log 1.1.18902. A game update can break it until the mod is updated.[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url] (BepInEx 6.0.0-be.788, IL2CPP). Install it first.[/*]
[/list]

[size=5]Installation[/size]
[list=1]
[*]Install [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i](if no other mods were installed before, start the game once so BepInEx finishes setup, then quit)[/i].[/*]
[*]Put [font=Courier New]FullQueue.dll[/font] in the BepInEx&#92;plugins folder inside the game folder. Full path example:
[code]C:&#92;Program Files (x86)&#92;Steam&#92;steamapps&#92;common&#92;Survival Log&#92;BepInEx&#92;plugins&#92;FullQueue.dll[/code][/*]
[/list]

[size=5]Uninstall[/size]
Delete [font=Courier New]FullQueue.dll[/font] from the [font=Courier New]BepInEx&#92;plugins[/font] folder.

[size=5]Configuration[/size]
To turn the mod off, the queue off for the start of a game or for expeditions, or the queue bar back to the game's first 5 orders, edit [font=Courier New]BepInEx&#92;config&#92;com.dev.survivallog.fullqueue.cfg[/font] while the game is closed (see [url=https://github.com/AnExiledDev/SurvivalLog-FullQueue/blob/main/CONFIG.md]CONFIG.md[/url] for every setting). Changes apply at the next game start.

[size=5]Feedback[/size]
Found a bug or have an idea? Come say hi on the [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url], or leave a comment here. For bugs, please include [font=Courier New]BepInEx&#92;LogOutput.log[/font] and [font=Courier New]BepInEx&#92;full-queue.log[/font].

[size=5]Source and license[/size]
Open source under MIT: [url=https://github.com/AnExiledDev/SurvivalLog-FullQueue]github.com/AnExiledDev/SurvivalLog-FullQueue[/url]

[line]
[size=5]简体中文[/size]

在家里，Survival Log 允许你把操作排成队列：走到那里、捡起那个、煮这个。但在灾变前的倒计时和外出探索中只会保留一个，每下达一个新操作，还在等待的操作就会被全部丢掉。Full Queue 让这些时候也能排队，新操作会像在家一样排到队列末尾。

[size=5]主要功能[/size]
[list]
[*][b]开局也能排满队列[/b]：灾变前的倒计时中，新操作会加入队列，而不是替换掉队列。[/*]
[*][b]外出探索也能排满队列[/b]：外出探索时同样如此。[/*]
[*][b]只改这一处[/b]：新操作排到队列末尾，而不是替换掉队列。操作的其他方面一概不变。[/*]
[*][b]看到每个等待中的操作[/b]：画面下方的队列栏会显示所有等待中的操作，不再只显示前 5 个。超过 5 个时，队列栏大小不变，可以用鼠标滚轮滚动，边缘的小数字会显示那一侧还有几个操作不在视野内。点击任意一个即可取消它。[/*]
[*][b]每处单独开关[/b]：如果你更喜欢游戏原本的做法，可以单独关闭开局或外出探索时的队列。[/*]
[/list]
没有新增按键或按钮。Full Queue 不会往存档里添加任何内容，也不会修改游戏文件，随时可以移除。

支持英文和中文。

[size=5]已知问题[/size]
[list]
[*]倒计时结束、灾变来临时，游戏会丢掉所有还在等待的操作，原版也是如此。[/*]
[*]外出探索结束时，所有还在等待的操作都会被丢掉，不会带回家。[/*]
[*]新手教程保持游戏原样。教程的引导步骤可能默认一次只有一个操作，所以模组不会介入。[/*]
[*]灾变前的倒计时中，家里能看到的队列栏可能不会出现，所以那时你可能看不到等待中的操作。倒计时中显示、调整顺序或取消排队操作的情况尚未测试。[/*]
[*]外出探索部分测试得最少。如果外出探索时仍然一次只能下达一个操作，请告诉我，并附上 [font=Courier New]BepInEx&#92;full-queue.log[/font]。[/*]
[/list]
如果开局或外出探索时操作出现异常，请把 Enabled 设为 false 再试一次，看看是不是模组造成的，无论结果如何都欢迎告诉我（见下方「反馈」）。

[size=5]前置要求[/size]
[list]
[*]Survival Log 1.1.18902。游戏更新后模组可能暂时失效，直到模组更新为止。[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url]（BepInEx 6.0.0-be.788，IL2CPP），请先安装。[/*]
[/list]

[size=5]安装[/size]
[list=1]
[*]安装 [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i]（如果之前没有装过其他模组，先启动一次游戏，让 BepInEx 完成初始化，然后退出）[/i]。[/*]
[*]把 [font=Courier New]FullQueue.dll[/font] 放进游戏目录下的 BepInEx&#92;plugins 文件夹。完整路径示例：
[code]C:&#92;Program Files (x86)&#92;Steam&#92;steamapps&#92;common&#92;Survival Log&#92;BepInEx&#92;plugins&#92;FullQueue.dll[/code][/*]
[/list]

[size=5]卸载[/size]
从 [font=Courier New]BepInEx&#92;plugins[/font] 文件夹中删除 [font=Courier New]FullQueue.dll[/font]。

[size=5]配置[/size]
如需关闭整个模组、单独关闭开局或外出探索时的队列，或让队列栏恢复为游戏原本只显示前 5 个操作，请在游戏关闭时编辑 [font=Courier New]BepInEx&#92;config&#92;com.dev.survivallog.fullqueue.cfg[/font]。配置文件中每项设置都有中文说明，完整说明见 [url=https://github.com/AnExiledDev/SurvivalLog-FullQueue/blob/main/CONFIG.md]CONFIG.md[/url]（英文）。修改会在下次启动游戏时生效。

[size=5]反馈[/size]
发现 bug 或有好点子？欢迎来 [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url] 打个招呼，或在这里留言。报告 bug 时，请附上 [font=Courier New]BepInEx&#92;LogOutput.log[/font] 和 [font=Courier New]BepInEx&#92;full-queue.log[/font]。

[size=5]源代码与许可[/size]
基于 MIT 许可开源：[url=https://github.com/AnExiledDev/SurvivalLog-FullQueue]github.com/AnExiledDev/SurvivalLog-FullQueue[/url]
