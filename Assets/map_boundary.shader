FEATURES
{
    #include "common/features.hlsl"
}

MODES
{
    VrForward();
    Depth();
}

COMMON
{
	#include "common/shared.hlsl"
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

PS
{
    #include "common/pixel.hlsl"

	#define BLEND_MODE_ALREADY_SET
	RenderState( BlendEnable, true );
	RenderState( SrcBlend, SRC_ALPHA );
	RenderState( DstBlend, ONE );
	RenderState( CullMode, NONE );
	BoolAttribute( translucent, true );

	float3 g_vGlowColor < Default3( 0.2, 0.6, 1.0 ); UiType( Color ); UiGroup( "Boundary,10/10" ); >;
	float g_flGlowIntensity < Default( 2.0 ); Range( 0.1, 10.0 ); UiGroup( "Boundary,10/20" ); >;
	float g_flFadeStart < Default( 2000.0 ); Range( 100.0, 10000.0 ); UiGroup( "Boundary,10/30" ); >;
	float g_flFadeEnd < Default( 500.0 ); Range( 50.0, 5000.0 ); UiGroup( "Boundary,10/40" ); >;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		// Distance from camera
		float flDist = length( i.vPositionWithOffsetWs.xyz );

		// Fade: 0 at FadeStart (far), 1 at FadeEnd (near)
		float flFade = saturate( ( g_flFadeStart - flDist ) / max( g_flFadeStart - g_flFadeEnd, 1.0 ) );

		// Discard fully transparent pixels — zero GPU cost when far away
		clip( flFade - 0.001 );

		// Fresnel for edge glow
		float3 vViewDir = normalize( -i.vPositionWithOffsetWs.xyz );
		float flNdotV = abs( dot( i.vNormalWs, vViewDir ) );
		float flFresnel = pow( 1.0 - flNdotV, 3.0 );

		float flAlpha = flFade * ( 0.05 + flFresnel * 0.8 );

		float3 vEmission = g_vGlowColor * g_flGlowIntensity * flFade * ( 0.2 + flFresnel );

		// Additive blend — just output color * alpha
		return float4( vEmission * flAlpha, flAlpha );
	}
}
