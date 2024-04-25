// Created by Crowbar 0.74

$modelname "weapons\fas2\world\rifles\ak74u_modern.mdl"

$bodygroup "studio"
{
	studio "ak74u_modern_ak74u_mag_ref.smd"
}
$bodygroup "Barrel"
{
	blank
	studio "ak74u_modern_ak74u_mag_ref_suppressor.smd"
}
$bodygroup "Sights"
{
	blank
	studio "ak74u_modern_ak74u_mag_ref_PSO.smd"
}
$bodygroup "Magazine"
{
	studio "ak74u_modern_ak74u_mag_ref_mag.smd"
	studio "ak74u_modern_ak74u_mag_ref_mag_ext.smd"
}


$surfaceprop "weapon"

$contents "solid"

$illumposition -2.217 0.129 -1.008

$cdmaterials "models\weapons\fas2\rifles\ak74u_m\"
$cdmaterials "models\weapons\view\hands\"
$cdmaterials "models\weapons\fas2\accessories\"
$cdmaterials "models\weapons\view\accessories\"
$cdmaterials "models\weapons\shells\"

$cbox 0 0 0 0 0 0

$bbox -2.155 -14.052 -8.374 1.549 18.633 5.254

$definebone "root" "" 0 0 0 0 0 0 0 0 0 0 0 0


$sequence "idle" {
	"ak74u_modern_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "ak74u_modern_physics.smd"
{
	$mass 8
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "
	$concave
	$maxconvexpieces 27

}
