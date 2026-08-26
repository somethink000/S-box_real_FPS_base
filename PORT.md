# Shaders

These are the project's own, written for the engine it came from. They came across as they were, and nothing here can compile one: this engine's shaders are its own format, bound its own way. Until each is rewritten, anything drawn with it draws nothing.

Their unit is an inch and this engine's is a metre, so every number in one of these that touches the world is wrong by about forty until it is changed. A **distance** - how far a wave of noise drifts, where a fade begins - is multiplied by `0.0254`. A **scale**, meaning a number the world position is multiplied by before anything is done with it, goes the other way and is multiplied by `39.37`: that is a frequency, and a frequency is one over a length. Numbers in texture coordinates, in normals, or in the cells of a noise that has already been scaled are in none of those units and want leaving alone.

- `map/gen_foliage.shader`
- `shaders/red_dot.shader`

# Animations with no source

These clips were read out of the compiled models themselves. That engine keeps a character's poses inside its model and ships no export beside them, so these files are the only copy of those animations there is. Deleting one loses it.

They are written compiled, with no `.anim` beside them, for the same reason: there is no art for one to name. Each model's clips sit in a folder next to it, and the model's definition lists them.

- `models/citizen_human/citizen_human_female.vmdl` — 444 clip(s)
- `models/citizen_human/citizen_human_male.vmdl` — 444 clip(s)

# What the port could not do

Each of these is something the port tried and did not manage. A file that would not read, a resource with nothing to build it from. Nothing here stopped the port; each is one thing that is missing from the project because of it.

- scenes\dom.vmap: its shape does not read: no numbering of the blocks has two patches agreeing about the samples they share, so the offsets are laid out some way this does not know - the surface was cut but not sculpted
- scenes\new map.vmap: its shape does not read: no numbering of the blocks has two patches agreeing about the samples they share, so the offsets are laid out some way this does not know - the surface was cut but not sculpted

# What the port did that is worth knowing

Decisions rather than failures: what was replaced with an equivalent, what was built rather than carried, what came out of a file that had no other copy.

