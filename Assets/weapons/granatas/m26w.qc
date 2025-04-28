$modelname "weapons\arc9\w_uplp_m26.mdl"

$bodygroup "studio"
{
	studio "m26w.smd"
}

$surfaceprop "metal"

$cdmaterials "models\weapons\arc9\uplp"

$sequence "idle" {
	"m26c.smd"
	fps 30
}

$collisionmodel "m26c.smd"
{
	$mass 5
}
