﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ChunkGenerator : MonoBehaviour
{
    public Vector3 localOffset;
    float[,,] chunkdata;


    Tile[,,] tileArray;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uv = new List<Vector2>();
    private List<Vector3> normals = new List<Vector3>();


    static Vector3[] CubeVertices = new Vector3[]
    {
        // Clockwise square, starting from bottomleft
        new Vector3(-0.5f, -0.5f, -0.5f),// 0
        new Vector3(-0.5f, -0.5f, 0.5f), // 1
        new Vector3(0.5f, -0.5f, 0.5f),  // 2
        new Vector3(0.5f, -0.5f, -0.5f), // 3
            
        // Top
        new Vector3(-0.5f, 0.5f, -0.5f),// 4
        new Vector3(-0.5f, 0.5f, 0.5f), // 5
        new Vector3(0.5f, 0.5f, 0.5f),  // 6
        new Vector3(0.5f, 0.5f, -0.5f), // 7
    };

    [HideInInspector]
    /// <summary>
    /// X axis
    /// </summary>
    public int width;
    [HideInInspector]
    /// <summary>
    /// Y axis
    /// </summary>
    public int height;
    [HideInInspector]
    /// <summary>
    /// Z axis
    /// </summary>
    public int depth;

    public float Scale
    {
        get
        {
            return GenerationManager.instance.scale;
        }
    }
    public float Modifier
    {
        get
        {
            return GenerationManager.instance.modifier;
        }
    }
    public int Seed
    {
        get
        {
            return GenerationManager.instance.seed;
        }
    }
    public int Octaves
    {
        get
        {
            return GenerationManager.instance.octaves;
        }
    }
    public float Persistance
    {
        get
        {
            return GenerationManager.instance.persistance;
        }
    }
    public float Lucanarity
    {
        get
        {
            return GenerationManager.instance.lucanarity;
        }
    }
    public Vector3 Offset
    {
        get
        {
            return GenerationManager.instance.offset + localOffset;
        }
    }

    internal ChunkGenerator[] neighborChunks = new ChunkGenerator[4];

    private bool isInitialized = false;
    public void Initialize()
    {
        GenerationManager.instance.ValueChanged += (o, b) => { PrepareChunk(); };
        if (width > 0 && depth > 0 && height > 0)
            PrepareChunk();

        isInitialized = true;
    }

    public void PrepareChunk()
    {
        chunkdata = Noise.GenerateNoiseMap(width, height, depth, Scale, Seed, Octaves, Persistance, Lucanarity, Offset);

        vertices.Clear();
        triangles.Clear();
        uv.Clear();
        normals.Clear();
        CreateTiles();

        if (isInitialized)
            CreateChunk();
    }

    public void CreateChunk()
    {
        //if (isInitialized)
            GenerateMesh();
    }

    private void CreateTiles()
    {
        width = chunkdata.GetLength(0);
        height = chunkdata.GetLength(1);
        depth = chunkdata.GetLength(2);
        tileArray = new Tile[width, height, depth];

        // Height last
        for (int y = 0; y < height; y++)
        {
            // Depth second
            for (int z = 0; z < depth; z++)
            {
                // width first
                for (int x = 0; x < width; x++)
                {
                    // Filter the types
                    var tileType = chunkdata[x, y, z] > Modifier ? TileType.None : TileType.Stone;
                    tileArray[x,y,z] = new Tile(tileType, x,y,z);
                }
            }
        }

        //width --; 
        //height --;
        //depth --;
    }

    float[,,] CreateSampleDataset()
    {
        float[,,] sample = new float[3, 2, 3];

        //sample[0, 0, 0] = 1f;
        //sample[2, 0, 0] = 1f;
        //sample[1, 0, 1] = 1f;
        //sample[0, 0, 2] = 1f;
        //sample[2, 0, 2] = 1f;

        sample[1, 0, 0] = 1f;
        sample[0, 0, 1] = 1f;
        sample[2, 0, 1] = 1f;
        sample[1, 0, 2] = 1f;

        sample[0, 1, 0] = 1f;
        sample[1, 1, 0] = 1f;
        sample[2, 1, 0] = 1f;
        sample[0, 1, 1] = 1f;
        sample[1, 1, 1] = 1f;
        sample[2, 1, 1] = 1f;
        sample[0, 1, 2] = 1f;
        sample[1, 1, 2] = 1f;
        sample[2, 1, 2] = 1f;


        return sample;
    }

    float[,,] CreateCheeseDataset()
    {
        float[,,] sample = new float[16, 32, 16];

        int i = 0;

        for (int x = 0; x < sample.GetLength(0); x++)
        {
            for (int y = 0; y < sample.GetLength(1); y++)
            {
                for (int z = 0; z < sample.GetLength(2); z++)
                {
                    if (i % 3 == 1)
                        sample[x, y, z] = 1f;
                    i++;
                }

            }
        }

        return sample;
    }

    float[,,] CreatePerlinDataset()
    {
        float[,,] sample = new float[32, 64, 32];


        for (int x = 0; x < sample.GetLength(0); x++)
        {
            for (int y = 0; y < sample.GetLength(1) -10; y++)
            {
                for (int z = 0; z < sample.GetLength(2); z++)
                {

                    //var val = Perlin.Noise(x * scale, y * scale, z * scale);
                    var val = Perlin.Perlin3D(x * Scale, y * Scale, z * Scale);
                    val = (val > Modifier) ? 1 : 0;

                    sample[x, y, z] = val;
                }

            }
        }

        return sample;
    }


    private void GenerateMesh()
    {
        // Height last
        for (int y = -1; y < height +1; y++)
        {
            // Depth second
            for (int z = -1; z < depth +1; z++)
            {
                // width first
                for (int x = -1; x < width +1; x++)
                {
                    if (!(x == -1 || y == -1 || z == -1 ||
                        x == width || y == height || z == depth))
                    {
                        var currentTile = tileArray[x, y, z];

                        if (currentTile.IsTileSolid())
                            continue;

                        DrawNeighbors(x, y, z);
                    }
                    // Check the neighbors
                    else
                    {
                        ValidateNeighborChunk(x, y, z);
                    }
                }
            }
        }

        MeshFilter meshfilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        // Allows for lots of vertices.. :)
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.normals = normals.ToArray();
        meshfilter.sharedMesh = mesh;

    }

    private void ValidateNeighborChunk(int x, int y, int z)
    {
        // Avoid corners
        if (x + y == -2 || x + y == width + height || x + y == height - 1 ||
            x + z == -2 || x + z == width + height || x + z == height - 1 ||
            y + z == -2 || y + z == width + height || y + z == height - 1) return;
        
        int neighborX = x;
        int neighborY = y;
        int neighborZ = z;
        int ownX = x;
        int ownY = y;
        int ownZ = z;
        bool bypass = false;
        ChunkGenerator chunk = null;
        Direction dir = Direction.left;
        if (x < 0)
        {
            if (y == height || z == depth) return;
            chunk = neighborChunks[0];
            neighborX = width -1;
            ownX = 0;
            dir = Direction.right;
        }
        else if (x >= width)
        {
            chunk = neighborChunks[1];
            neighborX = 0;
            ownX = width - 1;

            dir = Direction.left;
        }
        else if (y < 0)
        {
            // TODO, also expand the neigborChunks array to allow this
            //dir = Direction.down;
            //DrawTileSurface(TileType.Grass, dir, x, y, z);
            return;
        }
        else if (y >= height)
        {
            // See comment above.
            //dir = Direction.up;
            //DrawTileSurface(TileType.Grass, dir, x, y, z);

            return;
        }
        else if (z < 0)
        {
            chunk = neighborChunks[2];
            neighborZ = depth - 1;
            ownZ = 0;
            dir = Direction.front;
        }
        else if (z >= depth)
        {
            chunk = neighborChunks[3];
            neighborZ = 0;
            ownZ = depth - 1;
            dir = Direction.back;
        }

        if (chunk == null) return;
        var neighborTile = chunk.tileArray[neighborX, neighborY, neighborZ];
        var ownTile = tileArray[ownX, ownY, ownZ];

        if (!neighborTile.IsTileSolid() && ownTile.IsTileSolid())
            DrawTileSurface(ownTile.type, dir, x, y, z);
    }

    public void DrawNeighbors(int x, int y, int z)
    {
        // Check all the neigbors
        for (int i = -1; i < 2; i += 2)
        {
            var neigborX = x + i;
            var neigborY = y + i;
            var neigborZ = z + i;

            if (neigborX >= 0 && neigborX < width)
            {
                //if (z < 0) return;
                //Debug.Log($"{x} {y} {z}   neigborX {neigborX}");
                var neigbor = tileArray[neigborX, y, z];
                if (neigbor.IsTileSolid()) DrawTileSurface(neigbor.type, (Direction)(1 + i), x, y, z);
            }
            if (neigborY >= 0 && neigborY < height)
            {
                //Debug.Log($"{x} {y} {z}   neigbor {neigborY}");
                var neigbor = tileArray[x, neigborY, z];
                if (neigbor.IsTileSolid()) DrawTileSurface(neigbor.type, (Direction)(4 + i), x, y, z);
            }
            if (neigborZ >= 0 && neigborZ < depth)
            {
                var neigbor = tileArray[x, y, neigborZ];
                if (neigbor.IsTileSolid()) DrawTileSurface(neigbor.type, (Direction)(7 + i), x, y, z);
            }
        }
    }


    public void DrawNeighbor(int x, int y, int z, Direction dir)
    {
        ////Debug.DrawRay(new Vector3(x, y, z), Vector3.up, Color.red);
        ////Debug.Break();
        //int actualX = dir == Direction.left ? x : x + 1;
        //int actualX = dir == Direction.right ? x : x--;
        //int actualX = dir == Direction.left ? x : x--;
        //DrawTileSurface()
    }

    /// <summary>
    /// Draws the surface of a tile
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="direction">Which face should be drawn</param>
    public void DrawTileSurface(TileType type, Direction direction, int x, int y, int z)
    {
        Vector3 TilePivot = new Vector3(x, y, z);
        int[] verts;
        int VerticeIndex = vertices.Count;

        switch (direction)
        {
            case Direction.left:
                // 1,5,4,0
                verts = new int[] { 1, 5, 4, 0 };
                foreach (int vert in verts)
                {
                    vertices.Add(CubeVertices[vert] + TilePivot);
                    normals.Add(Vector3.right);
                }
                break;
            case Direction.right:
                // 3,7,6,2
                verts = new int[] { 3, 7, 6, 2 };
                foreach (int vert in verts)
                {
                    vertices.Add(CubeVertices[vert] + TilePivot);
                    normals.Add(Vector3.left);
                }
                break;
            case Direction.down:
                //return;
                // 1,0,3,2
                verts = new int[] { 1, 0, 3, 2 };
                foreach (int vert in verts)
                {
                    vertices.Add(CubeVertices[vert] + TilePivot);
                    normals.Add(Vector3.up);
                }
                break;
            case Direction.up:
                // 4,5,6,7
                verts = new int[] { 4, 5, 6, 7 };
                foreach (int vert in verts)
                {
                    vertices.Add(CubeVertices[vert] + TilePivot);
                    normals.Add(Vector3.down);
                }
                break;
            case Direction.front:
                // 0,4,7,3
                verts = new int[] { 2, 6, 5, 1 };
                foreach (int vert in verts)
                {
                    vertices.Add(CubeVertices[vert] + TilePivot);
                    normals.Add(Vector3.back);
                }
                break;
            case Direction.back:
                // 2,6,5,1
                verts = new int[] { 0, 4, 7, 3 };
                foreach (int vert in verts)
                {
                    vertices.Add(CubeVertices[vert] + TilePivot);
                    normals.Add(Vector3.forward);
                }
                break;
        }
        triangles.Add(VerticeIndex);
        triangles.Add(VerticeIndex + 2);
        triangles.Add(VerticeIndex + 1);

        triangles.Add(VerticeIndex + 3);
        triangles.Add(VerticeIndex + 2);
        triangles.Add(VerticeIndex);

        uv.AddRange(GetUv(type, x, y, z));
    }

    private Vector3 oldLocalOffset = new Vector3();
    private void OnValidate()
    {
        if (Application.isPlaying && oldLocalOffset != localOffset && isInitialized)
        {
            oldLocalOffset = localOffset;
            PrepareChunk();
        }
    }

    public Vector2[] GetUv(TileType type, int x, int y, int z)
    {
        // type 0 == none/air
        float tileSizeX1 = 1f / 8 * ((int)type -1);
        float tileSizeX2 = 1f/ 8 * ((int)type);
        float tileSizeY = 1f /32 * 16f;
        switch (type)
        {
            case TileType.Stone:
                return new Vector2[] { 
                    new Vector2(tileSizeX2, 0),
                    new Vector2(tileSizeX2, 1), 
                    new Vector2(tileSizeX1, 1),
                    new Vector2(tileSizeX1, 0),
                };
            case TileType.Grass:
                // TODO add additional logic to get the right tile.
                return new Vector2[]
                {
                    new Vector2(tileSizeX2, 0),
                    new Vector2(tileSizeX2, 1),
                    new Vector2(tileSizeX1, 1),
                    new Vector2(tileSizeX1, 0),
                };
        }
        throw new ArgumentOutOfRangeException();
    }


}
public enum Direction : int
{
    left = 0,
    right = 2,

    down = 3,
    up = 5,

    back = 6,
    front = 8,
}