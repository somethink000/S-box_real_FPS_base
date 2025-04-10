// Created by Crowbar 0.74

$modelname "shells\357mag.mdl"

$bodygroup "studio"
{
	studio "357mag.smd"
}

$lod 3
{
	replacemodel "357mag.smd" "357mag_lod1.smd"
	nofacial
}

$surfaceprop "casings_pistol"

$contents "solid"

$illumposition 0 -0.05 0.013

$cdmaterials "models\weapons\shells\"

$attachment "casing" "Casing" 0.88 0 0 rotate 0 0 0

$cbox 0 0 0 0 0 0

$bbox -1.025 -0.487 -0.557 1.122 0.301 0.557

$definebone "Casing" "" 0 0 0 0 -89.999982 0 0 0 0 0 0 0


$sequence "idle" {
	"357mag_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "357mag_physics.smd"
{
	$mass 0.1
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
