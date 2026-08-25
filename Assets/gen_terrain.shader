//
// Террейн (копия движкового base/terrain) + влажность (глобал "Wetness") +
// GPU-трава прямо в шейдере через geometry shader. Трава появляется только на
// материале g_nGrassMaterial. Назначается на Terrain.MaterialOverride.
//
// Поток: VS считает позицию/нормаль/uv → GS делает passthrough треугольника
// террейна и доэмитит лезвия травы на травяном материале → PS ветвится трава/террейн.
// Плотность травы привязана к тесселяции клипмапа (густо у камеры, редко вдали) = авто-LOD.
//

HEADER
{
	Description = "Gen Terrain (wet + GPU grass)";
    DevShader = true;
    DebugInfo = false;
}

FEATURES
{
    #include "vr_common_features.fxc"
    Feature( F_GRASS_TEXTURE, 0..1, "Grass" );
    Feature( F_GRASS_CHEAP_LIGHT, 0..1, "Grass Cheap Lighting" );
}

MODES
{
    Forward();
    Depth( S_MODE_DEPTH );
}

COMMON
{
    #define CUSTOM_MATERIAL_INPUTS

    #include "common/shared.hlsl"
    #include "common/Bindless.hlsl"
    #include "terrain/TerrainCommon.hlsl"

    int g_nDebugView < Attribute( "DebugView" ); >;
    int g_nPreviewLayer < Attribute( "PreviewLayer" ); >;

    bool g_bVertexDisplacement < Attribute( "VertexDisplacement" ); Default( 0 ); >;

    // Глобальная влажность 0..1 — пишется из C# (WeatherController) каждый кадр.
    float g_flWetness < Attribute( "Wetness" ); Default( 0.0 ); >;

    /// <summary>Затемнение мокрого альбедо на пике дождя (0 = чёрный, 1 = без затемнения). Дождь ТОЛЬКО
    /// темнит террейн — глянец/отражения не добавляются, поэтому множителя шероховатости больше нет.</summary>
    float g_flWetDarken < Default( 0.6 ); Range( 0.0, 1.0 ); UiGroup( "Wetness,90/10" ); >;

    // ===== Трава =====
    // Глобальный рантайм-тумблер травы. Пишется из C#: Scene.RenderAttributes.Set( "GrassEnabled", 0f/1f )
    // (как Wetness/Wind). 0 — трава не эмитится вообще (ранний выход в GS). Материальные параметры ниже — это
    // настройка внешнего вида, а этот атрибут — вкл/выкл из настроек графики без правки материала.
    float g_flGrassEnabled < Attribute( "GrassEnabled" ); Default( 1.0 ); >;

    /// <summary>ID террейн-материала, на котором растёт трава (0..31).</summary>
    int g_nGrassMaterial < Default( 0 ); Range( 0, 31 ); UiGroup( "Grass,80/10" ); >;

    /// <summary>Минимальный вес травяного материала в треугольнике, чтобы росла трава.</summary>
    float g_flGrassMaterialThreshold < Default( 0.5 ); Range( 0.0, 1.0 ); UiGroup( "Grass,80/20" ); >;

    /// <summary>Высота лезвия (units).</summary>
    float g_flGrassHeight < Default( 24.0 ); Range( 1.0, 200.0 ); UiGroup( "Grass,80/30" ); >;

    /// <summary>Ширина билборда травы (units).</summary>
    float g_flGrassWidth < Default( 16.0 ); Range( 0.2, 80.0 ); UiGroup( "Grass,80/40" ); >;

    /// <summary>Плотность: доля треугольников с травой (0..1).</summary>
    float g_flGrassDensity < Default( 0.5 ); Range( 0.0, 1.0 ); UiGroup( "Grass,80/50" ); >;

    /// <summary>Кол-во травинок на треугольник (1..16) — основной рычаг густоты.</summary>
    int g_nGrassBladesPerTri < Default( 6 ); Range( 1, 16 ); UiGroup( "Grass,80/55" ); >;

    /// <summary>Порог выреза маски формы (выше = больше срезает).</summary>
    float g_flGrassAlphaCutoff < Default( 0.3 ); Range( 0.0, 1.0 ); UiGroup( "Grass,80/56" ); >;

    /// <summary>Дальность, после которой трава не рисуется (units).</summary>
    float g_flGrassMaxDistance < Default( 1600.0 ); Range( 100.0, 12000.0 ); UiGroup( "Grass,80/60" ); >;

    /// <summary>Дальше этой дистанции крест вырождается в один билборд к камере (вблизи детали креста не видно; дешевле).</summary>
    float g_flGrassBillboardDistance < Default( 700.0 ); Range( 0.0, 12000.0 ); UiGroup( "Grass,80/62" ); >;

    /// <summary>Доля доворота верха билборда к камере по вертикали при взгляде сверху (0 — всегда строго вертикально, 1 — сильный наклон ~45°). Маленькие значения закрывают просветы сверху, не выдавая, что трава крутится вверх/вниз.</summary>
    float g_flGrassBillboardVerticalTilt < Default( 0.3 ); Range( 0.0, 1.0 ); UiGroup( "Grass,80/63" ); >;

    // Ветер — ГЛОБАЛЬНЫЙ (пишет WeatherController через Scene.RenderAttributes).
    // Одно направление/сила на всю траву. Стоимость = чтение константы (ноль).
    float3 g_vWindDir < Attribute( "WindDirection" ); Default3( 1.0, 0.0, 0.0 ); >;
    float g_flWindSpeed < Attribute( "WindSpeed" ); Default( 2.5 ); >;
    float g_flWindStrength < Attribute( "WindStrength" ); Default( 6.0 ); >;

    // Цвет травы (текстура — только маска формы). Низ темнее, верх светлее.
    float3 g_vGrassBottomColor < Default3( 0.05, 0.14, 0.03 ); UiType( Color ); UiGroup( "Grass,80/85" ); >;
    float3 g_vGrassTopColor < Default3( 0.22, 0.45, 0.10 ); UiType( Color ); UiGroup( "Grass,80/90" ); >;

    float GrassHash21( float2 p )
    {
        p = frac( p * float2( 123.34, 456.21 ) );
        p += dot( p, p + 45.32 );
        return frac( p.x * p.y );
    }

    // 2D-хэш → две независимые случайные величины за один вызов. Нужен чтобы
    // травинки внутри треугольника разбрасывались равномерно, а не «рядами»
    // (одномерный GrassHash21 для обеих barycentric-координат даёт корреляцию).
    float2 GrassHash22( float2 p )
    {
        float3 p3 = frac( p.xyx * float3( 0.1031, 0.1030, 0.0973 ) );
        p3 += dot( p3, p3.yzx + 33.33 );
        return frac( ( p3.xx + p3.yz ) * p3.zy );
    }
}

