// Renders layered objects with no lighting and stencil cutouts


 Shader "FableCine/FlipBook_Add_Particle"   
 {
    Properties 
    {
		_MainTex ("Texture", 2D) = "white" {}
		_cellsX ("Cells X", Float) = 4
		_cellsY ("Cells Y", Float) = 4
		_cellIndex ("Cell Index", Float) = 0
		_padX ("Padding X", Float) = 0
		_padY ("Padding Y", Float) = 0
		_Color ("Tint Color (RGB), Alpha (A)", Color) = (1,1,1,1) 
    }

	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Pass
		{
			Blend SrcAlpha One
			Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			int _cellIndex;
			int _cellsX;
			int _cellsY;
			float _padX;
			float _padY;
			float4 _Color;
			
			struct appdata_t
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 UVbase : TEXCOORD0;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;

				// convert cell # to UV index
				_cellIndex = floor(_cellIndex);
				half _x;
				half _y;
				half _xOff = (1.0 - _padX)/_cellsX;
				half _yOff = (1.0 - _padY)/_cellsY;

				_x = _cellIndex - _cellsX * floor(_cellIndex / _cellsX );
				
				modf(_cellIndex/_cellsX, _y);
				o.UVbase.x = (v.texcoord.x * _xOff) + (_x * _xOff );
				o.UVbase.y = (v.texcoord.y * _yOff) + ((1.0-_yOff) - (_y * _yOff));
				return o;
			}

			half4 frag (v2f IN) : COLOR
			{
				// mix maps
				half4 col = tex2D(_MainTex, IN.UVbase).rgba;
				col.rgba *= _Color;
				col.rgba *= IN.color;
				return col;
			}
			ENDCG
		}
	}
	
}
 
