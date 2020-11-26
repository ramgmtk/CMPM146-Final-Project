using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
