using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoomGenerator))]
public class PlayerPlanner : MonoBehaviour
{
    public bool plannerCanStart = false;
    private GameObject player;
    private PlayerStats ps;
    private List<GameObject> rooms;
    private List<char> path;
    private GameObject startRoom;
    private GameObject endRoom;
    private GameObject currentLocation;
    Dictionary<char, char> cardinal = new Dictionary<char, char>();
    int moveCount = 0;
    
    //Personal MinHeap class creation
    class MinHeap<T>
    {
        public List<(float, T)> queue = new List<(float, T)>();
        public int count;
        public MinHeap()
        {
            queue.Add((0f, default(T)));
            count = 0;
        }
        private void Heapify(int index)
        {
            if (2*index > this.count || 2*index + 1 > this.count) //maybe or should be and
            {
                if (2*index <= count)
                {
                    if (queue[2 *index].Item1 < queue[index].Item1)
                    {
                        (float, T) specialChild = queue[2*index];
                        queue[2*index] = queue[index];
                        queue[index] = specialChild;
                        Heapify(2*index);
                    }
                }
                return;
            }
            else if (queue[index].Item1 <= queue[2*index].Item1 && queue[index].Item1 <= queue[2*index + 1].Item1)
            {
                return;
            }
            else
            {
                (float, T) child;
                int tempIndex;
                if (queue[2*index].Item1 < queue[2*index + 1].Item1)
                {
                    child = queue[2*index];
                    tempIndex = 2*index;
                }
                else
                {
                    child = queue[2*index + 1];
                    tempIndex = 2*index + 1;
                }
                (float, T) parent = queue[index];
                queue[index] = child;
                queue[tempIndex] = parent;
                Heapify(tempIndex);
            }
        }

        public (float, T) ExtractMin()
        {
            if (count < 1)
            {
                return (0, default(T));
            }
            (float, T) returnValue = queue[1];
            queue[1] = queue[count];
            queue[count] = returnValue;
            count--;
            Heapify(1);
            return returnValue;
        }

        private void IncreaseKey(int index, float priority)
        {
            queue[index] = (priority, queue[index].Item2);
            while (index != 1 && queue[index / 2].Item1 > queue[index].Item1)
            {
                (float, T) child = queue[index];
                queue[index] = queue[index / 2];
                queue[index / 2] = child;
                index = index / 2;
            }
        }

        public void Insert((float, T) obj)
        {
            (float, T) newInsert = (float.PositiveInfinity, obj.Item2);
            queue.Insert(count + 1, newInsert);
            count++;
            IncreaseKey(count, obj.Item1);
        }
    }
    //End of minheap class
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        ps = player.GetComponent<PlayerStats>();
        cardinal.Add('W','E');
        cardinal.Add('N','S');
        cardinal.Add('E','W');
        cardinal.Add('S','N');
    }

    // Update is called once per frame
    void Update()
    {
        if (plannerCanStart && Input.GetKeyDown("space"))
        {
            DoMovement();
        }
    }

    private void DoMovement()
    {
        if (moveCount < path.Count)
        {
            GameObject nextLocation = currentLocation.GetComponent<Room>().neighbors[path[moveCount]];
            currentLocation = nextLocation;
            player.transform.position = new Vector3(currentLocation.transform.position.x, currentLocation.transform.position.y, player.transform.position.z);
            moveCount++;
        }
    }
    public void InitializePlanner()
    {
        rooms = GetComponent<RoomGenerator>().rooms;
        Debug.Log(rooms.Count);
        startRoom = rooms[0];
        endRoom = rooms[rooms.Count - 1];
        currentLocation = startRoom;
        path = FindPath();
    }

    private List<char> FindPath()
    {
        MinHeap<GameObject> queue = new MinHeap<GameObject>();
        Dictionary<GameObject, char> pathPredecessor = new Dictionary<GameObject, char>();
        Dictionary<GameObject, float> pathCost = new Dictionary<GameObject, float>();
        List<char> resultingPath = new List<char>();

        queue.Insert((0, startRoom));
        pathPredecessor.Add(startRoom, 'X');
        pathCost.Add(startRoom, 0f);

        while (queue.count > 0)
        {
            (float, GameObject) data = queue.ExtractMin();
            GameObject currentRoom = data.Item2;
            float currentCost = pathCost[currentRoom];

            if (currentRoom == endRoom)
            {
                //Do stuff
                char direction = pathPredecessor[currentRoom];
                Debug.Log("Found End Room: " + pathPredecessor.Count);
                Debug.Log(startRoom.GetComponent<Room>().roomType);
                while (direction != 'X')
                {
                    resultingPath.Add(direction);
                    currentRoom = currentRoom.GetComponent<Room>().neighbors[cardinal[direction]];
                    direction = pathPredecessor[currentRoom];
                }
                resultingPath.Reverse();
                Debug.Log("Assembled Path");
                plannerCanStart = true;
                break;
            }

            foreach(KeyValuePair<char, GameObject> neighbor in currentRoom.GetComponent<Room>().neighbors)
            {
                if (neighbor.Value ==  null)
                {
                    continue;
                }
                float travelCost = currentCost + 1f; //where 1 indicates the number of steps to get from the current room to this 1. Since rooms can only travel to their neighbors, we use the value 1.
                if (!pathCost.ContainsKey(neighbor.Value) || pathCost[neighbor.Value] > travelCost)
                {
                    pathCost[neighbor.Value] = travelCost;
                    float priority = travelCost + Heuristic(neighbor.Value);
                    queue.Insert((priority, neighbor.Value));
                    if (pathPredecessor.ContainsKey(neighbor.Value))
                    {
                        pathPredecessor[neighbor.Value] = neighbor.Key;
                    }
                    else
                    {
                        pathPredecessor.Add(neighbor.Value, neighbor.Key);
                    }
                }
            }
        }
        return resultingPath;
    }

    private float Heuristic(GameObject currRoom)
    {
        //For now return the euclidean distance.
        float val = Vector3.Distance(currRoom.transform.position, endRoom.transform.position);
        return val;
    }


}
