sampler2D TextureSampler : register(s0);

// Parameters for the fractal
float2 ViewOffset;    // Offset of the camera (center of the view)
float Zoom;           // Zoom level
int MaxIterations;    // Maximum number of iterations
float4 Color1;        // Gradient color 1
float4 Color2;        // Gradient color 2

// Pixel Shader Function
float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR {
    // Map screen coordinates to the fractal plane
    float x0 = (uv.x - 0.5) * Zoom + ViewOffset.x;
    float y0 = (uv.y - 0.5) * Zoom + ViewOffset.y;

    // Complex number: c = x0 + i * y0
    float x = 0.0;
    float y = 0.0;
    int iteration = 0;

    // Mandelbrot escape-time algorithm
    while (x*x + y*y < 4.0 && iteration < MaxIterations) {
        float xtemp = x*x - y*y + x0;
        y = 2.0 * x * y + y0;
        x = xtemp;
        iteration++;
    }

    // Map iteration count to a gradient
    float t = (float)iteration / MaxIterations;
    return lerp(Color1, Color2, t); // Linear interpolation for color
}

// Technique
technique BasicTech {
    pass P0 {
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}
