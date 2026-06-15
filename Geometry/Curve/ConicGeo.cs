using Rhino.Geometry;

namespace Rh.Geo.Crv
{
    /// <summary>
    /// 圆锥曲线（Conic）几何工具方法。
    /// 提供有理二次 NURBS 手工构造，用于 RhinoCommon 无直接 API 的圆锥曲线。
    /// </summary>
    public static class ConicGeo
    {
        /// <summary>
        /// 由起点、终点、切线交点和 rho 值创建圆锥截面曲线。
        /// 手工构造 degree=2 有理 NURBS（3 控制点 + 钳端节点）。
        ///
        /// rho 与中控制点权重 w 的关系：w = rho / (1 - rho)
        ///   rho < 0.5 → 椭圆
        ///   rho = 0.5 → 抛物线
        ///   rho > 0.5 → 双曲线
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="apex">切线交点（两端点切线的交汇）</param>
        /// <param name="rho">rho 值（0 < rho < 1）</param>
        /// <returns>NURBS 曲线；rho 无效时返回 null</returns>
        public static NurbsCurve CreateConic(Point3d start, Point3d end, Point3d apex, double rho)
        {
            if (rho <= 0.0 || rho >= 1.0)
                return null;

            double w = rho / (1.0 - rho);

            var nc = new NurbsCurve(3, true, 3, 3);
            nc.Points.SetPoint(0, start.X, start.Y, start.Z, 1.0);
            nc.Points.SetPoint(1, apex.X, apex.Y, apex.Z, w);
            nc.Points.SetPoint(2, end.X, end.Y, end.Z, 1.0);
            nc.Knots[0] = 0.0;
            nc.Knots[1] = 0.0;
            nc.Knots[2] = 0.0;
            nc.Knots[3] = 1.0;
            nc.Knots[4] = 1.0;
            nc.Knots[5] = 1.0;

            return nc;
        }

        /// <summary>
        /// 由焦点和顶点创建双曲线（rho > 0.5 的圆锥曲线）。
        /// 手工计算 apex 并调用 CreateConic。
        /// </summary>
        /// <param name="focus">焦点</param>
        /// <param name="vertex">顶点</param>
        /// <param name="endPoint">终点</param>
        /// <returns>NURBS 曲线；参数无效时返回 null</returns>
        public static NurbsCurve CreateHyperbola(Point3d focus, Point3d vertex, Point3d endPoint)
        {
            if (focus == vertex)
                return null;

            // 双曲线对应的 rho > 0.5
            double rho = 0.7;
            double w = rho / (1.0 - rho);

            // apex = 两端点切线交点的近似
            Vector3d axis = focus - vertex;
            axis.Unitize();
            Point3d mid = (vertex + endPoint) / 2.0;
            Point3d apex = mid + axis * (mid - vertex).Length;

            return CreateConic(vertex, endPoint, apex, rho);
        }
    }
}
