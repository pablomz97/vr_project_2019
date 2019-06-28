﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        HexagonGrid mainGrid = new GameObject().AddComponent<HexagonGrid>();

        Hexagon.prefabsLoaded = true;
        GameObject[] hexagonPrefabs = Resources.LoadAll<GameObject>("HexagonPrefabs");
        Hexagon.hexPrefabs = new SortedDictionary<int, List<Hexagon>>();
        Hexagon.hexPrefabsUsageIndex = new SortedDictionary<int, int>();

        // put hexagons in dictionary to allow easy access by exits.
        foreach (GameObject hObj in hexagonPrefabs)
        {
            Hexagon h = new Hexagon(hObj);
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
                hexagonsOfSameType = new List<Hexagon>
                {
                    h
                };
                Hexagon.hexPrefabs.Add(h.Encoding, hexagonsOfSameType);
                Hexagon.hexPrefabsUsageIndex.Add(h.Encoding, 0); //initalize as not used so far
                Debug.Log("Encoding: " + Convert.ToString(h.Encoding, 2) + ", " + h.EncodeStart);
            }
        }

        /* Hexagon.Direction[] deadEnd = { Hexagon.Direction.Right };

        Hexagon.Direction[] straight = { Hexagon.Direction.Right, Hexagon.Direction.Left };
        Hexagon.Direction[] wideCurve = { Hexagon.Direction.Right, Hexagon.Direction.TopLeft };
        Hexagon.Direction[] tightCurve = { Hexagon.Direction.Right, Hexagon.Direction.TopRight };

        Hexagon.Direction[] yJct = { Hexagon.Direction.Right, Hexagon.Direction.TopLeft, Hexagon.Direction.BotLeft };
        Hexagon.Direction[] leftSplitoff = { Hexagon.Direction.Right, Hexagon.Direction.Left, Hexagon.Direction.BotLeft };
        Hexagon.Direction[] rightSplitoff = { Hexagon.Direction.Right, Hexagon.Direction.Left, Hexagon.Direction.TopLeft };
        Hexagon.Direction[] arrowJct = { Hexagon.Direction.Right, Hexagon.Direction.TopRight, Hexagon.Direction.TopLeft };

        Hexagon.Direction[] xJct = { Hexagon.Direction.Right, Hexagon.Direction.Left, Hexagon.Direction.TopRight, Hexagon.Direction.BotLeft };
        Hexagon.Direction[] trident = { Hexagon.Direction.Right, Hexagon.Direction.Left, Hexagon.Direction.TopRight, Hexagon.Direction.BotRight };
        Hexagon.Direction[] crippleFourwayJct = { Hexagon.Direction.Left, Hexagon.Direction.TopLeft, Hexagon.Direction.TopRight, Hexagon.Direction.Right };

        Hexagon.Direction[] fivewayJct = { Hexagon.Direction.Left, Hexagon.Direction.TopLeft, Hexagon.Direction.TopRight, Hexagon.Direction.BotLeft, Hexagon.Direction.BotRight };

        Hexagon.Direction[] allwayJct = { Hexagon.Direction.Right, Hexagon.Direction.Left, Hexagon.Direction.TopLeft, Hexagon.Direction.TopRight, Hexagon.Direction.BotLeft, Hexagon.Direction.BotRight };


        List<Hexagon.Direction[]> tiles = new List<Hexagon.Direction[]>
        {
            deadEnd, deadEnd, deadEnd, deadEnd, deadEnd, deadEnd,
            straight, straight, straight, straight, wideCurve, wideCurve, wideCurve, wideCurve, tightCurve, tightCurve, tightCurve, tightCurve,
            yJct, yJct, yJct, yJct, leftSplitoff, leftSplitoff, leftSplitoff, leftSplitoff, rightSplitoff, rightSplitoff, rightSplitoff, rightSplitoff, arrowJct, arrowJct, arrowJct, arrowJct,
            xJct, xJct, xJct, xJct, trident, trident, trident, trident, crippleFourwayJct, crippleFourwayJct, crippleFourwayJct, crippleFourwayJct,
            fivewayJct, fivewayJct,
            allwayJct, allwayJct
        };

        List<Hexagon.Direction[]> tilesShuffled = new List<Hexagon.Direction[]>();



        for (int listLength = tiles.Count; listLength > 0; listLength--)
        {
            int randomIndex = UnityEngine.Random.Range(0, listLength);
            tilesShuffled.Add(tiles[randomIndex]);
            tiles.RemoveAt(randomIndex);
        }
        Debug.Log("tiles count: " + tilesShuffled.Count);
        Hexagon.Direction[,][] griddirs = new Hexagon.Direction[gridHeight, gridWidth][];
        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                Debug.Log("Index: (" + row + "," + col + ")");
                griddirs[row, col] = tilesShuffled[0];
                tilesShuffled.RemoveAt(0);
            }
        }
        */
        mainGrid.CreateHexagons();

        MapGenerator mapGen = new MapGeneratorMST(mainGrid);
        mapGen.GenerateMap();

        mainGrid.LoadHexagons();

        /*
        for(int row = 0; row < gridHeight; row++)
        {
            for(int col = 0; col < gridWidth; col++)
            {
                Hexagon h = Hexagon.Create(Hexagon.Direction.TopLeft, Hexagon.Direction.BotRight);
                SetHexagonAt(h, row, col);
            }
        }*/

        // Some tests for the data structure

        //SetHexagonAt(Hexagon.Create(), 2, 1);
        //hexagons[2, 1].GetNeighbor(Hexagon.Direction.TopRight).Orientation = Hexagon.Direction.TopLeft;

        //hexagons[0, 0].GetComponent<Hexagon>().SetNeighbor(Hexagon.Direction.TopLeft, new GameObject());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}