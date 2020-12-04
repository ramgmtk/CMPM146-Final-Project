using System.Collections;
using System.Collections.Generic;
using ArgumentException = System.ArgumentException;
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
        //Debug.Log(ps.health); //Example of accessing the player stats.
        //Sample function call
        //GameObject thisRoom = activeRooms[0];
        //EditElementInRoom(ref thisRoom, 5, 5, food);

        // Create # of locked rooms (random # = 0 to whatever max)
        int numberOfLockedRooms = GenerateLockedRooms(2);
        //Debug.Log(string.Format("Generated {0} locked room{1}.",
            //numberOfLockedRooms, numberOfLockedRooms == 1 ? "" : "s"));

        // Here is where we should make a BFS algorithm that places keys where it needs to be placed.
        //Debug.Log(string.Format("Keys placed = {0}", PlaceKeys(numberOfLockedRooms + 1)));
        int amountOfKeys = PlaceKeys(numberOfLockedRooms + 1);
        GenerateEnemies();
    }

    //EditElementInRoom
    //Return type: void
    //Paremeters: ref GameObject, int, int, GameObject
    //Given a reference to a room gameobject, instantiate GameObject newObject as the coordinates x, y inside of the room.
    //The newObject is then added to the rooms elements array, providing a reference to it.
    //THIS SHOULD BE USED TOO ADD THINGS SUCH AS KEYS, ENEMIES, ETC. TO THE LEVEL.
    //Do not edit this method, it is a helper method that should do what is exatly as described.
    public void EditElementInRoom(ref GameObject room, int x, int y, GameObject newObject)
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
            int chosenIndex = rng.Next(1, activeRooms.Count);
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

    // replaces a random tile with an element
    public void AddElementRandomly(GameObject room, GameObject element){
        Random rng = new Random();
        bool elementPlaced = false;
        while(!elementPlaced)
        {
            // Choose a random x and y position
            int randomX = rng.Next(1, room.GetComponent<Room>().roomSize - 1);
            int randomY = rng.Next(1, room.GetComponent<Room>().roomSize - 1);

            // TODO: make it so that it checks if randomly selected location is floor vs wall

            // Check if random location does not have an item. If so, place key there.
            if (!room.GetComponent<Room>().elements[randomX, randomY])
            {
                EditElementInRoom(ref room, randomX, randomY, element);
                elementPlaced = true;
            }
        }
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
        // Random rng
        Random rng = new Random();

        /*** RUN BFS AND CREATE DICTIONARY OF ROOMS THAT MAP TO # OF KEYS REQUIRED ***/
        // BFS based off of: https://www.redblobgames.com/pathfinding/a-star/introduction.html
        GameObject start = activeRooms[0];
        Queue<GameObject> rooms = new Queue<GameObject>();
        rooms.Enqueue(start);
        Dictionary<GameObject, GameObject> cameFromDict = new Dictionary<GameObject, GameObject>();
        cameFromDict.Add(start, null);
        Dictionary<GameObject, int> keysNeededDict = new Dictionary<GameObject, int>();
        keysNeededDict.Add(start, (start.GetComponent<Room>().locked?1:0));

        while (rooms.Count > 0)
        {
            GameObject currentRoom = rooms.Dequeue();
            foreach(KeyValuePair<char, GameObject> neighborPair in currentRoom.GetComponent<Room>().neighbors)
            {
                if (neighborPair.Value && !cameFromDict.ContainsKey(neighborPair.Value))
                {
                    GameObject nextRoom = neighborPair.Value;
                    rooms.Enqueue(nextRoom);
                    cameFromDict.Add(nextRoom, currentRoom);
                    int totalKeysNeeded = keysNeededDict[currentRoom];
                    totalKeysNeeded += (nextRoom.GetComponent<Room>().locked) ? 1 : 0;
                    keysNeededDict.Add(nextRoom, totalKeysNeeded);
                }
            }
        }

        // DEBUG: Go through activeRooms and check the keysNeededDict
        string keycountOutput = "";
        int loopIdx = 0;
        foreach (GameObject roomObj in activeRooms)
        {
            GameObject room = roomObj;
            keycountOutput += string.Format("Room {0} requires {1} key{2}\n",
                loopIdx, keysNeededDict[room], keysNeededDict[room] == 1 ? "" : "s");
            loopIdx++;
        }
        //Debug.Log(keycountOutput);

        /*** CREATE DICTIONARY THAT MAPS # OF KEYS NEEDED (defined by keysNeededDict)
             TO A LIST OF ROOMS THAT NEED THAT SAME # OF KEYS
         ***/
        // Create a new dictionary mapping keys needed to array of rooms
        Dictionary<int, List<GameObject>> keysToRoomDict = new Dictionary<int, List<GameObject>>();
        // Fill up this array
        foreach(KeyValuePair<GameObject, int> roomKeyPair in keysNeededDict)
        {
            // Do not include "End" room type
            if (roomKeyPair.Key.GetComponent<Room>().roomType.Equals("End"))
            {
                continue;
            }
            if (!keysToRoomDict.ContainsKey(roomKeyPair.Value))
            {
                keysToRoomDict.Add(roomKeyPair.Value, new List<GameObject>());
            }
            keysToRoomDict[roomKeyPair.Value].Add(roomKeyPair.Key);
        }

        /*** PLACE KEYS HERE, GOING THROUGH EACH DICTIONARY ENTRY IN keysToRoomDict
             AND PLACING 1 KEY IN A RANDOMLY CHOSEN SPOT + RANDOMLY CHOSEN ROOM
         ***/
        // Starting from value 0 to whatever value, place keys into every room
        int totalKeysPlaced = 0;
        //Debug.Log(string.Format("keysToRoomDict Count = {0}", keysToRoomDict.Count));
        foreach (KeyValuePair<int, List<GameObject>> keyRoomPair in keysToRoomDict)
        {
            // we do not want to place keys in rooms that require too many keys
            if (keyRoomPair.Key - 1 >= numKeys)
            {
                Debug.Log(string.Format("Avoiding Placing Key Here: current KeysNeeded = {0} - 1 for numKeys = {1}", keyRoomPair, numKeys));
                continue;
            }
            // Otherwise we choose randomly from the list of rooms
            List<GameObject> currentRoomList = keyRoomPair.Value;
            int chosenIndex = rng.Next(currentRoomList.Count);
            GameObject chosenRoom = currentRoomList[chosenIndex];
            AddElementRandomly(chosenRoom, key);
            totalKeysPlaced++;
        }

        /*** If there were any keys that still need to be placed, do it here.
         ***/
        // The last thing to be done is if totalKeysPlaced is less than numKeys, place keys
        //  in any room that requires 0 keys
        while (totalKeysPlaced < numKeys)
        {
            Debug.Log("extra key was placed");
            List<GameObject> currentRoomList = keysToRoomDict[0];
            int chosenIndex = rng.Next(currentRoomList.Count);
            GameObject chosenRoom = currentRoomList[chosenIndex];
            AddElementRandomly(chosenRoom, key);
            totalKeysPlaced++;
        }
        return numKeys;
    }

    private void GenerateEnemies()
    {
        Random rng = new Random();
        int NumEnemies(int health)
        {
            if(health < 20){return rng.Next(0, 1);}
            else if(health < 60) {return rng.Next(0, 2);}
            else {return rng.Next(0, 3);}
        }

        int playerHealth = ps.health;
        foreach (GameObject roomObj in activeRooms)
        {
            GameObject room = roomObj; // is this necessary?
            if (room.GetComponent<Room>().roomType == "Start" || room.GetComponent<Room>().roomType == "End")
            {
                continue;
            }
            int numEnemies =  NumEnemies(playerHealth);
            for(int i = 0; i<= numEnemies; i++)
            {
                AddElementRandomly(room, enemy);
            }
        }
    }
}
