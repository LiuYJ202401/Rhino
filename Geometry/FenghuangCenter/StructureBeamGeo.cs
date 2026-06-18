using System;
using System.Collections.Generic;
using Rhino.Geometry;
using Rh.Geo.Srf;
using Rh.Geo.Sld;

namespace Rh.Geo.FenghuangCenter
{
    /// <summary>
    /// 支撑梁生成工具。
    /// 自定义算法：沿法线偏移曲线 + 矩形截面定向。
    /// 放样委托 SurfaceGeo.CreateLoft，合并委托 SolidGeo.CreateSolidFromBreps。
    /// </summary>
    public static class StructureBeamGeo
    {
        /// <summary>
        /// 沿对角曲线生成支撑梁实体。
        /// </summary>
        /// <param name="curves">对角曲线列表（LinesA 或 LinesB）</param>
        /// <param name="division">网格数据（提供法线参考）</param>
        /// <param name="offset">沿法线偏移距离</param>
        /// <param name="width">矩形截面宽度</param>
        /// <param name="depth">矩形截面深度</param>
        /// <param name="closedLoop">是否闭合环路</param>
        /// <param name="tolerance">公差</param>
        /// <returns>支撑梁实体列表；单根失败时跳过</returns>
        public static List<Brep> Build(List<Curve> curves, DivisionResult division,
            double offset, double width, double depth, bool closedLoop, double tolerance)
        {
            var breps = new List<Brep>();

            for (int i = 0; i < curves.Count; i++)
            {
                try
                {
                    Brep structureBrep = BuildSingle(curves[i], division, offset, width, depth, tolerance, closedLoop);
                    if (structureBrep != null)
                        breps.Add(structureBrep);
                }
                catch
                {
                    // 单根梁失败时跳过，继续处理下一根
                }
            }

            return breps;
        }

        // ================================================================
        // 单根支撑梁生成
        // ================================================================

        /// <summary>
        /// 生成单根支撑梁：偏移曲线 → 采样点 → 构建定向截面 → 放样。
        /// </summary>
        private static Brep BuildSingle(Curve curve, DivisionResult division,
            double offset, double width, double depth, double tolerance, bool closedLoop)
        {
            if (curve == null || !curve.IsValid || curve.GetLength() < tolerance)
                return null;

            // 沿法线偏移曲线
            Curve offsetCurve = OffsetAlongNormal(curve, division, offset, tolerance, closedLoop);
            if (offsetCurve == null)
                offsetCurve = curve;

            // 采样点与切线
            int sampleCount = division.CurveCount;
            int numSamples = closedLoop ? sampleCount : sampleCount + 1;

            var samplePoints = new List<Point3d>();
            var sampleTangents = new List<Vector3d>();

            for (int i = 0; i < numSamples; i++)
            {
                double t = offsetCurve.Domain.T0 + (double)i / sampleCount * offsetCurve.Domain.Length;
                Point3d curvePoint = offsetCurve.PointAt(t);
                if (!curvePoint.IsValid)
                    continue;

                Vector3d tangent = offsetCurve.TangentAt(t);
                if (!tangent.IsValid || tangent.IsZero)
                    continue;
                tangent.Unitize();

                samplePoints.Add(curvePoint);
                sampleTangents.Add(tangent);
            }

            if (samplePoints.Count < 2)
                return null;

            // 构建定向截面（refUp 追踪保持截面连续性）
            var sections = BuildOrientedSections(samplePoints, sampleTangents, division, width, depth);

            if (sections.Count < 2)
                return null;

            // 放样（委托框架 SurfaceGeo.CreateLoft）
            Brep[] loftResults = SurfaceGeo.CreateLoft(sections, Point3d.Unset, Point3d.Unset, LoftType.Normal, closedLoop);
            if (loftResults == null || loftResults.Length == 0)
                return null;

            // 合并（委托框架 SolidGeo.CreateSolidFromBreps）
            Brep joined = SolidGeo.CreateSolidFromBreps(loftResults, tolerance);
            return joined ?? loftResults[0];
        }

        // ================================================================
        // 截面构建
        // ================================================================

