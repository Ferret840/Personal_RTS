using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainData;
using System.Threading;
using System.Diagnostics;

namespace Pathing
{

    public class FlowField
    {
        float[][] grid;
    
        public float this[int x, int y]
        {
            get
            {
                return grid[x][y];
            }
        }
    
        public FlowField(IntegrationField iField, int numThreads = 1)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            Grid g = Grid.GetGrid;
            int gridSizeX = g.gridSizeX, gridSizeY = g.gridSizeY;

            grid = new float[gridSizeX][];
            for(int i = 0; i < gridSizeX; ++i)
                grid[i] = new float[gridSizeY];

            Queue<Thread> generateThreads = new Queue<Thread>(numThreads);

            for (int i = 0; i < numThreads; ++i)
            {
                int i_thread = i;

                Thread t = new Thread(delegate ()
                {
                    for (int x = 0; x < gridSizeX; ++x)
                    {
                        UnityEngine.Debug.Log((i_thread + 1) * (gridSizeY / numThreads));
                        for (int y = i_thread * (gridSizeY / numThreads); y < (i_thread + 1) * (gridSizeY / numThreads); ++y)
                        {
                            CalculateSection(x, y, iField, gridSizeX, gridSizeY);
                        }
                    }
                });
                generateThreads.Enqueue(t);
                t.Start();
            }

            for (int x = 0; x < gridSizeX; ++x)
            {
                for (int y = numThreads * (gridSizeY / numThreads); y < gridSizeY; ++y)
                {
                    CalculateSection(x, y, iField, gridSizeX, gridSizeY);
                }
            }

            while (generateThreads.Count > 0)
            {
                generateThreads.Dequeue().Join();
            }

            timer.Stop();
            UnityEngine.Debug.Log("Flow Field: " + timer.ElapsedMilliseconds + "ms");
            

            //for (int x = 0; x < g.gridSizeX; ++x)
            //{
            //    for (int y = 0; y < g.gridSizeY; ++y)
            //    {
            //        ushort minD = ushort.MaxValue;
            //        int minX = 0, minY = 0;
            //
            //        for (int xDelta = -1; xDelta < 2; ++xDelta)
            //        {
            //            for (int yDelta = -1; yDelta < 2; ++yDelta)
            //            {
            //                int atX = x + xDelta;
            //                int atY = y + yDelta;
            //
            //                if (xDelta == 0 && yDelta == 0)
            //                    continue;
            //                else if (atX < 0 || atX >= g.gridSizeX || atY < 0 || atY >= g.gridSizeY)
            //                    continue;
            //
            //                ushort checkD = iField[atX, atY];
            //                if (checkD <= minD)
            //                {
            //                    minD = checkD;
            //                    minX = xDelta;
            //                    minY = yDelta;
            //                }
            //            }
            //        }
            //
            //        //Because Unity is stupid and circles start at up and rotate clockwise
            //        if (minD == ushort.MaxValue)
            //            grid[x, y] = 0;
            //        else if (minX == 1 && minY == 0)
            //            grid[x, y] = 90;
            //        else if (minX == -1 && minY == 0)
            //            grid[x, y] = 270;
            //        else if (minX == 0 && minY == 1)
            //            grid[x, y] = 0;
            //        else if (minX == 0 && minY == -1)
            //            grid[x, y] = 180;
            //        else if (minX == 1 && minY == 1)
            //            grid[x, y] = 45;
            //        else if (minX == -1 && minY == 1)
            //            grid[x, y] = 315;
            //        else if (minX == 1 && minY == -1)
            //            grid[x, y] = 135;
            //        else if (minX == -1 && minY == -1)
            //            grid[x, y] = 225;
            //        else
            //            grid[x, y] = 1000f;
            //    }
            //}
        }

        void CalculateSection(int x, int y, IntegrationField iField, int gridSizeX, int gridSizeY)
        {
            int minD = int.MaxValue;
            int minX = 0, minY = 0;

            for (int xDelta = -1; xDelta < 2; ++xDelta)
            {
                for (int yDelta = -1; yDelta < 2; ++yDelta)
                {
                    int atX = x + xDelta;
                    int atY = y + yDelta;

                    if (xDelta == 0 && yDelta == 0)
                        continue;
                    else if (atX < 0 || atX >= gridSizeX || atY < 0 || atY >= gridSizeY)
                        continue;

                    int checkD = iField[atX, atY];
                    if (checkD <= minD)
                    {
                        minD = checkD;
                        minX = xDelta;
                        minY = yDelta;
                    }
                }
            }

            //Because Unity is stupid and circles start at up and rotate clockwise
            if (minD == ushort.MaxValue)
                grid[x][y] = 0;
            else if (minX == 1 && minY == 0)
                grid[x][y] = 90;
            else if (minX == -1 && minY == 0)
                grid[x][y] = 270;
            else if (minX == 0 && minY == 1)
                grid[x][y] = 0;
            else if (minX == 0 && minY == -1)
                grid[x][y] = 180;
            else if (minX == 1 && minY == 1)
                grid[x][y] = 45;
            else if (minX == -1 && minY == 1)
                grid[x][y] = 315;
            else if (minX == 1 && minY == -1)
                grid[x][y] = 135;
            else if (minX == -1 && minY == -1)
                grid[x][y] = 225;
            else
                grid[x][y] = 1000f;
        }
    }
    
}