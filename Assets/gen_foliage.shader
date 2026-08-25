FEATURES
{
    #include "common/features.hlsl"
    Feature( F_ALPHA,     0..1, "Features" );
    // Билбординг чекбоксом (дефолт выкл): карточка поворачивается за камерой (деревья/
    // кусты-карты). Выкл — обычная листва с фиксированной геометрией (меши травы, кустов).
    Feature( F_BILLBOARD, 0..1, "Features" );
    // Рафнес включён по умолчанию (0 = "On"). "Off" полностью выкидывает карту/множитель
    // шероховатости — поверхность плоско-матовая, минус текстурная выборка.
    Feature( F_ROUGHNESS, 0..1( 0 = "On", 1 = "Off" ), "Features" );
    // Дешёвое освещение (как S_GRASS_CHEAP_LIGHT у травы террейна): только диффузный ambient
    // из проб/неба, без кластера источников, теней и спекуляра. Для массовой листвы — заметно
    // дешевле полного PBR, а на матовых листьях разницы почти нет.
    Feature( F_CHEAP_LIGHT, 0..1, "Features" );
    // Ветер чекбоксом (дефолт выкл). Амплитуда = высота вершины (нормировка g_flCardHeight,
    // качается только верх) × текстурная маска ветра (R: белое = качается, чёрное = жёстко;
    // дефолт белый — без маски чистый изгиб по высоте). Направление/скорость/сила ветра —
    // глобальные атрибуты WindDirection/WindSpeed/WindStrength из WeatherController,
    // общие с травой gen_terrain.
    Feature( F_WIND, 0..1, "Features" );
    // F_RENDER_BACKFACES приходит из common/features.hlsl — здесь только реализация:
    // куллинг выкл + разворот нормали на бекфейсе (см. PS).
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
	// При S_RENDER_BACKFACES движок сам добавляет сюда face : SV_IsFrontFace.
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	StaticCombo( S_BILLBOARD, F_BILLBOARD, Sys( ALL ) );
	StaticCombo( S_WIND, F_WIND, Sys( ALL ) );

	#if S_WIND
	// Глобальный ветер из WeatherController (Scene.RenderAttributes) — общий с травой террейна.
	// Дефолты совпадают с дефолтами контроллера, чтобы листва жила и без него.
	float3 g_vWindDir            < Attribute( "WindDirection" ); Default3( 1.0, 0.0, 0.0 ); >;
	float g_flGlobalWindSpeed    < Attribute( "WindSpeed" );     Default( 2.5 ); >;
	float g_flGlobalWindStrength < Attribute( "WindStrength" );  Default( 6.0 ); >;

	// Высота карточки/куста в мировых юнитах — нормировка ветрового изгиба (качается только верх).
	float g_flCardHeight   < Default( 80.0 ); Range( 8.0, 512.0 ); UiGroup( "Wind,20/30" ); >;

	// Маска ветра по UV (R): белое = качается, чёрное = жёстко (ствол, основание веток).
	// Дефолт белый — маска нейтральна. Семплится в VS на нулевом мипе.
	CreateInputTexture2D( TextureWindMask, Linear, 8, "", "_wind", "Wind,20/40", Default( 1.0 ) );
	CreateTexture2D( g_tWindMask ) < Channel( R, Box( TextureWindMask ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	SamplerState g_sWindSampler < Filter( BILINEAR ); AddressU( WRAP ); AddressV( WRAP ); >;
	#endif

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );

		float3x4 matObjectToWorld = GetTransformMatrix( i.nInstanceTransformID );
		float3 vObjOriginWs = mul( matObjectToWorld, float4( 0, 0, 0, 1 ) ).xyz;

		#if S_BILLBOARD
		{
			// Карточка авторится в плоскости XZ: локальный X = ширина, Z = высота.
			float3 vLocal = i.vPositionOs.xyz;

			// Мировой масштаб инстанса вдоль локальных X и Z — билборд не теряет размер
			// при масштабировании объекта (float3x4 хранит строки, берём столбцы 3x3).
			float flScaleX = length( float3( matObjectToWorld[0][0], matObjectToWorld[1][0], matObjectToWorld[2][0] ) );
			float flScaleZ = length( float3( matObjectToWorld[0][2], matObjectToWorld[1][2], matObjectToWorld[2][2] ) );

			// Цилиндрический билборд: поворот только по рысканью (ствол вертикален).
			float3 vToCam = g_vCameraPositionWs - vObjOriginWs;
			vToCam.z = 0.0;
			float3 vUp    = float3( 0.0, 0.0, 1.0 );
			float3 vRight = normalize( cross( vUp, vToCam ) );

			o.vPositionWs = vObjOriginWs
			              + vRight * ( vLocal.x * flScaleX )
			              + vUp    * ( vLocal.z * flScaleZ );

			// Мягкая листвяная нормаль: смотрит вверх с наклоном к камере. Не жёстко в камеру —
			// иначе яркость дерева скачет при облёте и свет читается «сломанным».
			o.vNormalWs = normalize( vUp + normalize( vToCam ) * 0.4 );
		}
		#else
		{
			o.vPositionWs = mul( matObjectToWorld, float4( i.vPositionOs.xyz, 1.0 ) );
		}
		#endif

		#if S_WIND
		{
			// Амплитуда на вершину: нормированная высота × текстурная маска (белый = без маски).
			float flTexMask = g_tWindMask.SampleLevel( g_sWindSampler, i.vTexCoord.xy, 0 ).r;
			float flMask = saturate( i.vPositionOs.z / max( g_flCardHeight, 1.0 ) ) * flTexMask;

			// Ветровая фаза от позиции объекта — соседние инстансы качаются вразнобой.
			float flObjPhase = dot( vObjOriginWs, float3( 1.0, 1.7, 0.0 ) );
			float flTime = g_flTime * g_flGlobalWindSpeed;
			float flSway = sin( flTime       + flObjPhase ) * 0.6
			             + sin( flTime * 1.7 + flObjPhase * 2.3 ) * 0.4;

			// Не симметричное качание вокруг нуля, а лежащий по ветру изгиб с пульсом порывов
			// (как трава террейна) — направление ветра читается глазом. Смещение в мировых XY,
			// работает одинаково для меша и билборда.
			float flLean = 0.3 + 0.7 * ( 0.5 + 0.5 * flSway );
			float2 vWindDir = normalize( g_vWindDir.xy + 1e-4 );

			o.vPositionWs.xyz += float3( vWindDir, 0.0 )
			                   * ( flLean * flMask * g_flGlobalWindStrength );
		}
		#endif

		o.vPositionPs = Position3WsToPs( o.vPositionWs.xyz );

		return FinalizeVertex( o );
	}
}

