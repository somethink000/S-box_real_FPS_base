HEADER
{
	Description = "Glass Material";
}

FEATURES
{
	#include "common/features.hlsl"
	Feature( F_BACKDROP, 0..1, "Scene backdrop" );
	Feature( F_BACKFACES, 0..1, "Features" );
	Feature( F_BLUR, 0..1, "Features" );
}

MODES
{
	// Forward, не легаси VrForward: у легаси-пути кулинг/атрибуты материала (renderbackfaces)
	// могут обрабатываться старым конвейером — все движковые двусторонние шейдеры на Forward.
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
	// Объектные координаты — для капель дождя (паттерн прибит к стеклу, не плывёт при движении машины).
	float3 vPositionOs : TEXCOORD14;
	// Поле face (SV_IsFrontFace) объявляет сам движковый pixelinput.hlsl при S_RENDER_BACKFACES.
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
	StaticCombo( S_BACKDROP, F_BACKDROP, Sys( ALL ) );
	// Двусторонний рендер (лобовое видно изнутри салона) — галка F_BACKFACES (Features), привязанная
	// к ДВИЖКОВОМУ комбо S_RENDER_BACKFACES: sbox_pixel.fxc (внутри common/pixel.hlsl) в конце
	// БЕЗУСЛОВНО ставит CullMode по этому комбо (else-ветка CullMode BACK), затирая любой свой
	// RenderState( CullMode ), написанный ДО инклюда — поэтому «не работал» и наш хардкод, и галка
	// движкового glass.shader (он это комбо вообще не объявляет). Изнанка корректна благодаря
	// развороту нормали по SV_IsFrontFace.
	StaticCombo( S_RENDER_BACKFACES, F_BACKFACES, Sys( ALL ) );
	// Блюр рефракции за отдельной галкой: машинам не нужен, а вариант без него дешевле.
	StaticCombo( S_BLUR, F_BLUR, Sys( ALL ) );

	#if S_BACKDROP
	#define BLEND_MODE_ALREADY_SET
	RenderState( BlendEnable, true );
	RenderState( SrcBlend, SRC_ALPHA );
	RenderState( DstBlend, INV_SRC_ALPHA );
	BoolAttribute( translucent, true );
	BoolAttribute( bWantsFBCopyTexture, true );
	#endif

	#include "common/pixel.hlsl"
	#include "common/classes/Depth.hlsl"
	// Свой SSR-трейс для лицевой стороны: пайплайновый пасс Dynamic Reflections стекло не покрывает
	// (трассирует по depth-препассу, куда translucent не пишет). Трейсим сами по hi-z depth chain.
	#include "common/classes/Bindless.hlsl"
	#include "common/classes/ScreenSpaceTrace.hlsl"
	#include "common/classes/DynamicReflections.hlsl"

	// Флаг материалу для нативных пассов — только в двустороннем варианте.
	#if S_RENDER_BACKFACES
	BoolAttribute( renderbackfaces, true );
	#endif

	#if S_BACKDROP
	Texture2D g_tFrameBufferCopyTexture < Attribute( "FrameBufferCopyTexture" ); SrgbRead( false ); >;
	#endif

	CreateInputTexture2D( TextureColor, Srgb, 8, "", "_color", "Material,10/10", Default3( 1.0, 1.0, 1.0 ) );
	CreateTexture2D( g_tColor ) < Channel( RGBA, Box( TextureColor ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;

	float g_flOpacity < Default( 0.1 ); Range( 0.0, 1.0 ); UiGroup( "Glass,10/20" ); >;
	float g_flRoughness < Default( 0.05 ); Range( 0.0, 1.0 ); UiGroup( "Glass,10/30" ); >;
	// «Глянец» ЛИЦЕВОЙ стороны: отражательность в лоб (F0). Физичное стекло — 0.04 (4% света),
	// отражения почти не видны; выше — гуще кубмапа/солнце/SSR (вес общий, френелевский).
	// Изнанки не касается — там отражений нет вовсе.
	float g_flReflectance < Default( 0.1 ); Range( 0.02, 0.5 ); UiGroup( "Glass,10/35" ); >;
	float3 g_vTint < Default3( 1.0, 1.0, 1.0 ); UiType( Color ); UiGroup( "Glass,10/40" ); >;
	#if S_BLUR
	float g_flBlur < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( "Glass,10/50" ); >;
	#endif

	// Капли дождя на стекле: глобальная мокрость от погоды × локальный множитель материала.
	float g_flWetness < Attribute( "RainWetness" ); Default( 0.0 ); >;
	float g_flRainDroplets < Default( 1.0 ); Range( 0.0, 1.0 ); UiGroup( "Glass,10/60" ); >;

	SamplerState g_sSampler < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;

	float RainHash21( float2 p )
	{
		p = frac( p * float2( 123.34, 456.21 ) );
		p += dot( p, p + 45.32 );
		return frac( p.x * p.y );
	}

	// Капли-бусины на стекле (две октавы, фаза жизни появление→высыхание). (град U, град V, высота).
	float3 RainBeads( float2 xy, float density )
	{
		float3 acc = 0.0;
		[unroll]
		for ( int k = 0; k < 2; k++ )
		{
			float cell = ( k == 0 ) ? 7.0 : 13.0;
			float2 p = xy / cell + (float)k * 7.31;
			float2 id = floor( p );
			float2 f = frac( p ) - 0.5;
			float rnd = RainHash21( id + (float)k * 13.7 );
			float cycle = g_flTime * 0.35 + rnd * 19.0;
			float slot = floor( cycle );
			if ( RainHash21( id + slot ) > density )
				continue;
			float life = frac( cycle );
			float fade = smoothstep( 0.0, 0.10, life ) * smoothstep( 1.0, 0.55, life );
			float2 off = ( float2( RainHash21( id + slot + 1.7 ), RainHash21( id + slot + 4.3 ) ) - 0.5 ) * 0.5;
			float r = lerp( 0.10, 0.30, RainHash21( id + slot + 9.1 ) );
			float2 d = f - off;
			float dist = max( length( d ), 1e-4 );
			float h = smoothstep( r, r * 0.15, dist ) * fade;
			acc += float3( -d / dist * h, h );
		}
		return acc;
	}

	// Стекающие дорожки: колонки + бегущая капля-комета со следом. xy.y растёт ВНИЗ.
	float3 RainStreaks( float2 xy )
	{
		float x = xy.x / 6.0;
		float id = floor( x );
		float rnd = RainHash21( float2( id, 3.7 ) );
		float f = frac( x ) - 0.5;
		float off = ( rnd - 0.5 ) * 0.6;
		float col = smoothstep( 0.15, 0.02, abs( f - off ) );
		float period = lerp( 60.0, 120.0, rnd );
		float speed = lerp( 20.0, 45.0, RainHash21( float2( id, 7.1 ) ) );
		float phase = frac( xy.y / period - g_flTime * speed / period + rnd * 11.0 );
		float s = ( phase - 0.5 ) * period;
		float head = smoothstep( 4.0, 0.0, abs( s ) );
		float trail = saturate( 1.0 + s / 22.0 ) * step( s, 0.0 ) * 0.5;
		float h = col * max( head, trail );
		return float3( -sign( f - off ) * h * 0.7, 0.0, h );
	}

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init( i );

		// С изнанки нормаль разворачиваем К КАМЕРЕ ГЕОМЕТРИЧЕСКИ (faceforward), НЕ по SV_IsFrontFace:
		// в живом вьюпорте флаг фронтфейса для этого пасса приходит инвертированным/битым (в офскрин-
		// рендере — корректным), нормаль оставалась «наружу», refract() отражал луч за камеру и стекло
		// сэмплило мусор из FB-копии — эффект «под картой» с изнанки.
		// bInsideFace = смотрим на стекло из салона (задняя грань к камере) — по нему в композите
		// глушим ВСЕ отражения: кубмапа = наружное небо (изнутри неверно), SSR мажет улицей из окна.
		bool bInsideFace = false;
		#if S_RENDER_BACKFACES
		{
			float3 vToPixelWs = normalize( i.vPositionWithOffsetWs.xyz );
			if ( dot( m.Normal, vToPixelWs ) > 0.0 )
			{
				m.Normal = -m.Normal;
				bInsideFace = true;
			}
		}
		#endif

		float3 baseCol = Tex2DS( g_tColor, g_sSampler, i.vTextureCoords.xy ).rgb * g_vTint;
		m.Albedo = baseCol;
		m.Roughness = g_flRoughness;
		m.Metalness = 0.0;

		// Капли дождя: возмущаем нормаль ДО расчёта рефракции — капли честно искажают
		// то, что за стеклом, и ловят френель-блик. Паттерн в объектных координатах.
		// Мягкая зависимость от влажности: остаточная влага (дорога визуально почти сухая, дождя нет)
		// всё ещё держит заметные капли на стекле; мёртвая зона у полностью сухого.
		float wetIn = saturate( ( g_flWetness - 0.06 ) / 0.94 );
		float wetGlass = pow( wetIn, 0.6 ) * saturate( g_flRainDroplets );
		if ( wetGlass > 0.001 )
		{
			float3 posOs = i.vPositionOs;
			float3 nOs = normalize( cross( ddy( posOs ), ddx( posOs ) ) );
			float horiz = saturate( abs( nOs.z ) * 1.6 - 0.5 );

			bool sideX = abs( nOs.x ) > abs( nOs.y );
			float2 planarUv = sideX ? posOs.yz : posOs.xz;

			float density = lerp( 0.30, 0.65, wetGlass );
			float3 beads = lerp( RainBeads( planarUv, density ), RainBeads( posOs.xy, density ), horiz );

			float2 streakUv = sideX ? float2( posOs.y, -posOs.z ) : float2( posOs.x, -posOs.z );
			float3 streaks = RainStreaks( streakUv ) * ( 1.0 - horiz );

			float2 grad = beads.xy + streaks.xy;
			float h = saturate( beads.z + streaks.z );

			float3 tW = ( abs( m.Normal.z ) > 0.9 ) ? float3( 1.0, 0.0, 0.0 ) : normalize( cross( float3( 0.0, 0.0, 1.0 ), m.Normal ) );
			float3 bW = cross( m.Normal, tW );

			// Сильнее, чем на кузове: капля должна заметно ломать картинку за стеклом.
			m.Normal = normalize( m.Normal + ( tW * grad.x + bW * grad.y ) * 1.1 * wetGlass );
			m.Roughness = lerp( m.Roughness, 0.02, h * wetGlass );
		}

		#if S_BACKDROP
		// Прозрачность через emission, не через alpha-blend с резким framebuffer (как glass.shader)
		m.Opacity = 1.0;
		m.Transmission = 0;

		float3 vViewRayWs = normalize( i.vPositionWithOffsetWs.xyz );
		float flNDotV = saturate( dot( -m.Normal, vViewRayWs ) );
		float3 vEnvBRDF = CalcBRDFReflectionFactor( flNDotV, m.Roughness, g_flReflectance );

		// Экранные отражения — ТОЛЬКО лицевая сторона: трейс отражённого луча по hi-z depth chain,
		// цвет хита из FB-копии кадра (мир уже отрисован). Изнутри салона SSR не гоняем — отражённый
		// луч попадал бы в пиксели улицы, видимой сквозь окно. Гейт по DynamicReflections::IsEnabled
		// (сценовый тумблер SSR). Промах/за экраном — confidence 0, остаётся кубмапа из Shade.
		float3 vSsrColor = 0.0;
		float flSsrConfidence = 0.0;
		if ( !bInsideFace && DynamicReflections::IsEnabled() )
		{
			float3 vAbsPosWs = i.vPositionWithOffsetWs.xyz + g_vCameraPositionWs.xyz;
			float3 vReflectWs = reflect( vViewRayWs, m.Normal );

			TraceResult trace = ScreenSpace::Trace( vAbsPosWs, vReflectWs, 64 );
			if ( trace.ValidHit && trace.Confidence > 0.0 )
			{
				// HitClipSpace.xy — уже UV экрана [0..1] (см. ProjectPosition в HierarchicalRaymarch).
				float2 vHitUv = trace.HitClipSpace.xy * g_vFrameBufferCopyInvSizeAndUvScale.zw;
				vSsrColor = g_tFrameBufferCopyTexture.SampleLevel( g_sTrilinearMirror, vHitUv, 0 ).rgb;
				flSsrConfidence = trace.Confidence;
			}
		}

		float flDepthPs = 1.0 - Depth::GetNormalized( i.vPositionSs.xy );
		float3 vRefractionWs = RecoverWorldPosFromProjectedDepthAndRay( flDepthPs, vViewRayWs ) - g_vCameraPositionWs;
		float flDistanceVs = distance( i.vPositionWithOffsetWs.xyz, vRefractionWs );

		const float flRefractionStrength = 1.005;
		float3 vRefractRayWs = refract( vViewRayWs, m.Normal, 1.0 / flRefractionStrength );
		// Смещение рефракции масштабируем НЕ полной глубиной сцены: изнутри салона до мира десятки
		// метров, и возмущённая каплями нормаль сдвигала UV на пол-экрана — всё «мылилось». Тонкое
		// стекло физически смещает картинку на ~свою толщину, поэтому дистанцию клампим: капли дают
		// локальное преломление, а не кашу. Снаружи (до салона близко) поведение прежнее.
		float flRefractDist = min( flDistanceVs, 16.0 );
		float3 vRefractWorldPosWs = i.vPositionWithOffsetWs.xyz + vRefractRayWs * flRefractDist;

		float4 vPositionPs = Position4WsToPs( float4( vRefractWorldPosWs, 0 ) );
		float2 vPositionSs = vPositionPs.xy / vPositionPs.w;
		vPositionSs = vPositionSs * 0.5 + 0.5;
		vPositionSs.y = 1.0 - vPositionSs.y;

		float2 vUV = vPositionSs * g_vFrameBufferCopyInvSizeAndUvScale.zw;

		// Явный mip 0: обычный Sample выбирает мип по производным UV, а на изнанке (развёрнутая
		// нормаль) рефракционные UV дёргаются от пикселя к пикселю → трилинейка уходит в глубокие
		// мипы и всё за стеклом мылится. С фиксированным уровнем изнанка такая же резкая, как лицо.
		float3 vRefractionColor = g_tFrameBufferCopyTexture.SampleLevel( g_sTrilinearMirror, vUV, 0 ).rgb;

		#if S_BLUR
		const int nNumMips = 7;
		float flAmount = g_flBlur * m.Roughness * ( 1.0 - ( 1.0 / max( flDistanceVs, 0.5 ) ) );
		flAmount /= max( flNDotV, 0.05 );
		if ( g_flBlur > 0.001 )
		{
			vRefractionColor = g_tFrameBufferCopyTexture.SampleLevel( g_sTrilinearMirror, vUV, sqrt( saturate( flAmount ) ) * nNumMips ).rgb;
		}
		#endif

		float flSee = saturate( 1.0 - g_flOpacity );
		float flTintLum = dot( baseCol, float3( 0.299, 0.587, 0.114 ) );
		float3 transMul = lerp( float3( 1, 1, 1 ), saturate( baseCol + 0.15 ), step( 0.01, flTintLum ) );

		// Салон (изнанка): стекло — просто окно наружу (рефракция) + капли искажением, БЕЗ отражений.
		// НЕ идём через ShadingModelStandard::Shade вовсе: именно он добавляет кубмапное indirect-
		// отражение, и занулить его материалом НЕЛЬЗЯ — Schlick-френель уходит в 1 на скользящих углах
		// даже при F0=0 (потому F0=0 и не убирал кубмапу на косых). Отдаём тонированную рефракцию сразу.
		if ( bInsideFace )
			return float4( vRefractionColor * flSee * transMul, 1.0 );

		// Снаружи: рефракция, ослабленная френелем под отражение, + SSR по экрану поверх, затем
		// полный Shade с кубмапой/солнцем. «Металл» с серым альбедо = чистый F0 без диффузы: у Material
		// нет отдельного F0, движок берёт lerp(0.04, Albedo, Metalness). Metalness=1 + Albedo=reflectance
		// даёт F0=reflectance — кубмапа/солнце/SSR глянцевее и НЕ красятся тонировкой (цвет — в transMul).
		m.Emission = lerp( vRefractionColor, 0.0, vEnvBRDF ) * flSee * transMul;
		m.Emission += vSsrColor * vEnvBRDF * flSsrConfidence;
		m.Metalness = 1.0;
		m.Albedo = g_flReflectance;

		float4 result = ShadingModelStandard::Shade( i, m );
		result.a = 1.0;
		return result;
		#else
		m.Opacity = 1.0;
		m.Transmission = 0;
		m.Emission = 0;
		return ShadingModelStandard::Shade( i, m );
		#endif
	}
}
