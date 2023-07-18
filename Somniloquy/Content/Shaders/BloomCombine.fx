sampler BloomSampler : register(s0);
sampler BaseSampler : register(s1);

float BloomIntensity;
float BaseIntensity;
float BloomSaturation;
float BaseSaturation;

float4 AdjustSaturation(float4 color, float saturation) {
    // Grayscale conversion weights: 0.3, 0.59, 0.11
    float grey = dot(color, float3(0.3, 0.59, 0.11));
    
    return lerp(grey, color, saturation);
}

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0 {
    float4 bloom = tex2D(BloomSampler, texCoord);
    float4 base = tex2D(BaseSampler, texCoord);
    
    bloom = AdjustSaturation(bloom, BloomSaturation) * BloomIntensity;
    base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;
    
    return base + bloom;
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
