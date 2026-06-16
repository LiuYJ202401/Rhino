using System.Collections.Generic;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rh.Geo;

namespace Rh.Geo.Srf
{
    /// <summary>
    /// 曲面几何工具。
    /// 纯几何计算，不假设平面方向，不访问 ActiveDoc。
    /// </summary>
    public static class SurfaceGeo
    {
        // ================================================================
        // 平面类
        // ================================================================

        public static Brep CreateFromPlane(Plane plane, Interval domainU, Interval domainV,
            int uDegree = 3, int vDegree = 3)
        {
            // pointCount = degree + 1（NURBS 最小要求）
            int uCount = uDegree + 1;
            int vCount = vDegree + 1;
            var ns = NurbsSurface.CreateFromPlane(plane, domainU, domainV, uDegree, vDegree, uCount, vCount);
            if (ns == null)
                return null;
            return Brep.CreateFromSurface(ns);
        }

        public static Brep CreateFrom3Points(Point3d first, Point3d second, Point3d third)
        {
            Vector3d xAxis = second - first;
            if (!xAxis.Unitize())
                return null;

            Vector3d normal = Vector3d.CrossProduct(xAxis, third - first);
            if (!normal.Unitize())
                return null;

            Vector3d yAxis = Vector3d.CrossProduct(normal, xAxis);
            var plane = new Plane(first, xAxis, yAxis);

            double u2 = xAxis * (second - first);
            Vector3d toThird = third - first;
            double u3 = xAxis * toThird;
            double v3 = yAxis * toThird;

            double uMin = System.Math.Min(0, System.Math.Min(u2, u3));
            double uMax = System.Math.Max(0, System.Math.Max(u2, u3));
            double vMin = System.Math.Min(0, v3);
            double vMax = System.Math.Max(0, v3);

            return CreateFromPlane(plane, new Interval(uMin, uMax), new Interval(vMin, vMax));
        }

        public static Brep CreateVertical(Point3d start, Point3d end, double height, Vector3d workPlaneNormal)
        {
            if (System.Math.Abs(height) < 1e-12)
                return null;

            Vector3d horizontal = end - start;
            if (!horizontal.Unitize())
                return null;

            Vector3d vertical = workPlaneNormal * (height < 0 ? -1 : 1);
            if (!vertical.Unitize())
                return null;

            var plane = new Plane(start, horizontal, vertical);

            double length = end.DistanceTo(start);
            return CreateFromPlane(plane, new Interval(0, length), new Interval(0, System.Math.Abs(height)));
        }

        public static Brep CreateThroughPoints(IEnumerable<Point3d> points)
        {
            PlaneFitResult result = Plane.FitPlaneToPoints(points, out Plane plane);
            if (result != PlaneFitResult.Success)
                return null;

            double uMin = double.MaxValue, uMax = double.MinValue;
            double vMin = double.MaxValue, vMax = double.MinValue;

            foreach (Point3d pt in points)
            {
                PlaneGeo.PointToUV(plane, pt, out double u, out double v);
                if (u < uMin) uMin = u;
                if (u > uMax) uMax = u;
                if (v < vMin) vMin = v;
                if (v > vMax) vMax = v;
            }

            return CreateFromPlane(plane, new Interval(uMin, uMax), new Interval(vMin, vMax));
        }

        // ================================================================
        // 从点创建
        // ================================================================

        public static Brep CreateFromCorners(Point3d p1, Point3d p2, Point3d p3, Point3d p4)
        {
            NurbsSurface ns;
            if (p4 == Point3d.Unset)
                ns = NurbsSurface.CreateFromCorners(p1, p2, p3);
            else
                ns = NurbsSurface.CreateFromCorners(p1, p2, p3, p4);

            if (ns == null)
                return null;
            return Brep.CreateFromSurface(ns);
        }

        public static Brep CreateThroughPointGrid(IEnumerable<Point3d> points, int uCount, int vCount, int uDegree, int vDegree)
        {
            var ns = NurbsSurface.CreateThroughPoints(points, uCount, vCount, uDegree, vDegree, false, false);
            if (ns == null)
                return null;
            return Brep.CreateFromSurface(ns);
        }

        public static Brep CreateFromControlPointGrid(IEnumerable<Point3d> points, int uCount, int vCount, int uDegree, int vDegree)
        {
            var ns = NurbsSurface.CreateFromPoints(points, uCount, vCount, uDegree, vDegree);
            if (ns == null)
                return null;
            return Brep.CreateFromSurface(ns);
        }

        // ================================================================
        // 从曲线创建
        // ================================================================

        public static Brep CreateEdgeSurface(IEnumerable<Curve> edges)
        {
            return Brep.CreateEdgeSurface(edges);
        }

