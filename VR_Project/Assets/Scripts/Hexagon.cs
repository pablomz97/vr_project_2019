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

    /// <summary>
    /// This calculates the position of the tile as it is defined by the Hexagon Grid. Therefore, controlGrid, colIndex and rowIndex must already be set
    /// </summary>
    /// <returns>Absolute position (i.e. using the coordinate system of the grid's parent)</returns>
    public static Vector3 CalcPosition(HexagonGrid controlGrid, int rowIndex, int colIndex)
    {
        //x-axis corresponds to col with the same orientation, z-axis to row but with inverted orientation
        float rightOffset = 0.0f;
        if (rowIndex % 2 == 1) rightOffset = controlGrid.TileDiam / 2.0f;

        // multiplying before taking the square root should decrease numerical error, the factor derives from the Pythagorean theorem
        float zDistance = Mathf.Sqrt(0.75f * Mathf.Pow(controlGrid.TileDiam, 2));

        return controlGrid.Position + new Vector3(colIndex * controlGrid.TileDiam + rightOffset, 0, -rowIndex * zDistance);
    }

    /// <summary>
    /// The orientation of the hexagon, where Right is the default
    /// </summary>
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
            oldRot.y = -(int)orientation * 60.0f;
            gameObject.transform.rotation = Quaternion.Euler(oldRot);
        }
    }

    /// <summary>
    /// Unique binary encoding independent of orientation based on which of the six boundaries have an exit
    /// </summary>
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

    public bool HasExitGlobal(Direction d)
    {
        return hasExit[(d - orientation + 6) % 6];
    }

    public bool isConnected(Direction d)
    {
        if(this.GetNeighbor(d) == null)
        {
            return false;
        }
        return this.HasExitGlobal(d) && this.GetNeighbor(d).HasExitGlobal((Direction)(((int)d + 3) % 6));
    }

    /// <summary>
    /// Creates a Hexagon which has exits at the specified directions. 
    /// </summary>
    /// <param name="exits">directions with an exit</param>
    /// <returns>Reference to the just created Hexagon</returns>
    public static Hexagon Create(params Hexagon.Direction[] exits)
    {
        // calculate encoding:
        bool[] hasExit = new bool[6];

        foreach (Hexagon.Direction exit in exits)
        {
            hasExit[(int)exit] = true;
        }

        if (!HexagonGrid.debugMode)
        {
            (byte encoding, Hexagon.Direction dir) = Hexagon.ExitArrayToEncoding(hasExit);

            List<Hexagon> hexagonOfType;
            bool success = hexPrefabs.TryGetValue(encoding, out hexagonOfType);
            //Debug.Log("Success: " + success);
            hexPrefabsUsageIndex.TryGetValue(encoding, out int offset);
            if (offset / hexagonOfType.Count >= MAX_DUPLICATE_TILES)
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
        else
        {
            (byte encoding, Hexagon.Direction dir) = Hexagon.ExitArrayToEncoding(hasExit);

            hexPrefabsUsageIndex.TryGetValue(encoding, out int offset);
            /*if (offset / hexagonOfType.Count >= MAX_DUPLICATE_TILES)
            {
                Debug.LogWarning("Maximum amount of instances from the same prefab has been exceeded");
            }*/
            hexPrefabsUsageIndex.Remove(encoding);
            hexPrefabsUsageIndex.Add(encoding, offset + 1);


            List<Hexagon> hexagonOfType;
            bool success = hexPrefabs.TryGetValue(0, out hexagonOfType);
            Hexagon prefab = hexagonOfType[0];
            Hexagon.Direction relDir = (Hexagon.Direction)((dir - prefab.EncodeStart + 6) % 6);

            Hexagon dummy = Hexagon.Create(prefab.gameObject, relDir);

            dummy.hasExit = hasExit;
            //TODO: draw lines
            float xDistance = 12.0f/2.0f;
            float zDistance = Mathf.Sqrt(0.75f * Mathf.Pow(xDistance, 2));

            for (int i = 0; i < 6; i++)
            {
                if(dummy.hasExit[i])
                {
                    Vector3 edgePoint = new Vector3();
                    switch ((Direction) i)
                    {
                        case Direction.Right:
                            edgePoint.x = xDistance;
                            break;
                        case Direction.Left:
                            edgePoint.x = -xDistance;
                            break;

                        case Direction.TopRight:
                            edgePoint.x = xDistance/2.0f;
                            edgePoint.z = zDistance;
                            break;
                        case Direction.BotLeft:
                            edgePoint.x = -xDistance / 2.0f;
                            edgePoint.z = -zDistance;
                            break;

                        case Direction.TopLeft:
                            edgePoint.x = -xDistance / 2.0f;
                            edgePoint.z = zDistance;
                            break;

                        case Direction.BotRight:
                            edgePoint.x = xDistance / 2.0f;
                            edgePoint.z = -zDistance;
                            break;
                    }

                    LineRenderer linRen = (new GameObject("edge")).AddComponent<LineRenderer>();
                    linRen.useWorldSpace = false;
                    linRen.positionCount = 2;
                    linRen.SetPosition(0, new Vector3(0.0f,0.0f,0.0f));
                    linRen.SetPosition(1, edgePoint);
                    linRen.widthMultiplier = 0.1f;
                    linRen.gameObject.transform.parent = dummy.gameObject.transform;
                }
            }
            //dummy.Orientation = (Direction)UnityEngine.Random.Range(0, 6);
            return dummy;
        }
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
        //h.gameObject.transform.rotation = Quaternion.Euler(-90, 0, 0);
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