struct VertexInput
{
	float3 PositionAndLod : POSITION < Semantic( PosXyz ); >;
	uint InstanceID : SV_InstanceID < Semantic( InstanceTransformUv ); >;
};

// Выход VS → вход GS. SV_Position здесь нет — её ставит GS.
struct GS_INPUT
{
    float3 LocalPosition : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float3 GeoNormal     : TEXCOORD2;
    float2 ControlUv     : TEXCOORD3;
};

struct PixelInput
{
    float3 LocalPosition : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    nointerpolation float IsGrass : TEXCOORD2;
    float2 GrassUv       : TEXCOORD3;

    #if ( ( PROGRAM == VFX_PROGRAM_VS ) || ( PROGRAM == VFX_PROGRAM_GS ) )
        float4 PixelPosition : SV_Position;
    #endif

    #if ( PROGRAM == VFX_PROGRAM_PS )
        float4 ScreenPosition : SV_Position;
        bool IsFrontFace : SV_IsFrontFace; // для ручного культинга обратных граней террейна
    #endif
};

VS
{
    #include "terrain/TerrainClipmap.hlsl"

	GS_INPUT MainVs( VertexInput i )
	{
        GS_INPUT o;

        // Клипмап теперь инстансится меблетами: позиция блока берётся из g_TerrainMeshlets по SV_InstanceID.
        TerrainMeshlet meshlet = g_TerrainMeshlets[i.InstanceID];

        Texture2D tHeightMap = Bindless::GetTexture2D( Terrain::Get().HeightMapTexture );
        float flLodLevel;
        o.LocalPosition = Terrain_ClipmapMeshlet( i.PositionAndLod.xy, meshlet, tHeightMap, Terrain::Get().UnitsPerTexel, flLodLevel );

        o.LocalPosition.z *= Terrain::Get().HeightScale;

        float2 texSize = TextureDimensions2D( tHeightMap, 0 );
        float2 uv = o.LocalPosition.xy / ( texSize * Terrain::Get().UnitsPerTexel );

        float2 texelSize = 1.0f / texSize;
        float center = abs( tHeightMap.SampleLevel( g_sBilinearBorder, uv, 0 ).r );
        float r = abs( tHeightMap.SampleLevel( g_sBilinearBorder, uv + texelSize * float2( 1, 0 ), 0 ).r );
        float b = abs( tHeightMap.SampleLevel( g_sBilinearBorder, uv + texelSize * float2( 0, 1 ), 0 ).r );

        float normalStrength = Terrain::Get().HeightScale / Terrain::Get().UnitsPerTexel;
        float3 geoNormal = normalize( float3( center - r, (b - center) * -1, 1.0f / normalStrength ) );

        geoNormal = normalize( mul( Terrain::Get().Transform, float4( geoNormal, 0.0 ) ).xyz );

    #if ( D_GRID == 0 )
        if ( g_bVertexDisplacement && Terrain::Get().ControlMapTexture != 0 )
        {
            float totalDisplacement = 0.0f;

            Texture2D tControlMap = Bindless::GetTexture2D( Terrain::Get().ControlMapTexture );
            float rawPixel = tControlMap.SampleLevel( g_sPointClamp, uv, 0 ).r;
            CompactTerrainMaterial material = CompactTerrainMaterial::DecodeFromFloat( rawPixel );

            TerrainMaterial mat = g_TerrainMaterials[material.BaseTextureId];
            float2 baseLayerUV = ( o.LocalPosition.xy / 32.0f ) * mat.uvscale;

            if( mat.HasFlag( TerrainFlags::NoTile ) )
                baseLayerUV = Terrain_SampleSeamlessUV( baseLayerUV );

            float4 baseNho = Bindless::GetTexture2D( mat.nho_texid ).SampleLevel( g_sAnisotropic, baseLayerUV, 0 );
            float baseDisplacement = ( baseNho.b - 0.5f ) * 2.0f * mat.displacementscale;

            mat = g_TerrainMaterials[material.OverlayTextureId];
            float2 overlayLayerUV = ( o.LocalPosition.xy / 32.0f ) * mat.uvscale;

            if( mat.HasFlag( TerrainFlags::NoTile ) )
                overlayLayerUV = Terrain_SampleSeamlessUV( overlayLayerUV );

            float4 overlayNho = Bindless::GetTexture2D( mat.nho_texid ).SampleLevel( g_sAnisotropic, overlayLayerUV, 0 );
            float overlayDisplacement = ( overlayNho.b - 0.5f ) * 2.0f * mat.displacementscale;

            float blend = material.GetNormalizedBlend();
            totalDisplacement = lerp( baseDisplacement, overlayDisplacement, blend );

            static const float DISPLACEMENT_FADE_START_LOD = 2.0f;
            static const float DISPLACEMENT_FADE_RANGE = 2.0f;
            float lodLevel = flLodLevel;
            float displacementFade = saturate(1.0 - (lodLevel - DISPLACEMENT_FADE_START_LOD) / DISPLACEMENT_FADE_RANGE);

            o.LocalPosition.xyz += geoNormal * totalDisplacement * displacementFade;
        }
    #endif

        o.WorldPosition = mul( Terrain::Get().Transform, float4( o.LocalPosition, 1.0 ) ).xyz;
        o.GeoNormal = geoNormal;
        o.ControlUv = uv;

        // Дырки в террейне — вырождаем вершину (NaN), треугольник отбросится.
        if ( Terrain::Get().ControlMapTexture != 0 )
        {
            Texture2D tControlMap = Bindless::GetTexture2D( Terrain::Get().ControlMapTexture );
            float rawPixel = tControlMap.SampleLevel( g_sPointClamp, uv, 0 ).r;
            CompactTerrainMaterial material = CompactTerrainMaterial::DecodeFromFloat( rawPixel );

            if ( material.IsHole )
            {
                o.LocalPosition = float3( 0. / 0., 0, 0 );
                o.WorldPosition = mul( Terrain::Get().Transform, float4( o.LocalPosition, 1.0 ) ).xyz;
            }
        }

		return o;
	}
}

