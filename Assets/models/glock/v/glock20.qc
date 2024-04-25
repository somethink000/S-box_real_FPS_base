// Created by Crowbar 0.74

$modelname "weapons\view\pistols\glock20.mdl"

$bodygroup "weapon"
{
	studio "glock20_glock.smd"
}
$bodygroup "Hands"
{
	studio "glock20_ref_hand_red.smd"
	studio "glock20_ref_hand_blue.smd"
}
$bodygroup "Barrel"
{
	blank
	studio "glock20_ref_suppressor.smd"
}
$bodygroup "Sights"
{
	blank
	studio "glock20_ref_tritium.smd"
}


$surfaceprop "default"

$contents "solid"

$illumposition -5.487 -5.297 -6.32

$cdmaterials "models\weapons\view\pistols\glock20\"
$cdmaterials "models\weapons\view\accessories\"
$cdmaterials "models\weapons\view\hands\"
$cdmaterials "models\weapons\shells\"

$attachment "muzzle" "muzzle_flash" 0 0 0 rotate 0 0 0
$attachment "ejector" "shell_ejection" 0 0 0 rotate 0 0 0
$attachment "muzzle_suppressor" "muzzle_flash_sup" 0 0 0 rotate 0 0 0
$attachment "Sight1" "glock_main" 1 -1.43 0.57 rotate 0.15 0 0

$cbox 0 0 0 0 0 0

$bbox -6.335 -16.886 -12.175 16.928 5.912 -0.466

