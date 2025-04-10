// Created by Crowbar 0.74

$modelname "shells\12g_buck.mdl"

$bodygroup "studio"
{
	studio "12g_buck.smd"
}

$lod 3
{
	replacemodel "12g_buck.smd" "12g_buck_lod1.smd"
	nofacial
}

$surfaceprop "casings_12g"

$contents "solid"

$illumposition 0.017 -1.458 0

$cdmaterials "models\weapons\shells\"

$attachment "casing" "Casing" 3.38 0 0 rotate 0 0 0

$cbox 0 0 0 0 0 0

$bbox -0.794 -0.811 -0.857 3.672 0.872 0.857

$definebone "Casing" "" 0 0 0 0 -89.999982 0 0 0 0 0 0 0


$sequence "idle" {
	"12g_buck_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "12g_buck_physics.smd"
{
	$mass 0.1
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
