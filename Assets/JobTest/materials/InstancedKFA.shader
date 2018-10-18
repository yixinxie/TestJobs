Shader "Custom/TestAnimatedBatch" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_AnimTex("Animation Tex", 2D) = "white" {}
		_Amount ("Amount", Range(0,1)) = 0.5
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert addshadow 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _AnimTex;
		 
		struct Input {
			float2 uv_MainTex;
		};
		struct appdata
		{
			float4 vertex        : POSITION;
			float3 normal        : NORMAL;
			float4 tagent			: TANGENT;

			float2 texcoord            : TEXCOORD0;
			float2 texcoord1            : TEXCOORD1;
			float2 texcoord2           : TEXCOORD2;
			uint   id                : SV_VertexID; 
		};

		half _Glossiness;
		half _Metallic;
		half _Amount;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata v, out Input o) {
			//v.vertex.xyz += v.normal * _Amount;
			float2 vuv;
			vuv.x = v.id / 16.0;
			vuv.y = _Time.y;
			v.vertex.xyz += tex2Dlod(_AnimTex, float4(vuv.xy, 0, 0)) * _Amount;
			//v.vertex.xyz += tex2D(_AnimTex, vuv.xy) * _Amount;
			
			o.uv_MainTex = v.texcoord;
			

			
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
