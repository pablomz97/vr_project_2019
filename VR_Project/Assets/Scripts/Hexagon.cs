using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexagon : MonoBehaviour
{
    public enum Direction { Right, TopRight, TopLeft, Left, BotLeft, BotRight};
    public int rowIndex;
    public int colIndex;
    public HexagonGrid controlGrid;
    private Direction orientation = Direction.Right;
    public bool[] hasExit = new bool[6]; // do not change at runtime!!! readonly is not possible because we want this to be editable in Unity

    // Start is called before the first frame update
    void Start()
    {
        
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

    public GameObject GetNeighbor(Direction d)
    {
        (int, int) index = GetNeighborIndex(d);

        if(index != (-1,-1))
        {
            return controlGrid.Hexagons[index.Item1,index.Item2];
        }
        return null;
    }

    public void SetNeighbor(Direction d, GameObject hexagon)
    {
        //TODO: update properties and delete old object
        if(hexagon.GetComponent<Hexagon>() == null)
        {
            throw new ArgumentException("Object is not a hexagon.");
        }
        (int, int) index = GetNeighborIndex(d);

        if (index != (-1, -1))
        {
            controlGrid.Hexagons[index.Item1, index.Item2] = hexagon;
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

    public static GameObject Create(GameObject prefabHexagon, HexagonGrid controlGrid, int rowIndex, int colIndex, Direction orientation)
    {
        if (prefabHexagon.GetComponent<Hexagon>() == null)
        {
            throw new ArgumentException("Object is not a hexagon.");
        }

        GameObject hObj = Instantiate(prefabHexagon, new Vector3(0, 0, 0), Quaternion.identity);
        Hexagon h = hObj.GetComponent<Hexagon>();
        h.controlGrid = controlGrid;
        h.rowIndex = rowIndex;
        h.colIndex = colIndex;

        hObj.transform.position = h.CalcPosition();

        // The Prefab rotation is dropped so we need to set it again
        hObj.transform.rotation = Quaternion.Euler(-90, 0, 0);

        if (controlGrid.vertexDistance)
        {
            hObj.transform.localScale *= Mathf.Sqrt(4.0f / 3.0f);
        }

        h.Orientation = orientation;

        // We change the reference before destroying the old object so there can never be a null reference
        GameObject old = controlGrid.Hexagons[rowIndex, colIndex];
        controlGrid.Hexagons[rowIndex, colIndex] = hObj;

        if (old != null)
        {
            Destroy(old);
        }
        return hObj;
    }
}
