using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration1 : MonoBehaviour
{
    //this script is inspired by brackeys: https://www.youtube.com/watch?v=vFvwyu_ZKfU
    //I didnt want to just copy sebastian lagues whole landmass generator video so instead, I will ATTEMPT to take a different approach, where I will mix elements from
    //Brackeys, Sebastian and Lachlans codes so that I turn out with something slightly more original of my part (if "frankensteining" code can be called original)
    //Im doing this not because it will be more efficient, it is exclusively because I want to show that I do know how to code as opposed to just copying

    //to avoid causing errors due to the resolution of the terrain not being in multiples of the power of 2, I have created these variables to multiply with
    //I have limited it to 8 because any more than that will escape the bounds of the noise map max size
    [Range(1, 8)] public int terrainResolution = 1;
    public int terrainRows = 1;
    public int terrainCollumns = 1;

    //I dont want these to be changed, so I'll hide them from view
    [HideInInspector] public int width = 32; //x-axis of the terrain
    [HideInInspector] public int height = 32; //z-axis

    public int depth = 20; //y-axis

    public float scale = 20f;

    private float offsetX = 0f;// equivalent to the width
    private float offsetY = 0f;// equivalent to the height

    private float storeOffsetX;
    private float storeOffsetY;

    [ContextMenu("GenerateTerrain")]
    private void TerrainPrep()
    {
        //delete the previous stuff prior to commencing
        ClearTerrain();

        //reset the x and y values because they might have been modified by the resolution
        width = 32;
        height = 32;

        //pick a new offset to make sure the noise is random each time
        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);

        storeOffsetX = 0f;
        storeOffsetY = 0f;

        //adjusts the size to the appropriate resolution
        for (int i = 0; i < terrainResolution - 1; i++)
        {
            width *= 2;
            height *= 2;
        }

        //newTerrain.transform.position = new Vector3(transform.position.x + width * x, transform.position.y, transform.position.z + width * y);
        //Debug.Log(offsetX);
        //offsetX += height;

        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);

        //for (int y = 0; y < terrainRows; y++)
        {
            for (int x = 0; x < terrainCollumns; x++)
            {
                //the first two floats show where the heights begin being calculated on the terrain plane, the generate heights determines the noise
                //itself as well as how much area is being calculated starting from the previously established points
                terrainData.SetHeights(width / terrainCollumns * x, height / terrainRows, GenerateHeights());


                storeOffsetX += (width + 1) / terrainCollumns;
                Debug.Log("offsetX = " + offsetX);
            }

            //storeOffsetY =+ height / terrainRows;
            //Debug.Log("offsetY = " + storeOffsetY);
        }
        
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        //heights represents how much area is being calcuated
        float[,] heights = new float[(width + 1) / terrainCollumns, (height + 1) / terrainRows];
        for (int x = 0; x < (width + 1) / terrainCollumns; x++)
        {
            for (int y = 0; y < (height + 1) / terrainRows; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        //for each time this function is called, I can make it roll a dice, and depending on the result, it sets different types of parameter
        //so that different types of biomes can be established
        //and to ensure that the transition is smooth, we can add onto the offset values, but its not correct atm

        float xCoord = (x + offsetX) / width * scale;
        float yCoord = (y + offsetY) / height * scale;

        //Debug.Log(" offsetY = " + yCoord);

        /*if (x == 0 && y == 0)
        {
            Debug.Log("BottomLeft: X = " + xCoord + " Y = " + yCoord);
        }

        if (x == widthChild && y == heightChild)
        {
            Debug.Log("TopRight: X = " + xCoord + " Y = " + yCoord);
        }
        if (y == 0)
        {
            Debug.Log("BottomXValues: X = " + xCoord + " Y = " + yCoord);
        }*/

        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    [ContextMenu("ClearTerrain")]
    private void ClearTerrain()
    {
        var tempDepth = depth;
        depth = 0;

        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);

        depth = tempDepth;
    }
}
