

using System.Collections.Generic;
using Controllers;
using UnityEngine.Tilemaps;

public class LandGridCell
{
    /*
     * Needs to have a Dictionary of the different Tiles from the different layers
     * Needs to be able to change the tile for the specific layer
     */

    private Dictionary<TilemapLayer, TileBase> TileBases = new Dictionary<TilemapLayer, TileBase>();

    private int x, y;

    public LandGridCell(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public TileBase GetTileBase(TilemapLayer tilemapLayer)
    {
        return TileBases[tilemapLayer];
    }

    public void SetTileBase(TilemapLayer tilemapLayer, TileBase tileBase)
    {
        TileBases[tilemapLayer] = tileBase;
    }
    
    public override string ToString()
    {
        string result = "";
        foreach (var tileBase in TileBases)
        {
            result += tileBase.ToString() + '\n';
        }

        return result;
    }
}
