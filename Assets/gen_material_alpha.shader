FEATURES
{
    #include "common/features.hlsl"
    Feature( F_TRANSLUCENT, 0..1, "Rendering" );
}

MODES
{
    VrForward();
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

	//
	// PBR Texture inputs
	//
	CreateInputTexture2D( TextureColor, Srgb, 8, "", "_color", "Material,10/10", Default3( 1.0, 1.0, 1.0 ) );
	CreateInputTexture2D( TextureNormal, Linear, 8, "", "_normal", "Material,10/20", Default3( 0.5, 0.5, 1.0 ) );
	CreateInputTexture2D( TextureRoughness, Linear, 8, "", "_rough", "Material,10/30", Default( 0.5 ) );
	CreateInputTexture2D( TextureDisplacement, Linear, 8, "", "_height", "Material,10/40", Default( 0.5 ) );
	CreateInputTexture2D( TextureOpacity, Linear, 8, "", "_opacity", "Material,10/45", Default( 1.0 ) );

	CreateTexture2D( g_tColor ) < Channel( RGBA, Box( TextureColor ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;
	CreateTexture2D( g_tNormal ) < Channel( RGB, Box( TextureNormal ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	CreateTexture2D( g_tRoughness ) < Channel( R, Box( TextureRoughness ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	CreateTexture2D( g_tDisplacement ) < Channel( R, Box( TextureDisplacement ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
	// Opacity может быть PNG с alpha: используем A как "валидность" маски.
	CreateTexture2D( g_tOpacity ) < Channel( RGBA, Box( TextureOpacity ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

	float g_flRoughnessMultiplier < Default( 1.0 ); Range( 0.0, 2.0 ); UiGroup( "Material,10/50" ); >;
	bool g_bOpacityMaskUseAlpha < Default( 0.0 ); UiGroup( "Material,10/55" ); >;
	#if ( !S_TRANSLUCENT )
		float g_flOpacityCutoff < Default( 0.5 ); Range( 0.0, 1.0 ); UiGroup( "Material,10/60" ); >;
		float g_flOpacityMipBoost < Default( 0.5 ); Range( 0.0, 1.0 ); UiGroup( "Material,10/65" ); >;
	#else
		// Резкость маски на дистанции/под острым углом: мипы размазывают альфу вширь
		// (разметка вдали становится широкой и мыльной) — возвращаем контраст. 0 = выкл.
		float g_flOpacitySharpness < Default( 0.7 ); Range( 0.0, 2.0 ); UiGroup( "Material,10/60" ); >;
	#endif

	// Перекраска: умножается на альбедо. По умолчанию белый = без эффекта.
	// Имя совпадает с VehicleComponent.BodyTintParameter.
	float3 g_vColorTint < Default3( 1.0, 1.0, 1.0 ); UiType( Color ); UiGroup( "Material,10/70" ); >;

	SamplerState g_sSampler < Filter( ANISO ); MaxAniso( 16 ); AddressU( WRAP ); AddressV( WRAP ); >;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = Material::Init( i );

		float2 vTexCoords = i.vTextureCoords.xy;

		{
			static const float flParallaxStrength = 0.035;
			static const float flParallaxFadeDistanceInv = 0.002222;
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

		// Sample textures
		float4 colorSample = Tex2DS( g_tColor, g_sSampler, vTexCoords );
		float3 normalSample = Tex2DS( g_tNormal, g_sSampler, vTexCoords ).rgb;
		float flRoughness = Tex2DS( g_tRoughness, g_sSampler, vTexCoords ).r;
		float4 opacitySample = Tex2DS( g_tOpacity, g_sSampler, vTexCoords );
		float flOpacity = opacitySample.r;
		// Опционально: если opacity-маска — PNG с прозрачным фоном, то по прозрачным пикселям НЕ применяем маску.
		// (пустая область не режет материал)
		if ( g_bOpacityMaskUseAlpha )
			flOpacity = lerp( 1.0, flOpacity, opacitySample.a );

		// Финальная прозрачность
		#if ( S_TRANSLUCENT )
			// На дистанции/под острым углом мип-футпринт вытягивается: альфа тонкой фичи
			// (линия разметки) размазывается по соседям — линия визуально шире и мыльная.
			// Лечение: сэмпл маски с отрицательным bias (резче мип) + возврат контраста
			// вокруг 0.5 пропорционально мипу. При mip 0 (вблизи) — без изменений.
			if ( g_flOpacitySharpness > 0.001 )
			{
				static const float flSharpMipBias = -1.0;
				static const float flSharpMipMax = 4.0;

				float flMip = max( g_tOpacity.CalculateLevelOfDetail( g_sSampler, vTexCoords ) + flSharpMipBias, 0.0 );
				float4 vOpacitySharp = g_tOpacity.SampleBias( g_sSampler, vTexCoords, flSharpMipBias );
				float flOpacityBase = vOpacitySharp.r;
				if ( g_bOpacityMaskUseAlpha )
					flOpacityBase = lerp( 1.0, flOpacityBase, vOpacitySharp.a );

				float flSharp = 1.0 + min( flMip, flSharpMipMax ) * g_flOpacitySharpness;
				flOpacity = saturate( ( flOpacityBase - 0.5 ) * flSharp + 0.5 );
			}

			float flFinalOpacity = flOpacity * g_flOpacityScale;
		#else
			// Мипы усредняют тонкие светлые детали с пустым фоном: альфа затухает ~2^-mip,
			// с расстоянием (под острым углом — раньше) проваливается под cutoff, и clip
			// убивает материал целиком. Компенсация экспоненциальная: exp2( mip * boost )
			// держит тонкие фичи, кэп мипа не даёт поднять среднее всей текстуры вдали.
			// Отрицательный bias берёт мип резче — кромка не расползается в ширину.
			static const float flOpacityMipMax = 7.0;
			static const float flOpacityMipBias = -1.0;

			float flMip = max( g_tOpacity.CalculateLevelOfDetail( g_sSampler, vTexCoords ) + flOpacityMipBias, 0.0 );
			float4 vOpacitySharp = g_tOpacity.SampleBias( g_sSampler, vTexCoords, flOpacityMipBias );
			float flOpacitySharp = vOpacitySharp.r;
			if ( g_bOpacityMaskUseAlpha )
				flOpacitySharp = lerp( 1.0, flOpacitySharp, vOpacitySharp.a );
			flOpacity = saturate( flOpacitySharp * exp2( min( flMip, flOpacityMipMax ) * g_flOpacityMipBoost ) );

			float flFinalOpacity = saturate( ( flOpacity - g_flOpacityCutoff ) / max( fwidth( flOpacity ), 0.001 ) + 0.5 );
			clip( flFinalOpacity - 0.5 );
		#endif

		// Material properties
		m.Albedo = colorSample.rgb * g_vColorTint;
		m.Opacity = flFinalOpacity;

		// Normal mapping
		normalSample = mad( normalSample, 2.0, -1.0 );
		m.Normal = TransformNormal( normalSample, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );

		m.Roughness = flRoughness * g_flRoughnessMultiplier;
		m.Metalness = 0.0;

		return ShadingModelStandard::Shade( m );
	}
}
