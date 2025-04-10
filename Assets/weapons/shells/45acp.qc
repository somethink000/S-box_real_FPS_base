// Created by Crowbar 0.74

$modelname "shells\45acp.mdl"

$bodygroup "studio"
{
	studio "45acp.smd"
}

$lod 3
{
	replacemodel "45acp.smd" "45acp_lod1.smd"
	nofacial
}

$surfaceprop "casings_pistol"

$contents "solid"

$illumposition 0 -0.069 0.035

$cdmaterials "models\weapons\shells\"

$attachment "casing" "Casing" 0.65 0 0 rotate 0 0 0

$cbox 0 0 0 0 0 0

$bbox -0.783 -0.535 -0.558 0.905 0.447 0.558

$definebone "Casing" "" 0 0 0 0 -89.999982 0 0 0 0 0 0 0


$sequence "idle" {
	"45acp_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "45acp_physics.smd"
{
	$mass 0.1
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
