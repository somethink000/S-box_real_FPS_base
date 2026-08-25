//=========================================================================================================
// Vehicle Smoke — дешёвый билборд-дым для шин. Заточен под «мало КРУПНЫХ прозрачных частиц»:
// тяжёлый пиксель × большой overdraw убивал FPS, поэтому шейдер намеренно лёгкий.
//
// Что делает:
//   1) Билборд к камере в вершинном шейдере (ParticleModelRenderer сам не билбордит).
//   2) Мягкий круглый клуб из UV (без текстуры) — сферическая нормаль даёт фейковый объём.
//   3) ЕДИНОЕ мировое поле шума (координата = мировая позиция фрагмента, НЕ пер-частичная): соседние
//      частицы сэмплируют одну «среду», перекрытия сливаются в сплошной клуб, а не в отдельные шарики.
//      Всего 3 выборки шума на пиксель (2 на domain-warp + 1 деталь) — вместо прежних семи.
//   4) Освещение — ТОЛЬКО амбиент (тёмный ночью) + фейковый небесный градиент для формы. Цикла по
//      всем источникам сцены больше НЕТ: он был главным пожирателем кадров на прозрачном overdraw.
//   5) soft-particles: одна выборка глубины — альфа гаснет у пересечения с геометрией.
//
// Прозрачность — низкая по замыслу: настоящий шинный дым просвечивает. Плотность строится
// перекрытием крупных частиц, а не непрозрачностью каждой.
//
// Per-particle цвет/альфа приходят из ParticleModelRenderer через so.ColorTint (extraShaderData.vTint).
//=========================================================================================================

HEADER
{
	Description = "Vehicle Smoke - cheap billboarded soft particles";
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

		// Центр частицы и её равномерный масштаб (R ортонормирована → длина столбца = scale).
		float3 vOriginWs = mul( matObjectToWorld, float4( 0, 0, 0, 1 ) ).xyz;
		float flScale = length( float3( matObjectToWorld._m00, matObjectToWorld._m10, matObjectToWorld._m20 ) );

		// Сферический билборд к камере (мировой Up как референс).
		float3 vToCam  = normalize( g_vCameraPositionWs - vOriginWs );
		float3 vRight  = normalize( cross( float3( 0, 0, 1 ), vToCam ) );
		float3 vUp     = cross( vToCam, vRight );

		// Угол квада из UV (0..1 → -1..1): размер задаётся ТОЛЬКО масштабом из матрицы, модель роли не играет.
		float2 vCorner = ( v.vTexCoord.xy - 0.5 ) * 2.0;
		float3 vWorldPos = vOriginWs + ( vRight * vCorner.x - vUp * vCorner.y ) * flScale;

		o.vPositionWs = vWorldPos;
		o.vPositionPs = Position3WsToPs( vWorldPos );
		o.vPosWs = vWorldPos;

		// Per-instance тинт (so.ColorTint): rgb = цвет, a = альфа частицы.
		ExtraShaderData_t extra = GetExtraPerInstanceShaderData( v.nInstanceTransformID );
		o.vTint = extra.vTint;

		return FinalizeVertex( o );
	}
}

