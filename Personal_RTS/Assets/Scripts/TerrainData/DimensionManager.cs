using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainData
{
    static class Layers
    {
        public const int m_s_OnlyDimension1 = (1 << 8);
        public const int m_s_OnlyDimension2 = (1 << 9);
        public const int m_s_OnlyDimension3 = (1 << 10);
        public const int m_s_Dimension1 = m_s_OnlyDimension1 + m_s_OnlyDimension3;
        public const int m_s_Dimension2 = m_s_OnlyDimension2 + m_s_OnlyDimension3;
        public const int m_s_Dimension3 = m_s_OnlyDimension1 + m_s_OnlyDimension2 + m_s_OnlyDimension3;
        public const int m_s_Default = 1 << 0;

        //Return a layermask with the given dimension number AND default
        static public int DimAndDefault_s(int _dimNum)
        {
            switch (_dimNum)
            {
                case 1: return m_s_Dimension1 + m_s_Default;
                case 2: return m_s_Dimension2 + m_s_Default;
                case 3: return m_s_Dimension3 + m_s_Default;
                default: return m_s_Default;
            }
        }
    }

    public class DimensionManager : MonoBehaviour
    {
        /*
    Grid[] grids;// = new List<Grid>();

    static public DimensionManager instance;

	// Use this for initialization
	void Awake ()
    {
        instance = this;

        grids = GameObject.FindObjectsOfType<Grid>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    static public Grid GetGridOfDimension(int i)
    {
        return instance.grids[i];
    }*/
    }

}
