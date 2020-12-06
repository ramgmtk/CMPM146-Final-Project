using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class PlayerStats
//Container for the players attributes (inventory, stats etc)
//Add attributes as necessary
//Script is held by the player GameObject
public class PlayerStats : MonoBehaviour
{
    public int health = 100;
    public int keyCount = 0;
    public int enemiesEncountered = 0;
    public int roomCount = 0;
    public Dictionary<GameObject, bool> visited = new Dictionary<GameObject, bool>();
}

public class StatContainer
{
    public int health;
    public int keyCount;
    public int enemiesEncountered;
    public int roomCount;
    public Dictionary<GameObject, bool> visited = new Dictionary<GameObject, bool>();
    public StatContainer(int h, int k, int e, int r, Dictionary<GameObject, bool> d)
    {
        health = h;
        keyCount = k;
        enemiesEncountered = e;
        roomCount = r;
        visited = new Dictionary<GameObject, bool>(d);
    }

    public override bool Equals(object o)
    {
        if (o == null || GetType() != o.GetType())
        {
            return false;
        }
        StatContainer obj = (StatContainer) o;
        return this == (StatContainer)o;
    }
    public static bool operator ==(StatContainer lhs, StatContainer rhs)
    {
        if (lhs.health== rhs.health && lhs.keyCount == rhs.keyCount && lhs.enemiesEncountered == rhs.enemiesEncountered && lhs.roomCount == rhs.roomCount)
        {
            return true;
        }
        return false;
    }
    public static bool operator !=(StatContainer lhs, StatContainer rhs)
    {
        if (lhs.health != rhs.health && lhs.keyCount != rhs.keyCount && lhs.enemiesEncountered != rhs.enemiesEncountered && lhs.roomCount != rhs.roomCount)
        {
            foreach(KeyValuePair<GameObject, bool> visit in lhs.visited)
            {
                if (!rhs.visited.ContainsKey(visit.Key))
                {
                    return false;
                }
            }
            return true;
        }
        return false;;
    }

    public override int GetHashCode() 
    {
            return base.GetHashCode();
    }
}
