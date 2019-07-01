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

    public void MatchConstraints()
    {
        byte[][] encodings = LevelController.allHexagonEncodings;
        for (int deg = 6; deg > 0; deg--)
        {
            for(int i = 0; i < encodings[deg].Length; i++)
            {
                grid.hexPrefabsUsageIndex.TryGetValue(encodings[deg][i], out int count);
                int maxCount = 6;
                if (count > maxCount)
                {
                    AugmentHexagons(deg, encodings[deg][i], maxCount);
                }
            }
        }
    }

    private void AugmentHexagons(int deg, byte encoding, int maxCount)
    {
        bool[] hasExit = Hexagon.EncodingToExitArray(encoding, Hexagon.Direction.Right);

        int[] falseIndices = new int[6 - deg];
        int[] trueIndices = new int[deg];

        int falseIndex = 0;
        int trueIndex = 0;
        for (int i = 0; i < 6; i++)
        {
            if (hasExit[i])
            {
                trueIndices[trueIndex++] = i;
            }
            else
            {
                falseIndices[falseIndex++] = i;
            }
        }


    }
}
