#ifndef CF_SCREENSPACE_HIGHLIGHT_INCLUDED
#define CF_SCREENSPACE_HIGHLIGHT_INCLUDED

inline void ComputeHighlight_float(float4 clipPos,
                                   float2 screenSize,
                                   float center,
                                   float width,
                                   float falloff,
                                   float intensity,
                                   out float t)
{
    float2 uv = (clipPos.xy / max(1e-6f, clipPos.w)) * 0.5f + 0.5f;
    float dx = abs((uv.x - center) * screenSize.x);

    float halfW = max(width * 0.5f, 0.0f);
    float falloffCorrected = max(falloff, 1e-4f);

    t = 1.0 - smoothstep(halfW, halfW + falloffCorrected, dx);
    t *= saturate(intensity);
    t = saturate(t);
}

#endif