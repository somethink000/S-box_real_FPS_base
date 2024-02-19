// Created by Crowbar 0.74

$modelname "weapons\fas2\view\rifles\ak74u_modern.mdl"

$bodygroup "studio"
{
	studio "ak74u_modern_ak74u_mag_ref.smd"
}
$bodygroup "Hands"
{
	studio "ak74u_modern_ak74u_mag_ref_hand_red.smd"
	studio "ak74u_modern_ak74u_mag_ref_hand_blue.smd"
}
$bodygroup "Barrel"
{
	blank
	studio "ak74u_modern_ak74u_mag_ref_suppressor.smd"
}
$bodygroup "Sights"
{
	blank
	studio "ak74u_modern_ak74u_mag_ref_PSO.smd"
}
$bodygroup "Magazine"
{
	studio "ak74u_modern_ak74u_mag_ref_mag.smd"
	studio "ak74u_modern_ak74u_mag_ref_mag_ext.smd"
}


$surfaceprop "default"

$contents "solid"

$illumposition 7.765 2.405 -6.776

$cdmaterials "models\weapons\fas2\rifles\ak74u_m\"
$cdmaterials "models\weapons\view\hands\"
$cdmaterials "models\weapons\view\accessories\"
$cdmaterials "models\weapons\shells\"

$attachment "muzzle" "RPK Base" 18 0 0 rotate 0 0 0
$attachment "ejector" "RPK Base" 2 0.6 0.2 rotate -60 90 0
$attachment "muzzle_suppressor" "RPK Base" 18 0 0 rotate 0 0 0
$attachment "Sight1" "RPK" -3 -1.87 0.45 rotate -0.3 0 0
$attachment "Sight4" "RPK" -2 -1.76 0.17 rotate 0 0 0

$cbox 0 0 0 0 0 0

$bbox -9.489 -15.62 -22.084 26.988 13.858 5.318

