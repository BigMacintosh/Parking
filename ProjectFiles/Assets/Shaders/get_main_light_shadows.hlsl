void GetMainLightShadows_half(float3 WorldPos, out half3 Direction, out half3 Color, out half DistanceAtten, out half ShadowAtten) {
    #if SHADERGRAPH_PREVIEW
        Direction     = half3(0.5, 0.5, 0);
        Color         = 1;
        DistanceAtten = 1;
        ShadowAtten   = 1;
    #else
        //half4 shadowCoord2 = half4(0, 0, 0, 0);
        #if SHADOWS_SCREEN
            half4 clipPos     = TransformWorldToHClip(WorldPos);
            half4 shadowCoord       = ComputeScreenPos(clipPos);
            //half4 shadowCoord = half4(0, 0, 0, 0);
        #else
            half4 shadowCoord       = TransformWorldToShadowCoord(WorldPos);
        #endif
        
        Light mainLight = GetMainLight(shadowCoord);
        //Light mainLight = GetMainLight();
        Direction       = mainLight.direction;
        Color           = mainLight.color;
        DistanceAtten   = mainLight.distanceAttenuation;
        
//        #if !defined(_MAIN_LIGHT_SHADOWS) || defined(_RECEIVE_SHADOWS_OFF)
//            ShadowAtten = 1.0h;
//        #endif
        
        #if SHADOWS_SCREEN
            ShadowAtten = SampleScreenSpaceShadowmap(shadowCoord);
            ShadowAtten = mainLight.shadowAttenuation;
        #else
            //ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
            //half shadowStrength                   = GetMainLightShadowStrength();
            //ShadowAtten = SampleShadowmap(shadowCoord,
            //                              TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture),
            //                              sampler_MainLightShadowmapTexture),
            //                              shadowSamplingData,
            //                              shadowStrength,
            //                              false);
            ShadowAtten = mainLight.shadowAttenuation;
        #endif
    #endif
}
