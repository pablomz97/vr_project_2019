using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonGrid : MonoBehaviour
{
    private int gridWidth = 7; // amount of tiles in x-direction
    private int gridHeight = 7; // amount of tiles in y-direction
    private Vector3 position; // position of the first tile's center
    private float tileDiam = 12.0f; // distance from an edge to the opposite edge by default
    public SortedDictionary<int, int> hexPrefabsUsageIndex = new SortedDictionary<int, int>();
    public static readonly bool debugMode = true;
    public readonly bool vertexDistance = true; // use distance from vertex to vertex for tileDiam
    private Hexagon[,] hexagons;
   

    // Start is called before the first frame update
    void Awake()
    {
        position = transform.position;

        hexagons = new Hexagon[gridHeight, gridWidth];

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Hexagon GetHexagonAt(int row, int col)
    {
        return hexagons[row, col];
    }

    public void Union(HexagonGrid other)
    {
        for(int i = 0; i < gridHeight; i++)
        {
            for(int j = 0; j < gridWidth; j++)
            {
                SetHexagonAt(Hexagon.Union(GetHexagonAt(i, j), other.GetHexagonAt(i, j)), i, j);
            }
        }
    }

    public void CreateHexagons()
    {
        int rowCount = hexagons.GetLength(0);
        int colCount = hexagons.GetLength(1);
        for (int i = 0; i < rowCount; i++)
        {
            for(int j = 0; j < colCount; j++)
            {
                Hexagon h = new Hexagon();
                SetHexagonAt(h, i, j);
            }
        }
       
    }

    public void LoadHexagons()
    {
        int rowCount = hexagons.GetLength(0);
        int colCount = hexagons.GetLength(1);
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {

                GetHexagonAt(i, j).Load();
            }
        }
    }

    public void SetHexagonAt(Hexagon h, int row, int col)
    {
        h.controlGrid = this;
        h.rowIndex = row;
        h.colIndex = col;
        h.UpdatePosition();
        h.PutInDictionary();
        // We change the reference before destroying the old object so there can never be a null reference
        Hexagon old = hexagons[row, col];
        hexagons[row, col] = h;

        if (old != null)
        {
            old.RemoveFromDictionary();
            Destroy(old.GameObject);
        }
    }

    public int GridWidth
    {
        get { return gridWidth; }
    }

    public int GridHeight
    {
        get { return gridHeight; }
    }

    public Vector3 Position
    {
        get { return position; }
    }

    public float TileDiam
    {
        get { return tileDiam; }
    }

}
