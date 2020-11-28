using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = System.Random;

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

        // Create # of locked rooms (random # = 0 to whatever max)
        int numberOfLockedRooms = GenerateLockedRooms(2);
        Debug.Log(string.Format("Generated {0} locked room{1}.",
            numberOfLockedRooms, numberOfLockedRooms == 1 ? "" : "s"));

        // Here is where we should make a BFS algorithm that places keys where it needs to be placed.
        PlaceKeys(numberOfLockedRooms);
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

    /** private GenerateLockedRooms
     *  Parameters: int maxNumberOfLockedRooms - Maximum # of locked rooms (default: 0)
     *              bool isRandomUpperLimit - Decides whether maxNumberOfLockedRooms is a random upper limit
     *                                        or a fixed limit (default: true)
     *  Return: int - number of locked rooms that were created
     *  This function will generate a number of locked rooms.
     */
    private int GenerateLockedRooms(int maxNumberOfLockedRooms = 0, bool isRandomUpperLimit = true)
    {
        Random rng = new Random();
        int limit = isRandomUpperLimit ? rng.Next(maxNumberOfLockedRooms + 1) : maxNumberOfLockedRooms;

        int lockedRoomCount = 0;
        while (lockedRoomCount < limit)
        {
            int chosenIndex = rng.Next(activeRooms.Count);
            Room chosenRoom = activeRooms[chosenIndex].GetComponent<Room>();
            if (chosenRoom.locked)
            {
                continue;
            }
            chosenRoom.SetLockStatus(true);
            lockedRoomCount++;
        }
        return limit;
    }

    /// private PlaceKeys
    /// <summary>
    /// This function will run BFS search and place keys in places that are needed.
    /// </summary>
    ///
    /// <param name="numKeys">Number of keys to place.</param>
    /// <returns>The number of keys that were placed.</returns>
    private int PlaceKeys(int numKeys)
    {
        // BFS based off of: https://www.redblobgames.com/pathfinding/a-star/introduction.html
        Room start = activeRooms[0].GetComponent<Room>();
        Queue<Room> rooms = new Queue<Room>();
        rooms.Enqueue(start);
        Dictionary<Room, Room> cameFromDict = new Dictionary<Room, Room>();
        cameFromDict.Add(start, null);
        Dictionary<Room, int> keysNeededDict = new Dictionary<Room, int>();
        keysNeededDict.Add(start, 0);

        while (rooms.Count > 0)
        {
            Room currentRoom = rooms.Dequeue();
            foreach(KeyValuePair<char, GameObject> neighborPair in currentRoom.neighbors)
            {
                if (neighborPair.Value && !cameFromDict.ContainsKey(neighborPair.Value.GetComponent<Room>()))
                {
                    Room nextRoom = neighborPair.Value.GetComponent<Room>();
                    rooms.Enqueue(nextRoom);
                    cameFromDict.Add(nextRoom, currentRoom);
                    int totalKeysNeeded = keysNeededDict[currentRoom];
                    totalKeysNeeded += (nextRoom.locked) ? 1 : 0;
                    keysNeededDict.Add(nextRoom, totalKeysNeeded);
                }
            }
        }

        // DEBUG: Go through activeRooms and check the keysNeededDict
        string keycountOutput = "";
        int loopIdx = 0;
        foreach (GameObject roomObj in activeRooms)
        {
            Room room = roomObj.GetComponent<Room>();
            keycountOutput += string.Format("Room {0} requires {1} key{2}\n",
                loopIdx, keysNeededDict[room], keysNeededDict[room] == 1 ? "" : "s");
            loopIdx++;
        }
        Debug.Log(keycountOutput);

        return numKeys;
    }
}
