// Created by Crowbar 0.74

$modelname "shells\7_62x39mm.mdl"

$bodygroup "studio"
{
	studio "7_62x39mm.smd"
}

$lod 3
{
	replacemodel "7_62x39mm.smd" "7_62x39mm_lod1.smd"
	nofacial
}

$surfaceprop "casings_rifle"

$contents "solid"

$illumposition 0 -0.059 0.036

$cdmaterials "models\weapons\shells\"

$attachment "casing" "Casing" 1.04 0 0 rotate 0 0 0

$cbox 0 0 0 0 0 0

$bbox -1.208 -0.531 -0.549 1.301 0.562 0.49

$definebone "Casing" "" 0 0 0 0 -89.999982 0 0 0 0 0 0 0


$sequence "idle" {
	"7_62x39mm_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "7_62x39mm_physics.smd"
{
	$mass 0.1
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
