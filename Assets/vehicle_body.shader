//
// Шейдер КУЗОВА машины: то же, что map/gen_material.shader (PBR + перекраска g_vColorTint +
// мокрость/капли дождя в object-space + эмиссия/альфа/параллакс за фичами), ПЛЮС слой наклеек,
// спроецированный прямо в материал в object-space. Вынесен в отдельный шейдер, чтобы не тащить
// наклейки в gen_material (тот — для всех обычных материалов). Задел под грязь/повреждения — тем же
// слой-механизмом (писать не только в Albedo, но и в Normal/Roughness).
//
FEATURES
{
    #include "common/features.hlsl"
    Feature( F_PARALLAX,   0..1, "Features" );
    Feature( F_EMISSIVE,   0..1, "Features" );
    Feature( F_ALPHA,      0..1, "Features" );
    Feature( F_BACKFACES,  0..1, "Features" );
    Feature( F_WETNESS,    0..1, "Features" );
    Feature( F_METALNESS,  0..1, "Features" );
    // Рафнес включён по умолчанию. Дефолт Feature — всегда минимальное значение диапазона,
    // поэтому 0 = "On": существующие материалы (без ключа в vmat) остаются с рафнесом.
    // "Off" полностью выкидывает рафнес из шейдера: ни карты, ни множителя, ни семпла.
    Feature( F_ROUGHNESS, 0..1( 0 = "On", 1 = "Off" ), "Features" );
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
	// Объектные координаты — для процедурных капель дождя и наклеек (паттерн прибит к кузову, не плывёт при движении).
	float3 vPositionOs : TEXCOORD14;
	#if ( PROGRAM == VFX_PROGRAM_PS ) && S_BACKFACES
		bool face : SV_IsFrontFace;
	#endif
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		o.vPositionOs = i.vPositionOs.xyz;
		return FinalizeVertex( o );
	}
}

