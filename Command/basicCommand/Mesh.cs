using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Rh.Data;
using Rh.Geo.Msh;

namespace Rh.Cmd
{
    /// <summary>
    /// 基础网格命令。
    /// 创建网格图元、从几何体/点集转换网格（不写入文档，返回几何对象）。
    /// 对应接口文档：Mesh.md
    ///
    /// 规则：
    ///   - 不直接与 RhinoDoc 交互（不调用 doc.Objects.AddXxx）
    ///   - 不获取用户输入
    ///   - 是唯一直接调用 DataReader 的层
    ///   - 同一功能统一方法名，通过参数类型区分重载
    /// </summary>
    public static class MeshCmd
    {
        // ================================================================
        // 内部常量与工具方法
        // ================================================================

        private const string DataPath = "Command/basicCommand/Mesh.json";

        private static void UpdateDefault<T>(string key, T value)
        {
            DataReader.SetValue<T>(DataPath, key, value);
        }

        private static T GetDefault<T>(string key, T defaultValue)
        {
            return DataReader.GetValue<T>(DataPath, key, defaultValue);
        }

        private static double ActiveTolerance()
        {
            return DataReader.GetDynamicValue("ModelAbsoluteTolerance");
        }

        private static double ActiveAngleTolerance()
        {
            return DataReader.GetDynamicValue("ModelAngleToleranceRadians");
        }

        // ================================================================
        // 默认值查询（供 Project 层初始化 UI）
        // ================================================================

        public static int GetDefaultMeshSegments()
        {
            return GetDefault("segments", 16);
        }

        public static int GetDefaultMeshRings()
        {
            return GetDefault("rings", 10);
        }

        public static int GetDefaultMeshVertical()
        {
            return GetDefault("vertical", 1);
        }

        public static int GetDefaultMeshAround()
        {
            return GetDefault("around", 16);
        }

        public static int GetDefaultMeshMajorSegments()
        {
            return GetDefault("majorSegments", 24);
        }

        public static int GetDefaultMeshMinorSegments()
        {
            return GetDefault("minorSegments", 12);
        }

        public static int GetDefaultMeshSubdivisions()
        {
            return GetDefault("subdivisions", 3);
        }

        public static int GetDefaultMeshBoxCount()
        {
            return GetDefault("boxCount", 1);
        }

        public static int GetDefaultMeshPlaneCount()
        {
            return GetDefault("planeCount", 1);
        }

        public static int GetDefaultQuadRemeshTarget()
        {
            return GetDefault("quadRemesh.targetQuadCount", 4000);
        }

        public static double GetDefaultQuadRemeshAdaptSize()
        {
            return GetDefault("quadRemesh.adaptSize", 0.0);
        }

        // ================================================================
        // 一、图元创建
        // ================================================================

        // ------------------------------------------------------------
        // CreateMeshBox
        // ------------------------------------------------------------

        /// <summary>创建网格长方体（包围盒方式）</summary>
        public static Mesh CreateMeshBox(BoundingBox bbox,
            int xCount, int yCount, int zCount, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("boxCount.x", xCount);
                UpdateDefault("boxCount.y", yCount);
                UpdateDefault("boxCount.z", zCount);
            }
            return MeshGeo.CreateFromBoundingBox(bbox, xCount, yCount, zCount);
        }

