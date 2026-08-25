//=========================================================================================================
// Tire Smoke Volume — ОБЪЁМНЫЙ дым (raymarch), НЕ партиклы. Один меш-куб на машину: пиксельный шейдер
// марширует луч сквозь коробку и накапливает плотность процедурного 3D-поля. Непрерывный клубящийся
// дым без «точек» отдельных спрайтов; привязан к колёсам, самозатеняется, тает по возрасту.
//
// Плотность = СУММА метабол-«кистей» в точках недавнего контакта колёс (SmokePtN: xyz мир, w
// интенсивность от drift-heat) × 3D value-шум (клубы). Точки живут в МИРЕ и стареют в C# — поэтому
// свежий дым «болтается» там, где нарисован, пока машина уезжает: получается реальный шлейф/зависание.
//
// Освещение: белая база (дым — рассеиватель) + короткий self-shadow вверх по Бер-Ламберту (ядра
// темнее, верх/кромки светлее) → объём без «свечения». Цвет держим НЕЙТРАЛЬНО-белым принудительно.
//
// Референс марша/шума — gen_sky.shader (облака). Шаги capped, ранний выход по прозрачности → FPS ок.
//=========================================================================================================

HEADER
{
	Description = "Tire Smoke Volume - raymarched volumetric smoke";
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
	float3 vPosWs : TEXCOORD14;
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput o = ProcessVertex( v );
		o.vPosWs = o.vPositionWs.xyz;
		return FinalizeVertex( o );
	}
}