- materials/grass_blend/forest_grass.vmat: a blended material of 4 layers - the first is what came across, because a material here holds one set of maps. The mesh keeps the weights it was painted with, so the rest is waiting on the material rather than lost.
- materials/grass_blend/forest_grass.vmat: its roughness, metalness and occlusion are separate pictures and none of them could be read, so the material keeps its own numbers. The files it named are missing from the project it was ported from.
- materials/grass_blend/forest_grass.vmat: its normal map is materials/default/default_normal.tga, which the project it was ported from does not contain - left empty, so the material draws plainly rather than as the missing texture.
- models\door\door.vmdl: door.fbx's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- models\doorframe_wood\door.vmdl: door.fbx's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- models\doorframe_wood\doorframe_wood.vmdl: doorframe_wood.fbx's mesh filter keeps 2 of its meshes - the rest were never meant to be drawn here
- models\pine_tree\elm_tree1\elm_tree1.vmdl: sm_elmtree_01.fbx's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- models\pine_tree\elm_tree2\elm_tree2.vmdl: sm_elmtree_02.fbx's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- models\pine_tree\tre1\sm_pinetree_01.vmdl: sm_pinetree_01.fbx's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- models\pine_tree\tre2\sm_pinetree_02.vmdl: sm_pinetree_02.fbx's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- models\pine_tree\tre3\sm_pinetree_03.vmdl: sm_pinetree_03.fbx's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- models\pine_tree\tre4\sm_pinetree_04.vmdl: sm_pinetree_04.fbx's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- scenes\dom.vmap: 1 mesh(es) had faces Hammer was subdividing, 0 of them with the shape they were sculpted into. A displacement is the cage in the mesh and the shape in a stream beside it; read without the stream, a lake is a ramp.
- scenes\new map.vmap: 1 mesh(es) had faces Hammer was subdividing, 0 of them with the shape they were sculpted into. A displacement is the cage in the mesh and the shape in a stream beside it; read without the stream, a lake is a ramp.
- weapons/ammos/12gauge.vmat: its roughness, metalness and occlusion are separate pictures and none of them could be read, so the material keeps its own numbers. The files it named are missing from the project it was ported from.
- weapons\ammos\12x76_slug.vmdl: 12x76_slug.smd's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- weapons\ammos\23mm.vmdl: 23mm.smd's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- weapons\ammos\45cal.vmdl: 45cal.smd's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- weapons\ammos\545x39.vmdl: 545x39.smd's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- weapons\ammos\762x39.vmdl: 762x39.smd's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- weapons\ammos\9x18.vmdl: 9x18.smd's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- weapons/ammos/ammo1.vmat: its roughness, metalness and occlusion are separate pictures and none of them could be read, so the material keeps its own numbers. The files it named are missing from the project it was ported from.
- weapons/ammos/ammo1.vmat: its normal map is materials/default/default_normal.tga, which the project it was ported from does not contain - left empty, so the material draws plainly rather than as the missing texture.
- weapons/ammos/ammo_light.vmat: its roughness, metalness and occlusion are separate pictures and none of them could be read, so the material keeps its own numbers. The files it named are missing from the project it was ported from.
- weapons/ammos/ammo_light.vmat: its normal map is materials/default/default_normal.tga, which the project it was ported from does not contain - left empty, so the material draws plainly rather than as the missing texture.
- weapons/ammos/cartridges.vmat: its roughness, metalness and occlusion are separate pictures and none of them could be read, so the material keeps its own numbers. The files it named are missing from the project it was ported from.
- weapons/awp/awp.ag: 11 state(s), 18 transition(s) and 19 clip(s) came across from their graph, driven by the 11 parameter(s) the game already sets.
- weapons/awp/awp.ag: 1 bone mask(s) came over with their weights.
- weapons\awp\w_awp.vmdl: 1 mesh(es) are hulls, not drawn
- weapons/bereta/beretta.ag: 10 state(s), 16 transition(s) and 21 clip(s) came across from their graph, driven by the 10 parameter(s) the game already sets.
- weapons/bereta/beretta.ag: 1 bone mask(s) came over with their weights.
- weapons\bereta\w_beretta.vmdl: 1 mesh(es) are hulls, not drawn
- weapons/emission.vmat: its roughness, metalness and occlusion are separate pictures and none of them could be read, so the material keeps its own numbers. The files it named are missing from the project it was ported from.
- weapons/emission.vmat: its normal map is materials/default/default_normal.tga, which the project it was ported from does not contain - left empty, so the material draws plainly rather than as the missing texture.
- weapons/granatas/granade.ag: 6 state(s), 6 transition(s) and 5 clip(s) came across from their graph, driven by the 3 parameter(s) the game already sets.
- weapons/granatas/granade.ag: 1 bone mask(s) came over with their weights.
- weapons\granatas\w\m26w.vmdl: m26w.smd's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- weapons/hands_l/hozq4rig.vmat: its roughness, metalness and occlusion are separate pictures and none of them could be read, so the material keeps its own numbers. The files it named are missing from the project it was ported from.
- weapons/hands_l/hozq4rig.vmat: its normal map is materials/default/default_normal.tga, which the project it was ported from does not contain - left empty, so the material draws plainly rather than as the missing texture.
- weapons/knife/knife.ag: 8 state(s), 12 transition(s) and 7 clip(s) came across from their graph, driven by the 6 parameter(s) the game already sets.
- weapons/knife/knife.ag: 1 bone mask(s) came over with their weights.
- weapons/mp7/mp7.ag: 10 state(s), 16 transition(s) and 24 clip(s) came across from their graph, driven by the 10 parameter(s) the game already sets.
- weapons/mp7/mp7.ag: 1 bone mask(s) came over with their weights.
- weapons\mp7\w_mp7.vmdl: 1 mesh(es) are hulls, not drawn
- weapons/pallete.vmat: its roughness, metalness and occlusion are separate pictures and none of them could be read, so the material keeps its own numbers. The files it named are missing from the project it was ported from.
- weapons/pallete_metal.vmat: its roughness, metalness and occlusion are separate pictures and none of them could be read, so the material keeps its own numbers. The files it named are missing from the project it was ported from.
- weapons/pallete_metal.vmat: its normal map is materials/default/default_normal.tga, which the project it was ported from does not contain - left empty, so the material draws plainly rather than as the missing texture.
- weapons/pallete_wood.vmat: its roughness, metalness and occlusion are separate pictures and none of them could be read, so the material keeps its own numbers. The files it named are missing from the project it was ported from.
- weapons/pallete_wood.vmat: its normal map is materials/default/default_normal.tga, which the project it was ported from does not contain - left empty, so the material draws plainly rather than as the missing texture.
- weapons/reming870/870.ag: 13 state(s), 23 transition(s) and 15 clip(s) came across from their graph, driven by the 11 parameter(s) the game already sets.
- weapons/reming870/870.ag: 1 bone mask(s) came over with their weights.
- weapons/reming870/870.ag: what has no counterpart here was passed through - Choice (1). The pose still arrives; what those nodes did to it does not.
- weapons\reming870\w_870.vmdl: 1 mesh(es) are hulls, not drawn
- weapons\shells\5_45x39mm.vmdl: 5_45x39mm.smd's mesh filter keeps 1 of its meshes - the rest were never meant to be drawn here
- weapons/vss/vss.ag: 10 state(s), 16 transition(s) and 16 clip(s) came across from their graph, driven by the 10 parameter(s) the game already sets.
- weapons/vss/vss.ag: 1 bone mask(s) came over with their weights.
- weapons/vss/vss.ag: what has no counterpart here was passed through - Choice (1). The pose still arrives; what those nodes did to it does not.
- weapons\vss\w_vss.vmdl: 1 mesh(es) are hulls, not drawn
- Materials/library_material.vmat: its roughness, metalness and occlusion are separate pictures and none of them could be read, so the material keeps its own numbers. The files it named are missing from the project it was ported from.
- Materials/library_material.vmat: its colour map is materials/default/default_color.tga, which the project it was ported from does not contain - left empty, so the material draws plainly rather than as the missing texture.
- Materials/library_material.vmat: its normal map is materials/default/default_normal.tga, which the project it was ported from does not contain - left empty, so the material draws plainly rather than as the missing texture.
- Libraries/evilinc.modeliconeditor/modeliconeditor.sbproj: came across as a library of its own, so its code is compiled with the project (ModelIconEditor)
- Code/Common/BasicParticleEmiter.cs: 3 number(s) in code multiplied through to metres
- Code/Common/Utils/SurfaceUtil.cs: 1 number(s) in code multiplied through to metres
- Code/Components/DoorComponent.cs: 1 number(s) in code multiplied through to metres
- Code/Components/ZombieSpawner.cs: 3 number(s) in code multiplied through to metres
- Code/Components/ZombiesButton.cs: 5 number(s) in code multiplied through to metres
- Code/NPC/Zombie.Detect.cs: 9 number(s) in code multiplied through to metres
- Code/NPC/Zombie.cs: 3 number(s) in code multiplied through to metres
- Code/Player/Controllers/HealthController.cs: 2 number(s) in code multiplied through to metres
- Code/Player/Controllers/Interactions/InteractionController.cs: 4 number(s) in code multiplied through to metres
- Code/Player/Controllers/Interactions/InteractionsComponent.cs: 3 number(s) in code multiplied through to metres
- Code/Player/Controllers/Movement/CameraController.cs: 4 number(s) in code multiplied through to metres
- Code/Player/Controllers/Movement/CharController.cs: 14 number(s) in code multiplied through to metres
- Code/Player/Controllers/Movement/MovementController.cs: 8 number(s) in code multiplied through to metres
- Code/Weapons/Bases/EntThrow.cs: 2 number(s) in code multiplied through to metres
- Code/Weapons/Bases/Gun/Bullet/HitScanBullet.cs: 1 number(s) in code multiplied through to metres
- Code/Weapons/Bases/Gun/Gun.Getters.cs: 1 number(s) in code multiplied through to metres
- Code/Weapons/Bases/MeleWeapon.cs: 1 number(s) in code multiplied through to metres
- Code/Weapons/Bases/ThrowableWeapon.cs: 2 number(s) in code multiplied through to metres
- Code/Weapons/Carriable.cs: 3 number(s) in code multiplied through to metres
- Code/Weapons/ViewModel/GunViewModel.cs: 3 number(s) in code multiplied through to metres
- Code/Weapons/ViewModel/ViewModel.cs: 3 number(s) in code multiplied through to metres
- Libraries/evilinc.modeliconeditor/Editor/IconEditor.cs: 3 number(s) in code multiplied through to metres
- units: this project declares its own 'Extents', which is also what this engine calls something it measures - reads of it were taken to be the game's own units unless the receiver said which it was
- units: this project declares its own 'Pos', which is also what this engine calls something it measures - reads of it were taken to be the game's own units unless the receiver said which it was
- units: this project declares its own 'Position', which is also what this engine calls something it measures - reads of it were taken to be the game's own units unless the receiver said which it was
- units: this project declares its own 'Radius', which is also what this engine calls something it measures - reads of it were taken to be the game's own units unless the receiver said which it was
- units: this project declares its own 'Velocity', which is also what this engine calls something it measures - reads of it were taken to be the game's own units unless the receiver said which it was
- units: Accelerate() is handed a measurement in argument(s) 0 - converted at its callers rather than inside it
- units: ApplyFriction() is handed a measurement in argument(s) 1 - converted at its callers rather than inside it
- units: BuildTrace() is handed a measurement in argument(s) 0, 1 - converted at its callers rather than inside it
- units: IsPointWater() is handed a measurement in argument(s) 0 - converted at its callers rather than inside it
- units: MakeTrace() is handed a measurement in argument(s) 0, 1, 2 - converted at its callers rather than inside it
- units: MoveTo() is handed a measurement in argument(s) 0 - converted at its callers rather than inside it
- units: Ragdoll() is handed a measurement in argument(s) 0, 1 - converted at its callers rather than inside it
- units: TraceDirection() is handed a measurement in argument(s) 0 - converted at its callers rather than inside it
- prefabs/npc/zombie.prefab: 7 measurements from inchs
- prefabs/player.prefab: 222 measurements from inchs
- prefabs/weapons/awp.prefab: 2 measurements from inchs
- prefabs/weapons/beretta.prefab: 4 measurements from inchs
- prefabs/weapons/effects/exp.prefab: 25 measurements from inchs
- prefabs/weapons/effects/granade_throw.prefab: 2 measurements from inchs
- prefabs/weapons/effects/muzzleflash.prefab: 45 measurements from inchs
- prefabs/weapons/effects/muzzlesilenceflash.prefab: 1 measurements from inchs
- prefabs/weapons/effects/shells/5.45.prefab: 8 measurements from inchs
- prefabs/weapons/effects/shells/9mm.prefab: 8 measurements from inchs
- prefabs/weapons/effects/shells/buck.prefab: 8 measurements from inchs
- prefabs/weapons/granade.prefab: 2 measurements from inchs
- prefabs/weapons/knife.prefab: 2 measurements from inchs
- prefabs/weapons/mp7.prefab: 2 measurements from inchs
- prefabs/weapons/reming870.prefab: 2 measurements from inchs
- prefabs/weapons/vss.prefab: 2 measurements from inchs
- scenes/dom.scene: 46 measurements from inchs
- scenes/dom.scene: 12 mesh vertex/vertices from inchs
- scenes/eeffe.scene: 22 measurements from inchs
- scenes/lobby.scene: 4 measurements from inchs
- scenes/weapon_editor.scene: 97 measurements from inchs
- models/citizen/citizen_human_f.ag: 258 state(s), 452 transition(s) and 698 clip(s) came across from their graph, driven by the 76 parameter(s) the game already sets.
- models/citizen/citizen_human_f.ag: 69 bone mask(s) came over with their weights.
- models/citizen/citizen_human_f.ag: what has no counterpart here was passed through - Choice (29), Subtract (15), Solve i k chain (3), Choreo (2), Hit react (1), Cycle control (1). The pose still arrives; what those nodes did to it does not.
- models/citizen/citizen_human_m.ag: 258 state(s), 452 transition(s) and 698 clip(s) came across from their graph, driven by the 76 parameter(s) the game already sets.
- models/citizen/citizen_human_m.ag: 69 bone mask(s) came over with their weights.
- models/citizen/citizen_human_m.ag: what has no counterpart here was passed through - Choice (29), Subtract (15), Solve i k chain (3), Choreo (2), Hit react (1), Cycle control (1). The pose still arrives; what those nodes did to it does not.
- 403 file(s) came from the s&box installation at C:\Program Files (x86)\Steam\steamapps\common\sbox - content the project names and does not contain.
- 67 resource path(s) in 19 file(s) were pointed at the files the port wrote, rather than at the names s&box knew them by.
- scenes/dom.scene: 'Map' had the map 'scenes/dom.prefab', which is a prefab at the top of the scene now, beside the object that named it

