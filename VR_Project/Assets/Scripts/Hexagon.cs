using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexagon : MonoBehaviour
{
    public enum Direction { Right, TopRight, TopLeft, Left, BotLeft, BotRight };
    public int rowIndex = -1;
    public int colIndex = -1;
    public HexagonGrid controlGrid;
    public Direction orientation = Direction.Right;
    public bool[] hasExit = new bool[6]; // do not change at runtime!!! readonly is not possible because we want this to be editable in Unity
    private byte encoding = 0xFF;
    private Direction encodeStart;

    public static SortedDictionary<int, List<Hexagon>> hexPrefabs;
    public static SortedDictionary<int, int> hexPrefabsUsageIndex;
    public static readonly int MAX_DUPLICATE_TILES = 100;
    public static bool prefabsLoaded = false;

    static Hexagon()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        //(encoding, encodeStart) = ExitArrayToEncoding(hasExit);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private (int,int) GetNeighborIndex(Direction d)
    {

        // The second row (i.e. index 1) will be shifted by half a tile to the right compared to the first row (i.e. index 0)
        switch (d)
        {
            case Direction.Right:
                if (colIndex < controlGrid.GridWidth - 1) return (rowIndex, colIndex + 1);
                break;

            case Direction.TopRight:

                if (rowIndex > 0)
                {
                    if (rowIndex % 2 == 0) //even row, further on the left, so the tile must exist
                    {
                        return (rowIndex - 1, colIndex);
                    }
                    else //odd row, further on the right
                    {
                        if (colIndex < controlGrid.GridWidth - 1)
                        {
                            return (rowIndex - 1, colIndex + 1);
                        }
                    }
                }

                break;

            case Direction.TopLeft:
               
                if (rowIndex > 0)
                {
                    if (rowIndex % 2 == 0) //even row, further on the left
                    {
                        if (colIndex > 0)
                        {
                            return (rowIndex - 1, colIndex - 1);
                        }
                    }
                    else //odd row, further on the right, so the tile must exist
                    {
                        return (rowIndex - 1, colIndex);
                    }
                }

                break;

            case Direction.Left:
                if (colIndex > 0) return (rowIndex, colIndex - 1);
                break;

            case Direction.BotLeft:

                if (rowIndex < controlGrid.GridHeight - 1)
                {
                    if (rowIndex % 2 == 0) //even row, further on the left
                    {
                        if (colIndex > 0)
                        {
                            return (rowIndex + 1, colIndex - 1);
                        }
                    }
                    else //odd row, further on the right, so the tile must exist
                    {
                        return (rowIndex + 1, colIndex);
                    }
                }

                break;

            case Direction.BotRight:

                if (rowIndex < controlGrid.GridHeight - 1)
                {
                    if (rowIndex % 2 == 0) //even row, further on the left, so the tile must exist
                    {
                        return (rowIndex + 1, colIndex);
                    }
                    else //odd row, further on the right
                    {
                        if (colIndex < controlGrid.GridWidth - 1)
                        {
                            return (rowIndex + 1, colIndex + 1);
                        }
                    }
                }

                break;

            default: break;
        }
        return (-1,-1);
    }

    public Hexagon GetNeighbor(Direction d)
    {
        (int, int) index = GetNeighborIndex(d);

        if(index != (-1,-1))
        {
            return controlGrid.GetHexagonAt(index.Item1,index.Item2);
        }
        return null;
    }

    public void SetNeighbor(Direction d, Hexagon hexagon)
    {
        //TODO: update properties and delete old object
        if(hexagon == null)
        {
            throw new ArgumentException("Object is not a hexagon.");
        }
        (int, int) index = GetNeighborIndex(d);

        if (index != (-1, -1))
        {
            controlGrid.SetHexagonAt(hexagon, index.Item1, index.Item2);
        }

        throw new ArgumentOutOfRangeException("Neighbor does not exist.");
    }

    public Vector3 CalcPosition()
    {
        //x-axis corresponds to col with the same orientation, z-axis to row but with inverted orientation
        float rightOffset = 0.0f;
        if (rowIndex % 2 == 1) rightOffset = controlGrid.TileDiam / 2.0f;

        // multiplying before taking the square root should decrease numerical error, the factor derives from the Pythagorean theorem
        float zDistance = Mathf.Sqrt(0.75f * Mathf.Pow(controlGrid.TileDiam, 2));

        return controlGrid.Position + new Vector3(colIndex * controlGrid.TileDiam + rightOffset, 0, -rowIndex * zDistance);
    }

    public Direction Orientation
    {
        get
        {
            return orientation;
        }

        set
        {
            orientation = value;
            Vector3 oldRot = gameObject.transform.rotation.eulerAngles;
            oldRot.z = -(int)orientation * 60.0f;
            gameObject.transform.rotation = Quaternion.Euler(oldRot);
        }
    }

    public byte Encoding
    {
        get
        {
            if (encoding == 0xFF)
            {
                (encoding, encodeStart) = ExitArrayToEncoding(hasExit);
            }
            return encoding;
        }
    }

    public Direction EncodeStart => encodeStart;

    public static Hexagon Create(params Hexagon.Direction[] exits)
    {
        // calculate encoding:
        bool[] hasExit = new bool[6];

        foreach (Hexagon.Direction exit in exits)
        {
            hasExit[(int)exit] = true;
        }

        (byte encoding, Hexagon.Direction dir) = Hexagon.ExitArrayToEncoding(hasExit);

        List<Hexagon> hexagonOfType;
        bool success = hexPrefabs.TryGetValue(encoding, out hexagonOfType);
        //Debug.Log("Success: " + success);
        hexPrefabsUsageIndex.TryGetValue(encoding, out int offset);
        if(offset / hexagonOfType.Count >= MAX_DUPLICATE_TILES)
        {
            throw new Exception("Maximum amount of instances from the same prefab has been exceeded");
        }
        Hexagon prefab = hexagonOfType[offset % hexagonOfType.Count];
        hexPrefabsUsageIndex.Remove(encoding);
        hexPrefabsUsageIndex.Add(encoding, offset + 1);

        Hexagon.Direction relDir = (Hexagon.Direction)((dir - prefab.EncodeStart + 6) % 6);
        Debug.Log("dir: " + dir + " EncodeStart: " + prefab.EncodeStart + " RelDir: " + relDir);

        return Hexagon.Create(prefab.gameObject, relDir);
    }
        public static Hexagon Create(GameObject prefabHexagon, Direction orientation)
    {
        if (prefabHexagon.GetComponent<Hexagon>() == null)
        {
            throw new ArgumentException("Object is not a hexagon.");
        }


        GameObject hObj = Instantiate(prefabHexagon, new Vector3(0, 0, 0), Quaternion.identity);
        Hexagon h = hObj.GetComponent<Hexagon>();
        // The Prefab rotation is dropped so we need to set it again
        h.gameObject.transform.rotation = Quaternion.Euler(-90, 0, 0);
        h.Orientation = orientation;

        return h;
    }



    public static (byte, Direction) ExitArrayToEncoding(bool[] hasExit)
    {
        byte encoding = 0xFF;
        Direction d = Direction.Right;

        for(int i = 0; i < hasExit.Length; i++) // iterate through all possible starting points
        {
            byte temp = 0;
            for(int j = 0; j < hasExit.Length; j++) //concatenate booleans
            {
                temp |= (byte)(Convert.ToByte(hasExit[(j + i) % hasExit.Length]) << (hasExit.Length - j - 1));
                
            }
           // Debug.Log("Encoding starting at " + (Direction)i + ": " + Convert.ToString(temp, 2));
            if (temp < encoding)
            {
                encoding = temp;
                d = (Direction)i;
            }
        }

        return (encoding, d);
    }
}
