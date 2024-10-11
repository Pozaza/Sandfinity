Shader "Limitless/CustomTextureShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_CustomTex("Texture", 2D) = "white" {}
	}
	HLSLINCLUDE

	 #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
     #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		TEXTURE2D(_MainTex);
	SAMPLER(sampler_MainTex);
	TEXTURE2D(_CustomTex);
	SAMPLER(sampler_CustomTex);

	struct Attributes
	{
		float4 positionOS       : POSITION;
		float2 uv               : TEXCOORD0;
	};

	struct Varyings
	{
		float2 uv        : TEXCOORD0;
		float4 vertex : SV_POSITION;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	Varyings vert(Attributes input)
	{
		Varyings output = (Varyings)0;
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

		VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
		output.vertex = vertexInput.positionCS;
		output.uv = input.uv;

		return output;
	}

	half fade;

	float4 Frag(Varyings i) : SV_Target
	{
		float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,i.uv);
		float4 col2 = SAMPLE_TEXTURE2D(_CustomTex, sampler_CustomTex, i.uv);
		return lerp(col, col2, col2.a*fade);
	}

		ENDHLSL

		SubShader
	{
		Cull Off ZWrite Off ZTest Always

			Pass
		{
			HLSLPROGRAM

				#pragma vertex vert
				#pragma fragment Frag

			ENDHLSL
		}
	}
}