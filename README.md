# Hunt Oxide Mod Plugin
This is a RPG system in development, I'm releasing it now to get feedback.

Currently the RPG is composed of:
* Levels
* Stats Points:
  * Agility : Increases your change to dodge attacks
  * Strength : Increases you Health
  * Intelligence : Decreases the crafting time
* Skill Points
* Skills
  * Lumberjack : Increases the gather rate for wood
  * Miner : Increases the gather rate for ores and stones
  * Hunter : Increases the gather rate from animals resources

You get exp when you gather stuff for now, later building stuff will give you exp to (hopefully, upgrading will give you more)
You get 1 stat point automatically assigned to each attribute when you level up, and 3 to distribute.
You get 1 skill point when you level up to distribute.

There is a in game help but is not 100% done yet, but the basics are.
**To see the list of commands type /hunt or /h**

The existing skills do not require level to get,  but the code allows restraining that. Also there is a max level for the current skills, 20 for now. So getting more than 60 levels will only give you the stats benefits for now. Max level for now is 200.

**[INSTALLATION]**
Just copy the .dll file to Managed folder.

*There is a config file and a data file, for now I recommend not changing then.*

*[COMPATIBILITY]*
I suggest that you dont use plugins that change the gather rate and crafting rate because the RPG will loose its purposes.

*[IMPORTANT]*
Since I'm going to add plenty more features there may be needed to reset the RPG, so dont get to attached.

**Please**, if you want to help with the plugin, make a push request at:
https://github.com/pedrorodrigues/hunt

I'll check your code and probably accept the request, if that happens I can give you my contact information so we can work together.

**[FORKING]**
You are welcome to fork my code, but I would really like if you keep me in the credits. I spent a lot of time researching the assembly, not for what there is in the extension now, but for features that I wanted to do, so if you could do that, you are more than welcome to fork it.

**[ADMIN CHAT COMMANDS]**
/hunt lvlup <desired_level> : will only level the admin character level, will give stats points and skill points to.

**[NEXT FEATURES]**
The next features I'm going to are:
* Researches skills - Probably going to separate this into specific research skills as, research tools, cloths, weapons, and others(medic stuff, code lock, etc)
* Taming - Not sure if I'm gone be able to do that, but the idea here is to tame wolf or bear so that it follows you, and only attack others, It's gonna be tricky.
* Blink to arrow - Teleport to where the arrow fall, I will probably limit the distance based on level.
* Blacksmith - Will increase the melting rate (Don't know if its an instance attribute yet) or increase the amount of results from using the fournace.
* Tailor - Will let you make stronger clothes (Again, not sure if its possible)
