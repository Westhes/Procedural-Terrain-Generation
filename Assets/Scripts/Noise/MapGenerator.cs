using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

[CanEditMultipleObjects]
public class MapGenerator : MonoBehaviour
{
    [Min(1)]
    public int mapWidth;
    [Min(1)]
    public int mapHeight;
    [Min(2f)]
    public float noiseScale;

    [Range(1, 10)]
    public int octaves;
    [Range(0f,1f)]
    public float persistance;
    [Min(1)]
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    private MapDisplay mapDisplay;


    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale, seed, octaves, persistance, lacunarity, offset);

        MapDisplay display = GetComponent<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
    }

    private void Start()
    {
        GenerateMap();
    }

    private void OnValidate()
    {
        if (mapWidth < 1)
            mapWidth = 1;
        if (mapHeight < 1)
            mapHeight = 1;
        if (lacunarity < 1)
            lacunarity = 1;
        if (octaves < 0)
            octaves = 0;
        if (noiseScale < 2)
            noiseScale = 2;
        GenerateMap();
    }
}
