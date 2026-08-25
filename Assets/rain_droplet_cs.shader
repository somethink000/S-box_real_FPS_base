HEADER
{
    DevShader = true;
    Description = "Screen-space droplet simulation (DriveClub-style) — ping-pong heightfield: spawn, mass-dependent slide, merge, trails, evaporation";
}

MODES
{
    Default();
}

CS
{
    #include "system.fxc"

    // Пинг-понг: предыдущее состояние (SRV) → новое (UAV). Один канал = масса воды.
    Texture2D<float> PrevTex < Attribute( "PrevTex" ); >;
    RWTexture2D<float> OutTex < Attribute( "OutTex" ); >;

    SamplerState g_sBilinearClampCs < Filter( BILINEAR ); AddressU( CLAMP ); AddressV( CLAMP ); >;

    float2 g_vRes      < Attribute( "Res" );      Default2( 512.0, 288.0 ); >;
    float  g_flSimDt   < Attribute( "SimDt" );    Default( 0.016 ); >;
    float  g_flSimTime < Attribute( "SimTime" );  Default( 0.0 ); >;
    // Поток по стеклу в UV/сек: гравитация экрана + аэродинамика (ускорение/скорость камеры).
    float2 g_vFlowUv   < Attribute( "FlowUv" );   Default2( 0.0, 0.35 ); >;
    // Радиальный разлёт от центра экрана (UV/сек) — встречный поток на объективе камеры.
    float  g_flRadialUv < Attribute( "RadialUv" ); Default( 0.0 ); >;
    // Интенсивность спавна капель 0..~1.5 (дождь × взгляд в небо × скорость).
    float  g_flSpawn   < Attribute( "SpawnRate" ); Default( 0.0 ); >;
    // Фактор скорости игрока 0..1 (как Move у стриков): ветер срывает даже одиночные капли.
    float  g_flWind    < Attribute( "Wind" );     Default( 0.0 ); >;
    // 1 = обнулить поле (первый кадр / телепорт камеры).
    int    g_nReset    < Attribute( "Reset" );    Default( 0 ); >;

    float Hash21( float2 p )
    {
        p = frac( p * float2( 123.34f, 456.21f ) );
        p += dot( p, p + 45.32f );
        return frac( p.x * p.y );
    }

    // Скорость стекания от массы (доля от FlowUv). Переходное состояние как в DriveClub:
    // в покое одиночная капля (~0.95) только ползёт, стекает лишь слияние двух (порог 1.15+).
    // На скорости (g_flWind) порог падает — но НЕ до нуля: одиночная капля (~0.9) на полном
    // ходу лишь МЕДЛЕННО ползёт (≈0.15 потока), иначе капли сдувает за край моментально и
    // на скорости экран всегда пустой, хотя дождь капает.
    float SlideSpeed( float h )
    {
        float t0 = lerp( 1.15f, 0.8f, g_flWind );
        float t1 = lerp( 1.8f, 1.35f, g_flWind );
        return 0.08f * smoothstep( 0.35f, 0.9f, h ) + smoothstep( t0, t1, h );
    }

    [numthreads( 8, 8, 1 )]
    void MainCs( uint3 id : SV_DispatchThreadID )
    {
        if ( id.x >= (uint)g_vRes.x || id.y >= (uint)g_vRes.y )
            return;

        if ( g_nReset != 0 )
        {
            OutTex[ id.xy ] = 0.0f;
            return;
        }

        float2 texel = 1.0f / g_vRes;
        float2 uv = ( float2( id.xy ) + 0.5f ) * texel;
        float dt = clamp( g_flSimDt, 0.001f, 0.05f );

        float2 flowDir = g_vFlowUv;

        // Радиальный разлёт от центра (встречный воздух на объективе камеры).
        float2 fromCenter = uv - 0.5f;
        float fcLen = max( length( fromCenter ), 1e-3f );
        flowDir += ( fromCenter / fcLen ) * g_flRadialUv;

        // Виляние пути: боковой снос меняется вдоль экрана И во времени — стекающая капля
        // идёт живой змейкой, а не ровной полоской. Статичные капли не трогает (их скорость ~0).
        float colRnd = Hash21( float2( floor( uv.x * g_vRes.x / 6.0f ), 7.7f ) );
        float wob = sin( uv.y * 47.0f + g_flSimTime * 2.2f + colRnd * 6.2832f );
        flowDir.x += ( wob * 0.30f + ( colRnd - 0.5f ) * 0.15f ) * length( g_vFlowUv );

        // Адвекция (semi-lagrangian). КРИТИЧНО: скорость оцениваем по максимальному шагу НАЗАД —
        // если брать скорость своей клетки, пустая клетка под каплей (v=0) никогда её не получит
        // и капли вообще не движутся, только испаряются в точечный шум.
        float h0 = PrevTex.SampleLevel( g_sBilinearClampCs, uv, 0 );
        float hUp = PrevTex.SampleLevel( g_sBilinearClampCs, uv - flowDir * dt, 0 );
        float vIn = SlideSpeed( max( h0, hUp ) );
        // Индивидуальная скорость: крупноклеточный случайный множитель (клетки ~12 пикс) —
        // капли едут кто быстрее, кто ленивее, и по пути меняют темп, проходя зоны.
        vIn *= 0.6f + 0.9f * Hash21( floor( uv * g_vRes / 12.0f ) + 3.3f );
        float h = PrevTex.SampleLevel( g_sBilinearClampCs, uv - flowDir * ( vIn * dt ), 0 );

        // Следа НЕТ: на объективе камеры стекающая капля не оставляет видимого пути
        // (дорожки живут на лобовом в glass_material). Слияние «при наезде» работает и без
        // следа — едущая капля забирает клетку через адвекцию.

        // Анти-диффузия: билинейный семплинг размывает движущиеся капли — стягиваем обратно.
        // Шарп ЗАЖАТ в локальный минимакс: без клампа одиночный пиксель (морось, огрызок следа)
        // самоусиливается до максимума и экран зарастает точечным шумом.
        float hl = PrevTex.SampleLevel( g_sBilinearClampCs, uv - float2( texel.x, 0.0f ), 0 );
        float hr = PrevTex.SampleLevel( g_sBilinearClampCs, uv + float2( texel.x, 0.0f ), 0 );
        float hu = PrevTex.SampleLevel( g_sBilinearClampCs, uv - float2( 0.0f, texel.y ), 0 );
        float hd = PrevTex.SampleLevel( g_sBilinearClampCs, uv + float2( 0.0f, texel.y ), 0 );
        float avg = ( hl + hr + hu + hd ) * 0.25f;
        float sharpened = h + ( h - avg ) * saturate( 33.0f * dt ); // нормировано к 60 fps
        float lo = min( h, min( min( hl, hr ), min( hu, hd ) ) );
        float hi = max( h, max( max( hl, hr ), max( hu, hd ) ) );
        h = clamp( sharpened, lo, hi );

        // Испарение: морось/следы сохнут за секунды, сидящие капли живут долго (~15с),
        // чтобы успевали дождаться второй капли и слиться.
        float evap = lerp( 0.35f, 0.05f, smoothstep( 0.15f, 0.8f, h ) );
        h = max( 0.0f, h - evap * dt );

        if ( g_flSpawn > 0.001f )
        {
            // Спавн: сетка клеток ~18 пикс, клетка кидает кубик каждый слот (24 Гц); удачная
            // клетка НАЛИВАЕТ каплю в течение слота (капля растёт и срывается — живее, чем скачок).
            // Слот в хэш подаётся через R2-последовательность МАЛЕНЬКИМИ числами: сырой slot
            // растёт со временем, большие аргументы убивают точность frac() в хэше и спавн
            // вырождается в коррелированные ряды (капли «сверху полосами»).
            const float cellPx = 18.0f;
            const float slotHz = 24.0f;
            float2 cellUv = uv * g_vRes / cellPx;
            float2 cellId = floor( cellUv );
            float slotW = fmod( floor( g_flSimTime * slotHz ), 4096.0f );
            float2 slotOff = frac( slotW * float2( 0.75487767f, 0.56984029f ) ) * 37.0f;
            float rnd = Hash21( cellId + slotOff );
            float chance = saturate( g_flSpawn ) * 0.012f;
            if ( rnd < chance )
            {
                float2 off = ( float2( Hash21( cellId + slotOff + 3.7f ), Hash21( cellId + slotOff + 8.1f ) ) - 0.5f ) * 0.55f;
                float dropR = lerp( 2.8f, 5.5f, Hash21( cellId + slotOff + 13.9f ) );  // радиус в пикселях
                float d = length( ( cellUv - ( cellId + 0.5f + off ) ) * cellPx );
                // Масса одиночной капли НИЖЕ порога стекания (1.15) — она сидит на месте;
                // поедет только когда на неё упадёт вторая. Масса случайная — после слияния
                // капли разные по весу и текут с разной скоростью.
                float mass = lerp( 0.80f, 1.05f, Hash21( cellId + slotOff + 21.3f ) );
                h += smoothstep( dropR, dropR * 0.25f, d ) * mass * ( dt * slotHz );
            }

            // Морось: редкие субпиксельные микро-капли по всему полю.
            float mist = step( 1.0f - saturate( g_flSpawn ) * 0.0004f, Hash21( float2( id.xy ) + frac( g_flSimTime ) * 71.3f ) );
            h += mist * 0.15f;
        }

        OutTex[ id.xy ] = min( h, 2.0f );
    }
}
