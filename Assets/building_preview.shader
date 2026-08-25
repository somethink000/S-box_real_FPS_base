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
	RenderState( DstBlend, INV_SRC_ALPHA );
	BoolAttribute( translucent, true );

	float3 g_vColor < Default3( 0.1, 0.4, 1.0 ); UiType( Color ); UiGroup( "Preview,10/10" ); >;
	float g_flOpacity < Default( 0.08 ); Range( 0.0, 1.0 ); UiGroup( "Preview,10/20" ); >;
	float g_flEdgeGlow < Default( 1.5 ); Range( 0.0, 5.0 ); UiGroup( "Preview,10/30" ); >;
	float g_flGridScale < Default( 0.02 ); Range( 0.001, 0.1 ); UiGroup( "Preview,10/40" ); >;
	float g_flGridThickness < Default( 0.04 ); Range( 0.01, 0.2 ); UiGroup( "Preview,10/50" ); >;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::From( i );

		// Fresnel — края объекта подсвечиваются как на чертеже
		float3 vViewDir = normalize( -i.vPositionWithOffsetWs.xyz );
		float flNdotV = saturate( dot( i.vNormalWs, vViewDir ) );
		float flEdge = pow( 1.0 - flNdotV, 2.5 );

		// Сетка по мировым координатам — wireframe / blueprint стиль
		float3 vWorldPos = i.vPositionWithOffsetWs.xyz + g_vCameraPositionWs.xyz;
		float3 vGrid = abs( frac( vWorldPos * g_flGridScale ) - 0.5 );
		float flGrid = 1.0 - saturate( min( min( vGrid.x, vGrid.y ), vGrid.z ) / g_flGridThickness );

		// Финальная прозрачность: тело еле видно, сетка и края яркие
		float flAlpha = g_flOpacity + flEdge * 0.4 + flGrid * 0.15;
		flAlpha = saturate( flAlpha );

		// Emission: края + сетка светятся цветом
		float flGlow = flEdge * g_flEdgeGlow + flGrid * 0.5;

		m.Albedo = 0;
		m.Emission = g_vColor * flGlow;
		m.Roughness = 1.0;
		m.Metalness = 0.0;
		m.Opacity = flAlpha;

		return ShadingModelStandard::Shade( m );
	}
}
