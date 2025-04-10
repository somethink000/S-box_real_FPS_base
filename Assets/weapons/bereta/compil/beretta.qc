$modelname "weapons\arc9\c_uplp_beretta.mdl"
 
$bodygroup "body" {
	studio "92fs.smd"
	studio "93r.smd"
	studio "9a3.smd"
	studio "9a4 tan.smd"
	studio "92fs sword.smd"
	studio "93r robo.smd"
}   
$bodygroup "irons" {
	studio "irons def.smd"
	studio "irons 93r.smd"
	studio "irons sword.smd"
	studio "irons rail.smd"
	studio "irons rail 93r.smd"
	blank
}  
$bodygroup "mag" {
	studio "mag.smd"
	studio "mag tan.smd"
	studio "mag ext.smd"
	studio "mag robo.smd"
}

$cdmaterials "models\weapons\arc9\uplp"

$attachment "1" "barrel" 0 0 0 rotate -90 0 0
$attachment "2" "slide" 0 0 0 rotate -10 140 180
$attachment "3" "Camera" 0 0 0 rotate -90 0 0
$attachment "4" "mag" 0 0 0 rotate -90 0 0
// $attachment "5" "fakemag" 0 0 0 rotate -90 0 0
 
$surfaceprop "default"
 
$contents "solid"
 
$illumposition 0 0 0

$bbox 0 0 0 0 0 0
 
$cbox 0 0 0 0 0 0

$poseparameter "sights" 0 1 loop 0

$sequence "idle"{
	"anims\idle.smd" 
	"anims\idle_iron.smd"
	fps 30
 	blend "sights" 0 1
	loop
	fadein 0
	fadeout 0.2
}

$sequence "idle_empty"{
	"anims\idle_empty.smd" 
	"anims\idle_empty_iron.smd"
	fps 30
	"ACT_VM_IDLE" 1
 	blend "sights" 0 1
	loop
	snap
	node "Ready"
	fadein 0
	fadeout 0.2
}

$sequence "fire" {
	"anims\fire.smd"
	"anims\fire_iron.smd"
	"anims\r_fire.smd"
	"anims\r_fire_iron.smd"
	fps 26
	"ACT_VM_PRIMARYATTACK" 1
 	blend "sights" 0 1
 	blend "raffica" 0 1
	fadein 0.02
	fadeout 0.08
}
$sequence "fire_empty" {
	"anims\fire_empty.smd"
	"anims\fire_empty_iron.smd"
	"anims\r_fire_empty.smd"
	"anims\r_fire_empty_iron.smd"
	fps 30
	"ACT_VM_PRIMARYATTACK" 1
 	blend "sights" 0 1
 	blend "raffica" 0 1
	fadein 0.02
	fadeout 0.08
}

$sequence "ready" {
	"anims\ready.smd"
	"anims\ready_iron.smd"
	"anims\r_ready.smd"
	"anims\r_ready.smd"
 	blend "sights" 0 1
 	blend "raffica" 0 1
	fps 30
	"ACT_VM_DRAW" 1
	snap
}



$sequence "draw" {
	"anims\draw.smd"
	fps 30
	"ACT_VM_DRAW" 1
	snap
}
$sequence "draw_empty" {
	"anims\draw_empty.smd"
	fps 30
	"ACT_VM_DRAW" 1
	snap
}
$sequence "holster" {
	"anims\holster.smd"
	fps 24
	"ACT_VM_DRAW" 1
	snap
}
$sequence "holster_empty" {
	"anims\holster_empty.smd"
	fps 30
	"ACT_VM_DRAW" 1
	snap
}



$sequence "modeswitch" {
	"anims\switch.smd"
	"anims\switch_ads.smd"
	blend "sights" 0 1
	fps 30
	snap
}

$sequence "modeswitch_empty" {
	"anims\switch_empty.smd"
	"anims\switch_empty_ads.smd"
	blend "sights" 0 1
	fps 30
	snap
}


$sequence "reload" {
	"anims\reload.smd"
	"anims\reload_iron.smd"
	blend "sights" 0 1
	fps 30
	"ACT_VM_RELOAD" 1
	snap
}

$sequence "reload_empty" {
	"anims\reload_empty.smd"
	"anims\reload_empty_iron.smd"
	blend "sights" 0 1
	fps 30
	"ACT_VM_RELOAD" 1
	snap
}

$sequence "reload_ext" {
	"anims\reload_ext.smd"
	"anims\reload_iron_ext.smd"
	blend "sights" 0 1
	fps 30
	"ACT_VM_RELOAD" 1
	snap
}

$sequence "reload_empty_ext" {
	"anims\reload_empty_ext.smd"
	"anims\reload_empty_iron_ext.smd"
	blend "sights" 0 1
	fps 30
	"ACT_VM_RELOAD" 1
	snap
}

$sequence "inspect" {
	"anims\look.smd"
	"anims\r_look.smd"
 	blend "raffica" 0 1
	// snap
	fps 30
}
$sequence "inspect_empty" {
	"anims\look_empty.smd"
	"anims\r_look_empty.smd"
 	blend "raffica" 0 1
	// snap
	fps 30
}



// robocop




$sequence "rc_idle"{
	"anims\rc_idle.smd" 
	"anims\rc_idle_iron.smd"
	fps 30
 	blend "sights" 0 1
	loop
	fadein 0
	fadeout 0.2
}

