using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Players
{

    public class Player
    {
        int m_PlayerNumber;

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

        public Player(int _playerNum, Vector3 _initialHeight, GameObject _playerCamPrefab)
        {
            m_PlayerNumber = _playerNum;

            GameObject g = (GameObject)GameObject.Instantiate(_playerCamPrefab, _initialHeight, Quaternion.Euler(75, 0, 0));
            Cam = g.GetComponent<PlayerCamera>();
            Cam.m_PlayerNumber = m_PlayerNumber;

            Selector = g.GetComponent<UnitSelection>();
            Selector.m_PlayerNumber = m_PlayerNumber;
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