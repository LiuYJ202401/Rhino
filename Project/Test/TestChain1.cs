using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rh.Cmd;
using Rh.Project.Test.Framework;

namespace Rh.Project.Test
{
    /// <summary>
    /// 测试链 1：基础创建链（点→线→面→体）
    /// 覆盖：Point 全部 + Curve 基础 + Surface 从曲线 + Solid 基础
    /// 空间布局：X=0~100
    /// </summary>
    public class TestChain1Cmd : TestBase
    {
        public override string EnglishName => "RhTestChain1";

        // 链名常量
        private const string C = "Chain1";

        // 衔接数据
        private Polyline _poly12;       // 步骤12
        private Polyline _rect13;       // 步骤13 → 步骤42,49,55
        private Polyline _rect14;       // 步骤14
        private Circle _circle18;       // 步骤18 → 步骤43
        private Circle _circle19;       // 步骤19 → 步骤43
        private NurbsCurve _ellipse29;  // 步骤29 → 步骤46
        private Brep _plane35;          // 步骤35 → 步骤39,54

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine($"\n════════ 测试链 1：基础创建链 ════════\n");

            // ================================================================
            // 7.1 Point 子区（X=0）
            // ================================================================

            Step("PointCmd.CreatePoint(#1)", () =>
            {
                var pt = PointCmd.CreatePoint(new Point3d(0, 0, 0));
                Assert.NotNull(pt, "CreatePoint");
                WritePoint(pt, C, "Point");
            });

            Step("PointCmd.CreatePoint(#2)", () =>
            {
                var pt = PointCmd.CreatePoint(0, 3, 0);
                Assert.NotNull(pt, "CreatePoint");
                WritePoint(pt, C, "Point");
            });

            Step("PointCmd.CreatePoints", () =>
            {
                var pts = new List<Point3d> {
                    new Point3d(0,5,0), new Point3d(0,6,0),
                    new Point3d(0,7,0), new Point3d(0,8,0), new Point3d(0,9,0)
                };
                var result = PointCmd.CreatePoints(pts);
                Assert.Count(5, result.Count, "CreatePoints");
                WritePoints(result, C, "Point");
            });

            Step("PointCmd.CreatePointGrid", () =>
            {
                var cloud = PointCmd.CreatePointGrid(
                    Plane.WorldXY, 3, 3,
                    new Interval(0, 6), new Interval(12, 18));
                Assert.NotNull(cloud, "CreatePointGrid");
                if (cloud != null) WriteToDoc(cloud, C, "Point");
            });

            PointCloud cloud5 = null;
            Step("PointCmd.CreatePointCloud", () =>
            {
                var pts = new List<Point3d> {
                    new Point3d(0,22,0), new Point3d(0,23,0),
                    new Point3d(0,24,0), new Point3d(0,25,0)
                };
                cloud5 = PointCmd.CreatePointCloud(pts);
                Assert.NotNull(cloud5, "CreatePointCloud");
                if (cloud5 != null) WriteToDoc(cloud5, C, "Point");
            });

            Step("PointCmd.CreatePointCloudFromMesh", () =>
            {
                // 临时 Box 网格
                var tempMesh = MeshCmd.CreateMeshBox(
                    new BoundingBox(new Point3d(0, 30, 0), new Point3d(2, 32, 2)),
                    1, 1, 1);
                var cloud = PointCmd.CreatePointCloudFromMesh(tempMesh);
                Assert.NotNull(cloud, "CreatePointCloudFromMesh");
                if (cloud != null) WriteToDoc(cloud, C, "Point");
            });

            Step("PointCmd.AddPointsToCloud", () =>
            {
                // AddPointsToCloud 返回新点云（原点云不变），必须赋值回 cloud5
                var addPts = new List<Point3d> { new Point3d(1, 22, 0), new Point3d(1, 23, 0) };
                cloud5 = PointCmd.AddPointsToCloud(cloud5, addPts);
                Assert.Count(6, cloud5.Count, "AddPointsToCloud");
            });