PS
{
	#define CUSTOM_MATERIAL_INPUTS
	#include "common/pixel.hlsl"

	// Полупрозрачность поверх сцены, без записи в глубину, без отбраковки граней.
	RenderState( CullMode, NONE );
	RenderState( DepthWriteEnable, false );
	RenderState( BlendEnable, true );
	RenderState( SrcBlend, SRC_ALPHA );
	RenderState( DstBlend, INV_SRC_ALPHA );
	RenderState( BlendOp, ADD );
	RenderState( SrcBlendAlpha, ONE );
	RenderState( DstBlendAlpha, INV_SRC_ALPHA );
	RenderState( BlendOpAlpha, ADD );

	// --- Освещение (дёшево: только амбиент + фейковая форма) ---
	float  g_flBrightness  < Default( 1.25 ); Range( 0.0, 3.0 );  UiGroup( "Lighting,20/10" ); >;
	float  g_flMinLight    < Default( 0.55 ); Range( 0.0, 1.0 );  UiGroup( "Lighting,20/20" ); >; // белый пол: дым — плотный рассеиватель, читается светлым даже в тени/тёплом свете
	// Дневной фактор из системы времени (1 днём → 0 ночью): масштабирует белый пол, иначе абсолютный
	// MinLight светит дым в темноте. Не задан (0 в атрибуте по умолчанию) → пола нет, дым по ambient.
	float  g_flDayFactor   < Attribute( "DayFactor" ); Default( 1.0 ); >;
	float  g_flLightDesat  < Default( 1.0 );  Range( 0.0, 1.0 );  UiGroup( "Lighting,20/30" ); >; // 1.0 = свет полностью обесцвечен в серый → тёплый амбиент/отскок асфальта НЕ красит дым в коричневый

	// --- Форма / прозрачность ---
	float  g_flOpacity     < Default( 0.8 );  Range( 0.0, 1.0 );   UiGroup( "Shape,30/10" ); >;  // общий потолок непрозрачности (дым всё равно чуть просвечивает)
	float  g_flEdgeSoftness< Default( 1.7 );  Range( 0.2, 4.0 );   UiGroup( "Shape,30/20" ); >;  // мягкость края (выше = пушистее)
	float  g_flSoftDepth   < Default( 20.0 ); Range( 1.0, 512.0 ); UiGroup( "Shape,30/30" ); >;  // дистанция soft-particle растворения, units

	// --- Фактура (единое мировое поле шума) ---
	float  g_flNoiseScale    < Default( 2.4 );  Range( 1.0, 8.0 ); UiGroup( "Turbulence,40/10" ); >; // размер клубов (выше = мельче)
	float  g_flNoiseStrength < Default( 0.6 );  Range( 0.0, 1.0 ); UiGroup( "Turbulence,40/20" ); >; // насколько шум лепит фактуру
	float  g_flSwirlSpeed    < Default( 0.9 );  Range( 0.0, 4.0 ); UiGroup( "Turbulence,40/30" ); >; // скорость «кипения» поля
	float  g_flSwirlAmount   < Default( 1.6 );  Range( 0.0, 6.0 ); UiGroup( "Turbulence,40/40" ); >; // сила domain-warp (закрутка клубов)
	float  g_flDissipate     < Default( 0.35 ); Range( 0.0, 1.0 ); UiGroup( "Turbulence,40/50" ); >; // с возрастом дым тает клочьями по щелям

	// Хеш и value-шум: без текстур, стабильно на любых координатах.
	float Hash21( float2 p )
	{
		p = frac( p * float2( 234.34, 435.345 ) );
		p += dot( p, p + 34.23 );
		return frac( p.x * p.y );
	}

	float ValueNoise( float2 p )
	{
		float2 i = floor( p );
		float2 f = frac( p );
		float2 u = f * f * ( 3.0 - 2.0 * f );
		float a = Hash21( i );
		float b = Hash21( i + float2( 1, 0 ) );
		float c = Hash21( i + float2( 0, 1 ) );
		float d = Hash21( i + float2( 1, 1 ) );
		return lerp( lerp( a, b, u.x ), lerp( c, d, u.x ), u.y );
	}

	// 3 октавы с поворотом между ними — ломает осевую решётку value-шума.
	float Fbm( float2 p )
	{
		const float2x2 rot = float2x2( 0.8, 0.6, -0.6, 0.8 );
		float v = 0.0;
		float amp = 0.5;
		[unroll]
		for ( int k = 0; k < 3; k++ )
		{
			v += amp * ValueNoise( p );
			p = mul( rot, p ) * 2.03 + 11.7;
			amp *= 0.5;
		}
		return v; // ~0..1
	}

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float2 uv = i.vTextureCoords.xy;
		float3 vWorldPos = i.vPosWs;

		// Радиус в квадрате частицы (0 в центре, 1 на вписанной окружности). Углы квада (r>1) режем
		// СРАЗУ — до шума и глубины: ~21% пикселей квада уходят даром, экономим на них весь расчёт.
		float2 c = uv * 2.0 - 1.0;
		float  r = length( c );
		clip( 0.999 - r );

		float4 tint = i.vTint;
		float  flAge = 1.0 - saturate( tint.a ); // альфа-кривая жизни 1→0, значит возраст растёт

		// Единое мировое поле: координата — мировая позиция фрагмента (НЕ центр частицы), поэтому
		// соседние частицы совпадают рисунком и сливаются в общий клуб. Поле медленно ползёт (кипение).
		float3 vp = vWorldPos * ( g_flNoiseScale * 0.014 );
		float2 wuv = float2( vp.x + vp.y * 0.6, vp.z + vp.y * 0.35 );
		wuv.y -= g_flTime * 0.25 * g_flSwirlSpeed;

		// Domain-warp: один шум гнёт координаты второго — клубы текучие, а не решётка. 3 Fbm всего.
		float2 warp = float2( Fbm( wuv ), Fbm( wuv + 3.7 ) ) - 0.5;
		float  n = Fbm( wuv + warp * g_flSwirlAmount * 0.4 ); // 0..1

		// Плотность: мягкий круглый спад от центра к краю, слегка промодулированный шумом. Силуэт
		// остаётся связным (шум не рвёт контур — это разваливало шлейф на клочья).
		float edge = saturate( 1.0 - r );
		edge = pow( edge, g_flEdgeSoftness );
		float density = edge * lerp( 1.0 - g_flNoiseStrength * 0.6, 1.0 + g_flNoiseStrength * 0.25, n );

		// Рассеивание: с возрастом эрозия раскрывает сначала «тонкие» места (низкий n) — дым тает
		// неравномерно, клочьями, а не общим ровным фейдом.
		density = saturate( density - g_flDissipate * flAge * ( 1.0 - n ) );

		float alpha = density * saturate( tint.a ) * g_flOpacity;
		clip( alpha - 0.003 );

		// Сферическая нормаль из UV → фейковый объём для освещения.
		float  nz = sqrt( saturate( 1.0 - r * r ) );
		float3 vToCam = normalize( g_vCameraPositionWs - vWorldPos );
		float3 vRight = normalize( cross( float3( 0, 0, 1 ), vToCam ) );
		float3 vUp    = cross( vToCam, vRight );
		float3 N = normalize( vRight * c.x - vUp * c.y + vToCam * nz );

		// Освещение: амбиент, но ПОЛНОСТЬЮ обесцвеченный в серый (Desat=1) — тёплый свет неба/отскока
		// от асфальта больше не красит дым в коричневый. Высокий белый пол (MinLight): плотный дым —
		// сильный рассеиватель, читается белым даже в тени; яркость (Brightness) добивает до белого.
		float3 amb = AmbientLight::From( vWorldPos, i.vPositionSs, N );
		float  lum = dot( amb, float3( 0.299, 0.587, 0.114 ) );
		amb = lerp( amb, lum.xxx, g_flLightDesat );
		// Белый пол только по мере дневного света: ночью g_flDayFactor→0, пол исчезает и дым
		// не светится в темноте — остаётся честный (тёмный) ambient сцены.
		amb = max( amb, g_flMinLight * g_flDayFactor );

		// Фейковая форма без выборок: верх клуба светлее (небо), низ в самозатенении; + лёгкая
		// неоднородность от того же шума (плотные места чуть темнее).
		float form  = lerp( 0.72, 1.15, saturate( N.z * 0.5 + 0.5 ) );
		float shade = lerp( 0.85, 1.1, n );
		float3 albedo = tint.rgb * amb * form * shade * g_flBrightness;

		// Soft particles: одна выборка глубины, гасим у пересечения с геометрией сцены.
		float2 vScreen = i.vPositionSs.xy;
		float3 vSceneWs = Depth::GetWorldPosition( vScreen );
		float  flSceneDist = length( vSceneWs - g_vCameraPositionWs );
		float  flFragDist  = length( vWorldPos - g_vCameraPositionWs );
		alpha *= saturate( ( flSceneDist - flFragDist ) / max( g_flSoftDepth, 0.001 ) );

		float4 result = float4( albedo, alpha );
		result = DoAtmospherics( vWorldPos, vScreen, result, false );
		return result;
	}
}
