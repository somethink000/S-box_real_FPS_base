// Created by Crowbar 0.74

$modelname "shells\9x18mm.mdl"

$bodygroup "studio"
{
	studio "9x18mm.smd"
}

$lod 3
{
	replacemodel "9x18mm.smd" "9x18mm_lod1.smd"
	nofacial
}

$surfaceprop "casings_pistol"

$contents "solid"

$illumposition 0 0.042 0.023

$cdmaterials "models\weapons\shells\"

$attachment "casing" "Casing" 0.39 0 0 rotate 0 0 0

$cbox 0 0 0 0 0 0

$bbox -0.762 -0.551 -0.395 0.665 0.584 0.511

$definebone "Casing" "" 0 0 0 0 -89.999982 0 0 0 0 0 0 0


$sequence "idle" {
	"9x18mm_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "9x18mm_physics.smd"
{
	$mass 0.1
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
