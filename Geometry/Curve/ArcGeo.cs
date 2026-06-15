using Rhino.Geometry;

namespace Rh.Geo.Crv
{
    /// <summary>
    /// 圆弧几何工具方法。
    /// 所有方法接收 Plane 参数或由输入自动确定平面，不假设平面方向。
    /// </summary>
    public static class ArcGeo
    {
        /// <summary>
        /// 由平面、圆心、半径和起始/终止角度创建圆弧。
        /// 旋转平面使弧从 startAngle 开始。
        /// </summary>
        public static Arc CreateFromCenterAngle(Plane plane, Point3d center,
            double radius, double startAngle, double endAngle)
        {
            Plane rotatedPlane = plane;
            rotatedPlane.Rotate(startAngle, plane.Normal);

            double sweepAngle = endAngle - startAngle;
            return new Arc(rotatedPlane, center, radius, sweepAngle);
        }

        /// <summary>
        /// 由三点创建圆弧。
        /// RhinoCommon：new Arc(start, pointOnArc, end)，平面由三点自动确定。
        /// </summary>
        public static Arc CreateFrom3Points(Point3d start, Point3d pointOnArc, Point3d end)
        {
            return new Arc(start, pointOnArc, end);
        }

        /// <summary>
        /// 由起点、终点和起点方向创建圆弧。
        /// RhinoCommon：new Arc(start, directionAtStart, end)，平面由方向和两点确定。
        /// </summary>
        public static Arc CreateFromStartEndDir(Point3d start, Point3d end, Vector3d directionAtStart)
        {
            return new Arc(start, directionAtStart, end);
        }
    }
}
