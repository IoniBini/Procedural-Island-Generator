using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlacer : MonoBehaviour
{
    //arrays for what you want to populate the scene with
    public GameObject[] tropicalTrees;
    public GameObject[] tundraTrees;
    public GameObject[] desertTrees;
    public GameObject[] seaTrees;

    public GameObject[] additionsPrefabs;
    //the latest spawned obj
    GameObject spawnedObj;

    //a public array that can be manually populated in the inspector with the colors present in the map
    public Color[] colors;
    //in case you want to specify what map should be used, I have made the texture public so you can use a different one if need be
    public Texture2D imageMap;

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
    [Range(1, 100)]
    public int treesPerRow;
    [Range(1, 100)]
    public int treesPerColumn;
    [Range(1, 200)]
    public int extrasPerRow;
    [Range(1, 200)]
    public int extrasPerColumn;
    [Range(10, 1000)]
    public int raycastHeight;

    //the color picker part of this code is based on this video: https://www.youtube.com/watch?v=P_nyEPAcWKE&list=PLuKojHpFPld-V8DFDkd2yXnQdZbv9FdD-&index=72&t=600s
    //I have heavily modified it for the purposes of this code though it is worth noting

    //this function will be fed a color which is to be represented as a color from the array, and returns the said number back
    private int FindIndexFromColor(Color color)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i] == color)
            {
                return i;
            }
        }

        return -1;
    }

    [ContextMenu("DeleteTrees")]
    void DeleteTrees()
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

        //stores the current position of the spawner so that it can be reset later
        var resetAllAxis = transform.position;

        //a 2d for loop, the first is for the collumns, the second is for the rows
        for (int j = 0; j <= treesPerColumn; j++)
        {
            var resetXAxis = transform.position.x;

            for (int i = 0; i <= treesPerRow; i++)
            {
                //int layerMask = 1 << 8;
                //layerMask = ~layerMask;

                //chooses to move between a random range of 10 and 2 on the x axis, and 10 and -10 for the z
                var resetZAxis = transform.position.z;
                transform.position = transform.position + new Vector3(-Random.Range(10f, 2f), 0, Random.Range(10f, -10f));

                //variables that the raycast use to determine the hit location
                Vector3 localOffset = new Vector3(0, 0, 1);
                Vector3 worldOffset = transform.rotation * localOffset;
                Vector3 spawnPosition = transform.position + worldOffset;

                //randomly picks a prefab in the list to spawn
                //var objToBeSpawned = treePrefabs[Random.Range(0, treePrefabs.Length)];

                //names the hit point of the raycast
                RaycastHit hit;

                //casts a ray downwards to check if it is currently hitting the floor and not something it should avoid
                Vector3 down = transform.TransformDirection(Vector3.down);
                if (Physics.Raycast(transform.position + new Vector3(0, 0, 1), down, out hit, raycastHeight))
                {
                    //layer 8 contains all the things that the raycast should avoid, hence it will only spawn anythig if it is not in that layer
                    if (hit.transform.gameObject.layer != 8)
                    {
                        Renderer renderer = hit.transform.GetComponent<MeshRenderer>();
                        Texture2D texture = renderer.sharedMaterial.mainTexture as Texture2D;
                        Vector2 pixelUV = hit.textureCoord;
                        pixelUV.x *= texture.width;
                        pixelUV.y *= texture.height;
                        Vector2 tiling = renderer.sharedMaterial.mainTextureScale;
                        Color color = imageMap.GetPixel(Mathf.FloorToInt(pixelUV.x * tiling.x), Mathf.FloorToInt(pixelUV.y * tiling.y));
                        /*Debug.Log("blue: " + color.b);
                        Debug.Log("green: " + color.g);
                        Debug.Log("red: " + color.r);*/


                        int index = FindIndexFromColor(color);
                        //Debug.Log("found color: " + index);

                        //depending on what color is found, a different array of prefabs is chosen to be spawned from
                        if (index == 0)
                        {
                            var chanceToSpawn = Random.Range(spawnChanceDesert, 10);

                            if (chanceToSpawn <= spawnChanceDesert)
                            {
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

                            if (chanceToSpawn <= spawnChanceSea)
                            {
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

                            if (chanceToSpawn <= spawnChanceTundra)
                            {
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

                            if (chanceToSpawn <= spawnChanceTropical)
                            {
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
                        }
                    }     
                }

                //after placing 1 tree, it goes back to the original z position so that it can continue making the row without running off
                transform.position = new Vector3(transform.position.x, transform.position.y, resetZAxis);
            }

            //once it is done spawining all the trees in the row, it goes up one column by adding 10 to the Z axis, and repeats the process
            transform.position = new Vector3(resetXAxis, transform.position.y, transform.position.z + 10);
        }
        
        //all the axis are reset so that the spawner can spawn the other extra things on the floor
        /*transform.position = new Vector3(resetAllAxis.x, resetAllAxis.y, resetAllAxis.z);
        
        //follows the exact same thing as the code above
        for (int j = 0; j <= extrasPerColumn; j++)
        {
            var resetXAxis = transform.position.x;

            for (int i = 0; i <= extrasPerRow; i++)
            {
                //int layerMask = 1 << 8;
                //layerMask = ~layerMask;

                var resetZAxis = transform.position.z;
                transform.position = transform.position + new Vector3(-Random.Range(4f, 1f), 0, Random.Range(5f, -5f));

                Vector3 localOffset = new Vector3(0, 0, 1);
                Vector3 worldOffset = transform.rotation * localOffset;
                Vector3 spawnPosition = transform.position + worldOffset;

                var objToBeSpawned = additionsPrefabs[Random.Range(0, additionsPrefabs.Length)];

                RaycastHit hit;
                Vector3 down = transform.TransformDirection(Vector3.down);
                if (Physics.Raycast(transform.position + new Vector3(0, 0, 1), down, out hit, 500))
                {
                    if (hit.transform.gameObject.layer != 8)
                    {
                        spawnedObj = Instantiate(objToBeSpawned, spawnPosition, Quaternion.identity) as GameObject;
                        spawnedObj.transform.position = hit.point;
                        var objRandomScale = Random.Range(0.5f, 1.5f);
                        spawnedObj.transform.localScale = new Vector3(objRandomScale, objRandomScale, objRandomScale);
                       // spawnedObj.transform.SetParent(gameObject.transform, false);
                    }
                }

                transform.position = new Vector3(transform.position.x, transform.position.y, resetZAxis);
            }

            transform.position = new Vector3(resetXAxis, transform.position.y, transform.position.z + 10);
        }*/

        transform.position = new Vector3(resetAllAxis.x, resetAllAxis.y, resetAllAxis.z);
    }

    private void OnDrawGizmosSelected()
    {
        //these gizmos use the average distances produced by spawning trees and their intervals versus the number of trees being spawned to visually demonstrate a square that delineates where the trees will appear

        Gizmos.color = new Color32 (0, 255, 0, 100);
        Gizmos.DrawCube(new Vector3(transform.position.x - treesPerRow * 6.5f/2, transform.position.y - (raycastHeight / 2), transform.position.z + treesPerColumn * 10f / 2), new Vector3(treesPerRow * 6.5f, raycastHeight, treesPerColumn * 10f));

        Gizmos.color = new Color32(255, 0, 0, 100);
        Gizmos.DrawCube(new Vector3(transform.position.x - extrasPerRow * 2.5f / 2, transform.position.y - (raycastHeight / 2), transform.position.z + extrasPerColumn * 10f / 2), new Vector3(extrasPerRow * 2.5f, raycastHeight, extrasPerColumn * 10f));
    }

}
