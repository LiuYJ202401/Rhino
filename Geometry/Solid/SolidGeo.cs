using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Geometry;
using Rh.Geo.Crv;

namespace Rh.Geo.Sld
{
    /// <summary>
    /// 实体几何工具。
    /// 创建闭合实体（Brep，IsSolid=true）。纯几何计算，不访问 ActiveDoc。
    /// 与 SurfaceGeo 的核心区别：本类强制调用 CapPlanarHoles 或使用带 cap 参数的 API。
    /// </summary>
    public static class SolidGeo
    {
        // ================================================================
        // 内部工具
        // ================================================================

        /// <summary>封盖开放 Brep 的平面开口</summary>
        private static Brep CapPlanarHoles(Brep brep, double tolerance)
        {
            if (brep == null)
                return null;
            if (!brep.IsSolid)
            {
                var capped = brep.CapPlanarHoles(tolerance);
                if (capped != null)
                    return capped;
            }
            return brep;
        }

        // ================================================================
        // 一、单曲面实体
        // ================================================================

        /// <summary>创建球体实体</summary>
        public static Brep CreateSphere(Point3d center, Vector3d normal, double radius)
        {
            if (radius <= 0)
                return null;
            normal.Unitize();
            var plane = new Plane(center, normal);
            var sphere = new Sphere(plane, radius);
            return sphere.ToBrep();
        }

        /// <summary>创建椭球体实体</summary>
        public static Brep CreateEllipsoid(Point3d center, Vector3d normal, Vector3d radii)
        {
            if (radii.X <= 0 || radii.Y <= 0 || radii.Z <= 0)
                return null;
            normal.Unitize();
            var plane = new Plane(center, normal);
            var sphere = new Sphere(plane, 1.0);
            var brep = sphere.ToBrep();
            if (brep == null)
                return null;
            var scale = Transform.Scale(plane, radii.X, radii.Y, radii.Z);
            brep.Transform(scale);
            return brep;
        }

        /// <summary>创建圆环体实体</summary>
        public static Brep CreateTorus(Point3d center, Vector3d normal, double majorRadius, double minorRadius)
        {
            if (majorRadius <= 0 || minorRadius <= 0 || minorRadius >= majorRadius)
                return null;
            normal.Unitize();
            var plane = new Plane(center, normal);
            var torus = new Torus(plane, majorRadius, minorRadius);
            var revSrf = torus.ToRevSurface();
            return Brep.CreateFromSurface(revSrf);
        }

        // ================================================================
        // 二、多曲面实体
        // ================================================================

        /// <summary>从 Box 创建长方体实体</summary>
        public static Brep CreateFromBox(Box box)
        {
            if (!box.IsValid)
                return null;
            return Brep.CreateFromBox(box);
        }

        /// <summary>从两底面角点和法向量创建长方体实体（法向量长度=高度）</summary>
        public static Brep CreateFromCorners(Point3d corner1, Point3d corner2, Vector3d normal)
        {
            if (!normal.IsValid || normal.IsZero)
                return null;

            // normal 的方向 = 底面法线，normal 的长度 = 高度
            double height = normal.Length;
            var dir = normal;
            dir.Unitize();

            // 将 corner2 转换到以 corner1 为原点、normal 方向为法线的平面局部坐标系
            var plane = new Plane(corner1, dir);
            plane.RemapToPlaneSpace(corner2, out Point3d local);

            var x = new Interval(0, local.X);
            var y = new Interval(0, local.Y);
            var z = new Interval(0, height);
            var box = new Box(plane, x, y, z);
            return Brep.CreateFromBox(box);
        }

        /// <summary>创建圆柱实体</summary>
        public static Brep CreateCylinder(Point3d baseCenter, Vector3d normal, double radius, double height,
            bool capEnds)
        {
            if (radius <= 0 || height <= 0)
                return null;
            normal.Unitize();
            var plane = new Plane(baseCenter, normal);
            var circle = new Circle(plane, radius);
            var cylinder = new Cylinder(circle, height);
            return cylinder.ToBrep(capEnds, capEnds);
        }

        /// <summary>创建圆锥实体</summary>
        public static Brep CreateCone(Point3d baseCenter, Vector3d normal, double bottomRadius, double height,
            bool capEnd)
        {
            if (bottomRadius <= 0 || height <= 0)
                return null;
            normal.Unitize();
            var plane = new Plane(baseCenter, normal);
            var cone = new Cone(plane, height, bottomRadius);
            return cone.ToBrep(capEnd);
        }