//=========================================================================================================================

GS
{
    StaticCombo( S_GRASS_TEXTURE, F_GRASS_TEXTURE, Sys( ALL ) );

    // Заполнить выходную вершину для passthrough-треугольника террейна.
    void EmitTerrainVertex( inout TriangleStream<PixelInput> stream, GS_INPUT v )
    {
        PixelInput o = (PixelInput)0;
        o.LocalPosition = v.LocalPosition;
        o.WorldPosition = v.WorldPosition;
        o.IsGrass = 0.0;
        o.GrassUv = float2( 0, 0 );
        o.PixelPosition = Position3WsToPs( v.WorldPosition );
        GSAppendVertex( stream, o );
    }

    void EmitGrassVertex( inout TriangleStream<PixelInput> stream, float3 worldPos, float2 grassUv )
    {
        PixelInput o = (PixelInput)0;
        o.LocalPosition = float3( 0, 0, 0 );
        o.WorldPosition = worldPos;
        o.IsGrass = 1.0;
        o.GrassUv = grassUv;
        o.PixelPosition = Position3WsToPs( worldPos );
        GSAppendVertex( stream, o );
    }

    // Один треугольник лезвия вокруг направления ширины right: низ-лево, низ-право, вершина.
    void EmitBladeTri( inout TriangleStream<PixelInput> stream, float3 ground, float3 right, float3 tip, float w )
    {
        float3 bl = ground - right * w * 0.5;
        float3 br = ground + right * w * 0.5;

        EmitGrassVertex( stream, bl, float2( 0, 0 ) );
        EmitGrassVertex( stream, br, float2( 1, 0 ) );
        EmitGrassVertex( stream, tip, float2( 0.5, 1 ) );
        GSRestartStrip( stream );
    }

    // Билборд к камере по горизонтали + небольшой вертикальный доворот верха к камере
    // при взгляде сверху. upness 0 (камера сбоку) → 1 (камера над травой): чем выше
    // камера, тем сильнее наклон, но всегда в пределах g_flGrassBillboardVerticalTilt,
    // поэтому при обычном взгляде разворот вверх/вниз незаметен, а сверху нет просветов.
    void EmitCameraBillboard( inout TriangleStream<PixelInput> stream, float3 ground, float3 tip, float w )
    {
        float3 up = float3( 0, 0, 1 );
        float3 toCam = g_vCameraPositionWs.xyz - ground;
        float3 viewDir = -normalize( toCam );
        float3 right = normalize( cross( viewDir, up ) );

        float upness = saturate( normalize( toCam ).z );
        float3 toCamFlat = normalize( ( toCam - up * toCam.z ) + right * 1e-4 );
        tip -= toCamFlat * ( length( tip - ground ) * g_flGrassBillboardVerticalTilt * upness );

        EmitBladeTri( stream, ground, right, tip, w );
    }

    // Травинка. Вершина наклонена по ветру (прямой наклон, без изгиба).
    // Текстурная трава: вблизи — скрещенные лезвия (2 треуг. под 90°, видно с любого угла);
    // на дистанции (billboard=true) — один треугольник, разворачивается к камере (дёшево, разницы не видно).
    // Меш-режим — всегда один треугольник, билборд шириной к камере.
    void EmitBlade( inout TriangleStream<PixelInput> stream, float3 ground, float seed, bool billboard )
    {
        float h = g_flGrassHeight * (0.7 + 0.6 * GrassHash21( ground.xy + seed ));
        float w = g_flGrassWidth * (0.85 + 0.3 * GrassHash21( ground.yx + seed * 1.7 ));

        float3 up = float3( 0, 0, 1 );

        // Наклон ВСЕГДА по ветру (порыв 0..1), без качания через вертикаль.
        float phase = GrassHash21( ground.xy * 0.5 + seed ) * 6.2831;
        float gust = 0.5 + 0.5 * sin( g_flTime * g_flWindSpeed + phase );
        float bend = ( 0.35 + 0.65 * gust ) * g_flWindStrength;
        float2 windDir = normalize( g_vWindDir.xy + 1e-4 );
        float3 tip = ground + up * h + float3( windDir * bend, 0 );

    #if S_GRASS_TEXTURE
        if ( billboard )
        {
            // Дальний LOD: один билборд к камере (+ вертикальный доворот сверху).
            EmitCameraBillboard( stream, ground, tip, w );
        }
        else
        {
            // Вблизи: случайная фиксированная ориентация + перпендикуляр → крест.
            float ang = GrassHash21( ground.xy * 0.7 + seed * 2.3 ) * 6.2831853;
            float3 r1 = float3( cos( ang ), sin( ang ), 0 );
            float3 r2 = float3( -r1.y, r1.x, 0 ); // +90°
            EmitBladeTri( stream, ground, r1, tip, w );
            EmitBladeTri( stream, ground, r2, tip, w );
        }
    #else
        EmitCameraBillboard( stream, ground, tip, w );
    #endif
    }

    // Лимит GS — не вершины, а СУММАРНЫЕ output-компоненты: maxvertexcount * 13 (компонентов
    // в PixelInput) <= 1024 → не больше 78 вершин. Текстурный крест = 6 верш./лезвие → макс 12
    // лезвий (3 + 12*6 = 75); меш = 3 верш./лезвие → влезает 16 (3 + 16*3 = 51).
    #if S_GRASS_TEXTURE
    [maxvertexcount(75)] // 3 террейн + 12 лезвий * 2 треуг. * 3 верш.
    #else
    [maxvertexcount(51)] // 3 террейн + 16 лезвий * 3 верш.
    #endif
    void MainGs( triangle GS_INPUT i[3], inout TriangleStream<PixelInput> stream )
    {
        // 1) Сам террейн (passthrough).
        EmitTerrainVertex( stream, i[0] );
        EmitTerrainVertex( stream, i[1] );
        EmitTerrainVertex( stream, i[2] );
        GSRestartStrip( stream );

    #if ( S_MODE_DEPTH == 0 )
        // 2) Трава (только в обычном проходе, не в depth).
        // Глобальный тумблер из C# (настройки графики) — 0 отключает траву целиком.
        if ( g_flGrassEnabled < 0.5 )
            return;

        if ( Terrain::Get().ControlMapTexture == 0 )
            return;

        float3 c = ( i[0].WorldPosition + i[1].WorldPosition + i[2].WorldPosition ) / 3.0;
        float2 cuv = ( i[0].ControlUv + i[1].ControlUv + i[2].ControlUv ) / 3.0;

        // Дальность.
        float dist = distance( c, g_vCameraPositionWs.xyz );
        if ( dist >= g_flGrassMaxDistance )
            return;

        // LOD: вдали меньше лезвий (дешевле GS).
        float distN = saturate( dist / g_flGrassMaxDistance );

        // Плотность (часть треугольников без травы).
        if ( GrassHash21( c.xy * 0.013 ) > g_flGrassDensity )
            return;

        // Травяной материал?
        Texture2D tControl = Bindless::GetTexture2D( Terrain::Get().ControlMapTexture );
        float raw = tControl.SampleLevel( g_sPointClamp, cuv, 0 ).r;
        CompactTerrainMaterial m = CompactTerrainMaterial::DecodeFromFloat( raw );
        if ( m.IsHole )
            return;

        float blend = m.GetNormalizedBlend();
        uint grassId = (uint)g_nGrassMaterial;
        float grassWeight = 0.0;
        if ( m.BaseTextureId == grassId )    grassWeight += (1.0 - blend);
        if ( m.OverlayTextureId == grassId ) grassWeight += blend;
        if ( grassWeight < g_flGrassMaterialThreshold )
            return;

        // Травинки со случайным размещением внутри треугольника.
        // Вдали агрессивно режем количество — главный выигрыш по GS и fill rate.
        // pow(distN,0.6) начинает резать раньше; на максимуме остаётся ~1 лезвие.
        // Текстурный крест ест вдвое больше вершин GS — там потолок 12 (см. maxvertexcount).
    #if S_GRASS_TEXTURE
        int maxBlades = clamp( g_nGrassBladesPerTri, 1, 12 );
    #else
        int maxBlades = clamp( g_nGrassBladesPerTri, 1, 16 );
    #endif
        float fade = pow( distN, 0.6 );
        int blades = max( 1, (int)round( maxBlades * (1.0 - 0.85 * fade) ) );

        // Дальше порога крест вырождается в один билборд к камере (детали креста на дистанции не видно).
        bool billboard = dist >= g_flGrassBillboardDistance;

        [loop]
        for ( int b = 0; b < blades; b++ )
        {
            // Уникальный seed на (треугольник, травинку): хэшируем центроид +
            // индекс, чтобы паттерн не повторялся из треугольника в треугольник.
            float2 rr = GrassHash22( c.xy * 0.5 + (float)b * 37.91 + 0.37 );
            float r1 = rr.x;
            float r2 = rr.y;
            float s = dot( rr, float2( 113.7, 271.3 ) ) + (float)b; // развязанный seed для размера/наклона
            // Равномерная выборка точки в треугольнике (sqrt по первой координате).
            float su = sqrt( r1 );
            float3 pos = i[0].WorldPosition * (1.0 - su)
                       + i[1].WorldPosition * (su * (1.0 - r2))
                       + i[2].WorldPosition * (su * r2);
            EmitBlade( stream, pos, s, billboard );
        }
    #endif
    }
}