$definebone "RPK Base" "" -2.370777 -10.765888 -2.515194 0 -89.999927 89.999982 0 0 0 0 0 0
$definebone "RPK BipodElevation" "RPK Base" 16.029057 -7.299944 0.000052 0 0 0 0 0 0 0 0 0
$definebone "RPK BipodPivot" "RPK BipodElevation" -0.00001 7.708657 -0.000006 0 0 0 0 0 0 0 0 0
$definebone "Trigger" "RPK BipodPivot" -16.421867 -0.681483 -0.000052 -0.000688 -69.943705 0.000115 0 0 0 0 0 0
$definebone "MagazineCatch" "RPK BipodPivot" -15.120115 -1.090753 -0.000047 -0.000057 -89.999982 0.000115 0 0 0 0 0 0
$definebone "Selector" "RPK BipodPivot" -16.972506 -0.220409 0.435322 0 0 0 0 0 0 0 0 0
$definebone "Bolt" "RPK BipodPivot" -12.824371 0.239286 -0.000042 -89.916729 -90.044501 0.069328 0 0 0 0 0 0
$definebone "RPK" "" -0.000005 -15.619987 -0.855055 0 -90.000037 90.000037 0 0 0 0 0 0
$definebone "Right Hand" "" -5.81565 -10.090681 -3.135012 -0.642114 89.643084 -179.659739 0 0 0 0 0 0
$definebone "Right Lower Arm2" "Right Hand" 0.001853 -0.011043 0.000007 -2.096854 -2.794373 0 0 0 0 0 0 0
$definebone "Right Demonstratus Phalanges1" "Right Hand" -2.693112 0.598938 -0.105602 -5.218156 175.165802 -88.556422 0 0 0 0 0 0
$definebone "Right Demonstratus Phalange2" "Right Demonstratus Phalanges1" 0.668921 -0.00023 0.000988 0.898168 0.209357 0.603439 0 0 0 0 0 0
$definebone "Right Demonstratus Phalange3" "Right Demonstratus Phalange2" 0.620682 -0.000352 0.000185 -0.267973 -0.515149 0.367323 0 0 0 0 0 0
$definebone "Right Impudicus Phalanges1" "Right Hand" -2.735406 0.046611 -0.154424 -5.927537 -179.679792 -88.760159 0 0 0 0 0 0
$definebone "Right Impudicus Phalanges2" "Right Impudicus Phalanges1" 0.693262 -0.001516 0.000457 0.476529 1.580502 -0.39007 0 0 0 0 0 0
$definebone "Right Impudicus Phalanges3" "Right Impudicus Phalanges2" 0.708672 0 0 -0.273416 -1.910702 -1.257642 0 0 0 0 0 0
$definebone "Right Annularis Phalanges1" "Right Hand" -2.579779 -0.47033 -0.041396 -0.877428 -174.239628 -86.902758 0 0 0 0 0 0
$definebone "Right Annularis Phalanges2" "Right Annularis Phalanges1" 0.698665 -0.00031 -0.000169 -0.23096 0.417801 -1.701055 0 0 0 0 0 0
$definebone "Right Annularis Phalanges3" "Right Annularis Phalanges2" 0.666528 0 0 -1.43566 -0.607335 -0.392877 0 0 0 0 0 0
$definebone "Right Auricularis Phalanges1" "Right Hand" -2.466993 -1.051966 0.081883 -0.942573 -166.863065 -87.70872 0 0 0 0 0 0
$definebone "Right Auricularis Phalanges2" "Right Auricularis Phalanges1" 0.572366 -0.000204 0.001442 3.261791 0.471544 -1.601876 0 0 0 0 0 0
$definebone "Right Auricularis Phalanges3" "Right Auricularis Phalanges2" 0.515579 0 0 -2.448937 -0.143583 -1.142363 0 0 0 0 0 0
$definebone "Right Polex Metacarpal" "Right Hand" -0.205823 0.527907 0.046739 44.660169 -89.716427 -89.094306 0 0 0 0 0 0
$definebone "Right Polex Phalange1" "Right Polex Metacarpal" -0.218396 0.054786 -0.682163 48.019191 -165.497082 114.931894 0 0 0 0 0 0
$definebone "Right Polex Phalange2" "Right Polex Phalange1" 0.873056 0.002831 -0.000315 -0.371857 -3.348997 -2.481187 0 0 0 0 0 0
$definebone "Right Polex Phalange3" "Right Polex Phalange2" 0.720599 -0.000998 -0.002572 -6.877049 2.674684 5.135774 0 0 0 0 0 0
$definebone "Left Hand" "" 1.260293 -10.043264 -3.142162 0.30934 -89.581838 0.200764 0 0 0 0 0 0
$definebone "Left Lower Arm 2" "Left Hand" -0.001855 0.00067 0.000001 4.118822 179.118528 0 0 0 0 0 0 0
$definebone "Left Demonstratus Phalanges1" "Left Hand" 2.65317 -0.698475 0.113861 4.564184 -5.408722 89.90298 0 0 0 0 0 0
$definebone "Left Demonstratus Phalanges2" "Left Demonstratus Phalanges1" 0.704207 -0.000156 -0.001868 -1.523781 0.127255 -0.679012 0 0 0 0 0 0
$definebone "Left Demonstratus Phalanges3" "Left Demonstratus Phalanges2" 0.621948 0 0 1.906287 -0.394881 1.217192 0 0 0 0 0 0
$definebone "Left Impudicus Phalanges1" "Left Hand" 2.696812 -0.150661 0.155077 4.614087 0.186154 89.226887 0 0 0 0 0 0
$definebone "Left Impudicus Phalanges2" "Left Impudicus Phalanges1" 0.759125 -0.000239 0.000019 0.034721 0.450802 0.871927 0 0 0 0 0 0
$definebone "Left Impudicus Phalanges3" "Left Impudicus Phalanges2" 0.692899 -0.000001 0 0 -1.665934 0 0 0 0 0 0 0
$definebone "Left Annularis Phalanges1" "Left Hand" 2.642568 0.371989 0.042955 3.893362 6.235328 89.042915 0 0 0 0 0 0
$definebone "Left Annularis Phalanges2" "Left Annularis Phalanges1" 0.678045 0.000483 -0.001802 -2.11496 -0.569693 1.356019 0 0 0 0 0 0
$definebone "Left Annularis Phalanges3" "Left Annularis Phalanges2" 0.668664 0 0 0.732125 -0.309055 -1.104319 0 0 0 0 0 0
$definebone "Left Auricularis Phalanges1" "Left Hand" 2.45263 0.938823 -0.082469 -0.668413 10.637536 89.485751 0 0 0 0 0 0
$definebone "Left Auricularis Phalanges2" "Left Auricularis Phalanges1" 0.601522 0.000082 -0.001163 -0.046409 -1.102142 0.765414 0 0 0 0 0 0
$definebone "Left Auricularis Phalanges3" "Left Auricularis Phalanges2" 0.550749 0 0 -0.31759 1.759152 -0.401643 0 0 0 0 0 0
$definebone "Left Polex Metacarpal" "Left Hand" 0.21984 -0.599415 -0.044417 44.800655 -90.1085 90.437947 0 0 0 0 0 0
$definebone "Left Polex Phalange1" "Left Polex Metacarpal" 0.322868 0.125605 -0.694551 51.101187 -11.870818 77.08643 0 0 0 0 0 0
$definebone "Left Polex Phalange2" "Left Polex Phalange1" 0.868585 -0.002157 -0.000869 -1.638593 4.068973 -2.680984 0 0 0 0 0 0
$definebone "Left Polex Phalange3" "Left Polex Phalange2" 0.674671 0.000336 0.000567 2.281872 -1.370227 -5.01401 0 0 0 0 0 0
$definebone "Right shoulder" "" -5.758168 7.237389 -3.577589 0.431323 -1.66851 179.738982 0 0 0 0 0 0
$definebone "Right Upper Arm" "Right shoulder" 0 0 0 -1.190664 -89.788103 0.003667 0 0 0 0 0 0
$definebone "Right Lower Arm" "Right Upper Arm" -9.996886 0 0 -0.054717 -3.963264 -0.000172 0 0 0 0 0 0
$definebone "PSO-1Main Dummy" "RPK BipodPivot" -15.900739 1.63566 -0.104212 89.999811 0 0 0 0 0 0 0 0
$definebone "PSO1_Eyepiece1" "PSO-1Main Dummy" 0.000667 0.014168 -3.948096 89.999982 0 0 0 0 0 0 0 0
$definebone "Magazine" "RPK BipodPivot" -14.337588 -1.450614 -0.000045 0 0 0 0 0 0 0 0 0
$definebone "SpareMagazine" "RPK BipodPivot" -14.337584 -11.88107 -0.00004 0 0 0 0 0 0 0 0 0


