using UnityEngine;


public static class Noise
{
    public static float Erode(float Gradient, float Coefficient)
    {
        // Starts out at one if the gradient is 0, reduces the influence the higher the gradient is
        float influence = 1 / (1 + Gradient * Coefficient);
        return influence;
    }

    public static float[,] GenerateNoiseMap(int MapWidth, int MapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, float erosionCoefficient, Vector2 offset)
    {
        // Creates a random system
        System.Random prng = new System.Random(seed);
        // Samples the noise from different places to create different types of terrain
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Creates the grid of values
        float[,] noiseMap = new float[MapWidth, MapHeight];
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = 0;
        float minNoiseHeight = Mathf.Infinity;


        float halfWidth = MapWidth / 2f;
        float halfHeight = MapHeight / 2f;

        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                // The noise code
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                Vector2 gradient = Vector2.zero;
                for (int i = 0; i < octaves; i++)
                {
                    // Calculates where to sample the noise from
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
                    // Offsets the other samples
                    float sampleX2 = sampleX + 0.01f;
                    float sampleY2 = sampleY + 0.01f;


                    // Samples at 3 different points
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    float perlinValueX = Mathf.PerlinNoise(sampleX2, sampleY);
                    float perlinValueY = Mathf.PerlinNoise(sampleX, sampleY2);

                    // Calculates the partial dervatives in x and y
                    // Multivariable calculus lol
                    float GradX = (perlinValueX - perlinValue) / 0.01f;
                    float GradY = (perlinValueY - perlinValue) / 0.01f;

                    // Creates a vector for the gradient
                    // The gradient is usually represented as a vector anyways

                    // The gradient of lower octaves is usually
                    gradient.x += GradX;
                    gradient.y += GradY;
                    // Calculates the magnitude
                    float Slope = gradient.magnitude;

                    // Adjusts the noise influence by the gradient
                    noiseHeight += perlinValue * amplitude * Erode(Slope, erosionCoefficient);

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                // Adds the calculated noise height to the noise map
                noiseMap[x, y] = noiseHeight;
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

            }
        }

        float newNoiseHeight;
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                // Adjusts the noise so it's always between 0 and 1
                newNoiseHeight = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                noiseMap[x, y] = newNoiseHeight;
            }
        }
        return noiseMap;
    }
}
