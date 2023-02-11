using UnityEngine; 

namespace TerrainGeneration
{
    public static class FalloffGenerator
    {
        private static float SquaredDistanceToCenter(int mapSize, int x, int y) => Mathf.Pow((x+1) - mapSize/2, 2) + Mathf.Pow((y+1) - mapSize/2, 2);
        public static float[,] GenerateFalloff(int mapSize, float alpha)
        {
            float[,] falloffMap = new float[mapSize, mapSize];

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    falloffMap[i,j] = 1 / (alpha * mapSize * mapSize) * SquaredDistanceToCenter(mapSize, i ,j);
                }
            }

            return falloffMap;
        }

        public static float[,] GenerateFalloffMap(int size)
        {
            float[,] map = new float[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    float x = i / (float)size * 2 - 1;
                    float y = j / (float)size * 2 - 1;

                    float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                    map[i, j] = Evaluate(value);
                }
            }

            return map;
        }

        static float Evaluate(float value)
        {
            float a = 3f;
            float b = 2.2f;

            return Mathf.Pow(value,a)/(Mathf.Pow(value, a) + Mathf.Pow(b-b*value, a));
        }

    }
}