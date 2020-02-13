using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainData;

namespace Pathing
{

    public class IntegrationField
    {
        Goal goal;

        IFieldNode[,][,] grid;

        public IntegrationField(Goal _goal)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

            timer.Start();

            goal = _goal;

            Grid g = Grid.GetGrid;

            grid = new IFieldNode[g.xSectorCount, g.ySectorCount][,];

            int lastSectorRemainderX = (int)(g.gridWorldSize.x % g.nodesPerSector), 
                lastSectorRemainderY = (int)(g.gridWorldSize.y % g.nodesPerSector);

            for (int xS = 0; xS < g.xSectorCount; ++xS)
            {
                for (int yS = 0; yS < g.ySectorCount; ++yS)
                {
                    int xSize = (xS + 1) * g.SectorSize <= g.gridWorldSize.x ? g.nodesPerSector : lastSectorRemainderX,
                        ySize = (yS + 1) * g.SectorSize <= g.gridWorldSize.y ? g.nodesPerSector : lastSectorRemainderY;

                    grid[xS, yS] = new IFieldNode[xSize, ySize];

                    for (int xN = 0; xN < xSize; ++xN)
                    {
                        for (int yN = 0; yN < ySize; ++yN)
                        {
                            grid[xS, yS][xN, yN] = new IFieldNode(xN + xS * g.nodesPerSector, yN + yS * g.nodesPerSector, this);
                        }
                    }
                }
            }
            UnityEngine.Debug.Log("Integration Field Allocation: " + timer.ElapsedMilliseconds + "ms");
            timer.Reset();
            timer.Start();
            Calculate();

            timer.Stop();
            UnityEngine.Debug.Log("Integration Field Calculation: " + timer.ElapsedMilliseconds + "ms");
        }

        void Calculate()
        {
            Grid g = Grid.GetGrid;

            IFieldNode n = grid[goal.xSector, goal.ySector][goal.xPos, goal.yPos];

            int circumference = (int)(Mathf.Min(g.gridSizeX, g.gridSizeY) * 2 * Mathf.PI);
            Queue<IFieldNode> openList = new Queue<IFieldNode>(circumference);

            n.Distance = 0;

            openList.Enqueue(n);

            while (openList.Count > 0)
            {
                n = openList.Dequeue();

                n.Used = true;

                foreach (IFieldNode neighbor in n.GetNeighbors(goal.Dimension))
                {
                    if (neighbor.Used)
                        continue;

                    neighbor.Distance = n.Distance + 1;
                    neighbor.Used = true;
                    openList.Enqueue(neighbor);
                }
            }
        }

        public int this[int x, int y]
        {
            get
            {
                Grid g = Grid.GetGrid;

                return grid[x / g.nodesPerSector, y / g.nodesPerSector][x % g.nodesPerSector, y % g.nodesPerSector].Distance;
            }
        }

        protected class IFieldNode
        {
            public int Distance = int.MaxValue;
            int xPos, yPos;
            IntegrationField iField;
            public bool Used = false;

            public IFieldNode(int x, int y, IntegrationField i)
            {
                xPos = x;
                yPos = y;
                iField = i;
            }

            public List<IFieldNode> GetNeighbors(char dim)
            {
                Grid g = Grid.GetGrid;

                List<IFieldNode> neighbors = new List<IFieldNode>();

                for (int x = -1; x < 2; ++x)
                {
                    for (int y = -1; y < 2; ++y)
                    {
                        int checkX = xPos + x;
                        int checkY = yPos + y;
                        if (Mathf.Abs(x) == Mathf.Abs(y))
                            continue;
                        else if (checkX < 0 || checkX >= g.gridSizeX || checkY < 0 || checkY >= g.gridSizeY)
                            continue;

                        if (g.GetWalkableAt(dim, checkX, checkY))
                        {
                            //int atX = (int)((checkX + 1) * g.SectorSize <= g.gridWorldSize.x ? g.SectorSize / (g.nodeRadius * 2) : g.gridWorldSize.x % g.SectorSize / (g.nodeRadius * 2)),
                            //    atY = (int)((checkY + 1) * g.SectorSize <= g.gridWorldSize.y ? g.SectorSize / (g.nodeRadius * 2) : g.gridWorldSize.y % g.SectorSize / (g.nodeRadius * 2));
                            neighbors.Add(iField.grid[checkX / g.nodesPerSector, checkY / g.nodesPerSector][checkX % g.nodesPerSector, checkY % g.nodesPerSector]);
                        }
                    }
                }

                return neighbors;
            }
        }
    }
    
}