        /// <summary>创建网格长方体（角点 + 法向量方式）</summary>
        public static Mesh CreateMeshBox(Point3d corner1, Point3d corner2, Vector3d normal,
            int xCount, int yCount, int zCount, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("boxCount.x", xCount);
                UpdateDefault("boxCount.y", yCount);
                UpdateDefault("boxCount.z", zCount);
            }
            return MeshGeo.CreateFromCorners(corner1, corner2, normal, xCount, yCount, zCount);
        }

        // ------------------------------------------------------------
        // CreateMeshSphere
        // ------------------------------------------------------------

        /// <summary>创建网格球体（UV 方式）</summary>
        public static Mesh CreateMeshSphere(Point3d center, Vector3d normal, double radius,
            int segments, int rings, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateMeshSphere.radius", radius);
                UpdateDefault("segments", segments);
                UpdateDefault("rings", rings);
            }
            return MeshGeo.CreateFromSphere(center, normal, radius, segments, rings);
        }

        /// <summary>创建二十面体细分球</summary>
        public static Mesh CreateMeshSphere(Point3d center, Vector3d normal, double radius,
            int subdivisions, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateMeshSphere.radius", radius);
                UpdateDefault("subdivisions", subdivisions);
            }
            return MeshGeo.CreateIcoSphere(center, normal, radius, subdivisions);
        }

        // ------------------------------------------------------------
        // CreateMeshCylinder
        // ------------------------------------------------------------

        /// <summary>创建网格圆柱体</summary>
        public static Mesh CreateMeshCylinder(Point3d center, Vector3d normal, double radius, double height,
            int vertical, int around, bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateMeshCylinder.radius", radius);
                UpdateDefault("CreateMeshCylinder.height", height);
                UpdateDefault("vertical", vertical);
                UpdateDefault("around", around);
            }
            return MeshGeo.CreateFromCylinder(center, normal, radius, height, vertical, around, capEnds);
        }

        // ------------------------------------------------------------
        // CreateMeshCone
        // ------------------------------------------------------------

        /// <summary>创建网格圆锥体</summary>
        public static Mesh CreateMeshCone(Point3d baseCenter, Vector3d normal, double bottomRadius, double height,
            int vertical, int around, bool capEnd, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateMeshCone.bottomRadius", bottomRadius);
                UpdateDefault("CreateMeshCone.height", height);
                UpdateDefault("vertical", vertical);
                UpdateDefault("around", around);
            }
            return MeshGeo.CreateFromCone(baseCenter, normal, bottomRadius, height, vertical, around);
        }

        // ------------------------------------------------------------
        // CreateMeshTorus
        // ------------------------------------------------------------

        /// <summary>创建网格圆环</summary>
        public static Mesh CreateMeshTorus(Point3d center, Vector3d normal, double majorRadius, double minorRadius,
            int majorSegments, int minorSegments, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateMeshTorus.majorRadius", majorRadius);
                UpdateDefault("CreateMeshTorus.minorRadius", minorRadius);
                UpdateDefault("majorSegments", majorSegments);
                UpdateDefault("minorSegments", minorSegments);
            }
            return MeshGeo.CreateFromTorus(center, normal, majorRadius, minorRadius, majorSegments, minorSegments);
        }

        // ------------------------------------------------------------
        // CreateMeshEllipsoid
        // ------------------------------------------------------------

        /// <summary>创建网格椭球体</summary>
        public static Mesh CreateMeshEllipsoid(Point3d center, Vector3d normal, Vector3d radii,
            int segments, int rings, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateMeshEllipsoid.radiiX", radii.X);
                UpdateDefault("CreateMeshEllipsoid.radiiY", radii.Y);
                UpdateDefault("CreateMeshEllipsoid.radiiZ", radii.Z);
                UpdateDefault("segments", segments);
                UpdateDefault("rings", rings);
            }
            return MeshGeo.CreateEllipsoid(center, normal, radii, segments, rings);
        }

        // ------------------------------------------------------------
        // CreateMeshPlane
        // ------------------------------------------------------------

        /// <summary>创建网格平面</summary>
        public static Mesh CreateMeshPlane(Plane plane, Interval domainU, Interval domainV,
            int xCount, int yCount, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("planeCount.x", xCount);
                UpdateDefault("planeCount.y", yCount);
            }
            return MeshGeo.CreateFromPlane(plane, domainU, domainV, xCount, yCount);
        }

        // ================================================================
        // 二、从几何体转换
        // ================================================================

        /// <summary>将 Brep 转换为网格</summary>
        public static Mesh[] CreateMeshFromBrep(Brep brep, MeshingParameters parameters, bool isPreview = false)
        {
            return MeshGeo.CreateFromBrep(brep, parameters);
        }

        /// <summary>将 Surface 转换为网格</summary>
        public static Mesh CreateMeshFromSurface(Surface surface, MeshingParameters parameters, bool isPreview = false)
        {
            return MeshGeo.CreateFromSurface(surface, parameters);
        }

        /// <summary>从封闭多段线创建网格</summary>
        public static Mesh CreateMeshFromPolyline(Polyline polyline, bool isPreview = false)
        {
            return MeshGeo.CreateFromClosedPolyline(polyline);
        }

        /// <summary>从封闭平面曲线创建平面网格</summary>
        public static Mesh CreateMeshFromPlanarBoundary(Curve boundary, double tolerance, bool isPreview = false)
        {
            return MeshGeo.CreateFromPlanarBoundary(boundary, tolerance);
        }

        /// <summary>沿向量挤出曲线创建网格</summary>
        public static Mesh CreateMeshExtrusion(Curve profile, Vector3d direction, bool isPreview = false)
        {
            return MeshGeo.CreateExtrusion(profile, direction);
        }

        // ================================================================
        // 三、从点集创建
        // ================================================================

        /// <summary>从点集创建凸包网格</summary>
        public static Mesh CreateMeshFromPoints(IEnumerable<Point3d> points, double tolerance, bool isPreview = false)
        {
            return MeshGeo.CreateConvexHull(points, tolerance, RhinoMath.ToRadians(1.0));
        }

        /// <summary>从点集和固定边约束创建三角化网格</summary>
        public static Mesh CreateMeshFromTessellation(IEnumerable<Point3d> points,
            IEnumerable<IEnumerable<Point3d>> edges, Plane plane, bool allowNewVertices, bool isPreview = false)
        {
            return MeshGeo.CreateFromTessellation(points, edges, plane, allowNewVertices);
        }

        /// <summary>从点集创建网格曲面（补面）</summary>
        public static Mesh CreateMeshPatch(IEnumerable<Point3d> points, double tolerance, bool isPreview = false)
        {
            return MeshGeo.CreatePatch(points, tolerance);
        }

        // ================================================================
        // 四、重网格化
        // ================================================================

        /// <summary>对 Brep 进行四边形重网格化</summary>
        public static Mesh CreateQuadRemesh(Brep brep, int targetQuadCount, double adaptSize, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("quadRemesh.targetQuadCount", targetQuadCount);
                UpdateDefault("quadRemesh.adaptSize", adaptSize);
            }
            return MeshGeo.QuadRemeshFromBrep(brep, targetQuadCount, adaptSize);
        }

        /// <summary>对 Mesh 进行四边形重网格化</summary>
        public static Mesh CreateQuadRemesh(Mesh mesh, int targetQuadCount, double adaptSize, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("quadRemesh.targetQuadCount", targetQuadCount);
                UpdateDefault("quadRemesh.adaptSize", adaptSize);
            }
            return MeshGeo.QuadRemeshFromMesh(mesh, targetQuadCount, adaptSize);
        }
    }
}
