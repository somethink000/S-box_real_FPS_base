HEADER
{
	DevShader = true;
	Description = "Day/Night sky — lite atmosphere, sun/moon, stars";
	Version = 1;
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
MODES
{
	Forward();
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
FEATURES
{
	#include "vr_common_features.fxc"
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
COMMON
{
	#include "system.fxc"
	#include "vr_common.fxc"

	// Roll вокруг Forward (X) — та же ось, что SunOrbit.WithRoll у луны/солнца.
	float3 RotateDirectionX( float3 dir, float angle )
	{
		float c = cos( angle );
		float s = sin( angle );
		return float3( dir.x, c * dir.y + s * dir.z, -s * dir.y + c * dir.z );
	}
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
struct VS_INPUT
{
	float4 vPositionOs : POSITION < Semantic( PosXyz ); >;
};

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
struct PS_INPUT
{
	float3 vPositionWs : TEXCOORD1;

	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs	: SV_Position;
	#endif
	#if ( PROGRAM == VFX_PROGRAM_PS )
		float4 vPositionSs : SV_Position;
	#endif
};

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
VS
{
	#define IS_SPRITECARD 1
	#include "system.fxc"

	PS_INPUT MainVs( const VS_INPUT i )
	{
		PS_INPUT o;
		float flSkyboxScale = g_flNearPlane + g_flFarPlane;
		float3 vPositionWs = g_vCameraPositionWs.xyz + i.vPositionOs.xyz * flSkyboxScale;

		o.vPositionPs = Position3WsToPs( vPositionWs );
		o.vPositionWs = vPositionWs;

		return o;
	}
}

//-------------------------------------------------------------------------------------------------------------------------------------------------------------
PS
{
	#include "vr_lighting.fxc"
	#include "volumetric_fog.fxc"
	#include "common/Shadow.hlsl"

	RenderState( CullMode, NONE );
	RenderState( DepthWriteEnable, false );
	RenderState( DepthEnable, true );
	RenderState( DepthFunc, GREATER_EQUAL );

	BoolAttribute( sky, true );

	struct PS_OUTPUT
	{
		float4 vColor0 : SV_Target0;
	};

	float g_flStarRotation < Default( 0.0 ); Range( 0.0, 6.283185 ); UiGroup( "Stars,20/10" ); >;
	float g_flStarsIntensity < Default( 1.0 ); Range( 0.0, 5.0 ); UiGroup( "Stars,20/20" ); >;
	float g_flStarScale < Default( 0.3 ); Range( 0.05, 1.0 ); UiGroup( "Stars,20/30" ); >;

	float g_flNightStart < Default( 0.08 ); Range( -1.0, 1.0 ); UiGroup( "Day/Night,30/10" ); >;
	float g_flNightEnd < Default( -0.18 ); Range( -1.0, 1.0 ); UiGroup( "Day/Night,30/20" ); >;
	float g_flDayIntensity < Default( 1.0 ); Range( 0.0, 2.0 ); UiGroup( "Day/Night,30/30" ); >;
	float g_flNightFactor < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "Day/Night,30/40" ); >;
	float3 g_vNightSkyColor < Default3( 0.02, 0.03, 0.07 ); UiType( Color ); UiGroup( "Day/Night,30/50" ); >;

	float3 g_vMoonColor < Default3( 0.82, 0.85, 0.92 ); UiType( Color ); UiGroup( "Moon,40/20" ); >;
	float g_flMoonIntensity < Default( 2.5 ); Range( 0.0, 10.0 ); UiGroup( "Moon,40/30" ); >;
	float g_flMoonGlow < Default( 0.35 ); Range( 0.0, 2.0 ); UiGroup( "Moon,40/40" ); >;

	// Weather: задаётся из C# (GenSkyBox2D / WeatherController) во время грозы.
	// Высокие облака (тучи): near-2D слой на куполе, проекция луча на плоскость.
	float  g_flCloudCoverage < Default( 0.45 ); Range( 0.0, 1.0 ); UiGroup( "Clouds,50/10" ); >;
	float  g_flCloudScale    < Default( 2.5 );  Range( 0.3, 8.0 ); UiGroup( "Clouds,50/20" ); >;
	float  g_flCloudOpacity  < Default( 0.9 );  Range( 0.0, 1.0 ); UiGroup( "Clouds,50/30" ); >;
	float  g_flCloudSoftness < Default( 0.5 );  Range( 0.0, 1.0 ); UiGroup( "Clouds,50/35" ); >;
	float2 g_vCloudWind      < Default2( 1.0, 0.6 ); UiGroup( "Clouds,50/40" ); >;
	// Накопленный ветровой офсет деки (мировые юниты), пишет C# (GenSkyBox2D) — скорость может
	// меняться на лету (в грозу быстрее) без телепорта облачного поля, в отличие от wind*g_flTime.
	float2 g_vCloudOffset    < Default2( 0.0, 0.0 ); UiGroup( "Clouds,50/42" ); >;
	float3 g_vCloudColor     < Default3( 0.95, 0.96, 1.0 ); UiType( Color ); UiGroup( "Clouds,50/50" ); >;
	float3 g_vCloudShadow    < Default3( 0.34, 0.39, 0.50 ); UiType( Color ); UiGroup( "Clouds,50/60" ); >;

	// Объёмные облака (raymarch в скайбоксе, порт pp_clouds): дека между Base и Base+Thickness в мире.
	float g_flCloudBase      < Default( 8000.0 ); Range( 500.0, 60000.0 ); UiGroup( "Clouds,50/12" ); >;
	float g_flCloudThickness < Default( 4000.0 ); Range( 500.0, 20000.0 ); UiGroup( "Clouds,50/14" ); >;
	float g_flCloudDensity   < Default( 1.0 );    Range( 0.1, 4.0 );      UiGroup( "Clouds,50/16" ); >;
	float g_flCloudSunBoost  < Default( 1.0 );    Range( 0.0, 4.0 );      UiGroup( "Clouds,50/55" ); >;

	// Глобальный рантайм-тумблер объёмных облаков (C#: Scene.RenderAttributes.Set("VolumetricClouds", 0f/1f)).
	// 0 — raymarch облаков ПОЛНОСТЬЮ пропускается (экономия GPU), небо чистое. Настройка графики клиента.
	float g_flVolumetricClouds < Attribute( "VolumetricClouds" ); Default( 1.0 ); >;

	// HDR: реалистичная динамика яркости под bloom-пост (как g_flGlobalEmissionBoost в car_lights:
	// эмиссия >1 блумит). В зените солнце ослепляет — жирный bloom; у горизонта (рассвет/закат)
	// диск тусклый — на него можно смотреть. Облака в полдень уходят >1 и мягко светятся.
	float g_flHdrSunMax     < Default( 40.0 ); Range( 1.0, 200.0 ); UiGroup( "HDR,57/10" ); >;
	float g_flHdrSunMin     < Default( 1.2 );  Range( 0.0, 10.0 );  UiGroup( "HDR,57/20" ); >;
	float g_flHdrCloudBoost < Default( 1.6 );  Range( 1.0, 6.0 );   UiGroup( "HDR,57/30" ); >;
	// Экспозиция самого купола: в полдень небо ярче (зенит/сторона солнца может блумить),
	// к закату гаснет вместе с высотой солнца — сумеречное небо тусклое само по себе.
	float g_flHdrSkyBoost   < Default( 1.5 );  Range( 1.0, 4.0 );   UiGroup( "HDR,57/40" ); >;
	// Околосолнечное гало (circumsolar): в полдень смотреть В СТОРОНУ солнца ярко (>1 → bloom),
	// бока купола остаются нормальными; у горизонта гало гаснет квадратично.
	float g_flHdrSunGlow    < Default( 2.5 );  Range( 0.0, 10.0 );  UiGroup( "HDR,57/50" ); >;

	float g_flStorminess < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "Weather,55/10" ); >;
	float g_flRainAmount < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "Weather,55/20" ); >;
	// Единая пасмурность из C# (GameTimeManager.Overcast01) — ТА ЖЕ величина, что глушит свет
	// сцены. Производная от инерционной облачности, а не от мгновенного дождя: купол темнеет
	// в одном темпе с освещением сцены.
	float g_flOvercast < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "Weather,55/22" ); >;
	// Итоговый ambient сцены из C# — им подсвечивается пасмурная дека и серая мгла купола,
	// чтобы небо всегда матчилось со сценой по яркости и оттенку (тёплый закат, тёмная гроза, ночь).
	float3 g_vSceneAmbient < Default3( 0.5, 0.5, 0.5 ); UiType( Color ); UiGroup( "Weather,55/24" ); >;
	float g_flBoltIntensity < Default( 0.0 ); Range( 0.0, 5.0 ); UiGroup( "Weather,55/30" ); >;
	float3 g_vLightningColor < Default3( 0.78, 0.82, 1.0 ); UiType( Color ); UiGroup( "Weather,55/40" ); >;
	float3 g_vStormColor < Default3( 0.18, 0.20, 0.24 ); UiType( Color ); UiGroup( "Weather,55/50" ); >;
	// Яркость плоского пасмурного освещения облаков: пасмурная дека — светлый серый купол, а не
	// тёмная грозовая туча (умножается на g_vCloudColor). Выше = светлее облака в дождь.
	float g_flOvercastCloudLight < Default( 0.55 ); Range( 0.0, 1.5 ); UiGroup( "Weather,55/60" ); >;

	float noise( in float3 x )
	{
		float3 p = floor( x );
		float3 f = frac( x );
		f = f * f * ( 3.0 - 2.0 * f );
		float2 uv = ( p.xy + float2( 37.0, 17.0 ) * p.z ) + f.xy;
		float2 rg = g_tBlueNoise.Sample( g_sBilinearWrap, ( uv + 0.5 ) / 256.0 ).xy;
		return lerp( rg.x, rg.y, f.z );
	}

	float StarHash( float3 cell, float salt )
	{
		float h = dot( cell, float3( 127.1 + salt, 311.7, 74.7 + salt * 1.7 ) );
		return frac( sin( h ) * 43758.5453 );
	}

	float3 StarTint( in float3 vRay )
	{
		float grid = g_vViewportSize.y * g_flStarScale * 0.75;
		float3 cell = floor( vRay * grid );

		float h1 = StarHash( cell, 0.0 );
		float h2 = StarHash( cell, 19.0 );

		float coolWarm = h1 * 2.0 - 1.0;
		float3 tint = float3( 0.90, 0.93, 1.00 );
		tint.r += coolWarm * 0.05;
		tint.g += coolWarm * 0.02;
		tint.b -= coolWarm * 0.07;
		tint = lerp( tint, float3( 1.00, 0.90, 0.76 ), saturate( ( h2 - 0.86 ) * 5.0 ) * 0.35 );

		return tint;
	}

	float3 Stars( in float3 vRay )
	{
		const float fStarAmount = 1.0;

		float vStars = noise( vRay * ( g_vViewportSize.y * g_flStarScale ) * 0.75 );
		vStars += noise( vRay * ( g_vViewportSize.y * g_flStarScale ) * 0.5 );
		vStars += noise( vRay * ( g_vViewportSize.y * g_flStarScale ) * 0.25 );
		vStars += noise( vRay * ( g_vViewportSize.y * g_flStarScale ) * 0.1 );
		vStars += noise( vRay * ( g_vViewportSize.y * g_flStarScale ) ) * ( 1.0 - fStarAmount );

		vStars = clamp( vStars, 0.0, 1.0 );
		vStars = ( 1.0 - vStars );

		// Мерцание: у каждой ячейки своя частота/фаза — один sin по времени, почти бесплатно.
		float3 cell = floor( vRay * ( g_vViewportSize.y * g_flStarScale * 0.75 ) );
		float h = StarHash( cell, 7.0 );
		float twinkle = 0.78 + 0.22 * sin( g_flTime * ( 1.5 + h * 4.5 ) + h * 39.0 );

		return vStars * StarTint( vRay ) * twinkle;
	}

	// ===== Объёмные облака: порт raymarch из pp_clouds.shader =====
	// Хеш-шум 3D + fbm (без LUT — компилятор отвергает глобальные массивы/матрицы).
	float Hash31( float3 p )
	{
		p = frac( p * 0.1031 );
		p += dot( p, p.zyx + 31.32 );
		return frac( ( p.x + p.y ) * p.z );
	}

	float ValueNoise3D( float3 p )
	{
		float3 ip = floor( p );
		float3 f = frac( p );
		f = f * f * ( 3.0 - 2.0 * f );

		float n000 = Hash31( ip );
		float n100 = Hash31( ip + float3( 1, 0, 0 ) );
		float n010 = Hash31( ip + float3( 0, 1, 0 ) );
		float n110 = Hash31( ip + float3( 1, 1, 0 ) );
		float n001 = Hash31( ip + float3( 0, 0, 1 ) );
		float n101 = Hash31( ip + float3( 1, 0, 1 ) );
		float n011 = Hash31( ip + float3( 0, 1, 1 ) );
		float n111 = Hash31( ip + float3( 1, 1, 1 ) );

		float nx00 = lerp( n000, n100, f.x );
		float nx10 = lerp( n010, n110, f.x );
		float nx01 = lerp( n001, n101, f.x );
		float nx11 = lerp( n011, n111, f.x );

		return lerp( lerp( nx00, nx10, f.y ), lerp( nx01, nx11, f.y ), f.z );
	}

	// Октавы fbm развёрнуты вручную: динамический [loop] не давал компилятору
	// ни разворота, ни переиспользования регистров — на 28+ вызовах за пиксель это заметно.
	static const float FBM_NORM2 = 0.825;   // 0.55 + 0.275
	static const float FBM_NORM4 = 1.03125; // + 0.1375 + 0.06875
	static const float FBM_REST  = 0.20625; // максимум вклада 3-4 октав

	float Fbm2( float3 p )
	{
		float v = 0.55 * ValueNoise3D( p );
		v += 0.275 * ValueNoise3D( p * 2.17 + 19.19 );
		return v / FBM_NORM2;
	}

	// Профиль слоя: плоские тёмные основания, округлые верхушки.
	float CloudProfile( float z )
	{
		float hf = saturate( ( z - g_flCloudBase ) / max( g_flCloudThickness, 1.0 ) );
		return smoothstep( 0.0, 0.12, hf ) * smoothstep( 1.0, 0.45, hf );
	}

	// g_flCloudScale (0.3..8) → мировой масштаб шума; больше = мельче облака.
	float3 CloudCoord( float3 p )
	{
		float3 q = p;
		q.xy += g_vCloudOffset; // ветер копится в C# — в грозу дека несётся быстрее без скачка поля
		q *= g_flCloudScale * 0.00017; // ниже частота = КРУПНЫЕ цельные облака (было 0.0003 — мелкая рвань)
		q.z *= 1.6; // облака шире, чем выше
		return q;
	}

	// Плотность облака в мировой точке (4 октавы + эрозия кромок). Покрытие cov считается
	// один раз в VolumetricClouds. Ранний выход: если даже максимум старших октав не пробивает
	// порог покрытия — точка пустая, половину шума не считаем (в рваном небе это большинство сэмплов).
	float CloudDensity( float3 p, float cov )
	{
		float profile = CloudProfile( p.z );
		if ( profile <= 0.001 )
			return 0.0;

		float3 q = CloudCoord( p );

		float v = 0.55 * ValueNoise3D( q );
		float3 q2 = q * 2.17 + 19.19;
		v += 0.275 * ValueNoise3D( q2 );

		if ( ( v + FBM_REST ) / FBM_NORM4 <= 1.0 - cov )
			return 0.0;

		float3 q3 = q2 * 2.17 + 19.19;
		v += 0.1375 * ValueNoise3D( q3 );
		v += 0.06875 * ValueNoise3D( q3 * 2.17 + 19.19 );
		float baseN = v / FBM_NORM4;

		float d = saturate( ( baseN - ( 1.0 - cov ) ) / max( cov, 0.01 ) );

		// Округляем ядра: большие ПОЛНЫЕ кучевые облака с чёткой кромкой, а не тонкая рвань.
		d = d * d * ( 3.0 - 2.0 * d );

		// Эрозия — только у самой тонкой кромки и мягкая: не дробит облако на клочья.
		if ( d > 0.0 && d < 0.4 )
		{
			float erodeAmt = lerp( 0.05, 0.20, g_flCloudSoftness );
			d = saturate( d - Fbm2( q * 3.1 ) * erodeAmt * ( 1.0 - d ) );
		}

		return d * profile * g_flCloudDensity;
	}

	// Дешёвая плотность (2 октавы, без эрозии) — для световой прогонки к солнцу.
	float CloudDensityCheap( float3 p, float cov )
	{
		float profile = CloudProfile( p.z );
		if ( profile <= 0.001 )
			return 0.0;

		float baseN = Fbm2( CloudCoord( p ) );
		float d = saturate( ( baseN - ( 1.0 - cov ) ) / max( cov, 0.01 ) );
		return d * profile * g_flCloudDensity;
	}

	float CloudHG( float cosTheta, float g )
	{
		float g2 = g * g;
		return 0.0796 * ( 1.0 - g2 ) / pow( abs( 1.0 + g2 - 2.0 * g * cosTheta ), 1.5 );
	}

	// Намеренно НИЗКОЕ качество марша: мало шагов = дёшево. Крупные цельные облака (низкая частота
	// шума ниже) прячут малое число слайсов — бандинг не читается, а бюджет уходит в размер/свет.
	static const int   CLOUD_MARCH_STEPS = 16;      // максимум шагов (длинные лучи у горизонта)
	static const int   CLOUD_MARCH_MIN   = 7;       // минимум (короткий путь у зенита)
	static const int   CLOUD_LIGHT_STEPS = 2;
	static const float CLOUD_EXT_COEF    = 0.004;   // поглощение на мировой юнит при density 1
	static const float CLOUD_MAX_PATH    = 50000.0; // ограничение пути у горизонта

	// Оптическая глубина к солнцу: дешёвое самозатенение внутри облака.
	float CloudLightTransmittance( float3 p, float3 sunDir, float cov )
	{
		float stepLen = g_flCloudThickness / CLOUD_LIGHT_STEPS;
		float optical = 0.0;
		[unroll]
		for ( int k = 1; k <= CLOUD_LIGHT_STEPS; k++ )
		{
			float3 sp = p + sunDir * stepLen * k;
			optical += CloudDensityCheap( sp, cov ) * CLOUD_EXT_COEF * stepLen;
		}
		return exp( -optical );
	}

	// Raymarch деки облаков вдоль луча неба. Возвращает накопленный in-scatter (rgb)
	// и пропускание (out transmittance) для альфа-композита: sky = sky*T + light.
	float3 VolumetricClouds( float3 ray, float3 sunDir, float3 sunColor, float nightFactor, out float transmittance )
	{
		transmittance = 1.0;
		float3 light = float3( 0, 0, 0 );

		float3 camPos = g_vCameraPositionWs;
		float cloudTop = g_flCloudBase + g_flCloudThickness;

		// Пересечение луча с декой [Base, Top]. Камера почти всегда ниже деки.
		float t0, t1;
		if ( camPos.z < g_flCloudBase )
		{
			if ( ray.z <= 0.0005 ) return light; // луч в горизонт/вниз — деки не достигает
			t0 = ( g_flCloudBase - camPos.z ) / ray.z;
			t1 = ( cloudTop - camPos.z ) / ray.z;
		}
		else if ( camPos.z > cloudTop )
		{
			if ( ray.z >= -0.0005 ) return light;
			t0 = ( cloudTop - camPos.z ) / ray.z;
			t1 = ( g_flCloudBase - camPos.z ) / ray.z;
		}
		else // внутри деки
		{
			t0 = 0.0;
			t1 = ray.z > 0.0005 ? ( cloudTop - camPos.z ) / ray.z
			   : ray.z < -0.0005 ? ( g_flCloudBase - camPos.z ) / ray.z
			   : CLOUD_MAX_PATH;
		}

		t0 = max( t0, 0.0 );
		t1 = min( t1, t0 + CLOUD_MAX_PATH );
		if ( t1 <= t0 ) return light;

		// Растворение у горизонта (длинный путь + кромка) — считаем ДО марша: широкая полоса
		// пикселей у горизонта всё равно умножилась бы на fade≈0, марш там — работа в мусор.
		float horizon = smoothstep( 0.0, 0.12, ray.z );
		float fade = exp( -t0 * 0.000012 ) * horizon;
		if ( fade < 0.01 ) return light;

		float cosTheta = dot( ray, sunDir );
		// Двухлепестковая фаза: тугой forward-scatter (яркие серебряные кромки) + мягкий back-scatter.
		float phase = CloudHG( cosTheta, 0.55 ) * 0.75 + CloudHG( cosTheta, -0.12 ) * 0.25;

		// Дневной/ночной ambient и масштаб прямой подсветки. Ночью «солнце» — луна (тот же
		// DirectionalLight с лунным цветом), поэтому прямую подсветку не зануляем, а оставляем
		// слабую — серебристые кромки лунных облаков.
		float overcast = saturate( g_flOvercast );
		// Покрытие уже ведёт C# (Cloudiness → g_flCloudCoverage); гроза лишь чуть доуплотняет.
		float cov = saturate( g_flCloudCoverage + g_flStorminess * 0.08 );
		float3 amb = lerp( g_vCloudShadow, g_vCloudShadow * 0.15, nightFactor );
		// Пасмурная дека подсвечивается AMBIENT-ом СЦЕНЫ: рассеянный свет всего неба и есть тот
		// свет, что заливает сцену — купол и земля согласованы по яркости/оттенку (тёплый закат,
		// тёмная гроза, лунная ночь) без отдельной константы дневного серого.
		float3 overcastAmb = g_vSceneAmbient * ( g_flOvercastCloudLight * 2.6 );
		amb = lerp( amb, max( amb, overcastAmb ), overcast );
		// Молния подсвечивает тучи ИЗНУТРИ — вспышка в толще, а не только на куполе неба.
		amb += g_vLightningColor * g_flBoltIntensity * 0.55;
		// sunColor уже затемнён пасмурностью в C# (OvercastDirectLight) — здесь второй раз не глушим.
		float sunScale = g_flCloudSunBoost * lerp( 1.0, 0.22, nightFactor );
		// HDR: в полдень освещённые верхушки уходят >1 и мягко блумят; в пасмурность буст гаснет.
		sunScale *= lerp( 1.0, g_flHdrCloudBoost, saturate( sunDir.z * 1.4 ) * ( 1.0 - nightFactor ) * ( 1.0 - overcast ) );

		// Адаптивное число шагов: у зенита путь через деку короткий — хватает меньшего
		// числа сэмплов; полный бюджет остаётся только длинным лучам у горизонта.
		int numSteps = (int)clamp( ( t1 - t0 ) / ( g_flCloudThickness * 0.65 ), (float)CLOUD_MARCH_MIN, (float)CLOUD_MARCH_STEPS );
		float stepLen = ( t1 - t0 ) / numSteps;
		// Статичный дизер по направлению луча (без времени) — ломает слайс-бэндинг без мерцания.
		float t = t0 + stepLen * Hash31( ray * 4096.0 );

		[loop]
		for ( int s = 0; s < numSteps; s++ )
		{
			float3 p = camPos + ray * t;
			float d = CloudDensity( p, cov );
			if ( d > 0.01 )
			{
				float sigma = d * CLOUD_EXT_COEF;
				float sampleT = exp( -sigma * stepLen );

				float lightT = CloudLightTransmittance( p, sunDir, cov );
				float powder = 1.0 - exp( -d * 2.0 );

				float hf = saturate( ( p.z - g_flCloudBase ) / max( g_flCloudThickness, 1.0 ) );
				float3 ambient = amb * lerp( 0.35, 1.0, hf ); // тёмные основания

				// Серебряная кромка: тонкие, просвеченные к СОЛНЦУ края ярко forward-рассеивают. Сильна
				// у тугого пика к солнцу (cosTheta^8), где свет пробивает облако (lightT) и слой тонкий
				// (1-powder). Уходит в HDR>1 → мягкий bloom в посте — реалистичное сияние облаков.
				float silver = pow( saturate( cosTheta ), 8.0 ) * lightT * ( 1.0 - powder );

				float3 lit = sunColor * g_vCloudColor * lightT * powder * phase * 14.0 * sunScale
				           + sunColor * silver * 10.0 * sunScale
				           + ambient * 0.5;

				light += transmittance * ( 1.0 - sampleT ) * lit;
				transmittance *= sampleT;
				if ( transmittance < 0.02 )
					break;
			}
			t += stepLen;
		}

		// Растворяем у горизонта (fade посчитан до марша).
		light *= fade;
		transmittance = lerp( 1.0, transmittance, fade );

		return light;
	}

	// Упрощённое рассеяние: синий зенит, белый горизонт.
	float3 LiteAtmosphere( float3 vRay, float3 sunDir, float3 sunColor, float3 nightColor, float nightFactor )
	{
		float3 ray = normalize( vRay );
		float3 sun = normalize( sunDir );

		float elevation = saturate( ray.z * 0.5 + 0.5 );
		float mu = saturate( dot( ray, sun ) );
		float sunLow = saturate( 1.0 - sun.z * 2.2 );

		// Без жёсткого пола: небо может темнеть вместе со сценой (гроза, граница день/ночь) —
		// иначе купол упирался в 0.15, пока освещение сцены продолжало гаснуть.
		float lum = max( dot( sunColor, float3( 0.299, 0.587, 0.114 ) ), 1e-4 );
		float sunBright = max( lum, 0.02 );

		// Чистый оттенок света (chroma, нейтральный полдень ≈ 1,1,1). На закате свет тёплый —
		// sunChroma несёт оранжевый, и именно им красим небо, чтобы синева уходила в цвет света.
		float3 sunChroma = sunColor / lum;
		// Теплота света: насколько красный канал превышает синий (0 днём, ~1 на закате).
		float warmth = saturate( ( sunChroma.r - sunChroma.b ) * 0.85 );

		float rayleighPhase = 0.50 + 0.50 * mu * mu;
		float3 zenith = float3( 0.14, 0.38, 1.00 ) * sunBright;
		float3 midSky = float3( 0.45, 0.62, 0.98 ) * sunBright;

		// Горизонт уходит от дневного бело-голубого к тёплому цвету САМОГО света на закате.
		float3 horizon = lerp( float3( 0.96, 0.98, 1.00 ), sunChroma, sunLow * ( 0.5 + warmth * 0.5 ) );
		horizon *= sunBright + 0.35;

		float3 daySky = lerp( horizon, lerp( midSky, zenith, pow( elevation, 0.75 ) ), pow( elevation, 0.32 ) );
		daySky *= rayleighPhase * ( 0.28 + elevation * 0.72 );

		// Широкий тёплый ореол со стороны солнца: на закате половина купола у солнца теплеет
		// (длинный путь сквозь атмосферу вымывает синее), противосолнечная сторона остаётся синей.
		float sunWash = pow( mu, 1.5 ) * sunLow * warmth;
		daySky = lerp( daySky, daySky * sunChroma, saturate( sunWash * 0.8 ) );

		float mieGlow = pow( mu, 6.0 ) * ( 0.22 + sunLow * 0.55 );
		float mieHaze = pow( 1.0 - elevation, 2.5 ) * ( 0.12 + sunLow * 0.35 );
		daySky += sunColor * ( mieGlow + mieHaze );

		// HDR купола: яркость неба следует ВЫСОТЕ солнца. В полдень весь купол ярче
		// (g_flHdrSkyBoost), плюс околосолнечное гало — в сторону солнца смотреть слепяще
		// (>1 → bloom), а бока/противосолнечная сторона остаются нормальными. К закату
		// sunElevHdr → 0: небо тускнеет само, гало гаснет, на солнце можно смотреть.
		// В пасмурность HDR-бусты гаснут полностью: ни яркого купола, ни гало у солнца сквозь тучи.
		float sunElevHdr = saturate( sun.z * 1.4 );
		float clearHdr = 1.0 - saturate( g_flOvercast );
		daySky *= lerp( 1.0, g_flHdrSkyBoost, sunElevHdr * clearHdr );
		daySky += sunColor * pow( mu, 8.0 ) * ( g_flHdrSunGlow * sunElevHdr * sunElevHdr * clearHdr );

		float3 nightSky = nightColor * ( 0.10 + pow( elevation, 0.55 ) * 0.32 );
		nightSky += nightColor * pow( mu, 18.0 ) * 0.10;
		// Мягкий лунный ореол на половине купола у луны (mu ночью считается к луне — тот же свет).
		nightSky += g_vMoonColor * g_flMoonGlow * 0.05 * pow( mu, 6.0 );

		float3 scatter = lerp( daySky, nightSky, nightFactor );
		float scale = lerp( 1.0, 0.20, nightFactor );
		scatter *= scale;

		// Пасмурная мгла купола: яркость = AMBIENT СЦЕНЫ (день/ночь/грозовое глушение уже внутри),
		// оттенок — хрома g_vStormColor. Лерп ПОЛНЫЙ: на сплошной облачности от ясного неба
		// (включая HDR-остатки) не остаётся ничего — купол ровно той же природы, что свет сцены.
		float3 stormChroma = g_vStormColor / max( dot( g_vStormColor, float3( 0.299, 0.587, 0.114 ) ), 1e-4 );
		float3 stormSky = g_vSceneAmbient * stormChroma * 1.15;
		scatter = lerp( scatter, stormSky, saturate( g_flOvercast ) );

		// Дождевая дымка у горизонта — от мгновенного дождя (это осадки, не свет).
		float rainHaze = saturate( g_flRainAmount ) * pow( 1.0 - elevation, 2.0 );
		scatter = lerp( scatter, stormSky * 0.85, rainHaze * 0.55 );

		// Молния подсвечивает воздух — добавка ко всему куполу при вспышке.
		scatter += g_vLightningColor * g_flBoltIntensity * 0.06 * ( 1.0 - nightFactor * 0.5 );

		return scatter;
	}

	float3 CelestialDisc( in float3 vRay, float3 lightDirWs, float3 color, float alignEpsilon, float discPower, float discScale, float glow, float glowPower )
	{
		float align = dot( vRay, normalize( lightDirWs ) );
		float disc = pow( saturate( align + alignEpsilon ), discPower ) * discScale;
		float halo = pow( saturate( align + 0.008 ), glowPower ) * glow;
		return color * ( disc + halo );
	}

	PS_OUTPUT MainPs( PS_INPUT i )
	{
		PS_OUTPUT o;
		float3 vPositionWs = i.vPositionWs;
		float3 vRay = normalize( vPositionWs - g_vCameraPositionWs );

		float3 rotatedRay = RotateDirectionX( vRay, g_flStarRotation );
		float3 sunDirectionWs = -g_DirectionalLightDirection.xyz;
		float3 sunColor = g_DirectionalLightColor.rgb;

		float3 skyColor = 0.0f;
		float nightFactor = g_flNightFactor;
		float sunDiscFade = 1.0f;

		const float sunDirLengthSq = dot( sunDirectionWs, sunDirectionWs );
		const float sunColorLengthSq = dot( sunColor, sunColor );
		if ( sunDirLengthSq > 0.0f && sunColorLengthSq > 0.0f )
		{
			float shaderNight = 1.0 - smoothstep( g_flNightEnd, g_flNightStart, sunDirectionWs.z );
			nightFactor = max( nightFactor, shaderNight );
			sunDiscFade = ( 1.0 - nightFactor ) * smoothstep( g_flNightEnd, g_flNightStart, sunDirectionWs.z );

			skyColor += LiteAtmosphere( vRay, sunDirectionWs, sunColor, g_vNightSkyColor, nightFactor ) * g_flDayIntensity;

			// Пасмурность затягивает диск солнца/луны дымкой.
			float discVisible = 1.0 - saturate( g_flOvercast ) * 0.92;

			// HDR-яркость диска: у горизонта — g_flHdrSunMin (можно смотреть на солнце),
			// в зените — g_flHdrSunMax (ослепляет, жирно блумит). Квадрат — реалистичнее подъём.
			float sunElev = saturate( sunDirectionWs.z * 1.4 );
			float sunHdr = lerp( g_flHdrSunMin, g_flHdrSunMax, sunElev * sunElev );

			skyColor += CelestialDisc( vRay, sunDirectionWs, sunColor, 0.00025, 10000.0, 10.0, 0.0, 80.0 )
				* g_flDayIntensity * sunDiscFade * ( 1.0 - nightFactor ) * discVisible * sunHdr;
			skyColor += CelestialDisc( vRay, sunDirectionWs, g_vMoonColor, 0.00015, 6000.0, g_flMoonIntensity, g_flMoonGlow, 80.0 )
				* g_flDayIntensity * nightFactor * discVisible;

			// Звёзды глушит пасмурность; днём (nightFactor≈0) ветку пропускаем целиком —
			// экономит 5 сэмплов blue-noise на пиксель без какой-либо потери качества.
			float starVis = g_flStarsIntensity * nightFactor * ( 1.0 - saturate( g_flOvercast ) * 0.85 );
			if ( starVis > 0.002 )
				skyColor += Stars( rotatedRay ) * starVis;
		}
		else
		{
			skyColor = g_vNightSkyColor * g_flDayIntensity;
			float starVisNoSun = g_flStarsIntensity * nightFactor;
			if ( starVisNoSun > 0.002 )
				skyColor += Stars( rotatedRay ) * starVisNoSun;
		}

		// Объёмные облака поверх неба — корректный альфа-композит (поглощение + рассеяние).
		// Перекрывают звёзды/диски, реагируют на погоду через g_flCloudCoverage/g_flOvercast.
		// Рейммарч пропускается целиком при нулевой опасити ИЛИ выключенном тумблере (настройка графики).
		if ( g_flCloudOpacity > 0.001 && g_flVolumetricClouds > 0.5 )
		{
			float cloudTransmittance;
			float3 cloudLight = VolumetricClouds( vRay, sunDirectionWs, sunColor, nightFactor, cloudTransmittance );
			cloudLight *= g_flCloudOpacity;
			cloudTransmittance = lerp( 1.0, cloudTransmittance, g_flCloudOpacity );
			skyColor = skyColor * cloudTransmittance + cloudLight;
		}

		o.vColor0 = float4( skyColor, 1.0 );
		o.vColor0.rgb = ApplyVolumetricFog( o.vColor0.rgb, i.vPositionWs, i.vPositionSs.xy );
		return o;
	}
}
