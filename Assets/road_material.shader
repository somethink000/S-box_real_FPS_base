//=========================================================================================================================
// Road Material — основной материал поверхности дороги.
//  • База: color / normal / roughness / displacement (parallax) — урезанный gen_material, opaque.
//  • Edge (комбо F_EDGE): БЕЗ кромки/бевела. Плавный неравномерный переход полотна в шумовые камушки
//    по бокам (U=0 слева, 1 справа). Чем ближе к физическому краю, тем больше дырок (alpha-test cutout),
//    сквозь которые проступает террейн — край дороги рассыпается в камни без жёсткой линии.
//    На дистанции шум субпиксельный (cutout мерцает) — ветка гасится общим фейдом детализации
//    (g_flDetailFadeDist, группа Detail — ОДНА дистанция на все фичи: кромку, грязь, разметку, лужи).
//  • Puddles (комбо F_PUDDLES): процедурные лужи в дождь — с ростом Wetness вода заливает низины
//    крупного шума по мировым XY. Вода = плоская нормаль + roughness→0 + рябь от капель.
//    Всё ALU, весь блок за юниформ-веткой Wetness>0 — в сухую погоду стоит ноль.
//  • Markings (комбо F_MARKINGS): продольная разметка АНАЛИТИЧЕСКИ из U-координаты полотна
//    (U=0..1 поперёк, |V|=дистанция/500 вдоль; знак V — флаг кромки, разметку он НЕ гасит:
//    разметка есть у любого полотна с F_MARKINGS в материале). Заменяет
//    отдельный меш линий: нет доп. дроуколов/z-fighting, кромка резкая на любой дистанции
//    и под любым углом (AA по fwidth, никаких текстур/мипов). Краевые линии + центральная
//    (сплошная/двойная/пунктир) + износ краски шумом. Позиции/ширины — в долях ширины полотна.
//  • Расширяемо: каждый новый эффект дороги — отдельная фича F_* + своя секция в MainPs.
//=========================================================================================================================

HEADER
{
	Description = "Road Material: PBR base + soft uneven pebble edge (no hard lip)";
	Version = 1;
}

