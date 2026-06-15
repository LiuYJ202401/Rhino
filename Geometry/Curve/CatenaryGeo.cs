using System.Collections.Generic;
using Rhino.Geometry;

namespace Rh.Geo.Crv
{
    /// <summary>
    /// 悬链线几何工具方法。
    /// 接收重力方向向量，不假设世界 Z 轴。
    /// </summary>
    public static class CatenaryGeo
    {
        /// <summary>
        /// 创建悬链线曲线。
        /// y = a * cosh(x/a)，牛顿迭代求解参数 a。
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="length">曲线总长度（>两点距离）</param>
        /// <param name="gravity">重力方向（单位向量，指向"下"）</param>
        /// <returns>NURBS 曲线；参数无效时返回 null</returns>
        public static NurbsCurve Create(Point3d start, Point3d end, double length, Vector3d gravity)
        {
            if (start == end || length < start.DistanceTo(end))
                return null;

            // 归一化重力方向
            Vector3d gravityUnit = gravity;
            gravityUnit.Unitize();

            // 分离水平分量和垂直分量
            Vector3d lineVec = end - start;
            double pointDist = lineVec.Length;
            double halfDist = pointDist / 2.0;

            // 水平方向：lineVec 在垂直于重力的平面上的投影
            Vector3d verticalComp = gravityUnit * (lineVec * gravityUnit);
            Vector3d horizontalVec = lineVec - verticalComp;
            double horizontalDist = horizontalVec.Length;

            // 牛顿迭代求解悬链线参数 a
            double a = halfDist;

            for (int iter = 0; iter < 50; iter++)
            {
                double sinhVal = System.Math.Sinh(halfDist / a);
                double f = 2.0 * a * sinhVal - length;
                double df = 2.0 * sinhVal - 2.0 * halfDist / a * System.Math.Cosh(halfDist / a);

                if (System.Math.Abs(df) < 1e-15)
                    break;

                a = a - f / df;
                if (a <= 0)
                    a = halfDist * 0.5;
            }

            // 采样点：沿 start→end 线性插值，再沿重力方向添加下垂量
            int sampleCount = 50;
            var samplePoints = new List<Point3d>(sampleCount + 1);

            for (int i = 0; i <= sampleCount; i++)
            {
                double t = (double)i / sampleCount;
                double x = -halfDist + t * pointDist;
                double sag = a * (System.Math.Cosh(x / a) - 1.0);

                Point3d basePt = start + lineVec * t;
                // 沿重力方向下垂
                basePt += gravityUnit * sag;
                samplePoints.Add(basePt);
            }

            return Rhino.Geometry.Curve.CreateInterpolatedCurve(samplePoints, 3, CurveKnotStyle.Uniform).ToNurbsCurve();
        }
    }
}
