using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Players
{

    public class Player
    {
        int PlayerNumber;

        public PlayerCamera Cam
        {
            get;
            private set;
        }

        public UnitSelection Selector
        {
            get;
            private set;
        }

        public Player(int _playerNum, Vector3 initialHeight, GameObject playerCamPrefab)
        {
            PlayerNumber = _playerNum;

            GameObject g = (GameObject)GameObject.Instantiate(playerCamPrefab, initialHeight, Quaternion.Euler(75, 0, 0));
            Cam = g.GetComponent<PlayerCamera>();
            Cam.PlayerNumber = PlayerNumber;

            Selector = g.GetComponent<UnitSelection>();
            Selector.PlayerNumber = PlayerNumber;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}