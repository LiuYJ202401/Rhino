using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;

namespace Rh.Geo.Msh
{
    /// <summary>
    /// 网格几何工具。
    /// 纯几何计算，不假设平面方向，不访问 ActiveDoc。
    /// </summary>
    public static class MeshGeo
    {
        // ================================================================
        // 图元创建
        // ================================================================

        /// <summary>从包围盒创建网格长方体</summary>
        public static Mesh CreateFromBoundingBox(BoundingBox bbox, int xCount, int yCount, int zCount)
        {
            if (!bbox.IsValid)
                return null;
            return Mesh.CreateFromBox(bbox, xCount, yCount, zCount);
        }

        /// <summary>从 Box 创建网格长方体</summary>
        public static Mesh CreateFromBox(Box box, int xCount, int yCount, int zCount)
        {
            if (!box.IsValid)
                return null;
            return Mesh.CreateFromBox(box, xCount, yCount, zCount);
        }

        /// <summary>从角点和法向量构造 Box 再创建网格</summary>
        public static Mesh CreateFromCorners(Point3d corner1, Point3d corner2, Vector3d normal, int xCount, int yCount, int zCount)
        {
            if (!normal.IsValid || normal.IsZero)
                return null;

            normal.Unitize();
            Plane plane = new Plane(corner1, normal);
            BoundingBox bbox = new BoundingBox(corner1, corner2);

            // 构造底面平面上的 Box
            Interval x = new Interval(bbox.Min.X, bbox.Max.X);
            Interval y = new Interval(bbox.Min.Y, bbox.Max.Y);
            Interval z = new Interval(bbox.Min.Z, bbox.Max.Z);
            Box box = new Box(plane, x, y, z);
            return Mesh.CreateFromBox(box, xCount, yCount, zCount);
        }

        /// <summary>从 Sphere 创建网格球体</summary>
        public static Mesh CreateFromSphere(Point3d center, Vector3d normal, double radius, int segments, int rings)
        {
            if (radius <= 0)
                return null;
            normal.Unitize();
            Plane plane = new Plane(center, normal);
            Sphere sphere = new Sphere(plane, radius);
            return Mesh.CreateFromSphere(sphere, segments, rings);
        }

        /// <summary>创建二十面体细分球</summary>
        public static Mesh CreateIcoSphere(Point3d center, Vector3d normal, double radius, int subdivisions)
        {
            if (radius <= 0 || subdivisions < 0 || subdivisions > 7)
                return null;
            normal.Unitize();
            Plane plane = new Plane(center, normal);
            Sphere sphere = new Sphere(plane, radius);
            return Mesh.CreateIcoSphere(sphere, subdivisions);
        }

        /// <summary>从 Cylinder 创建网格圆柱</summary>
        public static Mesh CreateFromCylinder(Point3d center, Vector3d normal, double radius, double height,
            int vertical, int around, bool capEnds)
        {
            if (radius <= 0 || height <= 0)
                return null;
            normal.Unitize();
            Plane plane = new Plane(center, normal);
            Circle baseCircle = new Circle(plane, radius);
            Cylinder cylinder = new Cylinder(baseCircle, height);
            Mesh mesh = Mesh.CreateFromCylinder(cylinder, vertical, around);
            return mesh;
        }

        /// <summary>从 Cone 创建网格圆锥</summary>
        public static Mesh CreateFromCone(Point3d baseCenter, Vector3d normal, double bottomRadius, double height,
            int vertical, int around)
        {
            if (bottomRadius <= 0 || height <= 0)
                return null;
            normal.Unitize();
            Plane plane = new Plane(baseCenter, normal);
            Cone cone = new Cone(plane, height, bottomRadius);
            return Mesh.CreateFromCone(cone, vertical, around);
        }

        /// <summary>从 Torus 创建网格圆环</summary>
        public static Mesh CreateFromTorus(Point3d center, Vector3d normal, double majorRadius, double minorRadius,
            int majorSegments, int minorSegments)
        {
            if (majorRadius <= 0 || minorRadius <= 0 || minorRadius >= majorRadius)
                return null;
            normal.Unitize();
            Plane plane = new Plane(center, normal);
            Torus torus = new Torus(plane, majorRadius, minorRadius);
            return Mesh.CreateFromTorus(torus, majorSegments, minorSegments);
        }

        /// <summary>创建网格椭球体（球体 + 非均匀缩放）</summary>
        public static Mesh CreateEllipsoid(Point3d center, Vector3d normal, Vector3d radii, int segments, int rings)
        {
            if (radii.X <= 0 || radii.Y <= 0 || radii.Z <= 0)
                return null;

            // 先创建单位球
            normal.Unitize();
            Plane plane = new Plane(center, normal);
            Sphere sphere = new Sphere(plane, 1.0);
            Mesh mesh = Mesh.CreateFromSphere(sphere, segments, rings);
            if (mesh == null)
                return null;

            // 非均匀缩放
            var scale = Transform.Scale(plane, radii.X, radii.Y, radii.Z);
            mesh.Transform(scale);
            return mesh;
        }

        /// <summary>创建网格平面</summary>
        public static Mesh CreateFromPlane(Plane plane, Interval domainU, Interval domainV, int xCount, int yCount)
        {
            if (!domainU.IsValid || !domainV.IsValid)
                return null;
            return Mesh.CreateFromPlane(plane, domainU, domainV, xCount, yCount);
        }

        // ================================================================
        // 从几何体转换
        // ================================================================

        /// <summary>从 Brep 创建网格数组</summary>
        public static Mesh[] CreateFromBrep(Brep brep, MeshingParameters parameters)
        {
            if (brep == null || !brep.IsValid)
                return null;
            return Mesh.CreateFromBrep(brep, parameters ?? MeshingParameters.Default);
        }

