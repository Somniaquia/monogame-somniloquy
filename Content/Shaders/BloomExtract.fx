sampler TextureSampler : register(s0);

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0 {
    float4 color = tex2D(TextureSampler, texCoord);

    float brightness = (color.r + color.g + color.b) / 3.0;

    return color * brightness;
}

technique Technique1 {
    pass Pass1 {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}