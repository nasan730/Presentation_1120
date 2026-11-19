Shader "UI/Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Dissolve)]
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _NoiseScale ("Noise Scale", Float) = 6
        [Toggle(_DISSOLVE_NOISE_TEX)] _UseNoiseTex ("Use Noise Texture", Float) = 0
        _NoiseTex ("Noise Texture (R)", 2D) = "gray" {}

        [Header(Edge)]
        [HDR] _EdgeColor ("Edge Color", Color) = (4, 1.6, 0.4, 1)
        _EdgeWidth ("Edge Width", Range(0, 0.3)) = 0.06
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.2)) = 0.02

        // ---- 以下 UI/Default と同じ（Mask / RectMask2D 用） ----
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma shader_feature_local _ _DISSOLVE_NOISE_TEX

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0; // _MainTex 用
                float2 noiseUV : TEXCOORD1; // ノイズ用（スプライトの UV そのまま）
                float4 mask : TEXCOORD2; // RectMask2D 用
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            float _Dissolve;
            float _NoiseScale;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            half4 _EdgeColor;
            float _EdgeWidth;
            float _EdgeSoftness;

            // ---------- 手続きノイズ ----------
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float Fbm(float2 p)
            {
                float v = 0.0;
                float amp = 0.5;
                for (int k = 0; k < 4; k++)
                {
                    v += amp * ValueNoise(p);
                    p = p * 2.03 + 17.1;
                    amp *= 0.5;
                }
                v /= 0.9375;
                // fBm は中央(0.5付近)に偏るので 0〜1 に引き伸ばす。
                // これをしないと Dissolve の 0付近 / 1付近で見た目が変化しない区間ができる。
                return saturate((v - 0.2) / 0.55);
            }

            float SampleNoise(float2 uv)
            {
                #ifdef _DISSOLVE_NOISE_TEX
                return tex2D(_NoiseTex, uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw).r;
                #else
                return Fbm(uv * _NoiseScale);
                #endif
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.vertex = vPosition;

                // RectMask2D（ソフトネス対応）
                float2 pixelSize = vPosition.w;
                pixelSize /= abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw,
                         0.25 / (0.25 * float2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.noiseUV = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                // ---------- Dissolve ----------
                float n = SampleNoise(IN.noiseUV);

                // しきい値 t を Dissolve 0→1 で動かす。
                // 0 のとき縁も含めて完全に表示、1 のとき完全に消えるように範囲を広げてある。
                float t = lerp(-(_EdgeWidth + _EdgeSoftness * 2.0), 1.0, _Dissolve);

                // n < t の部分を消す（境目は少しだけぼかす）
                float visible = smoothstep(t, t + _EdgeSoftness, n);
                // 消える境目から _EdgeWidth の範囲を縁として光らせる
                float edge = 1.0 - smoothstep(t + _EdgeWidth, t + _EdgeWidth + _EdgeSoftness, n);

                color.rgb = lerp(color.rgb, _EdgeColor.rgb, edge * _EdgeColor.a);
                color.a *= visible;

                // ---------- UI 標準処理 ----------
                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}