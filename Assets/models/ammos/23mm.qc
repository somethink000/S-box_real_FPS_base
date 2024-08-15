// Created by Crowbar 0.74

$modelname "ammo/fas2/23mm.mdl"

$bodygroup "studio"
{
	studio "23mm.smd"
}


$surfaceprop "plastic"

$contents "solid"

$illumposition 0 0 1.385

$cdmaterials "models\ammo\fas2\"

$cbox 0 0 0 0 0 0

$bbox -1.411 -3.725 -0.225 1.411 3.725 3.02

$hboxset "default"

$definebone "cube" "" 0 0 0 0 0 89.999982 0 0 0 0 0 0


$sequence "idle" {
	"23mm_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "23mm_physics.smd"
{
	$mass 10
	$inertia 1
	$damping 0.01
	$rotdamping 0
	$rootbone " "

}
