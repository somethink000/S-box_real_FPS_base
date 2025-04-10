$modelname "weapons\arc9\c_uplp_870.mdl"
 
$bodygroup "rec" { // 0
	studio "870.smd" // 0
	studio "870 gold.smd" // 0
}
$bodygroup "br" { // 2
	studio "870 barrel 7.smd" // 0
	studio "870 barrel 9.smd" // 1
	studio "870 barrel less mag 5.smd" // 1
	studio "870 barrel longass 5.smd" // 1
	studio "870 barrel police 5.smd" // 1
	studio "870 barrel sawed 5.smd" // 1
	studio "870 barrel serbu 3.smd" // 1
	studio "870 barrel swag 9.smd" // 1
	studio "870 barrel usmc 8.smd" // 1
}
$bodygroup "pump" { // 2
	studio "870 pump wood.smd" // 0
	studio "870 pump wood old.smd" // 0
	studio "870 pump.smd" // 0
	studio "870 pump rail.smd" // 0
	studio "870 pump flashlight.smd" // 1
	studio "870 pump magpul.smd" // 1
	blank
}

$bodygroup "stock" { // 2
	studio "870 stock wood.smd" // 0
	studio "870 stock.smd" // 0
	studio "870 stock magpul.smd" // 1
	studio "870 stock buffer.smd" // 1
	studio "870 stock grip.smd" // 1
	studio "870 stock short.smd" // 1
	studio "870 stock short wood.smd" // 1
	studio "870 stock fold.smd" // 1
	studio "870 stock fold f.smd" // 1
}
$bodygroup "rs" { // 4
	blank
	studio "870 rs rail sight.smd" // 0
	studio "870 rs rail.smd" // 0
}
$bodygroup "shellholder" { // 4
	blank
	studio "870 shell holder r.smd" // 0
	studio "870 shell holder b.smd" // 0
	studio "870 shell holder bl.smd" // 0
	studio "870 shell holder g.smd" // 0
	studio "870 shell holder o.smd" // 0
	studio "870 shell holder y.smd" // 0
}
$bodygroup "mags" { // 4
	blank
	studio "870 mag 6.smd" // 0
}

// $bodygroup "rnd" { // 6
// 	studio "ammo r.smd" // 0
// 	studio "ammo b.smd" // 1
// 	studio "ammo bl.smd" // 2
// 	studio "ammo g.smd" // 3
// 	studio "ammo o.smd" // 4
// 	studio "ammo y.smd" // 5
// 	blank  // 6
// }

$bodygroup "rnd" { // 6
	studio "ammos 590 r.smd" // 0
	studio "ammos 590 b.smd" // 0
	studio "ammos 590 bl.smd" // 0
	studio "ammos 590 g.smd" // 0
	studio "ammos 590 o.smd" // 0
	studio "ammos 590 y.smd" // 0

	studio "ammos 870m r.smd" // 1
	studio "ammos 870m b.smd" // 1
	studio "ammos 870m bl.smd" // 1
	studio "ammos 870m g.smd" // 1
	studio "ammos 870m o.smd" // 1
	studio "ammos 870m y.smd" // 1
	blank  // 6
}

$cdmaterials "models\weapons\arc9\uplp"

$attachment "1" "barrel" 0 0 0 rotate -90 0 0
$attachment "2" "bolt" 0 0 3.5 rotate -10 140 180
$attachment "3" "Camera" 0 0 0 rotate -90 0 0
// $attachment "4" "fakemag" 0 0 0 rotate -90 0 0
$attachment "4" "mag" 0 0 0 rotate -90 0 0
 
$surfaceprop "default"
 
$contents "solid"
 
$illumposition 0 0 0

$bbox 0 0 0 0 0 0
 
$cbox 0 0 0 0 0 0


// $bonemerge "hookontop"
$bonemerge "catch"
$bonemerge "bullet1"
$bonemerge "bullet2"
$bonemerge "bullet1mag"
$bonemerge "bullet2mag"
$bonemerge "bolt"
$bonemerge "bulletdoor"
$bonemerge "lifter"
$bonemerge "tac"
$bonemerge "rhandikbleh"

$ikchain "rhand" "ValveBiped.Bip01_R_Hand" knee 0.833 0.553 0

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


$sequence "look" {
	"anims\inspect870.smd"
	snap
	fps 30
	ikrule "rhand" touch "rhandikbleh" usesource
}


$includemodel "weapons/arc9/c_uplp_590.mdl"