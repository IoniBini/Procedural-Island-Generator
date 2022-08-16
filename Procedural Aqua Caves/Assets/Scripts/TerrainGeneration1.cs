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

    private int depth = 20; //y-axis
    private float scale = 20f;

    public int depthModifier;

    private float offsetX = 0f;// equivalent to the width
    private float offsetY = 0f;// equivalent to the height

    private float storeOffsetX;
    private float storeOffsetY;

    public int biomeType;
    //1 = forest
    //2 = sea
    //3 = mountain
    //4 = dessert

    [Header("Dessert Biome Variables")]
    public int desertDepth = 1;
    public float desertScale = 1f;
    [Header("Forest Biome Variables")]
    public int forestDepth = 1;
    public float forestScale = 1f;
    [Header("Mountain Biome Variables")]
    public int mountainDepth = 1;
    public float mountainScale = 1f;
    [Header("Sea Biome Variables")]
    public int seaDepth = 1;
    public float seaScale = 1f;


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

        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width;
        terrainData.size = new Vector3(width, depth, height);

        for (int y = 0; y < terrainRows; y++)
        {
            for (int x = 0; x < terrainCollumns; x++)
            {
                biomeType = Random.Range(1, 4);

                StartCoroutine(ApplyBiomeModifiers());

                //the first two floats show where the heights begin being calculated on the terrain plane, the generate heights determines the noise
                //itself as well as how much area is being calculated starting from the previously established points
                terrainData.size = new Vector3(width, depth, height);
                terrainData.SetHeights(width / terrainCollumns * x, height / terrainRows * y, GenerateHeights());

                //storeOffsetX += (width + 1) / terrainCollumns;
                //Debug.Log("offsetX = " + storeOffsetX);
            }

            //storeOffsetY =+ height / terrainRows;
            //Debug.Log("offsetY = " + storeOffsetY);
        }


        return terrainData;
    }

    IEnumerator ApplyBiomeModifiers()
    {
        //change scale, depth and chance to spawn for each biome type
        switch (biomeType)
        {
            //1 = forest
            //2 = sea
            //3 = mountain
            //4 = dessert

            case 1:
                depth = forestDepth;
                scale = forestScale;
                break;
            case 2:
                depth = seaDepth;
                scale = seaScale;
                break;
            case 3:
                depth = mountainDepth;
                scale = mountainScale;
                break;
            case 4:
                depth = desertDepth;
                scale = desertScale;
                break;
        }

        yield return new WaitForSeconds(.1f);
    }

    float[,] GenerateHeights()
    {
        //heights represents how much area is being calcuated
        float[,] heights = new float[(width) / terrainCollumns, (height) / terrainRows];
        for (int x = 0; x < (width) / terrainCollumns; x++)
        {
            for (int y = 0; y < (height) / terrainRows; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }   

    float CalculateHeight(int x, int y)
    {
        //and to ensure that the transition is smooth, we can add onto the offset values, but its not correct atm

        float xCoord = ((x + offsetX) / width * scale);
        float yCoord = ((y + offsetY) / height * scale);

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
