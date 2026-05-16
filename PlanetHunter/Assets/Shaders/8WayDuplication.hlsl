#ifndef LOOP_POSITIONS_INCLUDED
#define LOOP_POSITIONS_INCLUDED

void DuplicateVertices_float(
    float3 positionOS,
    float2 worldWidthHeight,
    out float3 outPosition1, out float3 outPosition2, out float3 outPosition3,
    out float3 outPosition4, out float3 outPosition5, out float3 outPosition6,
    out float3 outPosition7, out float3 outPosition8, out float3 outPosition9)
{
    // Note: this shader was not written by me
    float w = worldWidthHeight.x;
    float h = worldWidthHeight.y;

    // Map out the absolute mathematical layout for all 9 grid locations
    outPosition1 = positionOS; // True Center Player
    outPosition2 = positionOS + float3(-w, 0, 0); // Left
    outPosition3 = positionOS + float3(w, 0, 0); // Right
    outPosition4 = positionOS + float3(0, -h, 0); // Bottom
    outPosition5 = positionOS + float3(0, h, 0); // Top
    outPosition6 = positionOS + float3(-w, -h, 0); // Bottom-Left Corner
    outPosition7 = positionOS + float3(w, -h, 0); // Bottom-Right Corner
    outPosition8 = positionOS + float3(-w, h, 0); // Top-Left Corner
    outPosition9 = positionOS + float3(w, h, 0); // Top-Right Corner
}

#endif
