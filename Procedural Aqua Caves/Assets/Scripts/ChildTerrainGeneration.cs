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
        float[,] heights = new float[widthChild + 1, heightChild + 1];
        for (int x = 0; x < widthChild + 1; x++)
        {
            for (int y = 0; y < heightChild + 1; y++)
            {
                heights[x, y] = CalculateHeight(x, y);

            }
        }

        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        //the first one becomes 1 1 bc x and y start at exactly 32, hence the corners are workign bc they are perfect multiples of 32, but the midpoints are not
        float xCoord = ((float)x + offsetXChild) / widthChild * scaleChild;
        float yCoord = ((float)y + offsetYChild) / heightChild * scaleChild;

        if (x == 0 && y == 0)
        {
            Debug.Log("BottomLeft: X = " + xCoord + " Y = " + yCoord);
        }

        if (x == widthChild && y == heightChild)
        {
            Debug.Log("TopRight: X = " + xCoord + " Y = " + yCoord);
        }

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