# Levels

2 Hammer map(s) came across as prefabs, beside where the `.vmap` was. There is no map format on this side and nothing here compiles one: a level is a prefab, so it drops into whatever scene you want it in, alongside anything else already there, as many times as you like. The brushwork is an editable mesh, not baked geometry, so it can still be cut about.

Where a scene named its level through a `MapInstance`, the level itself is in it now: the component is gone and the prefab stands under the object that carried it, which is where that component put the map. 1 of those.

Anything above that says a map was named and not found is one this could not do: a map package off their site, a level whose file the port never saw, or a scene that took its map from the launch arguments. Those objects are empty now and the level has to be dropped in by hand.

Anything above that says an entity was kept as an empty object is a classname this engine has no component for, and each of those is somebody's afternoon.

# Units

This project measured in inches. This engine measures in metres.

The port converted every number it could name: 20 scene(s) and prefab(s), 2 map(s), and 79 number(s) in code, which were multiplied where they stand so the conversion is in the diff. Deleting one of those expressions and writing the metre value in its place is what finishing that number looks like.

## Left alone (158)

Each of these is a number the port could not prove the meaning of. It is still in the old units if it was a measurement at all - and plenty of them are counts, angles and fractions, which are already right.

- `prefabs/npc/zombie.prefab > zombie` **GeneralGame.Zombie.Damage** — no dimension is written down for this property
  ```
  10
  ```
