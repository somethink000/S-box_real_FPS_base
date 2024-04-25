// Created by Crowbar 0.74

$modelname "weapons\w_glock20.mdl"

$bodygroup "studio"
{
	studio "w_glock20LOD0.smd"
}

$lod 10
{
	replacemodel "w_glock20LOD0.smd" "w_glock20LOD0_lod1.smd"
	replacebone "Weapon_Magazine_End" "Weapon_Magazine"
	replacebone "Weapon_Belt01" "Weapon_Magazine"
	replacebone "Weapon_Belt02" "Weapon_Belt01"
	replacebone "Weapon_Belt03" "Weapon_Belt02"
	replacebone "Weapon_Belt04" "Weapon_Belt03"
	replacebone "Weapon_Belt05" "Weapon_Belt04"
	replacebone "Weapon_Trigger_End" "Weapon_Trigger"
	replacebone "Weapon_Misc" "Weapon_Grip"
	replacebone "Weapon_Misc_End" "Weapon_Misc"
	replacebone "Weapon_HK_Handle1" "Weapon_Grip"
	replacebone "Weapon_HK_Handle2" "Weapon_HK_Handle1"
	replacebone "Weapon_HK_Handle_End" "Weapon_HK_Handle2"
	replacebone "Weapon_Muzzle" "Weapon_Grip"
	replacebone "Weapon_Muzzle_End" "Weapon_Muzzle"
	replacebone "Weapon_Casing" "Weapon_Grip"
	replacebone "Weapon_Casing_End" "Weapon_Casing"
	replacebone "Weapon_Bolt_End" "Weapon_Bolt"
	replacebone "Weapon_Grip_End" "Weapon_Grip"
	nofacial
}
$lod 25
{
	replacemodel "w_glock20LOD0.smd" "w_glock20LOD0_lod2.smd"
	replacebone "Weapon_Magazine_End" "Weapon_Magazine"
	replacebone "Weapon_Belt01" "Weapon_Magazine"
	replacebone "Weapon_Belt02" "Weapon_Belt01"
	replacebone "Weapon_Belt03" "Weapon_Belt02"
	replacebone "Weapon_Belt04" "Weapon_Belt03"
	replacebone "Weapon_Belt05" "Weapon_Belt04"
	replacebone "Weapon_Trigger_End" "Weapon_Trigger"
	replacebone "Weapon_Misc" "Weapon_Grip"
	replacebone "Weapon_Misc_End" "Weapon_Misc"
	replacebone "Weapon_HK_Handle1" "Weapon_Grip"
	replacebone "Weapon_HK_Handle2" "Weapon_HK_Handle1"
	replacebone "Weapon_HK_Handle_End" "Weapon_HK_Handle2"
	replacebone "Weapon_Muzzle" "Weapon_Grip"
	replacebone "Weapon_Muzzle_End" "Weapon_Muzzle"
	replacebone "Weapon_Casing" "Weapon_Grip"
	replacebone "Weapon_Casing_End" "Weapon_Casing"
	replacebone "Weapon_Bolt_End" "Weapon_Bolt"
	replacebone "Weapon_Grip_End" "Weapon_Grip"
	nofacial
}

$surfaceprop "weapon"

$contents "solid"

$illumposition -0.582 3.525 1.426

$cdmaterials "models\weapons\w_models\"

