Shader "Custom/Wrap2"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float3 worldPos : TEXCOORD2;
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float4 newPos = IN.positionOS;
                //newPos.x = fmod(newPos.x, 10);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                //OUT.worldPos = (mul(unity_ObjectToWorld, IN.positionOS.xyz))%10;
                OUT.positionHCS = TransformObjectToHClip(newPos);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                //OUT.uv.z += OUT.worldPos.x;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                float dist = IN.worldPos.x;
                     // computes the distance between the fragment position 
                     // and the origin (the 4th coordinate should always be 
                     // 1 for points).
                  
                  if (floor(dist)%2 == 0)
                  {
                     return color*float4(0.0, 1.0, 0.0, 1.0); 
                        // color near origin
                  }
                  else
                  {
                     return color*float4(0.1, 0.1, 1.0, 1.0); 
                        // color far from origin
                  }

                return color;
            }
            ENDHLSL
        }
    }
}
