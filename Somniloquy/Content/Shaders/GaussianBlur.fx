sampler TextureSampler : register(s0);

#define SAMPLE_COUNT 11

float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0 {
    float4 color = float4(0.0f, 0.0f, 0.0f, 0.0f);
    
    // Apply the Gaussian blur
    for (int i = 0; i < SAMPLE_COUNT; i++) {
        color += tex2D(TextureSampler, texCoord + SampleOffsets[i]) * SampleWeights[i];
    }
    
    return color;
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
