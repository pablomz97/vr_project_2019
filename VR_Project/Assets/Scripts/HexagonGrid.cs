﻿using System;
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
    private Hexagon[,] hexagons;
   

    // Start is called before the first frame update
    void Start()
    {

        // Load Hexagon prefabs
        if (!Hexagon.prefabsLoaded)
        {
            Hexagon.prefabsLoaded = true;
            GameObject[] hexagonPrefabs = Resources.LoadAll<GameObject>("HexagonPrefabs");
            Hexagon.hexPrefabs = new SortedDictionary<int, List<Hexagon>>();
            Hexagon.hexPrefabsUsageIndex = new SortedDictionary<int, int>();

            // put hexagons in dictionary to allow easy access by exits.
            foreach (GameObject hObj in hexagonPrefabs)
            {
                Hexagon h = hObj.GetComponent<Hexagon>();
                List<Hexagon> hexagonsOfSameType;

                // Check if list of hexagons already exists in dictionary
                bool initialized = Hexagon.hexPrefabs.TryGetValue(h.Encoding, out hexagonsOfSameType);
                if (initialized)
                {
                    hexagonsOfSameType.Add(h); //add hexagon
                }
                else
                {
                    //create new list containing the hexagon and add it
                    hexagonsOfSameType = new List<Hexagon>();
                    hexagonsOfSameType.Add(h);
                    Hexagon.hexPrefabs.Add(h.Encoding, hexagonsOfSameType);
                    Hexagon.hexPrefabsUsageIndex.Add(h.Encoding, 0); //initalize as not used so far
                    Debug.Log("Encoding: " + Convert.ToString(h.Encoding, 2) + ", " + h.EncodeStart);
                }
            }
        }

        gridWidth = 10;
        gridHeight = 10;
        tileDiam = 12.0f;
        position = transform.position;

        hexagons = new Hexagon[gridHeight, gridWidth];

        for(int row = 0; row < gridHeight; row++)
        {
            for(int col = 0; col < gridWidth; col++)
            {
                Hexagon h = Hexagon.Create(Hexagon.Direction.TopLeft, Hexagon.Direction.BotRight);
                SetHexagonAt(h, row, col);
            }
        }

        // Some tests for the data structure

        SetHexagonAt(Hexagon.Create(), 2, 1);
        //hexagons[2, 1].GetNeighbor(Hexagon.Direction.TopRight).Orientation = Hexagon.Direction.TopLeft;

        //hexagons[0, 0].GetComponent<Hexagon>().SetNeighbor(Hexagon.Direction.TopLeft, new GameObject());

        if (hexagons[0,0].GetNeighbor(Hexagon.Direction.Left) == null)
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

    public Hexagon GetHexagonAt(int row, int col)
    {
        return hexagons[row, col];
    }

    public void SetHexagonAt(Hexagon h, int row, int col)
    {
        h.controlGrid = this;
        h.rowIndex = row;
        h.colIndex = col;
        h.gameObject.transform.position = h.CalcPosition();

        if (vertexDistance)
        {
            h.gameObject.transform.localScale *= Mathf.Sqrt(4.0f / 3.0f);
        }

        // We change the reference before destroying the old object so there can never be a null reference
        Hexagon old = hexagons[row, col];
        hexagons[row, col] = h;

        if (old != null)
        {
            Destroy(old.gameObject);
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
