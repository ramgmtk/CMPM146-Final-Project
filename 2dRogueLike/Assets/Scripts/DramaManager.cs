﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoomDirector), typeof(PlayerPlanner))]
public class DramaManager : MonoBehaviour
{
    public float dramaMeter;
    public const float MAX_DRAMAMETER = 100;
    public const float MIN_DRAMAMETER = 0;
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
                        //subtract player health from enemy damage, blocking by how many upgrades the player has
                        player.health = player.health - pp.enemyDamage + player.upgrades;
                        player.enemiesEncountered += 1;
                        //UpdateDramaMeter(ref player);
                    }
                    else if (currentElements[i, j].tag == "Key")
                    {
                        player.keyCount += 1;
                    }
                    else if (currentElements[i, j].tag == "Food")
                    {
                        player.health +=  5;
                        //UpdateDramaMeter(ref player);
                    }
                    rd.EditElementInRoom(ref room, i, j, null);
                }
            }
        }
        //After you encounter 10 enemies, upgrade
        if (player.enemiesEncountered - player.upgrades*10 >= 10)
        {
          player.upgrades += 1;
          Debug.Log(string.Format("Upgrade! {0}", player.upgrades));
        }
        UpdateDramaMeter(ref player);
        if (!player.visited.ContainsKey(room))
        {
            player.roomCount += 1;
            player.visited.Add(room, true);
        }
        Debug.Log(string.Format(
            "After Room Visit; PlayerStats = {0} HP, {1} keys, {2} enemies encountered, {3} rooms visited; dramaMeter at {4}",
            player.health, player.keyCount, player.enemiesEncountered, player.roomCount, dramaMeter));
    }

    public void DramatizeRoom(ref GameObject room, ref PlayerStats player)
    {
        if (player.visited.ContainsKey(room))
        {
            return;
        }
        //Fill in code here
        System.Random rng = new System.Random();
        int NumEnemies()
        {
            if(dramaMeter < 20)
            {
                return rng.Next(3, 7);
            }
            else if(dramaMeter < 45)
            {
                return rng.Next(2, 4);
            }
            else {
                return rng.Next(1, 2);
            }
        }

        /** Phases for drama
         * Increasing Intensity
         *  - Add more enemies in next room
         * Relaxation
         *  - Add Food and fewer/no enemies
        */
        // Increasing Intensity Phase - Add more enemies
        if (dramaMeter < 60)
		{
            // Set # of enemies to spawn from 0 to dramaMeter
            int numEnemyCount = NumEnemies();
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

            int numFoodCount = 0;
            if(player.health < 40)
            {
                numFoodCount = 4;
            }
            else if(player.health < 20)
            {
              numFoodCount = 5;
            }
            /*int foo  = rng.Next(1, 10);
            if ( foo <= 5)
            {
                numFoodCount = 50;
            }*/
            for (int i = 0; i < numFoodCount; i++)
            {
              rd.AddElementRandomly(room, rd.food);
            }
            Debug.Log(string.Format("Food added: {0}", numFoodCount));
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
                numFoodCount = 6;
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

    // Player stats: heatlh, keyCount, enemiesEncountered, roomCount
    public void UpdateDramaMeter(ref PlayerStats player)
	{
        float newDramaMeter = dramaMeter;

        // increase drama as more rooms are explored
        // ########################
        // Thought for increasing
        // At the beginning there should not be many enemies
        // Depending on health for the next coulpe rooms will decide the next Drama
        // As you continue, drama will increase, but if you are low health decrease drama inorder to gain health
        // ########################
        // int increasingIntesity = 0.5 * player.roomCount * player.roomCount;
        int increasingIntesity = 10 * player.roomCount;

        if(player.health == 100){
            newDramaMeter = 100 - player.health*.55f - increasingIntesity;
        }
        else if(player.health > 25)
        {
            newDramaMeter = 100 - player.health*.8f - increasingIntesity;
        }
        else
        {
          newDramaMeter = 100 - increasingIntesity + player.health;
        }

        if (newDramaMeter > MAX_DRAMAMETER)
		{
            newDramaMeter = MAX_DRAMAMETER;
		}
        else if (newDramaMeter < MIN_DRAMAMETER)
        {
            newDramaMeter = MIN_DRAMAMETER;
        }
        dramaMeter = newDramaMeter;
	}

    // /// <summary>
	// /// Updates the dramaMeter field, restricting it to
	// /// MAX_DRAMAMETER
	// /// </summary>
	// /// <param name="delta">how much to change (additive)</param>
    // public void UpdateDramaMeter(float delta = 0)
	// {
    //     float newDramaMeter = dramaMeter + delta;
    //     if (newDramaMeter > MAX_DRAMAMETER)
	// 	{
    //         newDramaMeter = MAX_DRAMAMETER;
	// 	}
    //     dramaMeter = newDramaMeter;
	// }
}
