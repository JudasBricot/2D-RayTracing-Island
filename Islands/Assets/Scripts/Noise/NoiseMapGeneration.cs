using UnityEngine;
using NoiseMath;

namespace TerrainGeneration
{
    public static class NoiseMapGeneration
    {
        // Vérifie que les paramètres sont corrects
        public static float[,] GenerateNoiseMap(int mapSize, Vector2Int position, Noise noise, float noiseScale)
        {
            float[,] noiseMap = new float[mapSize, mapSize];

            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    noiseMap[x,y] = (noise.GetPerlinValue((x + position.x) / noiseScale, (y - position.y) / noiseScale) + 1) / 2;
                }
            }

            return noiseMap;
        }
    }
}