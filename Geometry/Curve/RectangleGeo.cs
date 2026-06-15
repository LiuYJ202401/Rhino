using Rhino.Geometry;

namespace Rh.Geo.Crv
{
    /// <summary>
    /// 矩形几何工具方法。
    /// 所有方法接收 Plane 参数，不假设平面方向。
    /// </summary>
    public static class RectangleGeo
    {
        /// <summary>
        /// 由平面和对角两点创建矩形。
        /// 将世界坐标点投影到平面 UV 空间计算角点。
        /// </summary>
        public static Polyline CreateFromCorners(Plane plane, Point3d corner1, Point3d corner2)
        {
            double u1, v1, u2, v2;
            PlaneGeo.PointToUV(plane, corner1, out u1, out v1);
            PlaneGeo.PointToUV(plane, corner2, out u2, out v2);

            if (System.Math.Abs(u2 - u1) < 1e-12 || System.Math.Abs(v2 - v1) < 1e-12)
                return null;

            double uMin = System.Math.Min(u1, u2);
            double uMax = System.Math.Max(u1, u2);
            double vMin = System.Math.Min(v1, v2);
            double vMax = System.Math.Max(v1, v2);

            var poly = new Polyline(5);
            poly.Add(plane.PointAt(uMin, vMin));
            poly.Add(plane.PointAt(uMax, vMin));
            poly.Add(plane.PointAt(uMax, vMax));
            poly.Add(plane.PointAt(uMin, vMax));
            poly.Add(plane.PointAt(uMin, vMin));

            return poly;
        }

        /// <summary>
        /// 由平面、中心和宽高创建矩形。
        /// 将中心投影到平面 UV 空间后计算四角。
        /// </summary>
        public static Polyline CreateFromCenter(Plane plane, Point3d center, double width, double height)
        {
            double halfW = width / 2.0;
            double halfH = height / 2.0;

            double cu, cv;
            PlaneGeo.PointToUV(plane, center, out cu, out cv);

            var poly = new Polyline(5);
            poly.Add(plane.PointAt(cu - halfW, cv - halfH));
            poly.Add(plane.PointAt(cu + halfW, cv - halfH));
            poly.Add(plane.PointAt(cu + halfW, cv + halfH));
            poly.Add(plane.PointAt(cu - halfW, cv + halfH));
            poly.Add(plane.PointAt(cu - halfW, cv - halfH));

            return poly;
        }
    }
}
