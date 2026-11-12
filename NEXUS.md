Name: Full Queue
Summary: Queue up several orders in the countdown before the disaster and on expeditions, the same way you can at home. Without the mod, every new order there throws away the ones still waiting. The queue bar also shows every waiting order instead of stopping at 5, and scrolls with the mouse wheel. You can switch each place off on its own.

Description (BBCode):
[center][img]https://sldb.core-directive.com/media/save/1-save-safe.png[/img][/center]

At home, Survival Log lets you line up orders: walk there, pick that up, cook this. In the countdown before the disaster and on expeditions it only keeps one, and every new order throws away the ones still waiting. Full Queue lets your orders queue up there too, so a new order joins the end of the queue like it does at home.

[size=5]Main features[/size]
[list]
[*][b]A full queue at the start of a game[/b]: in the countdown before the disaster, new orders join the queue instead of replacing it.[/*]
[*][b]A full queue on expeditions[/b]: on an expedition, new orders join the queue instead of replacing it.[/*]
[*][b]Nothing else about orders changes[/b]: a new order waits its turn at the end of the queue, and that is the only difference.[/*]
[*][b]See every waiting order[/b]: the queue bar at the bottom of the screen shows all your waiting orders instead of stopping at 5. With more than 5 it keeps its size and scrolls with the mouse wheel, and a small number at each edge says how many orders are out of sight on that side. Click any order to cancel it, the same as the first 5. With 5 or fewer orders the bar looks the same as without the mod.[/*]
[*][b]A queue bar in the countdown[/b]: the game has no queue bar in the countdown before the disaster, so the mod adds one in the bottom left corner. The buttons there move up to make room for it. Like the normal queue bar, it starts with a big box showing what your character is doing right now, with a ring showing how far along it is. When your character is storing items, the box also shows how many are put away. Click an order or the big box to cancel it. The countdown queue bar goes away when the disaster starts and the normal one takes over.[/*]
[*][b]Each place has its own switch[/b]: you can turn the queue off for the countdown or for expeditions if you'd rather give one order at a time there.[/*]
[/list]
There are no new keys or buttons. Full Queue adds no data to your save and does not edit game files. You can remove it at any time.

Works in English and Chinese.

[size=5]Known issues[/size]
[list]
[*]When the countdown ends and the disaster hits, the game drops any orders still waiting. That happens without the mod too.[/*]
[*]When an expedition ends, any orders still waiting are dropped. They don't come home with you.[/*]
[*]The mod doesn't change how orders queue in the tutorial.[/*]
[/list]

[center][img]https://sldb.core-directive.com/img/mods-banner.jpg[/img]
[size=4][url=https://www.nexusmods.com/survivallog/mods/73]Get Mod Manager[/url] to set up all your mods in game  |  [url=https://sldb.core-directive.com/en/]Open the Survival Log wiki[/url] for items, recipes and maps[/size][/center]

[size=5]Requirements[/size]
[list]
[*]Survival Log 1.1.20719. A game update can break the mod until it is updated.[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url] (BepInEx 6.0.0-be.788, IL2CPP). Install it first.[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.20.0 or newer. Full Queue doesn't load without it. It applies settings you change on the Mod Manager's Mods screen without restarting the game.[/*]
[*]Optional: [url=https://www.nexusmods.com/survivallog/mods/73]Mod Manager[/url]. With it, you can change every setting in game on its Mods screen and see what each one does, without opening the config file.[/*]
[/list]

[size=5]Installation[/size]
[list=1]
[*]Install [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i](if you haven't installed any other mods before, start the game once so BepInEx finishes setting up, then quit)[/i].[/*]
[*]Install Exiled's Toolkit the same way. Download its zip and extract it into the game folder.[/*]
[*]Download the zip from the Files tab.[/*]
[*]Extract it into the game folder (the folder with [font=Courier New]SurvivalLog.exe[/font]). The zip holds a BepInEx folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\FullQueue.dll[/code][/*]
[/list]

[size=5]Uninstall[/size]
Delete [font=Courier New]FullQueue.dll[/font] from the [font=Courier New]BepInEx\plugins[/font] folder.

[size=5]Configuration[/size]
You can turn the whole mod off, turn the queue off for the countdown or for expeditions, or set the queue bar back to showing only the first 5 orders. Change the settings in game on the Mod Manager's Mods screen, or edit [font=Courier New]BepInEx\config\com.dev.survivallog.fullqueue.cfg[/font] while the game is closed. [url=https://github.com/AnExiledDev/SurvivalLog-FullQueue/blob/main/CONFIG.md]CONFIG.md[/url] lists every setting. If you change them on the Mods screen, QueueInPrologue applies the next time you load a game and QueueOnExpeditions applies from your next expedition. Enabled and ScrollQueue take effect after you restart the game.

[size=5]Feedback[/size]
Found a bug or have an idea? Come say hi on the [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url], or post in this page's Posts tab. For bugs, please include [font=Courier New]BepInEx\LogOutput.log[/font] and [font=Courier New]BepInEx\full-queue.log[/font].
To reach me directly, message [b]exiled.dev[/b] on Discord. You can find me in the [url=https://discord.gg/MZGg4gQAUe]official Survival Log Discord[/url]. I'm also on X as [url=https://x.com/AnExiledDev]@AnExiledDev[/url].

[size=5]Source and license[/size]
Open source under MIT: [url=https://github.com/AnExiledDev/SurvivalLog-FullQueue]github.com/AnExiledDev/SurvivalLog-FullQueue[/url]

[line]
[size=5]简体中文[/size]

在家里，Survival Log 允许你把操作排成队列：走到那里、捡起那个、煮这个。但在灾变前的倒计时和外出探索中只会保留一个，每下达一个新操作，还在等待的操作就会被全部丢掉。Full Queue 让这些时候也能排队，新操作会像在家一样排到队列末尾。

[size=5]主要功能[/size]
[list]
[*][b]开局倒计时也能排队[/b]：灾变前的倒计时中，新操作会加入队列，而不是替换掉队列。[/*]
[*][b]外出探索也能排队[/b]：外出探索时，新操作会加入队列，而不是替换掉队列。[/*]
[*][b]操作的其他方面不变[/b]：唯一的区别是新操作会排到队列末尾等待执行。[/*]
[*][b]看到每个等待中的操作[/b]：画面下方的队列栏会显示所有等待中的操作，不再只显示前 5 个。超过 5 个时，队列栏大小不变，可以用鼠标滚轮滚动，两端的小数字会显示那一侧还有几个操作没显示出来。点击任意一个操作即可取消，和前 5 个一样。操作不超过 5 个时，队列栏和不装模组时一样。[/*]
[*][b]灾变倒计时中的队列栏[/b]：游戏在灾变前的倒计时中没有队列栏，所以模组会在左下角加上一个，那里的按钮会上移让出位置。和平常的队列栏一样，最前面有一个大方框，显示角色当前正在做的事，并用进度环显示做到了哪一步。角色存放物品时，大方框还会显示已经放好的数量。点击其中的操作或大方框即可取消。灾变开始后，倒计时的队列栏会消失，由平常的队列栏接替。[/*]
[*][b]每处单独开关[/b]：如果你想在倒计时或外出探索时一次只下一个操作，可以单独关闭那里的队列。[/*]
[/list]
没有新增按键或按钮。Full Queue 不会往存档里添加任何内容，也不会修改游戏文件，随时可以移除。

支持英文和中文。

[size=5]已知问题[/size]
[list]
[*]倒计时结束、灾变来临时，游戏会丢掉所有还在等待的操作。不装模组时也是这样。[/*]
[*]外出探索结束时，所有还在等待的操作都会被丢掉，不会带回家。[/*]
[*]模组不会改变新手引导中的操作队列。[/*]
[/list]

[url=https://www.nexusmods.com/survivallog/mods/73]获取 Mod Manager[/url]，在游戏里设置所有模组。[url=https://sldb.core-directive.com/zh/]打开 Survival Log 维基[/url]，物品、配方、地图一查就有。

[size=5]前置要求[/size]
[list]
[*]Survival Log 1.1.20719。游戏更新后模组可能暂时失效，直到模组更新为止。[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url]（BepInEx 6.0.0-be.788，IL2CPP），请先安装。[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.20.0 或更新版本。没有它本模组不会加载。有了它，在 Mod Manager 的「模组」界面修改的设置无需重启游戏即可生效。[/*]
[*]可选：[url=https://www.nexusmods.com/survivallog/mods/73]Mod Manager[/url]。装了它，可以在游戏里它的「模组」界面修改每项设置，并看到每项的说明，不用打开配置文件。[/*]
[/list]

[size=5]安装[/size]
[list=1]
[*]安装 [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i]（如果之前没有装过其他模组，先启动一次游戏，让 BepInEx 完成初始化，然后退出）[/i]。[/*]
[*]用同样的方式安装 Exiled's Toolkit，下载它的压缩包并解压到游戏目录。[/*]
[*]在「文件」页下载压缩包。[/*]
[*]把压缩包解压到游戏目录（[font=Courier New]SurvivalLog.exe[/font] 所在的文件夹）。压缩包里带有 BepInEx 文件夹，DLL 会自动放到正确位置。如果提示合并文件夹，选择合并。解压后应为：
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\FullQueue.dll[/code][/*]
[/list]

[size=5]卸载[/size]
从 [font=Courier New]BepInEx\plugins[/font] 文件夹中删除 [font=Courier New]FullQueue.dll[/font]。

[size=5]配置[/size]
你可以关闭整个模组，单独关闭倒计时或外出探索时的队列，或者让队列栏恢复为只显示前 5 个操作。可以在游戏内 Mod Manager 的「模组」界面修改，也可以在游戏关闭时编辑 [font=Courier New]BepInEx\config\com.dev.survivallog.fullqueue.cfg[/font]。配置文件中每项设置都有中文说明，完整说明见 [url=https://github.com/AnExiledDev/SurvivalLog-FullQueue/blob/main/CONFIG.md]CONFIG.md[/url]（英文）。在「模组」界面修改时，QueueInPrologue 在下次载入游戏时生效，QueueOnExpeditions 从下一次外出探索起生效。Enabled 和 ScrollQueue 在重启游戏后生效。

[size=5]反馈[/size]
发现 bug 或有好点子？欢迎来 [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url] 打个招呼，或在本页的「Posts」标签里发帖。报告 bug 时，请附上 [font=Courier New]BepInEx\LogOutput.log[/font] 和 [font=Courier New]BepInEx\full-queue.log[/font]。
想直接联系我，可以在 Discord 上给 [b]exiled.dev[/b] 发消息，我在 [url=https://discord.gg/MZGg4gQAUe]Survival Log 官方 Discord[/url] 里。也可以在 X 上找我：[url=https://x.com/AnExiledDev]@AnExiledDev[/url]。

[size=5]源代码与许可[/size]
基于 MIT 许可开源：[url=https://github.com/AnExiledDev/SurvivalLog-FullQueue]github.com/AnExiledDev/SurvivalLog-FullQueue[/url]
