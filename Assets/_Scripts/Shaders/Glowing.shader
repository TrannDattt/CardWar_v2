// Shader "UI/Glowing"
// {
//     Properties
//     {
//         [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
//         _Color ("Tint", Color) = (1,1,1,1)

//         // Stencil để không lỗi Mask
//         _StencilComp ("Stencil Comparison", Float) = 8
//         _Stencil ("Stencil ID", Float) = 0
//         _StencilOp ("Stencil Operation", Float) = 0
//         _StencilWriteMask ("Stencil Write Mask", Float) = 255
//         _StencilReadMask ("Stencil Read Mask", Float) = 255
//         _ColorMask ("Color Mask", Float) = 15

//         // Glow settings
//         _GlowColor ("Glow Color", Color) = (1,1,0,1)
//         _GlowStrength ("Glow Strength", Range(0, 5)) = 1
//         _GlowSize ("Glow Size", Range(0, 10)) = 2
//         _GlowSamples ("Glow Samples", Range(4, 32)) = 16
//     }

//     SubShader
//     {
//         Tags
//         {
//             "Queue"="Transparent"
//             "IgnoreProjector"="True"
//             "RenderType"="Transparent"
//             "PreviewType"="Plane"
//             "CanUseSpriteAtlas"="True"
//         }

//         Stencil
//         {
//             Ref [_Stencil]
//             Comp [_StencilComp]
//             Pass [_StencilOp]
//             ReadMask [_StencilReadMask]
//             WriteMask [_StencilWriteMask]
//         }

//         Cull Off
//         Lighting Off
//         ZWrite Off
//         ZTest [unity_GUIZTestMode]
//         Blend SrcAlpha OneMinusSrcAlpha
//         ColorMask [_ColorMask]

//         Pass
//         {
//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #include "UnityCG.cginc"

//             struct appdata_t
//             {
//                 float4 vertex   : POSITION;
//                 float4 color    : COLOR0;
//                 float2 texcoord : TEXCOORD0;
//             };

//             struct v2f
//             {
//                 float4 vertex   : SV_POSITION;
//                 fixed4 color    : COLOR0;
//                 float2 texcoord : TEXCOORD0;
//             };

//             sampler2D _MainTex;
//             fixed4 _Color;

//             fixed4 _GlowColor;
//             float _GlowStrength;
//             float _GlowSize;
//             int _GlowSamples;

//             v2f vert (appdata_t v)
//             {
//                 v2f o;
//                 o.vertex = UnityObjectToClipPos(v.vertex);
//                 o.texcoord = v.texcoord;
//                 o.color = v.color * _Color;
//                 return o;
//             }

//             fixed4 frag (v2f i) : SV_Target
//             {
//                 float2 uv = i.texcoord;

//                 // Base sprite
//                 fixed4 baseCol = tex2D(_MainTex, uv) * i.color;

//                 // Glow blur quanh viền
//                 float glow = 0;
//                 for (int k = 0; k < _GlowSamples; k++)
//                 {
//                     float angle = (6.2831853 / _GlowSamples) * k; // 2*PI
//                     float2 dir = float2(cos(angle), sin(angle));
//                     glow += tex2D(_MainTex, uv + dir * 0.005 * _GlowSize).a;
//                 }

//                 glow /= _GlowSamples; // normalize

//                 fixed4 glowCol = _GlowColor * glow * _GlowStrength;

//                 fixed4 finalCol = glowCol + baseCol;
//                 finalCol.a = saturate(baseCol.a + glowCol.a * 0.6);

//                 return finalCol;
//             }
//             ENDCG
//         }
//     }
// }


Shader "UI/Glowing"
{
    Properties
    {
        _Color ("Glow Color", Color) = (1, 1, 1, 1)
        _Size ("Glow Size", Range(0, 0.5)) = 0.1
        _Intensity ("Glow Intensity", Range(0, 10)) = 2
        _BlurRadius ("Blur Radius", Range(0, 0.1)) = 0.02 // Thêm bán kính blur
        _MainTex ("Base Texture", 2D) = "white" {} // Texture UI

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        LOD 200

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha // Blend phù hợp cho UI

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 worldPos : TEXCOORD1;
            };

            fixed4 _Color;
            float _Size;
            float _Intensity;
            float _BlurRadius;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseColor = tex2D(_MainTex, i.uv) * i.color;
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                float glow = 1.0 - saturate(dist / _Size);
                float blur = 0.0;
                float2 blurStep = _BlurRadius * float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
                for (int j = -2; j <= 2; j++)
                {
                    for (int k = -2; k <= 2; k++)
                    {
                        float2 offset = float2(j, k) * blurStep;
                        float sampleDist = distance(i.uv + offset, center);
                        blur += 1.0 - saturate(sampleDist / _Size);
                    }
                }
                blur /= 25.0;
                glow = max(glow, blur * 0.5) * _Intensity;
                fixed4 finalColor = baseColor + (_Color * glow * baseColor.a);
                finalColor.a = baseColor.a + (glow * _Color.a);
                // Debug: In glow ra alpha để kiểm tra
                // finalColor = float4(0, 0, 0, glow); // Bỏ comment để xem glow
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}

// Shader "UI/Glowing" {
// 	Properties {
// 		_Color ("Color", Color) = (1,1,1,1)
// 		_Size ("Atmosphere Size Multiplier", Range(0,16)) = 4
// 		_Rim ("Fade Power", Range(0,8)) = 4
// 	}
// 	SubShader {
// 		Tags { "RenderType"="Transparent" }

//         Stencil
//         {
//             Ref [_Stencil]
//             Comp [_StencilComp]
//             Pass [_StencilOp]
//             ReadMask [_StencilReadMask]
//             WriteMask [_StencilWriteMask]
//         }


// 		LOD 200
//         Cull Off
		
// 		CGPROGRAM
// 		// Physically based Standard lighting model, and enable shadows on all light types
// 		#pragma surface surf Lambert fullforwardshadows alpha:fade
// 		#pragma vertex vert

// 		// Use shader model 3.0 target, to get nicer looking lighting
// 		#pragma target 3.0


// 		struct Input {
// 			float3 viewDir;
// 		};

// 		half _Size;
// 		half _Rim;
// 		fixed4 _Color;

// 		void vert (inout appdata_full v) {
// 			v.vertex.xyz += v.vertex.xyz * _Size / 10;
// 			v.normal *= -1;
// 		}

// 		void surf (Input IN, inout SurfaceOutput o) {
// 			half rim = saturate (dot (normalize (IN.viewDir), normalize (o.Normal)));

// 			// Albedo comes from a texture tinted by color
// 			fixed4 c = _Color;
// 			o.Emission = c.rgb;
// 			o.Alpha = lerp (0, 1, pow (rim, _Rim));
// 		}
// 		ENDCG
// 	}
// 	FallBack "Diffuse"
// }