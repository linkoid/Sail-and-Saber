
void GetPosition_float(in float time, in float3 worldPos,
    out float3 newPos) 
{
    newPos.xz = worldPos.xz;
    newPos.y = sin(worldPos.x + worldPos.z + time);
}

void GetPosition_half(in half time, in half3 worldPos,
    out half3 newPos)
{
    float3 newPos_float;
    GetPosition_float((float)time, (float3)worldPos, newPos_float);

    newPos.xyz = newPos_float.xyz;
}