        public static Brep[] CreatePlanarBreps(IEnumerable<Curve> curves, double tolerance)
        {
            return Brep.CreatePlanarBreps(curves, tolerance);
        }

        public static Brep[] CreateLoft(IEnumerable<Curve> curves, Point3d start, Point3d end, LoftType loftType, bool closed)
        {
            return Brep.CreateFromLoft(curves, start, end, loftType, closed);
        }

        public static Brep CreateNetworkSurface(IEnumerable<Curve> curves, int continuity,
            double edgeTolerance, double interiorTolerance, double angleTolerance)
        {
            int error;
            var ns = NurbsSurface.CreateNetworkSurface(curves, continuity,
                edgeTolerance, interiorTolerance, angleTolerance, out error);
            if (ns == null || error != 0)
                return null;
            return Brep.CreateFromSurface(ns);
        }

        public static Brep CreateNetworkSurface(IEnumerable<Curve> uCurves, IEnumerable<Curve> vCurves, int continuity,
            double edgeTolerance, double interiorTolerance, double angleTolerance)
        {
            int error;
            var ns = NurbsSurface.CreateNetworkSurface(uCurves, continuity, continuity,
                vCurves, continuity, continuity,
                edgeTolerance, interiorTolerance, angleTolerance, out error);
            if (ns == null || error != 0)
                return null;
            return Brep.CreateFromSurface(ns);
        }

        // ================================================================
        // 扫掠类
        // ================================================================

        public static Brep[] CreateSweep1(Curve rail, IEnumerable<Curve> shapes, bool closed, double tolerance)
        {
            return Brep.CreateFromSweep(rail, shapes, closed, tolerance);
        }

        public static Brep[] CreateSweep2(Curve rail1, Curve rail2, IEnumerable<Curve> shapes, bool closed, double tolerance)
        {
            return Brep.CreateFromSweep(rail1, rail2, shapes, closed, tolerance);
        }

        // ================================================================
        // 旋转类
        // ================================================================

        public static Brep CreateRevolveFull(Curve profile, Line axis)
        {
            var revSurf = RevSurface.Create(profile, axis, 0.0, 2.0 * System.Math.PI);
            if (revSurf == null)
                return null;
            return Brep.CreateFromSurface(revSurf);
        }

        public static Brep CreateRevolvePartial(Curve profile, Line axis, double startAngle, double endAngle)
        {
            var revSurf = RevSurface.Create(profile, axis, startAngle, endAngle);
            if (revSurf == null)
                return null;
            return Brep.CreateFromSurface(revSurf);
        }

        public static Brep CreateRailRevolve(Curve profile, Curve rail, Line axis, bool scaleHeight)
        {
            var ns = NurbsSurface.CreateRailRevolvedSurface(profile, rail, axis, scaleHeight);
            if (ns == null)
                return null;
            return Brep.CreateFromSurface(ns);
        }

        // ================================================================
        // 挤出类
        // ================================================================

        public static Brep CreateExtrudeDirection(Curve profile, Vector3d direction)
        {
            var srf = Surface.CreateExtrusion(profile, direction);
            if (srf == null)
                return null;
            return Brep.CreateFromSurface(srf);
        }

        public static Brep CreateExtrudeAlongCrv(Curve profile, Curve path, bool cap, double tolerance)
        {
            Brep[] breps = Brep.CreateFromSweep(path, new[] { profile }, false, tolerance);
            if (breps == null || breps.Length == 0)
                return null;

            if (!cap)
                return breps[0];

            // 加盖：从扫掠结果的裸边提取边界曲线，用 CreatePlanarBreps 封面
            var allBreps = new List<Brep> { breps[0] };

            // 提取裸边（nakedOnly=true: 仅获取只属于一个面的开放边缘）
            Curve[] nakedEdges = breps[0].DuplicateEdgeCurves(true);
            if (nakedEdges == null || nakedEdges.Length == 0)
                return breps[0];

            // 将裸边按闭合环分组（每端开口可能由多条边组成，需 JoinEdges）
            Curve[] loops = Curve.JoinCurves(nakedEdges);

            foreach (Curve loop in loops)
            {
                if (!loop.IsClosed)
                    continue;

                Brep[] caps = Brep.CreatePlanarBreps(loop, tolerance);
                if (caps != null)
                {
                    foreach (Brep capBrep in caps)
                        allBreps.Add(capBrep);
                }
            }

            if (allBreps.Count <= 1)
                return breps[0];

            // 合并所有面为单一实体
            var joined = Brep.JoinBreps(allBreps, tolerance);
            if (joined == null || joined.Length == 0)
                return breps[0];

            return joined[0];
        }

