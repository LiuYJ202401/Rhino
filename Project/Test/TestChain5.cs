using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rh.Cmd;
using Rh.Project.Test.Framework;

namespace Rh.Project.Test
{
    /// <summary>
    /// 测试链 5：网格与转换
    /// 覆盖：网格图元（Box/Sphere/Cylinder/Cone/Torus/Ellipsoid/Plane）、
    ///       从几何体转换（Brep/Surface/Polyline/PlanarBoundary/Extrusion）、
    ///       从点集创建（凸包/细分/补面）、四边形重网格化
    /// 空间布局：X=500~620
    /// </summary>
    public class TestChain5Cmd : TestBase
    {
        public override string EnglishName => "RhTestChain5";

        // 链名常量
        private const string C = "Chain5";

        // 基础几何
        private Brep _box0a;        // 0a：长方体
        private Brep _sphere0b;     // 0b：球体
        private Brep _plane0c;      // 0c：平面
        private Polyline _poly0d;   // 0d：多段线

        // 衔接数据
        private Mesh _meshFromBrep10;   // 步骤10 → 步骤19

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine($"\n════════ 测试链 5：网格与转换 ════════\n");

            // ================================================================
            // 基础几何（X=500）
            // ================================================================

            // 0a：长方体 c1=(500,0,0) c2=(506,6,6)
            _box0a = SolidCmd.CreateBox(
                new Point3d(500,0,0), new Point3d(506,6,6), Vector3d.ZAxis);
            WriteToDoc(_box0a, C, "Base");

            // 0b：球体 @ (500,12,4) r=4
            _sphere0b = SolidCmd.CreateSphere(
                new Point3d(500,12,4), Vector3d.ZAxis, 4);
            WriteToDoc(_sphere0b, C, "Base");

            // 0c：平面 @(500,18,0) domU=(0~8) domV=(0~6)
            _plane0c = SurfaceCmd.CreatePlane(
                new Plane(new Point3d(500,18,0), Vector3d.ZAxis),
                new Interval(0, 8), new Interval(0, 6));
            WriteToDoc(_plane0c, C, "Base");

            // 0d：多段线 [(500,28,0),(506,28,0),(506,34,0),(500,34,0)]
            _poly0d = CurveCmd.CreatePolyline(new List<Point3d> {
                new Point3d(500,28,0), new Point3d(506,28,0),
                new Point3d(506,34,0), new Point3d(500,34,0)
            }, true);

            // ================================================================
            // 网格图元（X=520）
            // ================================================================

            Step("MeshCmd.CreateMeshBox(#1)", () =>
            {
                var bbox = new BoundingBox(
                    new Point3d(520,0,0), new Point3d(528,6,6));
                var mesh = MeshCmd.CreateMeshBox(bbox, 1, 1, 1);
                Assert.IsValid(mesh, "CreateMeshBox(#1)");
                WriteToDoc(mesh, C, "MeshPrim");
            });

            Step("MeshCmd.CreateMeshBox(#2)", () =>
            {
                var mesh = MeshCmd.CreateMeshBox(
                    new Point3d(520,12,0), new Point3d(528,18,6),
                    Vector3d.ZAxis, 1, 1, 1);
                Assert.IsValid(mesh, "CreateMeshBox(#2)");
                WriteToDoc(mesh, C, "MeshPrim");
            });

            Step("MeshCmd.CreateMeshSphere(#1)", () =>
            {
                var mesh = MeshCmd.CreateMeshSphere(
                    new Point3d(520,26,4), Vector3d.ZAxis, 4, 16, 12);
                Assert.IsValid(mesh, "CreateMeshSphere(#1)");
                WriteToDoc(mesh, C, "MeshPrim");
            });

            Step("MeshCmd.CreateMeshSphere(#2)", () =>
            {
                var mesh = MeshCmd.CreateMeshSphere(
                    new Point3d(520,38,4), Vector3d.ZAxis, 4, 2);
                Assert.IsValid(mesh, "CreateMeshSphere(#2)");
                WriteToDoc(mesh, C, "MeshPrim");
            });

