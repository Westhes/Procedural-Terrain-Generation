using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise : MonoBehaviour
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int seed, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;

            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y ++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;
                float maxNoise = 0f;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = (Mathf.PerlinNoise(sampleX, sampleY) -0.5f) *2;
                    noiseHeight += perlinValue * amplitude;
                    maxNoise += amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;

                }

                noiseMap[x, y] = (noiseHeight + maxNoise) / (maxNoise * 2);
            }
        }


        return noiseMap;
    }


    public static float[,,] GenerateNoiseMap(int mapWidth, int mapHeight, int mapDepth, float scale, int seed, int octaves, float persistance, float lacunarity, Vector3 offset)
    {
        float[,,] noiseMap = new float[mapWidth, mapHeight, mapDepth];
        System.Random prng = new System.Random(seed);
        Vector3[] octaveOffsets = new Vector3[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            float offsetZ = prng.Next(-100000, 100000) + offset.z;

            octaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);
        }

        if (scale <= 0f)
            scale = 0.0001f;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;
        float halfDepth = mapDepth / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapDepth; z++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;
                    float maxNoise = 0f;


                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                        float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
                        float sampleZ = (z - halfDepth) / scale * frequency + octaveOffsets[i].z;

                        float perlinValue = (Perlin.Perlin3D(sampleX, sampleY, sampleZ) - 0.5f) * 2;
                        noiseHeight += perlinValue * amplitude;
                        maxNoise += amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    noiseMap[x, y, z] = (noiseHeight + maxNoise) / (maxNoise * 2);
                }

            }
        }

        return noiseMap;
    }
}
