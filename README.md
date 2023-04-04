# 4501Project-
 4501 project

 Group members: Andrew Krutelevich, Danny Michaud, Julien Rougerie

 Student IDs: 101146675, 101121600, 101067032

# Advanced Prototype feature notes
-------------------------------------------
# Splines
-----------------------------------------------------
- Tried to get splines working by using unity pathfinding to determine the control points,
then modifying the animation microdemo CR-spline code to work with the Movement component.
- Ease function was modified to act as an offset on t, rather than replacing the value of t entirely.
This is to support 'dynamic spline update' functionality, where I developed an algorithm to determine what
percentage of the path had actually been completed when the spline path had to be recalculated. 
The purpose of this was to keep the ease function from starting at 0 whenever a new spline path was calculated to replace a portion of another spline's
path, thus keeping the motion as smooth as possible. However, this didn't work all that well as calculating a new path still led to a brief stall in the motion.
- Originally wanted to just include this in the prototype demo scene, but due to the bugginess decided to create a separate SplineDemoScene for you to view.
Simply select the infantry unit and move it to the end of the path created by the civilian buildings.
- Speed control is there but can be a bit inconsistent in some cases.
- Due to setting buildings as NavMeshObstacles, ran into issues plotting spline paths that involved returning to a specific building, as the path planning failed
when a destination point was not on the NavMesh. Came up with methods FindUnobstructedPath and FindUnobstructedPathUnitHasObstacle in the Movement component for dealing with such issues.
- Speaking of methods, here are the relevant methods in the Movement component
StartSplineMovement: Gets the path for the spline to follow, and initiates movement along that path
SplineMovementUpdate: Updates the unit's movement along the spline path.
HandleDynamicSplineChange: Handles changing the spline when the target destination has moved by a certain threshold

# Animation 
--------------------------------------------------------
-Animation controllers have been made for for all soldiers, with the infantry and rocket launchers sharing the same controller while heavy machine gun receives a different version (one with a modified fireing animation of a flex). 

(animation_controller script is in Assets/)

- Current motions in game: Run/walk (the unit performs a movement of moving there legs, since the root is disabled, they should be moving in conjunction with the spline functionality). Fire/Attack: performs an attacking action which moves in conjunction with the units bullets or attack. Die: if theres no health left on the unit, the unit falls down dead. Harvest: a worker specific motion, basically just the unit crouching and a drill rotating with sparks. Idle: a unit's idle animation, should be returned with every occasion the unit is not moving.


-Enemy Units models came with an animation controllers but those have now also been remade. They now have less movement options from before but their controllers are now constructed from our own input rather than taking a premade one. 

-Using the any state, the controllers ideally should allow their units to cycle through their used animations: If you make a unit walk, it should walk, if a unit gets in range and should fire, the animation should play of the unit firing, and if the unit takes enough damage to die, they should have a death animation.
-There are a few other animation options a unit can do (some have an option to run and some have the option to crouch) but i believe they are not yet called in function. 

-Workers have a unique method for gathering minerals called the Harvest state.
-In the controller, all controllers share the same boolean conditions, more for the sake of running them with one controller.
-Some flourishes have been added to most units for specific animations. For example, if an infantry fires, the animation state will also enable some particle effects to show the soldiers gun firiing.

-Vehicles have no animations (well, artillary vehicle has some built in but it's incompatible with controller motion). The vehicle controller has no motions in it but the states are used to determine the state of the vehicle (is it firing? is it dead? etc....) 

- Units with no weapons have the attack function instead.
- Things which could be added/improved:
  -adding more motion to the vehicles 
  -giving the worker an attack animation (as it has no weapon)
  -giving the buildings some animation to represent if it's destroyed or functioning. 
  

# Flocking
---------------------------------------------
Tried to get working with selection of a group of units (leader will be the first unit in the selection which might be at a location you do not expect)

It's not tuned properly at all, alignment force in particular doesn't seem to have any effect. 

Leader will seek and wander, and units in the flock will follow but its very janky. Had to force the x/z portions of the rotations to 0 to prevent the units from falling over.
Might try a new collider in the final deliverable that avoids this problem? (capsule? :( )

Test by going into FlockingDemoScene, and clicking and dragging to select all of the units, then right click to move to a destination.

Code is in Unit Controller, and in Movement component (physicsBasedMovementUpdate)

# Misc features added in advanced prototype
--------------------------------------------
Area selection - Click and drag from one point to another to select all of the units in the region.
Since the camera is angled, had to use oriented bounding box collision, in this case just determined if 
each unit's position was inside the bounding box or not using a similar approach to the algorithm from the physics slides.

Visual indications - Added a health bar for each unit, not currently attached to the unit's hierarchy but should definitely consider doing that... 

# Missing from proposal
-------------------------------------------
Special abilities - ran short on time unfortunately, should be in final deliverable
    -took time planned for this, and spent it refactoring the movement/attack/targeting+ components to serve a new AI control component, which will be included in final deliverable
    (in summary: no more horrible 'ordered movement' spaghetti code, yay!)

Edenite Ravager was supposed to have multiple weapons, unclear if will make it into final delieverable

Visual indications are also limited, but it wasn't fleshed out in the proposal anyways so not sure why I mentioned this here.

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

1. Apologies for the mess in the Assets > Prefabs > Units folder, to see all the game unit prefabs check the "GameUnitsHere" subfolder within that folder.

2. To confirm the obvious - we haven't done any animation work with these models lol


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

UI background:
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