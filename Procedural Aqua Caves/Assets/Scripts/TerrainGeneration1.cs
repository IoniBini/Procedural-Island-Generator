using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class TerrainGeneration1 : MonoBehaviour
{
    //this script is an amalgamation of a bunch of references around the internet. Each time I use somehting I didn't create, I put a link to it
    //it is also worth noting that although this procedural generator works both in run and edit time, if you want to play the level in run time
    //then you have to generate the level again DURING playtime, otherwise the texture and the sea colour will disappear

    //variables to multiply width and height. I have limited it to 8 because any more than that will escape the bounds of the noise map max size
    [Range(1, 8)] public int terrainResolution = 1;
    //the number of times the terrain is divided into islands, both in rows and columns symmetrically 
    public int gridNumber = 1;

    //if you wish to save the current map as a png, tick this to true
    public bool saveMapAsPng = false;

    //The minimum width and height of the terrain. I dont want these to be changed, so I'll hide them from view
    [HideInInspector] public int width = 32; //x-axis of the terrain
    [HideInInspector] public int height = 32; //z-axis

    //the exposed material the terrain uses
    public TerrainLayer terrainMat;

    //a struct containing all the terrain mesh generation
    public TerrainType[] biomes;
    //a struct containing all the colour map generator variables
    public ColourPerHeight[] colourPerHeight;

    [Header("Sea Configs")]
    //self explenatory variables
    public Color seaColor = Color.blue;
    public float seaHeight = 0f;
    public ColorGrading _colorGrading;

    [Header("Tree Configs")]
    //more self explenatory variables
    public bool destroyTreesOnGenerate = false;
    public Vector2 Start;
    public int DebugSpawnCount;
    public Vector2 Size = new Vector2(1000f, 1000f);
    public float MaxHeight = 300f;
    public LayerMask ValidLayers;
    [Range(0f, 1f)] public float SizeVariance = 0.1f;
    //the extras factor causes more extras to be spawned in relation to trees, multiplied by this int bellow
    [UnityEngine.Min(2)] public int extrasFactor;

    //a struct containing all the tree spawning variables
    public TreePerHeight[] treePerHeight;

    //variables for generating the terrain mesh
    private int maxDepth = 100; //y-axis
    private float scale = 20f;
    private float depthModifier;

    private float offsetX = 0f;// equivalent to the width
    private float offsetY = 0f;// equivalent to the height

    //when randomly picking which type of terrain to create in the mesh, this is the variable to be set
    private int biomeType;

    public void Awake()
    {
        //for play mode, do this so that it generates in the exe.
        GenerateEverything();
    }

    //if you mess up the alignment of the tree placer, use this to reset it to maximum and default position
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

    //sets ALL of the generators off at the same time, AND CRUCIALLY, it has to be in this SPECIFIC order
    [ContextMenu("Generate Everything")]
    private void GenerateEverything()
    {
        TerrainPrep();
        MaximiseTreePlacementArea();
        GenerateColourMapParent();
        GenerateTreesParent();
        UpdateSeaVariables();
    }

    //bellow there are several context menus which execute each function individually in case you'd like to debug something

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
        //set up the terrain scale correctly
        terrainData.heightmapResolution = width;
        terrainData.size = new Vector3(width, maxDepth, height);

        //repeat the terrain generation process for each island in the grid, both in rows and in collumns
        for (int y = 0; y < gridNumber; y++)
        {
            for (int x = 0; x < gridNumber; x++)
            {
                //this CONVOLUTED thing bellow randomly chooses what type of biome to is to be spawned from the list
                //there are two known limitations to it:
                //1 - the chance variables must grow from small to large, starting from the top to the bottom of the list
                //2 - two items in the list CANNOT have the same chance number
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
        int currentChance = 0;

        //change scale and depth for each biome type
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
        //the falloff map generator is STRAIGHT UP COPIED from this video from Sebastian Lague, I only lightly changed it so that it all
        //fits within one script
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

        //the heights array represents how much area is being calcuated within the total area of the terrain
        float[,] heights = new float[(width) / gridNumber, (height) / gridNumber];

        //this process is repeated for each island in the grid, moving up in the x and z axis as it goes, so that islands don't generate on top of each other
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
        //takes into account the randomly picked offset as well as the scale of the texture
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

    //just like I did with the generation stuff, I have also made multiple, separate, context menus for each of the clearing functions,
    //again, for the sake of being able to debug if need be, as well as one that clears everything in one go

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
        //purely visual, has no impact in the code
        public string biomeName;
        //the higher, the more likely for this terrain to spawn
        [UnityEngine.Min(1)] public int spawnChance;
        //how high up and down the terrain can go
        [Range(0f, 1f)]public float biomeDepth;
        //how much area from the perlin noie is being used, the more, the more waves
        public float biomeScale;
    }

    [System.Serializable]
    public struct ColourPerHeight
    {
        //the lower, the closer to the floor the colour will be, and vice versa
        [Range(0f, 1f)] public float colourDepth;
        //the aforementioned colour to be placed in the set height
        public Color biomeColour;
    }

    [System.Serializable]
    public struct TreePerHeight
    {
        //the lower, the closer to the floor the tree will be, and vice versa
        [Range(0f, 1f)] public float treeDepth;
        //I made this variable because even though you can choose to spawn less trees in the debug count, if you want to individually
        //choose what trees in the list spawn more or less often, you can use the variable bellow
        [Range(0f, 1f)] public float spawnChance;
        //the trees that are to be spawned in the given location
        public GameObject[] biomeTrees;
        //the extra stuff that is to be spawned in the given location
        public GameObject[] biomeExtras;
    }
}
