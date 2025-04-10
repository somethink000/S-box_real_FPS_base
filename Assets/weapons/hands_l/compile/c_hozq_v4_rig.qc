// Created by Crowbar 0.74

$modelname "player/darsu/c_hozq_v4_rig.mdl"

$bodygroup "Gloves"
{
	studio "Knuckled gloves.smd"
	studio "Mechanix gloves fingerless.smd"
	studio "Knuckled fingerless.smd"
	studio "Mechanix gloves.smd"
	studio "Mesh gloves.smd"
	studio "Cloth gloves.smd"
	studio "Cloth gloves fingerless.smd"
	blank
}
$bodygroup "Sleeves"
{
	studio "GorkaUp.smd"
	studio "Gorka.smd"
	studio "BlouseUp.smd"
	studio "Blouse.smd"
	studio "Blouse2Up.smd"
	studio "Blouse2.smd"
	studio "Hoodie.smd"
	studio "HoodieFull.smd"
	studio "Barearms.smd"
	studio "BarearmsFull.smd"
	blank
}
$bodygroup "Accseroris"
{
	studio "Watch.smd"
	studio "Cuffs.smd"
	blank
}
$bodygroup "Ring"
{
	blank
	studio "Ring.smd"
}


$surfaceprop "default"

$contents "solid"

$illumposition -0.074 -0.892 -10.98

$cdmaterials "models\weapons\arc9\uplp\"

$cbox 0 0 0 0 0 0

$bbox -4.325 -27.474 -24.067 4.523 27.126 3.591

