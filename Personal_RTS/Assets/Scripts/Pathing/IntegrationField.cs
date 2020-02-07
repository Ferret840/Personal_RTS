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
            goal = _goal;

            Grid g = Grid.GetGrid;

            grid = new IFieldNode[g.xSectorCount, g.ySectorCount][,];

            for (int xS = 0; xS < g.xSectorCount; ++xS)
            {
                for (int yS = 0; yS < g.ySectorCount; ++yS)
                {
                    int xSize = (int)((xS + 1) * g.SectorSize <= g.gridWorldSize.x ? g.nodesPerSector : g.gridWorldSize.x % g.nodesPerSector),
                        ySize = (int)((yS + 1) * g.SectorSize <= g.gridWorldSize.y ? g.nodesPerSector : g.gridWorldSize.y % g.nodesPerSector);

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

            Calculate();
        }

        void Calculate()
        {
            Grid g = Grid.GetGrid;

            IFieldNode n = grid[goal.xSector, goal.ySector][goal.xPos, goal.yPos];

            Queue<IFieldNode> openList = new Queue<IFieldNode>();

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

                    neighbor.Distance = (ushort)(n.Distance + 1);
                    neighbor.Used = true;
                    openList.Enqueue(neighbor);
                }
            }
        }

        public ushort this[int x, int y]
        {
            get
            {
                Grid g = Grid.GetGrid;

                return grid[x / g.nodesPerSector, y / g.nodesPerSector][x % g.nodesPerSector, y % g.nodesPerSector].Distance;
            }
        }

        protected class IFieldNode
        {
            public ushort Distance = ushort.MaxValue;
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
                            try
                            {
                                neighbors.Add(iField.grid[checkX / g.nodesPerSector, checkY / g.nodesPerSector][checkX % g.nodesPerSector, checkY % g.nodesPerSector]);
                            }
                            catch
                            {
                                Debug.Log(string.Format("CheckX {0}, CheckY {1}, NodesPerSector {2}", checkX, checkY, g.nodesPerSector));
                            }
                        }
                    }
                }

                return neighbors;
            }
        }
    }
    
}