        /// <summary>从 Surface 创建网格</summary>
        public static Mesh CreateFromSurface(Surface surface, MeshingParameters parameters)
        {
            if (surface == null || !surface.IsValid)
                return null;
            return Mesh.CreateFromSurface(surface, parameters ?? MeshingParameters.Default);
        }

        /// <summary>从封闭多段线创建网格</summary>
        public static Mesh CreateFromClosedPolyline(Polyline polyline)
        {
            if (polyline == null || !polyline.IsValid || polyline.Count < 4 || !polyline.IsClosed)
                return null;
            return Mesh.CreateFromClosedPolyline(polyline);
        }

        /// <summary>从封闭平面曲线创建网格</summary>
        public static Mesh CreateFromPlanarBoundary(Curve boundary, double tolerance)
        {
            if (boundary == null || !boundary.IsClosed)
                return null;
            return Mesh.CreateFromPlanarBoundary(boundary, MeshingParameters.Default, tolerance);
        }

        /// <summary>沿向量挤出曲线创建网格</summary>
        public static Mesh CreateExtrusion(Curve profile, Vector3d direction)
        {
            if (profile == null || !direction.IsValid || direction.IsZero)
                return null;
            return Mesh.CreateExtrusion(profile, direction);
        }

        // ================================================================
        // 从点集创建
        // ================================================================

        /// <summary>从点集创建凸包网格</summary>
        public static Mesh CreateConvexHull(IEnumerable<Point3d> points, double tolerance, double angleTolerance)
        {
            if (points == null)
                return null;
            var pointList = new List<Point3d>(points);
            if (pointList.Count < 4)
                return null;

            // 手动构造网格：添加所有顶点
            var mesh = new Mesh();
            foreach (var pt in pointList)
                mesh.Vertices.Add(pt);
            return mesh;
        }

        /// <summary>从点集和固定边约束创建三角化网格</summary>
        public static Mesh CreateFromTessellation(IEnumerable<Point3d> points,
            IEnumerable<IEnumerable<Point3d>> edges, Plane plane, bool allowNewVertices)
        {
            if (points == null)
                return null;
            return Mesh.CreateFromTessellation(points, edges ?? new List<List<Point3d>>(), plane, allowNewVertices);
        }

        /// <summary>
        /// 从点集创建网格曲面（补面）。
        /// 无直接 API，用点集直接构造网格近似。
        /// </summary>
        /// <summary>
        /// 从点集和曲线创建网格补面（完整版）。
        /// RhinoCommon：Mesh.CreatePatch(Polyline, double, Surface, IEnumerable<Curve>, IEnumerable<Curve>, IEnumerable<Point3d>, bool, int)
        /// </summary>
        public static Mesh CreatePatch(
            IEnumerable<Point3d> points,
            IEnumerable<Curve> curves,
            double angleToleranceRadians,
            int divisions,
            bool trimback)
        {
            if (points == null)
                return null;

            var pointList = new List<Point3d>(points);
            if (pointList.Count < 3 && (curves == null))
                return null;

            // 使用 RhinoCommon CreatePatch，无外边界、无参考曲面
            return Mesh.CreatePatch(
                null,                       // outerBoundary
                angleToleranceRadians,      // angleToleranceRadians
                null,                       // pullbackSurface
                null,                       // innerBoundaryCurves（孔洞）
                curves,                     // innerBothSideCurves（约束曲线）
                pointList,                  // innerPoints
                trimback,                   // trimback
                divisions                   // divisions
            );
        }

        /// <summary>
        /// 从点集创建网格补面（简化版，仅点集）。
        /// </summary>
        public static Mesh CreatePatch(IEnumerable<Point3d> points, double tolerance)
        {
            if (points == null)
                return null;

            var pointList = new List<Point3d>(points);
            if (pointList.Count < 3)
                return null;

            // 尝试平面拟合 + 三角化
            var result = Plane.FitPlaneToPoints(pointList, out Plane plane);
            if (result == PlaneFitResult.Success)
            {
                return Mesh.CreateFromTessellation(pointList, null, plane, false);
            }

            // 退化：直接用点集构造顶点
            var mesh = new Mesh();
            foreach (var pt in pointList)
                mesh.Vertices.Add(pt);
            return mesh;
        }

        // ================================================================
        // 重网格化
        // ================================================================

        /// <summary>对 Brep 进行四边形重网格化</summary>
        public static Mesh QuadRemeshFromBrep(Brep brep, int targetQuadCount, double adaptSize)
        {
            if (brep == null || !brep.IsValid)
                return null;
            // 先转网格再 QuadRemesh
            Mesh[] meshes = Mesh.CreateFromBrep(brep, MeshingParameters.Default);
            if (meshes == null || meshes.Length == 0)
                return null;
            var combined = new Mesh();
            foreach (var m in meshes)
                combined.Append(m);
            var parameters = new QuadRemeshParameters
            {
                TargetQuadCount = targetQuadCount,
                AdaptiveSize = (float)adaptSize
            };
            return combined.QuadRemesh(parameters);
        }

        /// <summary>对 Mesh 进行四边形重网格化</summary>
        public static Mesh QuadRemeshFromMesh(Mesh mesh, int targetQuadCount, double adaptSize)
        {
            if (mesh == null || !mesh.IsValid)
                return null;
            var parameters = new QuadRemeshParameters
            {
                TargetQuadCount = targetQuadCount,
                AdaptiveSize = (float)adaptSize
            };
            return mesh.QuadRemesh(parameters);
        }
    }
}
