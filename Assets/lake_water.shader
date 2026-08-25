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

	//-----------------------------------------------------------------------------
	// Аналитическая Gerstner-сумма. Используется и в VS (для смещения геометрии,
	// если меш подразделён), и в PS (для нормалей через производные).
	//-----------------------------------------------------------------------------
	struct WaveSample
	{
		float  height;       // вертикальное смещение
		float2 derivative;   // ∂h/∂xy для нормали без finite-differences
	};

	WaveSample SumGerstner( float2 p, float t, float speedScale, float ampScale )
	{
		// Domain warp — медленно искажает входное поле через sin, разрушая видимый
		// тайл-паттерн. Чейн-рул для производных опускаем (warp слабый, нормали
		// получаются «достаточно» — визуально неотличимо от точных).
		float2 warp;
		warp.x = sin( p.x * 0.0047 + p.y * 0.0061 + t * 0.31 ) * 28.0
			   + sin( p.x * 0.012  - p.y * 0.008  + t * 0.19 ) * 9.0;
		warp.y = sin( p.x * 0.0053 - p.y * 0.0049 + t * 0.27 ) * 28.0
			   + sin( p.x * 0.009  + p.y * 0.011  - t * 0.23 ) * 9.0;
		float2 pw = p + warp;

		// 6 волн. Длины — простые/иррациональные числа: LCM огромный, тайл не виден.
		// Направления распределены без симметрии, чтобы интерференция не давала «решётку».
		const float2 dirs[6] =
		{
			float2(  0.96,  0.28 ),
			float2( -0.36,  0.93 ),
			float2(  0.71, -0.71 ),
			float2(  0.18,  0.98 ),
			float2( -0.87, -0.49 ),
			float2(  0.53,  0.85 ),
		};
		const float lengths[6] = { 313.0, 179.0,  97.0,  53.0, 233.0,  71.0 };
		const float amps[6]    = {   8.5,   5.2,   2.3,   1.0,   4.1,   1.5 };
		const float speeds[6]  = {  13.0,  10.5,  16.7,   9.1,  11.9,  14.3 };
		// Фазовые сдвиги — добавочное смещение, чтобы пики разных волн не выстраивались.
		const float phases[6]  = {   0.0,   1.73,  3.14,  4.71, 2.42,  5.93 };

		WaveSample r;
		r.height = 0.0;
		r.derivative = float2( 0, 0 );

		[unroll]
		for ( int i = 0; i < 6; i++ )
		{
			float w = 6.2831853 / lengths[i];
			float phi = speeds[i] * speedScale * w;
			float a = dot( dirs[i], pw ) * w + t * phi + phases[i];
			float s = sin( a );
			float c = cos( a );
			float A = amps[i] * ampScale;

			r.height += A * s;
			r.derivative += A * w * dirs[i] * c;
		}

		return r;
	}
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
		// Смещение вершины крупным Gerstner'ом — если меш подразделён, видна 3D-волна.
		// Используем object-space XY (для плоских lake-мешей это и есть world-space).
		float2 worldXY = i.vPositionOs.xy;
		WaveSample big = SumGerstner( worldXY, g_flTime, 1.0, 1.0 );
		i.vPositionOs.z += big.height * 0.7; // в VS только крупная составляющая

		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