$definebone "glock_main" "" -1.420426 -10.135149 -1.581428 0 0 0 0 0 0 0 0 0
$definebone "glock_magazine_release" "glock_main" 0.371365 0.268686 -0.462893 0 0 0 0 0 0 0 0 0
$definebone "glock_slide" "glock_main" 0 -0.685104 0.700115 0 0 0 0 0 0 0 0 0
$definebone "glock_bullet_indicator" "glock_slide" -0.174098 0.656852 0.072252 0 -6.271883 0 0 0 0 0 0 0
$definebone "glock_barrel" "glock_main" 0 -0.869208 0.294358 0 0 0 0 0 0 0 0 0
$definebone "muzzle_flash" "glock_barrel" -0.00672 -2.145278 0.41032 -0.000036 -90.000016 90.000016 0 0 0 0 0 0
$definebone "glock_trigger" "glock_main" 0 -0.474923 0.181298 0 0 0 0 0 0 0 0 0
$definebone "glock_slidelock" "glock_main" 0 -0.42353 0.335186 0 0 0 0 0 0 0 0 0
$definebone "shell_ejection" "glock_main" -0.141148 0.485793 0.859828 -23.732942 -175.045263 88.00168 0 0 0 0 0 0
$definebone "glock_magazine" "" -1.420426 -9.364954 -2.3526 0 0 0 0 0 0 0 0 0
$definebone "glock_bullet" "glock_magazine" -0.002597 -0.372232 1.24289 0 0 0 0 0 0 0 0 0
$definebone "glock_magazine_2" "" -2.663103 2.004578 -10.703464 0 0 0 0 0 0 0 0 0
$definebone "glock_bullet_2" "glock_magazine_2" -0.008425 -0.372233 1.24289 0 0 0 0 0 0 0 0 0
$definebone "Right Hand" "" -5.81565 -10.090681 -3.135012 -0.642114 89.643084 -179.659739 0 0 0 0 0 0
$definebone "Right Lower Arm2" "Right Hand" 0.001853 -0.011043 0.000007 -2.096854 -2.794373 0 0 0 0 0 0 0
$definebone "Right Demonstratus Phalanges1" "Right Hand" -2.693112 0.598938 -0.105602 -5.218156 175.165802 -88.556422 0 0 0 0 0 0
$definebone "Right Demonstratus Phalange2" "Right Demonstratus Phalanges1" 0.668916 -0.00023 0.000988 0.898168 0.209358 0.603439 0 0 0 0 0 0
$definebone "Right Demonstratus Phalange3" "Right Demonstratus Phalange2" 0.620677 -0.000352 0.000184 -0.267973 -0.515147 0.367323 0 0 0 0 0 0
$definebone "Right Impudicus Phalanges1" "Right Hand" -2.735406 0.046611 -0.154424 -5.927536 -179.679792 -88.760159 0 0 0 0 0 0
$definebone "Right Impudicus Phalanges2" "Right Impudicus Phalanges1" 0.693262 -0.001516 0.000457 0.476529 1.580503 -0.39007 0 0 0 0 0 0
$definebone "Right Impudicus Phalanges3" "Right Impudicus Phalanges2" 0.70867 0 0 -0.273416 -1.9107 -1.257642 0 0 0 0 0 0
$definebone "Right Annularis Phalanges1" "Right Hand" -2.579787 -0.47033 -0.041396 -0.877428 -174.239628 -86.902751 0 0 0 0 0 0
$definebone "Right Annularis Phalanges2" "Right Annularis Phalanges1" 0.698674 -0.00031 -0.00017 -0.230959 0.417801 -1.701055 0 0 0 0 0 0
$definebone "Right Annularis Phalanges3" "Right Annularis Phalanges2" 0.666528 0 0 -1.43566 -0.607335 -0.392877 0 0 0 0 0 0
$definebone "Right Auricularis Phalanges1" "Right Hand" -2.466993 -1.051966 0.081883 -0.942573 -166.863065 -87.70872 0 0 0 0 0 0
$definebone "Right Auricularis Phalanges2" "Right Auricularis Phalanges1" 0.572362 -0.000204 0.001442 3.261791 0.471544 -1.601876 0 0 0 0 0 0
$definebone "Right Auricularis Phalanges3" "Right Auricularis Phalanges2" 0.515585 0 0 -2.448937 -0.143583 -1.142363 0 0 0 0 0 0
$definebone "Right Polex Metacarpal" "Right Hand" -0.205823 0.527907 0.046739 44.660172 -89.716427 -89.094306 0 0 0 0 0 0
$definebone "Right Polex Phalange1" "Right Polex Metacarpal" -0.218396 0.054786 -0.682163 48.019181 -165.497068 114.93188 0 0 0 0 0 0
$definebone "Right Polex Phalange2" "Right Polex Phalange1" 0.873055 0.002833 -0.000315 -0.371852 -3.348995 -2.481195 0 0 0 0 0 0
$definebone "Right Polex Phalange3" "Right Polex Phalange2" 0.7206 -0.000998 -0.002572 -6.876986 2.674682 5.135761 0 0 0 0 0 0
$definebone "Left Hand" "" 1.260293 -10.043264 -3.142162 0.30934 -89.581838 0.200764 0 0 0 0 0 0
$definebone "Left Lower Arm 2" "Left Hand" -0.001849 0.00067 0.000001 4.118822 179.118528 0 0 0 0 0 0 0
$definebone "Left Demonstratus Phalanges1" "Left Hand" 2.65317 -0.698475 0.113861 4.564182 -5.408722 89.90298 0 0 0 0 0 0
$definebone "Left Demonstratus Phalanges2" "Left Demonstratus Phalanges1" 0.704213 -0.000156 -0.001868 -1.523781 0.127254 -0.679012 0 0 0 0 0 0
$definebone "Left Demonstratus Phalanges3" "Left Demonstratus Phalanges2" 0.621943 0 0 1.906287 -0.394882 1.217192 0 0 0 0 0 0
$definebone "Left Impudicus Phalanges1" "Left Hand" 2.69682 -0.150661 0.155077 4.614087 0.186154 89.226887 0 0 0 0 0 0
$definebone "Left Impudicus Phalanges2" "Left Impudicus Phalanges1" 0.759131 -0.000239 0.000019 0.034721 0.450803 0.871927 0 0 0 0 0 0
$definebone "Left Impudicus Phalanges3" "Left Impudicus Phalanges2" 0.692899 -0.000001 0 0 -1.665933 0 0 0 0 0 0 0
$definebone "Left Annularis Phalanges1" "Left Hand" 2.642577 0.371989 0.042955 3.893362 6.235328 89.042915 0 0 0 0 0 0
$definebone "Left Annularis Phalanges2" "Left Annularis Phalanges1" 0.678051 0.000483 -0.001802 -2.11496 -0.569692 1.356019 0 0 0 0 0 0
$definebone "Left Annularis Phalanges3" "Left Annularis Phalanges2" 0.668665 0 0 0.732125 -0.309054 -1.104319 0 0 0 0 0 0
$definebone "Left Auricularis Phalanges1" "Left Hand" 2.452636 0.938823 -0.082469 -0.668413 10.637536 89.485751 0 0 0 0 0 0
$definebone "Left Auricularis Phalanges2" "Left Auricularis Phalanges1" 0.601525 0.000082 -0.001163 -0.046409 -1.102142 0.765414 0 0 0 0 0 0
$definebone "Left Auricularis Phalanges3" "Left Auricularis Phalanges2" 0.550749 0 0 -0.317591 1.759152 -0.401644 0 0 0 0 0 0
$definebone "Left Polex Metacarpal" "Left Hand" 0.219846 -0.599415 -0.044417 44.800645 -90.10848 90.437927 0 0 0 0 0 0
$definebone "Left Polex Phalange1" "Left Polex Metacarpal" 0.322868 0.125605 -0.694551 51.101139 -11.870826 77.08643 0 0 0 0 0 0
$definebone "Left Polex Phalange2" "Left Polex Phalange1" 0.868582 -0.002156 -0.000869 -1.638602 4.069028 -2.680982 0 0 0 0 0 0
$definebone "Left Polex Phalange3" "Left Polex Phalange2" 0.674675 0.000336 0.000567 2.28186 -1.370177 -5.014009 0 0 0 0 0 0
$definebone "Right shoulder" "" -5.758168 7.237389 -3.577589 0.431323 -1.66851 179.738982 0 0 0 0 0 0
$definebone "Right Upper Arm" "Right shoulder" 0 0 0 -1.190663 -89.788103 0.003667 0 0 0 0 0 0
$definebone "Right Lower Arm" "Right Upper Arm" -9.996876 0 0 -0.054718 -3.963264 -0.000172 0 0 0 0 0 0
$definebone "Gemtech" "" -1.432598 -14.959097 -0.897061 0 0 0 0 0 0 0 0 0
$definebone "muzzle_flash_sup" "Gemtech" 0.005452 -1.926493 0.020311 -0.000036 -90.000016 90.000016 0 0 0 0 0 0