FEATURES
{
	#include "common/features.hlsl"
	Feature( F_PARALLAX, 0..1, "Features" );
	Feature( F_EDGE,     0..1, "Features" );
	Feature( F_PUDDLES,  0..1, "Features" );
	Feature( F_MARKINGS, 0..1, "Features" );
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

	StaticCombo( S_PARALLAX, F_PARALLAX, Sys( ALL ) );
	StaticCombo( S_EDGE,     F_EDGE,     Sys( ALL ) );
	StaticCombo( S_PUDDLES,  F_PUDDLES,  Sys( ALL ) );
	StaticCombo( S_MARKINGS, F_MARKINGS, Sys( ALL ) );

	// ── PBR база (color / normal / roughness / displacement) ───────────────────
	CreateInputTexture2D( TextureColor,     Srgb,   8, "", "_color",  "Material,10/10", Default3( 1.0, 1.0, 1.0 ) );
	CreateInputTexture2D( TextureNormal,    Linear, 8, "", "_normal", "Material,10/20", Default3( 0.5, 0.5, 1.0 ) );
	CreateInputTexture2D( TextureRoughness, Linear, 8, "", "_rough",  "Material,10/30", Default( 0.5 ) );
	#if S_PARALLAX
	CreateInputTexture2D( TextureDisplacement, Linear, 8, "", "_height", "Material,10/40", Default( 0.5 ) );
	#endif

	CreateTexture2D( g_tColor )     < Channel( RGBA, Box( TextureColor ),     Srgb );   OutputFormat( BC7 ); SrgbRead( true );  >;
	CreateTexture2D( g_tNormal )    < Channel( RGB,  Box( TextureNormal ),    Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	CreateTexture2D( g_tRoughness ) < Channel( R,    Box( TextureRoughness ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	#if S_PARALLAX
	CreateTexture2D( g_tDisplacement ) < Channel( R, Box( TextureDisplacement ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	#endif

	// ── Текстура грязи с обочины (материал, а не цвет) — всегда при включённой кромке. ──
	#if S_EDGE
	CreateInputTexture2D( TextureDirtColor,     Srgb,   8, "", "_dirtcolor",  "Edge Dirt,21/10", Default3( 0.26, 0.20, 0.14 ) );
	CreateInputTexture2D( TextureDirtNormal,    Linear, 8, "", "_dirtnormal", "Edge Dirt,21/20", Default3( 0.5, 0.5, 1.0 ) );
	CreateInputTexture2D( TextureDirtRoughness, Linear, 8, "", "_dirtrough",  "Edge Dirt,21/30", Default( 0.95 ) );
	CreateTexture2D( g_tDirtColor )     < Channel( RGB, Box( TextureDirtColor ),     Srgb );   OutputFormat( BC7 ); SrgbRead( true );  >;
	CreateTexture2D( g_tDirtNormal )    < Channel( RGB, Box( TextureDirtNormal ),    Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	CreateTexture2D( g_tDirtRoughness ) < Channel( R,   Box( TextureDirtRoughness ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	// Тайлинг текстуры грязи в юнитах (repeat каждые N юнитов).
	float g_flEdgeDirtTexTile < Default( 200.0 ); Range( 20.0, 1000.0 ); UiGroup( "Edge Dirt,21/40" ); >;
	#endif

	float g_flRoughnessMultiplier < Default( 1.0 ); Range( 0.0, 2.0 ); UiGroup( "Material,10/50" ); >;
	// Сила базовой нормали: масштаб тангенциальной части нормал-мапы (1 = как в текстуре).
	// Имена с Road-префиксом — движок уже занимает g_flDetailNormalStrength (vr_detail_texture.fxc).
	float g_flRoadNormalStrength  < Default( 1.0 ); Range( 0.0, 3.0 ); UiGroup( "Material,10/60" ); >;
	// ── Детейл-нормаль: второй тап той же карты на более мелком тайлинге — добавляет
	//    высокочастотный рельеф зерна асфальта поверх макро-тайла (по нему и виден parallax). ──
	// Множитель частоты детейла к базовому тайлу (больше = мельче зерно).
	float g_flRoadDetailTile      < Default( 7.0 ); Range( 1.0, 32.0 ); UiGroup( "Material,10/70" ); >;
	// Сила детейл-нормали (0 = выкл, слой не подмешивается).
	float g_flRoadDetailStrength  < Default( 0.6 ); Range( 0.0, 2.0 );  UiGroup( "Material,10/80" ); >;

	// ── Влажность от дождя (глобал "Wetness", пишет WeatherController) ──────────
	// Мокрый асфальт темнее и глянцевее. Без дождя Wetness=0 → эффект отсутствует.
	float g_flWetness         < Attribute( "Wetness" ); Default( 0.0 ); >;
	float g_flWetnessResponse < Default( 1.0 );  Range( 0.0, 1.0 ); UiGroup( "Wetness,15/10" ); >;
	float g_flWetDarken       < Default( 0.6 );  Range( 0.0, 1.0 ); UiGroup( "Wetness,15/20" ); >;
	float g_flWetRoughness    < Default( 0.25 ); Range( 0.0, 1.0 ); UiGroup( "Wetness,15/30" ); >;

	// ── DETAIL: одна дистанция детализации на ВСЕ процедурные фичи ──────────────
	// Раньше дистанция была только у кромки и трещин, а грязь обочины, износ/бортик разметки
	// и лужи считались на любой дальности — весь fbm/ddx крутился и на пикселях у горизонта,
	// где рисунок субпиксельный и всё равно не читается (а тонкий шум там ещё и мерцает).
	// Теперь дистанция ОДНА на все фичи и фейд считается один раз на пиксель (detailFade
	// в MainPs): гаснет последней четвертью радиуса, до этого детали в полную силу.
	// ВНИМАНИЕ при подборе: clip() кромки работает и в Depth-пассе, то есть дальность кромки
	// платится дважды, а трещины/лужи вблизи — самые дорогие фичи. Это ОДИН компромиссный
	// радиус на всё: больше — дальше видно детали, меньше — дешевле кадр.
	float g_flDetailFadeDist < Default( 12000.0 ); Range( 1000.0, 60000.0 ); UiGroup( "Detail,12/10" ); >;


	// ── Общие шумовые хелперы (EDGE / PUDDLES / MARKINGS) — всё ALU ────
	#if ( S_EDGE || S_PUDDLES || S_MARKINGS )
	float Road_Hash21( float2 p )
	{
		p = frac( p * float2( 123.34, 345.45 ) );
		p += dot( p, p + 34.345 );
		return frac( p.x * p.y );
	}

	float Road_ValueNoise( float2 x )
	{
		float2 i = floor( x );
		float2 f = frac( x );
		f = f * f * ( 3.0 - 2.0 * f );
		float a = Road_Hash21( i + float2( 0, 0 ) );
		float b = Road_Hash21( i + float2( 1, 0 ) );
		float c = Road_Hash21( i + float2( 0, 1 ) );
		float d = Road_Hash21( i + float2( 1, 1 ) );
		return lerp( lerp( a, b, f.x ), lerp( c, d, f.x ), f.y );
	}

	float Road_Fbm( float2 p )
	{
		float s = 0.0, a = 0.5;
		[unroll]
		for ( int o = 0; o < 4; o++ )
		{
			s += a * Road_ValueNoise( p );
			p *= 2.03;
			a *= 0.5;
		}
		return s;
	}
	#endif

	// ── EDGE: мягкий неравномерный переход в камушки (комбо F_EDGE) ────────────
	#if S_EDGE
	// Ширина краевой полосы по U (U = вся ширина дороги, держи маленькой).
	float g_flEdgeWidth    < Default( 0.10 ); Range( 0.0, 0.5 );   UiGroup( "Edge,20/10" ); >;
	// Масштаб шума камушков (меньше = крупнее камни).
	float g_flPebbleScale  < Default( 20.0 ); Range( 2.0, 256.0 ); UiGroup( "Edge,20/20" ); >;
	// Как быстро камни захватывают полосу наружу: 1 = линейно, выше = камни только у самого края.
	float g_flFalloffPower < Default( 2.0 );  Range( 0.5, 6.0 );   UiGroup( "Edge,20/30" ); >;
	// Мягкость кромки каждого камня в cutout (больше = размытее переход в дырки).
	float g_flEdgeSoftness < Default( 0.30 ); Range( 0.02, 0.8 );  UiGroup( "Edge,20/40" ); >;
	// Порог прорезания дырок (виден террейн). Выше = больше дырок раньше.
	float g_flEdgeCutout   < Default( 0.5 );  Range( 0.0, 1.0 );   UiGroup( "Edge,20/50" ); >;
	// Затемнение к краю: полотно у кромки темнее (грязь/износ просевшей обочины).
	float g_flEdgeDarken   < Default( 0.35 ); Range( 0.0, 1.0 );   UiGroup( "Edge,20/60" ); >;
	// ── Утопленная кромка: у края полотно «просаживается» ниже — 3D-эффект канавки. ──
	// Глубина фейкового скоса вниз к краю (наклон нормали, 0 = плоско).
	float g_flEdgeDepth         < Default( 0.6 );  Range( 0.0, 2.0 ); UiGroup( "Edge,20/62" ); >;
	// Обострение нормали у кромки — фактурнее/грубее рельеф обочины.
	float g_flEdgeNormalSharpen < Default( 0.9 );  Range( 0.0, 3.0 ); UiGroup( "Edge,20/64" ); >;
	// Прибавка roughness у кромки — обочина матовее полотна.
	float g_flEdgeRoughAdd      < Default( 0.18 ); Range( 0.0, 0.6 ); UiGroup( "Edge,20/66" ); >;
	// ── Грязь с обочины: у края на полотно заходит грязь клочками по шуму, еле заметная,
	//    к центру дороги полностью прозрачная. Полоса шире камушков (заходит дальше внутрь).
	//    Сам материал грязи (color/normal/roughness) — в группе «Edge Dirt». ──
	// Макс. заметность грязи у самого края (держи низко — «еле видимая»).
	float g_flEdgeDirtStrength  < Default( 0.35 ); Range( 0.0, 1.0 );  UiGroup( "Edge,20/81" ); >;
	// Ширина захода грязи внутрь полотна (доля ширины дороги; шире EdgeWidth).
	float g_flEdgeDirtWidth     < Default( 0.22 ); Range( 0.0, 0.5 );  UiGroup( "Edge,20/82" ); >;
	// Резкость фейда к центру: выше = быстрее уходит в прозрачность вглубь дороги.
	float g_flEdgeDirtFalloff   < Default( 2.2 );  Range( 0.5, 6.0 );  UiGroup( "Edge,20/83" ); >;
	// Масштаб шума клочков грязи (юниты; меньше = мельче пятна).
	float g_flEdgeDirtScale     < Default( 300.0 ); Range( 50.0, 2000.0 ); UiGroup( "Edge,20/84" ); >;
	// Дистанция погасания дырок кромки — общая g_flDetailFadeDist в группе Detail.
	#endif

	// ── MARKINGS: продольная разметка аналитически из U (комбо F_MARKINGS) ──────
	#if S_MARKINGS
	// Краевые линии по бокам полотна.
	bool  g_bMarkEdgeLines    < Default( 1 );     UiGroup( "Markings,22/10" ); >;
	// Отступ краевой линии от края полотна (доля ширины дороги).
	float g_flMarkEdgeInset   < Default( 0.07 );  Range( 0.0, 0.3 );    UiGroup( "Markings,22/20" ); >;
	// Центральная линия.
	bool  g_bMarkCenterLine   < Default( 1 );     UiGroup( "Markings,22/30" ); >;
	// Центральная — двойная сплошная.
	bool  g_bMarkCenterDouble < Default( 0 );     UiGroup( "Markings,22/40" ); >;
	// Центральная — пунктир (иначе сплошная).
	bool  g_bMarkCenterDashed < Default( 1 );     UiGroup( "Markings,22/50" ); >;
	// Полос в одну сторону: 1 = двухполоска (только центр+края), 2+ = между полосами
	// добавляются пунктирные разделители (автобан: центр сплошной, края сплошные,
	// между — пунктир). Максимум 4 (3 разделителя на сторону).
	float g_flMarkLanesPerSide < Default( 1.0 ); Range( 1.0, 4.0 ); UiGroup( "Markings,22/55" ); >;
	// Ширина линии (доля ширины дороги).
	float g_flMarkLineWidth   < Default( 0.012 ); Range( 0.002, 0.06 ); UiGroup( "Markings,22/60" ); >;
	// Длина штриха и разрыва пунктира (юниты вдоль дороги).
	float g_flMarkDashLen     < Default( 160.0 ); Range( 20.0, 600.0 ); UiGroup( "Markings,22/70" ); >;
	float g_flMarkDashGap     < Default( 240.0 ); Range( 20.0, 900.0 ); UiGroup( "Markings,22/80" ); >;
	// Цвет краски.
	float3 g_vMarkColor       < Default3( 0.80, 0.80, 0.78 ); UiType( Color ); UiGroup( "Markings,22/90" ); >;
	// Непрозрачность краски: <1 = сквозь краску виден асфальт (та самая «полупрозрачность»).
	float g_flMarkStrength    < Default( 0.9 );   Range( 0.0, 1.0 );    UiGroup( "Markings,22/95" ); >;
	// Износ краски шумом: 0 = свежая, больше = облезлая пятнами.
	float g_flMarkWear        < Default( 0.35 );  Range( 0.0, 1.0 );    UiGroup( "Markings,22/96" ); >;
	// Roughness краски (обычно глаже асфальта).
	float g_flMarkRoughness   < Default( 0.55 );  Range( 0.0, 1.0 );    UiGroup( "Markings,22/97" ); >;
	// Бортик краски: наклон нормали по краям линий/штрихов — слой краски перестаёт быть плоским.
	float g_flMarkBevel       < Default( 0.5 );   Range( 0.0, 1.0 );    UiGroup( "Markings,22/98" ); >;
	#endif

	// Экранный surface-gradient: наклон нормали по градиенту высоты h (бортики краски,
	// скос утопленной кромки). Вызывать в КОГЕРЕНТНОМ потоке (ddx/ddy). Наклон кэпится.
	#if ( S_EDGE || S_MARKINGS )
	float3 Road_SurfaceGrad( float3 geoN, float3 posWs, float h )
	{
		float3 dpx = ddx( posWs );
		float3 dpy = ddy( posWs );
		float2 dh = float2( ddx( h ), ddy( h ) );
		float3 r1 = cross( dpy, geoN );
		float3 r2 = cross( geoN, dpx );
		float det = dot( dpx, r1 );
		float3 sg = ( r1 * dh.x + r2 * dh.y ) * ( sign( det ) / max( abs( det ), 1e-6 ) );
		float l = length( sg );
		return l > 1.2 ? sg * ( 1.2 / l ) : sg;
	}
	#endif

	// ── PUDDLES: процедурные лужи в дождь (комбо F_PUDDLES) ─────────────────────
	#if S_PUDDLES
	// Масштаб крупных низин для луж (юниты).
	float g_flPuddleScale    < Default( 700.0 ); Range( 100.0, 3000.0 ); UiGroup( "Puddles,25/10" ); >;
	// Уровень воды при Wetness=1: доля «высот» шума под водой (больше = лужи шире).
	float g_flPuddleLevel    < Default( 0.42 );  Range( 0.0, 1.0 );      UiGroup( "Puddles,25/20" ); >;
	// Сила ряби от капель на глади луж.
	float g_flRippleStrength < Default( 0.35 );  Range( 0.0, 1.0 );      UiGroup( "Puddles,25/30" ); >;
	// ── Сухие низины: те же чаши, что заливаются лужами, видны и БЕЗ дождя как
	//    неоднородность асфальта — низкие места чуть темнее и резче по нормалям. ──
	// Затемнение асфальта в низине (0 = выкл).
	float g_flPuddleBasinDarken  < Default( 0.26 ); Range( 0.0, 0.6 ); UiGroup( "Puddles,25/40" ); >;
	// Обострение нормали в низине: усиливает деталь нормал-мапы (0 = как есть).
	float g_flPuddleBasinSharpen < Default( 1.3 );  Range( 0.0, 3.0 ); UiGroup( "Puddles,25/50" ); >;
	// Прибавка roughness в низине — сбивает однородный блик асфальта.
	float g_flPuddleBasinRough   < Default( 0.14 ); Range( 0.0, 0.4 ); UiGroup( "Puddles,25/60" ); >;
	// Мягкость кромки низины (доля высот шума на переход дно→полотно).
	float g_flPuddleBasinSoft    < Default( 0.22 ); Range( 0.02, 0.6 ); UiGroup( "Puddles,25/70" ); >;
	#endif

	SamplerState g_sSampler < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init( i );

		// База тайлится по мировым XY с тем же шагом, что заливка перекрёстка (repeat 500) → дорога и
		// junction выглядят и тайлятся одинаково, всё выровнено по миру. UV меша (U=across, V=флаг кромки)
		// в семпл базы НЕ участвует — нужен только секции EDGE (тангент-базис строится ниже из мира).
		float2 worldXY = i.vPositionWithOffsetWs.xy + g_vCameraPositionWs.xy;
		float2 vTexCoords = worldXY * ( 1.0 / 500.0 );

		// Дистанция до пикселя — общая для фейдов parallax/edge/разметки/луж.
		float flViewDist = length( i.vPositionWithOffsetWs.xyz );

		// Единый фейд детализации: 1 до 75% дистанции, дальше линейно в 0. Считается ОДИН раз
		// на пиксель и используется всеми фичами (кромка, грязь, трещины, детали разметки, лужи,
		// parallax) — отдельных дистанций у фич нет, чтобы не платить за пять разных фейдов.
		float detailFade = saturate( ( g_flDetailFadeDist - flViewDist ) * 4.0 / max( g_flDetailFadeDist, 1.0 ) );

		// ── Мировой тангент-базис (строим ДО parallax и нормали) ───────────────
		// База/нормаль/parallax тайлятся по мировым XY (vTexCoords = worldXY/500), а НЕ по UV меша.
		// Тангенты strip-меша (i.vTangentUWs/VWs) кривые/вырожденные — использовать их и для нормали,
		// и для parallax нельзя: сдвиг уезжал не в ту сторону (parallax «полз» вблизи вместо глубины).
		// Строим базис из гео-нормали и мировой оси X: vTanU совпадает с осью U текстуры (world +X),
		// vTanV — с осью V (world +Y). Один и тот же базис идёт и в parallax, и в нормаль-маппинг.
		float3 vGeoNormal = normalize( i.vNormalWs );
		float3 vWorldRef  = abs( vGeoNormal.x ) < 0.99 ? float3( 1, 0, 0 ) : float3( 0, 1, 0 );
		float3 vTanU      = normalize( vWorldRef - vGeoNormal * dot( vGeoNormal, vWorldRef ) );
		float3 vTanV      = cross( vGeoNormal, vTanU );

		// ── Parallax (опц.) ───────────────────────────────────────────────────
		#if S_PARALLAX
		{
			static const float flParallaxStrength = 0.035;
			static const float flParallaxNearStart = 12.0;
			static const float flParallaxNearRamp = 28.0;
			static const float flParallaxViewZMin = 0.35;
			static const float flParallaxMaxUvOffset = 0.012;

			float3 vCameraVec = -i.vPositionWithOffsetWs.xyz;

			// Дальний фейд — от общей дистанции детализации (Detail), ближний — как был.
			float flFarFade = detailFade;
			float flNearFade = saturate( ( flViewDist - flParallaxNearStart ) * rcp( flParallaxNearRamp ) );
			float flParallaxFade = flFarFade * flNearFade;

			// Ветвление по flParallaxFade тут НЕЛЬЗЯ: ниже идёт семпл карты высот, а он берёт
			// производные из квада — в квадах на границе фейда мип поехал бы. При fade=0 сдвиг
			// и так ноль (flHeightAdjusted = 0), так что ветка ничего бы и не сэкономила по картинке.
			// Вектор взгляда в ТОТ ЖЕ мировой тангент-базис, что и UV (world +X / +Y / гео-нормаль).
			// Так .xy сдвига совпадает с осями vTexCoords и parallax честно уводит текстуру в глубину.
			float3 vViewDirTs = float3(
				dot( vTanU, vCameraVec ),
				dot( vTanV, vCameraVec ),
				dot( vGeoNormal, vCameraVec )
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

		// ── Базовый PBR ───────────────────────────────────────────────────────
		// Тангент-базис (vGeoNormal/vTanU/vTanV) уже построен выше — общий с parallax.
		// Он из мировой XY-проекции (как база), а НЕ из UV меша: у дороги и перекрёстка разный UV,
		// UV-тангенты дали бы разный нормал-маппинг; мировой базис → дорога и junction совпадают.
		float3 colorSample  = Tex2DS( g_tColor,     g_sSampler, vTexCoords ).rgb;
		float3 normalSample = Tex2DS( g_tNormal,    g_sSampler, vTexCoords ).rgb;
		float  flRoughness  = Tex2DS( g_tRoughness, g_sSampler, vTexCoords ).r;

		m.Albedo = colorSample;
		m.Opacity = 1.0;
		normalSample = mad( normalSample, 2.0, -1.0 );

		// Детейл-нормаль: та же карта на мелком тайлинге поверх макро-тайла — высокочастотное зерно
		// асфальта, которого нет у одного тайла 1/500. Смешиваем «whiteout»-способом (складываем xy,
		// перемножаем z) — чище, чем простой lerp, не гасит рельеф. Сила детейла = отдельная ручка.
		if ( g_flRoadDetailStrength > 1e-3 )
		{
			float3 vDetail = Tex2DS( g_tNormal, g_sSampler, vTexCoords * g_flRoadDetailTile ).rgb;
			vDetail = mad( vDetail, 2.0, -1.0 );
			vDetail.xy *= g_flRoadDetailStrength;
			normalSample = normalize( float3( normalSample.xy + vDetail.xy, normalSample.z * vDetail.z ) );
		}

		// Сила базовой нормали: масштаб тангенциальной (xy) части, z пересобираем — управляет глубиной рельефа.
		normalSample.xy *= g_flRoadNormalStrength;
		normalSample = normalize( normalSample );

		m.Normal = TransformNormal( normalSample, vGeoNormal, vTanU, vTanV );
		// Весь спекуляр/отражения (env, SSR, area-lights) движок считает через m.WorldTangentU/V.
		// Material::Init положил туда мешевые тангенты дороги (у strip-меша кривые/вырожденные) →
		// рассинхрон с базисом нормали ломал vNormalTs и базис отражений: SSR/env «реагировали»
		// даже при полной roughness, поверхность темнела. Кладём тот же мировой базис, что и нормаль.
		m.WorldTangentU = vTanU;
		m.WorldTangentV = vTanV;
		m.Roughness = flRoughness * g_flRoughnessMultiplier;
		m.Metalness = 0.0;

		// ── EFFECT: Edge (мягкий неравномерный переход в камушки) ──────────────
		#if S_EDGE
		if ( i.vTextureCoords.y >= 0.0 ) // V<0 — флаг «кромка выключена» (дорога без кромки / перекрёсток)
		{
			// U=0 слева, U=1 справа → расстояние до ближайшего края (0 у края, 0.5 в центре).
			float across   = saturate( i.vTextureCoords.x );
			float edgeDist = min( across, 1.0 - across );

			// t: 0 в глубине дороги (внутр. граница полосы) → 1 у физического края. 0 вне полосы.
			float t = saturate( ( g_flEdgeWidth - edgeDist ) / max( g_flEdgeWidth, 1e-4 ) );

			// Фейд по дистанции: дальше шум субпиксельный — cutout мерцает, дырки закрываются
			// (alpha уходит в 1), шум и clip не считаются вовсе. До 75% дистанции fade = 1
			// (кромка в полную силу), гаснет только в последней четверти.
			float edgeFade = detailFade;

			// ── Утопленная кромка: полотно у края темнее, грубее и резче по нормали, плюс
			//    фейковый скос ВНИЗ к границе — край читается как просевшая обочина-канавка.
			//    Всё масштабируется на t (0 в глубине полосы → 1 у края), ALU, на любой дистанции. ──
			if ( t > 1e-4 )
			{
				m.Albedo *= 1.0 - t * saturate( g_flEdgeDarken );
				m.Roughness = saturate( m.Roughness + t * g_flEdgeRoughAdd );
				// Обострение детали нормали (экстраполяция прочь от гео-нормали, коэф. >1).
				float3 sharpN = normalize( lerp( vGeoNormal, m.Normal, 1.0 + g_flEdgeNormalSharpen * t ) );
				m.Normal = normalize( lerp( m.Normal, sharpN, t ) );
			}

			// Скос вниз к краю: высота падает к границе (h = -t·depth) → нормаль наклоняется, кромка
			// ловит свет как склон канавки, выглядит утопленной. Вне ветвления по t — ddx/ddy требуют
			// когерентного квада, t непрерывна (за пределами полосы h=0, наклона нет).
			if ( g_flEdgeDepth > 1e-4 )
			{
				float3 sgEdge = Road_SurfaceGrad( vGeoNormal, i.vPositionWithOffsetWs.xyz, -t * g_flEdgeDepth );
				m.Normal = normalize( m.Normal - sgEdge );
			}

			// ── Грязь с обочины: своя полоса (EdgeDirtWidth) шире камушков и заходит глубже
			//    внутрь полотна. Заметность = полоса^falloff (к центру резко в ноль = прозрачно)
			//    × шумовые клочки × Strength. Клочковатость по тем же мировым XY, что и база. ──
			// Грязь гаснет вместе с кромкой: вдали её полоса — пара пикселей, а стоит она
			// fbm + три тапа своей текстуры. Ниже edgeFade → расчёт пропускается целиком.
			if ( g_flEdgeDirtStrength > 1e-3 && edgeFade > 1e-3 )
			{
				float dirtBand = saturate( ( g_flEdgeDirtWidth - edgeDist ) / max( g_flEdgeDirtWidth, 1e-4 ) );
				dirtBand = pow( dirtBand, g_flEdgeDirtFalloff );
				float dirtN = saturate( Road_Fbm( worldXY / max( g_flEdgeDirtScale, 1.0 ) ) * 1.4 - 0.2 );
				float dirt = dirtBand * dirtN * saturate( g_flEdgeDirtStrength ) * edgeFade;

				// Грязь = отдельный материал (color/normal/roughness), тайлится по мировым XY тем же
				// базисом, что и полотно → ложится своей текстурой, а не плоским цветом.
				float2 dirtUV = worldXY / max( g_flEdgeDirtTexTile, 1.0 );
				m.Albedo = lerp( m.Albedo, Tex2DS( g_tDirtColor, g_sSampler, dirtUV ).rgb, dirt );
				m.Roughness = lerp( m.Roughness, Tex2DS( g_tDirtRoughness, g_sSampler, dirtUV ).r, dirt );
				float3 dirtNrm = mad( Tex2DS( g_tDirtNormal, g_sSampler, dirtUV ).rgb, 2.0, -1.0 );
				float3 dirtNws = TransformNormal( dirtNrm, vGeoNormal, vTanU, vTanV );
				m.Normal = normalize( lerp( m.Normal, dirtNws, dirt ) );
			}

			if ( t > 1e-5 )
			{
				if ( edgeFade > 1e-3 )
				{
					// Неравномерный шум камушков по абсолютным мировым XY (worldXY уже посчитан сверху).
					float pebbleN = Road_Fbm( worldXY / max( g_flPebbleScale, 1e-3 ) );

					// Порог дырок плавно растёт наружу → камни всё реже к краю (без жёсткой кромки).
					float level = saturate( pow( t, g_flFalloffPower ) );

					// Мягкий cutout: где шум выше порога — асфальт/камень, иначе дырка (виден террейн).
					float edgeAlpha = saturate( ( pebbleN - level ) / max( g_flEdgeSoftness, 1e-4 ) + 0.5 );

					// К дальней границе фейда дырки плавно зарастают.
					edgeAlpha = lerp( 1.0, edgeAlpha, edgeFade );

					clip( edgeAlpha - g_flEdgeCutout );
				}
			}
		}
		#endif

		// ── EFFECT: Markings (продольная разметка аналитически из U) ──────────
		// ДО Wetness/Puddles: краска мокнет и заливается лужами вместе с асфальтом.
		#if S_MARKINGS
		{
			// Знак V — флаг ТОЛЬКО кромки (см. секцию Edge выше), разметку он не гасит: раньше обе
			// секции сидели на одной проверке V>=0, и выключение кромки у дороги убирало заодно всю
			// разметку. Нужна дорога без разметки — материал без F_MARKINGS.
			float across = i.vTextureCoords.x;
			// AA-ширина пикселя в U: кромка линии всегда резкая ровно на 1 пиксель,
			// на любой дистанции и под любым углом. Субпиксельная линия вдали даёт
			// частичное покрытие (плавно тает, не мерцает) — мипов нет в принципе.
			float fwU = max( fwidth( across ), 1e-6 );
			float halfW = g_flMarkLineWidth * 0.5;

			float cov = 0.0;

			if ( g_bMarkEdgeLines )
			{
				float dE = min( abs( across - g_flMarkEdgeInset ),
				                abs( ( 1.0 - g_flMarkEdgeInset ) - across ) );
				cov = saturate( ( halfW - dE ) / fwU + 0.5 );
			}

			// Пунктир-фактор — общий для центральной-пунктира и разделителей полос.
			// Вдоль дороги: |V| = дистанция/500 (конвенция роадтула; знак — флаг кромки, поэтому abs).
			float along = abs( i.vTextureCoords.y ) * 500.0;
			float period = max( g_flMarkDashLen + g_flMarkDashGap, 1.0 );
			float ph = frac( along / period ) * period;
			float dOn = min( ph, g_flMarkDashLen - ph ); // >0 внутри штриха
			float fwA = max( fwidth( along ), 1e-4 );
			float dashF = saturate( dOn / fwA + 0.5 );
			// Вдали штрихи субпиксельны — сводим к средней заполненности, чтобы не муарило.
			dashF = lerp( dashF, g_flMarkDashLen / period, saturate( fwA / period * 2.0 - 0.2 ) );

			float dCenter = abs( across - 0.5 );

			if ( g_bMarkCenterLine )
			{
				// Двойная: две линии по бокам центра с зазором ~1.2 ширины.
				float dC = g_bMarkCenterDouble
					? abs( dCenter - g_flMarkLineWidth * 1.2 )
					: dCenter;
				float cCov = saturate( ( halfW - dC ) / fwU + 0.5 );
				if ( g_bMarkCenterDashed )
					cCov *= dashF;
				cov = max( cov, cCov );
			}

			// Разделители полос: при 2+ полосах в сторону — пунктир через равные доли
			// проезжей части (от центра до краевой линии), симметрично для обеих сторон.
			float lanes = round( g_flMarkLanesPerSide );
			if ( lanes > 1.5 )
			{
				float laneW = ( 0.5 - g_flMarkEdgeInset ) / lanes;
				[unroll]
				for ( int k = 1; k <= 3; k++ )
				{
					float on = (float)k <= lanes - 0.5 ? 1.0 : 0.0;
					float dD = abs( dCenter - laneW * (float)k );
					cov = max( cov, saturate( ( halfW - dD ) / fwU + 0.5 ) * dashF * on );
				}
			}

			// Детали краски (износ шумом и бортик) гаснут по общей дистанции детализации.
			// САМИ ЛИНИИ остаются на любой дальности — они аналитические и почти бесплатные,
			// а исчезающая вдали разметка читалась бы как пустое полотно.
			float markDetail = detailFade;

			float paint = 0.0;
			if ( cov > 0.001 )
			{
				// Износ: шум по мировым XY стирает краску пятнами (считается только на линиях).
				float keep = 1.0;
				if ( g_flMarkWear > 0.001 && markDetail > 1e-3 )
				{
					float wearN = Road_Fbm( worldXY * ( 1.0 / 45.0 ) );
					// Вдали краска ровная (keep→1), вблизи — облезлая как раньше.
					keep = lerp( 1.0, smoothstep( g_flMarkWear * 0.9 - 0.35, g_flMarkWear * 0.9 + 0.15, wearN ), markDetail );
				}

				paint = cov * saturate( g_flMarkStrength ) * keep;
			}

			// Бортик краски: слой имеет толщину — по краям линий, концам штрихов и сколам
			// нормаль наклоняется наружу (экранный градиент высоты = маски краски).
			// Вне ветвлений по покрытию: ddx/ddy требуют когерентного потока в квадах,
			// у пикселей без краски paint=0 → градиент честный.
			// Условие ветки — только юниформы: внутри ddx/ddy, а markDetail меняется по пикселям
			// (в квадах на границе фейда производные стали бы мусором). Гасим сам бортик через
			// высоту: при markDetail=0 градиент нулевого поля и так ноль.
			if ( g_flMarkBevel > 0.001 )
			{
				float3 sg = Road_SurfaceGrad( vGeoNormal, i.vPositionWithOffsetWs.xyz, paint * ( g_flMarkBevel * 0.5 * markDetail ) );
				m.Normal = normalize( m.Normal - sg );
			}

			if ( paint > 0.001 )
			{
				m.Albedo = lerp( m.Albedo, g_vMarkColor, paint );
				m.Roughness = lerp( m.Roughness, g_flMarkRoughness, paint );
			}
		}
		#endif

		// ── EFFECT: Wetness (мокрый асфальт от дождя) ──────────────────────────
		float flWet = saturate( g_flWetness ) * saturate( g_flWetnessResponse );
		{
			m.Albedo *= lerp( 1.0, saturate( g_flWetDarken ), flWet );
			m.Roughness = saturate( lerp( m.Roughness, m.Roughness * saturate( g_flWetRoughness ), flWet ) );
		}

		// ── EFFECT: Puddles (лужи в дождь + сухие низины, комбо F_PUDDLES) ─────
		#if S_PUDDLES
		// Лужи и низины — по общей дистанции детализации: вдали чаша меньше пикселя,
		// а стоит она fbm + (в дождь) две октавы value-шума ряби на каждый пиксель.
		float puddleFade = detailFade;
		if ( puddleFade > 1e-3 )
		{
			// Псевдовысота полотна по крупному шуму (мировые XY): низкие места = чаши,
			// куда стекает вода. Считаем ОДИН раз — общая база для сухих низин и залива.
			float h = Road_Fbm( worldXY / max( g_flPuddleScale, 1.0 ) );
			float hAdj = h;

			// ── Сухая неоднородность: чаши видны и без дождя как более тёмный/резкий
			//    асфальт. Маска = полный контур потенциальной лужи (уровень при Wetness=1),
			//    чтобы затемнение очерчивало те же низины, что позже зальёт вода. ──
			float basin = smoothstep( 0.0, max( g_flPuddleBasinSoft, 1e-3 ),
				saturate( g_flPuddleLevel ) - hAdj ) * puddleFade;
			if ( basin > 1e-3 )
			{
				m.Albedo *= 1.0 - basin * saturate( g_flPuddleBasinDarken );
				m.Roughness = saturate( m.Roughness + basin * g_flPuddleBasinRough );
				// Обострение нормали: экстраполируем деталь нормал-мапы прочь от гео-нормали
				// (lerp c коэффициентом >1) — рельеф асфальта в низине читается резче.
				float3 sharpN = normalize( lerp( vGeoNormal, m.Normal,
					1.0 + g_flPuddleBasinSharpen * basin ) );
				m.Normal = normalize( lerp( m.Normal, sharpN, basin ) );
			}

			// ── Мокрый залив: уровень воды растёт с Wetness. В сухую погоду пропущен. ──
			if ( flWet > 1e-3 )
			{
				float level = flWet * saturate( g_flPuddleLevel );
				float water = smoothstep( 0.0, 0.12, level - hAdj ) * puddleFade;

				if ( water > 1e-3 )
				{
					// Дно темнее; гладь — плоская гео-нормаль + рябь от капель; зеркальный roughness.
					// Отражения (env/SSR) движок даёт сам через низкий roughness.
					m.Albedo *= 1.0 - water * 0.35;

					float2 ruv = worldXY * ( 1.0 / 7.0 );
					float2 rn = float2(
						Road_ValueNoise( ruv + g_flTime * float2( 1.7, 1.3 ) ),
						Road_ValueNoise( ruv * 1.7 - g_flTime * float2( 1.1, 2.1 ) ) ) - 0.5;
					rn *= g_flRippleStrength * flWet * 0.35;

					float3 waterN = normalize( vGeoNormal + vTanU * rn.x + vTanV * rn.y );
					m.Normal = normalize( lerp( m.Normal, waterN, water ) );
					m.Roughness = lerp( m.Roughness, 0.02, water );
				}
			}
		}
		#endif

		// ── Здесь добавлять новые эффекты дороги: новая фича F_* + своя секция ──

		return ShadingModelStandard::Shade( m );
	}
}