$definebone "ValveBiped.Bip01_Spine4" "" 0.000006 0 0 -79.749938 -89.999982 -89.999873 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Clavicle" "ValveBiped.Bip01_Spine4" 2.033348 1.000771 1.937682 -73.711682 168.618478 91.481402 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_UpperArm" "ValveBiped.Bip01_L_Clavicle" 6.028206 0.000038 -0.000014 2.739026 -33.527934 -90.505327 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Forearm" "ValveBiped.Bip01_L_UpperArm" 11.692616 -0.000022 0.000095 0.000161 -3.458601 -0.000629 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Hand" "ValveBiped.Bip01_L_Forearm" 11.481735 -0.000102 0.000099 -6.108199 2.5315 90.137385 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger4" "ValveBiped.Bip01_L_Hand" 3.85973 -0.142338 -1.191976 8.68249 -50.430828 -26.821526 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger41" "ValveBiped.Bip01_L_Finger4" 1.312565 -0.00001 0.000004 -0.00051 -25.194227 0.000005 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger42" "ValveBiped.Bip01_L_Finger41" 0.729375 -0.000008 -0.000005 -0.000338 -13.997599 -0.000003 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger3" "ValveBiped.Bip01_L_Hand" 3.942345 0.04684 -0.431445 4.193993 -46.465558 -8.696238 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger31" "ValveBiped.Bip01_L_Finger3" 1.539099 0.000011 -0.00001 0.000228 -14.197368 -0.000225 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger32" "ValveBiped.Bip01_L_Finger31" 1.196331 -0.000006 -0.000003 -0.000514 -24.683539 -0.000053 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger2" "ValveBiped.Bip01_L_Hand" 3.883713 -0.046696 0.431442 1.250995 -33.681312 4.921763 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger21" "ValveBiped.Bip01_L_Finger2" 1.71958 -0.000006 0.000008 0.315929 -20.99781 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger22" "ValveBiped.Bip01_L_Finger21" 1.209187 0 0.000003 0.176931 -12.398409 -0.000001 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger1" "ValveBiped.Bip01_L_Hand" 3.859879 -0.137877 1.332467 -2.44974 -30.465007 20.308377 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger11" "ValveBiped.Bip01_L_Finger1" 1.719437 0.000017 -0.000014 0.233942 -20.798421 -0.000459 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger12" "ValveBiped.Bip01_L_Finger11" 1.099667 -0.000006 0.000003 0.149088 -13.998849 0.000345 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger0" "ValveBiped.Bip01_L_Hand" 0.806047 -0.348907 1.32126 -38.916225 -45.195024 -70.05475 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger01" "ValveBiped.Bip01_L_Finger0" 1.789782 0.000004 -0.000004 0.000051 13.09335 0.000005 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Finger02" "ValveBiped.Bip01_L_Finger01" 1.207008 0.000007 -0.000002 0.00005 20.790348 0.000058 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Ulna" "ValveBiped.Bip01_L_Forearm" 5.740883 -0.00005 0.000064 -0.000012 -0.0004 0.039822 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_L_Wrist" "ValveBiped.Bip01_L_Forearm" 11.48177 -0.000102 0.000123 -0.000012 -0.0004 0.07334 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Clavicle" "ValveBiped.Bip01_Spine4" 2.033358 1.000768 -1.937633 73.711982 168.618752 -95.596557 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_UpperArm" "ValveBiped.Bip01_R_Clavicle" 6.028208 0.000038 0.000016 -0.461919 -33.623806 93.935114 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Forearm" "ValveBiped.Bip01_R_UpperArm" 11.692619 0.000052 -0.000093 -0.00012 -3.458601 -0.000227 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Hand" "ValveBiped.Bip01_R_Forearm" 11.481762 -0.000028 -0.000121 6.10754 2.532188 -89.66395 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger4" "ValveBiped.Bip01_R_Hand" 3.859716 -0.132501 1.193092 -9.056507 -50.352151 26.712778 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger41" "ValveBiped.Bip01_R_Finger4" 1.312567 -0.000006 -0.000004 0.000412 -25.191281 0.000401 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger42" "ValveBiped.Bip01_R_Finger41" 0.729371 -0.000006 0.000005 0.000239 -13.995916 0.00005 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger3" "ValveBiped.Bip01_R_Hand" 3.942327 0.050398 0.431027 -4.536908 -46.420524 8.598551 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger31" "ValveBiped.Bip01_R_Finger3" 1.539099 0.000004 0.000004 -0.000285 -14.195827 0.000174 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger32" "ValveBiped.Bip01_R_Finger31" 1.196324 -0.000011 0 0.000288 -24.681075 -0.000231 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger2" "ValveBiped.Bip01_R_Hand" 3.883688 -0.050261 -0.431061 -1.512437 -33.671931 -4.921994 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger21" "ValveBiped.Bip01_R_Finger2" 1.719587 0.00001 0.000006 -0.456761 -20.995226 0 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger22" "ValveBiped.Bip01_R_Finger21" 1.209188 0.000006 0.000007 -0.256226 -12.396679 0.00023 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger1" "ValveBiped.Bip01_R_Hand" 3.859858 -0.148871 -1.331302 2.21041 -30.481478 -20.306941 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger11" "ValveBiped.Bip01_R_Finger1" 1.719433 0.000023 0.000022 -0.37918 -20.796246 0.000405 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger12" "ValveBiped.Bip01_R_Finger11" 1.099667 0.000013 0.000004 -0.241269 -13.997976 0.000116 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger0" "ValveBiped.Bip01_R_Hand" 0.806026 -0.35983 -1.31834 38.596252 -45.466735 69.981292 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger01" "ValveBiped.Bip01_R_Finger0" 1.789787 -0.000008 0 -0.000184 13.094908 -0.000169 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Finger02" "ValveBiped.Bip01_R_Finger01" 1.207005 0 -0.000002 -0.00024 20.793162 0.000229 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Ulna" "ValveBiped.Bip01_R_Forearm" 5.740883 -0.000013 -0.000062 -0.00001 0.000002 0.004757 0 0 0 0 0 0
$definebone "ValveBiped.Bip01_R_Wrist" "ValveBiped.Bip01_R_Forearm" 11.481762 -0.000028 -0.000121 -0.00001 -0.0004 0.009112 0 0 0 0 0 0

$proceduralbones "c_hozq_v4_rig.vrd"


$sequence "idle" {
	"c_hozq_v4_rig_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$includemodel "weapons/c_arms_animations.mdl"
