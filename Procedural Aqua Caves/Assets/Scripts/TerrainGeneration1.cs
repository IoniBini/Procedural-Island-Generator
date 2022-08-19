using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class TerrainGeneration1 : MonoBehaviour
{
    //this script is inspired by brackeys: https://www.youtube.com/watch?v=vFvwyu_ZKfU
    //I didnt want to just copy sebastian lagues whole landmass generator video so instead, I will ATTEMPT to take a different approach, where I will mix elements from
    //Brackeys, Sebastian and Lachlans codes so that I turn out with something slightly more original of my part (if "frankensteining" code can be called original)
    //Im doing this not because it will be more efficient, it is exclusively because I want to show that I do know how to code as opposed to just copying

    //to avoid causing errors due to the resolution of the terrain not being in multiples of the power of 2, I have created these variables to multiply with
    //I have limited it to 8 because any more than that will escape the bounds of the noise map max size
    [Range(1, 8)] public int terrainResolution = 1;
    public int gridNumber = 1;

    public bool saveMapAsPng = false;

    //I dont want these to be changed, so I'll hide them from view
    [HideInInspector] public int width = 32; //x-axis of the terrain
    [HideInInspector] public int height = 32; //z-axis

    public TerrainLayer terrainMat;

    public TerrainType[] biomes;
    public ColourPerHeight[] colourPerHeight;

    [Header("Sea Configs")]
    public Color seaColor = Color.blue;
    public float seaHeight = 0f;
    private ColorGrading _colorGrading;

    [Header("Tree Configs")]
    public bool destroyTreesOnGenerate = false;
    public Vector2 Start;
    public int DebugSpawnCount;
    public Vector2 Size = new Vector2(1000f, 1000f);
    public float MaxHeight = 300f; //this is my terrain height
    public LayerMask ValidLayers;
    [Range(0f, 1f)] public float SizeVariance = 0.1f;

    public TreePerHeight[] treePerHeight;

    private int maxDepth = 100; //y-axis
    private float scale = 20f;
    private float depthModifier;

    //[SerializeField]
    //public static float[,] falloffArray;

    private float offsetX = 0f;// equivalent to the width
    private float offsetY = 0f;// equivalent to the height

    private int biomeType;

    [ContextMenu("Maximise Tree Placement Area")]
    public void MaximiseTreePlacementArea()
    {
        //resets the position and size of the tree generator
        Start = new Vector2(transform.position.x, transform.position.z);
        MaxHeight = 300f;

        var tempWidth = 32;
        var tempHeight = 32;

        for (int i = 0; i < terrainResolution - 1; i++)
        {
            tempWidth *= 2;
            tempHeight *= 2;
        }

        Size = new Vector2(tempWidth, tempHeight);
    }

    [ContextMenu("Generate Everything")]
    private void GenerateEverything()
    {
        TerrainPrep();
        MaximiseTreePlacementArea();
        GenerateColourMapParent();
        GenerateTreesParent();
    }

    [ContextMenu("Only Generate Terrain")]
    private void TerrainPrep()
    {
        //delete the previous stuff prior to commencing
        ClearEverything();

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

        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width;
        terrainData.size = new Vector3(width, maxDepth, height);

        for (int y = 0; y < gridNumber; y++)
        {
            for (int x = 0; x < gridNumber; x++)
            {
                int totalChances = 0;

                for(int i = 0; i < biomes.Length; i++)
                {
                    totalChances = totalChances + biomes[i].spawnChance;
                }

                biomeType = Random.Range(0, totalChances);

                ApplyBiomeModifiers(totalChances);

                offsetX = Random.Range(0f, 9999f);
                offsetY = Random.Range(0f, 9999f);

                //the first two floats show where the heights begin being calculated on the terrain plane, the generate heights determines the noise
                //itself as well as how much area is being calculated starting from the previously established points
                terrainData.SetHeights(width / gridNumber * x, height / gridNumber * y, GenerateHeights());
            }
        }
        return terrainData;
    }

    public void ApplyBiomeModifiers(int totalChance)
    {
        //a known limitation of this method of handling randomness is that the first item in the list will ALWAYS take priority if
        //two or more chance variables are the exact same value, but should work otherwise
        int currentChance = 0;

        //change scale, depth and chance to spawn for each biome type
        for (int x = 0; x < biomes.Length; x++)
        {
            if(currentChance + biomes[x].spawnChance >= biomeType)
            {
                //Debug.Log("current x: " + x);
                //Debug.Log("current chance: " + currentChance);
                //Debug.Log("chosen biome: " + biomeType);

                depthModifier = biomes[x].biomeDepth;
                scale = biomes[x].biomeScale;
                break;
            }
            else
            {
                continue;
            }
        }
    }

    float[,] GenerateHeights()
    {
        #region Falloff Map Generator
        //https://www.youtube.com/watch?v=COmtTyLCd6I
        var size = (width) / gridNumber;
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

        //the heights array represents how much area is being calcuated
        float[,] heights = new float[(width) / gridNumber, (height) / gridNumber];
        for (int x = 0; x < (width) / gridNumber; x++)
        {
            for (int y = 0; y < (height) / gridNumber; y++)
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

    [ContextMenu("Only Generate Colourmap")]
    private void GenerateColourMapParent()
    {
        var colourMapGenerator = GetComponentInChildren<ColourMapGenerator>();
        colourMapGenerator.GenerateColourMap();
    }

    [ContextMenu("Only Generate Trees")]
    private void GenerateTreesParent()
    {
        var treeGenerator = GetComponentInChildren<TreePlacer2>();
        treeGenerator.SpawnDebug();
    }

    [ContextMenu("Clear Everything")]
    private void ClearEverything()
    {
        var tempDepth = maxDepth;
        maxDepth = 0;

        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);

        maxDepth = tempDepth;

        var colourMapGenerator = GetComponentInChildren<ColourMapGenerator>();
        colourMapGenerator.ClearTerrainMaterial();
        var treeGenerator = GetComponentInChildren<TreePlacer2>();
        treeGenerator.ClearObjects();
    }

    [ContextMenu("Only Clear Terrain")]
    private void ClearTerrain()
    {
        var colourMapGenerator = GetComponentInChildren<ColourMapGenerator>();
        colourMapGenerator.ClearTerrainMaterial();
    }

    [ContextMenu("Only Clear Colourmap")]
    private void ClearColourmap()
    {
        var colourMapGenerator = GetComponentInChildren<ColourMapGenerator>();
        colourMapGenerator.ClearTerrainMaterial();
    }

    [ContextMenu("Only Clear Trees")]
    private void ClearTrees()
    {
        var treeGenerator = GetComponentInChildren<TreePlacer2>();
        treeGenerator.ClearObjects();
    }

    [ContextMenu("Update Sea Variables")]
    public void UpdateSeaVariables()
    {
        var seaObj = transform.Find("Sea").gameObject;
        seaObj.transform.position = new Vector3(seaObj.transform.position.x, seaHeight, seaObj.transform.position.z);
        seaObj.GetComponent<PostProcessVolume>().profile.TryGetSettings(out _colorGrading);
        _colorGrading.colorFilter.value = seaColor;

        if (Application.isPlaying) seaObj.GetComponent<Renderer>().material.color = seaColor;
        else if (Application.isEditor) seaObj.GetComponent<Renderer>().sharedMaterial.color = seaColor;
    }

    [System.Serializable]
    public struct TerrainType
    {
        public string biomeName;
        [MinMax(1, 1000)]public int spawnChance;
        [Range(0f, 1f)]public float biomeDepth;
        public float biomeScale;
    }

    [System.Serializable]
    public struct ColourPerHeight
    {
        [Range(0f, 1f)] public float colourDepth;
        public Color biomeColour;
    }

    [System.Serializable]
    public struct TreePerHeight
    {
        [Range(0f, 1f)] public float treeDepth;
        [Range(0f, 1f)] public float spawnChance;
        public GameObject[] biomeTrees;
    }
}