PS
{
	#include "common/pixel.hlsl"
	#include "common/classes/Depth.hlsl"

	#define BLEND_MODE_ALREADY_SET
	RenderState( BlendEnable, true );
	RenderState( SrcBlend, SRC_ALPHA );
	RenderState( DstBlend, INV_SRC_ALPHA );
	BoolAttribute( translucent, true );
	BoolAttribute( bWantsFBCopyTexture, true );

	CreateTexture2D( g_tFrameBufferCopyTexture ) < Attribute( "FrameBufferCopyTexture" ); SrgbRead( false ); Filter( MIN_MAG_MIP_LINEAR ); AddressU( MIRROR ); AddressV( MIRROR ); >;

	//-----------------------------------------------------------------------------
	// UI — никаких текстур: цвета и параметры на материал.
	//-----------------------------------------------------------------------------
	float3 g_vShallowColor      < Default3( 0.42, 0.78, 0.85 ); UiType( Color ); UiGroup( "Color,10/10" ); >;
	float3 g_vDeepColor         < Default3( 0.02, 0.13, 0.18 ); UiType( Color ); UiGroup( "Color,10/20" ); >;
	float3 g_vFoamColor         < Default3( 0.98, 0.99, 1.00 ); UiType( Color ); UiGroup( "Color,10/30" ); >;
	float  g_flOpacity          < Default( 0.92 );  Range( 0.0, 1.0 );   UiGroup( "Color,10/40" ); >;

	float  g_flWaveSpeed        < Default( 0.5 );   Range( 0.0, 4.0 );   UiGroup( "Wave,20/10" ); >;
	float  g_flWaveStrength     < Default( 1.0 );   Range( 0.0, 3.0 );   UiGroup( "Wave,20/20" ); >;
	float  g_flRippleScale      < Default( 0.045 ); Range( 0.005, 0.2 ); UiGroup( "Wave,20/30" ); >;
	float  g_flRippleStrength   < Default( 0.55 );  Range( 0.0, 2.0 );   UiGroup( "Wave,20/40" ); >;

	float  g_flDepthMaxDistance < Default( 280.0 ); Range( 10.0, 2000.0 ); UiGroup( "Depth,30/10" ); >;
	float  g_flDepthFalloff     < Default( 1.4 );   Range( 0.1, 5.0 );    UiGroup( "Depth,30/20" ); >;
	float  g_flRefractionStrength < Default( 0.025 ); Range( 0.0, 0.2 );  UiGroup( "Depth,30/30" ); >;

	float  g_flFoamShoreWidth     < Default( 65.0 ); Range( 0.0, 400.0 ); UiGroup( "Foam,40/10" ); >;
	float  g_flFoamCrestThreshold < Default( 0.55 ); Range( 0.0, 1.0 );   UiGroup( "Foam,40/20" ); >;
	float  g_flFoamCrestSharpness < Default( 4.0 );  Range( 1.0, 12.0 ); UiGroup( "Foam,40/30" ); >;

	float  g_flCausticsScale      < Default( 0.035 ); Range( 0.005, 0.2 ); UiGroup( "Caustics,50/10" ); >;
	float  g_flCausticsStrength   < Default( 0.85 );  Range( 0.0, 4.0 );   UiGroup( "Caustics,50/20" ); >;

	float  g_flRoughness          < Default( 0.025 );  Range( 0.0, 0.3 );  UiGroup( "Surface,60/10" ); >;
	float  g_flLodDistance        < Default( 4500.0 ); Range( 100.0, 30000.0 ); UiGroup( "Surface,60/20" ); >;

	//-----------------------------------------------------------------------------
	// Лёгкий 2D value-noise + FBM для высокочастотной ряби (поверх Gerstner'а).
	//-----------------------------------------------------------------------------
	float Hash21( float2 p )
	{
		p = frac( p * float2( 123.34, 456.21 ) );
		p += dot( p, p + 45.32 );
		return frac( p.x * p.y );
	}

	float ValueNoise2D( float2 p )
	{
		float2 i = floor( p );
		float2 f = frac( p );
		f = f * f * ( 3.0 - 2.0 * f );

		float a = Hash21( i );
		float b = Hash21( i + float2( 1, 0 ) );
		float c = Hash21( i + float2( 0, 1 ) );
		float d = Hash21( i + float2( 1, 1 ) );

		return lerp( lerp( a, b, f.x ), lerp( c, d, f.x ), f.y );
	}

	// FBM рябь с возвратом производной для перпендикулярного возмущения нормали.
	float2 RippleDerivative( float2 p, float t, int octaves, float strength )
	{
		float2 grad = float2( 0, 0 );
		float amp = strength;
		float freq = 1.0;
		const float eps = 0.5;

		[loop]
		for ( int k = 0; k < octaves; k++ )
		{
			float2 q = p * freq + float2( t * 0.7, -t * 0.5 ) * ( 0.6 + k * 0.2 );
			// Центральная разность для производной — дешевле аналитики у value-noise.
			float h0 = ValueNoise2D( q );
			float hx = ValueNoise2D( q + float2( eps, 0 ) );
			float hy = ValueNoise2D( q + float2( 0, eps ) );
			grad += amp * float2( hx - h0, hy - h0 ) * freq / eps;

			amp *= 0.5;
			freq *= 2.13;
		}

		return grad;
	}

	//-----------------------------------------------------------------------------
	// Каустика: 2-октавный «отдалённо-сетчатый» паттерн.
	// Используется на ДНЕ (видно через прозрачность) — не на поверхности.
	//-----------------------------------------------------------------------------
	float Caustics( float2 worldXY, float t )
	{
		float2 p = worldXY * g_flCausticsScale;
		float a = sin( p.x + t * 0.5 ) + sin( p.y - t * 0.4 );
		float b = sin( ( p.x + p.y ) * 0.72 + t * 0.6 )
				+ sin( ( p.x - p.y ) * 0.83 - t * 0.45 );
		float c = pow( saturate( ( a + b ) * 0.25 + 0.5 ), 3.0 );
		return c;
	}

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float3 vWorldPos = g_vCameraPositionWs + i.vPositionWithOffsetWs.xyz;
		float2 worldXY = vWorldPos.xy;
		float t = g_flTime;

		// Дистанция до камеры — LOD по высокочастотной ряби (далеко = без неё).
		float camDist = length( i.vPositionWithOffsetWs.xyz );
		float lod = saturate( 1.0 - camDist / g_flLodDistance );
		int rippleOct = lod > 0.55 ? 3 : ( lod > 0.2 ? 2 : 1 );

		//---------------------------------------------------------------
		// Нормаль: аналитические производные Gerstner'а + рябь FBM.
		//---------------------------------------------------------------
		WaveSample big = SumGerstner( worldXY, t, g_flWaveSpeed, g_flWaveStrength );
		float2 grad = big.derivative;

		// Рябь — масштабирована через g_flRippleScale.
		grad += RippleDerivative( worldXY * g_flRippleScale, t * g_flWaveSpeed, rippleOct, g_flRippleStrength ) * lod;

		// Tangent-space нормаль из градиента: N = normalize( -dx, -dy, 1 )
		float3 nTangent = normalize( float3( -grad.x, -grad.y, 1.0 ) );

		// В мировое пространство через TBN из мешa.
		float3 vNormal = normalize(
			nTangent.x * i.vTangentUWs +
			nTangent.y * i.vTangentVWs +
			nTangent.z * i.vNormalWs );

		//---------------------------------------------------------------
		// Глубина воды в юнитах.
		//---------------------------------------------------------------
		float flSceneLinearDepth = Depth::GetLinear( i.vPositionSs.xy );
		float flWaterLinearDepth = Depth::Linearize( Depth::Normalize( i.vPositionSs.z ), i.vPositionSs.xy );
		float flWaterThickness = max( flSceneLinearDepth - flWaterLinearDepth, 0.0 );
		float flDepthFactor = pow( saturate( flWaterThickness / g_flDepthMaxDistance ), g_flDepthFalloff );

		//---------------------------------------------------------------
		// Цвет воды: depth gradient между мелким и глубоким.
		//---------------------------------------------------------------
		float3 vColor = lerp( g_vShallowColor, g_vDeepColor, flDepthFactor );

		// SSS-подсветка просвечивающей толщи у самой кромки.
		float shoreT = saturate( flWaterThickness / max( g_flFoamShoreWidth, 1.0 ) );
		vColor = lerp( vColor * 1.25, vColor, shoreT );

		//---------------------------------------------------------------
		// Френель.
		//---------------------------------------------------------------
		float3 vViewDir = normalize( -i.vPositionWithOffsetWs.xyz );
		float ndv = saturate( dot( vNormal, vViewDir ) );
		float flFresnel = pow( 1.0 - ndv, 5.0 );

		//---------------------------------------------------------------
		// Пена: на гребнях волн + на берегу.
		//---------------------------------------------------------------
		// Crest foam — гребни. Нормируем по сумме амплитуд (~22.6 для 6 волн).
		float crestNorm = saturate( big.height / 22.6 * 0.5 + 0.5 );
		float crest = smoothstep( g_flFoamCrestThreshold, 1.0, crestNorm );
		crest = pow( crest, g_flFoamCrestSharpness );

		// Shoreline foam — у границы с геометрией.
		float shoreFoam = ( 1.0 - shoreT ) * ( 1.0 - shoreT );

		// Шумовой паттерн поверх пены (не прямоугольная).
		float foamNoise = ValueNoise2D( worldXY * 0.08 + t * 0.7 );
		float foam = saturate( max( crest, shoreFoam * 0.9 ) * ( 0.55 + 0.55 * foamNoise ) );

		vColor = lerp( vColor, g_vFoamColor, foam );

		//---------------------------------------------------------------
		// Прозрачность.
		//---------------------------------------------------------------
		float flAlpha = lerp( 0.18, g_flOpacity, flDepthFactor );
		flAlpha = lerp( flAlpha, 1.0, flFresnel * 0.6 );
		flAlpha = saturate( max( flAlpha, foam ) );

		//---------------------------------------------------------------
		// Каустика на дне (видна через прозрачность, на мелководье ярче).
		//---------------------------------------------------------------
		float caust = Caustics( worldXY, t ) * ( 1.0 - flDepthFactor ) * g_flCausticsStrength;
		float3 causticsColor = float3( 0.95, 1.0, 0.9 ) * caust * ( 1.0 - foam );

		//---------------------------------------------------------------
		// Рефракция через framebuffer copy.
		//---------------------------------------------------------------
		float2 vScreenUV = i.vPositionSs.xy * rcp( g_vViewportSize.xy );
		float2 vRefractUV = mad(
			vScreenUV,
			g_vFrameBufferCopyInvSizeAndUvScale.zw,
			nTangent.xy * g_flRefractionStrength * g_vFrameBufferCopyInvSizeAndUvScale.zw );
		float3 vRefract = Tex2D( g_tFrameBufferCopyTexture, vRefractUV ).rgb;

		//---------------------------------------------------------------
		// Материал в движок.
		//---------------------------------------------------------------
		Material m = Material::From( i );
		m.Albedo = vColor;
		m.Normal = vNormal;
		// Пена слегка матовая, гладь — почти зеркальная.
		m.Roughness = lerp( g_flRoughness, 0.35, foam );
		m.Metalness = 0.0;
		m.AmbientOcclusion = 1.0;
		m.Opacity = flAlpha;
		m.Transmission = float3( 1, 1, 1 ) * ( 1.0 - flAlpha );

		// Просвечивающая толща + каустика на дне.
		m.Emission = vRefract * ( 1.0 - flFresnel ) * ( 1.0 - flAlpha ) + causticsColor;

		return ShadingModelStandard::Shade( m );
	}
}
