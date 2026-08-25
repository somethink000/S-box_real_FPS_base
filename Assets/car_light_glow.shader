//=========================================================================================================================
// LonePine Car Light Glow — простой эмиссивный шейдер для glow-мешей фар.
// Вкл/выкл через body groups (lowbeam / highbeam). Цвет и яркость задаются в .vmat.
//=========================================================================================================================
HEADER
{
	Description = "LonePine Car Light Glow";
	Version = 1;
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	Forward();
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

	CreateInputTexture2D( TextureColor, Srgb, 8, "", "_color", "Material,10/10", Default3( 1.0, 1.0, 1.0 ) );
	CreateTexture2D( g_tColor ) < Channel( RGBA, Box( TextureColor ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;

	float3 g_vGlowColor < UiType( Color ); Default3( 1.0, 0.95, 0.85 ); UiGroup( "Glow,20/10" ); >;
	float g_flGlowIntensity < Default( 8.0 ); Range( 0.0, 100.0 ); UiGroup( "Glow,20/20" ); >;
	float g_flGlobalEmissionBoost < Default( 1.0 ); Range( 0.0, 100.0 ); UiGroup( "Glow,20/30" ); >;

	SamplerState g_sSampler < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init( i );

		float2 uv = i.vTextureCoords.xy;
		float3 albedo = Tex2DS( g_tColor, g_sSampler, uv ).rgb;
		float luminance = dot( albedo, float3( 0.299, 0.587, 0.114 ) );

		m.Albedo = albedo * 0.1;
		m.Normal = float3( 0.0, 0.0, 1.0 );
		m.Roughness = 1.0;
		m.Metalness = 0.0;
		m.Emission = g_vGlowColor * g_flGlowIntensity * g_flGlobalEmissionBoost * max( luminance, 0.05 );
		m.Opacity = 1.0;

		return ShadingModelStandard::Shade( m );
	}
}
