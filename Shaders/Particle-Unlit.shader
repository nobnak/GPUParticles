Shader "Unlit/Particle-Unlit" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorTex ("Color Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Size ("Size", float) = 1
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend One One

        Pass {
            CGPROGRAM
            #pragma target 4.0
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                uint instanceID : SV_InstanceID;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            #include_with_pragmas "../ShaderLibrary/RenderCommon.hlsl"
            #define FADE 0.1

            sampler2D _MainTex;
            sampler2D _ColorTex;

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            float _Size;
            CBUFFER_END

            appdata vert(appdata v) {
                return v;
            }

            static const float4 quad[4] = {
				float4(-1,  1, 0, 0),
				float4( 1,  1, 0, 0),
				float4(-1, -1, 0, 0),
				float4( 1, -1, 0, 0)
			};
            static const float2 quad_uv[4] = {
                float2(0, 1),
                float2(1, 1),
                float2(0, 0),
                float2(1, 0)
			};

            [maxvertexcount(4)]
            void geom (point appdata v[1], inout TriangleStream<v2f> stream) {
                uint instanceID = v[0].instanceID;
                if (instanceID >= _ParticlesCount) {
                    return;
                }

                Particle p = _Particles[instanceID];
                if (p.activity <= 0) {
					return;
				}

                float3 center_wc = mul(unity_ObjectToWorld, float4(p.position, 1)).xyz;
                float size = p.size * _Size;

                float tot = p.lifetime;
                float rem = p.duration;
                float4 color = smoothstep(0, FADE, rem) 
                    * smoothstep(tot, tot - FADE, rem)
                    * p.color
                    * tex2Dlod(_ColorTex, float4(p.uvw, 0));

                for (int i = 0; i < 4; i++) {
                    float3 quadOffset_wc = mul(unity_CameraToWorld, quad[i]).xyz;
                    float3 pos_wc = center_wc + (0.5 * size) * quadOffset_wc;
                    
					v2f o;
					o.vertex = mul(UNITY_MATRIX_VP, float4(pos_wc, 1));
					o.uv = TRANSFORM_TEX(quad_uv[i], _MainTex);
                    o.color = color;
					stream.Append(o);
				}

                stream.RestartStrip();
            }

            float4 frag (v2f i) : SV_Target {
                float4 cmain = tex2D(_MainTex, i.uv);
                float4 cout = cmain * i.color * _Color;
                return cout;
            }
            ENDCG
        }
    }
}
