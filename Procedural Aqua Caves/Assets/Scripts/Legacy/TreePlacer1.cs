using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlacer1 : MonoBehaviour
{
    //THIS SCRIPT IS A LEGACY ONE, IT DOES NOT WORK AND I ONLY KEPT IT IN THE PROJECT FOR REFERENCE IN THE FUTURE


    public Texture2D test;

    [Header("Positioning")]
    private Vector2 Start;
    public Vector2 Size = new Vector2(1000f, 1000f);
    public float MaxHeight = 600f; //this is my terrain height
    public LayerMask ValidLayers;

    [Header("Appearance")]
    [Range(0f, 1f)] public float SizeVariance = 0.1f;

    [Header("Debug")]
    public int DebugSpawnCount = 1000;

    [ContextMenu("Spawn Debug")]
    public void SpawnDebug()
    {
        Spawn(DebugSpawnCount);
    }

    public void Spawn(int count)
    {
        Start = new Vector2(transform.position.x, transform.position.z);

        for (var i = 0; i < count; i++)
        {
            //We shine a virtual laster from a start point
            var rayPos = new Vector3(Start.x + Size.x * Random.value, MaxHeight, Start.y + Size.y * Random.value);

            //If that laser hits nothing, we stop here
            if (!Physics.Raycast(rayPos, Vector3.down, out var hit, MaxHeight)) continue;

            //If it hit an object on an invalid layer, we also stop here
            if (!MaskContainsLayer(ValidLayers, hit.collider.gameObject.layer)) continue;

            if (hit.collider.GetComponent<Terrain>() == false)
            {
                Renderer renderer = hit.collider.GetComponent<MeshRenderer>();
                Texture2D texture = renderer.material.mainTexture as Texture2D;
                Vector2 pCoord = hit.textureCoord;
                pCoord.x *= texture.width;
                pCoord.y *= texture.height;
                Vector2 tiling = renderer.material.mainTextureScale;
                Color color = texture.GetPixel(Mathf.FloorToInt(pCoord.x * tiling.x), Mathf.FloorToInt(pCoord.y * tiling.y));
                color.r = Mathf.Round(color.r * 1000f) * 0.001f;
                color.g = Mathf.Round(color.g * 1000f) * 0.001f;
                color.b = Mathf.Round(color.b * 1000f) * 0.001f;
                color.a = Mathf.Round(color.a * 1000f) * 0.001f;

                PlaceTreeAt(hit.point, color);

                Debug.Log(color);
            }
            else
            {
                //https://www.youtube.com/watch?v=YX8E1AE3BYs

                Terrain terrain = hit.collider.GetComponent<Terrain>();
                TerrainLayer layer = terrain.terrainData.terrainLayers[0];
                Vector3 terrainPosition = hit.point - terrain.transform.position;
                Vector3 splatMapPosition = new Vector3(
                    terrainPosition.x / terrain.terrainData.size.x,
                    0,
                    terrainPosition.x / terrain.terrainData.size.z
                    );

                int x = Mathf.FloorToInt(splatMapPosition.x * layer.diffuseTexture.width);
                int y = Mathf.FloorToInt(splatMapPosition.x * layer.diffuseTexture.height);

                Texture2D texture = layer.diffuseTexture;
                /*Vector2 pCoord = hit.textureCoord;
                pCoord.x *= texture.width;
                pCoord.y *= texture.height;
                Vector2 tiling = layer.diffuseTexture.texelSize;*/
                Color color = texture.GetPixel(x, y);
                color.r = Mathf.Round(color.r * 1000f) * 0.001f;
                color.g = Mathf.Round(color.g * 1000f) * 0.001f;
                color.b = Mathf.Round(color.b * 1000f) * 0.001f;
                color.a = Mathf.Round(color.a * 1000f) * 0.001f;//never forget to max the alpha in the color picker
                //Debug.Log("color PRE calculation: " + color);

                //Debug.Log("current color index: " + FindIndexFromColor(color));

                //But if it *does* hit something, a bunch of information about the ray hit is stored in the 'hit' variable
                //if (FindIndexFromColor(color) >= 0) PlaceTreeAt(hit.point, FindIndexFromColor(color));
                PlaceTreeAt(hit.point, color);
            }


        }
    }

    //Given a position that we successfully touch the terrain at, 
    public void PlaceTreeAt(Vector3 position, Color foundColor)
    {
        var terrainGeneration = GetComponentInParent<TerrainGeneration1>();

        for (int i = 0; i < terrainGeneration.colourPerHeight.Length; i++)
        {
            //Debug.Log("test");

            Color arrayColor = terrainGeneration.colourPerHeight[i].biomeColour;
            arrayColor.r = Mathf.Round(arrayColor.r * 1000f) * 0.001f;
            arrayColor.g = Mathf.Round(arrayColor.g * 1000f) * 0.001f;
            arrayColor.b = Mathf.Round(arrayColor.b * 1000f) * 0.001f;
            arrayColor.a = Mathf.Round(arrayColor.a * 1000f) * 0.001f;

            if (foundColor == arrayColor)
            {
                Debug.Log(foundColor);

                //if (terrainGeneration.colourPerHeight[i].biomeTrees[0] == null)
                {
                    //break;
                }

                //Spawns a random prefab at the provided position - and gives it a random rotation and scale
                /*var newObj = Instantiate(terrainGeneration.colourPerHeight[i].biomeTrees[Random.Range(0, terrainGeneration.colourPerHeight[i].biomeTrees.Length)]);
                newObj.transform.parent = transform;
                newObj.transform.position = position;
                newObj.transform.eulerAngles = new Vector3(0f, Random.value * 360f, 0f);
                newObj.transform.localScale = Vector3.one * Random.Range(1f - SizeVariance, 1f + SizeVariance);*/

                break;
            }
        }              

        //We need to do this to make sure that our new object will be seen by other raycasts etc. this frame
        Physics.SyncTransforms();
    }

    [ContextMenu("Clear Objects")]
    public void ClearObjects()
    {
        //We iterate backwards through children, since if we go forwards we end up changing child indices
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            //DestroyImmediate is for edit-mode, Destroy is for play-mode (including in the built application)
            if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
            else DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    //I swear I've rewritten this function so many times, so now I just copy-paste it whenever I need it
    //Don't worry if you don't understand it, bitwise coding is used for basically nothing other than this specific purpose
    public static bool MaskContainsLayer(LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }

    private int FindIndexFromColor(Color color)
    {
        var terrainGeneration = GetComponentInParent<TerrainGeneration1>();

        for (int i = 0; i < terrainGeneration.colourPerHeight.Length; i++)
        {
            //Debug.Log("found color = " + color.ToString());
            //Debug.Log("stored color = " + terrainGeneration.colourPerHeight[i].biomeColour.ToString());

            Color roundedColor = terrainGeneration.colourPerHeight[i].biomeColour;
            roundedColor.r = Mathf.Round(color.r * 1000f) * 0.001f;
            roundedColor.g = Mathf.Round(color.g * 1000f) * 0.001f;
            roundedColor.b = Mathf.Round(color.b * 1000f) * 0.001f;
            roundedColor.a = Mathf.Round(color.a * 1000f) * 0.001f;

            //Debug.Log("color from the array: " + roundedColor);
            //Debug.Log("color that was found: " + color);

            if (roundedColor == color)//THE ISSUE IS HERE, BECAUSE IT ONLY EVER ACCEPTS THE FIRST VALUE IN TEH ARRAY
            {
                Debug.Log("colour " + i + " found");
                return i;
            }
            else
            {
                Debug.Log("invalid colour");
                continue;
            }
        }

        return -1;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color32(0, 255, 0, 100);
        Gizmos.DrawCube(new Vector3(transform.position.x - Size.x / 2 * -1, transform.position.y, transform.position.z + Size.y / 2), new Vector3(Size.x, MaxHeight, Size.y));
    }

}
