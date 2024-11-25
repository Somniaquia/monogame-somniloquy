sampler2D TextureSampler : register(s0);

// Parameters for the fractal
float2 Offset;    // Offset of the camera (center of the view)
float Zoom;           // Zoom level
int MaxIterations;    // Maximum number of iterations

float3 HSVtoRGB(float h, float s, float v) {
    float c = v * s;                     // Chroma
    float x = c * (1 - abs(fmod(h * 6.0, 2.0) - 1)); // Intermediate value
    float m = v - c;

    float3 rgb;
    if (h < 1.0 / 6.0) rgb = float3(c, x, 0);
    else if (h < 2.0 / 6.0) rgb = float3(x, c, 0);
    else if (h < 3.0 / 6.0) rgb = float3(0, c, x);
    else if (h < 4.0 / 6.0) rgb = float3(0, x, c);
    else if (h < 5.0 / 6.0) rgb = float3(x, 0, c);
    else rgb = float3(c, 0, x);

    return rgb + m; // Add m to shift the RGB range to [0, 1]
}

// Pixel Shader Function
float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR {
    // Map screen coordinates to the fractal plane
    float x0 = (uv.x - 0.5) * Zoom + Offset.x;
    float y0 = (uv.y - 0.5) * Zoom + Offset.y;

    // Complex number: c = x0 + i * y0
    float x = 0.0;
    float y = 0.0;
    int iter = 0;

    while (x*x + y*y < 4.0 && iter < MaxIterations) {
        float xtemp = x*x - y*y + x0;
        y = 2.0 * x * y + y0;
        x = xtemp;
        iter++;
    }
    if (iter >= MaxIterations / 2) return float4(1, 1, 1, 1);

    float hue = (float)iter / MaxIterations + 0.25; 
    return float4(HSVtoRGB(hue, 0.5, 1), 1);
}

// Technique
technique BasicTech {
    pass P0 {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
