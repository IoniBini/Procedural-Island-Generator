using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlacer : MonoBehaviour
{
    public GameObject[] treePrefabs;
    public GameObject[] additionsPrefabs;
    GameObject spawnedObj;

    [ContextMenu("SpawnTree")]
    void SpawnTree()
    {
        var resetAllAxis = transform.position;

        for (int j = 0; j <= 10; j++)
        {
            var resetXAxis = transform.position.x;

            for (int i = 0; i <= 10; i++)
            {
                int layerMask = 1 << 8;
                layerMask = ~layerMask;

                var resetZAxis = transform.position.z;
                transform.position = transform.position + new Vector3(-Random.Range(10f, 2f), 0, Random.Range(10f, -10f));

                Vector3 localOffset = new Vector3(0, 0, 1);
                Vector3 worldOffset = transform.rotation * localOffset;
                Vector3 spawnPosition = transform.position + worldOffset;

                var objToBeSpawned = treePrefabs[Random.Range(0, treePrefabs.Length)];

                spawnedObj = Instantiate(objToBeSpawned, spawnPosition, Quaternion.identity) as GameObject;
                var objRandomScale = Random.Range(0.5f, 1.5f);
                spawnedObj.transform.localScale = new Vector3(objRandomScale, objRandomScale, objRandomScale);

                RaycastHit hit;
                Vector3 down = transform.TransformDirection(Vector3.down);
                if (Physics.Raycast(transform.position + new Vector3(0, 0, 1), down, out hit, 100, layerMask))
                {
                    spawnedObj.transform.position = hit.point;
                }

                transform.position = new Vector3(transform.position.x, transform.position.y, resetZAxis);
            }

            transform.position = new Vector3(resetXAxis, transform.position.y, transform.position.z + 10);
        }
        
        transform.position = new Vector3(resetAllAxis.x, resetAllAxis.y, resetAllAxis.z);

        for (int j = 0; j <= 10; j++)
        {
            var resetXAxis = transform.position.x;

            for (int i = 0; i <= 25; i++)
            {
                int layerMask = 1 << 8;
                layerMask = ~layerMask;

                var resetZAxis = transform.position.z;
                transform.position = transform.position + new Vector3(-Random.Range(4f, 1f), 0, Random.Range(5f, -5f));

                Vector3 localOffset = new Vector3(0, 0, 1);
                Vector3 worldOffset = transform.rotation * localOffset;
                Vector3 spawnPosition = transform.position + worldOffset;

                var objToBeSpawned = additionsPrefabs[Random.Range(0, additionsPrefabs.Length)];

                spawnedObj = Instantiate(objToBeSpawned, spawnPosition, Quaternion.identity) as GameObject;
                var objRandomScale = Random.Range(0.5f, 1.5f);
                spawnedObj.transform.localScale = new Vector3(objRandomScale, objRandomScale, objRandomScale);

                RaycastHit hit;
                Vector3 down = transform.TransformDirection(Vector3.down);
                if (Physics.Raycast(transform.position + new Vector3(0, 0, 1), down, out hit, 100, layerMask))
                {
                    spawnedObj.transform.position = hit.point;
                }

                transform.position = new Vector3(transform.position.x, transform.position.y, resetZAxis);
            }

            transform.position = new Vector3(resetXAxis, transform.position.y, transform.position.z + 10);
        }

        transform.position = new Vector3(resetAllAxis.x, resetAllAxis.y, resetAllAxis.z);
    }
}
