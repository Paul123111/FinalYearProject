Shader "Shaders/Wrap3" {
    Properties {
         _Size("Size",Vector) = (2,0,2,0)
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
         _BumpMap ("Bumpmap", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader {
 Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
     Pass {
        ZWrite On
        ColorMask 0
    }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade

        #pragma target 3.0

        sampler2D _MainTex,_BumpMap;

        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldPos: TEXCOORD2;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float4 _Size;

        void vert (inout appdata_full v, out Input o){
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.worldPos = mul(unity_ObjectToWorld, v.vertex);
        }


        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            float4 equation = (IN.worldPos.x/_Size.x)*(IN.worldPos.x/_Size.x);
            float4 InvertEquation = (IN.worldPos.z/_Size.z)*(IN.worldPos.z/_Size.z);
            float4 finalEquiation = max(equation,InvertEquation);
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = step(finalEquiation,0.5);
             o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
