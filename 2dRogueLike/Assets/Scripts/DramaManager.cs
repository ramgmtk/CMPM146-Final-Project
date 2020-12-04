﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoomDirector), typeof(PlayerPlanner))]
public class DramaManager : MonoBehaviour
{
    public float dramaMeter;
    public const float MAX_DRAMAMETER = 100;
    private RoomDirector rd;
    private PlayerPlanner pp;
    // Start is called before the first frame update
    void Start()
    {
        rd = GetComponent<RoomDirector>();
        pp = GetComponent<PlayerPlanner>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Update a room that a player visits
    //This will remove the elements from the room, performing their respective actions on the player.
    //ex: enemies will subtract from the player health, and keys will be added to player inventory.
    //This function is called from the playerplanner.
    //Drama management should be done before the room update is called.
    public void UpdateRoom(ref GameObject room, ref PlayerStats player)
    {
        Debug.Log(string.Format(
            "Before Room Visit; PlayerStats = {0}HP, {1} keys; dramaMeter at {2}",
            player.health, player.keyCount, dramaMeter));
        GameObject[,] currentElements = room.GetComponent<Room>().elements;
        if (room.GetComponent<Room>().locked)
        {
            room.GetComponent<Room>().SetLockStatus(false);
            player.keyCount -= 1;
        }
        for (int i = 0; i < currentElements.GetLength(0); i++)
        {
            for (int j = 0; j < currentElements.GetLength(0); j++)
            {
                if (currentElements[i,j])
                {
                    // UpdateDramaMeter(float) used to update dramaMeter
                    if (currentElements[i, j].tag == "Enemy")
                    {
                        player.health = player.health - pp.enemyDamage;
                        UpdateDramaMeter((float)(100 - player.health) / 10);
                    }
                    else if (currentElements[i, j].tag == "Key")
                    {
                        player.keyCount += 1;
                    }
                    else if (currentElements[i, j].tag == "Food")
                    {
                        player.health +=  5;
                        UpdateDramaMeter(-5);
                    }
                    rd.EditElementInRoom(ref room, i, j, null);
                }
            }
        }
        Debug.Log(string.Format(
            "After Room Visit; PlayerStats = {0}HP, {1} keys; dramaMeter at {2}",
            player.health, player.keyCount, dramaMeter));
    }

    public void DramatizeRoom(ref GameObject room, ref PlayerStats player)
    {
        //Fill in code here
        System.Random rng = new System.Random();
        /** Phases for drama
         * Increasing Intensity
         *  - Add more enemies in next room
         * Relaxation
         *  - Add Food and fewer/no enemies
        */
        // Increasing Intensity Phase - Add more enemies
        if (dramaMeter < 40)
		{
            // Set # of enemies to spawn from 0 to dramaMeter
            int numEnemyCount = rng.Next((int)dramaMeter);
            // Clamp this number so that we don't overgenerate too many enemies
            const int UPPER_ENEMY_LIMIT = 8;
            if (numEnemyCount > UPPER_ENEMY_LIMIT)
			{
                numEnemyCount = UPPER_ENEMY_LIMIT;
			}
            // Spawn Enemies
            for (int i = 0; i < numEnemyCount; i++)
			{
                rd.AddElementRandomly(room, rd.enemy);
			}
            Debug.Log(string.Format("Enemies added: {0}", numEnemyCount));
		}
        else
		{
            // Check player HP to decide how much food to provide
            int numFoodCount = 0;
            if (player.health < 25)
			{
                numFoodCount = 10;
			}
            else if (player.health < 50)
			{
                numFoodCount = 5;
			}
            else if (player.health < 75)
			{
                numFoodCount = 3;
			}
            else
			{
                numFoodCount = rng.Next(2); // either 1 or 0 food
			}
            for (int i = 0; i < numFoodCount; i++)
			{
                rd.AddElementRandomly(room, rd.food);
			}
            Debug.Log(string.Format("Food added: {0}", numFoodCount));
		}
        
        return;
    }

    /// <summary>
	/// Updates the dramaMeter field, restricting it to
	/// MAX_DRAMAMETER
	/// </summary>
	/// <param name="delta">how much to change (additive)</param>
    public void UpdateDramaMeter(float delta = 0)
	{
        float newDramaMeter = dramaMeter + delta;
        if (newDramaMeter > MAX_DRAMAMETER)
		{
            newDramaMeter = MAX_DRAMAMETER;
		}
        dramaMeter = newDramaMeter;
	}
}
