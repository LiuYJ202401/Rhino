using Rhino.Geometry;

namespace Rh.Geo
{
    /// <summary>
    /// 平面几何工具方法。
    /// 提供 Plane 相关的通用计算，供 Geometry 层和 Command 层复用。
    /// </summary>
    public static class PlaneGeo
    {
        /// <summary>
        /// 将世界坐标点转换为平面 UV 参数坐标。
        /// RhinoCommon 的 Plane 结构体无此方法，手动计算投影。
        /// </summary>
        /// <param name="plane">参考平面</param>
        /// <param name="point">世界坐标点</param>
        /// <param name="u">输出 U 参数</param>
        /// <param name="v">输出 V 参数</param>
        public static void PointToUV(Plane plane, Point3d point, out double u, out double v)
        {
            Vector3d toPt = point - plane.Origin;
            u = toPt * plane.XAxis;
            v = toPt * plane.YAxis;
        }

        /// <summary>
        /// 将世界坐标点转换为平面 UV 参数坐标（元组返回）。
        /// </summary>
        public static (double u, double v) PointToUV(Plane plane, Point3d point)
        {
            Vector3d toPt = point - plane.Origin;
            return (toPt * plane.XAxis, toPt * plane.YAxis);
        }
    }
}
