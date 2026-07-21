Shader "Hidden/ViewmodelComposite"
{
    HLSLINCLUDE
    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch switch2
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Cull Off
            ZWrite On
            ZTest Always

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            TEXTURE2D_X(_WeaponColor);
            TEXTURE2D_X(_WeaponDepth);

            struct FragOutput
            {
                float4 color : SV_Target;
                float depth  : SV_Depth;
            };

            FragOutput Frag(Varyings varyings)
            {
                uint2 pixelCoord = uint2(varyings.positionCS.xy);
                float weaponDepth = LOAD_TEXTURE2D_X(_WeaponDepth, pixelCoord).r;

                if (weaponDepth == UNITY_RAW_FAR_CLIP_VALUE)
                    discard; // тут оружия нет — оставляем сцену как есть

                FragOutput output;
                output.color = LOAD_TEXTURE2D_X(_WeaponColor, pixelCoord);
                output.depth = weaponDepth;
                return output;
            }
            ENDHLSL
        }
    }
    Fallback Off
}