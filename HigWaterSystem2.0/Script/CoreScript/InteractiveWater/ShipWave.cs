
using UnityEngine;
namespace HigWaterSystem2
{
    public static class ShipWave
    {
        public static float CalculateShipWave(
            float shipSpeed,       // Ship speed V, in m/s
            float wavelength,      // Wavelength λ, in meters
            float amplitude,       // Wave amplitude A, in meters
            float kelvinAngle,     // Kelvin angle θ, in radians
            float x,               // Spatial coordinate x, in meters
            float y,               // Spatial coordinate y, in meters
            float t               // Time t, in seconds
        )
        {
            // Calculate the wavenumber k
            float k = 2 * Mathf.PI / wavelength;

            // Main formula for ship waves based on the wave equation
            float waveHeight = amplitude * Mathf.Cos(
                k * (x - shipSpeed * t) * Mathf.Cos(kelvinAngle) + y * Mathf.Sin(kelvinAngle)
            );

            return waveHeight;
        }
    }
}