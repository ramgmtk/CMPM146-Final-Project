using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

//Class:Room Generator
//Handles the intiial creation of the dungeon
//Not of it's methods should be called by other scripts.
//Contains a list of all the rooms present within the dungeon.
public class RoomGenerator : MonoBehaviour
{
    //Determines how many rooms are generated. DO NOT CALL
    public int dungeonSize;
    //prefeb for the object ot be instantiated as a room in the dungeon. DO NOT CALL
    public GameObject room;
    //A list containing all of the rooms in the dungeon. List elements are GameObjects representing instances of a room.
    public List<GameObject> rooms = new List<GameObject>();
    
    //THE FOLLOW ARE USED FOR DUGEON GENERATION. DO NOT CALL
    [HideInInspector]public bool dungeonGenerated = false;
    [HideInInspector]public int offset;
    private Dictionary<Vector3, GameObject> roomLocations = new Dictionary<Vector3, GameObject>();
    // Start is called before the first frame update
    //acts as the constructor for this class
    void Start()
    {
        offset = room.GetComponent<Room>().roomSize;
    }

    //CreateDugeon
    //Return type: void
    //Parameters: none
    //Starts the coroutine that will generate the dungeon.
    public void CreateDungeon()
    {
        StartCoroutine(GenerateDungeon());
    }

    //NONE OF THE FOLLOWING METHODS/COROUTINES SHOULD BE ACCESSED BY SCRIPTS OUTSIDE THE CLASS
    //Generated the dungeon
    //Coroutine GenerateDungeon
    //Return type none
    //Parameters none
    //Generated a dungeon by randomly placing rooms adjacent to one another.
    IEnumerator GenerateDungeon()
    {
        Dictionary<char, char> cardinal = new Dictionary<char, char>();
        cardinal.Add('W','E');
        cardinal.Add('N','S');
        cardinal.Add('E','W');
        cardinal.Add('S','N');
        //generate starting room
        GameObject firstRoom = Instantiate(room, transform.position, Quaternion.identity) as GameObject;
        yield return new WaitUntil(() => firstRoom.GetComponent<Room>().roomGenerated == true);
        firstRoom.GetComponent<Room>().roomType = "Start";
        rooms.Add(firstRoom);
        roomLocations.Add(firstRoom.transform.position, firstRoom);
        firstRoom.transform.SetParent(this.transform);
        GameObject currRoom = firstRoom;
        for (int i = 1; i < dungeonSize; i++)
        {
            char direction = TraverseRooms(ref currRoom);
            float x = currRoom.transform.position.x;
            float y = currRoom.transform.position.y;
            if (direction == 'W')
            {
                x -= offset;
            }
            else if (direction  == 'N')
            {
                y += offset;
            }
            else if (direction == 'E')
            {
                x += offset;
            }
            else if (direction  == 'S')
            {
                y -= offset;
            } 
            GameObject instance = Instantiate(room, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
            yield return new WaitUntil(() => instance.GetComponent<Room>().roomGenerated == true);
            rooms.Add(instance);
            roomLocations.Add(instance.transform.position, instance);
            instance.transform.SetParent(this.transform);
            CheckForNeighbors(ref instance, cardinal);
            currRoom = instance;
        }
        currRoom.GetComponent<Room>().roomType = "End";
        currRoom.GetComponent<Room>().SetLockStatus(true);
        dungeonGenerated = true;
        yield return null;
    }

    //TravereRooms
    //Return type: char
    //Parameters: ref Gameobject
    //Randomely traverse the existing rooms to find a new place to add a new room.
    //Return the direction the new room should be added, relative the current room it is being added to.
    //The gameobject curroom will be modified during the function as it operateds as a pointer to whatever room the game is looking at.
    //Traverse the stage, until currRoom reaches a dead end
    private char TraverseRooms(ref GameObject currRoom)
    {
        string directions = "WNES";
        int choice = Random.Range(0, directions.Length);
        char direction = directions[choice];
        GameObject nextRoom = currRoom.GetComponent<Room>().neighbors[direction];
        while (nextRoom != null) //This has the potential to loop infinitely. Would be agood idea to add predecessor and directions.
        {
            currRoom = nextRoom;
            choice = Random.Range(0, directions.Length);
            direction = directions[choice];
            nextRoom = currRoom.GetComponent<Room>().neighbors[direction];
        }
        return direction;
    }

    //Given a room, check if it has neighbors.
    //Utilizes a rooms position in the game to determine it's neighbors.
    private void CheckForNeighbors(ref GameObject newRoom, Dictionary<char, char> directions)
    {
        foreach(KeyValuePair<char, char> direction in directions)
        {
            Vector3 pos = newRoom.transform.position;
            switch (direction.Key)
            {
                case 'W':
                    pos.x -= offset;
                    break;
                case 'N':
                    pos.y += offset;
                    break;
                case 'E':
                    pos.x += offset;
                    break;
                case 'S':
                    pos.y -= offset;
                    break;
                default:
                    Debug.Log("Error: Invalid direction provided to CheckForNeighbors");
                    break;
            }
            GameObject currNeighbor;
            if (roomLocations.TryGetValue(pos, out currNeighbor))
            {
                newRoom.GetComponent<Room>().AddNeighbor(direction.Key, ref currNeighbor);
                currNeighbor.GetComponent<Room>().AddNeighbor(directions[direction.Key], ref newRoom);
            }
        }

    }
}
