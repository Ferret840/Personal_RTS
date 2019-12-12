using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    static public PlayerManager instance
    {
        get;
        private set;
    }

    public Player[] PlayerList
    {
        get;
        private set;
    }

    public int PlayerCount;

    public Vector3 InitialCameraHeight;


    // Use this for initialization
    void Start ()
    {
        instance = this;

        PlayerList = new Player[PlayerCount];

        for (int i = 0; i < PlayerCount; ++i)
            PlayerList[i] = new Player(i, InitialCameraHeight);
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