        /// <summary>创建截锥体实体</summary>
        public static Brep CreateTruncatedCone(Point3d baseCenter, Vector3d normal,
            double bottomRadius, double topRadius, double height, bool capEnds, double tolerance)
        {
            if (bottomRadius <= 0 || topRadius <= 0 || height <= 0)
                return null;
            normal.Unitize();

            // 使用 RevSurface 构造截锥体（RhinoCommon 官方推荐方式）
            var bottomPlane = new Plane(baseCenter, normal);
            var topCenter = baseCenter + normal * height;
            var bottomCircle = new Circle(bottomPlane, bottomRadius);
            var topCircle = new Circle(new Plane(topCenter, normal), topRadius);

            // 母线：从底圆 PointAt(0) 到顶圆 PointAt(0)
            var p1 = bottomCircle.PointAt(0);
            var p2 = topCircle.PointAt(0);
            var shapeCurve = new LineCurve(p1, p2);
            var axis = new Line(bottomCircle.Center, topCircle.Center);
            var revSrf = RevSurface.Create(shapeCurve, axis);

            return Brep.CreateFromRevSurface(revSrf, capEnds, capEnds);
        }

        /// <summary>创建中空圆柱实体</summary>
        public static Brep CreateTube(Point3d baseCenter, Vector3d normal,
            double innerRadius, double outerRadius, double height, bool capEnds, double tolerance)
        {
            if (innerRadius <= 0 || outerRadius <= 0 || innerRadius >= outerRadius || height <= 0)
                return null;
            normal.Unitize();
            var plane = new Plane(baseCenter, normal);
            var outerCircle = new Circle(plane, outerRadius);
            var innerCircle = new Circle(plane, innerRadius);
            var outerCyl = new Cylinder(outerCircle, height);
            var innerCyl = new Cylinder(innerCircle, height);
            var outerBrep = outerCyl.ToBrep(capEnds, capEnds);
            var innerBrep = innerCyl.ToBrep(true, true);
            var result = Brep.CreateBooleanDifference(outerBrep, innerBrep, tolerance);
            return result != null && result.Length > 0 ? result[0] : null;
        }

        /// <summary>创建棱锥实体</summary>
        public static Brep CreatePyramid(Point3d baseCenter, Vector3d normal, int sides,
            double radius, double height, bool capBase, double tolerance)
        {
            if (sides < 3 || radius <= 0 || height <= 0)
                return null;
            normal.Unitize();
            var plane = new Plane(baseCenter, normal);
            var apex = baseCenter + normal * height;

            // 底面多边形
            var basePoly = PolygonGeo.CreateRegularPolygon(plane, baseCenter, sides, radius);
            if (basePoly == null)
                return null;

            var sideBreps = new List<Brep>();

            // 侧面三角形
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                var face = Brep.CreateFromCornerPoints(basePoly[i], basePoly[next], apex, tolerance);
                if (face != null)
                    sideBreps.Add(face);
            }

            if (sideBreps.Count == 0)
                return null;

            // 合并侧面为开放壳，再由 CapPlanarHoles 统一封盖
            var joined = Brep.JoinBreps(sideBreps, tolerance);
            if (joined == null || joined.Length == 0)
                return null;

            var result = joined[0];
            if (capBase)
                result = CapPlanarHoles(result, tolerance);

            return result;
        }

        /// <summary>创建截棱锥实体</summary>
        public static Brep CreateTruncatedPyramid(Point3d baseCenter, Vector3d normal, int sides,
            double bottomRadius, double topRadius, double height, bool capEnds, double tolerance)
        {
            if (sides < 3 || bottomRadius <= 0 || topRadius <= 0 || height <= 0)
                return null;
            normal.Unitize();
            var basePlane = new Plane(baseCenter, normal);
            var topCenter = baseCenter + normal * height;
            var topPlane = new Plane(topCenter, normal);

            var bottomPoly = PolygonGeo.CreateRegularPolygon(basePlane, baseCenter, sides, bottomRadius);
            var topPoly = PolygonGeo.CreateRegularPolygon(topPlane, topCenter, sides, topRadius);
            if (bottomPoly == null || topPoly == null)
                return null;

            var sideBreps = new List<Brep>();

            // 侧面四边形
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                var face = Brep.CreateFromCornerPoints(
                    bottomPoly[i], bottomPoly[next], topPoly[next], topPoly[i], tolerance);
                if (face != null)
                    sideBreps.Add(face);
            }

