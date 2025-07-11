Shader "Custom/LayerPixelize"
{
    Properties
    {
        _MainTex      ("Base (RGB)", 2D) = "white" {}
        _PixelSize    ("Pixel Size (px)", Float) = 8
        _FlakeSize    ("Flake Size (px)", Float) = 1
        _FlakeDensity ("Flake Density (0–1)", Range(0,1)) = 0.05
        _Color        ("Color Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _PixelSize;
            float _FlakeSize;
            float _FlakeDensity;
            float4 _Color;

            struct v2f
            {
                float4 pos     : SV_POSITION;
                float2 uv      : TEXCOORD0;
                float4 screenP : TEXCOORD1;
            };

            v2f vert(appdata_full v)
            {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.uv       = v.texcoord.xy;
                o.screenP  = o.pos;
                return o;
            }

            // Простая синус-шумовая функция
            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898,78.233))) * 43758.5453);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Нормализованные экранные координаты [0..1]
                float2 uvScreen = i.screenP.xy / i.screenP.w;
                // Переводим в пиксели
                float2 pixPos = uvScreen * _ScreenParams.xy;

                // Добавляем мелкие «рябинки»
                if (rand(pixPos) < _FlakeDensity)
                {
                    pixPos += (rand(pixPos * 1.37) - 0.5) * _FlakeSize;
                }

                // Квантование крупной сеткой
                pixPos = floor(pixPos / _PixelSize) * _PixelSize;

                // Обратно к UV
                float2 finalUV = pixPos / _ScreenParams.xy;

                // Сэмплим исходную текстуру по экранному UV
                fixed4 col = tex2D(_MainTex, finalUV);
                col *= _Color;
                return col;
            }
            ENDCG
        }

        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            struct v2f {
                float4 pos : SV_POSITION;
            };
            v2f vert(appdata_full v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            float4 frag(v2f i) : SV_Target {
                return 0;
            }
            ENDCG
        }
    }
    FallBack Off
}
