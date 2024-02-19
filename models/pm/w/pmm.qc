// Created by Crowbar 0.74

$modelname "weapons\fas2\world\pistols\pmm.mdl"

$bodygroup "weapon"
{
	studio "pmm_ref.smd"
}


$surfaceprop "weapon"

$contents "solid"

$illumposition -1.523 0.032 -0.712

$cdmaterials "models\weapons\fas2\pistols\pmm\"

$cbox 0 0 0 0 0 0

$bbox -0.976 -5.336 -3.683 0.871 2.234 2.263

$definebone "root" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"pmm_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "pmm_physics.smd"
{
	$mass 8
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "
	$concave
	$maxconvexpieces 14

}
