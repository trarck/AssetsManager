Shader "Custom/lllumin-Diffuse" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_LightColor ("_LightColor", Color) = (0.03, 0.03 ,0.03 ,0.1) 
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Scale("light",Range(0.1,10)) = 0.8
	_Range ("Range", Range(0, 100)) = 2
	_Cutoff("cut",Range(0,1)) =0.01
}



SubShader {
		Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutoff"  }//"LightMode" ="ForwardBase"
		// LOD 200

		Cull Back
		// AlphaTest off
		ColorMask RGB 
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting off
		// ZWrite off

		CGPROGRAM
		#pragma surface surf Custom alphatest:_Cutoff

		sampler2D _MainTex;
		half4 _Color;
		half _Scale;
		half4 _LightColor;
		half _Range;
		// half _Cutoff;


		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			
			half4 c = tex2D(_MainTex, IN.uv_MainTex) ;
			// o.Emission = c.rgb;
			
			o.Albedo = c.rgb *_Scale;
			o.Alpha = c.a *_Color.a;
			float gray = step(0.0000001, dot(_Color, fixed4(1, 1, 1, 0)));
			half3 co = dot(c.rgb, half3(0.299, 0.587, 0.184));			
			o.Albedo = gray * o.Albedo + (1 - gray) * (co * half3(1.0, 1.0, 1.25));
		}

		half4 LightingCustom (SurfaceOutput s, half3 lightDir, half atten)
		{
			half4 outCol =half4( s.Albedo ,s.Alpha);
			float diffuse =  max(0,dot(s.Normal,lightDir ));
			outCol = outCol+ _LightColor* diffuse *atten; 
			outCol *=_Color ;
			return  outCol *2;

		}  

		ENDCG
} 

// 	SubShader
// 	 {
// 		Tags { "RenderType"="Transparent" "Queue"="AlphaTest" "IgnoreProjector"="True" "LightMode"="ForwardBase"  }
// 		Pass {

// 		Cull Back
// 		AlphaTest off
// 		ColorMask RGB
// 		Blend SrcAlpha OneMinusSrcAlpha
// 		Lighting off
// 		// ZWrite Off 
// 		CGPROGRAM

// 		#pragma  vertex vert 
// 		#pragma fragment Frag
// 		#include "UnityCG.cginc" 
// 		#include "Lighting.cginc"

// 		sampler2D _MainTex;
// 		half4  _MainTex_ST;
// 		half4 _Color;
// 		half _Scale;
// 		half4 _LightColor;
// 		half _Range;

// 		struct Input 
// 		{
// 			float4 vertex :SV_POSITION;
// 			half2 uv :TEXCOORD0;
// 			half diffuse :TEXCOORD1;
// 			// float nh :TEXCOORD2;
// 			// float3 normal:TEXCOORD3;
// 		};

// 		Input vert( appdata_base   data)
// 		{
// 			Input IN;
// 			UNITY_INITIALIZE_OUTPUT(Input,IN)
// 			IN.vertex = mul(UNITY_MATRIX_MVP,data.vertex);
// 			IN.uv = TRANSFORM_TEX(data.texcoord,_MainTex);
// 			// IN.normal = mul((float3x3)UNITY_MATRIX_IT_MV,data.normal);

// 			// float3 viewDir  = ObjSpaceViewDir(data.vertex );
// 			// float4 worlpos = mul(_Object2World , data,vertex);
// 			half3 LightDir = ObjSpaceLightDir(data.vertex );//ObjSpaceLightDir(data.vertex );
// 			IN.diffuse =  max(0,dot(data.normal ,LightDir ));

// 			// float h = normalize(viewDir +LightDir);
// 			// IN.nh= max(0,dot(data.normal,h));
// 			return  IN;
// 		}

// 		half4  Frag(Input IN ):SV_Target
// 		{
// 			// float spec = pow(IN.nh,_Range);

// 			half4  outCol =tex2D(_MainTex,IN.uv) ;
// 			// 4  was for higher lighter
// 			outCol = half4 (outCol.rgb * 4 *_Scale,outCol.a) + _LightColor* IN.diffuse ; // + _LightColor *spec;//+_LightColor * spec;
// 			outCol *=_Color ;
// 			return outCol;
// 		}  
// 		ENDCG
// 		}
// }

FallBack "Diffuse"
// CustomEditor "LegacyIlluminShaderGUI"
}
