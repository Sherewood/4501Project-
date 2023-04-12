# 4501Project-
 4501 project

 Group members: Andrew Krutelevich, Danny Michaud, Julien Rougerie

 Student IDs: 101146675, 101121600, 101067032

# Final deliverable requirement notes
-------------------------------------------
(More detail provided in report - relevant section numbers provided here)

1) Pathfinding
------------------
Implemented using Unity pathfinding system as required. Buildings act as obstacles.
See the following sections in the report for more: 3.6, 4.1.1, 4.1.2, 4.1.3, 4.1.5

2) Semi-automatic actions
------------------
Implemented in two major ways -> combat unit automatic engagement of the enemy, and workers moving between deposits and the main base.
See the following sections in the report for more: 3.7.1

3) Enemy AI
------------------
Implemented with a rule-based system using text files. Unique behaviors for each of the 3 main enemy units.
See the following sections in the report for more: 3.7, 4.2

4) Full game mechanics
------------------
Game has reasonably complex mechanics imo.
See the following sections in the report for more: section 1 in full, 3.8

5) Visual feedback
-------------------
Several forms of visual feedback have been used.
See the following sections in the report for more: section 3.9

6) Report and Game Demonstration
------------------
...see report and game demonstration attached, if they aren't attached then I'm dumb lmao

# Final deliverable Bonus features
------------------

1) Research Tree - see section 5.1 of report

2) Game Narrator - see section 5.2 of report

3) Minimap - see section 5.3 of report

4) Game menus - see section 5.4 of report


# Where to find things
------------------

1) Models

    Actual units in the game are in Assets > Prefabs > Units > GameUnitsHere
        -models are mostly third party assets as before obviously

    Assets > Prefabs > Units > UnitUIElements contains the prefabs for visual effects that are attached to units in the scene
        -unit spawn effect is third party, others aren't

        -see 3.9

    Rest is a bunch of imported third party assets (see citations)

2) Code
See Assets > Scripts, then look at following subdirectories

Note: Worth reading section 2.1 of the report before diving in

    1) External Controller > External Controller classes (see section 2.4, 4.3 of report)

    2) Internal Controller > Internal Controller classes (see sections 2.5, 4.3, 4.4 of report)

    3) Model > Model classes (see section 2.3, 4.3 of report)

    4) UI > UI-specific classes (see section 2.4 of report, 2.4.2 in particular)

    5) Units > Unit components (see section 2.2 of report)

        -hierarchy defined for AI Control components (see section 4.2.2 of report)

        -hierarchy defined for powerup components (see section 2.2 of report)

3) Materials/Shaders

See Assets > Materials, then look at following subdirectories (section 3.9 in general helps a lot here)

    1) Building Effects > Visual effects applied to buildings
    2) Indicators > Effects that are attached to units or appear on the terrain to indicate something important
    3) Misc > idk lol
    4) Progress Bar > Effects used for health bar/assorted progress bars used to convey info to user

4) AI rule files (4.2 of report helpful here)

See Assets > AI

# Missing from proposal
-------------------------------------------
Edenite Ravager was supposed to have multiple weapons, it is mentioned why this wasn't included in section 2.2.3 of the report

# Testing
-------------------

- Start from the StartScene in Scenes > FinalProduct > Menus > StartScene

- Click play to start the game

# Controls
-------------------

Camera: Use arrow keys to move

Unit selection: Left click on a unit to select it.
    Multi-unit selection: Click and drag, then release at a point on the game world.

Movement: Right clicking on an empty region of the game world will cause your selected unit to move towards it

Return to base: Clicking on the 'returnToBase' button with a unit that supports the action selected will cause it to return to the Main Base.

Attack: Right clicking on an enemy unit with an ally unit selected will cause that unit to attack the enemy unit.

Guard: Clicking on the 'guard' button in the UI with a unit that supports the action selected will cause it to do the following

    -attacks nearby enemies as before
    -when done attacking enemies, it will return to the position it was in when it was commanded to guard.

Fortify: Clicking on the 'fortify' button in the UI with a unit that supports the action selected will cause it to hold position, and receive a defense bonus.

Return to base: Clicking on the 'return to base' button in the UI with a unit that supports the action selected will cause the unit to return to the main base.

Harvest resource: With a worker selected, right clicking on a resource deposit will cause the worker to head towards the deposit, 
    then start extracing resources if they are available. See 3.7.1 of the report for more on the behavior

Construct building: Perform the following sequence of actions with a worker selected.

    1. Click on the 'construct' button in the UI
    2. Click on the option that represents the building you want to construct, on the left side of the UI
    3. Right click on an empty region of the game world, the worker will then move towards that area and generate a building.

Evacuate civilians: Clicking on the 'evacuateCivies' button with a civlian building selected will cause the building to periodically start removing civlians, which
are then transferred to the main base.

Evacuate planet: Clicking on the 'evacuatePlanet' button with the main base selected will cause the game to end after ~30 seconds, if you met the requirements

    -for testing purposes, the threshold is set to 5 civilians and 100 fuel.