- `prefabs/npc/zombie.prefab > zombie` **GeneralGame.Zombie.DeleteTime** — no dimension is written down for this property
  ```
  10
  ```
- `prefabs/npc/zombie.prefab > zombie` **GeneralGame.Zombie.DetectRange** — no dimension is written down for this property
  ```
  456
  ```
- `prefabs/npc/zombie.prefab > zombie` **GeneralGame.Zombie.MaxHealth** — no dimension is written down for this property
  ```
  100
  ```
- `prefabs/npc/zombie.prefab > zombie` **GeneralGame.Zombie.VisionRange** — no dimension is written down for this property
  ```
  512
  ```
- `prefabs/player.prefab > player` **GeneralGame.MovementController.AirControl** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/player.prefab > player` **GeneralGame.MovementController.BaseGroundControl** — no dimension is written down for this property
  ```
  4
  ```
- `prefabs/player.prefab > player` **GeneralGame.MovementController.CrouchSpeed** — no dimension is written down for this property
  ```
  90
  ```
- `prefabs/player.prefab > player` **GeneralGame.MovementController.JumpForce** — no dimension is written down for this property
  ```
  350
  ```
- `prefabs/player.prefab > player` **GeneralGame.MovementController.MaxForce** — no dimension is written down for this property
  ```
  50
  ```
- `prefabs/player.prefab > player` **GeneralGame.MovementController.RunSpeed** — no dimension is written down for this property
  ```
  290
  ```
- `prefabs/player.prefab > player` **GeneralGame.MovementController.WalkSpeed** — no dimension is written down for this property
  ```
  160
  ```
- `prefabs/player.prefab > player` **GeneralGame.HealthController.MaxHealth** — no dimension is written down for this property
  ```
  100
  ```
- `prefabs/player.prefab > player` **GeneralGame.CharController.Acceleration** — no dimension is written down for this property
  ```
  10
  ```
- `prefabs/player.prefab > player` **GeneralGame.CharController.Bounciness** — no dimension is written down for this property
  ```
  0.3
  ```
- `prefabs/player.prefab > player` **GeneralGame.CharController.GroundAngle** — no dimension is written down for this property
  ```
  45
  ```
- `prefabs/player.prefab > player` **GeneralGame.CharController.Height** — no dimension is written down for this property
  ```
  64
  ```
- `prefabs/player.prefab > player` **GeneralGame.CharController.Radius** — no dimension is written down for this property
  ```
  16
  ```
- `prefabs/player.prefab > player` **GeneralGame.CharController.StepHeight** — no dimension is written down for this property
  ```
  18
  ```
- `prefabs/player.prefab > Camera` **Rinten.Bloom.Gamma** — no dimension is written down for this property
  ```
  2.2
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.AimAnimData.Angle** — no dimension is written down for this part of a property
  ```
  "0,0,0"
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.AimAnimData.Pos** — no dimension is written down for this part of a property
  ```
  "-2.321,-8.6,0.9990001"
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.AimFOV** — no dimension is written down for this property
  ```
  -30
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.ClipSize** — no dimension is written down for this property
  ```
  8
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.Damage** — no dimension is written down for this property
  ```
  100
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.Force** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.HitFlinch** — no dimension is written down for this property
  ```
  1.25
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.Icon.data.MaxSize** — no dimension is written down for this part of a property
  ```
  4096
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.Icon.data.Padding.Size** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.Icon.data.Padding.EdgeSize** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.Icon.data.TargetColor** — no dimension is written down for this part of a property
  ```
  "0,1,0,1"
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.Recoil** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.RPM** — no dimension is written down for this property
  ```
  500
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.RunAnimData.Angle** — no dimension is written down for this part of a property
  ```
  "16.68,30.02,0"
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.RunAnimData.Pos** — no dimension is written down for this part of a property
  ```
  "2.046,0,1.489"
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.Spread** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.TuckRange** — no dimension is written down for this property
  ```
  30
  ```
- `prefabs/weapons/awp.prefab > awp` **GeneralGame.Gun.ViewPunch** — no dimension is written down for this property
  ```
  5
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.AimAnimData.Angle** — no dimension is written down for this part of a property
  ```
  "-0.16,-0.03,-0.4"
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.AimAnimData.Pos** — no dimension is written down for this part of a property
  ```
  "-1.918,0,0.776"
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.AimFOV** — no dimension is written down for this property
  ```
  -30
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.ClipSize** — no dimension is written down for this property
  ```
  8
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.Damage** — no dimension is written down for this property
  ```
  36
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.Force** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.HitFlinch** — no dimension is written down for this property
  ```
  1.25
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.Icon.data.MaxSize** — no dimension is written down for this part of a property
  ```
  4096
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.Icon.data.Padding.Size** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.Icon.data.Padding.EdgeSize** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.Icon.data.TargetColor** — no dimension is written down for this part of a property
  ```
  "0,1,0,1"
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.Recoil** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.RPM** — no dimension is written down for this property
  ```
  500
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.RunAnimData.Angle** — no dimension is written down for this part of a property
  ```
  "25.51,21.44,-0.7"
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.RunAnimData.Pos** — no dimension is written down for this part of a property
  ```
  "0,0.3079997,3.589"
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.Spread** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.TuckRange** — no dimension is written down for this property
  ```
  30
  ```
- `prefabs/weapons/beretta.prefab > beretta` **GeneralGame.Gun.ViewPunch** — no dimension is written down for this property
  ```
  5
  ```
- `prefabs/weapons/effects/exp.prefab > Smoke` **Rinten.ParticleSpriteRenderer.Pivot** — no dimension is written down for this property
  ```
  "0.5,0.5"
  ```
- `prefabs/weapons/effects/exp.prefab > Smoke` **Rinten.BasicParticleEmiter.Angle** — no dimension is written down for this property
  ```
  "0,0,0"
  ```
- `prefabs/weapons/effects/exp.prefab > Smoke` **Rinten.BasicParticleEmiter.Burst** — no dimension is written down for this property
  ```
  25
  ```
- `prefabs/weapons/effects/exp.prefab > Dust Ring ` **Rinten.ParticleSpriteRenderer.Pivot** — no dimension is written down for this property
  ```
  "0.5,0.5"
  ```
- `prefabs/weapons/effects/exp.prefab > Dust Ring ` **Rinten.BasicParticleEmiter.Angle** — no dimension is written down for this property
  ```
  "0,0,0"
  ```
- `prefabs/weapons/effects/exp.prefab > Dust Ring ` **Rinten.BasicParticleEmiter.Burst** — no dimension is written down for this property
  ```
  20
  ```
- `prefabs/weapons/effects/exp.prefab > Shockwave` **Rinten.ParticleSpriteRenderer.Pivot** — no dimension is written down for this property
  ```
  "0.5,0.5"
  ```
- `prefabs/weapons/effects/exp.prefab > Shockwave` **Rinten.BasicParticleEmiter.Angle** — no dimension is written down for this property
  ```
  "0,0,0"
  ```
- `prefabs/weapons/effects/exp.prefab > Shockwave` **Rinten.BasicParticleEmiter.Burst** — no dimension is written down for this property
  ```
  2
  ```
- `prefabs/weapons/effects/granade_throw.prefab > granade_throw` **GeneralGame.EntThrow.Damage** — no dimension is written down for this property
  ```
  120
  ```
- `prefabs/weapons/effects/granade_throw.prefab > granade_throw` **GeneralGame.EntThrow.range** — no dimension is written down for this property
  ```
  700
  ```
- `prefabs/weapons/effects/muzzleflash.prefab > muzzleflash` **Rinten.ParticleSpriteRenderer.Pivot** — no dimension is written down for this property
  ```
  "0.5,0.5"
  ```
- `prefabs/weapons/effects/muzzleflash.prefab > muzzleflash` **Rinten.BasicParticleEmiter.Angle** — no dimension is written down for this property
  ```
  "0,0,0"
  ```
- `prefabs/weapons/effects/muzzleflash.prefab > Smoke` **Rinten.ParticleSpriteRenderer.Pivot** — no dimension is written down for this property
  ```
  "0.5,0.5"
  ```
- `prefabs/weapons/effects/muzzleflash.prefab > Smoke` **Rinten.BasicParticleEmiter.Angle** — no dimension is written down for this property
  ```
  "0,0,0"
  ```
- `prefabs/weapons/effects/muzzleflash.prefab > Spark` **Rinten.ParticleSpriteRenderer.Pivot** — no dimension is written down for this property
  ```
  "0.5,0.5"
  ```
- `prefabs/weapons/effects/muzzleflash.prefab > Spark` **Rinten.BasicParticleEmiter.Angle** — no dimension is written down for this property
  ```
  "0,0,0"
  ```
- `prefabs/weapons/effects/shells/5.45.prefab > 5.45` **Rinten.BasicParticleEmiter.Angle** — no dimension is written down for this property
  ```
  "-100,0,0"
  ```
- `prefabs/weapons/effects/shells/9mm.prefab > shellparticle` **Rinten.BasicParticleEmiter.Angle** — no dimension is written down for this property
  ```
  "-100,0,0"
  ```
- `prefabs/weapons/effects/shells/buck.prefab > buck` **Rinten.BasicParticleEmiter.Angle** — no dimension is written down for this property
  ```
  "-100,0,0"
  ```
- `prefabs/weapons/granade.prefab > granade` **GeneralGame.ThrowableWeapon.Icon.data.MaxSize** — no dimension is written down for this part of a property
  ```
  4096
  ```
- `prefabs/weapons/granade.prefab > granade` **GeneralGame.ThrowableWeapon.Icon.data.Padding.Size** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/granade.prefab > granade` **GeneralGame.ThrowableWeapon.Icon.data.Padding.EdgeSize** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/granade.prefab > granade` **GeneralGame.ThrowableWeapon.Icon.data.TargetColor** — no dimension is written down for this part of a property
  ```
  "0,1,0,1"
  ```
- `prefabs/weapons/granade.prefab > granade` **GeneralGame.ThrowableWeapon.PrepareTime** — no dimension is written down for this property
  ```
  2
  ```
- `prefabs/weapons/granade.prefab > granade` **GeneralGame.ThrowableWeapon.RunAnimData.Angle** — no dimension is written down for this part of a property
  ```
  "0,0,0"
  ```
- `prefabs/weapons/granade.prefab > granade` **GeneralGame.ThrowableWeapon.RunAnimData.Pos** — no dimension is written down for this part of a property
  ```
  "0,0,0"
  ```
- `prefabs/weapons/granade.prefab > granade` **GeneralGame.ThrowableWeapon.TuckRange** — no dimension is written down for this property
  ```
  30
  ```
- `prefabs/weapons/knife.prefab > knife` **GeneralGame.MeleWeapon.AttackDelay** — no dimension is written down for this property
  ```
  0.2
  ```
- `prefabs/weapons/knife.prefab > knife` **GeneralGame.MeleWeapon.Damage** — no dimension is written down for this property
  ```
  150
  ```
- `prefabs/weapons/knife.prefab > knife` **GeneralGame.MeleWeapon.Icon.data.MaxSize** — no dimension is written down for this part of a property
  ```
  4096
  ```
- `prefabs/weapons/knife.prefab > knife` **GeneralGame.MeleWeapon.Icon.data.Padding.Size** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/knife.prefab > knife` **GeneralGame.MeleWeapon.Icon.data.Padding.EdgeSize** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/knife.prefab > knife` **GeneralGame.MeleWeapon.Icon.data.TargetColor** — no dimension is written down for this part of a property
  ```
  "0,1,0,1"
  ```
- `prefabs/weapons/knife.prefab > knife` **GeneralGame.MeleWeapon.RunAnimData.Angle** — no dimension is written down for this part of a property
  ```
  "0,0,0"
  ```
- `prefabs/weapons/knife.prefab > knife` **GeneralGame.MeleWeapon.RunAnimData.Pos** — no dimension is written down for this part of a property
  ```
  "0,0,0"
  ```
- `prefabs/weapons/knife.prefab > knife` **GeneralGame.MeleWeapon.TuckRange** — no dimension is written down for this property
  ```
  30
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.AimAnimData.Angle** — no dimension is written down for this part of a property
  ```
  "0,0,0"
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.AimAnimData.Pos** — no dimension is written down for this part of a property
  ```
  "-3.101,-7,0.566"
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.AimFOV** — no dimension is written down for this property
  ```
  -20
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.ClipSize** — no dimension is written down for this property
  ```
  20
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.Damage** — no dimension is written down for this property
  ```
  8
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.Force** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.HitFlinch** — no dimension is written down for this property
  ```
  1.25
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.Icon.data.MaxSize** — no dimension is written down for this part of a property
  ```
  4096
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.Icon.data.Padding.Size** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.Icon.data.Padding.EdgeSize** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.Icon.data.TargetColor** — no dimension is written down for this part of a property
  ```
  "0,1,0,1"
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.Recoil** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.RPM** — no dimension is written down for this property
  ```
  900
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.RunAnimData.Angle** — no dimension is written down for this part of a property
  ```
  "25.51,21.44,-0.7"
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.RunAnimData.Pos** — no dimension is written down for this part of a property
  ```
  "0,0.3079997,3.589"
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.Spread** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.TuckRange** — no dimension is written down for this property
  ```
  30
  ```
- `prefabs/weapons/mp7.prefab > mp7` **GeneralGame.Gun.ViewPunch** — no dimension is written down for this property
  ```
  5
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.AimAnimData.Angle** — no dimension is written down for this part of a property
  ```
  "-0.84,-0.02,0"
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.AimAnimData.Pos** — no dimension is written down for this part of a property
  ```
  "-2.647,0,0.103"
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.AimFOV** — no dimension is written down for this property
  ```
  -30
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.Bullets** — no dimension is written down for this property
  ```
  12
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.ClipSize** — no dimension is written down for this property
  ```
  6
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.Damage** — no dimension is written down for this property
  ```
  14
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.Force** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.HitFlinch** — no dimension is written down for this property
  ```
  1.25
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.Icon.data.MaxSize** — no dimension is written down for this part of a property
  ```
  4096
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.Icon.data.Padding.Size** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.Icon.data.Padding.EdgeSize** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.Icon.data.TargetColor** — no dimension is written down for this part of a property
  ```
  "0,1,0,1"
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.Recoil** — no dimension is written down for this property
  ```
  0.2
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.RPM** — no dimension is written down for this property
  ```
  600
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.RunAnimData.Angle** — no dimension is written down for this part of a property
  ```
  "13.41,19.57,0"
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.RunAnimData.Pos** — no dimension is written down for this part of a property
  ```
  "0.102,0,0"
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.Spread** — no dimension is written down for this property
  ```
  0.2
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.TuckRange** — no dimension is written down for this property
  ```
  30
  ```
