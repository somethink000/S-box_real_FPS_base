//=========================================================================================================
// Exhaust Heat Haze — билборд-частица горячего выхлопа: НЕ рисует цвет, а ПРЕЛОМЛЯЕТ кадр за собой
// (framebuffer copy, как lake_water/glass). Струя у среза трубы — только дрожание воздуха; дальше
// частицы дымки умирают и остаётся обычный дым (vehicle_smoke) — так получается градация «воздух → дым».
//
// Билборд и per-particle тинт — тот же приём, что vehicle_smoke.shader (ParticleModelRenderer сам не
// билбордит). Сила преломления растёт к центру клуба (сферический профиль) и гаснет по альфе частицы.
//=========================================================================================================

HEADER
{
	Description = "Exhaust heat haze - refractive billboard particle";
	DevShader = true;
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	Forward();
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
	float4 vTint : COLOR0;
	float3 vPosWs : TEXCOORD14;
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput o = ProcessVertex( v );

		float3x4 matObjectToWorld = GetTransformMatrix( v.nInstanceTransformID );

		float3 vOriginWs = mul( matObjectToWorld, float4( 0, 0, 0, 1 ) ).xyz;
		float flScale = length( float3( matObjectToWorld._m00, matObjectToWorld._m10, matObjectToWorld._m20 ) );

		float3 vToCam  = normalize( g_vCameraPositionWs - vOriginWs );
		float3 vRight  = normalize( cross( float3( 0, 0, 1 ), vToCam ) );
		float3 vUp     = cross( vToCam, vRight );

		float2 vCorner = ( v.vTexCoord.xy - 0.5 ) * 2.0;
		float3 vWorldPos = vOriginWs + ( vRight * vCorner.x - vUp * vCorner.y ) * flScale;

		o.vPositionWs = vWorldPos;
		o.vPositionPs = Position3WsToPs( vWorldPos );
		o.vPosWs = vWorldPos;

		ExtraShaderData_t extra = GetExtraPerInstanceShaderData( v.nInstanceTransformID );
		o.vTint = extra.vTint;

		return FinalizeVertex( o );
	}
}

PS
{
	#define CUSTOM_MATERIAL_INPUTS
	#include "common/pixel.hlsl"

	// Без этих атрибутов движок НЕ делает копию кадра для этого дро-колла —
	// в g_tFrameBufferCopyTexture приезжает заглушка «missing texture».
	BoolAttribute( translucent, true );
	BoolAttribute( bWantsFBCopyTexture, true );

	// Полупрозрачный бленд: результат = преломлённый кадр × density поверх сцены. Глубина не пишется.
	RenderState( CullMode, NONE );
	RenderState( DepthWriteEnable, false );
	RenderState( BlendEnable, true );
	RenderState( SrcBlend, SRC_ALPHA );
	RenderState( DstBlend, INV_SRC_ALPHA );
	RenderState( BlendOp, ADD );

	CreateTexture2D( g_tFrameBufferCopyTexture ) < Attribute( "FrameBufferCopyTexture" ); SrgbRead( false ); Filter( MIN_MAG_MIP_LINEAR ); AddressU( MIRROR ); AddressV( MIRROR ); >;

	float g_flRefractionStrength < Default( 0.014 ); Range( 0.0, 0.08 ); UiGroup( "Haze,10/10" ); >; // смещение выборки кадра, доли экрана
	float g_flEdgeSoftness      < Default( 1.6 );   Range( 0.2, 4.0 );  UiGroup( "Haze,10/20" ); >;  // спад плотности к краю клуба
	float g_flWobbleSpeed       < Default( 9.0 );   Range( 0.0, 30.0 ); UiGroup( "Haze,10/30" ); >;  // частота дрожания горячего воздуха
	float g_flSoftDepth         < Default( 14.0 );  Range( 1.0, 128.0 ); UiGroup( "Haze,10/40" ); >; // soft-particle растворение у геометрии

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float2 uv = i.vTextureCoords.xy;
		float4 tint = i.vTint;
		float3 vWorldPos = i.vPosWs;

		// Сферический профиль клуба: центр плотный, край — ноль (шов с фоном невидим, offset→0).
		float2 c  = uv * 2.0 - 1.0;
		float  r  = length( c );
		float  nz = sqrt( saturate( 1.0 - r * r ) );
		float density = pow( saturate( 1.0 - r ), g_flEdgeSoftness ) * tint.a;

		// Soft particles: у пересечения с геометрией дрожание сходит на нет.
		float2 vScreen = i.vPositionSs.xy;
		float3 vSceneWs = Depth::GetWorldPosition( vScreen );
		float  flSceneDist = length( vSceneWs - g_vCameraPositionWs );
		float  flFragDist  = length( vWorldPos - g_vCameraPositionWs );
		density *= saturate( ( flSceneDist - flFragDist ) / max( g_flSoftDepth, 0.001 ) );

		clip( density - 0.004 );

		// Дрожание горячего воздуха: радиальное «линзирование» к центру, промодулированное временем
		// и мировой позицией (соседние клубы дрожат не в фазе).
		float wobble = 0.7 + 0.3 * sin( g_flTime * g_flWobbleSpeed + vWorldPos.z * 0.35 + vWorldPos.x * 0.21 );
		float2 vOffs = c * nz * wobble * g_flRefractionStrength * density;

		float2 vScreenUV = vScreen * rcp( g_vViewportSize.xy );
		float2 vRefractUV = mad( vScreenUV, g_vFrameBufferCopyInvSizeAndUvScale.zw, vOffs * g_vFrameBufferCopyInvSizeAndUvScale.zw );
		float3 vRefract = Tex2D( g_tFrameBufferCopyTexture, vRefractUV ).rgb;

		return float4( vRefract, saturate( density ) );
	}
}
