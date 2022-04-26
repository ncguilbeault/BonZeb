using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace BonZeb
{
    public class Hungarian
    {
        public static double distance(Point2f A, Point2f B)
        {
            return Math.Sqrt(Math.Pow(A.X - B.X, 2) + Math.Pow(A.Y - B.Y, 2));
        }

        public static double[,] cDist(Point2f[] XA, Point2f[] XB)
        {

            int mA = XA.Length, mB = XB.Length;

            double[,] result = new double[mA, mB];

            for (int i = 0; i < mA; i++)
            {
                for (int j = 0; j < mB; j++)
                {
                    result[i, j] = distance(XA[i], XB[j]);
                }
            }
            return result;
        }

        public static double[,] cDist(Point2f[] XA)
        {
            Point2f[] XB = new Point2f[XA.Length];
            for (int i = 0; i < XA.Length; i++)
            {
                XB[i] = new Point2f(0, 0);
            }
            return cDist(XA, XB);
        }

        public static List<int[]> LinearSumAssignment(double[,] cost)
        {

            var nr = cost.GetLength(0);
            var nc = cost.GetLength(1);

            if (nr >= nc)
            {
                var tmpCost = new double[nc, nr];
                for (var i = 0; i < nc; i++)
                    for (var j = 0; j < nr; j++)
                        tmpCost[i, j] = cost[j, i];
                cost = tmpCost;
                nr = cost.GetLength(0);
                nc = cost.GetLength(1);
            }

            var u = new double[nr];
            var v = new double[nc];
            var shortestPathCosts = new double[nc];
            var path = new int[nc];
            var x = new int[nr];
            var y = new int[nc];
            var sr = new bool[nr];
            var sc = new bool[nc];

            for (int i = 0; i < nc; i++)
            {
                path[i] = -1;
                y[i] = -1;
            }

            for (int i = 0; i < nr; i++)
            {
                x[i] = -1;
            }

            for (var curRow = 0; curRow < nr; curRow++)
            {
                double minVal = 0;
                var i = curRow;
                var remaining = new int[nc].ToList();
                var numRemaining = nc;
                for (var jp = 0; jp < nc; jp++)
                {
                    remaining[jp] = jp;
                    shortestPathCosts[jp] = double.PositiveInfinity;
                }
                Array.Clear(sr, 0, sr.Length);
                Array.Clear(sc, 0, sc.Length);

                var sink = -1;
                while (sink == -1)
                {
                    sr[i] = true;
                    var indexLowest = -1;
                    var lowest = double.PositiveInfinity;
                    for (var jk = 0; jk < numRemaining; jk++)
                    {
                        var jl = remaining[jk];
                        var r = minVal + cost[i, jl] - u[i] - v[jl];

                        if (r < shortestPathCosts[jl])
                        {
                            path[jl] = i;
                            shortestPathCosts[jl] = r;
                        }

                        if (shortestPathCosts[jl] < lowest || shortestPathCosts[jl] == lowest && y[jl] == -1)
                        {
                            lowest = shortestPathCosts[jl];
                            indexLowest = jk;
                        }
                    }
                    minVal = lowest;
                    var jp = remaining[indexLowest];
                    if (double.IsPositiveInfinity(minVal))
                        throw new InvalidOperationException("No feasible solution.");
                    if (y[jp] == -1)
                        sink = jp;
                    else
                        i = y[jp];

                    sc[jp] = true;
                    remaining[indexLowest] = remaining[--numRemaining];
                    remaining.RemoveAt(numRemaining);
                }
                if (sink < 0)
                    throw new InvalidOperationException("No feasible solution.");

                u[curRow] += minVal;
                for (var ip = 0; ip < nr; ip++)
                    if (sr[ip] && ip != curRow)
                        u[ip] += minVal - shortestPathCosts[x[ip]];

                for (var jp = 0; jp < nc; jp++)
                    if (sc[jp])
                        v[jp] -= minVal - shortestPathCosts[jp];

                var j = sink;
                while (true)
                {
                    var ip = path[j];
                    y[j] = ip;
                    (j, x[ip]) = (x[ip], j);
                    if (ip == curRow)
                        break;
                }
            }
            var ret = new List<int[]>();
            Array.Sort(y);
            ret.Add(y);
            ret.Add(x);
            return ret;
        }
    }
}
