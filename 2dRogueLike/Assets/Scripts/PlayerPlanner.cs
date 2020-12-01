using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoomGenerator))]
public class PlayerPlanner : MonoBehaviour
{
    private PlayerStats ps;
    private List<GameObject> rooms;
    private List<char> path;
    private GameObject startRoom;
    private GameObject endRoom;
    Dictionary<char, char> cardinal = new Dictionary<char, char>();
    
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
        public void Heapify(int index)
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

        public void IncreaseKey(int index, float priority)
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
        rooms = GetComponent<RoomGenerator>().rooms;
        ps = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
        cardinal.Add('W','E');
        cardinal.Add('N','S');
        cardinal.Add('E','W');
        cardinal.Add('S','N');
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializePlanner()
    {
        startRoom = rooms[0];
        endRoom = rooms[rooms.Count - 1];
    }

    /*private List<char> FindPath()
    {
        Dictionary<char, GameObject> pathPredecessor;
        Queue pathQueue = new Queue();
    }*/


}
