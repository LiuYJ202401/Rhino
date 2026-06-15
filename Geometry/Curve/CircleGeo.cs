using Rhino.Geometry;

namespace Rh.Geo.Crv
{
    /// <summary>
    /// 圆几何工具方法。
    /// 所有方法接收 Plane 参数，不假设平面方向。
    /// </summary>
    public static class CircleGeo
    {
        /// <summary>
        /// 由平面、圆心和半径创建圆。
        /// RhinoCommon：new Circle(plane, center, radius)
        /// </summary>
        public static Circle CreateFromCenterRadius(Plane plane, Point3d center, double radius)
        {
            return new Circle(plane, center, radius);
        }

        /// <summary>
        /// 由直径两端点创建圆。
        /// 无直接构造，计算中点和半距离后构造。
        /// </summary>
        public static Circle CreateFromDiameter(Plane plane, Point3d point1, Point3d point2)
        {
            Point3d center = (point1 + point2) / 2.0;
            double radius = point1.DistanceTo(point2) / 2.0;
            return new Circle(plane, center, radius);
        }

        /// <summary>
        /// 由三点创建圆。
        /// RhinoCommon：new Circle(p1, p2, p3)，平面由三点自动确定。
        /// </summary>
        public static Circle CreateFrom3Points(Point3d p1, Point3d p2, Point3d p3)
        {
            return new Circle(p1, p2, p3);
        }

        /// <summary>
        /// 由起点、起点切向和终点创建圆。
        /// RhinoCommon：new Circle(startPoint, tangentAtStart, endPoint)，平面由切向和两点确定。
        /// </summary>
        public static Circle CreateFromTangent(Point3d startPoint, Vector3d tangentAtStart, Point3d endPoint)
        {
            return new Circle(startPoint, tangentAtStart, endPoint);
        }
    }
}
