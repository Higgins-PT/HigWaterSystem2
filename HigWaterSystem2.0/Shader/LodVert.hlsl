
#ifndef LODVERT
#define LODVERT

void SnapToGrid(inout float3 pos, float cellSize)
{
    float2 snappingPos = round(pos.xz / cellSize) * cellSize;
    pos.xz = snappingPos;

}
float GetSmoothT(float2 offestPos, float gridSize, float gridMinSize)
{
    float length = max(abs(offestPos.x), abs(offestPos.y));
    float t = (length - gridMinSize) / (gridSize - gridMinSize);
    return clamp(t, 0, 1);
}

void LodSmooth(inout float3 pos, float3 camPos, float gridSize, float gridMinSize, float cellSize)
{
    float2 snappingPos = round(pos.xz / cellSize) * cellSize;
    pos.xz = lerp(pos.xz, snappingPos, GetSmoothT(pos.xz - camPos.xz, gridSize, gridMinSize));

}


#endif
