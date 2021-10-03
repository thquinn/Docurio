Shader "thquinn/SkyboxShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 position : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float4 vec : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata vertex) 
            {
                v2f output;
                output.position = UnityObjectToClipPos(vertex.position);
                output.vec = normalize(mul(unity_ObjectToWorld, vertex.position));
                return output;
            }

            float smax(float a, float b, float k)
            {
                return log(exp(k*a)+exp(k*b))/k;
            }
            float sminCubic(float a, float b, float k )
            {
                float h = max( k-abs(a-b), 0.0 )/k;
                return min( a, b ) - h*h*h*k*(1.0/6.0);
            }
            fixed4 minmaxColor(float4 vec, fixed4 col1, fixed4 col2, fixed4 col3, float t) {
                float3 vec1 = normalize(float3(cos(t), sin(t), cos(t) * sin(t)));
                float3 vec2 = normalize(float3(-cos(t * 1.5), sin(t * 1.7), cos(t * 2.1)));
                float3 vec3 = normalize(float3(sin(t * 1.4), sin(t * 1.1), cos(t * 2)));
                float dot1 = dot(vec, vec1) / 2 + .5;
                float dot2 = dot(vec, vec2) / 2 + .5;
                float dot3 = dot(vec, vec3) / 2 + .5;
                float d = sminCubic(dot1, dot2, 2);
                fixed4 col = lerp(col1, col2, d);
                d = smax(d, dot3, 2);
                col = lerp(col, col3, d / 2);
                return col;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = minmaxColor(i.vec,
                                         fixed4(.35, .4, .3, 1),
                                         fixed4(.55, .6, .8, 1),
                                         fixed4(1, 1, 1, 1),
                                         _Time.x + 200);
                float mx = abs((abs(i.vec.x) % .1) - .05);
                float my = abs((abs(i.vec.y) % .1) - .05);
                float mz = abs((abs(i.vec.z) % .1) - .05);
                float d = mx * .1 + my * .1 + mz * .1;
                return col - d;
            }
            ENDCG
        }
    }
}
