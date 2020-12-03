using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoomDirector), typeof(PlayerPlanner))]
public class DramaManager : MonoBehaviour
{
    public float dramaMeter;
    public GameObject food;
    public GameObject enemy;
    public GameObject key;
    private RoomDirector rd;
    private PlayerPlanner pp;
    // Start is called before the first frame update
    void Start()
    {
        rd = GetComponent<RoomDirector>();
        pp = GetComponent<PlayerPlanner>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Update a room that a player visits
    //This will remove the elements from the room, performing their respective actions on the player.
    //ex: enemies will subtract from the player health, and keys will be added to player inventory.
    //This function is called from the playerplanner.
    //Drama management should be done before the room update is called.
    public void UpdateRoom(ref GameObject room, ref PlayerStats player)
    {
        GameObject[,] currentElements = room.GetComponent<Room>().elements;
        if (room.GetComponent<Room>().locked)
        {
            room.GetComponent<Room>().SetLockStatus(false);
            player.keyCount -= 1;
        }
        for (int i = 0; i < currentElements.GetLength(0); i++)
        {
            for (int j = 0; j < currentElements.GetLength(0); j++)
            {
                if (currentElements[i,j])
                {
                    if (currentElements[i, j].tag == "Enemy")
                    {
                        player.health = player.health - pp.enemyDamage;
                    }
                    else if (currentElements[i, j].tag == "Key")
                    {
                        player.keyCount += 1;
                    }
                    else if (currentElements[i, j].tag == "Food")
                    {
                        player.health +=  5;
                    }
                    rd.EditElementInRoom(ref room, i, j, null);
                }
            }
        }
    }

    public void DramatizeRoom(ref GameObject room, ref PlayerStats player)
    {
        //Fill in code here
        //user rd.EditElementInRoom(room, xpos, ypos, GameObjectToBeAdded) to add new elements to a room at position x,y
        //let GameObjectToBeAdded = null if you want to remove an element at that x y coord.
        //valid gameobjects that can passed are the public GameObject member variables listed at the top of this file. ex: key.
        return;
    }
}
