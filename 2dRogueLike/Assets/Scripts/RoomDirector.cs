using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoomGenerator))]
public class RoomDirector : MonoBehaviour
{
    // Start is called before the first frame updat
    public GameObject food;
    public GameObject enemy;
    public GameObject barricade;
    private RoomGenerator rg;
    private List<GameObject> activeRooms;
    private int roomSize;
    private PlayerStats ps;
    
    public void InitializeDirector()
    {
        rg = GetComponent<RoomGenerator>();
        activeRooms = rg.rooms;
        roomSize = rg.offset;

        ps = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
    }

    //This is the main function we will be working in. Make all of your respective function calls from here.
    public void DoStuff()
    {
        Debug.Log(ps.health);
        //Sample function call
        //GameObject thisRoom = activeRooms[0];
        //EditElementInRoom(ref thisRoom, 5, 5, food);
    }

    // Update is called once per frame
    private void EditElementInRoom(ref GameObject room, int x, int y, GameObject newObject)
    {
        if (x == 0 || x == roomSize || y == 0 || y == roomSize)
        {
            Debug.Log("Error: Not allowed to edit borders of the room");
            return;
        }
        else
        {
            Room rm = room.GetComponent<Room>();
            if (rm.elements[x,y])
            {
                GameObject replaced = rm.elements[x,y];
                Destroy(replaced);
            }
            if (newObject)
            {
                GameObject instance = Instantiate(newObject, rm.tiles[x,y].transform.position, Quaternion.identity) as GameObject;
                rm.elements[x,y] = instance;
                instance.transform.SetParent(room.transform);
            }
        }
    }

    private void EditTileInRoom(ref GameObject room, int x, int y, GameObject newObject)
    {
        if (x == 0 || x == roomSize || y == 0 || y == roomSize)
        {
            Debug.Log("Error: Not allowed to edit borders of the room");
            return;
        }
        else
        {
            Room rm = room.GetComponent<Room>();
            GameObject replaced = rm.tiles[x,y];
            GameObject instance = Instantiate(newObject, replaced.transform.position, Quaternion.identity) as GameObject;
            rm.tiles[x,y] = instance;
            instance.transform.SetParent(replaced.transform.parent);
            Destroy(replaced);
        }
    }
}
