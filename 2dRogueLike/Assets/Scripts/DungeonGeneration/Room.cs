using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class: Room
//Handles all the internal knowledge for a Room in the dungeon.
//Rooms have 'elements' in them which is indicative of enemies and keys or other goodies.
//A Room is categorized by if it locked or unlocked, and has been visited.
public class Room : MonoBehaviour
{
    //THE FOLLOWING VARIABLES ARE USED FOR ROOM GENERATION DO NOT UTILIZE THEM. DO NOT CALL
    public int roomSize = 10;
    public GameObject wall;
    public GameObject floor;
    public GameObject door;
    public float offset = -4.5f;
    //THE ABOVE VARIABLES ARE USED FOR ROOM GENERATION DO NOT ACCESS THEM. DO NOT CALL

    //String that can be used to label a room. Think of it like a unity tag, but internal to this script.
    public string roomType = "None";

    // BOTH 2D ARRAYS USE THE INDEX AS A REPRESENTATION OF THE X, Y LOCATION IN THE ROOM. I.E. tiles[0,0] represents the bottom left corner of the room
    //Tiles is a 2D array which hold all of the tiles building a room such as the borders and background.
    [HideInInspector]public GameObject [,] tiles;
    //Enemies is a 2D array which holds all of the unique gameobject elements in a room. On creation this is empty.
    [HideInInspector]public GameObject [,] elements;

    //Locked is used to show the status of if a room is locked. NEVER change this directly, treat it as a read-only variable. 
    //Use the method 'SetLockStatus(bool)' to adjust the value.
    [HideInInspector]public bool locked = false;
    //Variable is used to indicate if a room has been visited. optional variable. A* should utilize it's own visited variable not this one.
    [HideInInspector]public bool visited = false;
    //A dictionary that holds all of the neighbors adjacent to a given room. Keys are the cardinal directions W,E,N,S. 
    //neighbors['W'] will return the room west of the current one if it exists
    [HideInInspector]public Dictionary<char, GameObject> neighbors = new Dictionary<char, GameObject>();
    //DO NOT UTILIZE THIS VARIABLE. IT IS BEING USED TO PREVENT A RACE CONDITION FROM OCCURRING DURING DUNGEON GENERATION.
    [HideInInspector]public bool roomGenerated = false;

    //THE FOLLOWING VARIABLES ARE USED FOR ROOM GENERATION DO NOT UTILIZE THEM. DO NOT CALL
    private Vector3 pos;
    private Transform roomHolder;
    private Dictionary<char, (int , int)> doorLocations = new Dictionary<char, (int, int)>();
    // Start is called before the first frame update
    //Start is kind of like the constructor for this script in this case. DO NOT INVOKE.
    void Start()
    {
        pos = this.transform.position;
        tiles = new GameObject[roomSize, roomSize];
        elements = new GameObject[roomSize, roomSize];
        SetupLevel();
        roomGenerated = true;
    }

    //SetLockStatus
    //Return type: void
    //Parameters: bool
    //Function will either lock or unlock a room based on the bool passed as an argument.
    //True means the room will be locked, false will unlock the room.
    public void SetLockStatus(bool value)
    {
        if (value)
        {
            LockDoors();
        }
        else
        {
            UnlockDoors();
        }
    }

    //THE FOLLOWING METHODS SHOULD NOT BE CALLED BY ANY OTHER SCRIPT ASIDE FROM 'RoomGenerator'. DO NOT UTILIZE THESE.
    //UnLockDoor
    //Return type: void
    //Parameters: None
    //Function will unlock all the doors in this room.
    //In the game space this means if a player unlocked the southern entrance, all the other entrances will also open.
    //For simplicity unlocking a door, unlocks all doors in that room. DO NOT CALL
    public void UnlockDoors()
    {
        //For each neighbor in this room, make sure the doors are unlocked.
        foreach(KeyValuePair<char, (int, int)> gate in doorLocations)
        {
            if (neighbors[gate.Key] != null)
            {
                ReplaceTile(floor, gate.Value.Item1, gate.Value.Item2);
            }
        }
        locked = false;
    }

