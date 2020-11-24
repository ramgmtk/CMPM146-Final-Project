using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public int roomSize = 10;
    public GameObject wall;
    public GameObject floor;
    public GameObject door;
    public float offset = -4.5f;
    public string roomType = "None";
    [HideInInspector]public GameObject [,] tiles;
    [HideInInspector]public bool locked = false;
    [HideInInspector]public Dictionary<char, GameObject> neighbors = new Dictionary<char, GameObject>();
    [HideInInspector]public bool roomGenerated = false;

    private Vector3 pos;
    private Transform roomHolder;
    private Dictionary<char, (int , int)> doorLocations = new Dictionary<char, (int, int)>();
    // Start is called before the first frame update
    void Start()
    {
        pos = this.transform.position;
        tiles = new GameObject[roomSize, roomSize];
        SetupLevel();
        roomGenerated = true;
    }

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
    //For simplicity unlocking a door, unlocks all doors in that room.
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

    public void UnlockDoors(char direction)
    {
        if (neighbors[direction] != null)
        {
            ReplaceTile(floor, doorLocations[direction].Item1, doorLocations[direction].Item2);
        }
        locked = false;
    }

    //lock all entrances of a room.
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

    public void LockDoors(char direction)
    {
        if (neighbors[direction] != null)
        {
            ReplaceTile(door, doorLocations[direction].Item1, doorLocations[direction].Item2);
        }
        locked = true;
    }

    //make this room aware of its neighbor. Accepts string to denote carinal direction of neighbor relative to this room, as well as 
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

    //Replace a Tile in this room with a Some Prefab at some x, y coord within the room.
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
        replaced.SetActive(false);
        instance.transform.SetParent(roomHolder);
    }

    //Setup the initial room. Populates the tile array. Called automatically on creation.
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

    //Mark the coordinates for where doors are meant to go.
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