PS
{
    #include "common/pixel.hlsl"

	StaticCombo( S_PARALLAX,  F_PARALLAX,  Sys( ALL ) );
	StaticCombo( S_EMISSIVE,  F_EMISSIVE,  Sys( ALL ) );
	StaticCombo( S_ALPHA,     F_ALPHA,     Sys( ALL ) );
	StaticCombo( S_BACKFACES, F_BACKFACES, Sys( ALL ) );
	StaticCombo( S_WETNESS,   F_WETNESS,   Sys( ALL ) );
	StaticCombo( S_METALNESS, F_METALNESS, Sys( ALL ) );
	StaticCombo( S_NO_ROUGHNESS, F_ROUGHNESS, Sys( ALL ) );

	#if S_BACKFACES
		RenderState( CullMode, NONE );
	#endif

	//
	// PBR Texture inputs
	//
	CreateInputTexture2D( TextureColor, Srgb, 8, "", "_color", "Material,10/10", Default3( 1.0, 1.0, 1.0 ) );
	CreateInputTexture2D( TextureNormal, Linear, 8, "", "_normal", "Material,10/20", Default3( 0.5, 0.5, 1.0 ) );
	#if ( S_NO_ROUGHNESS == 0 )
	CreateInputTexture2D( TextureRoughness, Linear, 8, "", "_rough", "Material,10/30", Default( 0.5 ) );
	#endif
	#if S_PARALLAX
	CreateInputTexture2D( TextureDisplacement, Linear, 8, "", "_height", "Material,10/40", Default( 0.5 ) );
	#endif

	CreateTexture2D( g_tColor ) < Channel( RGBA, Box( TextureColor ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;
	CreateTexture2D( g_tNormal ) < Channel( RGB, Box( TextureNormal ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	#if ( S_NO_ROUGHNESS == 0 )
	CreateTexture2D( g_tRoughness ) < Channel( R, Box( TextureRoughness ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	#endif
	#if S_PARALLAX
	CreateTexture2D( g_tDisplacement ) < Channel( R, Box( TextureDisplacement ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	#endif

	// Опасити — ОТДЕЛЬНОЙ картой (R-канал, фильтр Box): работает на текстурах любого размера
	// (в т.ч. не степень двойки) и с мип-компенсацией держит тонкую разметку/листву вдали.
	#if S_ALPHA
	CreateInputTexture2D( TextureOpacity, Linear, 8, "", "_opacity", "Alpha,30/10", Default( 1.0 ) );
	CreateTexture2D( g_tOpacity ) < Channel( R, Box( TextureOpacity ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

	float g_flOpacityCutoff < Default( 0.5 ); Range( 0.0, 1.0 ); UiGroup( "Alpha,30/20" ); >;
	float g_flOpacityMipBoost < Default( 0.5 ); Range( 0.0, 1.0 ); UiGroup( "Alpha,30/30" ); >;
	// Сужение вдали: буст поднимает над cutoff остатки мип-ореола линии — порог слегка растёт
	// с мипом (+N за мип, кэп на 4 мипах), изолиния стягивается к гребню. Основную резкость
	// поперёк даёт программная анизотропия (тапы), это только доводка. 0 = выкл.
	float g_flOpacityMipNarrow < Default( 0.02 ); Range( 0.0, 0.2 ); UiGroup( "Alpha,30/40" ); >;
	#endif

	#if ( S_NO_ROUGHNESS == 0 )
	float g_flRoughnessMultiplier < Default( 1.0 ); Range( 0.0, 2.0 ); UiGroup( "Material,10/50" ); >;
	#endif

	// Металличность за комбо-фичей (галочка F_METALNESS). Выкл по умолчанию → не компилится,
	// нулевая цена (металл-текстуры редкие). Вкл → текстура (опц.) × множитель; для краски кузова
	// без карты хватает множителя. Без металла отражения (SSR/пробы) слабые из-за Френеля диэлектрика.
	#if S_METALNESS
	CreateInputTexture2D( TextureMetalness, Linear, 8, "", "_metal", "Material,10/55", Default( 1.0 ) );
	CreateTexture2D( g_tMetalness ) < Channel( R, Box( TextureMetalness ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	float g_flMetalness < Default( 1.0 ); Range( 0.0, 1.0 ); UiGroup( "Material,10/56" ); >;
	#endif

	// Влажность от системы дождя — только при включённой фиче F_WETNESS (галочка Features).
	// Выключено по умолчанию → код не компилится, нулевая стоимость.
	#if S_WETNESS
	float g_flWetness < Attribute( "Wetness" ); Default( 0.0 ); >;
	float g_flWetnessResponse < Default( 1.0 ); Range( 0.0, 1.0 ); UiGroup( "Wetness,40/10" ); >;
	float g_flWetDarken < Default( 0.6 ); Range( 0.0, 1.0 ); UiGroup( "Wetness,40/20" ); >;
	float g_flWetRoughness < Default( 0.25 ); Range( 0.0, 1.0 ); UiGroup( "Wetness,40/30" ); >;
	// Процедурные капли/дорожки дождя (кузова машин). 0 по умолчанию — карта/здания не трогаются.
	float g_flDropletAmount < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "Wetness,40/40" ); >;
	#endif

	// Перекраска: умножается на альбедо. По умолчанию белый = без эффекта.
	// Имя совпадает с VehicleComponent.BodyTintParameter.
	float3 g_vColorTint < Default3( 1.0, 1.0, 1.0 ); UiType( Color ); UiGroup( "Material,10/60" ); >;

	// === НАКЛЕЙКИ (слои прямо в материале кузова, object-space проекция) ===
	// Данные ПЕР-ОБЪЕКТНЫЕ (renderer.Attributes из VehicleStickerComponent) → у каждой машины свои
	// наклейки без копий материала. Слой = плоскость в ЛОКАЛЬНЫХ координатах модели (центр + оси +
	// полуразмеры), спроецированная на кузов по vPositionOs → прибита к мешу и едет с машиной идеально
	// (в отличие от декали, которая проецируется в мире и отстаёт на ходу).
	// StickerCount=0 (дефолт) → блок пропускается.
	float g_flStickerCount < Attribute( "StickerCount" ); Default( 0.0 ); >;
	float4 g_vSticker0C < Attribute( "Sticker0C" ); Default4( 0.0, 0.0, 0.0, 0.0 ); >; // xyz центр(OS), w полуширина
	float4 g_vSticker0R < Attribute( "Sticker0R" ); Default4( 1.0, 0.0, 0.0, 0.0 ); >; // xyz right(OS,unit), w полувысота
	float4 g_vSticker0U < Attribute( "Sticker0U" ); Default4( 0.0, 1.0, 0.0, 1.0 ); >; // xyz up(OS,unit), w непрозрачность
	float4 g_vSticker1C < Attribute( "Sticker1C" ); Default4( 0.0, 0.0, 0.0, 0.0 ); >;
	float4 g_vSticker1R < Attribute( "Sticker1R" ); Default4( 1.0, 0.0, 0.0, 0.0 ); >;
	float4 g_vSticker1U < Attribute( "Sticker1U" ); Default4( 0.0, 1.0, 0.0, 1.0 ); >;
	float4 g_vSticker2C < Attribute( "Sticker2C" ); Default4( 0.0, 0.0, 0.0, 0.0 ); >;
	float4 g_vSticker2R < Attribute( "Sticker2R" ); Default4( 1.0, 0.0, 0.0, 0.0 ); >;
	float4 g_vSticker2U < Attribute( "Sticker2U" ); Default4( 0.0, 1.0, 0.0, 1.0 ); >;
	float4 g_vSticker3C < Attribute( "Sticker3C" ); Default4( 0.0, 0.0, 0.0, 0.0 ); >;
	float4 g_vSticker3R < Attribute( "Sticker3R" ); Default4( 1.0, 0.0, 0.0, 0.0 ); >;
	float4 g_vSticker3U < Attribute( "Sticker3U" ); Default4( 0.0, 1.0, 0.0, 1.0 ); >;
	CreateTexture2D( g_tSticker0 ) < Attribute( "Sticker0Tex" ); SrgbRead( true ); Filter( ANISO ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	CreateTexture2D( g_tSticker1 ) < Attribute( "Sticker1Tex" ); SrgbRead( true ); Filter( ANISO ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	CreateTexture2D( g_tSticker2 ) < Attribute( "Sticker2Tex" ); SrgbRead( true ); Filter( ANISO ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	CreateTexture2D( g_tSticker3 ) < Attribute( "Sticker3Tex" ); SrgbRead( true ); Filter( ANISO ); AddressU( CLAMP ); AddressV( CLAMP ); >;

	#if S_EMISSIVE
	CreateInputTexture2D( TextureEmissiveMask, Linear, 8, "", "_emissive", "Emissive,20/10", Default( 0.0 ) );
	CreateTexture2D( g_tEmissiveMask ) < Channel( R, Box( TextureEmissiveMask ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

	// Светиться собственным цветом материала (альбедо × тинт), а не g_vEmissiveColor —
	// маска тогда задаёт только ГДЕ светится (неон/вывески без перекраски тинтом эмиссии).
	bool g_bEmissiveUseAlbedo < UiType( CheckBox ); Default( 0 ); UiGroup( "Emissive,20/15" ); >;
	float3 g_vEmissiveColor < UiType( Color ); Default3( 1.0, 1.0, 1.0 ); UiGroup( "Emissive,20/20" ); >;
	float  g_flEmissiveIntensity < Default( 1.0 ); Range( 0.0, 100.0 ); UiGroup( "Emissive,20/30" ); >;
	#endif

	SamplerState g_sSampler < Filter( ANISO ); MaxAniso( 16 ); AddressU( WRAP ); AddressV( WRAP ); >;

	#if S_WETNESS
	float RainHash21( float2 p )
	{
		p = frac( p * float2( 123.34, 456.21 ) );
		p += dot( p, p + 45.32 );
		return frac( p.x * p.y );
	}

	// Капли-бусины: две октавы сетки, в клетке капля со случайной позицией/радиусом и фазой жизни
	// (появляется → сохнет). Возвращает (градиент U, градиент V, высота).
	float3 RainBeads( float2 xy, float density )
	{
		float3 acc = 0.0;
		[unroll]
		for ( int k = 0; k < 2; k++ )
		{
			float cell = ( k == 0 ) ? 9.0 : 16.0;
			float2 p = xy / cell + (float)k * 7.31;
			float2 id = floor( p );
			float2 f = frac( p ) - 0.5;
			float rnd = RainHash21( id + (float)k * 13.7 );
			float cycle = g_flTime * 0.30 + rnd * 19.0;
			float slot = floor( cycle );
			if ( RainHash21( id + slot ) > density )
				continue;
			float life = frac( cycle );
			float fade = smoothstep( 0.0, 0.12, life ) * smoothstep( 1.0, 0.55, life );
			float2 off = ( float2( RainHash21( id + slot + 1.7 ), RainHash21( id + slot + 4.3 ) ) - 0.5 ) * 0.5;
			float r = lerp( 0.10, 0.26, RainHash21( id + slot + 9.1 ) );
			float2 d = f - off;
			float dist = max( length( d ), 1e-4 );
			float h = smoothstep( r, r * 0.15, dist ) * fade;
			acc += float3( -d / dist * h, h );
		}
		return acc;
	}

	// Стекающие дорожки на вертикальных панелях: колонки + бегущая капля-комета со следом.
	// xy.y растёт ВНИЗ по поверхности. Возвращает (градиент U, градиент V, высота).
	float3 RainStreaks( float2 xy )
	{
		float x = xy.x / 7.0;
		float id = floor( x );
		float rnd = RainHash21( float2( id, 3.7 ) );
		float f = frac( x ) - 0.5;
		float off = ( rnd - 0.5 ) * 0.6;
		float col = smoothstep( 0.14, 0.02, abs( f - off ) );
		float period = lerp( 70.0, 140.0, rnd );
		float speed = lerp( 18.0, 40.0, RainHash21( float2( id, 7.1 ) ) );
		float phase = frac( xy.y / period - g_flTime * speed / period + rnd * 11.0 );
		float s = ( phase - 0.5 ) * period;
		float head = smoothstep( 5.0, 0.0, abs( s ) );
		float trail = saturate( 1.0 + s / 25.0 ) * step( s, 0.0 ) * 0.45;
		float h = col * max( head, trail );
		return float3( -sign( f - off ) * h * 0.7, 0.0, h );
	}

	// Капли дождя поверх материала: паттерн в объектных координатах (прибит к кузову),
	// возмущение нормали в мировом базисе, глянец и лёгкая тень в каплях.
	void ApplyRainDroplets( inout Material m, float3 posOs, float wet, float dropletAmount )
	{
		float amt = wet * dropletAmount;
		if ( amt <= 0.001 )
			return;

		// Объектная нормаль из производных — выбор проекции не зависит от поворота машины в мире.
		float3 nOs = normalize( cross( ddy( posOs ), ddx( posOs ) ) );
		float horiz = saturate( abs( nOs.z ) * 1.6 - 0.5 );

		bool sideX = abs( nOs.x ) > abs( nOs.y );
		float2 planarUv = sideX ? posOs.yz : posOs.xz;

		// Поля смешиваем по вертикальности, НЕ координаты — lerp двух UV-пространств
		// дал бы плывущий/растянутый паттерн на скруглениях крыльев.
		float density = lerp( 0.25, 0.55, amt );
		float3 beads = lerp( RainBeads( planarUv, density ), RainBeads( posOs.xy, density ), horiz );

		float2 streakUv = sideX ? float2( posOs.y, -posOs.z ) : float2( posOs.x, -posOs.z );
		float3 streaks = RainStreaks( streakUv ) * ( 1.0 - horiz );

		float2 grad = beads.xy + streaks.xy;
		float h = saturate( beads.z + streaks.z );

		// Мировой базис возмущения: горизонтальный тангент + перпендикуляр (на крыше — мировые X/Y).
		float3 tW = ( abs( m.Normal.z ) > 0.9 ) ? float3( 1.0, 0.0, 0.0 ) : normalize( cross( float3( 0.0, 0.0, 1.0 ), m.Normal ) );
		float3 bW = cross( m.Normal, tW );

		m.Normal = normalize( m.Normal + ( tW * grad.x + bW * grad.y ) * 0.55 * amt );
		m.Roughness = lerp( m.Roughness, 0.03, h * amt );
		m.Albedo *= 1.0 - 0.08 * h * amt;
	}
	#endif

	// Проецирует один слой наклейки в object-space и подмешивает его в альбедо материала.
	// Плоскость слоя: C.xyz — центр(OS), C.w — полуширина; R.xyz — right(unit), R.w — полувысота;
	// U.xyz — up(unit), U.w — непрозрачность. Texture2D передаётся параметром (поддерживается VFX).
	void ApplySticker( inout Material m, float3 posOs, float3 nrmOs, float4 C, float4 R, float4 U, Texture2D tex )
	{
		if ( C.w <= 0.0001 || R.w <= 0.0001 )
			return;

		float3 sr = R.xyz;
		float3 su = U.xyz;
		float3 sn = normalize( cross( sr, su ) );
		float3 sd = posOs - C.xyz;

		float uu = dot( sd, sr ) / C.w;
		float vv = dot( sd, su ) / R.w;
		float al = dot( sd, sn );

		if ( abs( uu ) > 1.0 || abs( vv ) > 1.0 || abs( al ) > max( C.w, R.w ) || dot( nrmOs, sn ) <= 0.0 )
			return;

		// -uu — зеркалим по горизонтали: иначе картинка проецируется отражённой (текст задом наперёд).
		float2 uv = float2( -uu, vv ) * 0.5 + 0.5;
		float4 sc = tex.Sample( g_sSampler, uv );
		m.Albedo = lerp( m.Albedo, sc.rgb, saturate( sc.a * U.w ) );
	}

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init( i );

		float2 vTexCoords = i.vTextureCoords.xy;

		#if S_PARALLAX
		{
			// Статичные настройки: умеренная сила, отключение вдали и вблизи камеры.
			static const float flParallaxStrength = 0.035;
			static const float flParallaxFadeDistanceInv = 0.002222; // 1 / 450 units
			static const float flParallaxNearStart = 12.0;
			static const float flParallaxNearRamp = 28.0;
			static const float flParallaxViewZMin = 0.35;
			static const float flParallaxMaxUvOffset = 0.012;

			float3 vCameraVec = -i.vPositionWithOffsetWs.xyz;
			float flDistance = sqrt( dot( vCameraVec, vCameraVec ) );

			float flFarFade = saturate( 1.0 - flDistance * flParallaxFadeDistanceInv );
			float flNearFade = saturate( ( flDistance - flParallaxNearStart ) * rcp( flParallaxNearRamp ) );
			float flParallaxFade = flFarFade * flNearFade;

			float3 vViewDirTs = float3(
				dot( i.vTangentUWs, vCameraVec ),
				dot( i.vTangentVWs, vCameraVec ),
				dot( i.vNormalWs, vCameraVec )
			);

			float flInvLength = rsqrt( dot( vViewDirTs, vViewDirTs ) );
			vViewDirTs *= flInvLength;

			float flHeight = Tex2DS( g_tDisplacement, g_sSampler, vTexCoords ).r;
			float flParallaxScale = flParallaxStrength * flParallaxFade;
			float flHeightAdjusted = mad( flHeight, flParallaxScale, -0.5 * flParallaxScale );
			float2 vParallaxOffset = vViewDirTs.xy * flHeightAdjusted * rcp( max( vViewDirTs.z, flParallaxViewZMin ) );

			float flOffsetLenSq = dot( vParallaxOffset, vParallaxOffset );
			float flMaxOffsetSq = flParallaxMaxUvOffset * flParallaxMaxUvOffset;
			vParallaxOffset *= sqrt( min( flOffsetLenSq, flMaxOffsetSq ) * rcp( max( flOffsetLenSq, 1e-8 ) ) );

			vTexCoords += vParallaxOffset;
		}
		#endif

		float4 colorSample = Tex2DS( g_tColor, g_sSampler, vTexCoords );
		float3 normalSample = Tex2DS( g_tNormal, g_sSampler, vTexCoords ).rgb;
		#if ( S_NO_ROUGHNESS == 0 )
		float flRoughness = Tex2DS( g_tRoughness, g_sSampler, vTexCoords ).r;
		#endif

		m.Albedo = colorSample.rgb * g_vColorTint;

		// Наклейки: проекция слоёв прямо в материал кузова (object-space, прибито к мешу).
		// g_flStickerCount — пер-объектный атрибут (константа на драв) → ветвление равномерное, ddx/ddy ок.
		if ( g_flStickerCount > 0.5 )
		{
			float3 vStickerPosOs = i.vPositionOs;
			// Нормаль в объектных координатах из производных — не зависит от поворота машины в мире.
			float3 vStickerNrmOs = normalize( cross( ddy( vStickerPosOs ), ddx( vStickerPosOs ) ) );
			ApplySticker( m, vStickerPosOs, vStickerNrmOs, g_vSticker0C, g_vSticker0R, g_vSticker0U, g_tSticker0 );
			if ( g_flStickerCount > 1.5 ) ApplySticker( m, vStickerPosOs, vStickerNrmOs, g_vSticker1C, g_vSticker1R, g_vSticker1U, g_tSticker1 );
			if ( g_flStickerCount > 2.5 ) ApplySticker( m, vStickerPosOs, vStickerNrmOs, g_vSticker2C, g_vSticker2R, g_vSticker2U, g_tSticker2 );
			if ( g_flStickerCount > 3.5 ) ApplySticker( m, vStickerPosOs, vStickerNrmOs, g_vSticker3C, g_vSticker3R, g_vSticker3U, g_tSticker3 );
		}

		#if S_ALPHA
		{
			static const float flOpacityMipMax = 7.0;
			static const float flOpacityMipBias = -1.0;
			static const float flHwAniso = 16.0;

			float2 dX = ddx( vTexCoords );
			float2 dY = ddy( vTexCoords );
			bool bXLong = dot( dX, dX ) > dot( dY, dY );
			float2 dLong = bXLong ? dX : dY;
			float2 dShort = bXLong ? dY : dX;

			float2 dGradL = dLong * ( 0.25 * exp2( flOpacityMipBias ) );
			float2 dGradS = dShort * exp2( flOpacityMipBias );

			float flOpacity = 0.0;
			[unroll]
			for ( int t = 0; t < 4; t++ )
			{
				float2 vOff = dLong * ( (float)t * 0.25 - 0.375 );
				flOpacity += g_tOpacity.SampleGrad( g_sSampler, vTexCoords + vOff, dGradL, dGradS ).r;
			}
			flOpacity *= 0.25;

			float flRatio = sqrt( max( dot( dLong, dLong ), 1e-12 ) / max( dot( dShort, dShort ), 1e-12 ) );
			float flLodGain = log2( clamp( flRatio / flHwAniso, 1.0, 4.0 ) );
			float flMip = max( g_tOpacity.CalculateLevelOfDetail( g_sSampler, vTexCoords ) + flOpacityMipBias - flLodGain, 0.0 );
			float flMipC = min( flMip, flOpacityMipMax );
			flOpacity = saturate( flOpacity * exp2( flMipC * g_flOpacityMipBoost ) );

			float flCutoff = saturate( g_flOpacityCutoff + min( flMipC, 4.0 ) * g_flOpacityMipNarrow );

			float flEdge = ( flOpacity - flCutoff ) / max( fwidth( flOpacity ), 0.001 ) + 0.5;
			clip( flEdge - 0.5 );
			m.Opacity = saturate( flEdge );
		}
		#else
		m.Opacity = 1.0;
		#endif

		normalSample = mad( normalSample, 2.0, -1.0 );
		#if S_BACKFACES
			float3 vGeomNormal = i.face ? i.vNormalWs : -i.vNormalWs;
			m.Normal = TransformNormal( normalSample, vGeomNormal, i.vTangentUWs, i.vTangentVWs );
		#else
			m.Normal = TransformNormal( normalSample, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		#endif

		#if S_NO_ROUGHNESS
		// Рафнес выключен фичей целиком — полностью матовая поверхность, никаких карт/множителей.
		m.Roughness = 1.0;
		#else
		m.Roughness = flRoughness * g_flRoughnessMultiplier;
		#endif
		#if S_METALNESS
		// Металличность = маска (R-канал карты _metal) × множитель g_flMetalness. Семплим ВСЕГДА (как у
		// эмиссии) — иначе неиспользуемая текстура выкидывается компилятором и слот маски не появляется.
		// Дефолт карты — белый (1.0): материал без своей _metal-карты = чистый множитель, поведение как было.
		float flMetalMask = Tex2DS( g_tMetalness, g_sSampler, vTexCoords ).r;
		m.Metalness = flMetalMask * g_flMetalness;
		#else
		m.Metalness = 0.0;
		#endif

		#if S_WETNESS
		{
			float wet = saturate( g_flWetness ) * saturate( g_flWetnessResponse );
			m.Albedo *= lerp( 1.0, saturate( g_flWetDarken ), wet );
			m.Roughness = saturate( lerp( m.Roughness, m.Roughness * saturate( g_flWetRoughness ), wet ) );
			ApplyRainDroplets( m, i.vPositionOs, wet, g_flDropletAmount );
		}
		#endif

		#if S_EMISSIVE
		{
			float flEmissiveMask = Tex2DS( g_tEmissiveMask, g_sSampler, vTexCoords ).r;
			float3 vEmissiveColor = g_bEmissiveUseAlbedo ? colorSample.rgb * g_vColorTint : g_vEmissiveColor;
			m.Emission = flEmissiveMask * vEmissiveColor * g_flEmissiveIntensity;
		}
		#endif

		return ShadingModelStandard::Shade( m );
	}
}
