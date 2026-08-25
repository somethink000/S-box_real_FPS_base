//=========================================================================================================================
// LonePine Car Lights — multi-zone emissive shader for car headlights / taillights / fog / reverse / blinkers.
// One material covers all light zones. ОДНА маска на каждый источник света (худож. workflow),
// внутри упаковываются в 2 RGBA-текстуры для эффективной выборки.
// Per-renderer state (g_flRunning / g_flBrake / ...) controlled from C# via Material.Set("...", value)
// or SceneObject.Attributes.Set("...", value).
//
// Visual goal: фара светится как реальное стекло-рассеиватель — HDR-эмиссия + bloom post.
// Глобальный множитель g_flGlobalEmissionBoost крути вверх (10–100) чтобы получить эффект «спрайта»
// (яркое пятно, жирно расплывается через bloom).
//=========================================================================================================================
HEADER
{
    Description = "LonePine Car Lights";
    Version = 1;
}

FEATURES
{
    #include "common/features.hlsl"
    // Включает стеклянный рассеиватель: alpha + лёгкий blur заднего плана.
    // Дорого по GPU (translucent-проход + копия фреймбуфера каждый кадр), по умолчанию ВЫКЛ.
    Feature( F_GLASS, 0..1, "Glass" );
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
    StaticCombo( S_GLASS, F_GLASS, Sys(ALL) );

    // Translucent + FB-copy включаются ТОЛЬКО когда у материала включена фича Glass.
    // Без неё материал — обычный opaque PBR (никакой лишней работы GPU).
    #if S_GLASS
        RenderState( BlendEnable, true );
        RenderState( SrcBlend, SRC_ALPHA );
        RenderState( DstBlend, INV_SRC_ALPHA );
    #endif

    #include "common/pixel.hlsl"

    #if S_GLASS
        BoolAttribute( translucent, true );
        BoolAttribute( bWantsFBCopyTexture, true );

        CreateTexture2D( g_tFrameBufferCopyTexture ) < Attribute( "FrameBufferCopyTexture" ); SrgbRead( false ); Filter( MIN_MAG_MIP_LINEAR ); AddressU( MIRROR ); AddressV( MIRROR ); >;
    #endif

    //
    // PBR base — фара остаётся стеклянной (нормали, шероховатость), эмиссия идёт сверху
    //
    CreateInputTexture2D( TextureColor,     Srgb,   8, "", "_color",  "Material,10/10", Default3( 0.05, 0.05, 0.05 ) );
    CreateInputTexture2D( TextureNormal,    Linear, 8, "", "_normal", "Material,10/20", Default3( 0.5, 0.5, 1.0 ) );
    CreateInputTexture2D( TextureRoughness, Linear, 8, "", "_rough",  "Material,10/30", Default( 0.15 ) );
    CreateInputTexture2D( TextureMetalness, Linear, 8, "", "_metal",  "Material,10/40", Default( 0.0 ) );

    CreateTexture2D( g_tColor )     < Channel( RGB, Box( TextureColor ),     Srgb );   OutputFormat( BC7 ); SrgbRead( true );  >;
    CreateTexture2D( g_tNormal )    < Channel( RGB, Box( TextureNormal ),    Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
    CreateTexture2D( g_tRoughness ) < Channel( R,   Box( TextureRoughness ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
    CreateTexture2D( g_tMetalness ) < Channel( R,   Box( TextureMetalness ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

    //
    // Маски света: по одной textura на каждый источник (художник работает с одной маской на свет).
    // Внутри движок пакует их в 2 RGBA-текстуры (4 канала каждая = 8 источников), чтобы рантайм
    // делал всего 2 sampler-чтения вместо 8.
    //
    CreateInputTexture2D( TextureMaskRunning,  Linear, 8, "", "_running",  "Light Masks,20/10", Default( 0 ) );
    CreateInputTexture2D( TextureMaskBrake,    Linear, 8, "", "_brake",    "Light Masks,20/15", Default( 0 ) );
    CreateInputTexture2D( TextureMaskLeftBlk,  Linear, 8, "", "_leftBlk",  "Light Masks,20/20", Default( 0 ) );
    CreateInputTexture2D( TextureMaskRightBlk, Linear, 8, "", "_rightBlk", "Light Masks,20/25", Default( 0 ) );
    CreateInputTexture2D( TextureMaskLowBeam,  Linear, 8, "", "_lowBeam",  "Light Masks,20/30", Default( 0 ) );
    CreateInputTexture2D( TextureMaskHighBeam, Linear, 8, "", "_highBeam", "Light Masks,20/35", Default( 0 ) );
    CreateInputTexture2D( TextureMaskReverse,  Linear, 8, "", "_reverse",  "Light Masks,20/40", Default( 0 ) );
    CreateInputTexture2D( TextureMaskFog,      Linear, 8, "", "_fog",      "Light Masks,20/45", Default( 0 ) );

    CreateTexture2D( g_tMaskA ) <
        Channel( R, Box( TextureMaskRunning ),  Linear );
        Channel( G, Box( TextureMaskBrake ),    Linear );
        Channel( B, Box( TextureMaskLeftBlk ),  Linear );
        Channel( A, Box( TextureMaskRightBlk ), Linear );
        OutputFormat( BC7 ); SrgbRead( false );
    >;
    CreateTexture2D( g_tMaskB ) <
        Channel( R, Box( TextureMaskLowBeam ),  Linear );
        Channel( G, Box( TextureMaskHighBeam ), Linear );
        Channel( B, Box( TextureMaskReverse ),  Linear );
        Channel( A, Box( TextureMaskFog ),      Linear );
        OutputFormat( BC7 ); SrgbRead( false );
    >;

    //
    // Цвета каждой зоны (HDR — значения > 1 дают bloom/glow на реальном стекле)
    //
    float3 g_vColorRunning  < UiType( Color ); Default3( 1.00, 0.45, 0.10 ); UiGroup( "Light Colors,30/10" ); >;
    float3 g_vColorBrake    < UiType( Color ); Default3( 1.00, 0.05, 0.00 ); UiGroup( "Light Colors,30/20" ); >;
    float3 g_vColorBlinker  < UiType( Color ); Default3( 1.00, 0.45, 0.05 ); UiGroup( "Light Colors,30/30" ); >;
    float3 g_vColorLowBeam  < UiType( Color ); Default3( 1.00, 0.95, 0.85 ); UiGroup( "Light Colors,30/40" ); >;
    float3 g_vColorHighBeam < UiType( Color ); Default3( 1.00, 1.00, 0.95 ); UiGroup( "Light Colors,30/50" ); >;
    float3 g_vColorReverse  < UiType( Color ); Default3( 1.00, 1.00, 1.00 ); UiGroup( "Light Colors,30/60" ); >;
    float3 g_vColorFog      < UiType( Color ); Default3( 1.00, 0.85, 0.55 ); UiGroup( "Light Colors,30/70" ); >;

    //
    // Глобальный множитель эмиссии — крути ВВЕРХ для эффекта спрайта в bloom-постпроцессе.
    // 1.0 = базовая HDR-яркость; 10..100 = ОЧЕНЬ яркие фары которые жирно блумят (sprite-look).
    //
    float g_flGlobalEmissionBoost < Default( 1.0 ); Range( 0.0, 100.0 ); UiGroup( "Light Intensity,40/05" ); >;

    //
    // HDR-яркость каждой зоны (умножается на g_flGlobalEmissionBoost в финале)
    //
    float g_flIntensityRunning  < Default(  3.0 ); Range( 0.0, 30.0 ); UiGroup( "Light Intensity,40/10" ); >;
    float g_flIntensityBrake    < Default(  8.0 ); Range( 0.0, 30.0 ); UiGroup( "Light Intensity,40/20" ); >;
    float g_flIntensityBlinker  < Default(  6.0 ); Range( 0.0, 30.0 ); UiGroup( "Light Intensity,40/30" ); >;
    float g_flIntensityLowBeam  < Default(  8.0 ); Range( 0.0, 30.0 ); UiGroup( "Light Intensity,40/40" ); >;
    float g_flIntensityHighBeam < Default( 15.0 ); Range( 0.0, 30.0 ); UiGroup( "Light Intensity,40/50" ); >;
    float g_flIntensityReverse  < Default(  5.0 ); Range( 0.0, 30.0 ); UiGroup( "Light Intensity,40/60" ); >;
    float g_flIntensityFog      < Default(  4.0 ); Range( 0.0, 30.0 ); UiGroup( "Light Intensity,40/70" ); >;

    //
    // Per-renderer state attributes — управляются из C# (VehicleComponent, группа Lights).
    // Имена должны совпадать с теми что ставит .Set(name, value) в материале.
    //
    float g_flRunning    < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "State (debug),50/10" ); >;
    float g_flBrake      < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "State (debug),50/20" ); >;
    float g_flLeftBlink  < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "State (debug),50/30" ); >;
    float g_flRightBlink < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "State (debug),50/40" ); >;
    float g_flLowBeam    < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "State (debug),50/50" ); >;
    float g_flHighBeam   < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "State (debug),50/60" ); >;
    float g_flReverse    < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "State (debug),50/70" ); >;
    float g_flFog        < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "State (debug),50/80" ); >;

    //
    // Glass tweaks — делает свет «стеклянным», а не «нарисованным»
    //
    float g_flFresnelBoost   < Default( 1.5 ); Range( 0.0, 5.0 ); UiGroup( "Glass,60/10" ); >;
    float g_flLensTint       < Default( 0.4 ); Range( 0.0, 1.0 ); UiGroup( "Glass,60/20" ); >;
    float g_flAlbedoDarken   < Default( 0.3 ); Range( 0.0, 1.0 ); UiGroup( "Glass,60/30" ); >;

    //
    // Glass mask + параметры — компилируются только если включена фича F_GLASS.
    // БЕЛЫЙ = прозрачно (видно сквозь, опционально с blur'ом заднего плана).
    // ЧЁРНЫЙ = непрозрачно (обычная PBR-поверхность).
    //
    #if S_GLASS
        CreateInputTexture2D( TextureGlass, Linear, 8, "", "_glass", "Glass,60/40", Default( 0.0 ) );
        CreateTexture2D( g_tGlass ) < Channel( R, Box( TextureGlass ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

        // Минимальная непрозрачность стекла при белой маске. 0 = белый пиксель полностью прозрачный
        // (видно сквозь насквозь); 0.15+ = всегда чуть-чуть видно стекло (как реальный лак-рассеиватель).
        float g_flGlassMinOpacity < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "Glass,60/45" ); >;
        // Сила размытия заднего плана через mip-уровни FB-copy. 0 = чистое прозрачное стекло,
        // 1 = matte/frosted glass. Вместо рефракции — простое размытие, дёшево.
        float g_flGlassBlur       < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "Glass,60/50" ); >;
    #endif

    //
    // Bulb hotspot — эффект «лампочки» в центре фары. Работает по градиенту маски:
    // pow(mask, Power) даёт нелинейное падение, поэтому самая яркая точка маски (центр)
    // светит сильнее краёв. Power выше = меньше «лампочка», Boost = насколько ярче центр.
    // Для бинарных масок (только 0/255) эффект превращается в равномерный буст яркости.
    //
    float g_flBulbPower < Default( 4.0 ); Range( 1.0, 16.0 ); UiGroup( "Bulb,65/10" ); >;
    float g_flBulbBoost < Default( 0.0 ); Range( 0.0, 10.0 ); UiGroup( "Bulb,65/20" ); >;

    SamplerState g_sSampler < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {
        Material m = Material::Init( i );

        float2 uv = i.vTextureCoords.xy;

        float3 albedo = Tex2DS( g_tColor,     g_sSampler, uv ).rgb;
        float3 nrm    = Tex2DS( g_tNormal,    g_sSampler, uv ).rgb;
        float  rough  = Tex2DS( g_tRoughness, g_sSampler, uv ).r;
        float  metal  = Tex2DS( g_tMetalness, g_sSampler, uv ).r;

        float4 maskA = Tex2DS( g_tMaskA, g_sSampler, uv );
        float4 maskB = Tex2DS( g_tMaskB, g_sSampler, uv );

        // Per-zone weights = mask × state — базовая ровная заливка света по всей зоне
        float wRunning   = maskA.r * g_flRunning;
        float wBrake     = maskA.g * g_flBrake;
        float wLeftBlk   = maskA.b * g_flLeftBlink;
        float wRightBlk  = maskA.a * g_flRightBlink;
        float wLowBeam   = maskB.r * g_flLowBeam;
        float wHighBeam  = maskB.g * g_flHighBeam;
        float wReverse   = maskB.b * g_flReverse;
        float wFog       = maskB.a * g_flFog;

        // Hotspot «лампочки» — концентрированное свечение там, где маска ярче.
        // Если в .vmat g_flBulbBoost = 0 — эффект отключён (default), всё работает как раньше.
        float bP = g_flBulbPower;
        float bB = g_flBulbBoost;
        float hRunning  = pow( maskA.r, bP ) * g_flRunning   * bB;
        float hBrake    = pow( maskA.g, bP ) * g_flBrake     * bB;
        float hLeftBlk  = pow( maskA.b, bP ) * g_flLeftBlink * bB;
        float hRightBlk = pow( maskA.a, bP ) * g_flRightBlink* bB;
        float hLowBeam  = pow( maskB.r, bP ) * g_flLowBeam   * bB;
        float hHighBeam = pow( maskB.g, bP ) * g_flHighBeam  * bB;
        float hReverse  = pow( maskB.b, bP ) * g_flReverse   * bB;
        float hFog      = pow( maskB.a, bP ) * g_flFog       * bB;

        // Fresnel — на скользящих углах эмиссия ярче, как настоящее стекло-рассеиватель.
        float3 vViewDirWs = normalize( g_vCameraPositionWs - i.vPositionWithOffsetWs.xyz );
        float flFresnel = 1.0 - saturate( dot( normalize( i.vNormalWs ), vViewDirWs ) );
        float flGlassMul = 1.0 + flFresnel * g_flFresnelBoost;

        // Сумма эмиссии = базовый свет по зоне + hotspot «лампочки» в центре маски
        float3 emission = 0;
        emission += (wRunning  + hRunning ) * g_flIntensityRunning  * g_vColorRunning;
        emission += (wBrake    + hBrake   ) * g_flIntensityBrake    * g_vColorBrake;
        emission += (wLeftBlk  + hLeftBlk ) * g_flIntensityBlinker  * g_vColorBlinker;
        emission += (wRightBlk + hRightBlk) * g_flIntensityBlinker  * g_vColorBlinker;
        emission += (wLowBeam  + hLowBeam ) * g_flIntensityLowBeam  * g_vColorLowBeam;
        emission += (wHighBeam + hHighBeam) * g_flIntensityHighBeam * g_vColorHighBeam;
        emission += (wReverse  + hReverse ) * g_flIntensityReverse  * g_vColorReverse;
        emission += (wFog      + hFog     ) * g_flIntensityFog      * g_vColorFog;
        // Финальная эмиссия = по зонам × fresnel-буст × глобальный bloom-множитель.
        emission *= flGlassMul * g_flGlobalEmissionBoost;

        // Cовокупный вес активных зон в текущем пикселе (0..N) и его клипованная версия 0..1
        float zoneWeight = wRunning + wBrake + wLeftBlk + wRightBlk + wLowBeam + wHighBeam + wReverse + wFog;
        float zoneNorm = saturate( zoneWeight );

        // Албедо подкрашивается под цвет источника света — лит-зона перестаёт выглядеть «серой»
        // даже без bloom; это и даёт «стеклянный подсвеченный» вид, а не плоский тон.
        float3 hueColor = normalize( emission + 0.0001 );
        float3 lensTint = lerp( float3( 1, 1, 1 ), hueColor, g_flLensTint * zoneNorm );

        // В лит-зонах слегка темним сам албедо — чтобы свет читался поверх, а не «растворялся»
        float3 albedoFinal = albedo * lensTint * lerp( 1.0, ( 1.0 - g_flAlbedoDarken ), zoneNorm );

        m.Albedo = albedoFinal;

        nrm = mad( nrm, 2.0, -1.0 );
        m.Normal = TransformNormal( nrm, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );

        // В лит-зонах фара становится «глаже» (как горячая нить лампы) — даёт точечные блики
        m.Roughness = lerp( rough, max( rough * 0.4, 0.05 ), zoneNorm );
        m.Metalness = metal;
        m.Emission = emission;

        //
        // Glass — простой alpha-blend + лёгкий blur заднего плана.
        // БЕЗ refraction-направлений и иридесценции (одна лёгкая фигня вместо тяжёлой).
        //
        #if S_GLASS
        float glass = Tex2DS( g_tGlass, g_sSampler, uv ).r;

        if ( glass > 0.001 && g_flGlassBlur > 0.0 )
        {
            // Sample заднего плана в текущем пикселе с mip-уровнем по Blur (0 = чёткий, 1 = сильно размыт).
            float2 screen01 = i.vPositionSs.xy * g_vFrameBufferCopyInvSizeAndUvScale.xy;
            float2 vUV = screen01 * g_vFrameBufferCopyInvSizeAndUvScale.zw;
            const int nNumMips = 7;
            float3 backdrop = Tex2DLevel( g_tFrameBufferCopyTexture, vUV, g_flGlassBlur * nNumMips ).rgb;

            // Размытый задник домешивается в эмиссию пропорционально маске стекла —
            // alpha-blend сам по себе уже даёт «прозрачно», а это добавляет blur поверх.
            m.Emission += backdrop * glass * (1.0 - g_flGlassMinOpacity);
        }

        // Opacity: белая маска → прозрачно (g_flGlassMinOpacity), чёрная → полностью непрозрачно
        m.Opacity = lerp( 1.0, g_flGlassMinOpacity, glass );
        #else
        m.Opacity = 1.0;
        #endif

        return ShadingModelStandard::Shade( m );
    }
}
