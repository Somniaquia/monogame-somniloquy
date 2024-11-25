sampler2D TextureSampler : register(s0);

float2 Offset;
float Zoom;
float Rotation;
float Time;
float2 Param;
int MaxIterations;
int FractalType;
bool Julia;

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

float2 ComplexSin(float x, float y) {
    float r = sin(x) * cosh(y);
    float i = cos(x) * sinh(y);

    return float2(r, i);
}

float2 ComplexPower(float x1, float y1, float x2, float y2) {
    float magnitude = sqrt(x1 * x1 + y1 * y1);
    float angle = atan2(y1, x1);

    // Compute logarithm of z1
    float logMag = log(magnitude);  // ln|z1|
    float logReal = logMag;         // Real part of ln(z1)
    float logImag = angle;          // Imaginary part of ln(z1)

    // Multiply z2 by ln(z1)
    float newReal = x2 * logReal - y2 * logImag;
    float newImag = x2 * logImag + y2 * logReal;

    // Exponentiate the result
    float expReal = exp(newReal);
    float resultReal = expReal * cos(newImag);
    float resultImag = expReal * sin(newImag);

    return float2(resultReal, resultImag);
}


int Mandelbrot(float x0, float y0) {
    float x = Param.x;
    float y = Param.y;
    int iter = 0;

    while (x * x + y * y < 4.0 && iter < MaxIterations) {
        float xtemp = x * x - y * y + x0;
        y = 2.0 * x * y + y0;
        x = xtemp;
        iter++;
    }

    return iter;
}

int MandelbrotJulia(float x0, float y0) {
    float x = x0;
    float y = y0;
    int iter = 0;
    
    while (x * x + y * y < 4.0 && iter < MaxIterations) {
        float xtemp = x * x - y * y + Param.x;
        y = 2.0 * x * y + Param.y;
        x = xtemp;
        iter++;
    }

    return iter;
}

int BurningShip(float x0, float y0) {
    float x = Param.x;
    float y = Param.y;
    int iter = 0;
    
    while (x * x + y * y < 4.0 && iter < MaxIterations) {
        float xtemp = x * x - y * y + x0;
        y = abs(2* x* y) + y0;
        x = xtemp;
        iter++;
    }

    return iter;
}

int BurningShipJulia(float x0, float y0) {
    float x = x0;
    float y = y0;
    int iter = 0;
    
    while (x * x + y * y < 4.0 && iter < MaxIterations) {
        float xtemp = x * x - y * y + Param.x;
        y = abs(2* x* y) + Param.y;
        x = xtemp;
        iter++;
    }

    return iter;
}

int SinFractal(float x0, float y0) {
    float x = Param.x;
    float y = Param.y;
    int iter = 0;
    
    while (x * x + y * y < 31.4 && iter < MaxIterations) {
        float2 sin = ComplexSin(x, y);
        y = sin.y + x0;
        x = sin.x + y0;
        iter++;
    }

    return iter;
}

int SinFractalJulia(float x0, float y0) {
    float x = x0;
    float y = y0;
    int iter = 0;
    
    while (x * x + y * y < 31.4 && iter < MaxIterations) {
        float2 sin = ComplexSin(x, y);
        y = sin.y + Param.x;
        x = sin.x + Param.y;
        iter++;
    }

    return iter;
}

int ExponentialFractal(float x0, float y0) {
    float x = Param.x;
    float y = Param.y;
    int iter = 0;
    
    while (x * x + y * y < 50 && iter < MaxIterations) {
        float2 power = ComplexPower(2.71828182845904, 0, x, y);
        y = power.y + x0;
        x = power.x + y0;
        iter++;
    }

    return iter;
}

int ExponentialFractalJulia(float x0, float y0) {
    float x = x0;
    float y = y0;
    int iter = 0;
    
    while (x * x + y * y < 50 && iter < MaxIterations) {
        float2 power = ComplexPower(2.71828182845904, 0, x, y);
        y = power.y + Param.x;
        x = power.x + Param.y;
        iter++;
    }

    return iter;
}

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR {
    float x0 = (uv.x - 0.5) * Zoom;
    float y0 = (uv.y - 0.5) * Zoom;

    // Apply rotation
    float cosAngle = cos(Rotation);
    float sinAngle = sin(Rotation);
    float xRotated = cosAngle * x0 - sinAngle * y0;
    float yRotated = sinAngle * x0 + cosAngle * y0;

    x0 = xRotated + Offset.x;
    y0 = yRotated + Offset.y;

    int type = FractalType % 4;
    int iter = 0;
    
    if (Julia != true) {
        if (type == 0) {
            iter = Mandelbrot(x0, y0);
        } else if (type == 1) {
            iter = BurningShip(x0, y0);
        } else if (type == 2) {
            iter = SinFractal(x0, y0);
        } else if (type == 3) {
            iter = ExponentialFractal(x0, y0);
        } 
    } else {
        if (type == 0) {
            iter = MandelbrotJulia(x0, y0);
        } else if (type == 1) {
            iter = BurningShipJulia(x0, y0);
        } else if (type == 2) {
            iter = SinFractalJulia(x0, y0);
        } else if (type == 3) {
            iter = ExponentialFractalJulia(x0, y0);
        } 
    }
    
    if (iter >= MaxIterations / 2) return float4(1, 1, 1, 1);

    float hue = (float)iter / MaxIterations + Time / 5.0; 
    // return float4(HSVtoRGB(hue, 1, 1), 1);
    return float4(sin(hue * 2) / 2 + 0.5, sin(hue * 3) / 2 + 0.5, sin(hue * 5) / 2 + 0.5, 1) / 2;
}

technique BasicTech {
    pass P0 {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
