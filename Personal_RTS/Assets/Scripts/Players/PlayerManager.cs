using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Players
{

    public class PlayerManager : MonoBehaviour
    {
        static public PlayerManager Instance
        {
            get;
            private set;
        }

        PlayerManager()
        {
            if (Instance != null)
                Destroy(Instance);

            Instance = this;
        }

        public Player[] PlayerList
        {
            get;
            private set;
        }

        public int PlayerCount;

        public Vector3 InitialCameraHeight;

        public GameObject PlayerCameraPrefab;

        // Use this for initialization
        void Start()
        {
            Initialize();
        }

        void Initialize()
        {
            PlayerList = new Player[PlayerCount];

            for (int i = 0; i < PlayerCount; ++i)
                PlayerList[i] = new Player(i, InitialCameraHeight, PlayerCameraPrefab);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}