using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainData
{
    static class Layers
    {
        public const int OnlyDimension1 = (1 << 8);
        public const int OnlyDimension2 = (1 << 9);
        public const int OnlyDimension3 = (1 << 10);
        public const int Dimension1 = OnlyDimension1 + OnlyDimension3;
        public const int Dimension2 = OnlyDimension2 + OnlyDimension3;
        public const int Dimension3 = OnlyDimension1 + OnlyDimension2 + OnlyDimension3;
        public const int Default = 1 << 0;

        //Return a layermask with the given dimension number AND default
        static public int DimAndDefault(int dimNum)
        {
            switch (dimNum)
            {
                case 1: return Dimension1 + Default;
                case 2: return Dimension2 + Default;
                case 3: return Dimension3 + Default;
                default: return Default;
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
