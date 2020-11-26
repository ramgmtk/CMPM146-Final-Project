using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RoomGenerator), typeof(RoomDirector))]
public class GameModerator : MonoBehaviour
{
    public static GameModerator instance = null;
    private RoomGenerator rg;
    private RoomDirector rd;
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
    }
}
