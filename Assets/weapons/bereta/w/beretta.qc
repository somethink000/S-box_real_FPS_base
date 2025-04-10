// Created by Crowbar 0.74

$modelname "weapons\arc9\w_uplp_beretta.mdl"
 

$bodygroup "body"
{
	studio "beretta.smd"
}

$lod 16
{
	replacemodel "beretta.smd" "berettalod.smd"
}

$surfaceprop "weapon"

$contents "solid"

$illumposition -0.342 -1.674 3.482

$cdmaterials "models/weapons/arc9/uplp"

$attachment "muzzle" "ValveBiped.Bip01_R_Hand" 10.3 -2.1 -3.8 rotate 3 -5 -90
$attachment "shell" "ValveBiped.Bip01_R_Hand" 5.2 -2 -3.5 rotate 30 -95 -90

$cbox 0 0 0 0 0 0
$bbox -3.759 -2.105 -0.184 7.253 1.421 7.149

$definebone "ValveBiped.Bip01_R_Hand" "" -1.182861 3.660735 2.522177 2.500022 -94.999995 -180.000005 0 0 0 0 0 0


$collisionmodel "berettalod.smd"
{
	$mass 12.0
	// $concave
}

$sequence "idle" {
	"beretta.smd"
}