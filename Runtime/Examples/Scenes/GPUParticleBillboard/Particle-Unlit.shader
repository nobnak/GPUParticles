Shader "Unlit/Particle-Unlit" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Size ("Size", float) = 1
    }
    SubShader {
        Tags { "RenderType"="Opaque" }

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
            };

            #include "../../../Scripts/Data/Particle.cs.hlsl"
            StructuredBuffer<Particle> _Particles;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Size;

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
                
                Particle p = _Particles[v[0].instanceID];
                if (p.activity <= 0) {
					return;
				}

                float3 center_wc = mul(unity_ObjectToWorld, float4(p.position, 1)).xyz;

                for (int i = 0; i < 4; i++) {
                    float3 quadOffset_wc = mul(unity_CameraToWorld, quad[i]).xyz;
                    float3 pos_wc = center_wc + (0.5 * _Size) * quadOffset_wc;
                    
					v2f o;
					o.vertex = mul(UNITY_MATRIX_VP, float4(pos_wc, 1));
					o.uv = TRANSFORM_TEX(quad_uv[i], _MainTex);
					stream.Append(o);
				}

                stream.RestartStrip();
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
