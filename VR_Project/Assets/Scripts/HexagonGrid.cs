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
    public SortedDictionary<int, List<Hexagon>> hexPrefabsUsageIndex = new SortedDictionary<int, List<Hexagon>>();
    public static readonly bool debugMode = false;
    public readonly bool vertexDistance = true; // use distance from vertex to vertex for tileDiam
    private Hexagon[,] hexagons;
    private Hexagon treasureRoom;
    public Hexagon keyTarget;
   

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
        Union(other, 0);
    }

    public void Union(HexagonGrid other, float pruningFactor)
    {
        Union(other, pruningFactor, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }

    public void Union(HexagonGrid other, float pruningFactor, int seed)
    {
        Debug.Log("pruning seed: " + seed);
        System.Random random = new System.Random(seed);
        for(int i = 0; i < gridHeight; i++)
        {
            for(int j = 0; j < gridWidth; j++)
            {
                SetHexagonAt(Hexagon.Union(GetHexagonAt(i, j), other.GetHexagonAt(i, j), pruningFactor, random), i, j);
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

        GameObject[] treasureRoomObjs = GameObject.FindGameObjectsWithTag("TreasureRoom");

        TreasureRoom = new Hexagon(treasureRoomObjs[UnityEngine.Random.Range(0, treasureRoomObjs.Length)]);
        TreasureRoom.UpdatePosition();
        TreasureRoom.Orientation = Hexagon.Direction.BotRight;

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                GameObject currentTile = GetHexagonAt(i, j).GameObject;

                currentTile.transform.parent = gameObject.transform;
                currentTile.GetComponent<staticBatchOnLoad>().Bake();
            }
        }



        StaticBatchingUtility.Combine(gameObject);
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

    public Hexagon TreasureRoom
    {
        get { return treasureRoom; }
        set
        {
            value.controlGrid = this;
            value.IsTreasureRoom = true;
            treasureRoom = value;
        }
    }
}