            Step("PointCmd.RemovePointsFromCloud", () =>
            {
                // cloud5 现在是 6 个点（步骤7已赋值回），移除索引0,1 → 剩余4个
                cloud5 = PointCmd.RemovePointsFromCloud(cloud5, new List<int> { 0, 1 });
                Assert.Count(4, cloud5.Count, "RemovePointsFromCloud");
            });

            Step("PointCmd.ReducePointCloud", () =>
            {
                // cloud5 现在是 4 个点，减少 1 个 → 剩余3个
                cloud5 = PointCmd.ReducePointCloud(cloud5, 1);
                Assert.Count(3, cloud5.Count, "ReducePointCloud");
            });

            // ================================================================
            // 7.2 Line/Poly 子区（X=15）
            // ================================================================

            Step("CurveCmd.CreateLine(#1)", () =>
            {
                var line = CurveCmd.CreateLine(new Point3d(15, 0, 0), new Point3d(15, 8, 0));
                Assert.AreEqual(8.0, line.Length, "Line.Length");
                WriteToDoc(line.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateLine(#2)", () =>
            {
                var pts = new List<Point3d> {
                    new Point3d(15,12,0), new Point3d(15,15,3),
                    new Point3d(15,18,0), new Point3d(15,21,0)
                };
                var crv = CurveCmd.CreateLine(pts).ToNurbsCurve();
                Assert.IsValid(crv, "CreateLine(#2)");
                WriteToDoc(crv, C, "Curve");
            });

            Step("CurveCmd.CreatePolyline", () =>
            {
                var pts = new List<Point3d> {
                    new Point3d(15,25,0), new Point3d(15,32,0),
                    new Point3d(15,32,5), new Point3d(15,25,5)
                };
                _poly12 = CurveCmd.CreatePolyline(pts, true);
                Assert.NotNull(_poly12, "CreatePolyline");
                WriteToDoc(_poly12.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateRectangle(#1)", () =>
            {
                // 测试 YZ 平面上的矩形（X=15）
                var yzPlane = new Plane(new Point3d(15, 36, 0), Vector3d.XAxis);
                _rect13 = CurveCmd.CreateRectangle(
                    yzPlane, new Point3d(15, 36, 0), new Point3d(15, 44, 6));
                Assert.NotNull(_rect13, "CreateRectangle(#1)");
                WriteToDoc(_rect13.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateRectangle(#2)", () =>
            {
                _rect14 = CurveCmd.CreateRectangle(
                    Plane.WorldXY, new Point3d(15, 50, 0), 8, 6);
                Assert.NotNull(_rect14, "CreateRectangle(#2)");
                WriteToDoc(_rect14.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreatePolygon(#1)", () =>
            {
                // 测试 XZ 平面上的多边形
                var xzPlane = new Plane(new Point3d(15, 64, 0), Vector3d.YAxis);
                var poly = CurveCmd.CreatePolygon(xzPlane, new Point3d(15, 64, 0), 6, 4);
                Assert.NotNull(poly, "CreatePolygon(#1)");
                WriteToDoc(poly.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreatePolygon(#2)", () =>
            {
                var poly = CurveCmd.CreatePolygon(
                    Plane.WorldXY, new Point3d(15, 72, 0), new Point3d(15, 76, 0), 5);
                Assert.NotNull(poly, "CreatePolygon(#2)");
                WriteToDoc(poly.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreatePolygon(#3)", () =>
            {
                var poly = CurveCmd.CreatePolygon(
                    Plane.WorldXY, new Point3d(15, 84, 0), 5, 5, 2);
                Assert.NotNull(poly, "CreatePolygon(#3)");
                WriteToDoc(poly.ToNurbsCurve(), C, "Curve");
            });

            // ================================================================
            // 7.3 Circle/Arc/Conic 子区（X=35）
            // ================================================================

            Step("CurveCmd.CreateCircle(#1)", () =>
            {
                // 测试 YZ 平面上的圆
                var yzPlane = new Plane(new Point3d(35, 0, 0), Vector3d.XAxis);
                _circle18 = CurveCmd.CreateCircle(yzPlane, new Point3d(35, 0, 0), 4);
                Assert.AreEqual(4.0, _circle18.Radius, "Circle.Radius");
                WriteToDoc(_circle18.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateCircle(#2)", () =>
            {
                _circle19 = CurveCmd.CreateCircle(new Point3d(35, 10, 0), Vector3d.ZAxis, 4);
                Assert.IsTrue(_circle19.IsValid, "CreateCircle(#2)");
                WriteToDoc(_circle19.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateCircle(#3)", () =>
            {
                var circle = CurveCmd.CreateCircle(
                    new Plane(new Point3d(35, 20, 0), Vector3d.ZAxis), 4);
                Assert.AreEqual(4.0, circle.Radius, "Circle(#3).Radius");
                WriteToDoc(circle.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateCircle(#4)", () =>
            {
                var circle = CurveCmd.CreateCircle(
                    Plane.WorldXY, new Point3d(35, 26, 0), new Point3d(35, 34, 0));
                Assert.IsTrue(circle.IsValid, "CreateCircle(#4)");
                WriteToDoc(circle.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateCircle(#5)", () =>
            {
                var circle = CurveCmd.CreateCircle(
                    new Point3d(31, 40, 0), new Point3d(35, 44, 0), new Point3d(39, 40, 0));
                Assert.IsTrue(circle.IsValid, "CreateCircle(#5)");
                WriteToDoc(circle.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateCircle(#6)", () =>
            {
                var circle = CurveCmd.CreateCircle(
                    new Point3d(35, 48, 0), new Vector3d(1, 0, 0), new Point3d(35, 56, 0));
                Assert.IsTrue(circle.IsValid, "CreateCircle(#6)");
                WriteToDoc(circle.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateCircle(#7)", () =>
            {
                // 场景 A：L 形垂直相交线段，交点处创建相切圆角圆（CreateFillet 路径）
                var c1 = new LineCurve(new Point3d(28, 60, 0), new Point3d(42, 60, 0));
                var c2 = new LineCurve(new Point3d(35, 53, 0), new Point3d(35, 67, 0));
                var circles = CurveCmd.CreateCircle(c1, c2, 3, Tol);
                Assert.IsTrue(circles != null && circles.Length > 0, "CreateCircle(#7) 场景A:L形相交");
                if (circles != null)
                    foreach (var c in circles) WriteToDoc(c.ToNurbsCurve(), C, "Curve");

                // 场景 B：平行线，半径被强制为 d/2（FilletGeo 半圆分支）
                // 间距 d=6，半径 r=3（传入 r 在平行线场景被忽略，实际使用 d/2）
                var p1 = new LineCurve(new Point3d(28, 62, 0), new Point3d(42, 62, 0));
                var p2 = new LineCurve(new Point3d(28, 68, 0), new Point3d(42, 68, 0));
                var circlesB = CurveCmd.CreateCircle(p1, p2, 3, Tol);
                Assert.IsTrue(circlesB != null && circlesB.Length > 0, "CreateCircle(#7) 场景B:平行线半圆");
                if (circlesB != null)
                    foreach (var c in circlesB) WriteToDoc(c.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateArc(#1)", () =>
            {
                var arc = CurveCmd.CreateArc(
                    Plane.WorldXY, new Point3d(35, 72, 0), 4, 0, Math.PI);
                Assert.IsTrue(arc.IsValid, "CreateArc(#1)");
                WriteToDoc(arc.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateArc(#2)", () =>
            {
                var arc = CurveCmd.CreateArc(
                    new Point3d(31, 80, 0), new Point3d(35, 84, 0), new Point3d(39, 80, 0));
                Assert.IsTrue(arc.IsValid, "CreateArc(#2)");
                WriteToDoc(arc.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateArc(#3)", () =>
            {
                var arc = CurveCmd.CreateArc(
                    new Point3d(31, 90, 0), new Point3d(39, 90, 0), new Vector3d(0, 1, 0));
                Assert.IsTrue(arc.IsValid, "CreateArc(#3)");
                WriteToDoc(arc.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateArc(#4)", () =>
            {
                // 场景 A：L 形垂直相交线段，交点处创建相切圆角弧（CreateFillet 路径）
                var c1 = new LineCurve(new Point3d(28, 98, 0), new Point3d(42, 98, 0));
                var c2 = new LineCurve(new Point3d(35, 91, 0), new Point3d(35, 105, 0));
                var arcs = CurveCmd.CreateArc(c1, c2, 3, Tol);
                Assert.IsTrue(arcs != null && arcs.Length > 0, "CreateArc(#4) 场景A:L形相交");
                if (arcs != null)
                    foreach (var a in arcs) WriteToDoc(a.ToNurbsCurve(), C, "Curve");

                // 场景 B：平行线，半径被强制为 d/2（FilletGeo 半圆分支）
                // 间距 d=6，半径 r=3（传入 r 在平行线场景被忽略，实际使用 d/2）
                var p1 = new LineCurve(new Point3d(28, 100, 0), new Point3d(42, 100, 0));
                var p2 = new LineCurve(new Point3d(28, 106, 0), new Point3d(42, 106, 0));
                var arcsB = CurveCmd.CreateArc(p1, p2, 3, Tol);
                Assert.IsTrue(arcsB != null && arcsB.Length > 0, "CreateArc(#4) 场景B:平行线半圆");
                if (arcsB != null)
                    foreach (var a in arcsB) WriteToDoc(a.ToNurbsCurve(), C, "Curve");
            });

            Step("CurveCmd.CreateEllipse(#1)", () =>
            {
                // 测试 XZ 平面上的椭圆
                var xzPlane = new Plane(new Point3d(35, 112, 0), Vector3d.YAxis);
                _ellipse29 = CurveCmd.CreateEllipse(
                    xzPlane, new Point3d(35, 112, 0), 5, 3);
                Assert.IsValid(_ellipse29, "CreateEllipse(#1)");
                WriteToDoc(_ellipse29, C, "Curve");
            });

            Step("CurveCmd.CreateEllipse(#2)", () =>
            {
                var crv = CurveCmd.CreateEllipse(
                    Plane.WorldXY, new Point3d(31, 122, 0), new Point3d(39, 122, 0), 3);
                Assert.IsValid(crv, "CreateEllipse(#2)");
                WriteToDoc(crv, C, "Curve");
            });

            Step("CurveCmd.CreateEllipse(#3)", () =>
            {
                var crv = CurveCmd.CreateEllipse(
                    new Point3d(33, 132, 0), new Point3d(37, 132, 0), new Point3d(35, 135, 0));
                Assert.IsValid(crv, "CreateEllipse(#3)");
                WriteToDoc(crv, C, "Curve");
            });

            Step("CurveCmd.CreateParabola", () =>
            {
                var crv = CurveCmd.CreateParabola(
                    new Point3d(31, 142, 0), new Point3d(35, 146, 0), new Point3d(39, 142, 0));
                Assert.IsValid(crv, "CreateParabola");
                WriteToDoc(crv, C, "Curve");
            });

            Step("CurveCmd.CreateHyperbola", () =>
            {
                var crv = CurveCmd.CreateHyperbola(
                    new Point3d(35, 154, 0), new Point3d(35, 156, 0), new Point3d(39, 158, 0));
                Assert.IsValid(crv, "CreateHyperbola");
                WriteToDoc(crv, C, "Curve");
            });

            Step("CurveCmd.CreateConic", () =>
            {
                var crv = CurveCmd.CreateConic(
                    new Point3d(31, 166, 0), new Point3d(39, 166, 0),
                    new Point3d(35, 170, 0), 0.5);
                Assert.IsValid(crv, "CreateConic");
                WriteToDoc(crv, C, "Curve");
            });

            // ================================================================
            // 7.4 Surface 子区（X=60）
            // ================================================================

            Step("SurfaceCmd.CreatePlane(#1)", () =>
            {
                var plane = new Plane(new Point3d(60, 0, 0), Vector3d.ZAxis);
                _plane35 = SurfaceCmd.CreatePlane(plane, new Interval(0, 8), new Interval(0, 6));
                Assert.IsValid(_plane35, "CreatePlane(#1)");
                WriteToDoc(_plane35, C, "Surface");
            });

            Step("SurfaceCmd.CreatePlane(#2)", () =>
            {
                var brep = SurfaceCmd.CreatePlane(
                    new Point3d(60, 12, 0), new Point3d(68, 12, 0), new Point3d(60, 18, 6));
                Assert.IsValid(brep, "CreatePlane(#2)");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreatePlane(#3)", () =>
            {
                var brep = SurfaceCmd.CreatePlane(
                    new Point3d(60, 24, 0), new Point3d(60, 32, 0), 6, Vector3d.ZAxis);
                Assert.IsValid(brep, "CreatePlane(#3)");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreatePlaneThroughPt", () =>
            {
                var pts = new List<Point3d> {
                    new Point3d(60,36,0), new Point3d(64,36,0),
                    new Point3d(60,40,0), new Point3d(64,40,0),
                    new Point3d(62,38,2)
                };
                var brep = SurfaceCmd.CreatePlaneThroughPt(pts);
                Assert.IsValid(brep, "CreatePlaneThroughPt");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateCutPlane", () =>
            {
                var cutPlane = new Plane(new Point3d(60, 46, 0), Vector3d.ZAxis);
                var brep = SurfaceCmd.CreateCutPlane(cutPlane, new GeometryBase[] { _plane35 });
                Assert.IsValid(brep, "CreateCutPlane");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateSrfPt", () =>
            {
                var brep = SurfaceCmd.CreateSrfPt(
                    new Point3d(60, 52, 0), new Point3d(66, 52, 0),
                    new Point3d(66, 58, 4), new Point3d(60, 58, 4));
                Assert.IsValid(brep, "CreateSrfPt");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateEdgeSrf", () =>
            {
                // 用4条边构成矩形边界，创建边缘曲面
                var p0 = new Point3d(60, 38, 0);
                var p1 = new Point3d(68, 38, 0);
                var p2 = new Point3d(68, 44, 0);
                var p3 = new Point3d(60, 44, 0);
                var edges = new Curve[] {
                    new LineCurve(p0, p1).ToNurbsCurve(),
                    new LineCurve(p1, p2).ToNurbsCurve(),
                    new LineCurve(p2, p3).ToNurbsCurve(),
                    new LineCurve(p3, p0).ToNurbsCurve()
                };
                var brep = SurfaceCmd.CreateEdgeSrf(edges);
                Assert.IsValid(brep, "CreateEdgeSrf");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreatePlanarSrf", () =>
            {
                var breps = SurfaceCmd.CreatePlanarSrf(new Curve[] { _rect13.ToNurbsCurve() });
                Assert.IsTrue(breps != null && breps.Length > 0, "CreatePlanarSrf");
                if (breps != null) foreach (var b in breps) WriteToDoc(b, C, "Surface");
            });

            Step("SurfaceCmd.CreateLoft", () =>
            {
                var curves = new Curve[] { _circle18.ToNurbsCurve(), _circle19.ToNurbsCurve() };
                var breps = SurfaceCmd.CreateLoft(curves);
                Assert.IsTrue(breps != null && breps.Length > 0, "CreateLoft");
                if (breps != null) foreach (var b in breps) WriteToDoc(b, C, "Surface");
            });

            Step("SurfaceCmd.CreateSweep(#1)", () =>
            {
                var rail = new LineCurve(
                    new Point3d(60, 88, 0), new Point3d(60, 96, 6)).ToNurbsCurve();
                var shapes = new Curve[] {
                    new Circle(new Plane(new Point3d(60, 88, 0), Vector3d.ZAxis), 2).ToNurbsCurve()
                };
                var breps = SurfaceCmd.CreateSweep(rail, shapes);
                Assert.IsTrue(breps != null && breps.Length > 0, "CreateSweep(#1)");
                if (breps != null) foreach (var b in breps) WriteToDoc(b, C, "Surface");
            });

            Step("SurfaceCmd.CreateSweep(#2)", () =>
            {
                var rail1 = new LineCurve(
                    new Point3d(60, 100, 0), new Point3d(60, 108, 6)).ToNurbsCurve();
                var rail2 = new LineCurve(
                    new Point3d(64, 100, 0), new Point3d(64, 108, 6)).ToNurbsCurve();
                var shapes = new Curve[] {
                    new Circle(new Plane(new Point3d(62, 100, 0), Vector3d.ZAxis), 2).ToNurbsCurve()
                };
                var breps = SurfaceCmd.CreateSweep(rail1, rail2, shapes);
                Assert.IsTrue(breps != null && breps.Length > 0, "CreateSweep(#2)");
                if (breps != null) foreach (var b in breps) WriteToDoc(b, C, "Surface");
            });

            Step("SurfaceCmd.CreateRevolve(#1)", () =>
            {
                var axis = new Line(new Point3d(40, 112, 0), new Point3d(40, 112, 10));
                var brep = SurfaceCmd.CreateRevolve(_ellipse29, axis, false);
                Assert.IsValid(brep, "CreateRevolve(#1)");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateRevolve(#2)", () =>
            {
                var tempArc = CurveCmd.CreateArc(
                    new Plane(new Point3d(60, 126, 0), Vector3d.ZAxis),
                    new Point3d(60, 126, 0), 3, 0, Math.PI / 2);
                var axis = new Line(new Point3d(60, 126, 0), new Point3d(60, 126, 10));
                var brep = SurfaceCmd.CreateRevolve(tempArc.ToNurbsCurve(), axis, 0, Math.PI / 2);
                Assert.IsValid(brep, "CreateRevolve(#2)");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateRailRevolve", () =>
            {
                var profile = new LineCurve(
                    new Point3d(60, 140, 0), new Point3d(60, 146, 4)).ToNurbsCurve();
                var rail = CurveCmd.CreateArc(
                    new Plane(new Point3d(60, 146, 0), Vector3d.ZAxis),
                    new Point3d(60, 146, 0), 4, 0, Math.PI / 2).ToNurbsCurve();
                var axis = new Line(new Point3d(60, 146, 0), new Point3d(60, 146, 10));
                var brep = SurfaceCmd.CreateRailRevolve(profile, rail, axis);
                Assert.IsValid(brep, "CreateRailRevolve");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateExtrude(#1)", () =>
            {
                var brep = SurfaceCmd.CreateExtrude(
                    _rect13.ToNurbsCurve(), new Vector3d(0, 0, 6));
                Assert.IsValid(brep, "CreateExtrude(#1)");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateExtrude(#2)", () =>
            {
                var profile = new Circle(
                    new Plane(new Point3d(60, 172, 0), Vector3d.ZAxis), 3).ToNurbsCurve();
                var path = new LineCurve(
                    new Point3d(60, 172, 0), new Point3d(62, 174, 4)).ToNurbsCurve();
                var brep = SurfaceCmd.CreateExtrude(profile, path, false);
                Assert.IsValid(brep, "CreateExtrude(#2)");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateExtrude(#3)", () =>
            {
                var profile = CurveCmd.CreateRectangle(
                    Plane.WorldXY, new Point3d(60, 180, 0), 6, 4).ToNurbsCurve();
                var brep = SurfaceCmd.CreateExtrude(
                    profile, Vector3d.ZAxis, 6, RhinoMath.ToRadians(5));
                Assert.IsValid(brep, "CreateExtrude(#3)");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateExtrude(#4)", () =>
            {
                var profile = new Circle(
                    new Plane(new Point3d(60, 196, 0), Vector3d.ZAxis), 3).ToNurbsCurve();
                var brep = SurfaceCmd.CreateExtrude(profile, new Point3d(60, 196, 8));
                Assert.IsValid(brep, "CreateExtrude(#4)");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateRibbon", () =>
            {
                var curve = CurveCmd.CreateArc(
                    new Plane(new Point3d(60, 210, 0), Vector3d.ZAxis),
                    new Point3d(60, 210, 0), 4, 0, Math.PI).ToNurbsCurve();
                var brep = SurfaceCmd.CreateRibbon(curve, 3, Plane.WorldXY);
                Assert.IsValid(brep, "CreateRibbon");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateFin", () =>
            {
                var curve = new LineCurve(
                    new Point3d(60, 222, 0), new Point3d(66, 222, 0)).ToNurbsCurve();
                var brep = SurfaceCmd.CreateFin(curve, _plane35.Faces[0], 5);
                Assert.IsValid(brep, "CreateFin");
                WriteToDoc(brep, C, "Surface");
            });

            // ================================================================
            // 7.5 Solid 子区（X=90）
            // ================================================================

            Step("SolidCmd.CreateExtrudeSolid", () =>
            {
                // YZ 平面闭合矩形，沿 X 方向挤出（X 垂直于 YZ 平面）
                // 测试非 XY 平面的挤出实体创建
                var yzPlane = new Plane(new Point3d(82, 8, 0), Vector3d.XAxis);
                var profile = CurveCmd.CreateRectangle(
                    yzPlane, new Point3d(82, 8, 0), new Point3d(82, 16, 6));
                var brep = SolidCmd.CreateExtrudeSolid(
                    profile.ToNurbsCurve(), new Vector3d(6, 0, 0), true);
                Assert.IsTrue(brep != null && brep.IsSolid, "CreateExtrudeSolid");
                WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateBox(#1)", () =>
            {
                var box = new Box(Plane.WorldXY,
                    new Interval(90, 98), new Interval(8, 16), new Interval(0, 6));
                var brep = SolidCmd.CreateBox(box);
                Assert.IsTrue(brep.IsSolid, "CreateBox(#1)");
                WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateBox(#2)", () =>
            {
                var brep = SolidCmd.CreateBox(
                    new Point3d(90, 22, 0), new Point3d(98, 30, 0), new Vector3d(0, 0, 6));
                Assert.IsTrue(brep.IsSolid, "CreateBox(#2)");
                WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateSphere", () =>
            {
                var brep = SolidCmd.CreateSphere(
                    new Point3d(90, 38, 4), Vector3d.ZAxis, 4);
                Assert.IsTrue(brep.IsSolid, "CreateSphere");
                WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateCylinder", () =>
            {
                var brep = SolidCmd.CreateCylinder(
                    new Point3d(90, 50, 0), Vector3d.ZAxis, 3, 8, true);
                Assert.IsTrue(brep.IsSolid, "CreateCylinder");
                WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateCone", () =>
            {
                var brep = SolidCmd.CreateCone(
                    new Point3d(90, 62, 0), Vector3d.ZAxis, 3, 8, true);
                Assert.IsTrue(brep.IsSolid, "CreateCone");
                WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateTruncatedCone", () =>
            {
                var brep = SolidCmd.CreateTruncatedCone(
                    new Point3d(90, 74, 0), Vector3d.ZAxis, 3, 1.5, 8, true);
                Assert.IsTrue(brep.IsSolid, "CreateTruncatedCone");
                WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreatePyramid", () =>
            {
                var brep = SolidCmd.CreatePyramid(
                    new Point3d(90, 86, 0), Vector3d.ZAxis, 5, 3, 8, true);
                Assert.NotNull(brep, "CreatePyramid");
                Assert.IsTrue(brep.IsSolid, "CreatePyramid.IsSolid");
                WriteToDoc(brep, C, "Solid");
            });

            Finish(C);
            return Result.Success;
        }
    }
}
