using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class GameModerator
//Container class which Generated the dungeon as well as modifies it.
//For the time being nothing should be modified in the file.
[RequireComponent(typeof(RoomGenerator), typeof(RoomDirector))]
public class GameModerator : MonoBehaviour
{
    public static GameModerator instance = null;
    private RoomGenerator rg;
    private RoomDirector rd;
    private PlayerPlanner pp;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        rg = GetComponent<RoomGenerator>();
        rd = GetComponent<RoomDirector>();
        pp = GetComponent<PlayerPlanner>();
        InitGame();
        StartCoroutine(ModerateGame());
    }

    private void InitGame()
    {
        rg.CreateDungeon();
        rd.InitializeDirector();
    }

    IEnumerator ModerateGame()
    {
        yield return new WaitUntil(() => rg.dungeonGenerated == true);
        rd.DoStuff();
        pp.InitializePlanner();
    }
}
