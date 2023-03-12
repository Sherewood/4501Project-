# 4501Project-
 4501 project

 Group members: Andrew Krutelevich, Danny Michaud, Julien Rougerie

 Student IDs: 101146675, 101121600, 101067032

# Missing from proposal
-------------------------------------------

Area selection - saving for the advanced prototype

Special abilities - ran short on time unfortunately, should be in advanced prototype

Visual indication is also limited, but it wasn't fleshed out in the proposal anyways so not sure why I mentioned this here.

# Testing
-------------------

- Use PrototypeDemoScene, other scenes probably don't even compile at this time lol

To the south, there are resource deposits. Use a worker to harvest them (small blueish dude who kinda looks like a mass effect husk)

    -if your worker died somehow, the civilian buildings (skyscrapers) will spawn workers

After the initial battle is settled, there are some enemy units to the west if you want to test out the combat actions.

To the east, all of the buildings in the game have been placed for you to test.

# Controls
-------------------

Camera: Use arrow keys to move

Unit selection: Left click on a unit to select it.
    Note: Multi-unit selection not available, will not be supported until advanced prototype

Movement: Right clicking on an empty region of the game world will cause your selected unit to move towards it

Return to base: Clicking on the 'returnToBase' button with a unit that supports the action selected will cause it to return to the Main Base.

Attack: Right clicking on an enemy unit with an ally unit selected will cause that unit to attack the enemy unit.

Guard: Clicking on the 'guard' button in the UI with a unit that supports the action selected will cause it to do the following

    -attacks nearby enemies as before
    -when done attacking enemies, it will return to the position it was in when it was commanded to guard.

Fortify: Clicking on the 'fortify' button in the UI with a unit that supports the action selected will cause it to hold position, and receive a defense bonus.

Return to base: Clicking on the 'return to base' button in the UI with a unit that supports the action selected will cause the unit to return to the main base.

Harvest resource: With a worker selected, right clicking on a resource deposit will cause the worker to head towards the deposit, 
    then start extracing resources if they are available.

Construct building: Perform the following sequence of actions with a worker selected.

    1. Click on the 'construct' button in the UI
    2. Click on the option that represents the building you want to construct, on the left side of the UI
    3. Right click on an empty region of the game world, the worker will then move towards that area and generate a building.

Evacuate civilians: Clicking on the 'evacuateCivies' button with a civlian building selected will cause the building to periodically start removing civlians, which
are then transferred to the main base.

Evacuate planet: Clicking on the 'evacuatePlanet' button with the main base selected will cause the game to end after ~30 seconds, if you met the requirements

    -for testing purposes: Set the threshold to 5 civilians evacuated and 0 fuel. Test out the game ending functionality by evacutating some civilians from 1+ civilian buildings,
        then trying to evacuate the planet (just wait ~10-15 seconds for the civilians to be evacuated, then 30 seconds after trying to evacuate the planet)

# Extra
----------------------------

To confirm the obvious - we haven't done any animation work with these models lol


# Model Sources:
------------------------------

Terrain materials
-------------------
Sandstone:https://polyhaven.com/a/mud_cracked_dry_riverbed_002
asphalt
https://polyhaven.com/a/asphalt_02

Infantry units
--------------------
Vangaurd By T.Choonyung:https://www.mixamo.com/#/?page=1&type=Character

Enemies
--------------------
Creature Crab (Edenite Devil):
https://assetstore.unity.com/packages/3d/characters/creatures/creature-crab-97918
Incestoid Crab Monster (Edenite Muncher):
https://assetstore.unity.com/packages/3d/characters/insectoid-crab-monster-lurker-of-the-shores-20-animations-107223
Weird ass spider thing (Edenite Ravager):
https://assetstore.unity.com/packages/3d/characters/creatures/fantastic-creature-1-103074

Barracks, factory, research lab, both vehicles
---------------------
RTS Space pack:
https://assetstore.unity.com/packages/3d/environments/sci-fi/rts-sci-fi-game-assets-v1-112251

UI icons
-----------------------
move Icon generated With Aiart:
https://creator.nightcafe.studio/creation/irJP7F5VQLfA5Y00vGZp

civbuilding also generated with Aiart:
https://creator.nightcafe.studio/creation/ImVQEdUZDVG3LgEZIxVH

BuildOptions: Research Lab:
https://www.pinterest.ca/pin/sci-fi-lab-techno-thriller-sci-fi--474144667012067693/

All Build Icons/UnitIcons/Ability Icons Except Research Lab:
Ai generated art made with nightcafe studios.
https://nightcafe.studio/
=======
Futuristic Soldier (used for Worker):
https://www.turbosquid.com/3d-models/3d-futuristic-soldier-sci-fi-character-male-1864193
Bazooka (RPG Infantry):
https://www.turbosquid.com/3d-models/3d-model-bazooka-rocket-launcher/361997
Machine Gun (Robot chain arms) (used for Heavy Minigun Infantry):
https://www.turbosquid.com/3d-models/free-chaingun-robotic-arm-3d-model/480360
Drill (not used at this time, but might be for worker long term):
https://www.turbosquid.com/3d-models/3d-berserker-drill-machine-model-2019334
Minerals (mineral deposit):
https://assetstore.unity.com/packages/3d/environments/fantasy/translucent-crystals-106274
Main Base, Civilian building, fuel deposit:
https://free-game-assets.itch.io/free-space-colony-3d-low-poly-models
Textures taken from sources not included:
Burnt metal:
https://media.istockphoto.com/id/89364201/photo/dark-background-abstract.jpg?s=612x612&w=0&k=20&c=86aHEgeG9bxRS5YQfEMX0zoGvX7gZ0m8ELgcmotWtdw=

