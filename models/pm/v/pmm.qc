// Created by Crowbar 0.74

$modelname "weapons\fas2\view\pistols\pmm.mdl"

$bodygroup "pmm"
{
	studio "pmm_ref.smd"
}
$bodygroup "Hands"
{
	studio "pmm_ref_hand_red.smd"
	studio "pmm_ref_hand_blue.smd"
}


$surfaceprop "default"

$contents "solid"

$illumposition 0 0 0

$cdmaterials "models\weapons\fas2\pistols\pmm\"
$cdmaterials "models\weapons\view\hands\"
$cdmaterials "models\weapons\shells\"

$attachment "muzzle" "Frame" -4.05 -1.57 0 rotate 0 -180 0
$attachment "ejector" "Frame" -1.65 -1.61 0.22 rotate -30 -90 180
$attachment "muzzle_suppressor" "Frame" -4.05 -1.57 0 rotate 0 -180 0
$attachment "Sight1" "Frame" 0.98 -1.9 0.91 rotate 0 0 0

$cbox 0 0 0 0 0 0

$bbox -8.934 -6.374 -7.299 17.676 7.237 -0.827

$definebone "Frame" "" -2.318561 -9.400087 -2.795653 0 89.999982 -89.999982 0 0 0 0 0 0
$definebone "Slide" "" -2.320715 -9.381457 -2.800651 0.000019 90.000016 -90.000016 0 0 0 0 0 0
$definebone "Trigger" "" -2.331338 -9.102321 -1.788354 -35.000215 89.999927 -90.000037 0 0 0 0 0 0
$definebone "Mag Release" "" -2.320373 -9.38695 -2.787323 0 89.999982 -90.000037 0 0 0 0 0 0
$definebone "Magazine" "" -2.309898 -9.3713 -2.788492 0 89.999982 -90.000037 0 0 0 0 0 0
$definebone "Safety" "" -2.361626 -9.464946 -2.808853 0 89.999982 -90.000037 0 0 0 0 0 0
$definebone "Slidestop" "" -2.322412 -9.392885 -2.789151 0 89.999982 -90.000037 0 0 0 0 0 0
$definebone "357SIG Round" "" -2.362714 -9.915875 -1.493519 0 0 89.999982 0 0 0 0 0 0
$definebone "Hammerbone" "" -2.3955 -8.3235 -1.821 -19.62896 -89.999982 -89.999982 0 0 0 0 0 0
$definebone "Right Hand" "" -5.81565 -10.090681 -3.135012 -0.642114 89.643084 -179.659739 0 0 0 0 0 0
$definebone "Right Lower Arm2" "Right Hand" 0.001853 -0.011043 0.000007 -2.096854 -2.794373 0 0 0 0 0 0 0
$definebone "Right Demonstratus Phalanges1" "Right Hand" -2.693112 0.598938 -0.105602 -5.218156 175.165802 -88.556422 0 0 0 0 0 0
$definebone "Right Demonstratus Phalange2" "Right Demonstratus Phalanges1" 0.66892 -0.00023 0.000988 0.898168 0.209354 0.603439 0 0 0 0 0 0
$definebone "Right Demonstratus Phalange3" "Right Demonstratus Phalange2" 0.620681 -0.000352 0.000185 -0.267973 -0.515152 0.367323 0 0 0 0 0 0
$definebone "Right Impudicus Phalanges1" "Right Hand" -2.735406 0.046611 -0.154424 -5.92754 -179.679792 -88.760159 0 0 0 0 0 0
$definebone "Right Impudicus Phalanges2" "Right Impudicus Phalanges1" 0.693262 -0.001516 0.000457 0.476529 1.580499 -0.39007 0 0 0 0 0 0
$definebone "Right Impudicus Phalanges3" "Right Impudicus Phalanges2" 0.708671 0 0 -0.273416 -1.910703 -1.257642 0 0 0 0 0 0
$definebone "Right Annularis Phalanges1" "Right Hand" -2.579781 -0.47033 -0.041396 -0.877428 -174.239628 -86.902758 0 0 0 0 0 0
$definebone "Right Annularis Phalanges2" "Right Annularis Phalanges1" 0.698667 -0.00031 -0.000169 -0.230961 0.417801 -1.701055 0 0 0 0 0 0
$definebone "Right Annularis Phalanges3" "Right Annularis Phalanges2" 0.666528 0 0 -1.435661 -0.607335 -0.392877 0 0 0 0 0 0
$definebone "Right Auricularis Phalanges1" "Right Hand" -2.466993 -1.051966 0.081883 -0.942573 -166.863065 -87.70872 0 0 0 0 0 0
$definebone "Right Auricularis Phalanges2" "Right Auricularis Phalanges1" 0.572366 -0.000204 0.001442 3.261791 0.471544 -1.601876 0 0 0 0 0 0
$definebone "Right Auricularis Phalanges3" "Right Auricularis Phalanges2" 0.515581 0 0 -2.448937 -0.143583 -1.142363 0 0 0 0 0 0
$definebone "Right Polex Metacarpal" "Right Hand" -0.205823 0.527907 0.046739 44.660169 -89.716427 -89.094306 0 0 0 0 0 0
$definebone "Right Polex Phalange1" "Right Polex Metacarpal" -0.218396 0.054786 -0.682163 48.019184 -165.497068 114.93188 0 0 0 0 0 0
$definebone "Right Polex Phalange2" "Right Polex Phalange1" 0.873055 0.002833 -0.000315 -0.371878 -3.349001 -2.481192 0 0 0 0 0 0
$definebone "Right Polex Phalange3" "Right Polex Phalange2" 0.7206 -0.000998 -0.002572 -6.877014 2.674679 5.135772 0 0 0 0 0 0
$definebone "Left Hand" "" 1.260293 -10.043264 -3.142162 0.30934 -89.581838 0.200764 0 0 0 0 0 0
$definebone "Left Lower Arm 2" "Left Hand" -0.001853 0.00067 0.000001 4.118822 179.118528 0 0 0 0 0 0 0
$definebone "Left Demonstratus Phalanges1" "Left Hand" 2.65317 -0.698475 0.113861 4.564182 -5.408722 89.90298 0 0 0 0 0 0
$definebone "Left Demonstratus Phalanges2" "Left Demonstratus Phalanges1" 0.704209 -0.000156 -0.001868 -1.523781 0.127258 -0.679012 0 0 0 0 0 0
$definebone "Left Demonstratus Phalanges3" "Left Demonstratus Phalanges2" 0.621946 0 0 1.906287 -0.394879 1.217192 0 0 0 0 0 0
$definebone "Left Impudicus Phalanges1" "Left Hand" 2.696814 -0.150661 0.155077 4.614087 0.186154 89.226887 0 0 0 0 0 0
$definebone "Left Impudicus Phalanges2" "Left Impudicus Phalanges1" 0.759127 -0.000239 0.000019 0.034721 0.450799 0.871927 0 0 0 0 0 0
$definebone "Left Impudicus Phalanges3" "Left Impudicus Phalanges2" 0.692899 -0.000001 0 0 -1.665937 0 0 0 0 0 0 0
$definebone "Left Annularis Phalanges1" "Left Hand" 2.64257 0.371989 0.042955 3.893362 6.235328 89.042915 0 0 0 0 0 0
$definebone "Left Annularis Phalanges2" "Left Annularis Phalanges1" 0.678047 0.000483 -0.001802 -2.11496 -0.569693 1.356019 0 0 0 0 0 0
$definebone "Left Annularis Phalanges3" "Left Annularis Phalanges2" 0.668665 0 0 0.732122 -0.309055 -1.104319 0 0 0 0 0 0
$definebone "Left Auricularis Phalanges1" "Left Hand" 2.452632 0.938823 -0.082469 -0.668413 10.637536 89.485751 0 0 0 0 0 0
$definebone "Left Auricularis Phalanges2" "Left Auricularis Phalanges1" 0.601524 0.000082 -0.001163 -0.046409 -1.102142 0.765414 0 0 0 0 0 0
$definebone "Left Auricularis Phalanges3" "Left Auricularis Phalanges2" 0.550749 0 0 -0.317588 1.759152 -0.401644 0 0 0 0 0 0
$definebone "Left Polex Metacarpal" "Left Hand" 0.219842 -0.599415 -0.044417 44.800645 -90.10848 90.437927 0 0 0 0 0 0
$definebone "Left Polex Phalange1" "Left Polex Metacarpal" 0.322868 0.125605 -0.694551 51.101174 -11.870824 77.08643 0 0 0 0 0 0
$definebone "Left Polex Phalange2" "Left Polex Phalange1" 0.868583 -0.002157 -0.000869 -1.6386 4.069003 -2.680982 0 0 0 0 0 0
$definebone "Left Polex Phalange3" "Left Polex Phalange2" 0.674673 0.000336 0.000567 2.281856 -1.370216 -5.014004 0 0 0 0 0 0
$definebone "Right shoulder" "" -5.758168 7.237389 -3.577589 0.431323 -1.66851 179.738982 0 0 0 0 0 0
$definebone "Right Upper Arm" "Right shoulder" 0 0 0 -1.190663 -89.788103 0.003667 0 0 0 0 0 0
$definebone "Right Lower Arm" "Right Upper Arm" -9.996882 0 0 -0.054718 -3.963264 -0.000172 0 0 0 0 0 0
$definebone "Hammer" "" -2.315703 -10.090808 -1.858433 59.999626 89.999982 -90.000037 0 0 0 0 0 0
$definebone "suppressorbone" "" -2.386959 -14.103163 -1.232752 0.26104 -90.000037 -89.999982 0 0 0 0 0 0


