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

    public TerrainLayer terrainMat;

    public TerrainType[] regions;

    private int maxDepth = 100; //y-axis
    private float scale = 20f;
    private float depthModifier;

    //[SerializeField]
    //public static float[,] falloffArray;

    private float offsetX = 0f;// equivalent to the width
    private float offsetY = 0f;// equivalent to the height

    private int biomeType;
    //1 = forest
    //2 = sea
    //3 = mountain
    //4 = dessert

    [Header("Dessert Biome Variables")]
    [Range(0f, 2f)]public float desertDepth = 1f;
    public float desertScale = 1f;
    [Header("Forest Biome Variables")]
    [Range(0f, 2f)] public float forestDepth = 1f;
    public float forestScale = 1f;
    [Header("Mountain Biome Variables")]
    [Range(0f, 2f)] public float mountainDepth = 1f;
    public float mountainScale = 1f;
    [Header("Sea Biome Variables")]
    [Range(0f, 2f)] public float seaDepth = 1f;
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

        //adjusts the size to the appropriate resolution
        for (int i = 0; i < terrainResolution - 1; i++)
        {
            width *= 2;
            height *= 2;
        }

        //StartCoroutine(GenerateColourMap());

        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width;
        terrainData.size = new Vector3(width, maxDepth, height);

        for (int y = 0; y < terrainRows; y++)
        {
            for (int x = 0; x < terrainCollumns; x++)
            {
                biomeType = Random.Range(1, 4);

                StartCoroutine(ApplyBiomeModifiers());

                offsetX = Random.Range(0f, 9999f);
                offsetY = Random.Range(0f, 9999f);

                //the first two floats show where the heights begin being calculated on the terrain plane, the generate heights determines the noise
                //itself as well as how much area is being calculated starting from the previously established points
                terrainData.SetHeights(width / terrainCollumns * x, height / terrainRows * y, GenerateHeights());
            }
        }
        return terrainData;
    }

    IEnumerator GenerateColourMap()
    {
        Color[] colourMap = new Color[width * height];
        float[,] heights = new float[(width), (height)];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                
                heights[x, y] = ((CalculateHeight(x, y) * depthModifier));// - map[x, y]);

                float currentHeight = heights[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].biomeHeight)
                    {
                        colourMap[y * width + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }


        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        terrainMat.diffuseTexture = texture;
        terrainMat.tileSize = new Vector2(width, height);

        yield return new WaitForSeconds(.1f);
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
                depthModifier = forestDepth;
                scale = forestScale;
                break;
            case 2:
                depthModifier = seaDepth;
                scale = seaScale;
                break;
            case 3:
                depthModifier = mountainDepth;
                scale = mountainScale;
                break;
            case 4:
                depthModifier = desertDepth;
                scale = desertScale;
                break;
        }

        yield return new WaitForSeconds(.1f);
    }

    float[,] GenerateHeights()
    {
        #region Falloff Map Generator
        //https://www.youtube.com/watch?v=COmtTyLCd6I
        var size = (width) / terrainCollumns;
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        static float Evaluate(float value)
        {
            float a = 3f;
            float b = 4f;

            return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
        }
        #endregion

        //Color[] colourMap = new Color[width * height];

        //the heights array represents how much area is being calcuated
        float[,] heights = new float[(width) / terrainCollumns, (height) / terrainRows];
        for (int x = 0; x < (width) / terrainCollumns; x++)
        {
            for (int y = 0; y < (height) / terrainRows; y++)
            {
                heights[x, y] = ((CalculateHeight(x, y)) * depthModifier - map[x, y]);
                float currentHeight = heights[x, y];
            }
        }

        return heights;
    }  

    float CalculateHeight(int x, int y)
    {
        float xCoord = ((x + offsetX) / width * scale);
        float yCoord = ((y + offsetY) / height * scale);

        //Debug.Log("X" + xCoord + " Y" + yCoord);

        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    [ContextMenu("ClearTerrain")]
    private void ClearTerrain()
    {
        var tempDepth = maxDepth;
        maxDepth = 0;

        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);

        maxDepth = tempDepth;
    }

    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        [Range(0f, 1f)]public float biomeHeight;
        public Color colour;
    }
}
