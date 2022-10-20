#ifndef WAVEFUNCTION_INCLUDE
#define WAVEFUNCTION_INCLUDE

static float WaveAmplitude = 0.1;

void WavePosition_float(in float time, in float3 worldPos,
    out float3 newPos) 
{
    // Position p = [x, y, z] = [x, f(t,x,z), z]
    // p_y = f(t,x,z)
    newPos.y = sin(worldPos.x + worldPos.z + time) * WaveAmplitude;
    newPos.xz = worldPos.xz;
}

void WaveTangents_float(in float time, in float3 worldPos,
    out half3x3 tangents)
{   
    // Tangent U = [ ?p_x, ?p_y, ?p_z ]
    
    // Tangent ?p_x = [?x, ?y, 0] = [?x, ?f/?x ?x, 0] = ?x[1, ?f_x(t,x,z), 0]
    tangents[0] = float3(1, cos(worldPos.x + worldPos.z + time) * WaveAmplitude, 0);
    
    // Tangent ?p_y = [0, 0, 0]
    tangents[1] = float3(0, 1, 0);

    // Tangent ?p_z = [0, ?y, ?z] = [0, ?f/?z ?z, ?z] = ?z[0, ?f_z(t,x,z), 1]
    tangents[2] = float3(0, cos(worldPos.x + worldPos.z + time) * WaveAmplitude, 1);
}
void WaveTangents_half(in float time, in half3 worldPos,
    out half3x3 tangents) 
{
    WaveTangents_float(time, worldPos, tangents);
}

void WaveNormal_float(in float time, in float3 worldPos,
    out half3 normal)
{
    float3x3 tangents;

    WaveTangents_float(time, worldPos, tangents);

    normal = cross(normalize(tangents[2]), normalize(tangents[0]));
}
void WaveNormal_half(in float time, in half3 worldPos,
    out half3 normal)
{
    WaveNormal_float(time, worldPos, normal);
}



void WavePosition_half(in float time, in half3 worldPos,
    out half3 newPos)
{
    WavePosition_float(time, worldPos, newPos);
}


#endif // WAVEFUNCTION_INCLUDE