

Texture2D textureColor[] : register(t1);
SamplerState samplerColor : register(s2);


struct PushConsts
{
    int i;
};
[[vk::push_constant]] PushConsts index;

struct VSOutput
{
	[[vk::location(0)]] float2 UV : TEXCOORD0;
};

float4 main(VSOutput input) : SV_TARGET
{
    float4 color = textureColor[NonUniformResourceIndex(index.i)].Sample(samplerColor, input.UV);
    //float4 colo2 = textureColor[NonUniformResourceIndex(index.i + 1)].Sample(samplerColor, input.UV);
    //float4 colo3 = textureColor[NonUniformResourceIndex(index.i + 2)].Sample(samplerColor, input.UV);
    //colo3 = colo2 / colo3;
    
    
    return color;
}