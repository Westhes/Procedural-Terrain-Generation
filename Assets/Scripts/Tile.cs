using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType : int
{
    None,
    Stone,
}

public struct Tile
{
    public readonly TileType type;

    // these shouldn't be used, but might prove helpfull when debugging.
    public readonly int x;
    public readonly int y;
    public readonly int z;

    public Tile(TileType type, int x, int y, int z) : this()
    {
        this.type = type;
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

public static class TileHelper
{
    public static bool IsTileSolid(this Tile tile)
    {
        return tile.type != TileType.None;
    }
}