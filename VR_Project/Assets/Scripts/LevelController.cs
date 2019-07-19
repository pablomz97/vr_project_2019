using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    // Start is called before the first frame update
    public static byte[][] allHexagonEncodings = new byte[7][]; //the first index represents the degree, i.e. the amount of exits
    public GameObject wallPrefab;
    void Start()
    {
        HexagonGrid mainGrid = new GameObject("HexagonGrid").AddComponent<HexagonGrid>();

        Hexagon.prefabsLoaded = true;
        GameObject[] hexagonPrefabs = GameObject.FindGameObjectsWithTag("HexTile");//Resources.LoadAll<GameObject>("HexagonPrefabs");
        Hexagon.hexPrefabs = new SortedDictionary<int, List<Hexagon>>();
        //mainGrid.hexPrefabsUsageIndex = new SortedDictionary<int, int>();

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
                //Debug.Log("Encoding: " + Convert.ToString(h.Encoding, 2) + ", " + h.EncodeStart);
            }
        }

        Hexagon.Direction[] deadEnd = { Hexagon.Direction.Right };

        allHexagonEncodings[1] = new byte[]{ Hexagon.ExitArrayToEncoding(deadEnd).Item1 };

        Hexagon.Direction[] straight = { Hexagon.Direction.Right, Hexagon.Direction.Left };
        Hexagon.Direction[] wideCurve = { Hexagon.Direction.Right, Hexagon.Direction.TopLeft };
        Hexagon.Direction[] tightCurve = { Hexagon.Direction.Right, Hexagon.Direction.TopRight };

        allHexagonEncodings[2] = new byte[] { Hexagon.ExitArrayToEncoding(straight).Item1, Hexagon.ExitArrayToEncoding(wideCurve).Item1, Hexagon.ExitArrayToEncoding(tightCurve).Item1 };

        Hexagon.Direction[] yJct = { Hexagon.Direction.Right, Hexagon.Direction.TopLeft, Hexagon.Direction.BotLeft };
        Hexagon.Direction[] leftSplitoff = { Hexagon.Direction.Right, Hexagon.Direction.Left, Hexagon.Direction.BotLeft };
        Hexagon.Direction[] rightSplitoff = { Hexagon.Direction.Right, Hexagon.Direction.Left, Hexagon.Direction.TopLeft };
        Hexagon.Direction[] arrowJct = { Hexagon.Direction.Right, Hexagon.Direction.TopRight, Hexagon.Direction.TopLeft };

        allHexagonEncodings[3] = new byte[] { Hexagon.ExitArrayToEncoding(yJct).Item1, Hexagon.ExitArrayToEncoding(leftSplitoff).Item1,
            Hexagon.ExitArrayToEncoding(rightSplitoff).Item1, Hexagon.ExitArrayToEncoding(arrowJct).Item1 };

        Hexagon.Direction[] xJct = { Hexagon.Direction.Right, Hexagon.Direction.Left, Hexagon.Direction.TopRight, Hexagon.Direction.BotLeft };
        Hexagon.Direction[] trident = { Hexagon.Direction.Right, Hexagon.Direction.Left, Hexagon.Direction.TopRight, Hexagon.Direction.BotRight };
        Hexagon.Direction[] crippleFourwayJct = { Hexagon.Direction.Left, Hexagon.Direction.TopLeft, Hexagon.Direction.TopRight, Hexagon.Direction.Right };

        allHexagonEncodings[4] = new byte[] { Hexagon.ExitArrayToEncoding(xJct).Item1, Hexagon.ExitArrayToEncoding(trident).Item1, Hexagon.ExitArrayToEncoding(crippleFourwayJct).Item1 };

        Hexagon.Direction[] fivewayJct = { Hexagon.Direction.Left, Hexagon.Direction.TopLeft, Hexagon.Direction.TopRight, Hexagon.Direction.BotLeft, Hexagon.Direction.BotRight };

        allHexagonEncodings[5] = new byte[] { Hexagon.ExitArrayToEncoding(fivewayJct).Item1 };

        Hexagon.Direction[] allwayJct = { Hexagon.Direction.Right, Hexagon.Direction.Left, Hexagon.Direction.TopLeft, Hexagon.Direction.TopRight, Hexagon.Direction.BotLeft, Hexagon.Direction.BotRight };

        allHexagonEncodings[6] = new byte[] { Hexagon.ExitArrayToEncoding(allwayJct).Item1 };

        /*
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
        while(!mapGen.GenerateMap())
        {
            mainGrid.CreateHexagons(); //reset hexagons
            mapGen = new MapGeneratorMST(mainGrid);
            Debug.Log("Map Generation Failed! Retrying...");
        }

        mainGrid.LoadHexagons();

        mapGen.initializeDoor();
        mapGen.placeHintObjects();
        mapGen.placeStones();

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


        //Test treasure room code
        /*
        HashSet<byte> mappings = new HashSet<byte>();

        for(byte i = 1; i < 128; i++)
        {
            byte res = Hexagon.TreasureRoomCode((byte)(i % 64), i >= 64);
            if(!mappings.Add(res))
            {
                Debug.LogError("code not unique");
            }
            Debug.Log(i + " maps to " + res);
        }

        if(!mappings.Add((byte)1))
        {
            Debug.Log("1 already contained as expected.");
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