        public static Brep CreateExtrudeTapered(Curve profile, Vector3d direction, double distance,
            double draftAngleRadians, double tolerance, double angleTolerance)
        {
            Point3d basePoint;
            if (profile.IsClosed)
            {
                BoundingBox bbox = profile.GetBoundingBox(true);
                basePoint = bbox.Center;
            }
            else
            {
                basePoint = profile.PointAtStart;
            }

            Brep[] breps = Brep.CreateFromTaperedExtrude(
                profile, distance, direction, basePoint,
                draftAngleRadians, ExtrudeCornerType.Sharp,
                tolerance, angleTolerance);

            if (breps == null || breps.Length == 0)
                return null;
            return breps[0];
        }

        public static Brep CreateExtrudeToPoint(Curve profile, Point3d apex)
        {
            var srf = Surface.CreateExtrusionToPoint(profile, apex);
            if (srf == null)
                return null;
            return Brep.CreateFromSurface(srf);
        }

        // ================================================================
        // 补面与拟合
        // ================================================================

        public static Brep CreatePatch(IEnumerable<GeometryBase> geometry, int uSpans, int vSpans, double tolerance)
        {
            return Brep.CreatePatch(geometry, uSpans, vSpans, tolerance);
        }

        // ================================================================
        // 特殊曲面
        // ================================================================

        /// <summary>
        /// 从两条曲线创建直纹面（内部复用方法）。
        /// 处理节点向量兼容性 + 直纹面构造 + Brep 转换。
        /// </summary>
        private static Brep CreateRuledFromTwoCurves(Curve curveA, Curve curveB)
        {
            var nc1 = curveA.ToNurbsCurve();
            var nc2 = curveB.ToNurbsCurve();
            NurbsCurve[] compatible = NurbsCurve.MakeCompatible(new[] { nc1, nc2 }, Point3d.Unset, Point3d.Unset, 0, 0, 0.0, 0.0);
            if (compatible == null || compatible.Length < 2)
                return null;

            var ns = NurbsSurface.CreateRuledSurface(compatible[0], compatible[1]);
            if (ns == null)
                return null;
            return Brep.CreateFromSurface(ns);
        }

        /// <summary>
        /// 切割平面：通过对象包围盒构造切割平面。
        /// 使用 PlaneSurface.CreateThroughBox。
        /// </summary>
        public static Brep CreateCutPlane(Plane plane, IEnumerable<GeometryBase> objects)
        {
            // 计算合并包围盒
            BoundingBox bbox = BoundingBox.Unset;
            foreach (GeometryBase obj in objects)
            {
                BoundingBox objBbox = obj.GetBoundingBox(true);
                if (objBbox.IsValid)
                    bbox = BoundingBox.Union(bbox, objBbox);
            }

            if (!bbox.IsValid)
                return null;

            // 稍微扩大包围盒避免退化
            bbox.Inflate(1.0);

            var planeSrf = PlaneSurface.CreateThroughBox(plane, bbox);
            if (planeSrf == null)
                return null;

            return Brep.CreateFromSurface(planeSrf);
        }

        /// <summary>
        /// 彩带曲面：偏移曲线后创建直纹曲面。
        /// 使用 Curve.Offset + NurbsSurface.CreateRuledSurface。
        /// </summary>
        public static Brep CreateRibbon(Curve curve, double distance, Plane plane, double tolerance)
        {
            Curve[] offset = curve.Offset(plane, distance, tolerance, CurveOffsetCornerStyle.Sharp);
            if (offset == null || offset.Length == 0)
                return null;

            // 合并多段偏移为一条
            Curve offsetCurve = offset.Length == 1 ? offset[0] : Curve.JoinCurves(offset)[0];

            return CreateRuledFromTwoCurves(curve, offsetCurve);
        }

        /// <summary>
        /// 翼面曲面：沿曲面法线方向挤出曲面上的曲线。
        /// 在曲线上采样点，获取法线方向，偏移生成第二条曲线后直纹面。
        /// </summary>
        public static Brep CreateFin(Curve curve, BrepFace face, double height, double tolerance)
        {
            // 在曲线上均匀采样点
            int sampleCount = 50;
            Point3d[] points;
            curve.DivideByCount(sampleCount, true, out points);
            if (points == null || points.Length < 2)
                return null;

            // 对每个采样点，在曲面上找最近点并获取法线
            var offsetPoints = new List<Point3d>();
            foreach (Point3d pt in points)
            {
                if (!face.ClosestPoint(pt, out double u, out double v))
                    return null;

                Vector3d normal = face.NormalAt(u, v);
                if (face.OrientationIsReversed)
                    normal.Reverse();

                offsetPoints.Add(pt + normal * height);
            }

            // 用偏移点构造插值曲线
            Curve offsetCurve = Curve.CreateInterpolatedCurve(offsetPoints, 3);
            if (offsetCurve == null)
                return null;

            return CreateRuledFromTwoCurves(curve, offsetCurve);
        }

