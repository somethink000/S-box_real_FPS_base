// Created by Crowbar 0.74

$modelname "weapons\fas2\world\explosives\m67.mdl"

$staticprop

$bodygroup "studio"
{
	studio "m67.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition -0.185 -0.329 -1.002

$cdmaterials "models\weapons\fas2\explosives\m67\"

$cbox 0 0 0 0 0 0

$bbox -2.01 -2.296 -3.851 2.636 1.918 1.837

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"m67_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 15
	loop
}

$collisionmodel "m67_physics.smd"
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
