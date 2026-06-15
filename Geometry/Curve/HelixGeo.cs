using Rhino.Geometry;

namespace Rh.Geo.Crv
{
    /// <summary>
    /// 螺旋线几何工具方法。
    /// 接收轴方向，不假设世界坐标轴。
    /// </summary>
    public static class HelixGeo
    {
        /// <summary>
        /// 计算垂直于轴方向的参考半径点。
        /// 选择与轴最不对齐的世界轴，取叉积作为垂直方向。
        /// </summary>
        public static Point3d GetRadiusPoint(Point3d axisStart, Vector3d axisDir, double radius)
        {
            Vector3d unitDir = axisDir;
            unitDir.Unitize();

            Vector3d perp = System.Math.Abs(unitDir * Vector3d.ZAxis) > 0.9
                ? Vector3d.CrossProduct(unitDir, Vector3d.XAxis)
                : Vector3d.CrossProduct(unitDir, Vector3d.ZAxis);
            perp.Unitize();

            return axisStart + perp * radius;
        }
    }
}
