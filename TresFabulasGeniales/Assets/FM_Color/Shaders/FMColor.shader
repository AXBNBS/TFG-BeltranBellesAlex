Shader "Hidden/FMColor"
{
	Properties
	{
		//_MainTex("Base", 2D) = "" {}
		//_EffectLut("Lut Effect", Range(0,1)) = 1.0

        //_PixelSize("Pixel Size", Range(1, 512)) = 0
        //_CelCuts("Cel Cuts", Range(2, 255)) = 0

		//_Tint("Tint (RGB)", Color) = (1, 1, 1, 1)      
		//_Hue("Hue", Range(0,360)) = 0
		//_Saturation("Saturation", Range(0,2)) = 1.0
		//_Brightness("Brightness", Range(0,3)) = 1.0
		//_Contrast("Contrast", Range(0,2)) = 1.0
		//_Sharpness("Sharpness", Range(0,1)) = 0

        //_EffectFresnel("Fresnel Effect", Range(0, 1)) = 0
        //_FresnelColor ("Fresnel Color", Color) = (1,1,1,1)
        //_FresnelPower ("Fresnel Power", Range(0.001, 10)) = 1
        
        //_FresnelScanlineX ("FresnelScanlineX", Range(0, 32)) = 0
        //_FresnelScanlineY ("FresnelScanlineY", Range(0, 32)) = 0

        //_EffectFog("Fog Effect", Range(0, 1)) = 0
        //_DepthClippingFar ("Depth Clipping Far", Range(0.001, 1)) = 1
        //_DepthLevel ("Depth Level", Range(1, 5)) = 1
        //_FogColor ("Fog Color", Color) = (1,1,1,1)
        //_ForegroundColor ("Foreground Color", Color) = (1,1,1,1)
        
        //_EffectGrain ("Grain Effect", Range(0, 1)) = 0
        //_GrainSize ("Grain Size", Range(1, 3)) = 0

        //_EffectVignette ("Vignette Effect", Range(0, 1)) = 0
        //_VignetteColor ("Vignette Color", Color) = (0,0,0,1)

        //==================outline===================
        //_OutlineColor ("Outline Color", Color) = (0,0,0,1)
        //_EffectOutline ("Outline Effect", Range(0, 1)) = 0
        //_NormalMult ("Normal Outline Multiplier", Range(0,4)) = 1
        //_NormalBias ("Normal Outline Bias", Range(1,4)) = 1
        //_DepthMult ("Depth Outline Multiplier", Range(0,4)) = 1
        //_DepthBias ("Depth Outline Bias", Range(1,4)) = 1
        //==================outline===================

        //ScanlineColor
        //_ScanlineColor ("Scanline Color", Color) = (0,0,0,1)
        //_EffectScanline ("Scanline Effect", Range(0, 1)) = 0
        //_ScanlineX ("ScanlineX", Range(0, 32)) = 0
        //_ScanlineY ("ScanlineY", Range(0, 32)) = 0
	}

	CGINCLUDE

#include "UnityCG.cginc"
#define IF(a, b, c) lerp(b, c, step((float) (a), 0));

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv  : TEXCOORD0;
	};
    
    uniform sampler2D _MainTex;
    uniform float4 _MainTex_TexelSize;
    
    uniform float _Scale;
    uniform float _Offset;
    
    uniform sampler3D _ClutTex;
    uniform float _EffectLut;
	uniform float4 _TintColor;

	uniform float _Hue;
	uniform float _Saturation;
	uniform float _Brightness;
	uniform float _Contrast;
    uniform float _Sharpness;

    uniform float _PixelSize;
    uniform float _CelCuts;
    
    uniform float _PaintRadius;
    uniform float _PaintBlurLevel;

    uniform float _EffectGrain;
    uniform float _GrainSize;
    
    uniform sampler2D _CameraDepthTexture;
    uniform sampler2D _CameraDepthNormalsTexture;
    uniform float4 _CameraDepthNormalsTexture_TexelSize;

    //==================outline===================
    uniform float4 _OutlineColor;
    uniform float _EffectOutline;
    uniform float _NormalMult;
    uniform float _NormalBias;
    uniform float _DepthMult;
    uniform float _DepthBias;
    
    inline void Compare(inout float depthOutline, inout float normalOutline, float baseDepth, float3 baseNormal, float2 uv, float2 offset)
    {
        //read neighbor pixel
        float4 neighborDepthnormal = tex2D(_CameraDepthNormalsTexture, uv + _CameraDepthNormalsTexture_TexelSize.xy * offset);
        float3 neighborNormal;
        float neighborDepth;
        DecodeDepthNormal(neighborDepthnormal, neighborDepth, neighborNormal);
        neighborDepth = neighborDepth * _ProjectionParams.z;

        float depthDifference = baseDepth - neighborDepth;
        depthOutline = depthOutline + depthDifference;

        float3 normalDifference = baseNormal - neighborNormal;
        normalDifference = normalDifference.r + normalDifference.g + normalDifference.b;
        normalOutline = normalOutline + normalDifference;
    }

    inline float3 EdgeDetection(float3 c, float2 uv)
    {
        //read depthnormal
        float4 depthnormal = tex2D(_CameraDepthNormalsTexture, uv);

        //decode depthnormal
        float3 normal;
        float depth;
        DecodeDepthNormal(depthnormal, depth, normal);

        //get depth as distance from camera in units 
        depth = depth * _ProjectionParams.z;

        float depthDifference = 0;
        float normalDifference = 0;

        Compare(depthDifference, normalDifference, depth, normal, uv, float2(1, 0));
        Compare(depthDifference, normalDifference, depth, normal, uv, float2(0, 1));
        Compare(depthDifference, normalDifference, depth, normal, uv, float2(0, -1));
        Compare(depthDifference, normalDifference, depth, normal, uv, float2(-1, 0));

        depthDifference = depthDifference * _DepthMult;
        depthDifference = saturate(depthDifference);
        depthDifference = pow(depthDifference, _DepthBias);

        normalDifference = normalDifference * _NormalMult;
        normalDifference = saturate(normalDifference);
        normalDifference = pow(normalDifference, _NormalBias);

        float outline = normalDifference + depthDifference;
        return lerp(c, _OutlineColor, outline * _EffectOutline);
    }
    //==================outline===================

    uniform float _EffectFresnel;
    uniform float3 _FresnelColor;
    uniform float _FresnelPower;
    uniform float _FresnelScanlineX;
    uniform float _FresnelScanlineY;

    uniform float _EffectFog;
    uniform float3 _FogColor;
    uniform float3 _ForegroundColor;
    uniform float _DepthLevel;
    uniform float _DepthClippingFar;

    
    uniform float3 _ScanlineColor;
    uniform float _EffectScanline;
    uniform float _ScanlineX;
    uniform float _ScanlineY;

    uniform float _EffectVignette;
    uniform float3 _VignetteColor;
    
    uniform float _DebugSlider;

    inline float GetLuma(float3 c) { return sqrt(dot(c, float3(0.299, 0.587, 0.114))); }
	inline float3 ApplyHue(float3 c, float hue)
	{
		float angle = radians(hue);
		float3 k = float3(0.57735, 0.57735, 0.57735);
		float cosAngle = cos(angle);      
		return c * cosAngle + cross(k, c) * sin(angle) + k * dot(k, c) * (1 - cosAngle);
	}   
    inline float3 ApplyContrast(float3 c) { return (((c - 0.5f) * _Contrast) + 0.5f); }
    inline float3 ApplyBrightness(float3 c) { return c * _Brightness; }
    inline float3 ApplySaturation(float3 c) { return lerp(dot(c, float3(0.299, 0.587, 0.114)), c, _Saturation); }

    inline float2 EdgeUV(float2 uv, float OffX, float OffY) { return uv + float2(_MainTex_TexelSize.x * OffX, _MainTex_TexelSize.y * OffY); }
	inline float3 ApplySharpness(float3 c, float2 uv)
	{
        float3 sum_col = c;
        sum_col += tex2D(_MainTex, EdgeUV(uv, -1,0)).rgb;
        sum_col += tex2D(_MainTex, EdgeUV(uv, 1,0)).rgb;
        sum_col += tex2D(_MainTex, EdgeUV(uv, 0,-1)).rgb;
        sum_col += tex2D(_MainTex, EdgeUV(uv, 0,1)).rgb;
        
        return saturate((c * 2) - (sum_col/5));
	}
    inline float3 ApplyFresnel(float3 c, float3 normals, float2 uv)
    {
        //perspective projection
        float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
        // conver the uvs into view space
        float3 viewDir = -normalize(float3((uv * 2 - 1) / p11_22, -1));
        float fresnel = 1.0 - saturate(dot(viewDir.xyz, normals));
        fresnel = pow(fresnel, _FresnelPower) * _EffectFresnel;
        
        float3 FresnelColor = _FresnelColor;
        if(_FresnelScanlineX == 0 && _FresnelScanlineY == 0) return lerp(c, FresnelColor, fresnel);

        float2 texel = _MainTex_TexelSize.xy;
        texel = IF(_PaintBlurLevel>0, texel/(1+_PaintBlurLevel), texel);

        FresnelColor = IF(uv.x % (texel.x*_FresnelScanlineX*2) < texel.x*_FresnelScanlineX, c, FresnelColor);
        FresnelColor = IF(uv.y % (texel.y*_FresnelScanlineY*2) < texel.y*_FresnelScanlineY, c, FresnelColor);

        return lerp(c, FresnelColor, fresnel);
    }
    inline float3 ApplyScanline(float3 c, float2 uv)
    {
        if(_ScanlineX == 0 && _ScanlineY == 0) return c;
        float3 ScanlineColor = _ScanlineColor;

        float2 texel = _MainTex_TexelSize.xy;
        texel = IF(_PaintBlurLevel>0, texel/(1+_PaintBlurLevel), texel);

        ScanlineColor = IF(uv.x % (texel.x*_ScanlineX*2) < texel.x*_ScanlineX, c, ScanlineColor);
        ScanlineColor = IF(uv.y % (texel.y*_ScanlineY*2) < texel.y*_ScanlineY, c, ScanlineColor);
        return lerp(c, ScanlineColor, _EffectScanline);
    }
    inline float3 ApplyFog(float3 c, float depth)
    {
        depth = saturate(pow(depth, _DepthLevel) / _DepthClippingFar);
        float3 DepthFog = lerp(_ForegroundColor, _FogColor, depth);
        return lerp(c, DepthFog, depth * _EffectFog);
    }
    inline float3 ApplyDepthNormals(float3 c, float2 uv)
    {
        //get depth normal texture
        float4 packedDepthNormals = tex2D(_CameraDepthNormalsTexture, uv);
        float depth;
        float3 normals;
        DecodeDepthNormal(packedDepthNormals, depth, normals);

        c = IF(_EffectFresnel > 0, ApplyFresnel(c, normals, uv), c);
        c = IF(_EffectFog > 0, ApplyFog(c, depth), c);
        return c;
    }

    inline float random (float2 p) { return frac(sin(dot(p.xy, float2(_Time.y % 10 * 0.1, 65.115))) * 2773.8856); }
    inline float3 ApplyGrain(float3 c, float2 uv)
    {
        float Luminance = GetLuma(c);
        float2 normUV = uv; 
        normUV.x *= (_MainTex_TexelSize.z) / _GrainSize;
        normUV.y *= (_MainTex_TexelSize.w) / _GrainSize;

        float2 ipos = floor(normUV);  // get the integer coords
        float rand = random(ipos);
        float3 mv = float3(1,0,0);
        mv = IF(c.g > c.r && c.g > c.b, float3(0,1,0), mv);
        mv = IF(c.b > c.r && c.b > c.g, float3(0,0,1), mv);

        float3 Noise = ApplyHue(mv* c, rand * 360);
        return lerp(c, Noise * (0.05 + 0.2 * (0.5-abs(0.5-Luminance))) + c * 0.9, _EffectGrain);
    }
    inline float3 ApplyVignette(float3 c, float2 uv)
    {
        float2 coord = (uv - 0.5) * 2;
        float rf = sqrt(dot(coord, coord)) * _EffectVignette;
        float rf2_1 = rf * rf + 1.0;
        float e = 1.0 / (rf2_1 * rf2_1);
        return lerp(c, _VignetteColor, (1-e));
    }

    inline float3 ApplyEffects(float3 c, float2 uv)
    {
        //Cel Shading
        c = IF(_CelCuts < 255, normalize(c) * ceil(GetLuma(c) * _CelCuts) * (1/_CelCuts), c);

        c = IF(_Hue > 0, ApplyHue(c, _Hue), c);
        c = IF(_Contrast != 1, ApplyContrast(c), c);
        c = IF(_Brightness != 1, ApplyBrightness(c), c);

        //Lut
        c = IF(_EffectLut > 0, lerp(c, tex3D(_ClutTex, c * _Scale + _Offset).rgb, _EffectLut), c);
        c *= _TintColor;

        c = IF(_EffectFog > 0 || _EffectFresnel > 0, ApplyDepthNormals(c, uv), c);

        c = IF(_EffectScanline > 0, ApplyScanline(c, uv), c);
        
        c = IF(_EffectGrain > 0, ApplyGrain(c, uv), c);
        c = IF(_EffectVignette > 0, ApplyVignette(c, uv), c);
        c = IF(_Saturation != 1, ApplySaturation(c), c);
        return c;
    }
    
	v2f vert(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;      
		return o;
	}

    inline float3 ApplyPixelate(float3 c, float2 uv)
    {
        float2 PS = (_PixelSize / _ScreenParams.xy);
        return IF(( ((uv.x+PS.x*0.5) % PS.x < PS.x *0.1) || ((uv.y+PS.y*0.5) % PS.y < PS.y * 0.1) ), normalize(c) * GetLuma(c), c);
    }

    inline float3 ApplyPaintEffect(float2 uv)
    {
        float3 mean[4];
        float3 sigma[4];
        mean[0]=mean[1]=mean[2]=mean[3]=sigma[0]=sigma[1]=sigma[2]=sigma[3]=float3(0,0,0);      
 
        float2 start[4] = {{-_PaintRadius, -_PaintRadius}, {-_PaintRadius, 0}, {0, -_PaintRadius}, {0, 0}};
        float2 pos;
        float3 col;
        for (int k = 0; k < 4; k++) 
        {
            for(int i = 0; i <= _PaintRadius; i++) 
            {
                for(int j = 0; j <= _PaintRadius; j++) 
                {
                    pos = float2(i, j) + start[k];
                    col = tex2D(_MainTex, float4(uv + float2(pos.x * _MainTex_TexelSize.x, pos.y * _MainTex_TexelSize.y), 0, 0)).rgb;
                    mean[k] += col;
                    sigma[k] += col * col;
                }
            }
        }
 
        float sigma2;
        float n = pow(_PaintRadius + 1, 2);
        float3 c = float3(1,1,1);
        float min = 1;
 
        for (int l = 0; l < 4; l++) 
        {
            mean[l] /= n;
            sigma[l] = abs(sigma[l] / n - mean[l] * mean[l]);
            sigma2 = sigma[l].r + sigma[l].g + sigma[l].b;
 
            //if (sigma2 < min) 
            //{
                //min = sigma2;
                //c = mean[l];
            //}
            
            c = IF(sigma2 < min, mean[l], c);
            min = IF(sigma2 < min, sigma2, min);
        }
        //if(GetLuma(c) < GetLuma(tex2D(_MainTex, uv))) c *=0.8 * (GetLuma(tex2D(_MainTex, uv)) * 0.5 + 0.5);
        //if((GetLuma(c) - GetLuma(tex2D(_MainTex, uv))) * (GetLuma(c) - GetLuma(tex2D(_MainTex, uv))) > 0.0001) c*=0.5;

        return c;
    }

    inline float3 ApplyDebug(float3 c, float x)
    {
        return IF(x > _DebugSlider - (_MainTex_TexelSize.x*10 / (1+_PaintBlurLevel)), float3(0.9,0.9,0.9), c);
    }

	float4 frag(v2f i) : SV_Target
	{
        float2 uv = IF(_PixelSize > 1, round(i.uv/(_PixelSize / _ScreenParams.xy)) * _PixelSize / _ScreenParams.xy, i.uv);
        float4 c = tex2D(_MainTex, uv);

		c.rgb = IF(_Sharpness > 0, lerp(c.rgb, ApplySharpness(c.rgb, uv), _Sharpness), c.rgb);    
		
        c.rgb = ApplyEffects(c.rgb, uv);

        c.rgb = IF(_EffectOutline > 0 && _PixelSize == 1, EdgeDetection(c.rgb, uv), c.rgb);
        c.rgb = IF(_PixelSize>=6, ApplyPixelate(c.rgb, i.uv), c.rgb);

        c.rgb = IF(i.uv.x < _DebugSlider, ApplyDebug(tex2D(_MainTex, i.uv).rgb, i.uv.x), c);
        return c;
	}

	float4 fragLinear(v2f i) : SV_Target
	{
        float2 uv = IF(_PixelSize > 1, round(i.uv/(_PixelSize / _ScreenParams.xy)) * _PixelSize / _ScreenParams.xy, i.uv);
        float4 c = tex2D(_MainTex, uv);

        c.rgb = IF(_Sharpness > 0, lerp(c.rgb, ApplySharpness(c.rgb, uv), _Sharpness), c.rgb);

        c.rgb = sqrt(c.rgb);
        c.rgb = ApplyEffects(c.rgb, uv);
        c.rgb *= c.rgb;

        c.rgb = IF(_EffectOutline > 0 && _PixelSize == 1, EdgeDetection(c.rgb, uv), c.rgb);
        c.rgb = IF(_PixelSize >= 6, ApplyPixelate(c.rgb, i.uv), c.rgb);

        c.rgb = IF(i.uv.x < _DebugSlider, ApplyDebug(tex2D(_MainTex, i.uv).rgb, i.uv.x), c);
        return c;
	}

    float4 fragPaint(v2f i) : SV_Target
    {
        float2 uv = IF(_PixelSize > 1, round(i.uv/(_PixelSize / _ScreenParams.xy)) * _PixelSize / _ScreenParams.xy, i.uv);
        float4 c = float4(ApplyPaintEffect(uv),1);

        c.rgb = IF(_Sharpness > 0, lerp(c.rgb, ApplySharpness(c.rgb, uv), _Sharpness), c.rgb);    
        
        c.rgb = ApplyEffects(c.rgb, uv);

        c.rgb = IF(_EffectOutline > 0 && _PixelSize == 1, EdgeDetection(c.rgb, uv), c.rgb);
        c.rgb = IF(_PixelSize>=6, ApplyPixelate(c.rgb, i.uv), c.rgb);

        c.rgb = IF(i.uv.x < _DebugSlider, ApplyDebug(tex2D(_MainTex, i.uv).rgb, i.uv.x), c);
        return c;
    }

    float4 fragLinearPaint(v2f i) : SV_Target
    {
        float2 uv = IF(_PixelSize > 1, round(i.uv/(_PixelSize / _ScreenParams.xy)) * _PixelSize / _ScreenParams.xy, i.uv);
        float4 c = float4(ApplyPaintEffect(uv),1);

        c.rgb = IF(_Sharpness > 0, lerp(c.rgb, ApplySharpness(c.rgb, uv), _Sharpness), c.rgb);

        c.rgb = sqrt(c.rgb);
        c.rgb = ApplyEffects(c.rgb, uv);
        c.rgb *= c.rgb;

        c.rgb = IF(_EffectOutline > 0 && _PixelSize == 1, EdgeDetection(c.rgb, uv), c.rgb);
        c.rgb = IF(_PixelSize >= 6, ApplyPixelate(c.rgb, i.uv), c.rgb);

        c.rgb = IF(i.uv.x < _DebugSlider, ApplyDebug(tex2D(_MainTex, i.uv).rgb, i.uv.x), c);
        return c;
    }

	ENDCG

	Subshader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
			ENDCG
		}

		Pass
		{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vert
#pragma fragment fragLinear
#pragma target 3.0
			ENDCG
		}

        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
#pragma vertex vert
#pragma fragment fragPaint
#pragma target 3.0
            ENDCG
        }

        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
#pragma vertex vert
#pragma fragment fragLinearPaint
#pragma target 3.0
            ENDCG
        }
	}

	Fallback off
}
