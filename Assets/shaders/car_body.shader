
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	VrForward();
	Depth(); 
	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsWireframe( "vr_tools_wireframe.shader" );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 1
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;

		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );

		return FinalizeVertex( i );
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler < Filter( POINT ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Color, Srgb, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Mask, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( Tint, Linear, 8, "None", "_tint", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );

	Texture2D g_tColor < Channel( RGBA, Box( Color ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tAlphaMask < Channel( RGBA, Box( Mask ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	Texture2D g_tTintMask < Channel( RGBA, Box( Tint ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	float g_flRoughness < UiType( Slider ); UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;
	float g_flMetalness < UiType( Slider ); UiGroup( ",0/,0/0" ); Default1( 0 ); Range1( 0, 1 ); >;

	Texture2DArray g_tReflection < Attribute( "Reflection" ); Channel( RGBA, Box( Color ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	bool g_bUseReflection < Attribute( "UseReflection" ); >;
	
	float4 g_vTint < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1, 1, 1, 1 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float flAlpha = Tex2DS( g_tAlphaMask, g_sSampler, i.vTextureCoords.xy ).r;
		float flTintMask = Tex2DS( g_tTintMask, g_sSampler, i.vTextureCoords.xy ).r;

		float4 vTint = g_vTint;
		vTint *= i.vVertexColor;

		float4 vColor = Tex2DS( g_tColor, g_sSampler, i.vTextureCoords.xy );
		vColor = lerp( vColor, vColor * vTint, flTintMask );

		if ( flAlpha < 0.5f )
			discard;

		Material m = Material::Init();
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = g_flRoughness;
		m.Metalness = g_flMetalness;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		m.Albedo = vColor.xyz;
		m.Opacity = flAlpha;
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );

		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
		m.TextureCoords = i.vTextureCoords.xy;		
		
		float4 vShaded = ShadingModelStandard::Shade( i, m );

		if ( g_bUseReflection )
		{
			float3 vReflection = reflect( -g_vCameraDirWs, i.vNormalWs );
			float3 vReflectionColor = g_tReflection.Sample( g_sSampler, float3( vReflection.xy, 0 ) ).rgb;
			vShaded.rgb = lerp( vShaded.rgb, vReflectionColor, vShaded.a );
		}

		return lerp( vShaded, vColor, .5 );
	}
}
