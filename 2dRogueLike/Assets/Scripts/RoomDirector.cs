using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoomGenerator))]
public class RoomDirector : MonoBehaviour
{
    // THE FOLLOWING FOUR GAMEOBJECTS ARE ELEMENTS THAT CAN BE ADDED TO A ROOM.
    public GameObject food;
    public GameObject enemy;
    public GameObject barricade;
    public GameObject key;

    //VARAIBLE THAT HOLDS THE INFORMATION OF THE CLASS 'RoomGenerator'
    private RoomGenerator rg;
    //A LIST OF ALL THE ROOMS IN THE DUNGEON
    private List<GameObject> activeRooms;
    //THE SIZE OF A DUNGEON. I.E. each dungeon is roomsize x roomsize big.
    private int roomSize;
    //VARIABLE THAT HOLDS THE INFORMATION OF THE CLASS PLAYERSTATS
    //USE THIS TO ACCESS PLAYERS ATTRIBUTES. EX. ps.health, ps.keyCount etc.
    private PlayerStats ps;
    
    //InitializeDirector
    //Return type: void
    //Parametes: none
    //Acts as the constructor for this class.
    //Initializes key variables such as the playerstat holder ps, and the roomgenerator varaible pg.
    public void InitializeDirector()
    {
        rg = GetComponent<RoomGenerator>();
        activeRooms = rg.rooms;
        roomSize = rg.offset;

        ps = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
    }

    //DoStuff
    //Return Type: void
    //Parameters: None
    //Function is called by the gamemoderator
    //This is the main function we will be working in. Make all of your respective function calls from here.
    public void DoStuff()
    {
        Debug.Log(ps.health); //Example of accessing the player stats.
        //Sample function call
        //GameObject thisRoom = activeRooms[0];
        //EditElementInRoom(ref thisRoom, 5, 5, food);
    }

    //EditElementInRoom
    //Return type: void
    //Paremeters: ref GameObject, int, int, GameObject
    //Given a reference to a room gameobject, instantiate GameObject newObject as the coordinates x, y inside of the room.
    //The newObject is then added to the rooms elements array, providing a reference to it.
    //THIS SHOULD BE USED TOO ADD THINGS SUCH AS KEYS, ENEMIES, ETC. TO THE LEVEL.
    //Do not edit this method, it is a helper method that should do what is exatly as described.
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

    //EditTileInRoom
    //Return type: void
    //Paremeters: ref GameObject, int, int, GameObject
    //Given a reference to a room gameobject, instantiate GameObject newObject as the coordinates x, y inside of the room.
    //The newObject is then added to the rooms Tiles array, providing a reference to it.
    //THIS SHOULD ONLY BE USED TO CHANGE THE ORGANIZATION OF THE ROOM, SUCH AS ADDING ADDITIONAL BLOCKS, OR REPLACING BACKGROUND/BORDER TILES.
    //Do not edit this method, it is a helper method that should do what is exatly as described.
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
