using System.Collections.Generic;
using Rhino.Geometry;

namespace Rh.Geo.Crv
{
    /// <summary>
    /// 相切圆角（Fillet）几何工具方法。
    /// 处理两条曲线之间的相切圆/圆弧创建，包括 CreateFillet 不支持的平行线退化情况。
    /// </summary>
    public static class FilletGeo
    {
        /// <summary>
        /// 在两条曲线之间创建相切圆角弧。
        /// 对相交线调用 Curve.CreateFillet；对平行线用半圆（r=d/2）连接。
        /// </summary>
        /// <param name="curve1">第一条曲线</param>
        /// <param name="curve2">第二条曲线</param>
        /// <param name="radius">期望半径（平行线场景下被忽略，强制使用 d/2）</param>
        /// <param name="tolerance">公差</param>
        /// <returns>相切圆角弧数组（通常 1 个；平行线场景可能 2 个）；无解返回空数组</returns>
        public static Arc[] CreateFilletArcs(Curve curve1, Curve curve2, double radius, double tolerance)
        {
            if (curve1 == null || curve2 == null || radius <= 0)
                return new Arc[0];

            // 先尝试 CreateFillet（处理相交线等常规情况）
            double t0 = curve1.Domain.Mid;
            double t1 = curve2.Domain.Mid;
            Arc filletArc = Curve.CreateFillet(curve1, curve2, radius, t0, t1);
            if (filletArc.IsValid)
                return new Arc[] { filletArc };

            // CreateFillet 失败：检查是否为平行线退化情况
            var parallelResult = CreateParallelFillet(curve1, curve2, tolerance);
            if (parallelResult != null && parallelResult.Count > 0)
                return parallelResult.ToArray();

            return new Arc[0];
        }

        /// <summary>
        /// 在两条曲线之间创建相切圆角圆。
        /// 对相交线调用 Curve.CreateFillet；对平行线用半圆（r=d/2）连接。
        /// </summary>
        public static Circle[] CreateFilletCircles(Curve curve1, Curve curve2, double radius, double tolerance)
        {
            var arcs = CreateFilletArcs(curve1, curve2, radius, tolerance);
            if (arcs.Length == 0)
                return new Circle[0];

            var circles = new List<Circle>(arcs.Length);
            foreach (var arc in arcs)
                circles.Add(new Circle(arc.Plane, arc.Center, arc.Radius));
            return circles.ToArray();
        }

        /// <summary>
        /// 平行线半圆相切处理。
        /// 平行线相切圆的半径被严格锁定为 r = d/2（d 为两线间距）。
        /// 圆心在两线之间的中点，沿线方向可能有两个对称位置（取决于线段范围）。
        /// </summary>
        private static List<Arc> CreateParallelFillet(Curve curve1, Curve curve2, double tolerance)
        {
            var result = new List<Arc>();

            // 仅处理 LineCurve（最常见且可精确分析）
            var line1 = curve1 as LineCurve;
            var line2 = curve2 as LineCurve;
            if (line1 == null || line2 == null)
                return result;

            var dir1 = line1.Line.Direction;
            var dir2 = line2.Line.Direction;
            if (!dir1.Unitize() || !dir2.Unitize())
                return result;

            // 检查平行（方向相同或相反）
            bool sameDir = dir1.IsParallelTo(dir2, 0.001) == 1;
            bool oppositeDir = dir1.IsParallelTo(dir2, 0.001) == -1;
            if (!sameDir && !oppositeDir)
                return result; // 非平行线，CreateFillet 失败说明真的无解

            // 计算两线间距 d
            var p1 = line1.Line.PointAt(0.5);  // 线1中点
            var p2 = line2.Line.PointAt(0.5);  // 线2中点
            Vector3d offset = p2 - p1;
            // 分解 offset 为沿线方向分量和垂直分量
            double along = offset * dir1;
            Vector3d perpVec = offset - dir1 * along;
            double d = perpVec.Length;
            if (d < tolerance)
                return result; // 重合，无意义

            double r = d / 2.0;  // 平行线相切圆半径被锁定为 d/2

            // 法向量（从线1指向线2的单位向量）
            Vector3d normal = perpVec;
            normal.Unitize();

            // 确定圆弧所在平面
            Vector3d planeNormal = Vector3d.CrossProduct(dir1, normal);
            if (planeNormal.Length < tolerance)
                return result;
            planeNormal.Unitize();

            // 找两线段在沿线方向的重叠区间（半圆的连接点必须同时落在两条线段上）
            // 将两条线段的端点投影到线1的参数空间
            double l1Start = 0.0;
            double l1End = line1.Line.Length;
            // 线2端点投影到线1方向
            var q2a = line2.Line.PointAt(0);
            var q2b = line2.Line.PointAt(1);
            double projA = (q2a - line1.Line.PointAt(0)) * dir1;
            double projB = (q2b - line1.Line.PointAt(0)) * dir1;
            double l2StartIn1 = System.Math.Min(projA, projB);
            double l2EndIn1 = System.Math.Max(projA, projB);

            // 重叠区间
            double overlapStart = System.Math.Max(l1Start, l2StartIn1);
            double overlapEnd = System.Math.Min(l1End, l2EndIn1);
            if (overlapEnd <= overlapStart)
                return result; // 两线段在沿线方向无重叠

            // 在重叠区间内取中点作为半圆连接点
            double midParam = (overlapStart + overlapEnd) / 2.0;
            Point3d c1 = line1.Line.PointAt(0) + dir1 * midParam;        // 线1上的连接点
            Point3d c2 = c1 + normal * d;                                  // 线2上的对应点

            // 圆心在 c1 和 c2 的中点
            Point3d center = (c1 + c2) / 2.0;
            var plane = new Plane(center, c1, c2 + planeNormal * r);

            // 半圆弧：从 c1 到 c2，扫过 180°
            var arc = new Arc(plane, center, r, System.Math.PI);
            result.Add(arc);

            return result;
        }
    }
}
