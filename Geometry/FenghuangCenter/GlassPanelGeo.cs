using System;
using System.Collections.Generic;
using Rhino.Geometry;
using Rh.Geo.Srf;
using Rh.Geo.Sld;

namespace Rh.Geo.FenghuangCenter
{
    /// <summary>
    /// 玻璃面板与窗框生成工具。
    /// 自定义算法：四边形偏移 + 面拼合。
    /// 平面面委托 SurfaceGeo.CreatePlanarBreps，合并委托 SolidGeo.CreateSolidFromBreps，Box 委托 SolidGeo.CreateFromBox。
    /// </summary>
    public static class GlassPanelGeo
    {
        // ================================================================
        // 玻璃面板
        // ================================================================

        /// <summary>
        /// 在四边形区域内生成玻璃面板实体（底面 + 顶面 + 侧面合并）。
        /// </summary>
        /// <param name="quads">四边形列表</param>
        /// <param name="offset">玻璃底面沿法线的偏移距离</param>
        /// <param name="thickness">玻璃厚度</param>
        /// <param name="tolerance">公差</param>
        /// <returns>玻璃面板实体列表；单块失败时跳过</returns>
        public static List<Brep> BuildPanels(List<Quad> quads, double offset, double thickness, double tolerance)
        {
            var breps = new List<Brep>();

            for (int i = 0; i < quads.Count; i++)
            {
                try
                {
                    Brep glassBrep = BuildSinglePanel(quads[i], offset, thickness, tolerance);
                    if (glassBrep != null)
                        breps.Add(glassBrep);
                }
                catch
                {
                    // 单块玻璃失败时跳过
                }
            }

            return breps;
        }

        /// <summary>
        /// 生成单块玻璃面板。
        /// </summary>
        private static Brep BuildSinglePanel(Quad quad, double offset, double thickness, double tolerance)
        {
            Point3d[] corners = quad.CornerPoints;
            Vector3d[] normals = quad.CornerNormals;
            if (corners == null || corners.Length != 4)
                return null;

            foreach (var c in corners)
                if (!c.IsValid) return null;

            // 计算平均法线
            Vector3d avgNormal = ComputeAverageNormal(normals);

            // 角点沿各自法线偏移
            Point3d[] offsetCorners = new Point3d[4];
            for (int i = 0; i < 4; i++)
            {
                Vector3d n = normals[i];
                if (!n.IsValid || n.IsZero)
                    n = avgNormal;
                n.Unitize();
                offsetCorners[i] = corners[i] + n * offset;
            }

            // 创建底面（委托框架 SurfaceGeo.CreatePlanarBreps）
            Brep bottomFace = CreateQuadFace(offsetCorners, tolerance);
            if (bottomFace == null)
                return null;

            // 顶面 = 底面平移 thickness
            Brep topFace = bottomFace.DuplicateBrep();
            Vector3d thicknessOffset = avgNormal * thickness;
            topFace.Translate(thicknessOffset);

            // 收集所有面
            var allBreps = new List<Brep> { bottomFace, topFace };

            // 4 个侧面
            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                Point3d b0 = offsetCorners[i];
                Point3d b1 = offsetCorners[next];
                Point3d t0 = b0 + thicknessOffset;
                Point3d t1 = b1 + thicknessOffset;

                var sidePoints = new Point3d[] { b0, b1, t1, t0, b0 };
                var sideCurve = new PolylineCurve(sidePoints);
                if (sideCurve != null && sideCurve.IsValid)
                {
                    Brep[] sideBreps = SurfaceGeo.CreatePlanarBreps(new[] { sideCurve }, tolerance);
                    if (sideBreps != null && sideBreps.Length > 0)
                        allBreps.Add(sideBreps[0]);
                }
            }

            // 合并（委托框架 SolidGeo.CreateSolidFromBreps）
            Brep joined = SolidGeo.CreateSolidFromBreps(allBreps, tolerance);
            return joined ?? bottomFace;
        }

