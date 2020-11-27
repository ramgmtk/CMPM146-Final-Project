using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class Root
//This script is what is being used to start the game.
//It essentialyl says: if a game does not exist already exist when we load in, start the game.
public class Root : MonoBehaviour
{
    public GameObject gm;

    void Awake()
    {
        if (GameModerator.instance == null)
        {
            Instantiate(gm);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
