$modelname "weapons\arc9\c_uplp_awp.mdl"
 
$bodygroup "awoopa" {
	studio "body awm.smd"
	studio "body awp.smd"
}   
$bodygroup "stock" {
	studio "stock awp.smd"
	studio "stock awm.smd"
	studio "stock tube.smd"
	studio "stock at.smd"
	studio "stock atx.smd"
	studio "stock atx BLUE.smd"
	studio "stock atx GRAY.smd"
	studio "stock atx GREEN.smd"
	studio "stock atx ORANGE.smd"
	studio "stock atx PURPLE.smd"
	studio "stock atx WHITE.smd"
}
$bodygroup "barrel" {
	studio "barrel awp.smd"
	studio "barrel long.smd"
	studio "barrel short.smd"
	studio "barrel aws.smd"
}
$bodygroup "fs" {
	blank
	studio "fs mid.smd"
	studio "fs long.smd"
	studio "fs short.smd"
}
$bodygroup "mag" {
	studio "mag awm.smd"
	studio "mag awp.smd"
	studio "mag awp big.smd"
	studio "mag awp tac.smd"

	studio "mag awp IN ATX.smd"
	studio "mag awp big IN ATX.smd"
	studio "mag awp tac IN ATX.smd"
} 
$bodygroup "bipod" {
	blank
	studio "bipod fold.smd"
	studio "bipod unfold.smd"
	studio "hbipod fold.smd"
	studio "hbipod unfold.smd"
}
$bodygroup "mount" {
	blank
	studio "mount.smd"
	studio "mount atx.smd"
}
$bodygroup "rs" {
	blank
	studio "rs.smd"
	studio "rs down.smd"
}

$cdmaterials "models\weapons\arc9\uplp"

$attachment "1" "body" 0.00 -0.60 30.00 rotate -90 -0 0
$attachment "2" "body" 0.00 -0.60 2.50 rotate -10 140 180
$attachment "3" "Camera" 0 0 0 rotate -90 0 0
$attachment "4" "mag" 0 0 0 rotate -90 0 0

$attachment "5" "body" 0.00 -0.60 34.00 rotate -90 -0 0
$attachment "6" "body" 0.00 -0.60 23.00 rotate -90 -0 0
$attachment "7" "body" 0.00 -0.60 27.50 rotate -90 -0 0
 
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
	fps 26
	"ACT_VM_PRIMARYATTACK" 1
 	blend "sights" 0 1
	fadein 0.02
	fadeout 0.08
}
$sequence "ready" {
	"anims\ready.smd"
	"anims\ready_iron.smd"
 	blend "sights" 0 1
	fps 30
	"ACT_VM_DRAW" 1
	snap
}



$sequence "draw" {
	"anims\draw.smd"
	fps 24
	"ACT_VM_DRAW" 1
	snap
}
$sequence "draw_empty" {
	"anims\draw_empty.smd"
	fps 24
	"ACT_VM_DRAW" 1
	snap
}
$sequence "holster" {
	"anims\holster.smd"
	fps 20
	"ACT_VM_DRAW" 1
	snap
}
$sequence "holster_empty" {
	"anims\holster_empty.smd"
	fps 20
	"ACT_VM_DRAW" 1
	snap
}



$sequence "modeswitch" {
	"anims\switch.smd"
	"anims\switch_iron.smd"
	blend "sights" 0 1
	fps 30
	snap
}

$sequence "modeswitch_empty" {
	"anims\switch_empty.smd"
	"anims\switch_empty_iron.smd"
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
	"anims\reload_long.smd"
	"anims\reload_long_iron.smd"
	blend "sights" 0 1
	fps 30
	"ACT_VM_RELOAD" 1
	snap
}

$sequence "reload_empty_ext" {
	"anims\reload_empty_long.smd"
	"anims\reload_empty_long_iron.smd"
	blend "sights" 0 1
	fps 30
	"ACT_VM_RELOAD" 1
	snap
}

$sequence "inspect" {
	"anims\inspect.smd"
	fps 30
}
$sequence "inspect_empty" {
	"anims\inspect_empty.smd"
	fps 30
}

$sequence "inspect_xstock" {
	"anims\inspect_xstock.smd"
	fps 30
}
$sequence "inspect_empty_xstock" {
	"anims\inspect_xstock_empty.smd"
	fps 30
}

$sequence "cycle" {
	"anims\cycle.smd"
	"anims\cycle_iron.smd"
	blend "sights" 0 1
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
