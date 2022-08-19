using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlacer2 : MonoBehaviour
{
    //before you start reading, READ THIS:
    //at first, I attempted to create a system for generating trees based on colour. I followed this tutorial and successfully made it work with 3D MESHES:
    //https://www.youtube.com/watch?v=P_nyEPAcWKE&t=525s
    //when I used the exact same set up for terrains, it wouldn't work. Turns out texture coordinates are completely different between terrains and meshes
    //I found this video bellow that explains how to do this correctly in terrains, and got it to work. The issue was that it was TERRIBLY inaccurate.
    //https://www.youtube.com/watch?v=YX8E1AE3BYs
    //the script was able to differentiate between colours just fine, but the placement of trees was totally off.
    //so, after wasting another 7 hours on this, I gave up.
    //I have sworn a sacred oath that I will NEVER code using terrains again, they are way too janky.
    //the new approach to tree placement is based on height, and this worked totally fine
    //the old scripts that don't work are inside the legacy folder if you wish to see it still
    //also worth noting that this script is heavily based on what Lachlan has shown in class

    [ContextMenu("Spawn Debug")]
    public void SpawnDebug()
    {
        var terrainGeneration = GetComponentInParent<TerrainGeneration1>();

        //this bool destroys the trees already placed before if true
        if (terrainGeneration.destroyTreesOnGenerate == true) ClearObjects();

        //spawns trees based on the number fed here
        Spawn(terrainGeneration.DebugSpawnCount);
    }

    public void Spawn(int count)
    {
        var terrainGeneration = GetComponentInParent<TerrainGeneration1>();

        //this bool goes off only after all the trees have been placed
        bool addExtras = false;

        //I did this because I want there to be more extras than trees
        //I also decided to do it this way instead of making each tree spawn its own extras because I want them to be placed independently
        //otherwise, extras will always be too close to trees, leaving many empty areas around
        for (var j = 0; j < terrainGeneration.extrasFactor; j++)
        {
            if (j >= 1) addExtras = true;

            for (var i = 0; i < count; i++)
            {
                //We shine a virtual laser from a start point
                var rayPos = new Vector3(terrainGeneration.Start.x + terrainGeneration.Size.x * Random.value, terrainGeneration.MaxHeight, terrainGeneration.Start.y + terrainGeneration.Size.y * Random.value);

                //If that laser hits nothing, we stop here
                if (!Physics.Raycast(rayPos, Vector3.down, out var hit, terrainGeneration.MaxHeight)) continue;

                //If it hit an object on an invalid layer, we also stop here
                if (!MaskContainsLayer(terrainGeneration.ValidLayers, hit.collider.gameObject.layer)) continue;

                //returns the hit location of the script as well as a bool to see if the extras are to be spawned yet or not
                PlaceTreeAt(hit.point, addExtras);
            }
        }

        
    }

    public void PlaceTreeAt(Vector3 position, bool addExtras)
    {
        var terrainGeneration = GetComponentInParent<TerrainGeneration1>();

        //repeat the tree placing process for each tree present in the list
        for (int i = 0; i < terrainGeneration.treePerHeight.Length; i++)
        {
            //normalizes the found height so it fits from 0 to 1
            float normalizedHeight = Mathf.InverseLerp(transform.parent.position.y, terrainGeneration.MaxHeight / 2, position.y);

            //if the height found is less or equal to that which is found in the list, then go ahead. This repeats for each tree in the list
            if (normalizedHeight <= terrainGeneration.treePerHeight[i].treeDepth)
            {
                //checks to see if this tree is allowed to be spawned based on the chance present in teh list
                if(terrainGeneration.treePerHeight[i].spawnChance >= Random.Range(0f, 1f))
                {
                    //breaks the loop if no obj was provided in the given height
                    if (terrainGeneration.treePerHeight[i].biomeTrees[0] == null)
                    {
                        break;
                    }

                    if(addExtras == false)
                    {
                        //Spawns a random prefab at the provided position - and gives it a random rotation and scale
                        var newObj = Instantiate(terrainGeneration.treePerHeight[i].biomeTrees[Random.Range(0, terrainGeneration.treePerHeight[i].biomeTrees.Length)]);
                        newObj.transform.parent = transform;
                        newObj.transform.position = position;
                        newObj.transform.eulerAngles = new Vector3(0f, Random.value * 360f, 0f);
                        newObj.transform.localScale = Vector3.one * Random.Range(1f - terrainGeneration.SizeVariance, 1f + terrainGeneration.SizeVariance);
                    }
                    else
                    {
                        //Spawns a random prefab at the provided position - and gives it a random rotation and scale
                        var newObj = Instantiate(terrainGeneration.treePerHeight[i].biomeExtras[Random.Range(0, terrainGeneration.treePerHeight[i].biomeExtras.Length)]);
                        newObj.transform.parent = transform;
                        newObj.transform.position = position;
                        newObj.transform.eulerAngles = new Vector3(0f, Random.value * 360f, 0f);
                        newObj.transform.localScale = Vector3.one * Random.Range(1f - terrainGeneration.SizeVariance, 1f + terrainGeneration.SizeVariance);
                    }
                    

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

    //I was REALLY annoyed about the lack of visual feedback of the tree placer that Lachlan made so I created a gizmo that makes it visible
    private void OnDrawGizmosSelected()
    {
        var terrainGeneration = GetComponentInParent<TerrainGeneration1>();

        Gizmos.color = new Color32(0, 255, 0, 100);
        Gizmos.DrawCube(new Vector3(terrainGeneration.Start.x - terrainGeneration.Size.x / 2 * -1, transform.position.y, terrainGeneration.Start.y + terrainGeneration.Size.y / 2), new Vector3(terrainGeneration.Size.x, terrainGeneration.MaxHeight, terrainGeneration.Size.y));
    }

}
