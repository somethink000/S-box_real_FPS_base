// Created by Crowbar 0.74

$modelname "medical\fas2\medical_supplies.mdl"

$bodygroup "box"
{
	studio "medical_supplies_box.smd"
}
$bodygroup "charges"
{
	studio "medical_supplies_4.smd"
	studio "medical_supplies_3.smd"
	studio "medical_supplies_2.smd"
	studio "medical_supplies_1.smd"
}


$surfaceprop "plastic"

$contents "solid"

$illumposition 0 0 0

$cdmaterials "models\medical\fas2\"

$cbox 0 0 0 0 0 0

$bbox -10.845 -10.642 -0.093 10.841 10.682 20.971

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"medical_supplies_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "medical_supplies_physics.smd"
{
	$mass 20
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}
