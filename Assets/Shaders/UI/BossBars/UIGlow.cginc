#ifndef UI_GLOW_INCLUDED
#define UI_GLOW_INCLUDED

#include "UnityCG.cginc"
//////////////////
// Math Functions
float remap(float s, float a1, float a2, float b1, float b2)
{
    return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
}

////////////////////

#if defined(GLOW_PULSE_OVER_TIME)
#endif

// Glow Params
sampler2D _GlowRamp;
float4 _GlowRamp_ST;

float _GlowMultiplier;
float _GlowWidth;
float4 _GlowParams; // Glow XSpeed (X) XOffset (Y)

float CalculateGlowPositionX(float4 texTileOffset)
{
    // Glow position
    half totalSeconds = 1 / (_GlowParams.x);
    half offset = _GlowParams.y;
    half delay = _GlowParams.z;
    half width = _GlowWidth;

    half currentTime = _Time.y + delay;
    half glowPosition = frac((currentTime * texTileOffset) / totalSeconds) * texTileOffset.x + offset;
    
    // Remap glow position (0, 1) to (-w, 1+w) to account for width
   return remap(glowPosition, 0, 1, -width, 1.0 + width);
}

half4 IlluminateSection(float position, float glowPosition, float width)
{
    // Color ramp
    fixed min = glowPosition - width;
    fixed max = glowPosition + width;

    fixed2 rampUV = fixed2(remap(position, min, max, 0, 1), 0);
    fixed4 rampColor = tex2D(_GlowRamp, rampUV * _GlowRamp_ST.xy);
    float ramp = rampColor.r;

    // base glow 
    float glowValue = 1.0;

    // Within 0.0 and 1.0
    if (position >= min && position <= max)
    {
        glowValue = _GlowMultiplier;
    }

    // Width >= 1.0
    if (max > 1.0)
    {
        if (position >= min || position <= max - 1.0)
            glowValue = _GlowMultiplier;
    }

    // Width <= 0.0
    if (min < 0)
    {
        if (position >= min + 1.0 || position <= max)
            glowValue = _GlowMultiplier;
    }

    return ramp * _GlowMultiplier;
}

#endif
