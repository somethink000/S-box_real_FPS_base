HEADER
{
    DevShader = true;
    Description = "World-space 3D rain — depth-reconstructed shells along the view ray, perspective + parallax + geometry occlusion, no particles";
}

MODES
{
    Default();
    Forward();
}

COMMON
{
    #include "postprocess/shared.hlsl"
}

struct VertexInput
{
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;
};

struct PixelInput
{
    float2 vTexCoord : TEXCOORD0;

    #if ( PROGRAM == VFX_PROGRAM_VS )
        float4 vPositionPs : SV_Position;
    #endif

    #if ( PROGRAM == VFX_PROGRAM_PS )
        float4 vPositionSs : SV_Position;
    #endif
};

VS
{
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
        o.vPositionPs = float4( i.vPositionOs.xy, 0.0f, 1.0f );
        o.vTexCoord = i.vTexCoord;
        return o;
    }
}

PS
{
    #include "postprocess/common.hlsl"
    #include "common/classes/Depth.hlsl"

    Texture2D g_tColorBuffer < Attribute( "ColorBuffer" ); SrgbRead( true ); >;

    // Диагностика стриков (rain_debug): фиолетовый = intensity 0; иначе R=интенсивность,
    // G=depth (sceneDist/2000), B=результат райтмарча ×8.
    float  g_flRainDebug < Attribute( "RainDebug" ); Default( 0.0f ); >;
    // Плотность дождя 0..1 (из WeatherController.RainAmount).
    float  g_flIntensity < Attribute( "Intensity" ); Default( 0.0f ); >;
    // Собственное время эффекта (сек) — падение/снос капель.
    float  g_flRainTime  < Attribute( "RainTime" );  Default( 0.0f ); >;
    // Цвет/тон капель. C# уже промодулировал его текущим освещением сцены (день/ночь/гроза/молния).
    float3 g_vTint       < Attribute( "Tint" );      Default3( 0.72f, 0.77f, 0.86f ); >;
    // Подсветка капель локальным светом сцены: яркие места (фары, фонари) «зажигают» стрики.
    float  g_flPickup    < Attribute( "Pickup" );  Default( 0.35f ); >;
    // Множитель яркости/заметности стриков (RainEffect.StreakBoost).
    float  g_flBoost     < Attribute( "Boost" );   Default( 1.5f ); >;
    // Сила колец-сплешей на горизонтальных поверхностях (0 = выкл).
    float  g_flSplash    < Attribute( "Splash" );  Default( 1.0f ); >;

    // Heightfield симуляции капель на «стекле» (rain_droplet_cs, пинг-понг из RainEffect).
    Texture2D<float> g_tDroplets < Attribute( "Droplets" ); >;
    // Сила эффекта капель 0..1 (0 = сим выключен, ветка пропускается целиком).
    float  g_flDropletStrength < Attribute( "DropletStrength" ); Default( 0.0f ); >;
    float2 g_vDropletRes < Attribute( "DropletRes" ); Default2( 512.0f, 288.0f ); >;

    // Карта окклюзии дождя: высота первой поверхности сверху (крыши/навесы/мосты) в окне
    // вокруг игрока — CPU-трейсы в RainEffect. Точка ниже «крыши» над ней → дождя там нет.
    Texture2D<float> g_tRainOcc < Attribute( "RainOcc" ); >;
    float2 g_vRainOccOrigin < Attribute( "RainOccOrigin" ); >;
    // 1/размер окна (юниты). 0 = карты нет, всё считается открытым небом.
    float  g_flRainOccSpanInv < Attribute( "RainOccSpanInv" ); Default( 0.0f ); >;

    // POINT: R32F не на всём железе фильтруется билинейно — с BILINEAR сэмпл молча
    // возвращает 0 и карта «не работает» (крыша на нулевой высоте).
    SamplerState g_sRainOccSampler < Filter( POINT ); AddressU( CLAMP ); AddressV( CLAMP ); >;

    // Видимость дождя в мировой точке: 1 = открытое небо, 0 = под крышей. Мягкая кромка.
    float RainVisibility( float3 wp )
    {
        if ( g_flRainOccSpanInv <= 0.0f )
            return 1.0f;

        float2 occUv = ( wp.xy - g_vRainOccOrigin ) * g_flRainOccSpanInv;
        if ( occUv.x < 0.0f || occUv.y < 0.0f || occUv.x > 1.0f || occUv.y > 1.0f )
            return 1.0f;

        float roof = g_tRainOcc.SampleLevel( g_sRainOccSampler, occUv, 0 );
        return smoothstep( -24.0f, 8.0f, wp.z - roof );
    }
    // Видимая скорость дождя в мире (units/sec): падение + ветер − скорость игрока/камеры.
    // Направление = ось стрика (куда «летят» капли), длина = скорость (задаёт длину стрика).
    float3 g_vAppVel     < Attribute( "AppVel" ); Default3( 0.0f, 0.0f, -1100.0f ); >;
    // Фактор движения игрока 0..1 — гуще капли и длиннее стрики на скорости.
    float  g_flMove      < Attribute( "Move" );   Default( 0.0f ); >;

    float Hash21( float2 p )
    {
        p = frac( p * float2( 123.34f, 456.21f ) );
        p += dot( p, p + 45.32f );
        return frac( p.x * p.y );
    }

    // Interleaved gradient noise — дешёвый дизер, чтобы шаги райтмарча не банговались.
    float Ign( float2 p )
    {
        return frac( 52.9829189f * frac( dot( p, float2( 0.06711056f, 0.00583715f ) ) ) );
    }

    // Мировой луч камеры для пикселя (единичный, от позиции камеры).
    // Реконструкция как в Depth::GetWorldPosition, но без глубины — берём направление.
    float3 RayDir( float2 uv )
    {
        float3 ndc = float3( uv.x, 1.0f - uv.y, 0.5f );
        ndc.xy = 2.0f * ndc.xy - 1.0f;
        float4 wp = mul( g_matProjectionToWorld, float4( ndc, 1.0f ) );
        return normalize( wp.xyz / max( wp.w, 1e-10f ) );
    }

    // Плотность капель в мировой точке wp (сэмпл райтмарча).
    // Стрик тянется вдоль оси dir (видимая скорость дождя); колонки — сетка в плоскости (u,v) ⊥ dir;
    // короткий сегмент бежит вдоль оси со временем. Ось зависит от ветра И скорости игрока:
    // стоя — вертикально, на ходу dir ложится вдоль луча → стрики летят в камеру (даёт райтмарч).
    // cellXY — шаг решётки (юниты), period — период повторения капель вдоль оси (юниты),
    // segLen — длина светящегося сегмента (юниты, << period), speed — скролл,
    // seed — сдвиг октавы, thresh — порог заполнения (меньше = гуще).
    float RainField( float3 wp, float3 dir, float3 u, float3 v, float speed, float cellXY, float period, float segLen, float seed, float thresh )
    {
        // Координаты в плоскости, перпендикулярной оси стрика.
        float2 ab = float2( dot( wp, u ), dot( wp, v ) ) / cellXY;

        float2 id = floor( ab );
        float rnd = Hash21( id + seed );
        if ( rnd < thresh )
            return 0.0f;

        float2 f = frac( ab ) - 0.5f;
        float2 off = ( float2( Hash21( id + seed + 3.1f ), Hash21( id + seed + 7.7f ) ) - 0.5f ) * 0.7f;
        float col = smoothstep( 0.09f, 0.0f, length( f - off ) );   // колонка ⊥ оси (толще = заметнее)
        if ( col <= 0.0f )
            return 0.0f;

        // Период БОЛЬШОЙ (редкие капли, нет «колеса дилижанса»), сегмент КОРОТКИЙ →
        // падение читается плавно даже при неподвижной камере.
        float along = dot( wp, dir );
        float phase = frac( along / period - g_flRainTime * ( speed / period ) + rnd * 13.3f );
        float sAlong = ( phase - 0.5f ) * period;         // знаковое расстояние вдоль оси (+ = по ходу/вниз)

        // Комета: яркая голова у ведущего края (вниз), затухающий хвост позади (вверх).
        float head = smoothstep( segLen * 0.5f, 0.0f, abs( sAlong ) );
        float tail = saturate( 1.0f + sAlong / ( segLen * 2.4f ) ) * step( sAlong, 0.0f );
        float streak = max( head, tail * 0.6f );

        float tw = 0.7f + 0.3f * Hash21( id + seed + 19.7f );
        return col * streak * tw;
    }

    // Брызги от удара капли: корона микро-капель, разлетающихся ВВЕРХ и в стороны по
    // баллистике от точки удара на земле. Оценивается для точек в тонкой полосе над землёй
    // (groundZ — из карты окклюзии). thresh — порог активности клетки (больше = реже).
    float SplashField( float3 wp, float groundZ, float thresh )
    {
        const float cellSz = 34.0f;
        const float rate = 0.9f;     // жизней в секунду на клетку (медленные, плавные)
        float2 id = floor( wp.xy / cellSz );

        float rnd = Hash21( id * 0.731f );
        float cycle = g_flRainTime * rate + rnd * 29.0f;
        float slot = floor( cycle );
        float2 slotOff = frac( slot * float2( 0.75487767f, 0.56984029f ) ) * 31.0f;
        if ( Hash21( id + slotOff ) < thresh )
            return 0.0f;

        float life = frac( cycle );

        // Точка удара в клетке.
        float2 basePt = ( id + float2( Hash21( id + slotOff + 1.3f ), Hash21( id + slotOff + 2.6f ) ) ) * cellSz;
        float planar = length( wp.xy - basePt );
        if ( planar > 14.0f )
            return 0.0f;

        float hgt = wp.z - groundZ;
        float t = life * 1.1f;

        float total = 0.0f;
        [unroll]
        for ( int j = 0; j < 3; j++ )
        {
            float a = Hash21( id + slotOff + 7.0f + (float)j * 3.3f );
            float v0 = lerp( 40.0f, 65.0f, a );            // начальная скорость вверх, юнит/сек
            float dropH = v0 * t - 45.0f * t * t;           // пологая парабола — медленный взлёт
            if ( dropH < 0.0f )
                continue;
            float side = lerp( 8.0f, 26.0f, Hash21( id + slotOff + 11.0f + (float)j ) ) * t;
            float dot = smoothstep( 4.0f, 1.0f, abs( planar - side ) )
                      * smoothstep( 4.5f, 1.2f, abs( hgt - dropH ) );
            total += dot;
        }
        return total * ( 1.0f - life * life );
    }

    float4 MainPs( PixelInput i ) : SV_Target0
    {
        float2 ss = i.vPositionSs.xy;
        float2 uv = ( ss - g_vViewportOffset ) * g_vInvViewportSize;

        // Капли на объективе камеры: мягкая рефракция сцены по градиенту heightfield.
        // Тапы ШИРОКИЕ (2 текселя сим-текстуры) и высота усреднена — края капель размытые.
        float2 uvScene = uv;
        float dropMask = 0.0f;
        float2 dropGrad = 0.0f;
        if ( g_flDropletStrength > 0.001f )
        {
            float2 dTexel = 2.0f / g_vDropletRes;
            float hC = g_tDroplets.Sample( g_sBilinearMirror, uv );
            float hR = g_tDroplets.Sample( g_sBilinearMirror, uv + float2( dTexel.x, 0.0f ) );
            float hL = g_tDroplets.Sample( g_sBilinearMirror, uv - float2( dTexel.x, 0.0f ) );
            float hD = g_tDroplets.Sample( g_sBilinearMirror, uv + float2( 0.0f, dTexel.y ) );
            float hU = g_tDroplets.Sample( g_sBilinearMirror, uv - float2( 0.0f, dTexel.y ) );
            float hSoft = ( hC * 2.0f + hL + hR + hU + hD ) / 6.0f;
            dropGrad = float2( hR - hL, hD - hU );
            // Нижний порог повыше — тонкая плёнка/морось не даёт белёсую вуаль по экрану.
            dropMask = smoothstep( 0.08f, 0.60f, hSoft ) * g_flDropletStrength;
            // Градиент высоты ~±2 на текстель — масштаб маленький, иначе сдвиг на пол-экрана = шум.
            uvScene = uv + dropGrad * ( 0.014f * dropMask );
        }

        float3 scene = g_tColorBuffer.Sample( g_sBilinearMirror, uvScene ).rgb;

        // Внутри капли сцена сильно заблюрена (8 тапов кольцом) + мягкий блик и молочная дымка.
        if ( dropMask > 0.001f )
        {
            float2 bo = g_vInvViewportSize * 5.0f;
            float3 blurred = scene;
            blurred += g_tColorBuffer.SampleLevel( g_sBilinearMirror, uvScene + float2(  bo.x, 0.0f ), 0 ).rgb;
            blurred += g_tColorBuffer.SampleLevel( g_sBilinearMirror, uvScene - float2(  bo.x, 0.0f ), 0 ).rgb;
            blurred += g_tColorBuffer.SampleLevel( g_sBilinearMirror, uvScene + float2( 0.0f,  bo.y ), 0 ).rgb;
            blurred += g_tColorBuffer.SampleLevel( g_sBilinearMirror, uvScene - float2( 0.0f,  bo.y ), 0 ).rgb;
            blurred += g_tColorBuffer.SampleLevel( g_sBilinearMirror, uvScene + bo * 0.7f, 0 ).rgb;
            blurred += g_tColorBuffer.SampleLevel( g_sBilinearMirror, uvScene - bo * 0.7f, 0 ).rgb;
            blurred += g_tColorBuffer.SampleLevel( g_sBilinearMirror, uvScene + float2( bo.x, -bo.y ) * 0.7f, 0 ).rgb;
            blurred += g_tColorBuffer.SampleLevel( g_sBilinearMirror, uvScene - float2( bo.x, -bo.y ) * 0.7f, 0 ).rgb;
            scene = lerp( scene, blurred / 9.0f, dropMask );

            // Реалистичный вид: капля — просто мутная линза. Тёмный ободок по краю
            // (полное внутреннее отражение) + маленький блик; никакой молочной заливки.
            float rim = dropMask * ( 1.0f - dropMask ) * 4.0f;
            scene *= 1.0f - rim * 0.18f;
            float spec = saturate( -dropGrad.y * 1.8f ) * dropMask;
            scene += g_vTint * spec * 0.18f;
        }

        float intensity = saturate( g_flIntensity );
        if ( intensity <= 0.001f )
            return g_flRainDebug > 0.5f ? float4( 0.5f, 0.0f, 0.5f, 1.0f ) : float4( scene, 1.0f );

        float3 camPos = g_vCameraPositionWs;
        float3 rd = RayDir( uv );

        // Дистанция до сцены (для окклюзии геометрией) — капли за поверхностью не видны.
        float3 sceneWp = Depth::GetWorldPosition( ss );
        float sceneDist = length( sceneWp - camPos );

        // Ось стрика = видимая скорость дождя (падение+ветер−игрок). Базис (dir,u,v).
        float3 vel = g_vAppVel;
        float speed = max( length( vel ), 1e-3f );
        float3 dir = vel / speed;
        float3 up = ( abs( dir.z ) > 0.99f ) ? float3( 0.0f, 1.0f, 0.0f ) : float3( 0.0f, 0.0f, 1.0f );
        float3 u = normalize( cross( dir, up ) );
        float3 v = cross( dir, u );

        // Быстрее (сильный ветер / разгон) → длиннее стрики. Плюс явный буст от движения.
        float lenScale = clamp( speed / 1900.0f, 0.7f, 3.0f ) + g_flMove * 0.8f;

        // Короткий сегмент стрика (юниты), растёт со скоростью → в лицо полоски длиннее.
        float segLen = 12.0f * lenScale;

        // Плотность: в покое редко, заметно гуще от интенсивности И от движения игрока.
        float thresh = lerp( 0.95f, 0.72f, intensity ) - g_flMove * 0.30f;

        // Райтмарч по лучу (окклюзия по глубине). Видимые капли живут БЛИЗКО к камере —
        // марш идёт только до ближнего предела (дальний дождь субпиксельный), а шаги
        // уплотнены к камере (нелинейно). Иначе около камеры шаги редкие и тонкие колонки
        // пропускаются → капли выглядят точками (полоски только у ближних поверхностей).
        const int STEPS = 14;
        float tNear = 120.0f;
        float tFar = min( sceneDist, 900.0f );
        float rain = 0.0f;
        if ( tFar > tNear )
        {
            float dither = Ign( ss );
            [loop]
            for ( int s = 0; s < STEPS; s++ )
            {
                float fs = ( (float)s + dither ) / STEPS;   // 0..1
                float t = tNear + fs * fs * ( tFar - tNear ); // квадратично → гуще у камеры
                float3 wp = camPos + rd * t;

                // Окклюзия крышами/навесами: под поверхностью сверху капель нет.
                float vis = RainVisibility( wp );
                if ( vis <= 0.001f )
                    continue;

                // Две октавы капель — мелкая гуще, крупная реже. period большой (редко), segLen короткий.
                rain += ( RainField( wp, dir, u, v, speed, 11.0f, 150.0f, segLen,        0.0f,  thresh )
                        + RainField( wp, dir, u, v, speed, 19.0f, 200.0f, segLen * 1.1f, 37.0f, thresh ) * 0.7f ) * vis;
            }
            rain *= 0.75f;
        }

        rain = saturate( rain * g_flBoost ) * intensity;

        if ( g_flRainDebug > 0.5f )
            return float4( intensity, saturate( sceneDist / 2000.0f ), saturate( rain * 8.0f ), 1.0f );

        // Стрики: тонируем сцену к цвету капли (тинт уже освещён по времени суток —
        // ночью почти чёрный, дождь не «светится» поверх тёмной сцены).
        float3 col = lerp( scene, g_vTint, rain * 0.55f );
        col += g_vTint * rain * 0.12f;

        // Капли подхватывают локальный свет сцены: в темноте дождь читается в свете фар/фонарей.
        col += scene * rain * g_flPickup;

        // Брызги: фонтанчики вверх в тонкой полосе над землёй. Высота земли — из карты
        // окклюзии (на открытом месте «крыша» = сама земля), поэтому под навесом
        // groundZ = крыша, полоса не совпадает с полом → брызг там нет автоматически.
        if ( g_flSplash > 0.001f && sceneDist < 1200.0f && g_flRainOccSpanInv > 0.0f )
        {
            float2 occUv = ( sceneWp.xy - g_vRainOccOrigin ) * g_flRainOccSpanInv;
            if ( occUv.x > 0.0f && occUv.y > 0.0f && occUv.x < 1.0f && occUv.y < 1.0f )
            {
                float groundZ = g_tRainOcc.SampleLevel( g_sRainOccSampler, occUv, 0 );
                float hgt = sceneWp.z - groundZ;
                if ( hgt > -6.0f && hgt < 34.0f )
                {
                    float thresh = lerp( 0.94f, 0.7f, intensity );
                    float distFade = 1.0f - saturate( sceneDist / 1200.0f );
                    float spl0 = SplashField( sceneWp, groundZ, thresh );
                    float spl = saturate( spl0 ) * intensity * g_flSplash * distFade;
                    if ( spl > 0.003f )
                    {
                        // Брызги — вода, не краска: градиент поля (2 доп. пробы по экранному
                        // базису) даёт псевдо-нормаль → искажаем фон, как линзой, + чуть блика.
                        float3 rgt = ( abs( rd.z ) > 0.99f ) ? float3( 1.0f, 0.0f, 0.0f )
                                                             : normalize( cross( rd, float3( 0.0f, 0.0f, 1.0f ) ) );
                        float3 upv = cross( rgt, rd );
                        const float e = 3.0f;
                        float2 grad = float2( SplashField( sceneWp + rgt * e, groundZ, thresh ) - spl0,
                                              SplashField( sceneWp + upv * e, groundZ, thresh ) - spl0 );
                        float3 refr = g_tColorBuffer.SampleLevel( g_sBilinearMirror, uv + grad * 0.012f, 0 ).rgb;
                        col = lerp( col, refr, spl * 0.75f );
                        col += g_vTint * spl * 0.07f;
                    }
                }
            }
        }

        return float4( col, 1.0f );
    }
}
