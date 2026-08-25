HEADER
{
    DevShader = true;
    Description = "Vehicle motion blur — per-pixel velocity (camera + per-object), velocity-weighted reconstruction (racing-game style)";
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
    #include "common/classes/Motion.hlsl"

    Texture2D g_tColorBuffer < Attribute( "ColorBuffer" ); SrgbRead( true ); >;

    // Множитель длины смаза (≈ «выдержка»). Низкий = лёгкий смаз на крейсе, высокий = разгон.
    float g_flStrength < Attribute( "Strength" ); Default( 1.0f ); >;
    // 0..1 — насколько держать центр резким, а смаз гнать к краям (фокус-расширения вперёд).
    float g_flEdgeBias < Attribute( "EdgeBias" ); Default( 0.6f ); >;
    // Потолок длины смаза в пикселях — ограничивает «кашу» у ближних поверхностей на скорости.
    float g_flMaxBlur < Attribute( "MaxBlur" ); Default( 64.0f ); >;

    // Velocity-буфер динамики: rg = реальный экранный сдвиг пикселя машины (свой + камеры),
    // b = линейная глубина, a = покрытие. Заполняется vehicle_velocity.shader по всем машинам в кадре.
    Texture2D g_tVehicleVelocity < Attribute( "VehicleVelocity" ); SrgbRead( false ); >;
    float g_flHasVelocity < Attribute( "HasVelocity" ); Default( 0.0f ); >;

    // Кабина от первого лица: ближе этой линейной глубины кадр держится резким (стойки/торпедо),
    // к 2× дистанции смаз выходит на полную. 0 = выключено (третье лицо).
    float g_flCabinSharpDist < Attribute( "CabinSharpDist" ); Default( 0.0f ); >;

    // Interleaved gradient noise — дешёвый дизер вдоль смаза, чтобы выборки были гладкими.
    float Ign( float2 pos )
    {
        return frac( 52.9829189 * frac( dot( pos, float2( 0.06711056, 0.00583715 ) ) ) );
    }

    // Насколько сцена может быть ГЛУБЖЕ записанной поверхности машины, чтобы пиксель всё ещё
    // считался машиной (юниты). Стекло velocity пишет, а глубину сцены — нет → под его пикселями
    // глубина салона/кузова за ним; в пределах этой дистанции это «стекло поверх собственного
    // интерьера» — резкое. Дальше — сквозь стекло виден мир (дорога), мажется камера-вектором.
    #define GLASS_INTERIOR_DEPTH 180.0

    // Экранная скорость пикселя (в пикселях за кадр, указывает на прошлую позицию).
    // Статика — репроекция камеры по depth; машины — их реальный вектор из velocity-буфера
    // (сверка глубины отбрасывает записи машин, перекрытых в сцене чем-то ближним).
    float2 FetchVelocity( float2 ss )
    {
        if ( g_flHasVelocity > 0.5 )
        {
            float4 ov = g_tVehicleVelocity.Load( int3( ss, 0 ) );
            if ( ov.a > 0.5 )
            {
                // Сверка глубин АСИММЕТРИЧНАЯ. Сцена БЛИЖЕ записи → машину перекрыло что-то
                // ближнее → камера-вектор. Сцена ГЛУБЖЕ — это стекло: в пределах салона за ним
                // своя же машина → вектор машины (иначе стекло/интерьер мазались, хотя они часть
                // машины), сильно глубже — мир через лобовое → камера-вектор.
                // Окно «салона» масштабируется дистанцией до стекла: в FP стекло в 30-60 юнитах
                // от камеры → окно сжимается, и дорога сразу за капотом (низ экрана) мажется,
                // а не липнет к окну как «салон». Кромки стоек в FP добивает CabinSharpDist ниже.
                float sceneZ = Depth::GetLinear( ss );
                float dz = sceneZ - ov.z; // >0 — сцена глубже поверхности машины
                float interiorWindow = min( GLASS_INTERIOR_DEPTH, ov.z * 0.5 );
                if ( dz > -( sceneZ * 0.05 + 16.0 ) && dz < interiorWindow )
                    return ov.xy;
            }
        }

        return Motion::Get( ss ).xy - ss;
    }

    float4 MainPs( PixelInput i ) : SV_Target0
    {
        float2 ss = i.vPositionSs.xy;
        float2 uv = ss * g_vInvViewportSize;

        // Край экрана тянем сильнее, центр держим резче — натуральная форма гоночного смаза.
        float2 d = uv - 0.5;
        float edge = saturate( dot( d, d ) * 4.0 );
        float edgeW = lerp( 1.0 - g_flEdgeBias, 1.0, edge );
        float exposure = g_flStrength * edgeW;

        float2 streak = FetchVelocity( ss ) * exposure;

        // FP-кабина: близкое (стойки, торпедо, руль) держим резким независимо от классификации —
        // добивает остатки полос на кромках и микро-смаз от движения головы относительно кузова.
        if ( g_flCabinSharpDist > 0.5 )
            streak *= saturate( Depth::GetLinear( ss ) / g_flCabinSharpDist - 1.0 );

        float len = length( streak );
        if ( len > g_flMaxBlur )
        {
            streak *= g_flMaxBlur / max( len, 1e-4 );
            len = g_flMaxBlur;
        }

        // Меньше пикселя — смаза не видно, возвращаем резкий кадр. Машина, едущая с камерой,
        // сюда попадает сама: её реальный экранный вектор ≈ 0 (никакого выреза не нужно).
        if ( len < 1.0 )
            return g_tColorBuffer.Sample( g_sBilinearMirror, uv );

        float2 dirN = streak / len;
        float2 streakUv = streak * g_vInvViewportSize;
        float dither = Ign( ss );

        // Центр всегда участвует с полным весом.
        float4 color = g_tColorBuffer.Sample( g_sBilinearMirror, uv );
        float wsum = 1.0;
        const int numSamples = 12;

        // Реконструкция: сэмпл размазывается на центр только если ЕГО СОБСТВЕННАЯ скорость
        // дотягивает его сюда. Статичный относительно камеры объект (своя машина) в чужой смаз
        // не попадает, а объект с другим вектором (встречка, трафик) мажется по-своему.
        [unroll]
        for ( int s = 0; s < numSamples; s++ )
        {
            float t = ( (float)s + dither ) / (float)numSamples - 0.5;
            float2 sss = ss + streak * t;
            float2 suv = uv + streakUv * t;

            float2 vs = FetchVelocity( sss ) * exposure;
            float vAlong = min( abs( dot( vs, dirN ) ), g_flMaxBlur );

            // Конус: сэмпл достаёт до центра, пока дистанция < половины его смаза (+мягкая кромка ~2px).
            float dist = abs( t ) * len;
            float w = saturate( 1.0 - ( dist - vAlong * 0.5 ) * 0.5 );

            color += g_tColorBuffer.Sample( g_sBilinearMirror, suv ) * w;
            wsum += w;
        }

        return color / wsum;
    }
}
