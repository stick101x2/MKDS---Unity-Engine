// Shadow Volumes Toolkit
// Copyright 2013 Gustav Olsson
Shader "Shadow Volumes/Alpha Volume Back"
{
	SubShader
	{
		Tags
		{
			"Queue" = "AlphaTest+507"
			"IgnoreProjector" = "True"
		}
		
		Pass
		{
			Lighting Off
			Cull Front
			ZTest LEqual
			ZWrite Off
			Blend Zero One, One One
			BlendOp RevSub
			
			Fog
			{
				Mode Off
			}
			
			CGPROGRAM
			#pragma vertex ShadowVolumeVertex
			#pragma fragment ShadowVolumeFragment
			#include "../Includes/ShadowVolume.cginc"
			ENDCG
		}
	}
}