// Created by Crowbar 0.74

$modelname "weapons\fas2\world\explosives\m67_thrown.mdl"

$staticprop

$bodygroup "studio"
{
	studio "m67_thrown_m67.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -0.185 -0.532 -1.035

$cdmaterials "models\weapons\fas2\explosives\m67\"

$cbox 0 0 0 0 0 0

$bbox -1.598 -2.296 -3.851 2.636 1.926 1.818

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"m67_thrown_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 15
	loop
}

$collisionmodel "m67_thrown_physics.smd"
{
	$mass 4
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}

$keyvalues
{
	prop_data 
	{
		"base" "Metal.Small"
	}
}
