using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    //this script is inspired by brackeys: https://www.youtube.com/watch?v=vFvwyu_ZKfU
    //I didnt want to just copy sebastian lagues whole landmass generator video so instead, I will ATTEMPT to take a different approach, where I will mix elements from
    //Brackeys, Sebastian and Lachlans codes so that I turn out with something slightly more original of my part (if "frankensteining" code can be called original)
    //Im doing this not because it will be more efficient, it is exclusively because I want to show that I do know how to code as opposed to just copying

    //to avoid causing errors due to the resolution of the terrain not being in multiples of the power of 2, I have created these variables to multiply with
    //I have limited it to 7 arbitrarily, I just think that after 4, the performance gets too janky to be acceptable
    [Range(1, 7)] public int terrainResolution = 1;
    public int terrainRows = 1;
    public int terrainCollumns = 1;

    //I dont want these to be changed, so I'll hide them from view
    [HideInInspector] public int width = 32; //x-axis of the terrain
    [HideInInspector] public int height = 32; //z-axis

    public int depth = 20; //y-axis

    public float scale = 20f;

    [HideInInspector]public float offsetX = 100f;// equivalent to the width
    [HideInInspector]public float offsetY = 100f;// equivalent to the height

    [HideInInspector] public Terrain left;
    [HideInInspector] public Terrain top;
    [HideInInspector] public Terrain right;
    [HideInInspector] public Terrain bottom;

    [ContextMenu("GenerateTerrain")]
    private void ParentGenerateTerrain()
    {
        

        //delete the previous stuff prior to commencing
        ClearTerrain();

        //reset the x and y values because they might have been modified by the resolution
        width = 32;
        height = 32;

        //pick a new offset to make sure its random each time
        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);
        var offsetXReset = offsetX;
        var offsetYReset = offsetY;


        for (int i = 0; i < terrainResolution - 1; i++)
        {
            width *= 2;
            height *= 2;
        }

        for (int y = 0; y <= terrainRows - 1; y++)
        {
            //stores and reasigns the offsetX so that it doesnt keep on infinitely adding after beign done with one row and going up to the next one
            var tempOffsetX = offsetX;
            //Debug.Log(tempOffsetX);

            //the minus 1 is due to the fact that the parent terrain occupies one of the slots
            for (int x = 0; x <= terrainCollumns - 1; x++)
            {
                //https://answers.unity.com/questions/292982/how-to-create-terraindata-at-runtime.html

                TerrainData tData = new TerrainData();
                var newTerrain = Terrain.CreateTerrainGameObject(tData);
                newTerrain.transform.position = new Vector3(transform.position.x + width * x, transform.position.y, transform.position.z + width * y);
                newTerrain.AddComponent<ChildTerrainGeneration>();
                newTerrain.name = "X" + x + "Y" + y;
                newTerrain.gameObject.transform.parent = transform;

                //use the names to determine who is going to be a neighbour to the current terrain
                //it currently works, but the issue is that the meshes dont actually connect at the seams
                //Debug.Log("Left: " + "X" + (j - 1).ToString() + "Y" + i.ToString());
                if (GameObject.Find("X" + (x - 1).ToString() + "Y" + y.ToString()) != null)
                {
                    left = GameObject.Find("X" + (x - 1).ToString() + "Y" + y.ToString()).GetComponent<Terrain>(); 
                }
                else
                {
                    left = null;
                }

                //Debug.Log("Top: " + "X" + j.ToString() + "Y" + (i + 1).ToString());
                if (GameObject.Find("X" + x.ToString() + "Y" + (y + 1).ToString()) != null)
                {
                    top = GameObject.Find("X" + x.ToString() + "Y" + (y + 1).ToString()).GetComponent<Terrain>();
                }
                else
                {
                    top = null;
                }

                //Debug.Log("Right: " + "X" + (j + 1).ToString() + "Y" + i.ToString());
                if (GameObject.Find("X" + (x + 1).ToString() + "Y" + y.ToString()) != null)
                {
                    right = GameObject.Find("X" + (x + 1).ToString() + "Y" + y.ToString()).GetComponent<Terrain>();
                }
                else
                {
                    right = null;
                }

                //Debug.Log("Bottom: " + "X" + j.ToString() + "Y" + (i - 1).ToString());
                if (GameObject.Find("X" + x.ToString() + "Y" + (y - 1).ToString()) != null)
                {
                    bottom = GameObject.Find("X" + x.ToString() + "Y" + (y - 1).ToString()).GetComponent<Terrain>();
                }
                else
                {
                    bottom = null;
                }


                //Debug.Log(offsetX);
                offsetX += height;
                //Debug.Log(offsetX);
            }

            offsetY += width;

            offsetX = tempOffsetX;
        }

        offsetX = offsetXReset;
        offsetY = offsetYReset;

        if (transform.childCount != 0)
        {
            //We iterate backwards through children, since if we go forwards we end up changing child indices
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                transform.GetChild(i).GetComponent<ChildTerrainGeneration>().ChildGenerateTerrain();
                transform.GetChild(i).GetComponent<Terrain>().SetNeighbors(left, top, right, bottom);
                transform.GetChild(i).GetComponent<Terrain>().Flush();
            }
        }
    }

    [ContextMenu("ClearTerrain")]
    private void ClearTerrain()
    {
        if (transform.childCount != 0)
        {
            //We iterate backwards through children, since if we go forwards we end up changing child indices
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                //DestroyImmediate is for edit-mode, Destroy is for play-mode (including in the built application)
                if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
                else DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
}
