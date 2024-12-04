
#ifndef QUATERNION
#define QUATERNION
inline float4 DirectionToQuaternion(float3 direction)
{
    float3 forward = float3(0, 1, 0);

    float3 axis = normalize(cross(forward, direction));
    float dotProd = dot(forward, direction);

    if (abs(dotProd - 1.0) < 0.0001) 
    {
        return float4(0, 0, 0, 1); 
    }
    else if (abs(dotProd + 1.0) < 0.0001) 
    {
        axis = float3(1, 0, 0);
        return float4(axis, 0);
    }

    float angle = acos(dotProd);

    float s = sin(angle * 0.5);
    float4 quaternion = float4(axis * s, cos(angle * 0.5));
    return quaternion;
}

inline float3 QuaternionToDirection(float4 quaternion)
{
    float3 forward = float3(0, 1, 0);
    float3 u = quaternion.xyz;
    float s = quaternion.w;
    float3 rotatedDirection = 2.0 * dot(u, forward) * u
                            + (s * s - dot(u, u)) * forward
                            + 2.0 * s * cross(u, forward);

    return normalize(rotatedDirection);
}
inline float4 QuaternionMultiply(float4 q1, float4 q2)
{
    float3 xyz = q1.w * q2.xyz + q2.w * q1.xyz + cross(q1.xyz, q2.xyz);
    float w = q1.w * q2.w - dot(q1.xyz, q2.xyz);
    return float4(xyz, w);
}

float3 AddNormals(float3 normal1, float3 normal2)
{
    float4 quaternion1 = DirectionToQuaternion(normal1);
    float4 quaternion2 = DirectionToQuaternion(normal2);
    float4 combinedQuaternion = QuaternionMultiply(quaternion1, quaternion2);
    return QuaternionToDirection(combinedQuaternion);
}
float3 ScaleAngleBetweenVectors(float3 refVector, float3 inputVector, float n)
{
    float3 refVectorNorm = normalize(refVector);
    float3 inputVectorNorm = normalize(inputVector);
    float cosTheta = dot(refVectorNorm, inputVectorNorm);
                                                                                                                                                                                                                                                                                                                                                                            
    cosTheta = clamp(cosTheta, -1.0, 1.0);
    float theta = acos(cosTheta);


    float thetaNew = n * theta;
    float deltaTheta = thetaNew - theta;


    float3 axis = cross(refVectorNorm, inputVectorNorm);

    float axisLengthSq = dot(axis, axis);
    if (axisLengthSq < 1e-6)
    {

        axis = cross(refVectorNorm, float3(1.0, 0.0, 0.0));
        if (length(axis) < 1e-6)
        {
            axis = cross(refVectorNorm, float3(0.0, 1.0, 0.0));
        }
    }
    axis = normalize(axis);


    float cosDeltaTheta = cos(deltaTheta);
    float sinDeltaTheta = sin(deltaTheta);

    float3 rotatedVector = inputVectorNorm * cosDeltaTheta +
                           cross(axis, inputVectorNorm) * sinDeltaTheta +
                           axis * dot(axis, inputVectorNorm) * (1.0 - cosDeltaTheta);

    float inputVectorLength = length(inputVector);
    float3 resultVector = rotatedVector * inputVectorLength;

    return resultVector;
}

#endif