$sequence "rc_idle_empty"{
	"anims\rc_idle_empty.smd" 
	"anims\rc_idle_empty_iron.smd"
	fps 30
	"ACT_VM_IDLE" 1
 	blend "sights" 0 1
	loop
	snap
	node "Ready"
	fadein 0
	fadeout 0.2
}

$sequence "rc_fire" {
	"anims\rc_fire.smd"
	"anims\rc_fire_iron.smd"
	fps 26
	"ACT_VM_PRIMARYATTACK" 1
 	blend "sights" 0 1
	fadein 0.02
	fadeout 0.08
}
$sequence "rc_fire_empty" {
	"anims\rc_fire_empty.smd"
	"anims\rc_fire_empty_iron.smd"
	fps 30
	"ACT_VM_PRIMARYATTACK" 1
 	blend "sights" 0 1
	fadein 0.02
	fadeout 0.08
}

$sequence "rc_ready" {
	"anims\rc_ready.smd"
	"anims\rc_ready_iron.smd"
 	blend "sights" 0 1
	fps 30
	"ACT_VM_DRAW" 1
	snap
}



$sequence "rc_draw" {
	"anims\rc_draw.smd"
	fps 30
	"ACT_VM_DRAW" 1
	snap
}
$sequence "rc_draw_empty" {
	"anims\rc_draw_empty.smd"
	fps 30
	"ACT_VM_DRAW" 1
	snap
}
$sequence "rc_holster" {
	"anims\rc_holster.smd"
	fps 24
	"ACT_VM_DRAW" 1
	snap
}
$sequence "rc_holster_empty" {
	"anims\rc_holster_empty.smd"
	fps 30
	"ACT_VM_DRAW" 1
	snap
}



$sequence "rc_modeswitch" {
	"anims\rc_switch.smd"
	"anims\rc_switch_ads.smd"
	blend "sights" 0 1
	fps 30
	snap
}

$sequence "rc_modeswitch_empty" {
	"anims\rc_switch_empty.smd"
	"anims\rc_switch_empty_ads.smd"
	blend "sights" 0 1
	fps 30
	snap
}


$sequence "rc_reload" {
	"anims\rc_reload.smd"
	"anims\rc_reload_iron.smd"
	blend "sights" 0 1
	fps 30
	"ACT_VM_RELOAD" 1
	snap
}

$sequence "rc_reload_empty" {
	"anims\rc_reload_empty.smd"
	"anims\rc_reload_empty_iron.smd"
	blend "sights" 0 1
	fps 30
	"ACT_VM_RELOAD" 1
	snap
}

$sequence "rc_inspect" {
	"anims\rc_look.smd"
	// snap
	fps 30
}
$sequence "rc_inspect_empty" {
	"anims\rc_look_empty.smd"
	// snap
	fps 30
}


$bonemerge "ValveBiped.Bip01_Spine4"
$bonemerge "ValveBiped.Bip01_L_Clavicle"
$bonemerge "ValveBiped.Bip01_L_UpperArm"
$bonemerge "ValveBiped.Bip01_L_Forearm"
$bonemerge "ValveBiped.Bip01_L_Hand"
$bonemerge "ValveBiped.Bip01_L_Finger4"
$bonemerge "ValveBiped.Bip01_L_Finger41"
$bonemerge "ValveBiped.Bip01_L_Finger42"
$bonemerge "ValveBiped.Bip01_L_Finger3"
$bonemerge "ValveBiped.Bip01_L_Finger31"
$bonemerge "ValveBiped.Bip01_L_Finger32"
$bonemerge "ValveBiped.Bip01_L_Finger2"
$bonemerge "ValveBiped.Bip01_L_Finger21"
$bonemerge "ValveBiped.Bip01_L_Finger22"
$bonemerge "ValveBiped.Bip01_L_Finger1"
$bonemerge "ValveBiped.Bip01_L_Finger11"
$bonemerge "ValveBiped.Bip01_L_Finger12"
$bonemerge "ValveBiped.Bip01_L_Finger0"
$bonemerge "ValveBiped.Bip01_L_Finger01"
$bonemerge "ValveBiped.Bip01_L_Finger02"
$bonemerge "ValveBiped.Bip01_R_Clavicle"
$bonemerge "ValveBiped.Bip01_R_UpperArm"
$bonemerge "ValveBiped.Bip01_R_Forearm"
$bonemerge "ValveBiped.Bip01_R_Hand"
$bonemerge "ValveBiped.Bip01_R_Wrist"
$bonemerge "ValveBiped.Bip01_R_Finger4"
$bonemerge "ValveBiped.Bip01_R_Finger41"
$bonemerge "ValveBiped.Bip01_R_Finger42"
$bonemerge "ValveBiped.Bip01_R_Finger3"
$bonemerge "ValveBiped.Bip01_R_Finger31"
$bonemerge "ValveBiped.Bip01_R_Finger32"
$bonemerge "ValveBiped.Bip01_R_Finger2"
$bonemerge "ValveBiped.Bip01_R_Finger21"
$bonemerge "ValveBiped.Bip01_R_Finger22"
$bonemerge "ValveBiped.Bip01_R_Finger1"
$bonemerge "ValveBiped.Bip01_R_Finger11"
$bonemerge "ValveBiped.Bip01_R_Finger12"
$bonemerge "ValveBiped.Bip01_R_Finger0"
$bonemerge "ValveBiped.Bip01_R_Finger01"
$bonemerge "ValveBiped.Bip01_R_Finger02"

$bonemerge "round1"
$bonemerge "round2"
$bonemerge "round3"
// $collisionmodel "col.smd"
// {
// 	$mass 12
// 	$concave
// }