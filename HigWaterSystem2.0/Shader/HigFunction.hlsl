
#ifndef HIGFUNCTION
#define HIGFUNCTION

void Bump(float strength,
          float dist,
          float height,
          float3 N,
          float2 height_xy,
          float3 worldPos,
          float invert,
          out float3 result)
{

    N = normalize(N);
    dist *= invert;

    float3 dPdx = ddx(float3(worldPos));
    float3 dPdy = ddy(float3(worldPos));
    

    float3 Rx = normalize(cross(dPdy, N));
    float3 Ry = normalize(cross(N, dPdx));

    float det = dot(dPdx, Rx);

    float2 dHd = height_xy - float2(height);
    float3 surfgrad = dHd.x * Rx + dHd.y * Ry;

    strength = max(strength, 0.0);
    
    result = normalize(abs(det) * N - dist * sign(det) * surfgrad);
    result = normalize(lerp(N, result, strength));

}


#endif
