using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rh.Cmd;
using Rh.Project.Test.Framework;

namespace Rh.Project.Test
{
    /// <summary>
    /// 测试链 6：Solid 专属
    /// 覆盖：管道、楼板、加厚、封口、布尔并集、圆环、椭球、管、棱台、
    ///       放样实体、旋转实体、扫掠实体
    /// 空间布局：X=640~680
    /// </summary>
    public class TestChain6Cmd : TestBase
    {
        public override string EnglishName => "RhTestChain6";

        // 链名常量
        private const string C = "Chain6";

        // 基础几何
        private Curve _line0a;          // 0a：直线
        private Polyline _poly0c;       // 0c：封闭多段线
        private Brep _loft0d;           // 0d：放样曲面
        private Brep _extrude0e;        // 0e：未封闭挤出

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine($"\n════════ 测试链 6：Solid 专属 ════════\n");

            // ================================================================
            // 基础几何（X=640）
            // ================================================================

            // 0a：直线 (640,0,0)-(640,0,12)
            var line = CurveCmd.CreateLine(
                new Point3d(640,0,0), new Point3d(640,0,12));
            _line0a = new LineCurve(line).ToNurbsCurve();
            WriteToDoc(_line0a, C, "Base");

            // 0b：圆弧 plane=WorldXY center=(640,15,0) r=5 startAng=0 endAng=π
            var arc0b = CurveCmd.CreateArc(
                Plane.WorldXY, new Point3d(640,15,0), 5, 0, Math.PI);
            WriteToDoc(arc0b.ToNurbsCurve(), C, "Base");

            // 0c：封闭多段线 [(640,25,0),(648,25,0),(648,33,0),(640,33,0)]
            _poly0c = CurveCmd.CreatePolyline(new List<Point3d> {
                new Point3d(640,25,0), new Point3d(648,25,0),
                new Point3d(648,33,0), new Point3d(640,33,0)
            }, true);
            WriteToDoc(_poly0c.ToNurbsCurve(), C, "Base");

            // 0d：放样 [圆A(640,40,0) r=3, 圆B(640,40,5) r=2]
            var circleA = new Circle(
                new Plane(new Point3d(640,40,0), Vector3d.ZAxis), 3);
            var circleB = new Circle(
                new Plane(new Point3d(640,40,5), Vector3d.ZAxis), 2);
            var loftCurves = new Curve[] {
                circleA.ToNurbsCurve(), circleB.ToNurbsCurve()
            };
            var loftBreps = SurfaceCmd.CreateLoft(loftCurves);
            if (loftBreps != null && loftBreps.Length > 0)
            {
                _loft0d = loftBreps[0];
                WriteToDoc(_loft0d, C, "Base");
            }

            // 0e：挤出临时矩形 dir=(0,0,4) cap=false（未封闭）
            var tempRect = CurveCmd.CreateRectangle(
                Plane.WorldXY, new Point3d(640,50,0), 6, 4);
            _extrude0e = SurfaceCmd.CreateExtrude(
                tempRect.ToNurbsCurve(), new Vector3d(0,0,4));
            WriteToDoc(_extrude0e, C, "Base");

            // ================================================================
            // Solid 专属操作（X=660）
            // ================================================================

