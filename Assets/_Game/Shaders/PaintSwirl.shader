Shader "Custom/PolarCoordinatesEffect"
{
    Properties
    {
        _PolarCenter ("Polar Center", Vector) = (0.5, 0.5, 0, 0)
        _PolarZoom ("Polar Zoom", Float) = 1.0
        _PolarRepeat ("Polar Repeat", Float) = 1.0
        _SpinRotation ("Spin Rotation", Float) = 0.0
        _SpinSpeed ("Spin Speed", Float) = 1.0
        _Offset ("Offset", Vector) = (0.0, 0.0, 0, 0)
        _Colour1 ("Color 1", Color) = (1, 0, 0, 1)
        _Colour2 ("Color 2", Color) = (0, 1, 0, 1)
        _Colour3 ("Color 3", Color) = (0, 0, 1, 1)
        _Contrast ("Contrast", Float) = 2.0
        _SpinAmount ("Spin Amount", Float) = 0.36
        _PixelFilter ("Pixel Filter", Float) = 700.0
        _PolarCoordinates ("Use Polar Coordinates", Float) = 0.0
    }
    
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            static const float PI = 3.14159265f;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            // Shader properties
            float4 _PolarCenter;
            float _PolarZoom;
            float _PolarRepeat;
            float _SpinRotation;
            float _SpinSpeed;
            float4 _Offset;
            float4 _Colour1;
            float4 _Colour2;
            float4 _Colour3;
            float _Contrast;
            float _SpinAmount;
            float _PixelFilter;
            float _PolarCoordinates;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 effect(float2 screenSize, float2 screenCoords) {

            }

            float2 polar_coords(float2 uv, float2 center, float zoom, float repeat)
            {
                float2 dir = uv - center;
                float radius = length(dir) * 2.0;
                float angle = atan2(dir.y, dir.x) / (PI * 2.0);
                return frac(float2(radius * zoom, angle * repeat));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                
            }
            ENDCG
        }
    }
}
