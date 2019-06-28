using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapGenerator
{
    protected HexagonGrid grid;

    public MapGenerator(HexagonGrid grid)
    {
        this.grid = grid;
    }

    public abstract void GenerateMap();

    public void RandomizeEdgeCost(HexagonGrid grid)
    {
        for(int i = 0; i < grid.GridHeight; i++)
        {
            for(int j = 0; j < grid.GridWidth; j++)
            {
                for(int k = 0; k < 6; k++)
                {
                    grid.GetHexagonAt(i, j).edgeCost[k] = Random.Range(0, 1024);
                }
            }
        }
    }
}
