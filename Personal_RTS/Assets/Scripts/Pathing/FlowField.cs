using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainData;

namespace Pathing
{

    public class FlowField
    {
        float[,] grid;
    
        public float this[int x, int y]
        {
            get
            {
                return grid[x, y];
            }
        }
    
        public FlowField(IntegrationField iField)
        {
            Grid g = Grid.GetGrid;
    
            grid = new float[g.gridSizeX, g.gridSizeY];
    
            for (int x = 0; x < g.gridSizeX; ++x)
            {
                for (int y = 0; y < g.gridSizeY; ++y)
                {
                    ushort minD = ushort.MaxValue;
                    int minX = 0, minY = 0;
    
                    for (int xDelta = -1; xDelta < 2; ++xDelta)
                    {
                        for (int yDelta = -1; yDelta < 2; ++yDelta)
                        {
                            int atX = x + xDelta;
                            int atY = y + yDelta;
    
                            if (xDelta == 0 && yDelta == 0)
                                continue;
                            else if (atX < 0 || atX >= g.gridSizeX || atY < 0 || atY >= g.gridSizeY)
                                continue;
    
                            ushort checkD = iField[atX, atY];
                            if (checkD <= minD)
                            {
                                minD = checkD;
                                minX = xDelta;
                                minY = yDelta;
                            }
                        }
                    }

                    //Because Unity is stupid and circle start at up and rotate clockwise
                    if (minD == ushort.MaxValue)
                        grid[x, y] = 0;
                    else if (minX == 1 && minY == 0)
                        grid[x, y] = 90;
                    else if (minX == -1 && minY == 0)
                        grid[x, y] = 270;
                    else if (minX == 0 && minY == 1)
                        grid[x, y] = 0;
                    else if (minX == 0 && minY == -1)
                        grid[x, y] = 180;
                    else if (minX == 1 && minY == 1)
                        grid[x, y] = 45;
                    else if (minX == -1 && minY == 1)
                        grid[x, y] = 315;
                    else if (minX == 1 && minY == -1)
                        grid[x, y] = 135;
                    else if (minX == -1 && minY == -1)
                        grid[x, y] = 225;
                    else
                        grid[x, y] = 1000f;
                }
            }
        }
    }
    
}