$sectionframes 30 120

$sequence "idle" {
	"glock20_anims\idle.smd"
	activity "ACT_VM_IDLE" 1
	fadein 0.2
	fadeout 0.2
	snap
	fps 31
	loop
}

$sequence "idle_empty" {
	"glock20_anims\idle_empty.smd"
	activity "ACT_VM_IDLE_EMPTY" 1
	fadein 0.2
	fadeout 0.2
	snap
	fps 31
	loop
}

$sequence "idle_iron" {
	"glock20_anims\idle_iron.smd"
	activity "ACT_VM_IDLE_SCOPED" 1
	fadein 0.2
	fadeout 0.2
	fps 30
	loop
}

$sequence "idle_iron_empty" {
	"glock20_anims\idle_iron_empty.smd"
	activity "ACT_VM_IDLE_EMPTY_SCOPED" 1
	fadein 0.2
	fadeout 0.2
	fps 30
	loop
}

$sequence "draw" {
	"glock20_anims\draw.smd"
	activity "ACT_VM_DRAW" 1
	{ event AE_CL_PLAYSOUND 0 "Weapon.Deploy" }
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "draw_empty" {
	"glock20_anims\draw_empty.smd"
	activity "ACT_VM_DRAW_EMPTY" 1
	{ event AE_CL_PLAYSOUND 0 "Weapon.Deploy" }
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "holster" {
	"glock20_anims\holster.smd"
	activity "ACT_VM_HOLSTER" 1
	{ event AE_CL_PLAYSOUND 0 "Weapon.Holster" }
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "holster_empty" {
	"glock20_anims\holster_empty.smd"
	activity "ACT_VM_HOLSTER_EMPTY" 1
	{ event AE_CL_PLAYSOUND 0 "Weapon.Holster" }
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "fire_1" {
	"glock20_anims\fire_1.smd"
	activity "ACT_VM_PRIMARYATTACK" 1
	{ event AE_FAS_MUZZLE_EFFECT 0 "Muzzleflash_G20_View" }
	{ event AE_FAS_EJECT_BRASS 1 "" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 30
}

$sequence "fire_2" {
	"glock20_anims\fire_2.smd"
	activity "ACT_VM_PRIMARYATTACK" 1
	{ event AE_FAS_MUZZLE_EFFECT 0 "Muzzleflash_G20_View" }
	{ event AE_FAS_EJECT_BRASS 1 "" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 30
}

$sequence "fire_last" {
	"glock20_anims\fire_last.smd"
	activity "ACT_VM_FIRE_LAST" 1
	{ event AE_FAS_MUZZLE_EFFECT 0 "Muzzleflash_G20_View" }
	{ event AE_FAS_EJECT_BRASS 1 "" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 30
}

$sequence "iron_fire_last" {
	"glock20_anims\iron_fire_last.smd"
	activity "ACT_VM_FIRE_LAST_SCOPED" 1
	{ event AE_FAS_MUZZLE_EFFECT 0 "Muzzleflash_G20_View" }
	{ event AE_FAS_EJECT_BRASS 1 "" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 30
}

$sequence "iron_fire" {
	"glock20_anims\iron_fire.smd"
	activity "ACT_VM_PRIMARYATTACK_SCOPED" 1
	{ event AE_FAS_MUZZLE_EFFECT 0 "Muzzleflash_G20_View" }
	{ event AE_FAS_EJECT_BRASS 1 "" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 30
}

$sequence "reload" {
	"glock20_anims\reload.smd"
	activity "ACT_VM_RELOAD" 1
	{ event 5004 0 "Generic.Cloth_Movement" }
	{ event 5004 9 "Weapon_Misc.Switch" }
	{ event 5004 11 "Weapon_Glock20.MagOut" }
	{ event 5004 12 "Generic.Cloth_Fast" }
	{ event 5004 40 "Generic.Magpouch" }
	{ event 5004 56 "Weapon_Glock20.MagIn" }
	{ event 5004 57 "Generic.Cloth_Movement" }
	{ event 3018 56 "" }
	fadein 0.2
	fadeout 0.2
	fps 32.2
}

$sequence "reload_empty" {
	"glock20_anims\reload_empty.smd"
	activity "ACT_VM_RELOAD_EMPTY" 1
	{ event 5004 0 "Generic.Cloth_Movement" }
	{ event 5004 13 "Weapon_Glock20.MagOutEmpty" }
	{ event 5004 14 "Generic.Cloth_Fast" }
	{ event 5004 27 "Generic.Magpouch" }
	{ event 5004 44 "Weapon_Glock20.MagIn" }
	{ event 5004 45 "Generic.Cloth_Movement" }
	{ event 5004 58 "Weapon_Glock20.SlideForward" }
	{ event 3018 44 "" }
	fadein 0.2
	fadeout 0.2
	fps 31.5
}

$sequence "reload_nomen" {
	"glock20_anims\reload_nomen.smd"
	activity "ACT_VM_RELOAD_NOMEN" 1
	{ event 5004 0 "Generic.Cloth_Movement" }
	{ event 5004 11 "Generic.Magpouch" }
	{ event 5004 23 "Weapon_Misc.Switch" }
	{ event 5004 23 "Weapon_Glock20.MagOutEmpty" }
	{ event 5004 38 "Weapon_Glock20.MagIn" }
	{ event 5004 42 "Generic.Cloth_Fast" }
	{ event 5004 55 "Generic.Cloth_Movement" }
	{ event 3018 40 "" }
	fadein 0.2
	fadeout 0.2
	fps 37
}

$sequence "reload_empty_nomen" {
	"glock20_anims\reload_empty_nomen.smd"
	activity "ACT_VM_RELOAD_EMPTY_NOMEN" 1
	{ event 5004 0 "Generic.Cloth_Movement" }
	{ event 5004 11 "Generic.Magpouch" }
	{ event 5004 23 "Weapon_Misc.Switch" }
	{ event 5004 23 "Weapon_Glock20.MagOutEmpty" }
	{ event 5004 38 "Weapon_Glock20.MagIn" }
	{ event 5004 46 "Weapon_Glock20.SlideForward" }
	{ event 5004 55 "Generic.Cloth_Movement" }
	{ event 3018 40 "" }
	fadein 0.2
	fadeout 0.2
	fps 37
}
