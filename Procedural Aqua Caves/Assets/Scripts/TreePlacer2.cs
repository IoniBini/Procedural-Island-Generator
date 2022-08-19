using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlacer2 : MonoBehaviour
{
    [ContextMenu("Spawn Debug")]
    public void SpawnDebug()
    {
        var terrainGeneration = GetComponentInParent<TerrainGeneration1>();

        if (terrainGeneration.destroyTreesOnGenerate == true) ClearObjects();

        Spawn(terrainGeneration.DebugSpawnCount);
    }

    public void Spawn(int count)
    {
        var terrainGeneration = GetComponentInParent<TerrainGeneration1>();

        //terrainGeneration.Start = new Vector2(transform.position.x, transform.position.z);

        for (var i = 0; i < count; i++)
        {
            //We shine a virtual laster from a start point
            var rayPos = new Vector3(terrainGeneration.Start.x + terrainGeneration.Size.x * Random.value, terrainGeneration.MaxHeight, terrainGeneration.Start.y + terrainGeneration.Size.y * Random.value);

            //If that laser hits nothing, we stop here
            if (!Physics.Raycast(rayPos, Vector3.down, out var hit, terrainGeneration.MaxHeight)) continue;

            //If it hit an object on an invalid layer, we also stop here
            if (!MaskContainsLayer(terrainGeneration.ValidLayers, hit.collider.gameObject.layer)) continue;

            PlaceTreeAt(hit.point);
        }
    }

    //Given a position that we successfully touch the terrain at, 
    public void PlaceTreeAt(Vector3 position)
    {
        var terrainGeneration = GetComponentInParent<TerrainGeneration1>();

        for (int i = 0; i < terrainGeneration.treePerHeight.Length; i++)
        {
            float normalizedHeight = Mathf.InverseLerp(transform.parent.position.y, terrainGeneration.MaxHeight / 2, position.y);

            if (normalizedHeight <= terrainGeneration.treePerHeight[i].treeDepth)
            {
                if(terrainGeneration.treePerHeight[i].spawnChance >= Random.Range(0f, 1f))
                {
                    //breaks the loop if no obj was provided in the given height
                    if (terrainGeneration.treePerHeight[i].biomeTrees[0] == null)
                    {
                        break;
                    }

                    //Spawns a random prefab at the provided position - and gives it a random rotation and scale
                    var newObj = Instantiate(terrainGeneration.treePerHeight[i].biomeTrees[Random.Range(0, terrainGeneration.treePerHeight[i].biomeTrees.Length)]);
                    newObj.transform.parent = transform;
                    newObj.transform.position = position;
                    newObj.transform.eulerAngles = new Vector3(0f, Random.value * 360f, 0f);
                    newObj.transform.localScale = Vector3.one * Random.Range(1f - terrainGeneration.SizeVariance, 1f + terrainGeneration.SizeVariance);

                    break;
                }
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

    private void OnDrawGizmosSelected()
    {
        var terrainGeneration = GetComponentInParent<TerrainGeneration1>();

        Gizmos.color = new Color32(0, 255, 0, 100);
        Gizmos.DrawCube(new Vector3(terrainGeneration.Start.x - terrainGeneration.Size.x / 2 * -1, transform.position.y, terrainGeneration.Start.y + terrainGeneration.Size.y / 2), new Vector3(terrainGeneration.Size.x, terrainGeneration.MaxHeight, terrainGeneration.Size.y));
    }

}
