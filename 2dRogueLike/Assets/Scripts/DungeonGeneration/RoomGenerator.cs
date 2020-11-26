using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public int dungeonSize;
    public GameObject room;
    public List<GameObject> rooms = new List<GameObject>();
    [HideInInspector]public bool dungeonGenerated = false;
    [HideInInspector]public int offset;
    private Dictionary<Vector3, GameObject> roomLocations = new Dictionary<Vector3, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        offset = room.GetComponent<Room>().roomSize;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateDungeon()
    {
        StartCoroutine(GenerateDungeon());
    }

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
        dungeonGenerated = true;
        yield return null;
    }

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
