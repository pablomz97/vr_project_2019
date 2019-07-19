using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapGenerator
{
    protected HexagonGrid grid;

    public MapGenerator(HexagonGrid grid)
    {
        this.grid = grid;
    }

    public abstract bool GenerateMap();

    public void RandomizeEdgeCost(HexagonGrid grid)
    {
        RandomizeEdgeCost(grid, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }

    public void RandomizeEdgeCost(HexagonGrid grid, int seed)
    {
        Debug.Log("Seed: " + seed);
        System.Random rng = new System.Random(seed);
        for(int i = 0; i < grid.GridHeight; i++)
        {
            for(int j = 0; j < grid.GridWidth; j++)
            {
                for(int k = 0; k < 6; k++)
                {
                    grid.GetHexagonAt(i, j).edgeCost[k] = rng.Next(0, 1024);
                }
            }
        }
    }

    protected bool MatchConstraints()
    {
        // Set upper right half-edge for treasure room
        grid.GetHexagonAt(grid.GridHeight - 1, grid.GridWidth - 1).SetHasExit(Hexagon.Direction.BotRight, true);

        byte[][] encodings = LevelController.allHexagonEncodings;
        for (int deg = 6; deg > 0; deg--)
        {
            for(int i = 0; i < encodings[deg].Length; i++)
            {
                grid.hexPrefabsUsageIndex.TryGetValue(encodings[deg][i], out List<Hexagon> hexagonsOfType);
                
                
                if (hexagonsOfType != null)
                {
                    int maxCount = Hexagon.hexPrefabs.TryGetValue(encodings[deg][i], out List<Hexagon> existingPrefabsOfType) ? existingPrefabsOfType.Count : 0;
                    Debug.Log("maxCount: " + maxCount);
                    if (hexagonsOfType.Count > maxCount)
                    {
                        if (!AugmentHexagonsByInsertion(deg, encodings[deg][i], maxCount, hexagonsOfType))
                        {
                            Debug.Log("Constraints could not be matched.");
                            return false;
                        }
                    }
                }
            }
        }

        /*/for (int deg = 1; deg <= 6; deg++)
        {
            for (int i = 0; i < encodings[deg].Length; i++)
            {
                grid.hexPrefabsUsageIndex.TryGetValue(encodings[deg][i], out List<Hexagon> hexagonsOfType);
                if (hexagonsOfType != null && hexagonsOfType.Count > maxCount)
                {
                    AugmentHexagonsByDeletion(deg, encodings[deg][i], maxCount, hexagonsOfType);
                }
            }
        }*/
        return true;

    }

    /// <summary>
    /// Determine which hexagon is used for the code at the entrance to the treasure room
    /// </summary>
    /// <returns></returns>
    public Hexagon CalcTreasureRoomLock()
    {
        int max = 0;
        (int, int) maxIndex = (1, 1);
        for (int row = 1; row < grid.GridHeight - 1; row++)
        {
            for (int col = 1; col < grid.GridWidth - 1; col++)
            {
                int temp = AccuDistToNeighbors(grid.GetHexagonAt(row, col));
                if (temp > max)
                {
                    max = temp;
                    maxIndex = (row, col);
                }
            }
        }

        return grid.GetHexagonAt(maxIndex.Item1, maxIndex.Item2);
    }

    public void placeHintObjects()
    {
        GameObject treasurePrefab = grid.treasureRoom.GameObject;
        Transform hintPos =  treasurePrefab.transform.Find("HintPosition");

        GameObject[] hintObjects = GameObject.FindGameObjectsWithTag("HintObject");
        int correctIndex = UnityEngine.Random.Range(0, hintObjects.Length);

        HexagonGrid.Instantiate(hintObjects[correctIndex], hintPos.position, Quaternion.identity);

        List<Hexagon> hexTiles = grid.GetHexagons();
        hexTiles.Remove(grid.keyTarget);
        hexTiles.Remove(grid.GetHexagonAt(0, 0));

        Vector2 offset = UnityEngine.Random.insideUnitCircle;
        Vector3 targetPos = new Vector3(grid.keyTarget.GameObject.transform.position.x + offset.x, grid.keyTarget.GameObject.transform.position.y, grid.keyTarget.GameObject.transform.position.z + offset.y);

        HexagonGrid.Instantiate(hintObjects[correctIndex], targetPos, Quaternion.identity);

        int i = UnityEngine.Random.Range(0, hexTiles.Count);
        while(hexTiles[i].TreasureRoomCode() == grid.keyTarget.TreasureRoomCode())
        {
            i = UnityEngine.Random.Range(0, hexTiles.Count);
        }

        targetPos = new Vector3(hexTiles[i].GameObject.transform.position.x + offset.x, hexTiles[i].GameObject.transform.position.y, hexTiles[i].GameObject.transform.position.z + offset.y);

        HexagonGrid.Instantiate(hintObjects[correctIndex], targetPos, Quaternion.identity);
        hexTiles.Remove(hexTiles[i]);

        for(int j = 0; j < hintObjects.Length; ++j)
        {
            if(j == correctIndex)
                continue;

            for(int k = 0; k < 2; ++k)
            {
                int index = UnityEngine.Random.Range(0, hexTiles.Count);
                targetPos = new Vector3(hexTiles[index].GameObject.transform.position.x + offset.x, hexTiles[index].GameObject.transform.position.y, hexTiles[index].GameObject.transform.position.z + offset.y);

                HexagonGrid.Instantiate(hintObjects[j], targetPos, Quaternion.identity);
                hexTiles.Remove(hexTiles[index]);
            }
        }

    }

    public void placeStones()
    {
        GameObject stone = GameObject.Find("PF_Rock");

        List<Hexagon> hexTiles = grid.GetHexagons();

        for(int k = 0; k < 12; ++k)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * 2;
            int index = UnityEngine.Random.Range(0, hexTiles.Count);
            Vector3 targetPos = new Vector3(hexTiles[index].GameObject.transform.position.x + offset.x, hexTiles[index].GameObject.transform.position.y + 0.2f, hexTiles[index].GameObject.transform.position.z + offset.y);

            HexagonGrid.Instantiate(stone, targetPos, Quaternion.identity);
            hexTiles.Remove(hexTiles[index]);
        }
    }

    public void initializeDoor()
    {
        GameObject treasurePrefab = grid.treasureRoom.GameObject;
        DoorLocked Door = treasurePrefab.transform.Find("PF_Door_locked").Find("animRoot").gameObject.GetComponent<DoorLocked>();
        
        Door.middleSymbol.GetComponent<SymbolPanel>().setNumber(grid.keyTarget.TreasureRoomCode());


        for(int dir = 0; dir < 6; ++dir)
        {
            Hexagon neighbor = grid.keyTarget.GetNeighbor((Hexagon.Direction)dir);
            //Door.symbols[dir].GetComponent<SymbolPanel>().setNumber(neighbor.TreasureRoomCode());
            Door.targetCode[dir]= neighbor.TreasureRoomCode();
        }
    }

    private bool AugmentHexagonsByDeletion(int deg, byte encoding, int maxCount, List<Hexagon> hexagonsOfType)
    {
        bool[] hasExit = Hexagon.EncodingToExitArray(encoding, Hexagon.Direction.Right);

        int[] falseIndices = new int[6 - deg];
        int[] trueIndices = new int[deg];

        int falseIndex = 0;
        int trueIndex = 0;
        for (int i = 0; i < 6; i++)
        {
            if (hasExit[i])
            {
                trueIndices[trueIndex++] = i;
            }
            else
            {
                falseIndices[falseIndex++] = i;
            }
        }

        for (int i = 1; i <= Mathf.Pow(2, trueIndices.Length) + 0.5f; i++)
        {
            for (int j = 0; j < trueIndices.Length; j++)
            {
                hasExit[trueIndices[j]] = !Convert.ToBoolean(i & (1 << j));
            }

            grid.hexPrefabsUsageIndex.TryGetValue(Hexagon.ExitArrayToEncoding(hasExit).Item1, out List<Hexagon> replacementCandidates);

            if (replacementCandidates == null || replacementCandidates.Count < maxCount)
            {
                int failedAttempts = 0;
                do
                {
                    Hexagon hexToModify = hexagonsOfType[failedAttempts];
                    Hexagon.Direction orientation = hexToModify.EncodeStart;
                    List<Hexagon> neighbors = new List<Hexagon>();

                    //Preliminary removal of edges
                    for (int j = 0; j < trueIndices.Length; j++)
                    {
                        hexToModify.SetHasExit((Hexagon.Direction)(((int)orientation + trueIndices[j]) % 6), hasExit[trueIndices[j]]);
                        neighbors.Add(hexToModify.GetNeighbor((Hexagon.Direction)(((int)orientation + trueIndices[j]) % 6)));
                    }

                    bool canBeRemoved;

                    // ensure that connection to treasure room is not deleted
                    if (hexToModify.rowIndex == grid.GridHeight - 1 && hexToModify.colIndex == grid.GridWidth - 1 && hexToModify.GetHasExit(Hexagon.Direction.BotRight) == false)
                    {
                        canBeRemoved = false;
                    }
                    else
                    {
                        canBeRemoved = IsConnected(hexToModify, neighbors, new List<Hexagon>());
                    }

                    //Re-insert edges to avoid breaking the hexagons encoding
                    for (int j = 0; j < trueIndices.Length; j++)
                    {
                        hexToModify.SetHasExit((Hexagon.Direction)(((int)orientation + trueIndices[j]) % 6), true);
                    }

                    if (canBeRemoved)
                    {
                        //Remove edges for good
                        for (int j = 0; j < trueIndices.Length; j++)
                        {
                            hexToModify.SetHasExit((Hexagon.Direction)(((int)orientation + trueIndices[j]) % 6), hasExit[trueIndices[j]]);
                        }
                    }
                    else
                    {
                        failedAttempts++;
                    }
                }
                while (hexagonsOfType.Count > maxCount && failedAttempts < hexagonsOfType.Count);
            }
        }

        return false;
    }

    private bool AugmentHexagonsByInsertion(int deg, byte encoding, int maxCount, List<Hexagon> hexagonsOfType)
    {
        bool[] hasExit = Hexagon.EncodingToExitArray(encoding, Hexagon.Direction.Right);

        int[] falseIndices = new int[6 - deg];
        int[] trueIndices = new int[deg];

        int falseIndex = 0;
        int trueIndex = 0;
        for (int i = 0; i < 6; i++)
        {
            if (hasExit[i])
            {
                trueIndices[trueIndex++] = i;
            }
            else
            {
                falseIndices[falseIndex++] = i;
            }
        }

        for(int i = 1; i < Mathf.Pow(2, falseIndices.Length) + 0.5f; i++)
        {
            for(int j = 0; j < falseIndices.Length; j++)
            {
                hasExit[falseIndices[j]] = Convert.ToBoolean(i & (1 << j));
            }

            grid.hexPrefabsUsageIndex.TryGetValue(Hexagon.ExitArrayToEncoding(hasExit).Item1, out List<Hexagon> replacementCandidates);
            Hexagon.hexPrefabs.TryGetValue(Hexagon.ExitArrayToEncoding(hasExit).Item1, out List<Hexagon> existingReplacementCandidates);
            int rcMaxCount = existingReplacementCandidates.Count;
            Debug.Log("rcMaxCount: " + rcMaxCount);
            while (hexagonsOfType.Count > maxCount && (replacementCandidates == null || replacementCandidates.Count < rcMaxCount))
            {
                Hexagon hexToModify = hexagonsOfType[0];
                Hexagon.Direction orientation = hexToModify.EncodeStart;

                for (int j = 0; j < falseIndices.Length; j++)
                {
                    hexToModify.SetHasExit((Hexagon.Direction)(((int)orientation + falseIndices[j]) % 6), hasExit[falseIndices[j]]);
                }

                if(replacementCandidates == null) // we might now be able to obtain a reference
                {
                    grid.hexPrefabsUsageIndex.TryGetValue(Hexagon.ExitArrayToEncoding(hasExit).Item1, out  replacementCandidates);
                }
            }
        }

        grid.hexPrefabsUsageIndex.TryGetValue(encoding, out List<Hexagon> controllist);
        Debug.Log("Size: " + controllist.Count);
        
        return hexagonsOfType.Count <= maxCount;
    }

    private bool IsEdgeOnCycle((Hexagon, Hexagon) edge)
    {
        Hexagon.Direction? connectionDirOpt = edge.Item1.DirectionOfContact(edge.Item2);

        if(!connectionDirOpt.HasValue || !edge.Item1.IsConnected(connectionDirOpt.Value))
        {
            throw new ArgumentException("The edge does not exist.");
        }

        edge.Item1.SetHasExit(connectionDirOpt.Value, false);

        bool result = IsConnected(edge.Item1, new List<Hexagon>{ edge.Item2}, new List<Hexagon>());

        edge.Item1.SetHasExit(connectionDirOpt.Value, true);
        return result;
    }

    private bool IsConnected(Hexagon current, List<Hexagon> targets, List<Hexagon> visited)
    {
        if (targets.Contains(current))
        {
            targets.Remove(current);
            if (targets.Count == 0) return true;
        }

        visited.Add(current);
        List<Hexagon> unvisitedNeighbors = new List<Hexagon>();
        for(int dir = 0; dir < 6; dir++)
        {
            if(current.IsConnected((Hexagon.Direction)dir) && !visited.Contains(current.GetNeighbor((Hexagon.Direction)dir)))
            {
                unvisitedNeighbors.Add(current.GetNeighbor((Hexagon.Direction)dir));
            }
        }

        bool found = false;
        foreach(Hexagon h in unvisitedNeighbors)
        {
            found = found || IsConnected(h, targets, visited);
        }

        return targets.Count == 0;
    }

    protected int AccuDistToNeighbors(Hexagon start)
    {
        int amtNeighborsFound = 0;
        int accuDist = 0;
        HashSet<Hexagon> visited = new HashSet<Hexagon>();
        Queue<(Hexagon,int)> q = new Queue<(Hexagon,int)>();
        q.Enqueue((start,0));
        visited.Add(start);
        while(amtNeighborsFound < 6 && q.Count != 0)
        {
            (Hexagon, int) current = q.Dequeue();
            if(current.Item1.IsNeighborOf(start))
            {
                amtNeighborsFound++;
                accuDist += current.Item2;
                Debug.Log("Distance to neighbor (" + current.Item1.rowIndex + "," + current.Item1.colIndex + "): "+ current.Item2);
            }

            for(int dir = 0; dir < 6; dir++)
            {
                if(current.Item1.IsConnected((Hexagon.Direction)dir) && !visited.Contains(current.Item1.GetNeighbor((Hexagon.Direction)dir)))
                {
                    visited.Add(current.Item1.GetNeighbor((Hexagon.Direction)dir));
                    q.Enqueue((current.Item1.GetNeighbor((Hexagon.Direction)dir), current.Item2 + 1));
                }
            }
        }

        return accuDist;
    }
}
