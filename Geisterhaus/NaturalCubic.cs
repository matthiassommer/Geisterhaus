using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Geisterhaus
{
    class NaturalCubic
    {
        private const int STEPS = 12;

        /// <summary>
        /// calculates the natural cubic spline that interpolates y[0], y[1], ...
        /// y[n] The first segment is returned as C[0].a + C[0].b*u + C[0].c*u^2 +
        /// C[0].d*u^3 0<=u <1 the other segments are in C[1], C[2], ... C[n-1]
        /// </summary>
        /// <param name="n"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public Cubic[] calcNaturalCubic(int n, int[] x)
        {
            float[] gamma = new float[n + 1];
            float[] delta = new float[n + 1];
            float[] D = new float[n + 1];
            int i;
            /*
             * We solve the equation [2 1 ] [D[0]] [3(x[1] - x[0]) ] |1 4 1 | |D[1]|
             * |3(x[2] - x[0]) | | 1 4 1 | | . | = | . | | ..... | | . | | . | | 1 4
             * 1| | . | |3(x[n] - x[n-2])| [ 1 2] [D[n]] [3(x[n] - x[n-1])]
             * 
             * by using row operations to convert the matrix to upper triangular and
             * then back sustitution. The D[i] are the derivatives at the knots.
            */
            gamma[0] = 1.0f / 2.0f;
            for (i = 1; i < n; i++)
            {
                gamma[i] = 1 / (4 - gamma[i - 1]);
            }
            gamma[n] = 1 / (2 - gamma[n - 1]);

            delta[0] = 3 * (x[1] - x[0]) * gamma[0];
            for (i = 1; i < n; i++)
            {
                delta[i] = (3 * (x[i + 1] - x[i - 1]) - delta[i - 1]) * gamma[i];
            }
            delta[n] = (3 * (x[n] - x[n - 1]) - delta[n - 1]) * gamma[n];

            D[n] = delta[n];
            for (i = n - 1; i >= 0; i--)
            {
                D[i] = delta[i] - gamma[i] * D[i + 1];
            }

            /* now compute the coefficients of the cubics */
            Cubic[] C = new Cubic[n];
            for (i = 0; i < n; i++)
            {
                C[i] = new Cubic((float)x[i], D[i], 3 * (x[i + 1] - x[i]) - 2
                        * D[i] - D[i + 1], 2 * (x[i] - x[i + 1]) + D[i] + D[i + 1]);
            }
            return C;
        }

        private Polygon Vector2ToPolygon(Vector2[] input)
        {
            Polygon poly = new Polygon((int)input[0].X, (int)input[0].Y);
            for (int i = 1; i < input.Length; i++)
            {
                poly.addPoint((int)input[i].X, (int)input[i].Y);
            }
            return poly;
        }

        private Vector2[] PolygonToVector2(Polygon input)
        {
            Vector2[] vec = new Vector2[input.npoints];
            for (int i = 0; i < input.npoints; i++)
            {
                vec[i].X = (float)input.xpoints[i];
                vec[i].Y = (float)input.ypoints[i];
            }
            return vec;
        }

        private List<Vector2> PolygonToVectorList(Polygon input)
        {
            List<Vector2> list = new List<Vector2>();
            for (int i = 0; i < input.npoints; i++)
            {
                list.Add(new Vector2((float)input.xpoints[i], (float)input.ypoints[i]));
            }
            return list;
        }

        public List<Vector2> getPositionPathList(Vector2[] controlPoints)
        {
            Polygon pts = Vector2ToPolygon(controlPoints);
            Cubic[] X = calcNaturalCubic(pts.npoints - 1, pts.xpoints);
            Cubic[] Y = calcNaturalCubic(pts.npoints - 1, pts.ypoints);

            /*
             * very crude technique - just break each segment up into steps
             * lines
             */
            Polygon p = new Polygon((int)Math.Round(X[0].eval(0)), (int)Math.Round(Y[0].eval(0)));
            for (int i = 0; i < X.Length; i++)
            {
                for (int j = 1; j <= STEPS; j++)
                {
                    float u = j / (float)STEPS;
                    p.addPoint((int)Math.Round(X[i].eval(u)), (int)Math.Round(Y[i].eval(u)));
                }
            }
            return PolygonToVectorList(p);
        }
        /*
        public Vector2[] getPositionPath(Vector2[] controlPoints)
        {
            Polygon pts = Vector2ToPolygon(controlPoints);
            Cubic[] X = calcNaturalCubic(pts.npoints - 1, pts.xpoints);
            Cubic[] Y = calcNaturalCubic(pts.npoints - 1, pts.ypoints);

            Polygon p = new Polygon((int)Math.Round(X[0].eval(0)), (int)Math.Round(Y[0].eval(0)));
            for (int i = 0; i < X.Length; i++)
            {
                for (int j = 1; j <= STEPS; j++)
                {
                    float u = j / (float)STEPS;
                    p.addPoint((int)Math.Round(X[i].eval(u)), (int)Math.Round(Y[i].eval(u)));
                }
            }
            return PolygonToVector2(p);
        }*/

        private Polygon ListToPolygon(List<Vector2> input)
        {
            Polygon poly = new Polygon((int)input[0].X, (int)input[0].Y);
            for (int i = 1; i < input.Count; i++)
            {
                poly.addPoint((int)input[i].X, (int)input[i].Y);
            }
            return poly;
        }

        public List<Vector2> getPositionPath(List<Vector2> controlPoints)
        {
            Polygon pts = ListToPolygon(controlPoints);
            Cubic[] X = calcNaturalCubic(pts.npoints - 1, pts.xpoints);
            Cubic[] Y = calcNaturalCubic(pts.npoints - 1, pts.ypoints);

            /*
             * very crude technique - just break each segment up into steps
             * lines
             */
            Polygon p = new Polygon((int)Math.Round(X[0].eval(0)), (int)Math.Round(Y[0].eval(0)));
            for (int i = 0; i < X.Length; i++)
            {
                for (int j = 1; j <= STEPS; j++)
                {
                    float u = j / (float)STEPS;
                    p.addPoint((int)Math.Round(X[i].eval(u)), (int)Math.Round(Y[i].eval(u)));
                }
            }
            return PolygonToVectorList(p);
        }

    }
}
