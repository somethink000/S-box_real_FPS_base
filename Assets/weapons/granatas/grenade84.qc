$modelname "weapons\arc9\c_uplp_grenade_m84.mdl"
 
$bodygroup "knif" { // 0
	studio "m84.smd"
	blank
}

$cdmaterials "models\weapons\arc9\uplp"

// $attachment "1" "barrel" 0 0 0 rotate -90 0 0
// $attachment "2" "bolt" 0 0 1.5 rotate -10 140 180
$attachment "3" "Camera" 0 0 0 rotate -90 0 0
$attachment "4" "handle" 0.00 0.00 0.00 rotate 180 180 180
// $attachment "4" "mag" 0 0 0 rotate -90 0 0
// $attachment "5" "fakemag" 0 0 0 rotate -90 0 0
 
$surfaceprop "default"
 
$contents "solid"
 
$illumposition 0 0 0

$bbox 0 0 0 0 0 0
 
$cbox 0 0 0 0 0 0


$sequence "idle"{
	"84anims\idle.smd" 
	fps 25
	loop
	fadein 0.2
	fadeout 0.2
}


$sequence "draw" {
	"84anims\draw.smd"
	fps 25
}
$sequence "holster" {
	"84anims\holster.smd"
	fps 25
}

$sequence "quicknade" {
	"84anims\quicknade.smd"
	fps 25
	
}

$sequence "pullpin" {
	"84anims\pullpin.smd"
	fps 25
}

$sequence "throw" {
	"84anims\throw.smd"
	fps 25
	fadein 0.1
	fadeout 0.2
}
$sequence "throw_under" {
	"84anims\throw_under.smd"
	fps 25
	fadein 0.1
	fadeout 0.2
}
$sequence "throw_idle"{
	"84anims\throw_idle.smd" 
	fps 25
	loop
	fadein 0.2
	fadeout 0.2
}

$sequence "sprint" {
	"84anims\sprint.smd"
	fps 28
	fadein 0.35
	fadeout 0.35
	frame 0 31
	loop
}

$sequence "sprint_in" {
	"84anims\sprint_in.smd"
	fps 28
	fadein 0.3
	fadeout 0.3
}
$sequence "sprint_out" {
	"84anims\sprint_out.smd"
	fps 28
	fadein 0.3
	fadeout 0.3
}

$sequence "inspect" {
	"84anims\inspect.smd"
	fps 25
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
// $bonemerge "charmbone"