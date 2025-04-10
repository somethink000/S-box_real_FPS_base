// Created by Crowbar 0.74

$modelname "ammo/fas2/45cal.mdl"

$bodygroup "studio"
{
	studio "45cal.smd"
}


$surfaceprop "plastic"

$contents "solid"

$illumposition -0.057 0.009 1.121

$cdmaterials "models\ammo\fas2\"

$cbox 0 0 0 0 0 0

$bbox -2.233 -3.511 -0.25 2.215 3.397 2.492

$hboxset "default"

$definebone "cube" "" 0 0 0 0 0 89.999982 0 0 0 0 0 0


$sequence "idle" {
	"45cal_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "45cal_physics.smd"
{
	$mass 10
	$inertia 1
	$damping 0.01
	$rotdamping 0
	$rootbone " "

}
