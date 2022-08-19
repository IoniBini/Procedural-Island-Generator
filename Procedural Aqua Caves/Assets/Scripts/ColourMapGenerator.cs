using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ColourMapGenerator : MonoBehaviour
{
    //before you start reading, READ THIS:
    //at first, I attempted to create a system for creating colour maps which used the same functions that are used in the terrain generation1
    //to do that, I followed this Sebastian Lague tutorial: https://www.youtube.com/watch?v=RDQK1_SWFuc
    //however, I tried doing it for 5 hours, no hyperbole, and it wouldn't work
    //I took a different approach instead. Once the heights already have been established, a raycast passes through the whole map collecting the heights
    //in a big array, and using this array, it generates the colours depending on the height
    //the old scripts that don't work are inside the legacy folder if you wish to see it still

    //raycast height
    public float rayHeight = 150;
    //the material that is used by the terrain
    public TerrainLayer terrainMat;
    //the material to reset to once you delete the generated colourmap
    public Texture2D defaultTerrainColour;

    [ContextMenu("Generate Colour Map")]
    public void GenerateColourMap()
    {
        //resets your position to directly above the bottom left corner from the terrain
        transform.position = new Vector3(transform.parent.position.x, transform.parent.position.y + rayHeight - 1, transform.parent.position.z);

        var terrainGeneration = GetComponentInParent<TerrainGeneration1>();

        //sets the size of the array where the heights will be stored based on the terrains size
        Color[] colourMap = new Color[terrainGeneration.width * terrainGeneration.height];

        //repeat this process for each meter of the terrain, both in rows and collumns
        for (int i = 0; i <= terrainGeneration.width; i++)
        {
            for (int j = 0; j <= terrainGeneration.height; j++)
            {
                //names the hit point of the raycast
                RaycastHit hit;

                //casts a ray downwards to check if it is currently hitting the floor and not something it should avoid
                Vector3 down = transform.TransformDirection(Vector3.down);
                if (Physics.Raycast(transform.position + new Vector3(0, 0, 1), down, out hit, rayHeight))
                {
                    float currentHeight = hit.distance;
                    //Debug.Log(hit.distance);

                    //normalizes the currently found height by placing it within the range of the start point and the maximum ray height
                    float normalizedHeight = Mathf.InverseLerp(0, rayHeight, currentHeight);
                    //Debug.Log(normalizedHeight);

                    //this process must be repeated for each colour present on the list
                    for (int x = 0; x < terrainGeneration.colourPerHeight.Length; x++)
                    {
                        //you subtract from 1 because you want the ray value to begin from the bottom, not the top, wehre the ray was cast from
                        if (1 - normalizedHeight <= terrainGeneration.colourPerHeight[x].colourDepth)// regions[x].biomeHeight)
                        {
                            //impedes the loop to outgrowing the array boudnries
                            if(j * terrainGeneration.width + i + 1 >= terrainGeneration.width * terrainGeneration.height)
                            {
                                break;
                            }
                            else
                            {
                                //stores the colour that was found
                                colourMap[j * terrainGeneration.width + i] = terrainGeneration.colourPerHeight[x].biomeColour;
                            }

                            //Debug.Log(j * terrainGeneration.width + i);
                            break;
                        }
                    }
                }

                //moves the generator based on its current values within the two for loops
                transform.position = new Vector3(transform.parent.position.x + i, transform.parent.position.y + rayHeight - 1f, transform.parent.position.z + j);
                //Debug.Log("x" + transform.position.x);
            }
        }

        //stores all the values found in the array into the texture, then 
        //sets some important settings to the newly created texture so that it can be assigned to the terrain layer texture correctly
        Texture2D texture = new Texture2D(terrainGeneration.width, terrainGeneration.height, TextureFormat.RGB24, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();

        //at one point I thought that saving the image and assigning it as a png would make the tree placer work better with colours,
        //but it didn't help at all. Still, I was proud that I figured out how to save it as a texture lol
        //so I created a bool so that if you want to save it as a png, you can. It doesn't serve any purpose though
        //I used the link bellow as reference
        //https://answers.unity.com/questions/1331297/how-to-save-a-texture2d-into-a-png.html
        if (terrainGeneration.saveMapAsPng == true)
        {
            //this takes a little bit of time to load, so if it doesn't show up rigth away, wait and reload the assets folder
            byte[] bytes = texture.EncodeToPNG();
            var dirPath = Application.dataPath + "/TerrainImages";
            File.WriteAllBytes(dirPath + "TerrainCapture" + ".png", bytes);
        }    

        //once the array has been made into a texture, the texture is assigned to the layer texture used by the terrain
        terrainMat.diffuseTexture = texture;
        terrainMat.tileSize = new Vector2(terrainGeneration.width, terrainGeneration.height);

        //just to make sure the raycast position has been reset correctly, we reset it at the end, just like we did at the start
        transform.position = new Vector3(transform.parent.position.x, transform.parent.position.y + rayHeight - 1f, transform.parent.position.z);
    }

    //deletes the texture currently stored and puts a chess pattern on it
    [ContextMenu("Clear Material")]
    public void ClearTerrainMaterial()
    {
        terrainMat.diffuseTexture = defaultTerrainColour;
    }
}