- `prefabs/weapons/reming870.prefab > reming870` **GeneralGame.Gun.ViewPunch** — no dimension is written down for this property
  ```
  5
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.AimAnimData.Angle** — no dimension is written down for this part of a property
  ```
  "0.05,-0.01,0.5"
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.AimAnimData.Pos** — no dimension is written down for this part of a property
  ```
  "-2.226,0,1.27"
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.AimFOV** — no dimension is written down for this property
  ```
  -30
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.ClipSize** — no dimension is written down for this property
  ```
  30
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.Damage** — no dimension is written down for this property
  ```
  15
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.Force** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.HitFlinch** — no dimension is written down for this property
  ```
  1.25
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.Icon.data.MaxSize** — no dimension is written down for this part of a property
  ```
  4096
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.Icon.data.Padding.Size** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.Icon.data.Padding.EdgeSize** — no dimension is written down for this part of a property
  ```
  "0,0"
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.Icon.data.TargetColor** — no dimension is written down for this part of a property
  ```
  "0,1,0,1"
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.Recoil** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.RPM** — no dimension is written down for this property
  ```
  600
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.RunAnimData.Angle** — no dimension is written down for this part of a property
  ```
  "11.89,19.64,0"
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.RunAnimData.Pos** — no dimension is written down for this part of a property
  ```
  "-0.199,0,0.38"
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.Spread** — no dimension is written down for this property
  ```
  0.1
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.TuckRange** — no dimension is written down for this property
  ```
  30
  ```
