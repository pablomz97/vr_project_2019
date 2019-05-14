using UnityEngine;
using System.Collections;
using System.Linq; // used for Sum of array

public class paintTerrain : MonoBehaviour
{

        
    void Start()
    {
        // Get the attached terrain component
        Terrain terrain = GetComponent<Terrain>();

        // Get a reference to the terrain data
        TerrainData terrainData = terrain.terrainData;

        float[,,] brush = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for(int x = 0; x < terrainData.alphamapHeight/2; x++)
        {
            for (int y = 0; y < terrainData.alphamapWidth/2; y++)
            {
                int locationX = x;
                int locationY = y;

                if (x - y < 20 && x - y > -20)
                {
                    brush[locationY, locationX, 0] = 0;
                    brush[locationY, locationX, 1] = 1; //changing texture to "chalk"
                } 
            }
        }

        terrainData.SetAlphamaps(0, 0, brush);
    }
}