            Step("MeshCmd.CreateMeshCylinder", () =>
            {
                var mesh = MeshCmd.CreateMeshCylinder(
                    new Point3d(520,48,0), Vector3d.ZAxis, 3, 8, 4, 12, true);
                Assert.IsValid(mesh, "CreateMeshCylinder");
                WriteToDoc(mesh, C, "MeshPrim");
            });

            Step("MeshCmd.CreateMeshCone", () =>
            {
                var mesh = MeshCmd.CreateMeshCone(
                    new Point3d(520,60,0), Vector3d.ZAxis, 3, 8, 4, 12, true);
                Assert.IsValid(mesh, "CreateMeshCone");
                WriteToDoc(mesh, C, "MeshPrim");
            });

            Step("MeshCmd.CreateMeshTorus", () =>
            {
                var mesh = MeshCmd.CreateMeshTorus(
                    new Point3d(520,72,0), Vector3d.ZAxis, 5, 1.5, 16, 8);
                Assert.IsValid(mesh, "CreateMeshTorus");
                WriteToDoc(mesh, C, "MeshPrim");
            });

            Step("MeshCmd.CreateMeshEllipsoid", () =>
            {
                var mesh = MeshCmd.CreateMeshEllipsoid(
                    new Point3d(520,84,0), Vector3d.ZAxis,
                    new Vector3d(5,3,2), 16, 12);
                Assert.IsValid(mesh, "CreateMeshEllipsoid");
                WriteToDoc(mesh, C, "MeshPrim");
            });

            Step("MeshCmd.CreateMeshPlane", () =>
            {
                var plane = new Plane(new Point3d(520,96,0), Vector3d.ZAxis);
                var mesh = MeshCmd.CreateMeshPlane(
                    plane, new Interval(0, 8), new Interval(0, 6), 4, 3);
                Assert.IsValid(mesh, "CreateMeshPlane");
                WriteToDoc(mesh, C, "MeshPrim");
            });

            // ================================================================
            // 从几何体转换（X=550）
            // ================================================================

            Step("MeshCmd.CreateMeshFromBrep", () =>
            {
                var meshes = MeshCmd.CreateMeshFromBrep(
                    _box0a, MeshingParameters.Default);
                Assert.GreaterThanZero(
                    meshes != null ? meshes.Length : 0, "CreateMeshFromBrep.Length");
                if (meshes != null && meshes.Length > 0)
                {
                    _meshFromBrep10 = meshes[0];
                    foreach (var m in meshes) WriteToDoc(m, C, "MeshFromGeo");
                }
            });

            Step("MeshCmd.CreateMeshFromSurface", () =>
            {
                var surface = _plane0c.Faces[0];
                var mesh = MeshCmd.CreateMeshFromSurface(
                    surface, MeshingParameters.Default);
                Assert.IsValid(mesh, "CreateMeshFromSurface");
                if (mesh != null) WriteToDoc(mesh, C, "MeshFromGeo");
            });

            Step("MeshCmd.CreateMeshFromPolyline", () =>
            {
                var mesh = MeshCmd.CreateMeshFromPolyline(_poly0d);
                Assert.IsValid(mesh, "CreateMeshFromPolyline");
                if (mesh != null) WriteToDoc(mesh, C, "MeshFromGeo");
            });

            Step("MeshCmd.CreateMeshFromPlanarBoundary", () =>
            {
                var boundary = _poly0d.ToPolylineCurve();
                var mesh = MeshCmd.CreateMeshFromPlanarBoundary(boundary, 0.001);
                Assert.IsValid(mesh, "CreateMeshFromPlanarBoundary");
                if (mesh != null) WriteToDoc(mesh, C, "MeshFromGeo");
            });