        /// <summary>
        /// 构建定向矩形截面列表。每个采样点直接用曲面法线定向：
        /// - depthDir = 曲面法线投影到垂直切线的平面（梁厚度沿法线伸出曲面）
        /// - widthDir = tangent × depthDir（在曲面切平面内，梁宽面贴着曲面）
        /// 由于曲面法线本身平滑变化，无需 refUp 传递，自然避免扭转。
        /// </summary>
        private static List<Curve> BuildOrientedSections(
            List<Point3d> samplePoints, List<Vector3d> sampleTangents,
            DivisionResult division, double width, double depth)
        {
            var sections = new List<Curve>();
            double minSectionDist = Math.Min(width, depth) * 0.5;
            Point3d lastCenter = Point3d.Unset;

            for (int i = 0; i < samplePoints.Count; i++)
            {
                Point3d center = samplePoints[i];
                Vector3d tangent = sampleTangents[i];

                // 避免截面过密
                if (lastCenter.IsValid && center.DistanceTo(lastCenter) < minSectionDist)
                    continue;

                // 获取该点的曲面法线（每点独立查找，不传递）
                Vector3d surfaceNormal = FindClosestNormal(center, division);
                if (!surfaceNormal.IsValid || surfaceNormal.IsZero)
                    surfaceNormal = EstimateNormalFromTangent(tangent);
                surfaceNormal.Unitize();

                // depthDir = 曲面法线在垂直切线平面的投影（梁厚度方向，沿法线伸出）
                Vector3d depthDir = surfaceNormal - tangent * (surfaceNormal * tangent);
                if (!depthDir.IsValid || depthDir.IsZero)
                {
                    // 切线与法线几乎平行，用回退估算法线
                    depthDir = EstimateNormalFromTangent(tangent);
                }
                depthDir.Unitize();

                // widthDir = tangent × depthDir（梁宽度方向，在曲面切平面内）
                Vector3d widthDir = Vector3d.CrossProduct(tangent, depthDir);
                widthDir.Unitize();

                // 构建矩形截面四角点
                Point3d p0 = center - widthDir * (width / 2) - depthDir * (depth / 2);
                Point3d p1 = center + widthDir * (width / 2) - depthDir * (depth / 2);
                Point3d p2 = center + widthDir * (width / 2) + depthDir * (depth / 2);
                Point3d p3 = center - widthDir * (width / 2) + depthDir * (depth / 2);

                var rectPoints = new List<Point3d> { p0, p1, p2, p3, p0 };
                var sectionCurve = new PolylineCurve(rectPoints);
                if (sectionCurve.IsValid)
                {
                    sections.Add(sectionCurve);
                    lastCenter = center;
                }
            }

            return sections;
        }

        // ================================================================
        // 沿法线偏移
        // ================================================================

        /// <summary>
        /// 沿曲面法线偏移曲线。在每个采样点查找最近法线并偏移。
        /// </summary>
        private static Curve OffsetAlongNormal(Curve curve, DivisionResult division,
            double offset, double tolerance, bool closedLoop)
        {
            if (Math.Abs(offset) < tolerance)
                return curve.DuplicateCurve();

            int sampleCount = Math.Max(50, (int)(curve.GetLength() / 100));
            sampleCount = Math.Min(sampleCount, 500);

            var offsetPoints = new List<Point3d>();

            for (int i = 0; i <= sampleCount; i++)
            {
                double t = curve.Domain.T0 + (double)i / sampleCount * curve.Domain.Length;
                Point3d pt = curve.PointAt(t);
                if (!pt.IsValid)
                    continue;

                Vector3d surfaceNormal = FindClosestNormal(pt, division);
                if (!surfaceNormal.IsValid || surfaceNormal.IsZero)
                {
                    offsetPoints.Add(pt);
                    continue;
                }

                surfaceNormal.Unitize();
                offsetPoints.Add(pt + surfaceNormal * offset);
            }

            if (offsetPoints.Count < 4)
                return null;

            if (closedLoop)
            {
                Curve result = NurbsCurve.CreateInterpolatedCurve(offsetPoints, 3, CurveKnotStyle.UniformPeriodic);
                if (result != null && result.IsValid)
                    return result;
            }

            Curve fallback = Curve.CreateInterpolatedCurve(offsetPoints, 3);
            return (fallback != null && fallback.IsValid) ? fallback : null;
        }

        // ================================================================
        // 法线工具
        // ================================================================

        /// <summary>
        /// 在网格中查找距离指定点最近的法线。
        /// </summary>
        private static Vector3d FindClosestNormal(Point3d point, DivisionResult division)
        {
            if (division.NormalGrid == null || division.PointGrid == null)
                return Vector3d.Unset;

            double minDist = double.MaxValue;
            Vector3d bestNormal = Vector3d.Unset;

            for (int i = 0; i < division.CurveCount; i++)
            {
                for (int j = 0; j < division.PointsPerCurve; j++)
                {
                    double d = point.DistanceTo(division.PointGrid[i, j]);
                    if (d < minDist)
                    {
                        minDist = d;
                        bestNormal = division.NormalGrid[i, j];
                    }
                }
            }

            return bestNormal;
        }

        /// <summary>
        /// 由切线估算法线（当曲面法线不可用时的回退）。
        /// </summary>
        private static Vector3d EstimateNormalFromTangent(Vector3d tangent)
        {
            Vector3d up = Vector3d.ZAxis;
            if (Math.Abs(tangent * up) > 0.99)
                up = Vector3d.YAxis;

            Vector3d binormal = Vector3d.CrossProduct(tangent, up);
            if (binormal.IsZero)
                return Vector3d.ZAxis;

            binormal.Unitize();
            Vector3d normal = Vector3d.CrossProduct(binormal, tangent);
            normal.Unitize();
            return normal;
        }
    }
}
