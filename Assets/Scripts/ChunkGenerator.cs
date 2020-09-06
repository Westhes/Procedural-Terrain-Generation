using System;
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

    /// <summary>
    /// X axis
    /// </summary>
    public int width;
    /// <summary>
    /// Y axis
    /// </summary>
    public int height;
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

        if (isInitialized)
            BakeChunk();
        isInitialized = true;
    }

    public void PrepareChunk()
    {
        chunkdata = Noise.GenerateNoiseMap(width, height, depth, Scale, Seed, Octaves, Persistance, Lucanarity, Offset);
        //CreateCheeseDataset();
        vertices.Clear();
        triangles.Clear();
        uv.Clear();
        normals.Clear();
        CreateTiles();
    }

    public void BakeChunk()
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

        sample[0, 0, 0] = 1f;
        sample[2, 0, 0] = 1f;
        sample[1, 0, 1] = 1f;
        sample[0, 0, 2] = 1f;
        sample[2, 0, 2] = 1f;

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

                        DrawNeigbors(x, y, z);
                    }
                    // Check the neighbors
                    else
                    {
                        // Avoid corners TODO: test.. especially the greater than.
                        //if (x + y == -2 || x + z == -2 || y + z == -2) continue;
                        //if (x == -1 && y == -1) continue;
                        //if (x == -1 && z == -1) continue;
                        //if (y == -1 && z == -1) continue;
                        if (x + y == -2 || x + y == width + height || x + y == height - 1) continue;
                        if (x + z == -2 || x + z == width + height || x + z == height - 1) continue;
                        if (y + z == -2 || y + z == width + height || y + z == height - 1) continue;
                        //if (x + y > width -1 || x + z > width -1 || y + z > width -1) continue;

                        Tile neighborTile = new Tile(TileType.Stone, 0, 0, 0);
                        ChunkGenerator chunk = null;
                        Direction dir = Direction.down;
                        if (x < 0)
                        {
                            if (y == height || z == depth) continue;
                            chunk = neighborChunks[0];
                            if (chunk == null) continue;
                            var ownTile = tileArray[0, y, z];
                            neighborTile = chunk.tileArray[width -1, y, z];
                            
                            //dir = Direction.left;
                            if (!neighborTile.IsTileSolid() && ownTile.IsTileSolid())
                            {
                                //DrawTileSurface(TileType.Grass, Direction.down, x, y, z);
                                DrawTileSurface(TileType.Grass, Direction.right, x, ownTile.y, ownTile.z);
                            }
                        }
                        else if (x >= width)
                        {
                            chunk = neighborChunks[1];
                            if (chunk == null) continue;
                            neighborTile = chunk.tileArray[0, y, z];
                            var ownTile = tileArray[width-1, y, z];
                            if (!neighborTile.IsTileSolid() && ownTile.IsTileSolid())
                            {
                                DrawTileSurface(TileType.Grass, Direction.left, x, ownTile.y, ownTile.z);
                            }
                        }
                        else if (y < 0)
                        {
                            // TODO, also expand the neigborChunks array to allow this
                            continue;
                        }
                        else if (y >= height)
                        {
                            // See comment above.
                            continue;
                        }
                        else if (z < 0)
                        {
                            chunk = neighborChunks[2];
                            if (chunk == null) continue;

                            neighborTile = chunk.tileArray[x, y, depth -1];

                            var ownTile = tileArray[x, y, 0];
                            dir = Direction.right;
                            if (!neighborTile.IsTileSolid() && ownTile.IsTileSolid())
                            {
                                DrawTileSurface(TileType.Grass, Direction.front, x, y, z);
                                //var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                                //obj.transform.parent = this.transform;
                                //obj.transform.localPosition = new Vector3(ownTile.x, ownTile.y, ownTile.z);
                            }
                        }
                        else if (z >= depth)
                        {
                            chunk = neighborChunks[3];
                            if (chunk == null) continue;
                            neighborTile = chunk.tileArray[x, y, 0];

                            var ownTile = tileArray[x, y, depth -1];
                            dir = Direction.right;
                            if (!neighborTile.IsTileSolid() && ownTile.IsTileSolid())
                            {
                                DrawTileSurface(TileType.Grass, Direction.back, x, y, z);
                                //DrawTileSurface(TileType.Grass, Direction.down, x, y, z);
                                //var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                                //obj.transform.parent = this.transform;
                                //obj.transform.localPosition = new Vector3(ownTile.x, ownTile.y, ownTile.z);
                            }
                        }
                        if (chunk == null || neighborTile.IsTileSolid())
                            continue;

                        //DrawTileSurface(x, y, z);
                        //DrawTileSurface(TileType.Stone, Direction.down, x, y, z);
                        //DrawNeighbor(x, y, z, dir);
                    }
                }
            }
        }

        MeshFilter meshfilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        // Allows for lots of vertices.. :)
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.normals = normals.ToArray();
        meshfilter.sharedMesh = mesh;

    }

    // Update is called once per frame
    //void Update()
    //{
    //    foreach(var vert in vertices)
    //    {
    //        //Debug.DrawRay(vert, Vector3.up);
    //    }
    //}

    public void DrawNeigbors(int x, int y, int z)
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

        
        //vertices.Add(new Vector3(-0.5f, -0.5f, -0.5f) + TilePivot);
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
                    Debug.DrawRay(vertices[vertices.Count - 1], Vector3.right * 0.5f, Color.red);
                }
                break;
            case Direction.right:
                // 3,7,6,2
                verts = new int[] { 3, 7, 6, 2 };
                foreach (int vert in verts)
                {
                    vertices.Add(CubeVertices[vert] + TilePivot);
                    normals.Add(Vector3.left);
                    Debug.DrawRay(vertices[vertices.Count - 1], Vector3.left * 0.5f, Color.yellow);
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
                    Debug.DrawRay(vertices[vertices.Count - 1], Vector3.up * 0.5f, Color.blue);
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
                    Debug.DrawRay(vertices[vertices.Count - 1], Vector3.back * 0.5f, Color.green);
                }
                break;
            case Direction.back:
                // 2,6,5,1
                verts = new int[] { 0, 4, 7, 3 };
                foreach (int vert in verts)
                {
                    vertices.Add(CubeVertices[vert] + TilePivot);
                    normals.Add(Vector3.forward);
                    Debug.DrawRay(vertices[vertices.Count - 1], Vector3.forward * 0.5f, Color.black);
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