            Step("MeshCmd.CreateMeshExtrusion", () =>
            {
                // 临时圆作为截面
                var profile = new Circle(
                    new Plane(new Point3d(550,48,0), Vector3d.ZAxis), 3).ToNurbsCurve();
                var mesh = MeshCmd.CreateMeshExtrusion(
                    profile, new Vector3d(0,0,6));
                Assert.IsValid(mesh, "CreateMeshExtrusion");
                if (mesh != null) WriteToDoc(mesh, C, "MeshFromGeo");
            });

            // ================================================================
            // 从点集创建（X=575）
            // ================================================================

            Step("MeshCmd.CreateMeshFromPoints", () =>
            {
                // 非共面点：立方体 8 顶点 + 2 个外部点 = 10 个 3D 点
                // 凸包应该包裹所有点
                var pts = new List<Point3d> {
                    new Point3d(575,0,0), new Point3d(579,0,0),
                    new Point3d(575,4,0), new Point3d(579,4,0),
                    new Point3d(575,0,4), new Point3d(579,0,4),
                    new Point3d(575,4,4), new Point3d(579,4,4),
                    new Point3d(577,2,6),   // 顶部外部点
                    new Point3d(577,2,-2)   // 底部外部点
                };
                var mesh = MeshCmd.CreateMeshFromPoints(pts, 0.001);
                Assert.IsValid(mesh, "CreateMeshFromPoints");
                // 凸包必须有面（不是只有顶点）
                if (mesh != null)
                    Assert.GreaterThanZero(mesh.Faces.Count, "CreateMeshFromPoints.Faces");
                if (mesh != null) WriteToDoc(mesh, C, "MeshFromPts");
            });

            Step("MeshCmd.CreateMeshFromTessellation", () =>
            {
                // 5 个非共线点，无固定边约束 → Delaunay 三角化
                var pts = new List<Point3d> {
                    new Point3d(575,10,0), new Point3d(579,10,0),
                    new Point3d(575,14,0), new Point3d(579,14,0),
                    new Point3d(577,12,0)  // 中心点
                };
                var mesh = MeshCmd.CreateMeshFromTessellation(
                    pts, null, Plane.WorldXY, false);
                Assert.IsValid(mesh, "CreateMeshFromTessellation");
                if (mesh != null)
                    Assert.GreaterThanZero(mesh.Faces.Count, "CreateMeshFromTessellation.Faces");
                if (mesh != null) WriteToDoc(mesh, C, "MeshFromPts");
            });

            Step("MeshCmd.CreateMeshPatch", () =>
            {
                var pts = new List<Point3d> {
                    new Point3d(575,20,0), new Point3d(579,20,0),
                    new Point3d(575,24,0), new Point3d(579,24,0)
                };
                var mesh = MeshCmd.CreateMeshPatch(pts, 0.001);
                Assert.IsValid(mesh, "CreateMeshPatch");
                if (mesh != null) WriteToDoc(mesh, C, "MeshFromPts");
            });

            // ================================================================
            // 重网格化（X=600）
            // ================================================================

            Step("MeshCmd.CreateQuadRemesh(#1)", () =>
            {
                var mesh = MeshCmd.CreateQuadRemesh(_box0a, 50, 0.5);
                Assert.IsValid(mesh, "CreateQuadRemesh(#1)");
                if (mesh != null) WriteToDoc(mesh, C, "MeshRemesh");
            });

            Step("MeshCmd.CreateQuadRemesh(#2)", () =>
            {
                if (_meshFromBrep10 == null)
                {
                    Assert.IsValid(null, "CreateQuadRemesh(#2) 前置步骤无结果");
                    return;
                }
                var mesh = MeshCmd.CreateQuadRemesh(_meshFromBrep10, 50, 0.5);
                Assert.IsValid(mesh, "CreateQuadRemesh(#2)");
                if (mesh != null) WriteToDoc(mesh, C, "MeshRemesh");
            });

            Finish(C);
            return Result.Success;
        }
    }
}