        /// <summary>
        /// 覆盖曲面：在工作平面上生成点网格，投影到物体表面后拟合曲面。
        /// 使用 Rhino.Geometry.Intersection.RayShoot + NurbsSurface.CreateThroughPoints。
        /// </summary>
        public static Brep CreateDrape(IEnumerable<GeometryBase> objects, Plane plane,
            int uSpacing, int vSpacing, double tolerance)
        {
            // 计算对象在投影平面上的 UV 包围盒
            double uMin = double.MaxValue, uMax = double.MinValue;
            double vMin = double.MaxValue, vMax = double.MinValue;

            foreach (GeometryBase obj in objects)
            {
                BoundingBox bbox = obj.GetBoundingBox(true);
                Point3d[] corners = bbox.GetCorners();
                foreach (Point3d corner in corners)
                {
                    PlaneGeo.PointToUV(plane, corner, out double u, out double v);
                    if (u < uMin) uMin = u;
                    if (u > uMax) uMax = u;
                    if (v < vMin) vMin = v;
                    if (v > vMax) vMax = v;
                }
            }

            if (uMin >= uMax || vMin >= vMax)
                return null;

            // 生成点网格
            var points = new List<Point3d>();
            var geometryList = new List<GeometryBase>(objects);
            Vector3d dir = -plane.Normal;

            for (int j = 0; j < vSpacing; j++)
            {
                double v = vMin + (vMax - vMin) * j / (vSpacing - 1);
                for (int i = 0; i < uSpacing; i++)
                {
                    double u = uMin + (uMax - uMin) * i / (uSpacing - 1);
                    Point3d origin = plane.Origin + plane.XAxis * u + plane.YAxis * v;

                    // 射线投影求交
                    var ray = new Ray3d(origin, dir);
                    Point3d[] hits = Intersection.RayShoot(ray, geometryList, 1);

                    if (hits != null && hits.Length > 0)
                        points.Add(hits[0]);
                    else
                        points.Add(origin); // 无交点则保持原高度
                }
            }

            var ns = NurbsSurface.CreateThroughPoints(points, uSpacing, vSpacing, 3, 3, false, false);
            if (ns == null)
                return null;
            return Brep.CreateFromSurface(ns);
        }

        /// <summary>
        /// 高度场曲面：由灰度图像创建曲面。
        /// 读取图像像素灰度值映射为高度。
        /// </summary>
        public static Brep CreateHeightfield(string imagePath, Plane plane,
            double width, double heightSize, double maxHeight,
            int samplesX, int samplesY)
        {
            // 读取图像
            System.Drawing.Bitmap bmp;
            try
            {
                bmp = new System.Drawing.Bitmap(imagePath);
            }
            catch
            {
                return null;
            }

            // 采样像素灰度值
            var points = new List<Point3d>();
            double stepX = (double)bmp.Width / samplesX;
            double stepY = (double)bmp.Height / samplesY;

            for (int j = 0; j < samplesY; j++)
            {
                for (int i = 0; i < samplesX; i++)
                {
                    int px = (int)((i + 0.5) * stepX);
                    int py = (int)((j + 0.5) * stepY);
                    px = System.Math.Min(px, bmp.Width - 1);
                    py = System.Math.Min(py, bmp.Height - 1);

                    System.Drawing.Color color = bmp.GetPixel(px, py);
                    double gray = (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) / 255.0;
                    double h = gray * maxHeight;

                    double u = (double)i / (samplesX - 1) * width - width / 2;
                    double v = (double)j / (samplesY - 1) * heightSize - heightSize / 2;
                    Point3d pt = plane.Origin + plane.XAxis * u + plane.YAxis * v + plane.Normal * h;
                    points.Add(pt);
                }
            }

            bmp.Dispose();

            var ns = NurbsSurface.CreateThroughPoints(points, samplesX, samplesY, 3, 3, false, false);
            if (ns == null)
                return null;
            return Brep.CreateFromSurface(ns);
        }

        /// <summary>
        /// 可展开曲面：在两条曲线间创建近似可展开曲面。
        /// 用双轨扫掠近似实现。
        /// </summary>
        public static Brep CreateDevLoft(Curve rail1, Curve rail2, double tolerance)
        {
            // 用一个截面曲线（连接两轨道起点）作为形状
            Point3d start1 = rail1.PointAtStart;
            Point3d start2 = rail2.PointAtStart;
            Line shape = new Line(start1, start2);
            Curve shapeCurve = new LineCurve(shape);

            Brep[] breps = Brep.CreateFromSweep(rail1, rail2, new[] { shapeCurve }, false, tolerance);
            if (breps == null || breps.Length == 0)
                return null;
            return breps[0];
        }
    }
}
