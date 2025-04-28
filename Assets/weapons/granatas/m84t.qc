$modelname "weapons\arc9\w_uplp_m84_thrown.mdl"

$bodygroup "studio"
{
	studio "m84t.smd"
}

$surfaceprop "metal"

$cdmaterials "models\weapons\arc9\uplp"

$sequence "idle" {
	"m84c.smd"
	fps 30
}

$collisionmodel "m84c.smd"
{
	$mass 5
}
