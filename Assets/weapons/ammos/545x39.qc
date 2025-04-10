// Created by Crowbar 0.74

$modelname "ammo/fas2/545x39.mdl"

$bodygroup "studio"
{
	studio "545x39.smd"
}


$surfaceprop "plastic"

$contents "solid"

$illumposition 0 0 1.637

$cdmaterials "models\ammo\fas2\"

$cbox 0 0 0 0 0 0

$bbox -2.555 -4.557 -0.189 2.555 4.557 3.525

$hboxset "default"

$definebone "cube" "" 0 0 0 0 0 89.999982 0 0 0 0 0 0


$sequence "idle" {
	"545x39_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "545x39_physics.smd"
{
	$mass 10
	$inertia 1
	$damping 0.01
	$rotdamping 0
	$rootbone " "

}