$sectionframes 30 120

$sequence "deploy_first" {
	"ak74u_modern_anims\deploy_first.smd"
	activity "ACT_VM_DRAW_FIRST" 1
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "deploy" {
	"ak74u_modern_anims\deploy.smd"
	activity "ACT_VM_DRAW" 1
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "holster" {
	"ak74u_modern_anims\holster.smd"
	activity "ACT_VM_HOLSTER" 1
	fadein 0.2
	fadeout 0.2
	fps 60
}

$sequence "idle" {
	"ak74u_modern_anims\idle.smd"
	activity "ACT_VM_IDLE" 1
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "idle_scoped" {
	"ak74u_modern_anims\idle_scoped.smd"
	activity "ACT_VM_IDLE_SCOPED" 1
	fadein 0.2
	fadeout 0.2
	fps 0.01
}

$sequence "fire1" {
	"ak74u_modern_anims\fire1.smd"
	activity "ACT_VM_PRIMARYATTACK" 1
	fadein 0.2
	fadeout 0.2
	snap
	fps 60
}

$sequence "fire2" {
	"ak74u_modern_anims\fire2.smd"
	activity "ACT_VM_PRIMARYATTACK" 1
	fadein 0.2
	fadeout 0.2
	snap
	fps 60
}

$sequence "fire3" {
	"ak74u_modern_anims\fire3.smd"
	activity "ACT_VM_PRIMARYATTACK" 1
	fadein 0.2
	fadeout 0.2
	snap
	fps 60
}

$sequence "fire1_scoped" {
	"ak74u_modern_anims\fire1_scoped.smd"
	activity "ACT_VM_PRIMARYATTACK_SCOPED" 1
	fadein 0.2
	fadeout 0.2
	snap
	fps 60
}

$sequence "fire2_scoped" {
	"ak74u_modern_anims\fire2_scoped.smd"
	activity "ACT_VM_PRIMARYATTACK_SCOPED" 1
	fadein 0.2
	fadeout 0.2
	snap
	fps 60
}

$sequence "fire3_scoped" {
	"ak74u_modern_anims\fire3_scoped.smd"
	activity "ACT_VM_PRIMARYATTACK_SCOPED" 1
	fadein 0.2
	fadeout 0.2
	snap
	fps 60
}

$sequence "reload" {
	"ak74u_modern_anims\reload.smd"
	activity "ACT_VM_RELOAD" 1
	fadein 0.2
	fadeout 0.2
	fps 30
}

$sequence "reload_empty" {
	"ak74u_modern_anims\reload_empty.smd"
	activity "ACT_VM_RELOAD_EMPTY" 1
	fadein 0.2
	fadeout 0.2
	snap
	fps 30
}

$sequence "reload_nomen" {
	"ak74u_modern_anims\reload_nomen.smd"
	activity "ACT_VM_RELOAD_NOMEN" 1
	fadein 0.2
	fadeout 0.2
	snap
	fps 30
}

$sequence "reload_empty_nomen" {
	"ak74u_modern_anims\reload_empty_nomen.smd"
	activity "ACT_VM_RELOAD_EMPTY_NOMEN" 1
	fadein 0.2
	fadeout 0.2
	snap
	fps 30
}
