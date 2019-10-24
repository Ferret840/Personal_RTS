using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sector
{
    public Color sectorColor
    {
        get;
        private set;
    }

    //Table of node locations. 1 = Walkable, 0 = Impassable terrain
    bool[,] grid;

    List<SubSector> subs;

    public Sector(LayerMask dim, int xSize, int ySize, Vector3 cornerPos, float nodeRadius)
    {
        Color c = Random.ColorHSV();
        c.a = 0.5f;
        sectorColor = c;

        grid = new bool[xSize, ySize];

        for (int x = 0; x < xSize; ++x)
        {
            for (int y = 0; y < ySize; ++y)
            {
                grid[x, y] = true;

                Vector3 worldPoint = cornerPos + Vector3.right * (x * nodeRadius * 2) + Vector3.forward * (y * nodeRadius * 2);

                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);

                //Dimension
                RaycastHit[] hits = Physics.RaycastAll(ray, 100, dim);
                foreach (RaycastHit h in hits)
                {
                    if (h.transform.gameObject.tag == "Unit")
                        continue;

                    grid[x, y] = false;
                    break;
                }
                if (dim == 1 << 8 || dim == 1 << 9)
                {
                    hits = Physics.RaycastAll(ray, 100, 1 << 10);
                    foreach (RaycastHit h in hits)
                    {
                        if (h.transform.gameObject.tag == "Unit")
                            continue;

                        grid[x, y] = false;
                        break;
                    }
                }
            }
        }
    }

    public bool GetWalkableAt(int x, int y)
    {
        return grid[x, y];
    }

    public bool SetWalkableAt(int x, int y, bool newCanWalk)
    {
        return grid[x, y] = newCanWalk;
    }

    private class SubSector
    {

    }
}
