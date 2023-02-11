using System;
using UnityEngine;

namespace NoiseMath
{
    [Serializable]
    public class Noise
    {

#if UNITY_EDITOR
        [Serializable] public enum NoiseFractalType { ValueFractal, Value }
        [SerializeField] private NoiseFractalType noiseFractalType;
        [SerializeField] [Range(0f, 1f)] private float frequency;
        [SerializeField] public int seed;
        [SerializeField] [Range(0, 10)] private short octaves;
        [SerializeField] [Range(0f, 2f)] private float gain;
        [SerializeField] [Range(0f, 2f)] private float lacunarity;
#else
        private enum NoiseFractalType { ValueFractal, Value }
        private NoiseFractalType noiseFractalType;
        private readonly float frequency;
        private readonly int seed;
        private short octaves;
        private float gain;
        private float lacunarity;
#endif
        private readonly struct Float2
        {
            public readonly float x, y;
            public Float2(float x, float y)
            {
                this.x = x;
                this.y = y;
            }
        }

        private static int FastFloor(float x) { return x > 0 ? (int)x : (int)x - 1; }
        private static float Lerp(float a, float b, float t) { return (1 - t) * a + t * b; }
        private static float Interpolate(float t) { return t * t * t * (t * (t * 6 - 15) + 10); }
        private static float Dot(Float2 g, float x, float y) { return g.x * x + g.y * y; }

        private static readonly Float2[] Grad = {
            new Float2(-1,-1), new Float2( 1,-1), new Float2(-1, 1), new Float2( 1, 1),
            new Float2( 0,-1), new Float2(-1, 0), new Float2( 0, 1), new Float2( 1, 0),
        };

        public static readonly short[] perm =
        {
            182,104,35,233,62,52,70,183,187,115,223,177,146,85,138,161,53,137,92,186,49,8,244,6,130,106,84,253,248,66,149,228,204,164,112,16,68,191,226,28,122,102,156,71,150,196,175,82,210,169,51,94,172,202,205,231,126,255,65,239,159,215,131,37,78,198,36,40,21,43,119,218,123,251,207,22,147,48,0,129,250,224,189,89,180,245,127,93,206,72,160,214,83,120,55,227,252,30,181,163,117,46,77,74,95,219,197,64,243,11,4,155,54,133,80,114,238,134,225,109,69,247,18,13,105,12,100,249,47,20,73,185,79,19,220,96,152,125,208,33,56,144,230,121,34,14,132,211,229,3,63,158,103,38,194,50,5,174,213,81,209,167,29,39,178,23,188,235,44,110,173,139,184,99,98,153,86,168,128,236,42,217,90,165,203,232,26,1,193,199,31,87,200,170,254,27,41,148,75,10,145,166,57,141,2,201,45,221,246,58,67,24,212,113,97,154,240,17,60,32,88,157,136,135,108,241,9,107,162,190,143,140,101,216,242,171,142,237,179,91,124,7,118,25,15,195,222,234,76,61,116,192,151,111,176,59,
            182,104,35,233,62,52,70,183,187,115,223,177,146,85,138,161,53,137,92,186,49,8,244,6,130,106,84,253,248,66,149,228,204,164,112,16,68,191,226,28,122,102,156,71,150,196,175,82,210,169,51,94,172,202,205,231,126,255,65,239,159,215,131,37,78,198,36,40,21,43,119,218,123,251,207,22,147,48,0,129,250,224,189,89,180,245,127,93,206,72,160,214,83,120,55,227,252,30,181,163,117,46,77,74,95,219,197,64,243,11,4,155,54,133,80,114,238,134,225,109,69,247,18,13,105,12,100,249,47,20,73,185,79,19,220,96,152,125,208,33,56,144,230,121,34,14,132,211,229,3,63,158,103,38,194,50,5,174,213,81,209,167,29,39,178,23,188,235,44,110,173,139,184,99,98,153,86,168,128,236,42,217,90,165,203,232,26,1,193,199,31,87,200,170,254,27,41,148,75,10,145,166,57,141,2,201,45,221,246,58,67,24,212,113,97,154,240,17,60,32,88,157,136,135,108,241,9,107,162,190,143,140,101,216,242,171,142,237,179,91,124,7,118,25,15,195,222,234,76,61,116,192,151,111,176,59
        };

        public Noise(int _seed = 1337, float _frequency = 0.1f, NoiseFractalType _noiseFractalType = NoiseFractalType.Value)
        {
            this.seed = _seed;
            this.frequency = _frequency;
            this.noiseFractalType = _noiseFractalType;
        }

        public void SetFractalParameters(short _octaves = 5, float _gain = 0.75f, float _lacunarity = 1.75f)
        {
            this.noiseFractalType = NoiseFractalType.ValueFractal;
            this.octaves = _octaves;
            this.gain = _gain;
            this.lacunarity = _lacunarity;
        }

        public float GetPerlinValue(float x, float y)
        {
            switch (noiseFractalType)
            {
                case NoiseFractalType.ValueFractal:
                    return FractalPerlinValue(seed, frequency * x, frequency * y);

                default:
                    return SinglePerlinValue(seed, frequency * x, frequency * y);
            }
        }

        private float FractalPerlinValue(int _seed, float x, float y)
        {
            int seed = _seed;
            float sum = SinglePerlinValue(seed, x, y);

            float amp = 1;
            for (int i = 0; i < octaves; i++)
            {
                x *= lacunarity;
                y *= lacunarity;

                amp *= gain;
                sum += SinglePerlinValue(++seed, x, y) * amp;
            }

            return sum;
        }

        private float SinglePerlinValue(int seed, float x, float y)
        {
            int x0 = FastFloor(x), y0 = FastFloor(y);
            x -= x0;
            y -= y0;
            x0 = (x0 + seed) & 255;
            y0 = (y0 + seed) & 255;

            float g00 = Dot(Grad[(perm[x0 + perm[y0]]) & 7], x, y);
            float g01 = Dot(Grad[(perm[x0 + 1 + perm[y0]]) & 7], x - 1, y);
            float g10 = Dot(Grad[(perm[x0 + perm[y0 + 1]]) & 7], x, y - 1);
            float g11 = Dot(Grad[(perm[x0 + 1 + perm[y0 + 1]]) & 7], x - 1, y - 1);

            x = Interpolate(x);
            y = Interpolate(y);

            return Lerp(Lerp(g00, g01, x), Lerp(g10, g11, x), y);
        }
    }
}