- `prefabs/weapons/vss.prefab > vss` **GeneralGame.Gun.ViewPunch** — no dimension is written down for this property
  ```
  5
  ```
- `scenes/dom.scene > spawner` **GeneralGame.ZombieSpawner.Delay** — no dimension is written down for this property
  ```
  50
  ```
- `scenes/dom.scene > spawner` **GeneralGame.ZombieSpawner.Delay** — no dimension is written down for this property
  ```
  50
  ```
- `scenes/dom.scene > spawner` **GeneralGame.ZombieSpawner.Delay** — no dimension is written down for this property
  ```
  20
  ```
- `scenes/dom.scene > Door` **GeneralGame.DoorComponent.Angle** — no dimension is written down for this property
  ```
  -100
  ```
- `scenes/dom.scene > Door` **GeneralGame.DoorComponent.OpenTime** — no dimension is written down for this property
  ```
  0.25
  ```
- `scenes/dom.scene > Door` **GeneralGame.DoorComponent.Pivot** — no dimension is written down for this property
  ```
  "0,0,0"
  ```
- `scenes/dom.scene > Door` **GeneralGame.DoorComponent.Angle** — no dimension is written down for this property
  ```
  -100
  ```
- `scenes/dom.scene > Door` **GeneralGame.DoorComponent.OpenTime** — no dimension is written down for this property
  ```
  0.25
  ```
- `scenes/dom.scene > Door` **GeneralGame.DoorComponent.Pivot** — no dimension is written down for this property
  ```
  "0,0,0"
  ```
- `scenes/weapon_editor.scene > Camera` **GeneralGame.UI.GunEditor.CurFov** — no dimension is written down for this property
  ```
  75
  ```
