HEADER
{
	DevShader = true;
	Description = "Пер-объектные motion-векторы: пишет реальный экранный сдвиг пикселя с прошлого кадра (движение объекта + камеры) для velocity-буфера моушенблюра";
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

	// Ригид-дельта объекта: текущая мировая позиция точки → её мировая позиция на прошлом кадре.
	// Ставится через CommandList.Set перед отрисовкой каждой машины (вращение колёс не учитывается).
	float4x4 g_matPrevFrameDelta < Attribute( "PrevFrameDelta" ); >;

	float4 MainPs( PS_INPUT i ) : SV_Target0
	{
		float3 worldPos = i.vPositionWithOffsetWs.xyz + g_vCameraPositionWs.xyz;

		// Где эта точка была на прошлом кадре (мир) → куда её проецировала прошлая камера (экран).
		float3 prevWorldPos = mul( g_matPrevFrameDelta, float4( worldPos, 1.0 ) ).xyz;
		float2 prevSs = ReprojectFromLastFrameSs( prevWorldPos ).xy;

		float2 velocity = prevSs - i.vPositionSs.xy;

		// Линейная глубина — для сверки с depth-буфером сцены (отсечь перекрытые стеной части).
		float viewDepth = dot( g_vCameraDirWs.xyz, worldPos - g_vCameraPositionWs.xyz );

		return float4( velocity, viewDepth, 1.0 );
	}
}