PS
{
	#define CUSTOM_MATERIAL_INPUTS
	#include "common/pixel.hlsl"

	// Прозрачный дым поверх сцены. Глубина НЕ пишется и НЕ тестируется — окклюзию геометрией делаем
	// в шейдере (clamp марша к глубине сцены), иначе задние грани коробки резались бы неверно.
	RenderState( CullMode, FRONT );        // рендерим ЗАДНИЕ грани → фрагмент есть даже когда камера внутри коробки
	RenderState( DepthWriteEnable, false );
	RenderState( DepthEnable, false );
	RenderState( BlendEnable, true );
	RenderState( SrcBlend, SRC_ALPHA );
	RenderState( DstBlend, INV_SRC_ALPHA );
	RenderState( BlendOp, ADD );
	RenderState( SrcBlendAlpha, ONE );
	RenderState( DstBlendAlpha, INV_SRC_ALPHA );
	RenderState( BlendOpAlpha, ADD );

	// --- Пер-объектные данные (renderer.Attributes из C#) ---
	float  g_flSmokeCount  < Attribute( "SmokeCount" );  Default( 0.0 ); >;   // сколько активных точек (0 → drop)
	float4 g_vSmokePt0 < Attribute( "SmokePt0" ); Default4( 0,0,0,0 ); >;      // xyz мир, w интенсивность(0..1)
	float4 g_vSmokePt1 < Attribute( "SmokePt1" ); Default4( 0,0,0,0 ); >;
	float4 g_vSmokePt2 < Attribute( "SmokePt2" ); Default4( 0,0,0,0 ); >;
	float4 g_vSmokePt3 < Attribute( "SmokePt3" ); Default4( 0,0,0,0 ); >;
	float4 g_vSmokePt4 < Attribute( "SmokePt4" ); Default4( 0,0,0,0 ); >;
	float4 g_vSmokePt5 < Attribute( "SmokePt5" ); Default4( 0,0,0,0 ); >;
	float4 g_vSmokePt6 < Attribute( "SmokePt6" ); Default4( 0,0,0,0 ); >;
	float4 g_vSmokePt7 < Attribute( "SmokePt7" ); Default4( 0,0,0,0 ); >;
	float3 g_vSmokeBoxMin < Attribute( "SmokeBoxMin" ); Default3( -100,-100,0 ); >; // мировой AABB для клампа марша
	float3 g_vSmokeBoxMax < Attribute( "SmokeBoxMax" ); Default3(  100, 100, 200 ); >;
	float3 g_vSmokeScroll < Attribute( "SmokeScroll" ); Default3( 0,0,8 ); >;     // адвекция шума (юниты/с): ветер/подъём
	float3 g_vSmokeColor  < Attribute( "SmokeColor" );  Default3( 0.95,0.95,0.96 ); >; // база (держим белым)
	float  g_flSmokeBright < Attribute( "SmokeBright" ); Default( 1.0 ); >;       // дневная/ночная яркость (из C#)

	// --- Тюнинг формы (в .vmat) ---
	float g_flPuffRadius   < Default( 46.0 ); Range( 10.0, 160.0 ); UiGroup( "Shape,10/10" ); >; // радиус кисти у контакта
	float g_flRiseHeight   < Default( 95.0 ); Range( 20.0, 300.0 ); UiGroup( "Shape,10/20" ); >; // на сколько поднимается столб
	float g_flSpread       < Default( 0.8 );  Range( 0.0, 2.0 );    UiGroup( "Shape,10/30" ); >;  // расширение кверху
	float g_flDensity      < Default( 1.5 );  Range( 0.1, 6.0 );    UiGroup( "Shape,10/40" ); >;  // общая насыщенность
	float g_flNoiseScale   < Default( 0.03 ); Range( 0.005, 0.12 ); UiGroup( "Noise,20/10" ); >;  // размер клубов (1/юниты)
	float g_flNoiseStrength< Default( 0.85 ); Range( 0.0, 1.0 );    UiGroup( "Noise,20/20" ); >;  // насколько шум ест плотность
	float g_flStepScale    < Default( 1.0 );  Range( 0.5, 2.0 );    UiGroup( "Quality,30/10" ); >; // множитель длины шага (перф)

	static const int   MARCH_STEPS = 28;   // потолок шагов основного марша
	static const int   LIGHT_STEPS = 3;    // шаги self-shadow вверх
	static const float EXT_COEF     = 0.06; // поглощение на юнит при density 1

	// value-шум 3D (как gen_sky): дёшево, без текстур.
	float Hash13( float3 p )
	{
		p = frac( p * 0.3183099 + 0.1 );
		p *= 17.0;
		return frac( p.x * p.y * p.z * ( p.x + p.y + p.z ) );
	}

	float ValueNoise3D( float3 p )
	{
		float3 i = floor( p );
		float3 f = frac( p );
		f = f * f * ( 3.0 - 2.0 * f );
		float n000 = Hash13( i + float3( 0, 0, 0 ) );
		float n100 = Hash13( i + float3( 1, 0, 0 ) );
		float n010 = Hash13( i + float3( 0, 1, 0 ) );
		float n110 = Hash13( i + float3( 1, 1, 0 ) );
		float n001 = Hash13( i + float3( 0, 0, 1 ) );
		float n101 = Hash13( i + float3( 1, 0, 1 ) );
		float n011 = Hash13( i + float3( 0, 1, 1 ) );
		float n111 = Hash13( i + float3( 1, 1, 1 ) );
		float x00 = lerp( n000, n100, f.x );
		float x10 = lerp( n010, n110, f.x );
		float x01 = lerp( n001, n101, f.x );
		float x11 = lerp( n011, n111, f.x );
		return lerp( lerp( x00, x10, f.y ), lerp( x01, x11, f.y ), f.z );
	}

	// FBM 3 октавы → «клубы».
	float Fbm3( float3 p )
	{
		float v = 0.55 * ValueNoise3D( p );
		v += 0.28 * ValueNoise3D( p * 2.03 + 19.19 );
		v += 0.14 * ValueNoise3D( p * 4.07 + 7.13 );
		return v; // ~0..1
	}

	// Вклад одной кисти: плотный купол у пятна контакта, поднимается и ширится кверху, гаснет к верху.
	float PuffAt( float3 p, float4 pt )
	{
		if ( pt.w <= 0.001 ) return 0.0;
		float h = p.z - pt.z;                                   // высота над контактом
		if ( h < -8.0 || h > g_flRiseHeight ) return 0.0;
		float hN = saturate( h / g_flRiseHeight );
		float radius = g_flPuffRadius * ( 0.55 + g_flSpread * hN ); // конус кверху
		float dh = length( p.xy - pt.xy );
		float radial = saturate( 1.0 - dh / max( radius, 1.0 ) );
		float vert   = saturate( 1.0 - hN ) * saturate( hN * 6.0 + 0.15 ); // низ плотный, верх сходит, у самой земли чуть мягче
		return pt.w * radial * radial * vert;
	}

	// Плотность поля в точке: сумма кистей × шум.
	float SampleDensity( float3 p, int count )
	{
		float foot = 0.0;
		foot += PuffAt( p, g_vSmokePt0 );
		if ( count > 1 ) foot += PuffAt( p, g_vSmokePt1 );
		if ( count > 2 ) foot += PuffAt( p, g_vSmokePt2 );
		if ( count > 3 ) foot += PuffAt( p, g_vSmokePt3 );
		if ( count > 4 ) foot += PuffAt( p, g_vSmokePt4 );
		if ( count > 5 ) foot += PuffAt( p, g_vSmokePt5 );
		if ( count > 6 ) foot += PuffAt( p, g_vSmokePt6 );
		if ( count > 7 ) foot += PuffAt( p, g_vSmokePt7 );
		foot = saturate( foot );
		if ( foot <= 0.001 ) return 0.0;

		// Шум клубит и рвёт края; смещаем координату во времени (адвекция вверх/по ветру).
		float3 np = p * g_flNoiseScale + g_vSmokeScroll * ( g_flTime * g_flNoiseScale );
		float n = Fbm3( np );
		float clumps = lerp( 1.0, n * 1.6, g_flNoiseStrength ); // 0.85 str → плотные комки и полупрогалы
		return foot * clumps * g_flDensity;
	}

	// Пересечение луча с мировым AABB.
	float2 IntersectAABB( float3 ro, float3 rd, float3 bmin, float3 bmax )
	{
		float3 inv = 1.0 / rd;
		float3 ta = ( bmin - ro ) * inv;
		float3 tb = ( bmax - ro ) * inv;
		float3 tmin = min( ta, tb );
		float3 tmax = max( ta, tb );
		float tnear = max( max( tmin.x, tmin.y ), tmin.z );
		float tfar  = min( min( tmax.x, tmax.y ), tmax.z );
		return float2( tnear, tfar );
	}

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		int count = (int)( g_flSmokeCount + 0.5 );
		if ( count <= 0 ) discard;

		float3 ro = g_vCameraPositionWs;
		float3 rd = normalize( i.vPosWs - ro );

		float2 t = IntersectAABB( ro, rd, g_vSmokeBoxMin, g_vSmokeBoxMax );
		float tnear = max( t.x, 0.0 );   // если камера внутри — стартуем от неё
		float tfar  = t.y;
		if ( tfar <= tnear ) discard;

		// Окклюзия геометрией сцены: не марширем дальше твёрдой поверхности.
		float2 vScreen = i.vPositionSs.xy;
		float3 vSceneWs = Depth::GetWorldPosition( vScreen );
		float  flSceneDist = length( vSceneWs - ro );
		tfar = min( tfar, flSceneDist );
		if ( tfar <= tnear ) discard;

		// Джиттер старта по screen-хешу — прячет «слоистость» марша без лишних шагов.
		float jitter = frac( Hash13( float3( vScreen, g_flTime ) ) );

		float span = tfar - tnear;
		float stepLen = max( span / (float)MARCH_STEPS, 1.0 ) * g_flStepScale;
		int   steps = (int)min( span / stepLen + 1.0, (float)MARCH_STEPS );

		// Направление к небу для self-shadow (плюс лёгкий наклон, чтоб не совпадать с осью шума).
		float3 lightDir = normalize( float3( 0.15, 0.1, 1.0 ) );
		float  lightStep = g_flRiseHeight / (float)( LIGHT_STEPS + 1 );

		float3 skyCol = g_vSmokeColor * g_flSmokeBright;

		float transmittance = 1.0;
		float3 scatter = 0.0;

		[loop]
		for ( int s = 0; s < steps; s++ )
		{
			float dist = tnear + ( (float)s + jitter ) * stepLen;
			float3 p = ro + rd * dist;

			float density = SampleDensity( p, count );
			if ( density > 0.002 )
			{
				// Self-shadow: короткий марш к небу — сколько дыма над этой точкой.
				float shadowDens = 0.0;
				[unroll]
				for ( int l = 1; l <= LIGHT_STEPS; l++ )
				{
					float3 sp = p + lightDir * ( lightStep * (float)l );
					shadowDens += SampleDensity( sp, count );
				}
				float shadow = exp( -shadowDens * EXT_COEF * lightStep );

				// Верх клуба ещё и от неба светлее (градиент по высоте в коробке).
				float hGrad = saturate( ( p.z - g_vSmokeBoxMin.z ) / max( g_vSmokeBoxMax.z - g_vSmokeBoxMin.z, 1.0 ) );
				float3 lit = skyCol * lerp( 0.55, 1.15, shadow ) * lerp( 0.8, 1.1, hGrad );

				float dT = exp( -density * EXT_COEF * stepLen );
				// front-to-back накопление in-scatter
				scatter += transmittance * ( 1.0 - dT ) * lit;
				transmittance *= dT;
				if ( transmittance < 0.02 ) break; // ранний выход — почти непрозрачно
			}
		}

		float alpha = saturate( 1.0 - transmittance );
		if ( alpha < 0.004 ) discard;

		float4 result = float4( scatter, alpha );
		result = DoAtmospherics( i.vPosWs, vScreen, result, false );
		return result;
	}
}
