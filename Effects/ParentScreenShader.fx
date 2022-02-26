sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 SpotlightCoordinate(float2 coords : TEXCOORD0) : COLOR0
{
    //float2 drawPosition = uScreenPosition + uScreenResolution / 2;
    float2 drawPosition = uTargetPosition - uScreenPosition;
    float size = 120 * uIntensity;
    float2 drawCoordinate = coords * uScreenResolution;
    float distance = length(drawCoordinate - drawPosition);
    if (distance < size)
    {
        return lerp(tex2D(uImage0, coords), float4(1, 1, 1, 1), 1 - distance / size);
    }
    return tex2D(uImage0, coords);
}

float4 Vignette(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    float mult = length(float2(coords.x - 0.5f, coords.y - 0.5f)) / 1.4142135f;
    mult = min(mult * uIntensity * uOpacity, 1);
    return color * (1 - mult);
}

float4 GetOffsetClr(float2 coords, float2 offset)
{
    return tex2D(uImage0, coords + offset) + tex2D(uImage0, coords - offset);
}

float4 FlashCoordinate(float2 coords : TEXCOORD0) : COLOR0
{
    float2 target = uTargetPosition;
    float2 dir = normalize((uScreenPosition + coords * uScreenResolution) - target);
    float2 pixelScale = float2(1 / uScreenResolution.x, 1 / uScreenResolution.y) * 2;
    float2 offsetNormal = dir * pixelScale * uIntensity;
    float4 color = GetOffsetClr(coords, offsetNormal * 11);
    color += GetOffsetClr(coords, offsetNormal * 10);
    color += GetOffsetClr(coords, offsetNormal * 9);
    color += GetOffsetClr(coords, offsetNormal * 8);
    color += GetOffsetClr(coords, offsetNormal * 7);
    color += GetOffsetClr(coords, offsetNormal * 6);
    color += GetOffsetClr(coords, offsetNormal * 5);
    color += GetOffsetClr(coords, offsetNormal * 4);
    color += GetOffsetClr(coords, offsetNormal * 3);
    color += GetOffsetClr(coords, offsetNormal * 2);
    color += GetOffsetClr(coords, offsetNormal);
    float intensity = max(uIntensity / 4.0f, 1 / 22.0f);
    return (color + tex2D(uImage0, coords)) * intensity;
}

technique Technique1
{
    pass FlashCoordinatePass
    {
        PixelShader = compile ps_2_0 FlashCoordinate();
    }
    pass SpotlightCoordinatePass
    {
        PixelShader = compile ps_2_0 SpotlightCoordinate();
    }
    pass VignettePass
    {
        PixelShader = compile ps_2_0 Vignette();
    }
}