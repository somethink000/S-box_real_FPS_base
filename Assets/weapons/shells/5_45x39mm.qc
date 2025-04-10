// Created by Crowbar 0.74

$modelname "shells\5_45x39mm.mdl"

$bodygroup "studio"
{
	studio "5_45x39mm.smd"
}

$lod 3
{
	replacemodel "5_45x39mm.smd" "5_45x39mm_lod1.smd"
	nofacial
}

$surfaceprop "casings_rifle"

$contents "solid"

$illumposition 0 -0.059 0.036

$cdmaterials "models\weapons\shells\"

$attachment "casing" "Casing" 1.04 0 0 rotate 0 0 0

$cbox 0 0 0 0 0 0

$bbox -1.201 -0.355 -0.569 1.319 0.454 0.506

$definebone "Casing" "" 0 0 0 0 -89.999982 0 0 0 0 0 0 0


$sequence "idle" {
	"5_45x39mm_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "5_45x39mm_physics.smd"
{
	$mass 0.1
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