# Extra
----------------------------

⢀⡴⠑⡄⠀⠀⠀⠀⠀⠀⠀⣀⣀⣤⣤⣤⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀ 
⠸⡇⠀⠿⡀⠀⠀⠀⣀⡴⢿⣿⣿⣿⣿⣿⣿⣿⣷⣦⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀ 
⠀⠀⠀⠀⠑⢄⣠⠾⠁⣀⣄⡈⠙⣿⣿⣿⣿⣿⣿⣿⣿⣆⠀⠀⠀⠀⠀⠀⠀⠀ 
⠀⠀⠀⠀⢀⡀⠁⠀⠀⠈⠙⠛⠂⠈⣿⣿⣿⣿⣿⠿⡿⢿⣆⠀⠀⠀⠀⠀⠀⠀ 
⠀⠀⠀⢀⡾⣁⣀⠀⠴⠂⠙⣗⡀⠀⢻⣿⣿⠭⢤⣴⣦⣤⣹⠀⠀⠀⢀⢴⣶⣆ 
⠀⠀⢀⣾⣿⣿⣿⣷⣮⣽⣾⣿⣥⣴⣿⣿⡿⢂⠔⢚⡿⢿⣿⣦⣴⣾⠁⠸⣼⡿ 
⠀⢀⡞⠁⠙⠻⠿⠟⠉⠀⠛⢹⣿⣿⣿⣿⣿⣌⢤⣼⣿⣾⣿⡟⠉⠀⠀⠀⠀⠀ 
⠀⣾⣷⣶⠇⠀⠀⣤⣄⣀⡀⠈⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⠀⠀⠀⠀⠀⠀ 
⠀⠉⠈⠉⠀⠀⢦⡈⢻⣿⣿⣿⣶⣶⣶⣶⣤⣽⡹⣿⣿⣿⣿⡇⠀⠀⠀⠀⠀⠀ 
⠀⠀⠀⠀⠀⠀⠀⠉⠲⣽⡻⢿⣿⣿⣿⣿⣿⣿⣷⣜⣿⣿⣿⡇⠀⠀⠀⠀⠀⠀ 
⠀⠀⠀⠀⠀⠀⠀⠀⢸⣿⣿⣷⣶⣮⣭⣽⣿⣿⣿⣿⣿⣿⣿⠀⠀⠀⠀⠀⠀⠀ 
⠀⠀⠀⠀⠀⠀⣀⣀⣈⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠇⠀⠀⠀⠀⠀⠀⠀ 
⠀⠀⠀⠀⠀⠀⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠃⠀⠀⠀⠀⠀⠀⠀⠀ 
⠀⠀⠀⠀⠀⠀⠀⠹⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀ 
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠛⠻⠿⠿⠿⠿⠛⠉

# Model Sources:
------------------------------

Terrain materials
-------------------
Sandstone:https://polyhaven.com/a/mud_cracked_dry_riverbed_002
asphalt
https://polyhaven.com/a/asphalt_02

Materials
-------------------
muzzle fash:https://twitter.com/VulshokB/status/1503791128612687879/photo/2

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
Edenite spawner visual effects:
https://assetstore.unity.com/packages/vfx/particles/blood-gush-73426

Barracks, factory, both vehicles
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

UI background
---------------------------
https://assetstore.unity.com/packages/2d/gui/sci-fi-gui-skin-15606
UI Event Director (face in the top right):
Ai generated in nightcafe:
https://creator.nightcafe.studio/creation/KybLHwnf7TFzD6V0Mw77
Television Screen effect (both on/off):
Taken from youtube video: https://www.youtube.com/watch?v=E-vv1aL-JHU
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
Main Base, research lab, fuel deposit:
https://free-game-assets.itch.io/free-space-colony-3d-low-poly-models
Textures taken from sources not included:
Burnt metal:
https://media.istockphoto.com/id/89364201/photo/dark-background-abstract.jpg?s=612x612&w=0&k=20&c=86aHEgeG9bxRS5YQfEMX0zoGvX7gZ0m8ELgcmotWtdw=
Animations:
Soldier controller animations:
Shooter pack mixamo:https://www.mixamo.com/#/?page=1&query=pack&type=Motion%2CMotionPack
Machine gunner fire: mixamo Mutant Flexing muscles: https://www.mixamo.com/#/?page=1&query=mutant+flexing+muscles&type=Motion%2CMotionPack
Creature animations: From the same pack as the creatures (see above)
UI director sound:
https://mixkit.co/free-sound-effects/robot/
BGM:
https://www.chosic.com/download-audio/45434/

# Code Sources:
-----------------
These are sites which helped develop certain scripts of code
The TimeTracker script was sourced form this online work:https://alexdunn.org/2017/05/31/unity-tip-create-a-rotating-sun/
The Typewriter effect was created using this site as a source: https://unitycoder.com/blog/2015/12/03/ui-text-typewriter-effect-script/
