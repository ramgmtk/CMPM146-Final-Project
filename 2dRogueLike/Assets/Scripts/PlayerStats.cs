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
}

public class StatContainer
{
    public int health;
    public int keyCount;
    public Dictionary<GameObject, bool> visited = new Dictionary<GameObject, bool>();
    public StatContainer(int h, int k, Dictionary<GameObject, bool> d)
    {
        health = h;
        keyCount = k;
        visited = new Dictionary<GameObject, bool>(d);
    }
}
