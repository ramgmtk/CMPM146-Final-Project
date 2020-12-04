using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoomGenerator))]
public class PlayerPlanner : MonoBehaviour
{
    public int enemyDamage = 5;
    public int keyWeight = 100;
    [HideInInspector]public bool plannerCanStart = false;
    private GameObject player;
    private PlayerStats ps;
    private DramaManager dm;
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

    //State class
    class State
    {
        public GameObject room;
        public StatContainer stats;
        public State(GameObject r, StatContainer s)
        {
            room = r;
            stats = new StatContainer(s.health, s.keyCount, s.visited);
        }

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }
            State obj = (State) o;
            return this == (State)o;
        }
        public static bool operator ==(State lhs, State rhs)
        {
            if (lhs.room == rhs.room && lhs.stats == rhs.stats)
            {
                return true;
            }
            return false;
        }
        public static bool operator !=(State lhs, State rhs)
        {
            if (lhs.room != rhs.room && lhs.stats != rhs.stats)
            {
                return true;
            }
            return false;;
        }

        public override int GetHashCode() 
        {
            return base.GetHashCode();
        }
    }
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        ps = player.GetComponent<PlayerStats>();
        dm = GetComponent<DramaManager>();
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
            dm.DramatizeRoom(ref currentLocation, ref ps);
            dm.UpdateRoom(ref currentLocation, ref ps);
            //Do drama management
            moveCount++;
        }
        else
        {
            Debug.Log("Goal Reached");
        }
    }
    public void InitializePlanner()
    {
        rooms = GetComponent<RoomGenerator>().rooms;
        startRoom = rooms[0];
        endRoom = rooms[rooms.Count - 1];
        currentLocation = startRoom;
        dm.UpdateRoom(ref currentLocation, ref ps); //Special case of first room. Because I am lazy. It just checks if a key/enemy are in first room and updates the player accordingly.
        path = FindPath();
    }

    private List<char> FindPath()
    {
        MinHeap<State> queue = new MinHeap<State>();
        Dictionary<State, (char, State)> pathPredecessor = new Dictionary<State, (char, State)>();
        Dictionary<State, float> pathCost = new Dictionary<State, float>();
        List<char> resultingPath = new List<char>();

        Dictionary<GameObject, bool> initVisited = new Dictionary<GameObject, bool>();
        //initVisited.Add(startRoom, true);
        StatContainer initPlayer = new StatContainer(ps.health, ps.keyCount, initVisited);
        State initState = new State(startRoom, initPlayer);
        queue.Insert((0, initState));
        pathPredecessor.Add(initState, ('X', null)); //since we visit a room multiple times, the key can't jsut be the room but the instead must be the state
        pathCost.Add(initState, 0f);

        while (queue.count > 0)
        {
            (float, State) data = queue.ExtractMin();
            State currentState = data.Item2;
            GameObject currentRoom = currentState.room;
            StatContainer currentPlayer = currentState.stats;
            float currentCost = pathCost[currentState];
            if (currentPlayer.visited.ContainsKey(currentRoom))
            {
                currentPlayer.visited[currentRoom] = true;
            }
            else
            {
                currentPlayer.visited.Add(currentRoom, true);
            }

            //If we have reached a destination
            if (currentRoom == endRoom)
            {
                (char, State) foo = pathPredecessor[currentState];
                char direction = foo.Item1;
                while (direction != 'X')
                {
                    resultingPath.Add(direction);
                    currentRoom = currentRoom.GetComponent<Room>().neighbors[cardinal[direction]];
                    foo = pathPredecessor[foo.Item2];
                    direction = foo.Item1;
                }
                resultingPath.Reverse();
                plannerCanStart = true; //consider moving this to after this entire function is finished. There is a error that could occur if player spams space on first frame.
                break;
            }

            foreach(KeyValuePair<char, GameObject> neighbor in currentRoom.GetComponent<Room>().neighbors)
            {
                if (neighbor.Value == null || (neighbor.Value.GetComponent<Room>().locked && currentPlayer.keyCount < 1))
                {
                    continue;
                }
                float travelCost = currentCost + 1f;
                State nextState = new State(neighbor.Value, currentState.stats);
                EvaluateRoom(ref nextState);
                if (!pathCost.ContainsKey(nextState) || pathCost[nextState] > travelCost) // || THE KEYCOUNT IS GREATER!
                {
                    StatContainer futureStats = new StatContainer(currentPlayer.health, currentPlayer.keyCount, currentPlayer.visited);
                    pathCost[nextState] = travelCost;
                    float priority = travelCost + Heuristic(neighbor.Value, ref futureStats);
                    queue.Insert((priority, nextState));
                    if (pathPredecessor.ContainsKey(nextState))
                    {
                        pathPredecessor[nextState] = (neighbor.Key, currentState);
                    }
                    else
                    {
                        pathPredecessor.Add(nextState, (neighbor.Key, currentState));
                    }
                }
            }
        }
        return resultingPath;
    }

    private float Heuristic(GameObject nextRoom, ref StatContainer futureStats)
    {
        //For now return the euclidean distance.
        float val = Vector3.Distance(nextRoom.transform.position, endRoom.transform.position);
        /*if (!futureStats.visited.ContainsKey(nextRoom))
        {
            return val;
        }*/
        return val;
    }

    private void EvaluateRoom(ref State st)
    {
        GameObject nextRoom = st.room;
        GameObject[,] currElements = nextRoom.GetComponent<Room>().elements;
        int keys = 0;
        int enemies  = 0;
        if (st.stats.visited.ContainsKey(nextRoom))
        {
            return;
        }
        for(int i = 0; i < currElements.GetLength(0); i++)
        {
            for (int j = 0; j < currElements.GetLength(1); j++)
            {
                if (currElements[i,j] != null)
                {
                    if (currElements[i,j].tag == "Enemy")
                    {
                        enemies += 1;
                    }
                    else if (currElements[i,j].tag == "Key")
                    {
                        keys += 1;
                    }
                }
            }
        }
        if (nextRoom.GetComponent<Room>().locked)
        {
            st.stats.keyCount -= 1;
        }
        st.stats.keyCount += keys;

        st.stats.health -= enemies * enemyDamage;
    }
}
