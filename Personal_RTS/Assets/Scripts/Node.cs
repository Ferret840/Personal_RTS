using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    ///<summary>
    ///Always 0, 1, 2, or 3. Set bits indicate walkable in that dimension
    ///</summary>
    char walkable;
    public Vector3 worldPosition;

    public Node(char _blocked, Vector3 _worldPos)
    {
        walkable = _blocked;
        worldPosition = _worldPos;
    }

    public void SetWalkable(char newWalkable)
    {
        walkable = newWalkable;

        if (newWalkable != (char)0)
        {
            
        }
        else
        {

        }
    }

    /// <summary>
    /// Takes a dimension and returns True if this Node can be traversed by that dimension.
    /// </summary>
    /// <param name="dimension">The dimension to be checked against as a LayerMask. 2^8, 2^9, 2^10)</param>
    /// <returns>Returns true if the node can be traversed by the given dimension.</returns>
    public bool GetWalkableByDimension(LayerMask dimension)
    {
        char walkableBool = (char)0;

        if ((dimension & (1 << 8)) != 0)
        {
            walkableBool |= (char)1;
        }
        else if ((dimension & (1 << 9)) != 0)
        {
            walkableBool |= (char)2;
        }
        else if ((dimension & (1 << 10)) != 0)
        {
            if (walkable == (char)3)
            {
                return true;
            }

            return false;
        }

        return (walkable & walkableBool) != 0;
    }

    public char GetWalkable()
    {
        return walkable;
    }
}
