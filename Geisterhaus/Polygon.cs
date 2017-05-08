namespace Geisterhaus
{
    class Polygon
    {
        public int[] ypoints;
        public int[] xpoints;
        public int npoints;

        /// <summary>
        /// initializes new Polygon with just one point 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Polygon(int x, int y)
        {
            ypoints = new int[1];
            ypoints[0] = y;
            xpoints = new int[1];
            xpoints[0] = x;
            npoints = 1;
        }

        /// <summary>
        /// adds a point to the Polygon
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void addPoint(int x, int y)
        {
            int[] _xpoints = xpoints;
            int[] _ypoints = ypoints;
            npoints++;
            xpoints = new int[npoints];
            ypoints = new int[npoints];
            for (int i = 0; i < _xpoints.Length; i++)
            {
                xpoints[i] = _xpoints[i];
                ypoints[i] = _ypoints[i];
            }
            xpoints[npoints - 1] = x;
            ypoints[npoints - 1] = y;
        }

        public void removePoint(int index)
        {
            npoints--;
            int[] _xpoints = xpoints;
            int[] _ypoints = ypoints;
            xpoints = new int[npoints];
            ypoints = new int[npoints];
            for (int i = 0; i < index; i++)
            {
                xpoints[i] = _xpoints[i];
                ypoints[i] = _ypoints[i];
            }
            for (int i = index; i < npoints; i++)
            {
                xpoints[i] = _xpoints[i + 1];
                ypoints[i] = _ypoints[i + 1];
            }
        }
    }
}