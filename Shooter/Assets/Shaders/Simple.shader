Shader "Custom/Simple"
{
	Properties
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex ("Main texture", 2D) = "white"{}
		//_BumpMap ("Normal map", 2D) = "bump"{}
		_CubeMap ("Cube map", CUBE) = ""{}
	}

	SubShader
	{
		Tags {"ShaderRender" = "Opaque"}

		CGPROGRAM
		#pragma surface surf Lambert
		struct Input
		{
			float2 uv_MainTex;
			float3 worldRefl;
			//INTERNAL_DATA
		};

		float4 _Color;
		sampler2D _MainTex;
		//sampler2D _BumpMap;
		samplerCUBE _CubeMap;

		void surf (Input input, inout SurfaceOutput output)
		{
			//tex2D(_MainTex, input.uv_MainTex).rgb
			output.Albedo =  lerp(_Color, texCUBE(_CubeMap, input.worldRefl), 0.5f);
			//output.Normal = UnpackNormal(tex2D(_BumpMap, input.uv_MainTex));

			//output.Albedo = texCUBE(_CubeMap, WorldReflectionVector (input, output.Normal));
			//output.Albedo = texCUBE(_CubeMap, input.worldRefl);
		}
		ENDCG
	}

	Fallback "Diffuse"
}
