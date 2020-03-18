# ChatSource

![Icon](https://raw.githubusercontent.com/direwolf420/ChatSource/master/icon.png)

Terraria Forum link: https://forums.terraria.org/index.php?threads/chat-source-shows-the-source-of-messages-in-the-chat.86574/

For those who need to know which mod writes stuff to the chat!

#### Common situation:

![Situation](https://raw.githubusercontent.com/direwolf420/ChatSource/master/situation.png)

Shows the source of messages in the chat. Clientside, toggleable via config

#### Example:

**Before:**

```
Your world has been blessed with Cobalt!"
Player got 10 tier 1 Reversivity coins and now has 70 in total."
```

**After:**

```
"Your world has been blessed with Cobalt!"
"[AlchemistNPC] Player got 10 tier 1 Reversivity coins and now has 70 in total."
```


Notes
* Some stuff like ore spawn announcements in multiplayer will show no mod associated because those messages are serversided
* "[Terraria]" prefix for vanilla messages is by default disabled
* To view the display name of a mod you don't know, type in chat for example "/modName RecipeBrowser"