            if (sideBreps.Count == 0)
                return null;

            // 合并侧面为开放壳，再由 CapPlanarHoles 统一封盖
            var joined = Brep.JoinBreps(sideBreps, tolerance);
            if (joined == null || joined.Length == 0)
                return null;

            var result = joined[0];
            if (capEnds)
                result = CapPlanarHoles(result, tolerance);

            return result;
        }

        // ================================================================
        // 三、从曲线挤出为实体
        // ================================================================

        /// <summary>闭合曲线沿方向挤出为实体</summary>
        public static Brep CreateExtrudeSolid(Curve profile, Vector3d direction, bool capEnds, double tolerance)
        {
            if (profile == null || !profile.IsClosed || direction.IsZero)
                return null;

            // 检测挤出方向是否在轮廓平面内（会导致退化几何，无法封盖）
            if (profile.TryGetPlane(out Plane profilePlane, tolerance))
            {
                double dot = Math.Abs(direction * profilePlane.Normal);
                if (dot < tolerance)
                {
                    RhinoApp.WriteLine("[CreateExtrudeSolid] 错误：挤出方向与轮廓平面平行，会导致退化几何");
                    return null;
                }
            }

            var srf = Surface.CreateExtrusion(profile, direction);
            if (srf == null)
                return null;
            var brep = Brep.CreateFromSurface(srf);
            if (capEnds)
                brep = CapPlanarHoles(brep, tolerance);
            return brep;
        }

        /// <summary>闭合轮廓绕轴旋转后封盖形成实体</summary>
        public static Brep CreateRevolveSolid(Curve profile, Line axis,
            double startAngle, double endAngle, bool capEnds, double tolerance)
        {
            if (profile == null || !axis.IsValid)
                return null;
            var revSrf = RevSurface.Create(profile, axis, startAngle, endAngle);
            if (revSrf == null)
                return null;
            var brep = Brep.CreateFromSurface(revSrf);
            if (capEnds)
                brep = CapPlanarHoles(brep, tolerance);
            return brep;
        }

        /// <summary>闭合截面沿双轨扫掠后封盖形成实体</summary>
        public static Brep CreateSweepSolid(Curve rail1, Curve rail2,
            IEnumerable<Curve> sections, bool capEnds, double tolerance)
        {
            if (rail1 == null || rail2 == null || sections == null)
                return null;
            var shapeList = sections.ToList();
            if (shapeList.Count == 0)
                return null;
            var breps = Brep.CreateFromSweep(rail1, rail2, shapeList, false, tolerance);
            if (breps == null || breps.Length == 0)
                return null;
            var result = breps[0];
            if (capEnds)
                result = CapPlanarHoles(result, tolerance);
            return result;
        }

        /// <summary>多条闭合截面放样后封盖形成实体</summary>
        public static Brep CreateLoftSolid(IEnumerable<Curve> curves, int loftType, bool capEnds, double tolerance)
        {
            if (curves == null)
                return null;
            var curveList = curves.ToList();
            if (curveList.Count < 2)
                return null;
            var type = (LoftType)loftType;
            var breps = Brep.CreateFromLoft(curveList, Point3d.Unset, Point3d.Unset, type, false);
            if (breps == null || breps.Length == 0)
                return null;
            var result = breps[0];
            if (capEnds)
                result = CapPlanarHoles(result, tolerance);
            return result;
        }

        // ================================================================
        // 四、Solid 专属命令
        // ================================================================

        /// <summary>创建单壁管道</summary>
        public static Brep CreatePipe(Curve rail, double radius, PipeCapMode capMode,
            double tolerance, double angleTolerance)
        {
            if (rail == null || radius <= 0)
                return null;
            var pipes = Brep.CreatePipe(rail, radius, false, capMode, true, tolerance, angleTolerance);
            if (pipes == null || pipes.Length == 0)
                return null;
            if (pipes.Length == 1)
                return pipes[0];
            // 合并多个管道段为单一实体
            var joined = Brep.JoinBreps(pipes, tolerance);
            return (joined != null && joined.Length > 0) ? joined[0] : null;
        }

        /// <summary>创建双壁厚壁管道</summary>
        public static Brep CreateThickPipe(Curve rail, double innerRadius, double outerRadius,
            PipeCapMode capMode, double tolerance, double angleTolerance)
        {
            if (rail == null || innerRadius <= 0 || outerRadius <= 0 || innerRadius >= outerRadius)
                return null;
            var railParams = new double[] { 0.0, 1.0 };
            var innerRadii = new double[] { innerRadius, innerRadius };
            var outerRadii = new double[] { outerRadius, outerRadius };
            var pipes = Brep.CreateThickPipe(rail, railParams, innerRadii, outerRadii,
                false, capMode, true, tolerance, angleTolerance);
            if (pipes == null || pipes.Length == 0)
                return null;
            if (pipes.Length == 1)
                return pipes[0];
            // 合并多个管道段为单一实体
            var joined = Brep.JoinBreps(pipes, tolerance);
            return (joined != null && joined.Length > 0) ? joined[0] : null;
        }

        /// <summary>偏移多段线并加盖形成实体板（原轮廓与偏移轮廓之间为板面）</summary>
        public static Brep CreateSlab(PolylineCurve profile, double offsetDistance, Vector3d direction,
            bool capEnds, double tolerance)
        {
            if (profile == null || !profile.IsClosed || direction.IsZero || offsetDistance == 0)
                return null;
            direction.Unitize();
            // 以 direction 为法向构建偏移平面
            var plane = new Plane(profile.PointAtStart, direction);

            // 偏移轮廓（实例方法，参数顺序: plane, distance）
            var offsetCurves = profile.Offset(plane, offsetDistance, tolerance, CurveOffsetCornerStyle.Sharp);
            if (offsetCurves == null || offsetCurves.Length == 0)
                return null;

            var original = profile.ToNurbsCurve();
            var offset = offsetCurves[0].ToNurbsCurve();

            var breps = new List<Brep>();

            // 侧面：原轮廓与偏移轮廓之间创建直纹面
            var ruled = NurbsSurface.CreateRuledSurface(original, offset);
            if (ruled != null)
            {
                var sideBrep = Brep.CreateFromSurface(ruled);
                if (sideBrep != null)
                    breps.Add(sideBrep);
            }

            // 封盖：原轮廓平面和偏移轮廓平面
            if (capEnds)
            {
                var caps1 = Brep.CreatePlanarBreps(original, tolerance);
                var caps2 = Brep.CreatePlanarBreps(offset, tolerance);
                if (caps1 != null)
                    foreach (var c in caps1)
                        breps.Add(c);
                if (caps2 != null)
                    foreach (var c in caps2)
                        breps.Add(c);
            }

            if (breps.Count == 0)
                return null;

            // 合并所有面为单一实体
            var joined = Brep.JoinBreps(breps, tolerance);
            if (joined == null || joined.Length == 0)
                return null;

            return joined[0];
        }

        /// <summary>将开放曲面偏移加厚形成闭合实体</summary>
        public static Brep CreateThicken(Brep brep, double distance, bool bothSides, double tolerance)
        {
            if (brep == null || !brep.IsValid)
                return null;
            var breps = new List<Brep>();
            foreach (var face in brep.Faces)
            {
                var thickened = Brep.CreateFromOffsetFace(face, distance, tolerance, bothSides, true);
                if (thickened != null)
                    breps.Add(thickened);
            }
            if (breps.Count == 0)
                return null;
            // 合并所有面为单一实体
            var joined = Brep.JoinBreps(breps, tolerance);
            if (joined == null || joined.Length == 0)
                return null;
            return joined[0];
        }

        // ================================================================
        // 五、辅助命令
        // ================================================================

        /// <summary>为开放多重曲面的平面开口加盖</summary>
        public static Brep CreateCap(Brep brep, double tolerance)
        {
            if (brep == null)
                return null;
            return CapPlanarHoles(brep.DuplicateBrep(), tolerance);
        }

        /// <summary>多个相交曲面自动裁剪并合并为闭合实体</summary>
        public static Brep CreateSolidFromBreps(IEnumerable<Brep> breps, double tolerance)
        {
            if (breps == null)
                return null;
            var list = breps.ToList();
            if (list.Count == 0)
                return null;
            var result = Brep.CreateSolid(list, tolerance);
            return result != null && result.Length > 0 ? result[0] : null;
        }
    }
}
