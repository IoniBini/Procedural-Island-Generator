using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlacer : MonoBehaviour
{
    public GameObject[] treePrefabs;
    public GameObject[] additionsPrefabs;
    GameObject spawnedObj;

    [Range(1, 100)]
    public int treesPerRow;
    [Range(1, 100)]
    public int treesPerColumn;
    [Range(1, 200)]
    public int extrasPerRow;
    [Range(1, 200)]
    public int extrasPerColumn;

    [ContextMenu("SpawnTree")]
    void SpawnTree()
    {
        if (GameObject.FindGameObjectWithTag("Tree") != null)
        {
            var oldTrees = GameObject.FindGameObjectsWithTag("Tree");
            int objsNumber = oldTrees.Length;

            for (int i = 0; i < objsNumber; i++)
            {
                DestroyImmediate(oldTrees[i]);
            }
        }  

        var resetAllAxis = transform.position;

        for (int j = 0; j <= treesPerColumn; j++)
        {
            var resetXAxis = transform.position.x;

            for (int i = 0; i <= treesPerRow; i++)
            {
                //int layerMask = 1 << 8;
                //layerMask = ~layerMask;

                var resetZAxis = transform.position.z;
                transform.position = transform.position + new Vector3(-Random.Range(10f, 2f), 0, Random.Range(10f, -10f));

                Vector3 localOffset = new Vector3(0, 0, 1);
                Vector3 worldOffset = transform.rotation * localOffset;
                Vector3 spawnPosition = transform.position + worldOffset;

                var objToBeSpawned = treePrefabs[Random.Range(0, treePrefabs.Length)];

                

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
                        //spawnedObj.transform.SetParent(gameObject.transform, false);
                    }     
                }

                transform.position = new Vector3(transform.position.x, transform.position.y, resetZAxis);
            }

            transform.position = new Vector3(resetXAxis, transform.position.y, transform.position.z + 10);
        }
        
        transform.position = new Vector3(resetAllAxis.x, resetAllAxis.y, resetAllAxis.z);

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
        }

        transform.position = new Vector3(resetAllAxis.x, resetAllAxis.y, resetAllAxis.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color32 (0, 255, 0, 100);
        Gizmos.DrawCube(new Vector3(transform.position.x - treesPerRow * 6.5f/2, transform.position.y, transform.position.z + treesPerColumn * 10f / 2), new Vector3(treesPerRow * 6.5f, 100, treesPerColumn * 10f));

        Gizmos.color = new Color32(255, 0, 0, 100);
        Gizmos.DrawCube(new Vector3(transform.position.x - extrasPerRow * 2.5f / 2, transform.position.y, transform.position.z + extrasPerColumn * 10f / 2), new Vector3(extrasPerRow * 2.5f, 100, extrasPerColumn * 10f));
    }

}
