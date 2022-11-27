## LuckyBranches

## What..
Allows the User to find, depending on the biome currently positioned in, with a configurable chance and amount, additional special woods:
- Fine wood in the Meadows,
- Round logs in Black Forest and last but not least
- Ancient Bark in the swamps.

## Config..
The settings would mostly reside in the &lt;ValheimDirector&gt;\BepInEx\config\mennowar.mods.FineWoodBranches.cfg

 ### Section [General]
setting | value | meaning
------- | ----- | -------
isEnabled | True / False | Enables or disables the complete mod
dropChance | int | The %-Chance to find a special wood
dropAmount | int | How many special wood the user may find additional to the default wood from branches
showMessage | True / False | Sets a value whether the Information-Message when found some wood should be displayed

 #### Section [Debug]
 writeDebug - True / False - Sets a value, indicating if additional log lines in the console should be written

 #### Section [Biomes]
setting | value | meaning
------- | ----- | -------
meadowsEnabled | True / False | Enable or Disable finding of the wood in the Meadows
blackforestEnabled | True / False | Enable or Disable finding of the wood in the Black Forest
swampEnabled | True / False | Enable or Disable finding of the wood in the Swamp
 
 <i style="font-size:0.7em">*Here is no Plains added, because when entering the Plains, you are supposed to be able to get all woods with ease.</i>

### Other..
It should be installed in the server and the client. Changing the Config requires restarting the game to take effect.<br />
Thanks go out to the lovely &quot;<b>schattentraum</b>&quot; for the idea of this mod.