//=========================================================================================================================

PS
{
    DynamicCombo( D_GRID, 0..1, Sys( ALL ) );
    DynamicCombo( D_AUTO_SPLAT, 0..1, Sys( ALL ) );
    StaticCombo( S_GRASS_TEXTURE, F_GRASS_TEXTURE, Sys( ALL ) );
    StaticCombo( S_GRASS_CHEAP_LIGHT, F_GRASS_CHEAP_LIGHT, Sys( ALL ) );

    #include "common/pixel.hlsl"
    #include "common/material.hlsl"
    #include "common/shadingmodel.hlsl"
    #include "common/classes/AmbientLight.hlsl"

    // ВАЖНО: ставить ПОСЛЕ инклюдов. sbox_pixel.fxc (через common/pixel.hlsl) в конце
    // безусловно пишет RenderState( CullMode, S_RENDER_BACKFACES ? NONE : BACK ), и BACK
    // перетирал наш NONE, если он стоял выше. Поэтому раньше бэкфейсы не рисовались, а
    // встроенная галочка Render Backfaces не работала (S_RENDER_BACKFACES у нас не заведён).
    // Форсим NONE здесь → трава/террейн двусторонние, низ/обратная сторона травинок видны.
    RenderState( CullMode, NONE );

    // Текстура травы (слот "_grass") — ТОЛЬКО в режиме F_GRASS_TEXTURE.
    // Форма из альфы, цвет из RGB. Без галочки текстура не объявляется и не грузится:
    // травинка рисуется сплошным меш-лезвием (треугольник) с градиентным тинтом.
#if S_GRASS_TEXTURE
    CreateInputTexture2D( TextureGrass, Srgb, 8, "", "_grass", "Grass,80/95", Default4( 0.2, 0.4, 0.1, 1.0 ) );
    CreateTexture2D( g_tGrass ) < Channel( RGBA, Box( TextureGrass ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;
#endif

    // ===== Блендинг материалов — БЕЗ кастомщины: копия хелперов движкового base/terrain.shader =====
    // (в TerrainCommon.hlsl их нет, поэтому копия; наша старая реализация с дистанционным
    // порогом 4-тап/1-тап и упрощённым height-блендом удалена)

    /// <summary>
    /// Add a material to the output list, or add to existing if already present.
    /// </summary>
    void AddMaterial( uint materialIndex, float weight, inout uint outIndices[4], inout float outWeights[4], inout int count )
    {
        // Check if material already exists (always accumulate, even tiny weights)
        for ( int i = 0; i < count; i++ )
        {
            if ( outIndices[i] == materialIndex )
            {
                outWeights[i] += weight;
                return;
            }
        }

        // Only skip adding NEW materials if weight is negligible
        if ( weight <= 0.001 ) return;

        // Add new material if we have space
        if ( count < 4 )
        {
            outIndices[count] = materialIndex;
            outWeights[count] = weight;
            count++;
        }
    }

    /// <summary>
    /// Samples neighbors material stack(4 material & weight). Pick the top-4 heaviest material
    /// </summary>
    void MergeBilinearMaterials(
        uint indices00[4], float weights00[4], float blend00,
        uint indices10[4], float weights10[4], float blend10,
        uint indices01[4], float weights01[4], float blend01,
        uint indices11[4], float weights11[4], float blend11,
        out uint outIndices[4], out float outWeights[4] )
    {
        for(int i = 0; i < 4; i++)
        {
            outIndices[i] = 0;
            outWeights[i] = 0;
        }
        int count = 0;

        // 0, 0
        AddMaterial( indices00[0], weights00[0] * blend00, outIndices, outWeights, count );
        AddMaterial( indices00[1], weights00[1] * blend00, outIndices, outWeights, count );
        AddMaterial( indices00[2], weights00[2] * blend00, outIndices, outWeights, count );
        AddMaterial( indices00[3], weights00[3] * blend00, outIndices, outWeights, count );

        // 1, 0
        AddMaterial( indices10[0], weights10[0] * blend10, outIndices, outWeights, count );
        AddMaterial( indices10[1], weights10[1] * blend10, outIndices, outWeights, count );
        AddMaterial( indices10[2], weights10[2] * blend10, outIndices, outWeights, count );
        AddMaterial( indices10[3], weights10[3] * blend10, outIndices, outWeights, count );

        // 0, 1
        AddMaterial( indices01[0], weights01[0] * blend01, outIndices, outWeights, count );
        AddMaterial( indices01[1], weights01[1] * blend01, outIndices, outWeights, count );
        AddMaterial( indices01[2], weights01[2] * blend01, outIndices, outWeights, count );
        AddMaterial( indices01[3], weights01[3] * blend01, outIndices, outWeights, count );

        // 1, 1
        AddMaterial( indices11[0], weights11[0] * blend11, outIndices, outWeights, count );
        AddMaterial( indices11[1], weights11[1] * blend11, outIndices, outWeights, count );
        AddMaterial( indices11[2], weights11[2] * blend11, outIndices, outWeights, count );
        AddMaterial( indices11[3], weights11[3] * blend11, outIndices, outWeights, count );

        // Sort by material index to maintain consistent blend order
        // This prevents harsh cutoffs from materials flipping order at pixel boundaries
        for ( int pass = 0; pass < 3; pass++ )
        {
            for ( int i = 0; i < 3 - pass; i++ )
            {
                if ( outIndices[i] > outIndices[i + 1] && outWeights[i + 1] > 0 )
                {
                    uint tempIndex = outIndices[i];
                    outIndices[i] = outIndices[i + 1];
                    outIndices[i + 1] = tempIndex;

                    float tempWeight = outWeights[i];
                    outWeights[i] = outWeights[i + 1];
                    outWeights[i + 1] = tempWeight;
                }
            }
        }
    }

    void Terrain_SplatIndexed( in float2 texUV, in float2 texDdx, in float2 texDdy, in uint indices[4], in float weights[4],
        out float3 albedo, out float3 normal, out float roughness, out float ao, out float metal )
    {
        texUV /= 32;
        texDdx /= 32;
        texDdy /= 32;

        float3 albedos[4], normals[4];
        float heights[4], roughnesses[4], aos[4], metalness[4];

        // Sample materials by index
        for ( int i = 0; i < 4; i++ )
        {
            // Empty slot after merging - skip its taps
            if ( weights[i] <= 0.0f )
            {
                albedos[i] = 0;
                normals[i] = float3( 0, 0, 1 );
                roughnesses[i] = 0;
                heights[i] = 0;
                aos[i] = 0;
                metalness[i] = 0;
                continue;
            }

            TerrainMaterial mat = g_TerrainMaterials[ indices[i] ];
            float2 layerUV = texUV * mat.uvscale;
            float2x2 uvAngle = float2x2( 1, 0, 0, 1 );

            // Apply NoTile if needed
            if ( mat.HasFlag( TerrainFlags::NoTile ) )
            {
                layerUV = Terrain_SampleSeamlessUV( layerUV, uvAngle );
            }

            Texture2D tBcr = Bindless::GetTexture2D( mat.bcr_texid );
            Texture2D tNho = Bindless::GetTexture2D( mat.nho_texid );
            SamplerState materialSampler = Bindless::GetSampler( Terrain::Get().samplerindex );

            // Explicit gradients: this runs inside divergent flow where implicit derivatives are undefined
            float2 layerDdx = mul( uvAngle, texDdx * mat.uvscale );
            float2 layerDdy = mul( uvAngle, texDdy * mat.uvscale );
            float4 bcr = tBcr.SampleGrad( materialSampler, layerUV, layerDdx, layerDdy );
            float4 nho = tNho.SampleGrad( materialSampler, layerUV, layerDdx, layerDdy );

            float3 normal = ComputeNormalFromRGTexture( nho.rg );
            normal.xy = mul( uvAngle, normal.xy );
            normal.xz *= mat.normalstrength;
            normal = normalize( normal );

            albedos[i] = SrgbGammaToLinear( bcr.rgb );
            normals[i] = normal;
            roughnesses[i] = bcr.a;
            heights[i] = nho.b * mat.heightstrength;
            aos[i] = nho.a;
            metalness[i] = mat.metalness;
        }

        // Normalize base weights
        float sum = weights[0] + weights[1] + weights[2] + weights[3];
        if ( sum > 0 && sum != 1.0 )
        {
            float scale = 1.0 / sum;
            weights[0] *= scale;
            weights[1] *= scale;
            weights[2] *= scale;
            weights[3] *= scale;
        }

        float blendWeights[4];

        if ( Terrain::Get().HeightBlending )
        {
            // Parallel height blending (order-independent)
            // Calculate average height
            float avgHeight = (heights[0] * weights[0] + heights[1] * weights[1] +
                              heights[2] * weights[2] + heights[3] * weights[3]);

            // Modulate weights based on height differences
            float sharpness = Terrain::Get().HeightBlendSharpness * 10.0; // Scale for better control

            for ( int idx = 0; idx < 4; idx++ )
            {
                if ( weights[idx] > 0.0 )
                {
                    // Boost weight based on how much higher this material is than average
                    float heightBias = (heights[idx] - avgHeight) * sharpness;
                    blendWeights[idx] = weights[idx] * pow( 2.0, heightBias );
                }
                else
                {
                    blendWeights[idx] = 0.0;
                }
            }

            // Normalize adjusted weights
            float total = blendWeights[0] + blendWeights[1] + blendWeights[2] + blendWeights[3];
            if ( total > 0.0 )
            {
                blendWeights[0] /= total;
                blendWeights[1] /= total;
                blendWeights[2] /= total;
                blendWeights[3] /= total;
            }
        }
        else
        {
            // No height blending - use base weights directly
            blendWeights[0] = weights[0];
            blendWeights[1] = weights[1];
            blendWeights[2] = weights[2];
            blendWeights[3] = weights[3];
        }

        // Blend all materials simultaneously (order-independent)
        albedo = albedos[0] * blendWeights[0] + albedos[1] * blendWeights[1] + albedos[2] * blendWeights[2] + albedos[3] * blendWeights[3];
        normal = normals[0] * blendWeights[0] + normals[1] * blendWeights[1] + normals[2] * blendWeights[2] + normals[3] * blendWeights[3];
        roughness = roughnesses[0] * blendWeights[0] + roughnesses[1] * blendWeights[1] + roughnesses[2] * blendWeights[2] + roughnesses[3] * blendWeights[3];
        ao = aos[0] * blendWeights[0] + aos[1] * blendWeights[1] + aos[2] * blendWeights[2] + aos[3] * blendWeights[3];
        metal = metalness[0] * blendWeights[0] + metalness[1] * blendWeights[1] + metalness[2] * blendWeights[2] + metalness[3] * blendWeights[3];
    }

    /// <summary>
    /// Witcher format splatting - blends base and overlay materials with blend factor
    /// </summary>
    void Terrain_Splat( in float2 texUV, in float2 texDdx, in float2 texDdy, in CompactTerrainMaterial material,
        out float3 albedo, out float3 normal, out float roughness, out float ao, out float metal )
    {
        texUV /= 32;
        texDdx /= 32;
        texDdy /= 32;

        // Sample base material with optional seamless UVs when requested
        TerrainMaterial baseMat = g_TerrainMaterials[material.BaseTextureId];
        SamplerState baseSampler = Bindless::GetSampler( Terrain::Get().samplerindex );
        float2 baseUV = texUV * baseMat.uvscale;
        float2x2 baseUvAngle = float2x2( 1, 0, 0, 1 );
        float2 baseSampleUV = baseUV;

        if ( baseMat.HasFlag( TerrainFlags::NoTile ) )
        {
            baseSampleUV = Terrain_SampleSeamlessUV( baseUV, baseUvAngle );
        }

        float2 baseDdx = mul( baseUvAngle, texDdx * baseMat.uvscale );
        float2 baseDdy = mul( baseUvAngle, texDdy * baseMat.uvscale );
        float4 baseBcr = Bindless::GetTexture2D( baseMat.bcr_texid ).SampleGrad( baseSampler, baseSampleUV, baseDdx, baseDdy );
        float4 baseNho = Bindless::GetTexture2D( baseMat.nho_texid ).SampleGrad( baseSampler, baseSampleUV, baseDdx, baseDdy );

        float3 baseNormal = ComputeNormalFromRGTexture( baseNho.rg );
        baseNormal.xy = mul( baseUvAngle, baseNormal.xy );
        baseNormal.xz *= baseMat.normalstrength;
        baseNormal = normalize( baseNormal );

        float blend = material.GetNormalizedBlend();

        if ( blend <= 0.0f && !Terrain::Get().HeightBlending )
        {
            albedo = SrgbGammaToLinear( baseBcr.rgb );
            normal = baseNormal;
            roughness = baseBcr.a;
            ao = baseNho.a;
            metal = baseMat.metalness;
            return;
        }

        // Sample overlay material with optional seamless UVs when requested
        TerrainMaterial overlayMat = g_TerrainMaterials[material.OverlayTextureId];
        SamplerState overlaySampler = Bindless::GetSampler( Terrain::Get().samplerindex );
        float2 overlayUV = texUV * overlayMat.uvscale;
        float2x2 overlayUvAngle = float2x2( 1, 0, 0, 1 );
        float2 overlaySampleUV = overlayUV;

        if ( overlayMat.HasFlag( TerrainFlags::NoTile ) )
        {
            overlaySampleUV = Terrain_SampleSeamlessUV( overlayUV, overlayUvAngle );
        }

        float2 overlayDdx = mul( overlayUvAngle, texDdx * overlayMat.uvscale );
        float2 overlayDdy = mul( overlayUvAngle, texDdy * overlayMat.uvscale );
        float4 overlayBcr = Bindless::GetTexture2D( overlayMat.bcr_texid ).SampleGrad( overlaySampler, overlaySampleUV, overlayDdx, overlayDdy );
        float4 overlayNho = Bindless::GetTexture2D( overlayMat.nho_texid ).SampleGrad( overlaySampler, overlaySampleUV, overlayDdx, overlayDdy );

        float3 overlayNormal = ComputeNormalFromRGTexture( overlayNho.rg );
        overlayNormal.xy = mul( overlayUvAngle, overlayNormal.xy );
        overlayNormal.xz *= overlayMat.normalstrength;
        overlayNormal = normalize( overlayNormal );

        if ( Terrain::Get().HeightBlending )
        {
            float baseHeight = baseNho.b * baseMat.heightstrength;
            float overlayHeight = overlayNho.b * overlayMat.heightstrength;
            blend = Terrain_HeightBlendWeight( blend, baseHeight, overlayHeight, Terrain::Get().HeightBlendSharpness );
        }

        // Blend materials
        albedo = lerp( SrgbGammaToLinear( baseBcr.rgb ), SrgbGammaToLinear( overlayBcr.rgb ), blend );
        normal = lerp( baseNormal, overlayNormal, blend );
        roughness = lerp( baseBcr.a, overlayBcr.a, blend );
        ao = lerp( baseNho.a, overlayNho.a, blend );
        metal = lerp( baseMat.metalness, overlayMat.metalness, blend );
    }

    // Затенение билборда травы (текстура + альфа-вырез).
    float4 ShadeGrass( PixelInput i )
    {
        float v = saturate( i.GrassUv.y );

    #if S_GRASS_TEXTURE
        // PNG-билборд: форма из альфы, цвет из RGB текстуры.
        float2 guv = float2( i.GrassUv.x, 1.0 - i.GrassUv.y ); // V перевёрнут
        float4 g = g_tGrass.Sample( g_sAnisotropic, guv );
        clip( g.a - g_flGrassAlphaCutoff );
        float3 albedo = g.rgb;
    #else
        // Сплошное меш-лезвие: без текстуры и альфа-выреза, цвет — градиент низ→верх.
        float3 albedo = lerp( g_vGrassBottomColor, g_vGrassTopColor, v );
    #endif

        // Влажность темнит траву.
        float wet = saturate( g_flWetness );
        albedo *= lerp( 1.0, saturate( g_flWetDarken ), wet );

        float3 up = float3( 0, 0, 1 );

    #if S_GRASS_CHEAP_LIGHT
        // Дешёвый свет: только диффузный от окружения/неба (IBL/пробы), без кластера
        // источников/теней/спекуляра. AO снизу лезвия — простым множителем.
        float ao = lerp( 0.6, 1.0, v );
        float3 ambient = AmbientLight::From( i.WorldPosition, i.ScreenPosition, up );
        return float4( albedo * ambient * ao, 1.0 );
    #else
        float3 tU = float3( 1, 0, 0 );
        float3 tV = normalize( cross( up, tU ) );

        Material p = Material::Init();
        p.Albedo = albedo;
        p.Normal = up;
        p.Roughness = 0.7; // мокрая трава темнее (albedo выше), но НЕ глянцевее — без отражений
        p.Metalness = 0.0;
        p.AmbientOcclusion = lerp( 0.6, 1.0, v ); // низ лезвия чуть затенён
        p.TextureCoords = i.GrassUv;

        p.WorldPosition = i.WorldPosition;
        p.WorldPositionWithOffset = i.WorldPosition - g_vHighPrecisionLightingOffsetWs.xyz;
        p.ScreenPosition = i.ScreenPosition;
        p.WorldTangentU = tU;
        p.WorldTangentV = tV;

        return ShadingModelStandard::Shade( p );
    #endif
    }

	float4 MainPs( PixelInput i ) : SV_Target0
	{
        // Трава — отдельная ветка (двусторонняя: CullMode NONE для всего прохода).
        if ( i.IsGrass > 0.5 )
            return ShadeGrass( i );

        // Террейн оставляем односторонним: вручную отбрасываем обратные грани (эквивалент
        // CullMode BACK), т.к. общий проход травы требует NONE. Низ террейна рендерить не надо.
        if ( !i.IsFrontFace )
        {
            clip( -1 );
            return float4( 0, 0, 0, 0 );
        }

        Texture2D tHeightMap = Bindless::GetTexture2D( Terrain::Get().HeightMapTexture );
        float2 texSize = TextureDimensions2D( tHeightMap, 0 );
        float3 localPos = i.LocalPosition;
        float2 uv = localPos.xy / ( texSize * Terrain::Get().UnitsPerTexel );

        if ( uv.x < 0.0 || uv.y < 0.0 || uv.x > 1.0 || uv.y > 1.0 )
        {
            clip( -1 );
            return float4( 0, 0, 0, 0 );
        }

        float3 tangentU, tangentV;
        float3 geoNormal;

        geoNormal = Terrain_Normal( tHeightMap, uv, Terrain::Get().HeightScale, tangentU, tangentV );

        geoNormal = mul( Terrain::Get().Transform, float4( geoNormal, 0.0 ) ).xyz;
        tangentU = mul( Terrain::Get().Transform, float4( tangentU, 0.0 ) ).xyz;
        tangentV = mul( Terrain::Get().Transform, float4( tangentV, 0.0 ) ).xyz;

        geoNormal = normalize( geoNormal );
        tangentU = normalize( tangentU - geoNormal * dot( tangentU, geoNormal ) );
        tangentV = normalize( cross( geoNormal, tangentU ) );

        float3 albedo = float3( 1, 1, 1 );
        float3 norm = float3( 0, 0, 1 );
        float roughness = 1;
        float ao = 1;
        float metalness = 0;

    #if D_GRID
        Terrain_ProcGrid( localPos.xy, albedo, roughness );
    #else
        // Compact format: simple base/overlay blending — тот же путь, что у base/terrain.shader
        if ( Terrain::Get().ControlMapTexture != 0 )
        {
            float4 quadWeights;
            uint4 controlBits = Terrain::GatherControlQuad( uv, quadWeights );

            CompactTerrainMaterial mat00 = CompactTerrainMaterial::Decode( controlBits.x );
            CompactTerrainMaterial mat10 = CompactTerrainMaterial::Decode( controlBits.y );
            CompactTerrainMaterial mat01 = CompactTerrainMaterial::Decode( controlBits.z );
            CompactTerrainMaterial mat11 = CompactTerrainMaterial::Decode( controlBits.w );

            float blend00 = quadWeights.x;
            float blend10 = quadWeights.y;
            float blend01 = quadWeights.z;
            float blend11 = quadWeights.w;

            // Check for holes - blend hole values
            float holeBlend = 0.0;
            if ( mat00.IsHole ) holeBlend += blend00;
            if ( mat10.IsHole ) holeBlend += blend10;
            if ( mat01.IsHole ) holeBlend += blend01;
            if ( mat11.IsHole ) holeBlend += blend11;

            // Clip if predominantly a hole
            if ( holeBlend > 0.5 )
            {
                clip( -1 );
                return float4( 0, 0, 0, 0 );
            }

            float2 splatDdx = ddx( localPos.xy );
            float2 splatDdy = ddy( localPos.xy );

            // Corners are identical everywhere except within a texel of a painted boundary - one splat is exact
            if ( controlBits.x == controlBits.y && controlBits.x == controlBits.z && controlBits.x == controlBits.w )
            {
                Terrain_Splat( localPos.xy, splatDdx, splatDdy, mat00, albedo, norm, roughness, ao, metalness );
            }
            else if ( Terrain::Get().HeightBlending )
            {
                CompactTerrainMaterial mats[4];
                mats[0] = mat00; mats[1] = mat10; mats[2] = mat01; mats[3] = mat11;

                albedo = 0; norm = 0; roughness = 0; ao = 0; metalness = 0;

                [unroll]
                for ( int c = 0; c < 4; c++ )
                {
                    float3 cornerAlbedo, cornerNormal;
                    float cornerRough, cornerAo, cornerMetal;
                    Terrain_Splat( localPos.xy, splatDdx, splatDdy, mats[c], cornerAlbedo, cornerNormal, cornerRough, cornerAo, cornerMetal );

                    float w = quadWeights[c];
                    albedo += cornerAlbedo * w;
                    norm += cornerNormal * w;
                    roughness += cornerRough * w;
                    ao += cornerAo * w;
                    metalness += cornerMetal * w;
                }
            }
            else
            {
                // Merge the corner stacks into the top-4 heaviest materials and sample each distinct
                // material once, instead of splatting all four corners and blending.
                uint indices00[4], indices10[4], indices01[4], indices11[4];
                float weights00[4], weights10[4], weights01[4], weights11[4];
                mat00.GetMaterialStack( indices00, weights00 );
                mat10.GetMaterialStack( indices10, weights10 );
                mat01.GetMaterialStack( indices01, weights01 );
                mat11.GetMaterialStack( indices11, weights11 );

                uint mergedIndices[4];
                float mergedWeights[4];
                MergeBilinearMaterials(
                    indices00, weights00, blend00,
                    indices10, weights10, blend10,
                    indices01, weights01, blend01,
                    indices11, weights11, blend11,
                    mergedIndices, mergedWeights );

                Terrain_SplatIndexed( localPos.xy, splatDdx, splatDdy, mergedIndices, mergedWeights, albedo, norm, roughness, ao, metalness );
            }
        }
    #endif

        // Влажность: дождь ТЕМНИТ террейн (сильнее во впадинах, низкий AO), но НЕ делает его глянцевым —
        // отражений/зеркальности от дождя нет, только затемнение.
        float wet = saturate( g_flWetness );
        if ( wet > 0.0 )
        {
            float pooling = wet * lerp( 0.65, 1.0, saturate( 1.0 - ao ) );
            // Только затемняем — шероховатость НЕ трогаем: снижение делало террейн зеркальным к небу.
            albedo *= lerp( 1.0, saturate( g_flWetDarken ), pooling );
        }

        Material p = Material::Init();
        p.Albedo = albedo;
        p.Normal = TransformNormal( norm, geoNormal, tangentU, tangentV );
        p.Roughness = roughness;
        p.Metalness = metalness;
        p.AmbientOcclusion = ao;
        p.TextureCoords = uv;

        p.WorldPosition = i.WorldPosition;
        p.WorldPositionWithOffset = i.WorldPosition - g_vHighPrecisionLightingOffsetWs.xyz;
        p.ScreenPosition = i.ScreenPosition;

        p.WorldTangentU = tangentU;
        p.WorldTangentV = tangentV;

	    return ShadingModelStandard::Shade( p );
	}
}
