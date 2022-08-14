using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildTerrainGeneration : MonoBehaviour
{
    private int terrainResolutionChild = 1;

    private int widthChild; //x-axis of the terrain
    private int heightChild; //z-axis

    private int depthChild; //y-axis

    private float scaleChild;

    [SerializeField] private float offsetXChild;
    [SerializeField] private float offsetYChild;

    public TerrainGeneration parentData;

    public void ChildGenerateTerrain()
    {
        //the first step is to collect all the data present in the parent and store it in each individual child obj
        parentData = transform.parent.GetComponent<TerrainGeneration>();

        terrainResolutionChild = parentData.terrainResolution;
        widthChild = parentData.width;
        heightChild = parentData.height;
        depthChild = parentData.depth;
        scaleChild = parentData.scale;
        offsetXChild = parentData.offsetX;
        offsetYChild = parentData.offsetY;
                       
        //terrain.SetNeighbors(null , null, null, null);

        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = widthChild + 1;
        terrainData.size = new Vector3(widthChild, depthChild, heightChild);

        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[widthChild, heightChild];
        for (int x = 0; x < widthChild; x++)
        {
            for (int y = 0; y < heightChild; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / widthChild * scaleChild + offsetXChild;
        float yCoord = (float)y / heightChild * scaleChild + offsetYChild;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
