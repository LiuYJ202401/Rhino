using Rhino.Geometry;

namespace Rh.Geo.Crv
{
    /// <summary>
    /// 椭圆几何工具方法。
    /// 所有方法接收 Plane 参数，不假设平面方向。
    /// </summary>
    public static class EllipseGeo
    {
        /// <summary>
        /// 由平面、圆心和两半轴创建椭圆。
        /// 在圆心处重建平面（继承方向），再用 RhinoCommon 构造。
        /// </summary>
        public static NurbsCurve CreateFromCenterRadii(Plane plane, Point3d center,
            double radius1, double radius2)
        {
            var ellipsePlane = new Plane(center, plane.XAxis, plane.YAxis);
            var ellipse = new Ellipse(ellipsePlane, radius1, radius2);
            return ellipse.ToNurbsCurve();
        }

        /// <summary>
        /// 由直径两端点和第二轴半径创建椭圆。
        /// 从直径方向重建平面，法线来自传入平面的法线。
        /// </summary>
        public static NurbsCurve CreateFromDiameter(Plane plane,
            Point3d point1, Point3d point2, double secondRadius)
        {
            Point3d center = (point1 + point2) / 2.0;
            Vector3d xAxis = point2 - point1;
            double firstRadius = xAxis.Length / 2.0;
            xAxis.Unitize();

            Vector3d yAxis = Vector3d.CrossProduct(plane.Normal, xAxis);
            yAxis.Unitize();

            var ellipsePlane = new Plane(center, xAxis, yAxis);
            var ellipse = new Ellipse(ellipsePlane, firstRadius, secondRadius);
            return ellipse.ToNurbsCurve();
        }

        /// <summary>
        /// 由两焦点和椭圆上一点创建椭圆。
        /// 三点自动确定平面，无需传入法线。
        /// </summary>
        public static NurbsCurve CreateFromFoci(Point3d focus1, Point3d focus2, Point3d pointOnEllipse)
        {
            double distSum = focus1.DistanceTo(pointOnEllipse) + focus2.DistanceTo(pointOnEllipse);
            double a = distSum / 2.0;
            double c = focus1.DistanceTo(focus2) / 2.0;

            double b = System.Math.Sqrt(a * a - c * c);

            Point3d center = (focus1 + focus2) / 2.0;
            Vector3d xAxis = focus2 - focus1;
            xAxis.Unitize();

            // 用与 xAxis 不平行的参考向量计算法线
            Vector3d refVec = System.Math.Abs(xAxis * Vector3d.ZAxis) > 0.9
                ? Vector3d.XAxis : Vector3d.ZAxis;
            Vector3d normal = Vector3d.CrossProduct(xAxis, refVec);
            normal.Unitize();
            Vector3d yAxis = Vector3d.CrossProduct(normal, xAxis);
            yAxis.Unitize();

            var ellipsePlane = new Plane(center, xAxis, yAxis);
            var ellipse = new Ellipse(ellipsePlane, a, b);
            return ellipse.ToNurbsCurve();
        }
    }
}
