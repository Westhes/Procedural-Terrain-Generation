using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationManager : MonoBehaviour
{
    // s_Instance is used to cache the instance found in the scene so we don't have to look it up every time.
    private static GenerationManager s_Instance = null;


    // A static property that finds or creates an instance of the manager object and returns it.
    public static GenerationManager instance
    {
        get
        {
            if (s_Instance == null)
            {
                // FindObjectOfType() returns the first AManager object in the scene.
                s_Instance = FindObjectOfType(typeof(GenerationManager)) as GenerationManager;
            }

            // If it is still null, create a new instance
            if (s_Instance == null)
            {
                var obj = new GameObject("GenerationManager");
                s_Instance = obj.AddComponent<GenerationManager>();
            }

            return s_Instance;
        }
    }


    // Ensure that the instance is destroyed when the game is stopped in the editor.
    void OnApplicationQuit()
    {
        s_Instance = null;
    }


    [Range(0f, 100f)] //0.99999f)]
    public float scale = 0.1f;
    [field: SerializeField]
    public static float Scale { get; set; }
    [Range(0.01f, 0.99999f)]
    public float modifier = 0.1f;
    public int seed = 0;
    [Range(0,20)]
    public int octaves = 0;
    [Range(0f, 1f)]
    public float persistance = 0f;
    public float lucanarity = 0f;
    public Vector3 offset;

    public GameObject chunk;
    public int chunkSize = 64;

    [Tooltip("Not updated at runtime")]
    [Range(1, 100)]
    public int chunkCount = 3;

    private ChunkGenerator[,] chunks;

    private void Awake()
    {
        chunks = new ChunkGenerator[chunkCount, chunkCount];

        // Create objects
        for (int x = 0; x < chunkCount; x++)
        {
            for (int z = 0; z < chunkCount; z++)
            {
                var c = Instantiate(this.chunk, new Vector3(x * (chunkSize), 0, z * (chunkSize)), Quaternion.identity);
                var chunk = c.GetComponent<ChunkGenerator>();
                chunk.width = chunkSize;
                chunk.height = chunkSize;
                chunk.depth = chunkSize;
                chunk.localOffset = new Vector3(x * 1.3f, 0, z * 1.3f);

                chunks[x, z] = chunk;
            }
        }

        // Prepare the chunks by assigning them neighbors, allow them to generate their own maps
        for (int x = 0; x < chunkCount; x++)
        {
            for (int z = 0; z < chunkCount; z++)
            {
                // Left
                if (x > 0)
                    chunks[x, z].neighborChunks[0] = chunks[x - 1, z];
                // Right
                if (x < chunkCount - 1)
                    chunks[x, z].neighborChunks[1] = chunks[x + 1, z];

                // backward
                if (z > 0)
                    chunks[x, z].neighborChunks[2] = chunks[x, z - 1];
                // forward
                if (z < chunkCount - 1)
                    chunks[x, z].neighborChunks[3] = chunks[x, z + 1];

                // Create the 3d noisemap now that it contains all the components it needs
                chunks[x, z].Initialize();
            }
        }

        // Create the chunk, while aware of its neighbors
        for (int x = 0; x < chunkCount; x++)
        {
            for (int z = 0; z < chunkCount; z++)
            {
                chunks[x, z].CreateChunk();
            }
        }
    }

    public event EventHandler ValueChanged;
    protected void OnValueChanged()
    {
        EventHandler handler = ValueChanged;
        handler?.Invoke(this, null);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            OnValueChanged();
    }
}
