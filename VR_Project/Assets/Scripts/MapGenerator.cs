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

    public abstract void GenerateMap();

    public void RandomizeEdgeCost(HexagonGrid grid)
    {
        RandomizeEdgeCost(grid, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
    }

    public void RandomizeEdgeCost(HexagonGrid grid, int seed)
    {
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

    protected void MatchConstraints()
    {
        int maxCount = 4;
        byte[][] encodings = LevelController.allHexagonEncodings;
        for (int deg = 6; deg > 0; deg--)
        {
            for(int i = 0; i < encodings[deg].Length; i++)
            {
                grid.hexPrefabsUsageIndex.TryGetValue(encodings[deg][i], out List<Hexagon> hexagonsOfType);
                
                if (hexagonsOfType != null && hexagonsOfType.Count > maxCount)
                {
                    AugmentHexagonsByInsertion(deg, encodings[deg][i], maxCount, hexagonsOfType);
                }
            }
        }

        for (int deg = 1; deg <= 6; deg++)
        {
            for (int i = 0; i < encodings[deg].Length; i++)
            {
                grid.hexPrefabsUsageIndex.TryGetValue(encodings[deg][i], out List<Hexagon> hexagonsOfType);
                if (hexagonsOfType != null && hexagonsOfType.Count > maxCount)
                {
                    AugmentHexagonsByDeletion(deg, encodings[deg][i], maxCount, hexagonsOfType);
                }
            }
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

                    bool canBeRemoved = IsConnected(hexToModify, neighbors, new List<Hexagon>());

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

            while (hexagonsOfType.Count > maxCount && (replacementCandidates == null || replacementCandidates.Count < maxCount))
            {
                Hexagon hexToModify = hexagonsOfType[0];
                Hexagon.Direction orientation = hexToModify.EncodeStart;

                for (int j = 0; j < falseIndices.Length; j++)
                {
                    hexToModify.SetHasExit((Hexagon.Direction)(((int)orientation + falseIndices[j]) % 6), hasExit[falseIndices[j]]);
                }
            }
        }

        return false;
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
}
