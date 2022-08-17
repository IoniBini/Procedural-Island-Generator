using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourMapGenerator : MonoBehaviour
{
    public float rayHeight = 100;
    public TerrainLayer terrainMat;
    public TerrainType[] regions;

    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        [Range(0f, 1f)] public float biomeHeight;
        public Color colour;
    }

    [ContextMenu("Generate Colour Map")]
    public void GenerateColourMap()
    {
        transform.position = new Vector3(transform.parent.position.x, transform.parent.position.y + rayHeight - 1, transform.parent.position.z);

        var terrainGeneration = GetComponentInParent<TerrainGeneration1>();

        Color[] colourMap = new Color[terrainGeneration.width * terrainGeneration.height];

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

                    float normalizedHeight = Mathf.InverseLerp(0, rayHeight, currentHeight);
                    //Debug.Log(normalizedHeight);

                    for (int x = 0; x < regions.Length; x++)
                    {
                        if (1 - normalizedHeight <= regions[x].biomeHeight)
                        {
                            colourMap[j * terrainGeneration.width + i] = regions[x].colour;
                            break;
                        }
                    }
                }

                transform.position = new Vector3(transform.parent.position.x + i, rayHeight - 1, transform.parent.position.y + j);
                //Debug.Log("x" + transform.position.x);
            }
        }

        Texture2D texture = new Texture2D(terrainGeneration.width, terrainGeneration.height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        terrainMat.diffuseTexture = texture;
        terrainMat.tileSize = new Vector2(terrainGeneration.width, terrainGeneration.height);

        transform.position = new Vector3(transform.parent.position.x, transform.parent.position.y + rayHeight - 1f, transform.parent.position.z);
    }
}
