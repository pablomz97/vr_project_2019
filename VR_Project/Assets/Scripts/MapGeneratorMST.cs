using Mischel.Collections;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using UnityEngine;

public class MapGeneratorMST : MapGenerator
{
    public MapGeneratorMST(HexagonGrid grid) : base(grid)
    {

    }

    public override void GenerateMap()
    {
        GameObject secondGridObj = HexagonGrid.Instantiate(grid.gameObject);
        HexagonGrid secondGrid = secondGridObj.GetComponent<HexagonGrid>();
        secondGrid.CreateHexagons();

        RandomizeEdgeCost(grid);
        RandomizeEdgeCost(secondGrid);

        CalculateMST(grid);
        CalculateMST(secondGrid);

        grid.Union(secondGrid);
        HexagonGrid.Destroy(secondGrid);
    }

    private void CalculateMST(HexagonGrid mstGrid)
    {
        int[,] dist = new int[mstGrid.GridHeight, mstGrid.GridWidth];
        (int,int)[,] pre = new (int, int)[mstGrid.GridHeight, mstGrid.GridWidth];

        for (int i = 0; i < mstGrid.GridHeight; i++)
        {
            for (int j = 0; j < mstGrid.GridWidth; j++)
            {
                dist[i, j] = int.MaxValue;
                pre[i, j] = (-1, -1);
            }
        }

        dist[0, 0] = 0;

        PriorityQueue<(int, int), int> pQueue = new PriorityQueue<(int, int), int>(mstGrid.GridHeight * mstGrid.GridWidth, (x, y) => y.CompareTo(x));

        for (int i = 0; i < mstGrid.GridHeight; i++)
        {
            for (int j = 0; j < mstGrid.GridWidth; j++)
            {
                pQueue.Enqueue((i, j), dist[i, j]);
            }
        }

        while(pQueue.Count != 0)
        {
            //pQueue.TryGetValue(pQueue.Keys.First<int>(), out (int, int) vertex);
            (int, int) vertex = pQueue.Dequeue().Value;

            Hexagon vertexHex = mstGrid.GetHexagonAt(vertex.Item1, vertex.Item2);

            if (vertex != (0, 0))
            {
                Hexagon.Direction connectedNeighborDir = vertexHex.DirectionOfContact(pre[vertex.Item1, vertex.Item2].Item1, pre[vertex.Item1, vertex.Item2].Item2).Value;
                vertexHex.hasExit[(int)connectedNeighborDir] = true;
                vertexHex.GetNeighbor(connectedNeighborDir).hasExit[((int)connectedNeighborDir + 3) % 6] = true;
            }

            for (int k = 0; k < 6; k++)
            {
                Hexagon neighbor = vertexHex.GetNeighbor((Hexagon.Direction)k);

                if (neighbor != null)
                {
                    if (pQueue.Contains((neighbor.rowIndex, neighbor.colIndex)) && dist[neighbor.rowIndex, neighbor.colIndex] > (vertexHex.edgeCost[k] + neighbor.edgeCost[(k+3)%6]))
                    {
                        dist[neighbor.rowIndex, neighbor.colIndex] = (vertexHex.edgeCost[k] + neighbor.edgeCost[(k + 3) % 6]);
                        pre[neighbor.rowIndex, neighbor.colIndex] = vertex;
                        

                        pQueue.Remove((neighbor.rowIndex, neighbor.colIndex));
                        pQueue.Enqueue((neighbor.rowIndex, neighbor.colIndex), dist[neighbor.rowIndex, neighbor.colIndex]);
                    }
                }
            }
        }


    }
}
