using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonGrid : MonoBehaviour
{
    private int gridWidth; // amount of tiles in x-direction
    private int gridHeight; // amount of tiles in y-direction
    private Vector3 position; // position of the first tile's center
    private float tileDiam; // distance from an edge to the opposite edge by default
    public readonly bool vertexDistance = true; // use distance from vertex to vertex for tileDiam
    private GameObject[,] hexagons;
    public GameObject hexagonPrefabBasic;
    public GameObject hexagonPrefabStraight;

    // Start is called before the first frame update
    void Start()
    {
        gridWidth = 10;
        gridHeight = 10;
        tileDiam = 12.0f;
        position = transform.position;

        hexagons = new GameObject[gridHeight, gridWidth];

        for(int row = 0; row < gridHeight; row++)
        {
            for(int col = 0; col < gridWidth; col++)
            {
                hexagons[row, col] = Hexagon.Create(hexagonPrefabStraight, this, row, col, Hexagon.Direction.Right);
            }
        }

        // Some tests for the data structure

        Hexagon.Create(hexagonPrefabBasic, this, 2, 1, Hexagon.Direction.Right);
        hexagons[2, 1].GetComponent<Hexagon>().GetNeighbor(Hexagon.Direction.TopRight).GetComponent<Hexagon>().Orientation = Hexagon.Direction.TopLeft;

        //hexagons[0, 0].GetComponent<Hexagon>().SetNeighbor(Hexagon.Direction.TopLeft, new GameObject());

        if (hexagons[0,0].GetComponent<Hexagon>().GetNeighbor(Hexagon.Direction.Left) == null)
        {
            Debug.Log("It has been detected that the object does not exist.");
        }
        else
        {
            Debug.Log("object does not exist but it was not detected.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject[,] Hexagons
    {
        get{ return hexagons; }
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