    //UnLock Door
    //Return type: void
    //Paremeters: char
    //The function will unlock the door of room for the given cardinal direction indicated by the char passed in.
    //The char should be one of ['W', 'N', 'E', 'S'] DO NOT CALL
    public void UnlockDoors(char direction)
    {
        if (neighbors[direction] != null)
        {
            ReplaceTile(floor, doorLocations[direction].Item1, doorLocations[direction].Item2);
        }
        locked = false;
    }
    //LockDoor
    //Return type: void
    //Parameters: None
    //Function will lock all the doors in this room.
    //In the game space this means all entrances to the room will become locked.
    //lock all entrances of a room. DO NOT CALL
    public void LockDoors()
    {
        foreach(KeyValuePair<char, (int, int)> gate in doorLocations)
        {
            if (neighbors[gate.Key] != null)
            {
                ReplaceTile(door, gate.Value.Item1, gate.Value.Item2);
            }
        }
        locked = true;
    }

    //Lock Door
    //Return type: void
    //Paremeters: char
    //The function will lock the door of room for the given cardinal direction indicated by the char passed in.
    //The char should be one of ['W', 'N', 'E', 'S'] DO NOT CALL
    public void LockDoors(char direction)
    {
        if (neighbors[direction] != null)
        {
            ReplaceTile(door, doorLocations[direction].Item1, doorLocations[direction].Item2);
        }
        locked = true;
    }
    //AddNeighbor
    //Return Type: void
    //Paremeters: char, ref gameobject
    //Add a neighbor to a rooms neighbors dictionary. A cardinal direction and a reference to the room becoming the neighbor must be passed in.
    //DO NOT CALL 
    public void AddNeighbor(char direction, ref GameObject otherRoom)
    {
        if (neighbors[direction] != null)
        {
            Debug.Log("Error: This room already has a neighbor in that direction");
            return;
        }
        neighbors[direction] = otherRoom;
        //Below is inefficient because the code will run through all the the entrances and unlock the doors, not just the direction of the neighbor.
        if (!locked) 
        {
            UnlockDoors(direction);
        }
        else
        {
            LockDoors(direction);
        }
    }

    //ReplaceTile
    //Return type: void
    //Parameters: Gameobject, int, int
    //Given a gameobject you want to add, provide x, y coordinated within the room for where you want to add it.
    //This function is used for replacing elements in the tiles array. It should be be used to add design elements like keys or enemies.
    //Replace a Tile in this room with a Some Prefab at some x, y coord within the room. DO NOT CALL
    public void ReplaceTile(GameObject replacer, int x, int y)
    {
        if (x < 0 || x > roomSize || y < 0 || y > roomSize)
        {
            Debug.Log("Error: Invalid Coordinates Provided. Range(0-10)");
            return;
        }
        GameObject replaced = tiles[x, y];
        GameObject instance = Instantiate(replacer, new Vector3(replaced.transform.position.x, replaced.transform.position.y, 0f),Quaternion.identity) as GameObject;
        tiles[x, y] = instance;
        //replaced.SetActive(false);
        Destroy(replaced);
        instance.transform.SetParent(roomHolder);
    }

    //Setup the initial room. Populates the tile array. Called automatically on creation. DO NOT CALL
    private void SetupLevel()
    {
        SetupExits();
        roomHolder = new GameObject("Room").transform;
        roomHolder.transform.SetParent(this.transform);
        for (int x = 0; x < roomSize; x++) 
        {
            for (int y = 0; y < roomSize; y++)
            {
                GameObject toInstantiate = floor;
                if (x == 0 || x == roomSize - 1 || y == 0 || y == roomSize - 1)
                {
                    toInstantiate = wall;
                }
                GameObject instance = Instantiate(toInstantiate, new Vector3(pos.x + x + offset, pos.y + y + offset, 0f),Quaternion.identity) as GameObject;
                tiles[x, y] = instance;
                instance.transform.SetParent(roomHolder);
            }
        }
    }

    //Mark the coordinates for where doors are meant to go. DO NOT CALL
    private void SetupExits()
    {
        doorLocations.Add('W', (0, roomSize/2));
        doorLocations.Add('N', (roomSize/2, roomSize - 1));
        doorLocations.Add('E', (roomSize - 1, roomSize/2));
        doorLocations.Add('S', (roomSize/2, 0));
        neighbors.Add('W', null);
        neighbors.Add('N', null);
        neighbors.Add('E', null);
        neighbors.Add('S', null);
    }
}