$attachment "2" "Weapon_Muzzle" 7 -1 0 rotate 0 0 0
$attachment "fas_Weapon_Belt01" "Weapon_Belt01" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Belt02" "Weapon_Belt02" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Belt03" "Weapon_Belt03" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Belt04" "Weapon_Belt04" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Belt05" "Weapon_Belt05" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Bolt" "Weapon_Bolt" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Bolt_End" "Weapon_Bolt_End" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Casing" "Weapon_Casing" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Casing_End" "Weapon_Casing_End" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Grip" "Weapon_Grip" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Grip_End" "Weapon_Grip_End" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_HK_Handle1" "Weapon_HK_Handle1" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_HK_Handle2" "Weapon_HK_Handle2" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_HK_Handle_End" "Weapon_HK_Handle_End" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Magazine" "Weapon_Magazine" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Magazine_End" "Weapon_Magazine_End" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Misc" "Weapon_Misc" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Misc_End" "Weapon_Misc_End" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Muzzle" "Weapon_Muzzle" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Muzzle_End" "Weapon_Muzzle_End" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Trigger" "Weapon_Trigger" 0 0 0 rotate 0 0 0
$attachment "fas_Weapon_Trigger_End" "Weapon_Trigger_End" 0 0 0 rotate 0 0 0

$cbox 0 0 0 0 0 0

$bbox -8.017 -2.453 -1.812 0.967 1.289 4.665

$hboxset "default"

$definebone "Weapon_Grip" "" 0 0 0 0 89.999982 89.999982 0 0 0 0 0 0
$definebone "Weapon_Magazine" "Weapon_Grip" 5.18667 0.019576 0.000001 0 0 0.000057 0 0 0 0 0 0
$definebone "Weapon_Magazine_End" "Weapon_Magazine" 0 -1.647436 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Belt01" "Weapon_Magazine" 0.019771 1.352835 -2.452978 -45.000049 -89.999982 89.999982 0 0 0 0 0 0
$definebone "Weapon_Belt02" "Weapon_Belt01" 0 0.351617 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Belt03" "Weapon_Belt02" 0 0.374191 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Belt04" "Weapon_Belt03" 0 0.361788 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Belt05" "Weapon_Belt04" 0 0.372698 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Trigger" "Weapon_Grip" 2.284173 1.891014 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Trigger_End" "Weapon_Trigger" 0 -0.989732 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Misc" "Weapon_Grip" 6.585266 2.630651 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Misc_End" "Weapon_Misc" 1.033857 0 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_HK_Handle1" "Weapon_Grip" 5.955614 4.664553 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_HK_Handle2" "Weapon_HK_Handle1" 1.032243 0 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_HK_Handle_End" "Weapon_HK_Handle2" 1.028899 0 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Muzzle" "Weapon_Grip" -0.094144 3.792605 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Muzzle_End" "Weapon_Muzzle" 1.032243 0 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Casing" "Weapon_Grip" 4.946103 4.124418 1.288599 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Casing_End" "Weapon_Casing" 1.032243 0 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Bolt" "Weapon_Grip" 2.794364 4.077009 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Bolt_End" "Weapon_Bolt" 0.863692 0 0 0 0 0 0 0 0 0 0 0
$definebone "Weapon_Grip_End" "Weapon_Grip" 1.371147 1.778537 0 0 0 0 0 0 0 0 0 0

$bonemerge "Weapon_Grip"
$bonemerge "Weapon_Magazine"
$bonemerge "Weapon_Magazine_End"
$bonemerge "Weapon_Belt01"
$bonemerge "Weapon_Belt02"
$bonemerge "Weapon_Belt03"
$bonemerge "Weapon_Belt04"
$bonemerge "Weapon_Belt05"
$bonemerge "Weapon_Trigger"
$bonemerge "Weapon_Trigger_End"
$bonemerge "Weapon_Misc"
$bonemerge "Weapon_Misc_End"
$bonemerge "Weapon_HK_Handle1"
$bonemerge "Weapon_HK_Handle2"
$bonemerge "Weapon_HK_Handle_End"
$bonemerge "Weapon_Muzzle"
$bonemerge "Weapon_Muzzle_End"
$bonemerge "Weapon_Casing"
$bonemerge "Weapon_Casing_End"
$bonemerge "Weapon_Bolt"
$bonemerge "Weapon_Bolt_End"
$bonemerge "Weapon_Grip_End"


$sequence "idle" {
	"w_glock20_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}