$sequence "idle" {
	"pmm_anims\idle.smd"
	activity "ACT_VM_IDLE" 1
	fadein 0.2
	fadeout 0.2
	node "0"
	snap
	fps 0.1
	loop
}

$sequence "idle_empty" {
	"pmm_anims\idle_empty.smd"
	activity "ACT_VM_IDLE_EMPTY" 1
	fadein 0.2
	fadeout 0.2
	node "0"
	snap
	fps 0.1
	loop
}

$sequence "idle_empty_iron" {
	"pmm_anims\idle_empty_iron.smd"
	activity "ACT_VM_IDLE_EMPTY_SCOPED" 1
	fadein 0.2
	fadeout 0.2
	node "0"
	snap
	fps 0.1
	loop
}

$sequence "draw" {
	"pmm_anims\draw.smd"
	activity "ACT_VM_DRAW" 1
	{ event 5004 0 "Weapon.Deploy" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 45
}

$sequence "holster" {
	"pmm_anims\holster.smd"
	activity "ACT_VM_HOLSTER" 1
	{ event 5004 0 "Weapon.Holster" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 45
}

$sequence "draw_empty" {
	"pmm_anims\draw_empty.smd"
	activity "ACT_VM_DRAW_EMPTY" 1
	{ event 5004 0 "Weapon.Deploy" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 45
}

$sequence "holster_empty" {
	"pmm_anims\holster_empty.smd"
	activity "ACT_VM_HOLSTER_EMPTY" 1
	{ event 5004 0 "Weapon.Holster" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 45
}

$sequence "fire1" {
	"pmm_anims\fire1.smd"
	activity "ACT_VM_PRIMARYATTACK" 1
	{ event AE_FAS_MUZZLE_EFFECT 0 "Muzzleflash_pmm_View" }
	{ event AE_FAS_EJECT_BRASS 1 "" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 33.5
}

$sequence "fire2" {
	"pmm_anims\fire2.smd"
	activity "ACT_VM_PRIMARYATTACK" 1
	{ event AE_FAS_MUZZLE_EFFECT 0 "Muzzleflash_pmm_View" }
	{ event AE_FAS_EJECT_BRASS 1 "" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 33.5
}

$sequence "fire_scoped2" {
	"pmm_anims\fire_scoped2.smd"
	activity "ACT_VM_PRIMARYATTACK_SCOPED" 1
	{ event AE_FAS_MUZZLE_EFFECT 0 "Muzzleflash_pmm_View" }
	{ event AE_FAS_EJECT_BRASS 1 "" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 40
}

$sequence "fire_last" {
	"pmm_anims\fire_last.smd"
	activity "ACT_VM_FIRE_LAST" 1
	{ event AE_FAS_MUZZLE_EFFECT 0 "Muzzleflash_pmm_View" }
	{ event AE_FAS_EJECT_BRASS 1 "" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 33.5
}

$sequence "fire_last_iron" {
	"pmm_anims\fire_last_iron.smd"
	activity "ACT_VM_FIRE_LAST_SCOPED" 1
	{ event AE_FAS_MUZZLE_EFFECT 0 "Muzzleflash_pmm_View" }
	{ event AE_FAS_EJECT_BRASS 1 "" }
	fadein 0.2
	fadeout 0.2
	snap
	fps 40
}

$sequence "reload" {
	"pmm_anims\reload.smd"
	activity "ACT_VM_RELOAD" 1
	{ event 5004 0 "Generic.Cloth_Movement" }
	{ event 5004 10 "Weapon_pmm.MagOut" }
	{ event 5004 31 "Generic.Magpouch_Pistol" }
	{ event 5004 39 "Generic.Cloth_Movement" }
	{ event 5004 40 "Weapon_pmm.MagInPartial" }
	{ event 5004 45 "Weapon_pmm.MagIn" }
	{ event 3018 50 "" }
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "reload_empty" {
	"pmm_anims\reload_empty.smd"
	activity "ACT_VM_RELOAD_EMPTY" 1
	{ event 5004 0 "Generic.Cloth_Movement" }
	{ event 5004 8 "Weapon_pmm.MagOutEmpty" }
	{ event 5004 18 "Generic.Magpouch_Pistol" }
	{ event 5004 26 "Generic.Cloth_Movement" }
	{ event 5004 27 "Weapon_pmm.MagInPartial" }
	{ event 5004 34 "Weapon_pmm.MagIn" }
	{ event 5004 65 "Generic.Cloth_Movement" }
	{ event 5004 66 "Weapon_pmm.SlideBack" }
	{ event 5004 70 "Weapon_pmm.SlideForward" }
	{ event 3018 75 "" }
	fadein 0.2
	fadeout 0.2
	fps 37
}

$sequence "reload_nomen" {
	"pmm_anims\reload_nomen.smd"
	activity "ACT_VM_RELOAD_NOMEN" 1
	{ event 5004 0 "Generic.Cloth_Movement" }
	{ event 5004 10 "Weapon_pmm.MagOut" }
	{ event 5004 21 "Generic.Magpouch_Pistol" }
	{ event 5004 27 "Generic.Cloth_Movement" }
	{ event 5004 28 "Weapon_pmm.MagInPartial" }
	{ event 5004 38 "Weapon_pmm.MagIn" }
	{ event 3018 38 "" }
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "reload_empty_nomen" {
	"pmm_anims\reload_empty_nomen.smd"
	activity "ACT_VM_RELOAD_EMPTY_NOMEN" 1
	{ event 5004 0 "Generic.Cloth_Movement" }
	{ event 5004 10 "Weapon_pmm.MagOutEmpty" }
	{ event 5004 21 "Generic.Magpouch_Pistol" }
	{ event 5004 27 "Generic.Cloth_Movement" }
	{ event 5004 28 "Weapon_pmm.MagInPartial" }
	{ event 5004 38 "Weapon_pmm.MagIn" }
	{ event 5004 47 "Weapon_pmm.SlideStop" }
	{ event 3018 48 "" }
	fadein 0.2
	fadeout 0.2
	fps 30
}
