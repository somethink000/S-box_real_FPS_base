HEADER
{
	DevShader = true;
	Description = "Flat white silhouette — рисуется в маску для выреза объекта из пост-эффекта";
}

MODES
{
	Forward();
}

FEATURES
{
	#include "vr_common_features.fxc"
}

COMMON
{
	#define BLEND_MODE_ALREADY_SET
	#include "system.fxc"
	#include "vr_common.fxc"
}

struct VS_INPUT
{
	#include "vr_shared_standard_vs_input.fxc"
};

struct PS_INPUT
{
	#include "vr_shared_standard_ps_input.fxc"
};

VS
{
	#include "vr_shared_standard_vs_code.fxc"

	PS_INPUT MainVs( VS_INPUT i )
	{
		PS_INPUT o = VS_SharedStandardProcessing( i );
		return VS_CommonProcessing_Post( o );
	}
}

PS
{
	RenderState( DepthWriteEnable, true );
	RenderState( DepthEnable, true );

	float4 MainPs( PS_INPUT i ) : SV_Target0
	{
		return float4( 1.0, 1.0, 1.0, 1.0 );
	}
}