PS
{
	#include "common/pixel.hlsl"
	#include "common/classes/AmbientLight.hlsl"

	StaticCombo( S_ALPHA, F_ALPHA, Sys( ALL ) );
	StaticCombo( S_NO_ROUGHNESS, F_ROUGHNESS, Sys( ALL ) );
	StaticCombo( S_CHEAP_LIGHT, F_CHEAP_LIGHT, Sys( ALL ) );
	StaticCombo( S_RENDER_BACKFACES, F_RENDER_BACKFACES, Sys( ALL ) );

	#if S_RENDER_BACKFACES
		RenderState( CullMode, NONE );
	#endif

	CreateInputTexture2D( TextureColor, Srgb, 8, "", "_color", "Material,10/10", Default3( 1.0, 1.0, 1.0 ) );
	CreateTexture2D( g_tColor ) < Channel( RGBA, Box( TextureColor ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;

	// Опасити — ОТДЕЛЬНОЙ картой (R-канал, фильтр Box): работает на текстурах любого размера
	// (в т.ч. не степень двойки) и с мип-компенсацией держит тонкую листву/хвою вдали.
	#if S_ALPHA
	CreateInputTexture2D( TextureOpacity, Linear, 8, "", "_opacity", "Alpha,30/10", Default( 1.0 ) );
	CreateTexture2D( g_tOpacity ) < Channel( R, Box( TextureOpacity ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

	float g_flOpacityCutoff < Default( 0.5 ); Range( 0.0, 1.0 ); UiGroup( "Alpha,30/20" ); >;
	float g_flOpacityMipBoost < Default( 0.5 ); Range( 0.0, 1.0 ); UiGroup( "Alpha,30/30" ); >;
	#endif

	// Рафнес за комбо-фичей (F_ROUGHNESS). "Off" → не компилится вообще (плоско-матовая листва).
	#if ( S_NO_ROUGHNESS == 0 )
	CreateInputTexture2D( TextureRoughness, Linear, 8, "", "_rough", "Material,10/40", Default( 1.0 ) );
	CreateTexture2D( g_tRoughness ) < Channel( R, Box( TextureRoughness ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	float g_flRoughness < Default( 1.0 ); Range( 0.0, 1.0 ); UiGroup( "Material,10/50" ); >;
	#endif

	SamplerState g_sSampler < Filter( ANISO ); MaxAniso( 16 ); AddressU( WRAP ); AddressV( WRAP ); >;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init( i );

		float2 vTexCoords = i.vTextureCoords.xy;
		float4 colorSample = Tex2DS( g_tColor, g_sSampler, vTexCoords );

		m.Albedo    = colorSample.rgb;
		m.Metalness = 0.0;

		#if S_RENDER_BACKFACES
		// Тыльная сторона листа: нормаль разворачиваем, иначе бекфейс освещается «спиной» к свету.
		m.Normal = i.face ? i.vNormalWs : -i.vNormalWs;
		#else
		m.Normal = i.vNormalWs;
		#endif

		#if S_NO_ROUGHNESS
		m.Roughness = 1.0;
		#else
		m.Roughness = Tex2DS( g_tRoughness, g_sSampler, vTexCoords ).r * g_flRoughness;
		#endif

		#if S_ALPHA
		{
			// Мип-компенсация: exp2( mip * boost ) держит тонкую хвою/листву вдали (1D-фичи
			// затухают как 2^-mip и иначе провалились бы под cutoff → дерево «лысеет»), а
			// точечный шум фона затухает как 2^-2mip и не вылезает. Отрицательный bias берёт
			// мип резче, чтобы изолиния выреза не расползалась в ширину.
			static const float flOpacityMipMax = 7.0;
			static const float flOpacityMipBias = -1.5;

			float flOpacity = g_tOpacity.SampleBias( g_sSampler, vTexCoords, flOpacityMipBias ).r;
			float flMip = max( g_tOpacity.CalculateLevelOfDetail( g_sSampler, vTexCoords ) + flOpacityMipBias, 0.0 );
			flOpacity = saturate( flOpacity * exp2( min( flMip, flOpacityMipMax ) * g_flOpacityMipBoost ) );

			float flEdge = ( flOpacity - g_flOpacityCutoff ) / max( fwidth( flOpacity ), 0.001 ) + 0.5;
			clip( flEdge - 0.5 );
			m.Opacity = saturate( flEdge );
		}
		#else
		m.Opacity = 1.0;
		#endif

		#if S_CHEAP_LIGHT
		// Дешёвый свет (как у травы террейна): только диффузный ambient из проб/неба —
		// без кластера источников, теней и спекуляра. Рафнес/металл тут не участвуют.
		float3 ambient = AmbientLight::From( m.WorldPosition, m.ScreenPosition, m.Normal );
		return float4( m.Albedo * ambient, 1.0 );
		#else
		return ShadingModelStandard::Shade( m );
		#endif
	}
}
