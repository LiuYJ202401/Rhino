using System.Collections.Generic;
using Rhino.Geometry;

namespace Rh.Geo.Crv
{
    /// <summary>
    /// 多边形几何工具方法。
    /// 提供正多边形、星形多边形顶点生成的通用方法，供 Command 层复用。
    /// </summary>
    public static class PolygonGeo
    {
        /// <summary>
        /// 在指定平面上以中心和外接圆半径生成正多边形顶点。
        /// 无直接 RhinoCommon 构造，手动计算顶点。
        /// </summary>
        /// <param name="plane">所在平面</param>
        /// <param name="center">中心点</param>
        /// <param name="sides">边数（≥3）</param>
        /// <param name="radius">外接圆半径（>0）</param>
        /// <param name="startAngle">起始角度（弧度），默认 0</param>
        /// <returns>闭合多段线；参数无效时返回 null</returns>
        public static Polyline CreateRegularPolygon(Plane plane, Point3d center,
            int sides, double radius, double startAngle = 0.0)
        {
            if (sides < 3 || radius <= 0)
                return null;

            double cu, cv;
            PlaneGeo.PointToUV(plane, center, out cu, out cv);
            Point3d planeCenter = plane.PointAt(cu, cv);

            var poly = new Polyline(sides + 1);
            double angleStep = 2.0 * System.Math.PI / sides;

            for (int i = 0; i < sides; i++)
            {
                double angle = startAngle + i * angleStep;
                poly.Add(planeCenter
                    + plane.XAxis * (radius * System.Math.Cos(angle))
                    + plane.YAxis * (radius * System.Math.Sin(angle)));
            }
            poly.Add(poly[0]);

            return poly;
        }

        /// <summary>
        /// 在指定平面上以内外半径生成星形多边形顶点。
        /// 无直接 RhinoCommon 构造，手动交替内外半径。
        /// </summary>
        /// <param name="plane">所在平面</param>
        /// <param name="center">中心点</param>
        /// <param name="sides">角数（≥3）</param>
        /// <param name="outerRadius">外半径（>0）</param>
        /// <param name="innerRadius">内半径（>0）</param>
        /// <returns>闭合星形多段线；参数无效时返回 null</returns>
        public static Polyline CreateStarPolygon(Plane plane, Point3d center,
            int sides, double outerRadius, double innerRadius)
        {
            if (sides < 3 || outerRadius <= 0 || innerRadius <= 0)
                return null;

            double cu, cv;
            PlaneGeo.PointToUV(plane, center, out cu, out cv);
            Point3d planeCenter = plane.PointAt(cu, cv);

            int vertexCount = sides * 2;
            var poly = new Polyline(vertexCount + 1);
            double angleStep = System.Math.PI / sides;

            for (int i = 0; i < vertexCount; i++)
            {
                double angle = i * angleStep;
                double r = (i % 2 == 0) ? outerRadius : innerRadius;
                poly.Add(planeCenter
                    + plane.XAxis * (r * System.Math.Cos(angle))
                    + plane.YAxis * (r * System.Math.Sin(angle)));
            }
            poly.Add(poly[0]);

            return poly;
        }

        /// <summary>
        /// 由边长计算正多边形外接圆半径。
        /// </summary>
        /// <param name="edgeLength">边长</param>
        /// <param name="sides">边数</param>
        /// <returns>外接圆半径</returns>
        public static double EdgeLengthToRadius(double edgeLength, int sides)
        {
            return edgeLength / (2.0 * System.Math.Sin(System.Math.PI / sides));
        }

        /// <summary>
        /// 由一条边的两端点计算正多边形的中心、外接圆半径和起始角度。
        /// </summary>
        /// <param name="plane">所在平面</param>
        /// <param name="start">边起点</param>
        /// <param name="end">边终点</param>
        /// <param name="sides">边数</param>
        /// <param name="center">输出：中心点</param>
        /// <param name="radius">输出：外接圆半径</param>
        /// <param name="startAngle">输出：起始角度（弧度）</param>
        public static void EdgeToCenter(Plane plane, Point3d start, Point3d end, int sides,
            out Point3d center, out double radius, out double startAngle)
        {
            double edgeLength = start.DistanceTo(end);
            radius = EdgeLengthToRadius(edgeLength, sides);

            double su, sv, eu, ev;
            PlaneGeo.PointToUV(plane, start, out su, out sv);
            PlaneGeo.PointToUV(plane, end, out eu, out ev);

            double midU = (su + eu) / 2.0;
            double midV = (sv + ev) / 2.0;

            double apothem = radius * System.Math.Cos(System.Math.PI / sides);
            double edgeDu = eu - su;
            double edgeDv = ev - sv;
            double edgeLen = System.Math.Sqrt(edgeDu * edgeDu + edgeDv * edgeDv);
            double perpU = -edgeDv / edgeLen;
            double perpV = edgeDu / edgeLen;

            center = plane.PointAt(midU + perpU * apothem, midV + perpV * apothem);

            Vector3d toStart = start - center;
            startAngle = System.Math.Atan2(toStart * plane.YAxis, toStart * plane.XAxis);
        }
    }
}
