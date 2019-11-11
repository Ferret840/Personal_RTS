using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public uint subsectorInstance = uint.MaxValue;
    ///<summary>
    ///Always 0, 1, 2, or 3. Set bits indicate walkable in that dimension
    ///</summary>
    /*public bool isWalkable
    {
        get;
        private set;
    }

    public Node(bool _isWalkable)
    {
        isWalkable = _isWalkable;
    }

    public bool SetWalkable(bool _newIsWalkable)
    {
        return isWalkable = _newIsWalkable;
    }*/
}
