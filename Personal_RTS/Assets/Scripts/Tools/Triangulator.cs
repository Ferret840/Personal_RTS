using UnityEngine;
using System.Collections.Generic;

namespace Tools
{

    //Credit: http://wiki.unity3d.com/index.php?title=Triangulator
    //Author: runevision

    public class Triangulator
    {
        private List<Vector2> m_Points = new List<Vector2>();

        public Triangulator(Vector2[] _points)
        {
            m_Points = new List<Vector2>(_points);
        }

        public Triangulator(List<Vector2> _points)
        {
            m_Points = _points;
        }

        public int[] Triangulate()
        {
            List<int> indices = new List<int>();

            int n = m_Points.Count;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area() > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private float Area()
        {
            int n = m_Points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = m_Points[p];
                Vector2 qval = m_Points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }

        private bool Snip(int _u, int _v, int _w, int _n, int[] _vArray)
        {
            int p;
            Vector2 A = m_Points[_vArray[_u]];
            Vector2 B = m_Points[_vArray[_v]];
            Vector2 C = m_Points[_vArray[_w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < _n; p++)
            {
                if ((p == _u) || (p == _v) || (p == _w))
                    continue;
                Vector2 P = m_Points[_vArray[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private bool InsideTriangle(Vector2 _a, Vector2 _b, Vector2 _c, Vector2 _p)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = _c.x - _b.x;
            ay = _c.y - _b.y;
            bx = _a.x - _c.x;
            by = _a.y - _c.y;
            cx = _b.x - _a.x;
            cy = _b.y - _a.y;
            apx = _p.x - _a.x;
            apy = _p.y - _a.y;
            bpx = _p.x - _b.x;
            bpy = _p.y - _b.y;
            cpx = _p.x - _c.x;
            cpy = _p.y - _c.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }

}