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

        public int m_PlayerCount;

        public Vector3 m_InitialCameraHeight;

        public GameObject m_PlayerCameraPrefab;

        // Use this for initialization
        void Start()
        {
            Initialize();
        }

        void Initialize()
        {
            PlayerList = new Player[m_PlayerCount];

            for (int i = 0; i < m_PlayerCount; ++i)
                PlayerList[i] = new Player(i, m_InitialCameraHeight, m_PlayerCameraPrefab);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}