        /// <summary>
        /// 创建四边形平面面。优先用 CreatePlanarBreps，失败时回退到放样。
        /// </summary>
        private static Brep CreateQuadFace(Point3d[] corners, double tolerance)
        {
            var boundary = new PolylineCurve(new Point3d[]
            {
                corners[0], corners[1], corners[2], corners[3], corners[0]
            });

            Brep[] planarBreps = SurfaceGeo.CreatePlanarBreps(new[] { boundary }, tolerance);
            if (planarBreps != null && planarBreps.Length > 0)
                return planarBreps[0];

            // 回退：用直线放样（委托框架 SurfaceGeo.CreateLoft）
            Brep[] loftResult = SurfaceGeo.CreateLoft(
                new Curve[]
                {
                    new LineCurve(corners[0], corners[1]),
                    new LineCurve(corners[3], corners[2])
                },
                Point3d.Unset, Point3d.Unset,
                LoftType.Straight, false);

            if (loftResult != null && loftResult.Length > 0)
                return loftResult[0];

            return null;
        }

        // ================================================================
        // 窗框
        // ================================================================

        /// <summary>
        /// 沿网格边生成窗框实体（Box 沿边定向）。
        /// </summary>
        /// <param name="edges">网格边列表</param>
        /// <param name="frameOffset">窗框中心沿法线的偏移距离</param>
        /// <param name="width">窗框宽度（横向）</param>
        /// <param name="depth">窗框深度（沿法线）</param>
        /// <param name="tolerance">公差</param>
        /// <returns>窗框实体列表；单根失败时跳过</returns>
        public static List<Brep> BuildFrames(List<GridEdge> edges, double frameOffset,
            double width, double depth, double tolerance)
        {
            var breps = new List<Brep>();
            double halfWidth = width / 2.0;
            double halfDepth = depth / 2.0;

            for (int i = 0; i < edges.Count; i++)
            {
                try
                {
                    Brep frameBrep = BuildSingleFrame(edges[i], frameOffset, halfWidth, halfDepth, tolerance);
                    if (frameBrep != null)
                        breps.Add(frameBrep);
                }
                catch
                {
                    // 单根窗框失败时跳过
                }
            }

            return breps;
        }

        /// <summary>
        /// 生成单根窗框 Box。
        /// </summary>
        private static Brep BuildSingleFrame(GridEdge edge, double frameOffset,
            double halfWidth, double halfDepth, double tolerance)
        {
            Point3d p0 = edge.Start;
            Point3d p1 = edge.End;
            if (!p0.IsValid || !p1.IsValid) return null;

            Vector3d edgeDir = p1 - p0;
            double edgeLen = edgeDir.Length;
            if (edgeLen < tolerance) return null;
            edgeDir.Unitize();

            // 平均法线
            Vector3d n0 = edge.StartNormal;
            Vector3d n1 = edge.EndNormal;
            if (!n0.IsValid || n0.IsZero) n0 = Vector3d.ZAxis;
            if (!n1.IsValid || n1.IsZero) n1 = Vector3d.ZAxis;
            n0.Unitize();
            n1.Unitize();

            Vector3d avgN = (n0 + n1) / 2.0;
            avgN.Unitize();

            // 计算截面定向
            Vector3d lateral = Vector3d.CrossProduct(edgeDir, avgN);
            if (lateral.IsZero) return null;
            lateral.Unitize();

            Vector3d correctedN = Vector3d.CrossProduct(lateral, edgeDir);
            correctedN.Unitize();

            if (correctedN * avgN < 0)
            {
                correctedN.Reverse();
                lateral.Reverse();
            }

            // Box 中心：边中点 + 法线偏移
            Point3d boxCenter = (p0 + p1) / 2.0 + correctedN * frameOffset;

            var plane = new Plane(boxCenter, edgeDir, lateral);
            Box box = new Box(plane,
                new Interval(-edgeLen / 2.0, edgeLen / 2.0),
                new Interval(-halfWidth, halfWidth),
                new Interval(-halfDepth, halfDepth));

            if (!box.IsValid)
                return null;

            // 创建 Box（委托框架 SolidGeo.CreateFromBox）
            return SolidGeo.CreateFromBox(box);
        }

        // ================================================================
        // 工具方法
        // ================================================================

        /// <summary>
        /// 计算法线数组的平均法线。
        /// </summary>
        private static Vector3d ComputeAverageNormal(Vector3d[] normals)
        {
            Vector3d avgNormal = Vector3d.Zero;
            int normalCount = 0;

            foreach (var n in normals)
            {
                if (n.IsValid && !n.IsZero)
                {
                    avgNormal += n;
                    normalCount++;
                }
            }

            if (normalCount > 0)
            {
                avgNormal /= normalCount;
                avgNormal.Unitize();
            }
            else
            {
                avgNormal = Vector3d.ZAxis;
            }

            return avgNormal;
        }
    }
}
