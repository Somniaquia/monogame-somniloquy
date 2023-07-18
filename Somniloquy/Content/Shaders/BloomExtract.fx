sampler TextureSampler : register(s0);

float BrightnessThreshold;

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0 {
    float4 color = tex2D(TextureSampler, texCoord);

    if (color.r < BrightnessThreshold)
        color.rgb = 0;
        
    return color;
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}