            Step("SolidCmd.CreatePipe(#1)", () =>
            {
                var brep = SolidCmd.CreatePipe(_line0a, 1.5, 1);
                Assert.IsTrue(brep != null && brep.IsSolid, "CreatePipe(#1)");
                if (brep != null) WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreatePipe(#2)", () =>
            {
                var rail = _line0a.DuplicateCurve();
                var brep = SolidCmd.CreatePipe(rail, 1, 2, 2);
                Assert.IsTrue(brep != null && brep.IsSolid, "CreatePipe(#2)");
                if (brep != null) WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateSlab", () =>
            {
                var profile = _poly0c.ToPolylineCurve();
                var brep = SolidCmd.CreateSlab(
                    profile, 2, new Vector3d(0,0,1), true);
                Assert.IsTrue(brep != null && brep.IsSolid, "CreateSlab");
                if (brep != null) WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateTextObject", () =>
            {
                var breps = SolidCmd.CreateTextObject(
                    "Test", new Plane(new Point3d(660,36,0), Vector3d.ZAxis),
                    4, 1, "Arial", false, false);
                Assert.GreaterThanZero(
                    breps != null ? breps.Length : 0, "CreateTextObject");
                if (breps != null)
                    foreach (var b in breps) WriteToDoc(b, C, "Solid");
            });

            Step("SolidCmd.CreateThicken", () =>
            {
                if (_loft0d == null)
                {
                    Assert.IsTrue(false, "CreateThicken 前置步骤无结果");
                    return;
                }
                var brep = SolidCmd.CreateThicken(_loft0d, 1, false);
                Assert.IsTrue(brep != null && brep.IsSolid, "CreateThicken");
                if (brep != null) WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateCap", () =>
            {
                var brep = SolidCmd.CreateCap(_extrude0e);
                Assert.IsTrue(brep != null && brep.IsSolid, "CreateCap");
                if (brep != null) WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateSolidFromBreps", () =>
            {
                // 两个相交的长方体
                var boxA = SolidCmd.CreateBox(
                    new Point3d(660,72,0), new Point3d(670,82,8), Vector3d.ZAxis);
                var boxB = SolidCmd.CreateBox(
                    new Point3d(665,74,0), new Point3d(675,84,8), Vector3d.ZAxis);
                var breps = new Brep[] { boxA, boxB };
                var result = SolidCmd.CreateSolidFromBreps(breps);
                Assert.IsTrue(result != null && result.IsSolid, "CreateSolidFromBreps");
                if (result != null) WriteToDoc(result, C, "Solid");
            });

            Step("SolidCmd.CreateTorus", () =>
            {
                var brep = SolidCmd.CreateTorus(
                    new Point3d(660,84,0), Vector3d.ZAxis, 5, 1.5);
                Assert.IsTrue(brep != null && brep.IsSolid, "CreateTorus");
                if (brep != null) WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateEllipsoid", () =>
            {
                var brep = SolidCmd.CreateEllipsoid(
                    new Point3d(660,96,0), Vector3d.ZAxis,
                    new Vector3d(5,3,2));
                Assert.IsTrue(brep != null && brep.IsSolid, "CreateEllipsoid");
                if (brep != null) WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateTube", () =>
            {
                var brep = SolidCmd.CreateTube(
                    new Point3d(660,108,0), Vector3d.ZAxis,
                    2, 4, 8, true);
                Assert.IsTrue(brep != null && brep.IsSolid, "CreateTube");
                if (brep != null) WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateTruncatedPyramid", () =>
            {
                var brep = SolidCmd.CreateTruncatedPyramid(
                    new Point3d(660,124,0), Vector3d.ZAxis,
                    4, 4, 2, 8, true);
                Assert.IsTrue(brep != null && brep.IsSolid, "CreateTruncatedPyramid");
                if (brep != null) WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateLoftSolid", () =>
            {
                // 三个圆截面：圆C r=3, 圆D r=2, 圆E r=1
                var circleC = new Circle(
                    new Plane(new Point3d(660,140,0), Vector3d.ZAxis), 3).ToNurbsCurve();
                var circleD = new Circle(
                    new Plane(new Point3d(660,140,5), Vector3d.ZAxis), 2).ToNurbsCurve();
                var circleE = new Circle(
                    new Plane(new Point3d(660,140,10), Vector3d.ZAxis), 1).ToNurbsCurve();
                var curves = new Curve[] { circleC, circleD, circleE };
                var brep = SolidCmd.CreateLoftSolid(curves, 0, true);
                Assert.IsTrue(brep != null && brep.IsSolid, "CreateLoftSolid");
                if (brep != null) WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateRevolveSolid", () =>
            {
                // 截面弧线必须在包含旋转轴的竖直平面内（法向 X），不能在垂直于轴的水平面内
                // 弧从 angle=0 (距轴3) 到 π/2 (触碰轴上 Z=3)，旋转 360° 后形成 1/4 球壳
                var profile = CurveCmd.CreateArc(
                    new Plane(new Point3d(660,154,0), Vector3d.XAxis),
                    new Point3d(660,154,0), 3, 0, Math.PI / 2).ToNurbsCurve();
                var axis = new Line(
                    new Point3d(660,154,0), new Point3d(660,154,10));
                var brep = SolidCmd.CreateRevolveSolid(
                    profile, axis, 0, 2 * Math.PI, true);
                Assert.IsTrue(brep != null && brep.IsSolid, "CreateRevolveSolid");
                if (brep != null) WriteToDoc(brep, C, "Solid");
            });

            Step("SolidCmd.CreateSweepSolid", () =>
            {
                // 双轨扫掠：两条平行直线 + 圆截面
                var rail1 = new LineCurve(
                    new Point3d(660,168,0), new Point3d(660,168,10)).ToNurbsCurve();
                var rail2 = new LineCurve(
                    new Point3d(666,168,0), new Point3d(666,168,10)).ToNurbsCurve();
                var section = new Circle(
                    new Plane(new Point3d(663,168,0), Vector3d.ZAxis), 2).ToNurbsCurve();
                var sections = new Curve[] { section };
                var brep = SolidCmd.CreateSweepSolid(
                    rail1, rail2, sections, true);
                Assert.IsTrue(brep != null && brep.IsSolid, "CreateSweepSolid");
                if (brep != null) WriteToDoc(brep, C, "Solid");
            });

            Finish(C);
            return Result.Success;
        }
    }
}
