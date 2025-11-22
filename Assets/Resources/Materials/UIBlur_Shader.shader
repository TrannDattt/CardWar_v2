Shader "UI/BlurSprite"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Blur ("Blur Amount", Range(0, 8)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "GAUSS_HORIZONTAL"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Blur;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 texel = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);

                float b = _Blur;
                float2 uv = i.uv;

                fixed4 col = 0;
                col += tex2D(_MainTex, uv - float2(texel.x * 2 * b, 0)) * 0.1;
                col += tex2D(_MainTex, uv - float2(texel.x * 1 * b, 0)) * 0.2;
                col += tex2D(_MainTex, uv) * 0.4;
                col += tex2D(_MainTex, uv + float2(texel.x * 1 * b, 0)) * 0.2;
                col += tex2D(_MainTex, uv + float2(texel.x * 2 * b, 0)) * 0.1;

                return col;
            }
            ENDCG
        }

        Pass
        {
            Name "GAUSS_VERTICAL"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Blur;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 texel = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);

                float b = _Blur;
                float2 uv = i.uv;

                fixed4 col = 0;
                col += tex2D(_MainTex, uv - float2(0, texel.y * 2 * b)) * 0.1;
                col += tex2D(_MainTex, uv - float2(0, texel.y * 1 * b)) * 0.2;
                col += tex2D(_MainTex, uv) * 0.4;
                col += tex2D(_MainTex, uv + float2(0, texel.y * 1 * b)) * 0.2;
                col += tex2D(_MainTex, uv + float2(0, texel.y * 2 * b)) * 0.1;

                return col;
            }
            ENDCG
        }
    }
}
