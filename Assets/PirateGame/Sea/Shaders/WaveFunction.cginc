#ifndef WAVEFUNCTION_INCLUDE
#define WAVEFUNCTION_INCLUDE

#include "HLSLSupport.cginc"

// TODO Considering including this and using _Time instead of requiring time as an argument
//#include "UnityShaderVariables.cginc"

CBUFFER_START(PirateGameWaves)

float WaveAmplitude = 1; 
float WaveDistance = 10;
float WaveSpeed = 5;
float2 WaveDirection = float2(1, 1);
float WavePhaseOffset = 0;

CBUFFER_END

#define pi2 6.28318531

//float pi2 = 3.14 * 2; //radians(360);

void WaveParams_float(out float amplitude, out float distance, out float speed, out float2 direction, out float phaseOffset) {
    amplitude = WaveAmplitude;
    distance = WaveDistance;
    speed = WaveSpeed;
    direction = WaveDirection;
    phaseOffset = WavePhaseOffset;
}

void WaveParams_half(out half amplitude, out half distance, out half speed, out half2 direction, out half phaseOffset) {
    amplitude = WaveAmplitude;
    distance = WaveDistance;
    speed = WaveSpeed;
    direction = WaveDirection;
    phaseOffset = WavePhaseOffset;
}

void WavePosition_float(in float time, in float3 worldPos,
    out float3 newPos) 
{
    // Position p = [x, y, z] = [x, f(t,x,z), z]
    // p_y = f(t,x,z)
    float2 direction = normalize(WaveDirection);
    newPos.y = sin((worldPos.x * direction.x + worldPos.z * direction.y + time * WaveSpeed) / WaveDistance + WavePhaseOffset) * WaveAmplitude;
    newPos.xz = worldPos.xz;
}

void WaveTangents_float(in float time, in float3 worldPos,
    out half3x3 tangents)
{   
    float2 direction = normalize(WaveDirection);

    // Tangent U = [ ?p_x, ?p_y, ?p_z ]
    
    // Tangent ?p_x = [?x, ?y, 0] = [?x, ?f/?x ?x, 0] = ?x[1, ?f_x(t,x,z), 0]
    tangents[0] = float3(1, cos((worldPos.x * direction.x + worldPos.z * direction.y + time * WaveSpeed) / WaveDistance + WavePhaseOffset) * WaveAmplitude, 0);
    
    // Tangent ?p_y = [0, 0, 0]
    tangents[1] = float3(0, 1, 0);

    // Tangent ?p_z = [0, ?y, ?z] = [0, ?f/?z ?z, ?z] = ?z[0, ?f_z(t,x,z), 1]
    tangents[2] = float3(0, cos((worldPos.x * direction.x + worldPos.z * direction.y + time * WaveSpeed) / WaveDistance + WavePhaseOffset) * WaveAmplitude, 1);
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
    normal = normalize(normal);
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