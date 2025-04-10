// Created by Crowbar 0.74

$modelname "ammo/fas2/9x18.mdl"

$bodygroup "studio"
{
	studio "9x18.smd"
}


$surfaceprop "plastic"

$contents "solid"

$illumposition -0.046 0.008 0.945

$cdmaterials "models\ammo\fas2\"

$cbox 0 0 0 0 0 0

$bbox -1.922 -2.881 -0.25 1.906 2.788 2.14

$hboxset "default"

$definebone "cube" "" 0 0 0 0 0 89.999982 0 0 0 0 0 0


$sequence "idle" {
	"9x18_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "9x18_physics.smd"
{
	$mass 10
	$inertia 1
	$damping 0.01
	$rotdamping 0
	$rootbone " "

}
