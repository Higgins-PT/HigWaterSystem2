// MyShaderFunctions.cginc
#ifndef MY_SHADER_FUNCTIONS_INCLUDED
#define MY_SHADER_FUNCTIONS_INCLUDED
float2 CompressComplexNumbers(float2 complexA, float2 complexB)
{
    float realPart = complexA.x - complexB.y; // a_r - b_i
    float imagPart = complexA.y + complexB.x; // a_i + b_r
    
    return float2(realPart, imagPart);
}

float SafeTanh(float x)
{


    return tanh(min(x, 20));

}
float GetR(float omega, float omegaP)
{
    float sigema;
    if (omega <= omegaP)
    {
        sigema = 0.07;

    }
    else
    {
        sigema = 0.09;  

    }
    return exp(-pow(omega - omegaP, 2) / (2 * sigema * sigema * omegaP * omegaP));

}


float JonsSpectrum(float omega, float g, float alpha, float omegaP, float gamma, float scale)
{
    float divOmega = 1 / omega;
    float value_1 = (alpha * g * g) * divOmega * divOmega * divOmega * divOmega * divOmega;
    float divValue = omegaP / omega;
    float value_2 = exp(-1.25 * divValue * divValue * divValue * divValue);
    
    float value_3 = pow(abs(gamma), GetR(omega, omegaP));
    return scale * value_2 * value_1 * value_3;
}
float TMPSpectrum(float omega, float g, float alpha, float depth, float omegaP, float gamma, float scale)
{
    float omegaH = omega * sqrt(depth / g);
    float jonsValue = JonsSpectrum(omega, g, alpha, omegaP, gamma, scale);
    float fai;
    if (omegaH <= 1)
    {
        fai = 0.5 * omegaH * omegaH;

    }
    else if (omegaH < 2)
    {
        float value = 2 - omegaH;
        fai = 1 - (0.5 * value * value);
    }
    else
    {
        fai = 1;

    }
    return jonsValue * fai;

}

float SpreadPower(float omega, float omegaP, float windspeed, float g)
{
    if (omega > omegaP)
        return 9.77 * pow(abs(omega / omegaP), -2.33 - 1.45 * ((windspeed * omegaP / g) - 1.17));
    else
        return 6.97 * pow(abs(omega / omegaP), 4.06);
}

float NormalisationFactor(float s)
{
    float a6 = -4.144e-06;
    float a5 = 1.536e-04;
    float a4 = -2.202e-03;
    float a3 = 1.557e-02;
    float a2 = -6.079e-02;
    float a1 = 2.048e-01;
    float a0 = 1.618e-01;

    float s1 = s;
    float s2 = s1 * s1;
    float s3 = s2 * s1;
    float s4 = s3 * s1;
    float s5 = s4 * s1;
    float s6 = s5 * s1;

    return a6 * s6 + a5 * s5 + a4 * s4 + a3 * s3 + a2 * s2 + a1 * s1 + a0;
}
float Cosine2s(float theta, float s)
{
    return NormalisationFactor(s) * pow(abs(cos(0.5 * theta)), 2 * s);
}
float Direction(float omega, float theta, float omegaP, float SwellStrength, float windspeed, float g,float dir)
{
    float s = SpreadPower(omega, omegaP, windspeed, g) + 16 * SafeTanh(omega / omegaP) * SwellStrength * SwellStrength;
    float value_1 = Cosine2s(theta - dir, s);
    return value_1;
}

float DispersionDerivative(float k, float g, float h)
{
    float shallowWaterThreshold = 1.0 / 20.0;
    float deepWaterThreshold = h * 2.0 * 3.141592653;

    if (k * h < shallowWaterThreshold)
    {

        return 0;
    }
    else if (k * h > 80.0)
    {

        return 0.5 * sqrt(g / k);
    }
    else
    {
        float tanh_kh = SafeTanh(k * h);
        float cosh_kh = cosh(k * h);
        float sech_kh_squared = 1.0 / (cosh_kh * cosh_kh);
        return (g * tanh_kh + g * k * h * sech_kh_squared) / (2.0 * sqrt(g * k * tanh_kh));
    }
}

float Frequency(float k, float g, float h)
{
    float shallowWaterThreshold = 1.0 / 20.0;
    float deepWaterThreshold = h * 2.0 * 3.14159;
    if (k * h < shallowWaterThreshold)
    {
        return sqrt(g * h);
    }
    else if (k * h > 80.0)
    {
        return sqrt(g * k);
    }
    else
    {
        return sqrt(g * k * SafeTanh(k * h));
    }
}
float GetAlpha(float windSpeed,float g,float fetch)
{
    float value = (windSpeed * windSpeed) / (g * fetch);
    
    return 0.076 * pow(abs(value), 0.22);


}
float GetOmegaP(float windSpeed, float g, float fetch)
{
    float value = (g * g) / (windSpeed * fetch);
    return 22 * pow(abs(value), 0.33);
}

float2 hash(float2 seed,float time)
{
    float2 randomSeed = seed; 
    randomSeed = frac(sin(dot(randomSeed, float2(12.9898, 78.233))) * 43758.5453 + time);
    return randomSeed;
}


float2 GenerateGaussianRandom(float2 uv,float seed_2)
{
    float2 randomSeed_1 = hash(uv, seed_2);
    if (randomSeed_1.y == 0)
    {
        randomSeed_1.y = 0.1;

    }
    float r = sqrt(-2.0 * log(randomSeed_1.y));
    float phi = 2.0 * 3.14159265359 * randomSeed_1.x;
    return r * float2(sin(phi), cos(phi));
}
float2 ComplexMul(float2 a, float2 b)
{

    float realPart = a.x * b.x - a.y * b.y;
    float imaginaryPart = a.x * b.y + a.y * b.x;
    return float2(realPart, imaginaryPart);
}
float2 ComplexExp(float2 k)
{
    return float2(cos(k.y), sin(k.y)) * exp(k.x);
}
#endif
