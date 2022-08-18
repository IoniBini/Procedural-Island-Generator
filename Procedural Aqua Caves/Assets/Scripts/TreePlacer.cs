using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlacer : MonoBehaviour
{
    public TerrainLayer terrainMat;
    //public Texture2D pngTerrainTexture;

    public bool destroyTreesOnGenerate = true;

    //arrays for what you want to populate the scene with
    public GameObject[] tropicalTrees;
    public GameObject[] tundraTrees;
    public GameObject[] desertTrees;
    public GameObject[] seaTrees;

    //the latest spawned obj
    GameObject spawnedObj;

    //a variable that the lower it is, the less likley it is for a tree to spawn, and the higher, the more likely
    [Range(1, 10)]
    public int spawnChanceDesert;
    [Range(1, 10)]
    public int spawnChanceTundra;
    [Range(1, 10)]
    public int spawnChanceTropical;
    [Range(1, 10)]
    public int spawnChanceSea;

    //sliders for the amount of objs you wish to spawn in the x and z axis
    [Range(1, 1000)]
    public int treesPerRow;
    [Range(1, 1000)]
    public int treesPerColumn;
    [Range(10, 1000)]
    public int raycastHeight;

    [ContextMenu("Maximise Tree Placement Area")]
    public void MaximiseTreePlacementArea()
    {
        //resets the position of the tree generator
        //transform.position = new Vector3(transform.parent.position.x, transform.parent.position.y + raycastHeight - 1, transform.parent.position.z);

        var terrainGeneration = GetComponentInParent<TerrainGeneration1>();

        treesPerRow = terrainGeneration.width;
        treesPerColumn = terrainGeneration.height;
    }

    [ContextMenu("DeleteTrees")]
    void DeleteTrees()
    {
        //checks to see if the trees have already been generated, and if yes, clears all the old ones
        if (GameObject.FindGameObjectWithTag("Tree") != null && destroyTreesOnGenerate == true)
        {
            var oldTrees = GameObject.FindGameObjectsWithTag("Tree");
            int objsNumber = oldTrees.Length;

            for (int i = 0; i < objsNumber; i++)
            {
                DestroyImmediate(oldTrees[i]);
            }
        }
    }

    [ContextMenu("SpawnTree")]
    void SpawnTree()
    {
        //checks to see if the trees have already been generated, and if yes, clears all the old ones
        if (GameObject.FindGameObjectWithTag("Tree") != null)
        {
            var oldTrees = GameObject.FindGameObjectsWithTag("Tree");
            int objsNumber = oldTrees.Length;

            for (int i = 0; i < objsNumber; i++)
            {
                DestroyImmediate(oldTrees[i]);
            }
        }

        //resets the position of the tree generator
        //transform.position = new Vector3(transform.parent.position.x, transform.parent.position.y + raycastHeight - 1, transform.parent.position.z);

        //stores the current position of the spawner so that it can be reset later
        var resetAllAxis = transform.position;

        //a 2d for loop, the first is for the collumns, the second is for the rows
        for (int j = 0; j <= treesPerColumn; j++)
        {
            for (int i = 0; i <= treesPerRow; i++)
            {
                //int layerMask = 1 << 8;
                //layerMask = ~layerMask;

                //names the hit point of the raycast
                RaycastHit hit;

                //casts a ray downwards to check if it is currently hitting the floor and not something it should avoid
                Vector3 down = transform.TransformDirection(Vector3.down);
                if (Physics.Raycast(transform.position + new Vector3(0, 0, 1), down, out hit, raycastHeight))
                {
                    //layer 8 contains all the things that the raycast should avoid, hence it will only spawn anythig if it is not in that layer
                    if (hit.transform.gameObject.layer != 8)
                    {
                        //Renderer renderer = hit.transform.GetComponent<MeshRenderer>();
                        Vector2 pixelUV = hit.textureCoord;
                        //Debug.Log(hit.textureCoord);

                        var objToBeSpawned = desertTrees[Random.Range(0, desertTrees.Length)];
                        spawnedObj = Instantiate(objToBeSpawned, hit.point, Quaternion.identity) as GameObject;

                        /*pixelUV.x *= pngTerrainTexture.width;
                        pixelUV.y *= pngTerrainTexture.height;
                        Vector2 tiling = pngTerrainTexture.texelSize;
                        Color color = pngTerrainTexture.GetPixel(Mathf.FloorToInt(pixelUV.x * tiling.x), Mathf.FloorToInt(pixelUV.y * tiling.y));
                        Debug.Log("blue: " + color.b);
                        Debug.Log("green: " + color.g);
                        Debug.Log("red: " + color.r);
                        //Debug.Log(color.ToString());


                        int index = FindIndexFromColor(color);

                        //depending on what color is found, a different array of prefabs is chosen to be spawned from
                        if (index == 0)
                        {
                            var chanceToSpawn = Random.Range(spawnChanceDesert, 10);

                            //if (chanceToSpawn <= spawnChanceDesert)
                            {
                                //Debug.Log("spawn complete");

                                var objToBeSpawned = desertTrees[Random.Range(0, desertTrees.Length)];
                                spawnedObj = Instantiate(objToBeSpawned, spawnPosition, Quaternion.identity) as GameObject;
                                spawnedObj.transform.position = hit.point;
                                var objRandomScale = Random.Range(0.5f, 1.5f);
                                spawnedObj.transform.localScale = new Vector3(objRandomScale, objRandomScale, objRandomScale);
                                var objRandomRotationX = Random.Range(-10f, 10f);
                                var objRandomRotationY = Random.Range(0f, 360f);
                                var objRandomRotationZ = Random.Range(-10f, 10f);
                                spawnedObj.transform.localRotation = Quaternion.Euler(objRandomRotationX, objRandomRotationY, objRandomRotationZ);


                            }
                        }
                        else if (index == 1)
                        {
                            var chanceToSpawn = Random.Range(spawnChanceSea, 10);

                            //if (chanceToSpawn <= spawnChanceSea)
                            {
                                //Debug.Log("spawn complete");

                                var objToBeSpawned = seaTrees[Random.Range(0, seaTrees.Length)];
                                spawnedObj = Instantiate(objToBeSpawned, spawnPosition, Quaternion.identity) as GameObject;
                                spawnedObj.transform.position = hit.point;
                                var objRandomScale = Random.Range(0.5f, 1.5f);
                                var objRandomScaleY = Random.Range(3f, 6f);
                                spawnedObj.transform.localScale = new Vector3(objRandomScale, objRandomScaleY, objRandomScale);
                                var objRandomRotationX = Random.Range(-10f, 10f);
                                var objRandomRotationY = Random.Range(0f, 360f);
                                var objRandomRotationZ = Random.Range(-10f, 10f);
                                spawnedObj.transform.localRotation = Quaternion.Euler(objRandomRotationX, objRandomRotationY, objRandomRotationZ);
                            }
                        }
                        else if (index == 2 || index == 3 || index == 4)
                        {
                            var chanceToSpawn = Random.Range(spawnChanceTundra, 10);

                            //if (chanceToSpawn <= spawnChanceTundra)
                            {
                                //Debug.Log("spawn complete");

                                var objToBeSpawned = tundraTrees[Random.Range(0, tundraTrees.Length)];
                                spawnedObj = Instantiate(objToBeSpawned, spawnPosition, Quaternion.identity) as GameObject;
                                spawnedObj.transform.position = hit.point;
                                var objRandomScale = Random.Range(0.5f, 1.5f);
                                spawnedObj.transform.localScale = new Vector3(objRandomScale, objRandomScale, objRandomScale);
                                var objRandomRotationX = Random.Range(-10f, 10f);
                                var objRandomRotationY = Random.Range(0f, 360f);
                                var objRandomRotationZ = Random.Range(-10f, 10f);
                                spawnedObj.transform.localRotation = Quaternion.Euler(objRandomRotationX, objRandomRotationY, objRandomRotationZ);
                            }
                        }
                        else if (index == 5 || index == 6)
                        {
                            var chanceToSpawn = Random.Range(spawnChanceTropical, 10);

                            //if (chanceToSpawn <= spawnChanceTropical)
                            {
                                //Debug.Log("spawn complete");

                                var objToBeSpawned = tropicalTrees[Random.Range(0, tropicalTrees.Length)];
                                spawnedObj = Instantiate(objToBeSpawned, spawnPosition, Quaternion.identity) as GameObject;
                                spawnedObj.transform.position = hit.point;
                                var objRandomScale = Random.Range(0.5f, 1.5f);
                                spawnedObj.transform.localScale = new Vector3(objRandomScale, objRandomScale, objRandomScale);
                                var objRandomRotationX = Random.Range(-10f, 10f);
                                var objRandomRotationY = Random.Range(0f, 360f);
                                var objRandomRotationZ = Random.Range(-10f, 10f);
                                spawnedObj.transform.localRotation = Quaternion.Euler(objRandomRotationX, objRandomRotationY, objRandomRotationZ);
                            }
                        }*/
                    }     
                }
                else if (Physics.Raycast(transform.position + new Vector3(0, 0, 1), down, out hit, raycastHeight) == false)
                {
                    break;
                }

                transform.position = new Vector3(transform.position.x + j, transform.position.y + raycastHeight - 1f, transform.position.z + i);
            }
        }

        //transform.position = new Vector3(resetAllAxis.x, resetAllAxis.y, resetAllAxis.z);
    }

    //the color picker part of this code is based on this video: https://www.youtube.com/watch?v=P_nyEPAcWKE&list=PLuKojHpFPld-V8DFDkd2yXnQdZbv9FdD-&index=72&t=600s
    //I have heavily modified it for the purposes of this code though it is worth noting

    //this function will be fed a color which is to be represented as a color from the array, and returns the said number back
    private int FindIndexFromColor(Color color)
    {
        var terrainGeneration = GameObject.Find("TerrainUpdated").GetComponent<TerrainGeneration1>();

        for (int i = 0; i < terrainGeneration.colourPerHeight.Length; i++)
        {
            //Debug.Log("found color = " + color.ToString());
            //Debug.Log("stored color = " + terrainGeneration.colourPerHeight[i].biomeColour.ToString());

            if (terrainGeneration.colourPerHeight[i].biomeColour == color)
            {
                //Debug.Log("colour " + i + " found");
                return i;
            }
            else
            {
                //Debug.Log("invalid colour");
                //continue;
            }
        }

        return -1;
    }

    private void OnDrawGizmosSelected()
    {
        //these gizmos use the average distances produced by spawning trees and their intervals versus the number of trees being spawned to visually demonstrate a square that delineates where the trees will appear

        Gizmos.color = new Color32 (0, 255, 0, 100);
        Gizmos.DrawCube(new Vector3(transform.position.x - treesPerRow/2 * -1, transform.position.y - (raycastHeight / 2), transform.position.z + treesPerColumn / 2), new Vector3(treesPerRow, raycastHeight, treesPerColumn));

    }

}
