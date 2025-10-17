#include <Packages/com.blendernodesgraph.core/Editor/Includes/Importers.hlsl>

void CardFrame_float(float3 _POS, float3 _PVS, float3 _PWS, float3 _NOS, float3 _NVS, float3 _NWS, float3 _NTS, float3 _TWS, float3 _BTWS, float3 _UV, float3 _SP, float3 _VVS, float3 _VWS, float4 Color, Texture2D gradient_28898, Texture2D gradient_28910, Texture2D image_28912, out float3 Surface)
{
	
	float _SeparateHSV_28904_h = separate_h(float4(0.05268828, 0.07513415, 0.09973402, 1));
	float _SeparateHSV_28904_s = separate_s(float4(0.05268828, 0.07513415, 0.09973402, 1));
	float _SeparateHSV_28904_v = separate_v(float4(0.05268828, 0.07513415, 0.09973402, 1));
	float4 _ColorRamp_28898 = color_ramp(gradient_28898, _SeparateHSV_28904_v);
	float _SeparateHSV_28906_v = separate_v(_ColorRamp_28898);
	float4 _CombineHSV_28902 = combine_hsv(_SeparateHSV_28904_h, _SeparateHSV_28904_s, _SeparateHSV_28906_v);
	float4 _ImageTexture_28912 = node_image_texture(image_28912, _UV, 0);
	float4 _ColorRamp_28910 = color_ramp(gradient_28910, _ImageTexture_28912);
	float4 _MixRGB_28896 = mix_light(0.5, _CombineHSV_28902, _ColorRamp_28910);

	Surface = _MixRGB_28896;
}