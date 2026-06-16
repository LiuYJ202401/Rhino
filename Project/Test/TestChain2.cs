using System;
using System.Collections.Generic;
using System.IO;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rh.Cmd;
using Rh.Project.Test.Framework;

namespace Rh.Project.Test
{
    /// <summary>
    /// 测试链 2：自由曲线与曲面
    /// 覆盖：NURBS 曲线、插值曲线、手柄曲线、悬链线、螺旋线、螺旋面、
    ///       通过点网格曲面、控制点网格曲面、网络曲面、补面、开发放样、垂幕、高度场
    /// 空间布局：X=120~140
    /// </summary>
    public class TestChain2Cmd : TestBase
    {
        public override string EnglishName => "RhTestChain2";

        // 链名常量
        private const string C = "Chain2";

        // 衔接数据
        private NurbsCurve _nurbsCurve1;   // 步骤1 → 步骤12,14
        private NurbsCurve _interpCrv3;    // 步骤3 → 步骤13

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine($"\n════════ 测试链 2：自由曲线与曲面 ════════\n");

            // ================================================================
            // 自由曲线（X=120）
            // ================================================================

            Step("CurveCmd.CreateNurbsCurve(#1)", () =>
            {
                var pts = new List<Point3d> {
                    new Point3d(120,0,0), new Point3d(124,2,3),
                    new Point3d(128,4,0), new Point3d(132,6,3),
                    new Point3d(136,8,0)
                };
                _nurbsCurve1 = CurveCmd.CreateNurbsCurve(pts, 3);
                Assert.IsValid(_nurbsCurve1, "CreateNurbsCurve(#1)");
                WriteToDoc(_nurbsCurve1, C, "Curve");
            });

            Step("CurveCmd.CreateNurbsCurve(#2)", () =>
            {
                var pts = new List<Point3d> {
                    new Point3d(120,12,0), new Point3d(124,14,0),
                    new Point3d(128,16,0)
                };
                var knots = new List<double> { 0, 0, 0, 1, 1, 1 };
                var weights = new List<double> { 1, 1, 1 };
                var crv = CurveCmd.CreateNurbsCurve(pts, knots, 2, weights);
                Assert.IsValid(crv, "CreateNurbsCurve(#2)");
                WriteToDoc(crv, C, "Curve");
            });

            Step("CurveCmd.CreateInterpCrv", () =>
            {
                var pts = new List<Point3d> {
                    new Point3d(120,20,0), new Point3d(124,22,2),
                    new Point3d(128,24,0), new Point3d(132,26,2)
                };
                _interpCrv3 = CurveCmd.CreateInterpCrv(pts, 3);
                Assert.IsValid(_interpCrv3, "CreateInterpCrv");
                WriteToDoc(_interpCrv3, C, "Curve");
            });

            Step("CurveCmd.CreateHandleCurve", () =>
            {
                var handles = new List<Tuple<Point3d, Point3d>> {
                    Tuple.Create(new Point3d(120,30,0), new Point3d(122,33,0)),
                    Tuple.Create(new Point3d(126,34,0), new Point3d(128,32,0))
                };
                var crv = CurveCmd.CreateHandleCurve(handles, false);
                Assert.IsValid(crv, "CreateHandleCurve");
                WriteToDoc(crv, C, "Curve");
            });

            Step("CurveCmd.CreateCurveThroughPt", () =>
            {
                var pts = new List<Point3d> {
                    new Point3d(120,38,0), new Point3d(124,40,1),
                    new Point3d(128,42,0), new Point3d(132,44,1)
                };
                var crv = CurveCmd.CreateCurveThroughPt(pts, 3);
                Assert.IsValid(crv, "CreateCurveThroughPt");
                WriteToDoc(crv, C, "Curve");
            });

            Step("CurveCmd.CreateCatenary", () =>
            {
                var crv = CurveCmd.CreateCatenary(
                    new Point3d(120,48,0), new Point3d(132,48,0), 16,
                    new Vector3d(0, 0, -1));
                Assert.IsValid(crv, "CreateCatenary");
                WriteToDoc(crv, C, "Curve");
            });

            Step("CurveCmd.CreateHelix(#1)", () =>
            {
                var axis = new Line(new Point3d(120,54,0), new Point3d(120,54,12));
                var crv = CurveCmd.CreateHelix(axis, 2, 2, 3, 4);
                Assert.IsValid(crv, "CreateHelix(#1)");
                WriteToDoc(crv, C, "Curve");
            });

            Step("CurveCmd.CreateHelix(#2)", () =>
            {
                var rail = new LineCurve(
                    new Point3d(120,62,0), new Point3d(132,62,0));
                var crv = CurveCmd.CreateHelix(rail, 1, 2, 2);
                Assert.IsValid(crv, "CreateHelix(#2)");
                WriteToDoc(crv, C, "Curve");
            });

            Step("CurveCmd.CreateSpiral", () =>
            {
                var plane = new Plane(new Point3d(120,70,0), Vector3d.ZAxis);
                var crv = CurveCmd.CreateSpiral(plane, new Point3d(120,70,0), 1, 5, 3);
                Assert.IsValid(crv, "CreateSpiral");
                WriteToDoc(crv, C, "Curve");
            });

            // ================================================================
            // 曲面（X=120, Y=80起）
            // ================================================================

            Step("SurfaceCmd.CreateSrfThroughPts", () =>
            {
                // 构造 4x4 点网格
                var srfPts = new List<Point3d>();
                for (int u = 0; u < 4; u++)
                    for (int v = 0; v < 4; v++)
                        srfPts.Add(new Point3d(120 + u * 2, 80 + v * 2, Math.Sin(u) + Math.Cos(v)));
                var brep = SurfaceCmd.CreateSrfThroughPts(srfPts, 4, 4, 3, 3);
                Assert.IsValid(brep, "CreateSrfThroughPts");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateSrfControlPts", () =>
            {
                // 构造 4x4 控制点网格
                var ctrlPts = new List<Point3d>();
                for (int u = 0; u < 4; u++)
                    for (int v = 0; v < 4; v++)
                        ctrlPts.Add(new Point3d(120 + u * 2, 96 + v * 2, Math.Sin(u) + Math.Cos(v)));
                var brep = SurfaceCmd.CreateSrfControlPts(ctrlPts, 4, 4, 3, 3);
                Assert.IsValid(brep, "CreateSrfControlPts");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateNetworkSrf(#1)", () =>
            {
                // 步骤1的 NurbsCurve + 3条临时U/V曲线
                var uCrv1 = new LineCurve(
                    new Point3d(120,0,0), new Point3d(120,8,0)).ToNurbsCurve();
                var uCrv2 = new LineCurve(
                    new Point3d(136,0,0), new Point3d(136,8,0)).ToNurbsCurve();
                var vCrv1 = new LineCurve(
                    new Point3d(120,0,0), new Point3d(136,0,0)).ToNurbsCurve();
                var curves = new Curve[] { _nurbsCurve1, uCrv1, uCrv2, vCrv1 };
                var brep = SurfaceCmd.CreateNetworkSrf(curves);
                Assert.IsValid(brep, "CreateNetworkSrf(#1)");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateNetworkSrf(#2)", () =>
            {
                // uCurves=[步骤3的InterpCrv+临时1], vCurves=[2条临时曲线]
                var uCrv2 = new LineCurve(
                    new Point3d(120,28,0), new Point3d(132,28,0)).ToNurbsCurve();
                var uCurves = new Curve[] { _interpCrv3, uCrv2 };
                var vCrv1 = new LineCurve(
                    new Point3d(120,20,0), new Point3d(120,28,0)).ToNurbsCurve();
                var vCrv2 = new LineCurve(
                    new Point3d(132,20,0), new Point3d(132,28,0)).ToNurbsCurve();
                var vCurves = new Curve[] { vCrv1, vCrv2 };
                var brep = SurfaceCmd.CreateNetworkSrf(uCurves, vCurves);
                Assert.IsValid(brep, "CreateNetworkSrf(#2)");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreatePatch", () =>
            {
                // 临时弧线 + 临时线段 + 步骤1曲线
                var tempArc = CurveCmd.CreateArc(
                    new Plane(new Point3d(124,116,0), Vector3d.ZAxis),
                    new Point3d(124,116,0), 4, 0, Math.PI).ToNurbsCurve();
                var tempLine = new LineCurve(
                    new Point3d(120,112,0), new Point3d(128,120,0)).ToNurbsCurve();
                var geometry = new GeometryBase[] { tempArc, tempLine, _nurbsCurve1 };
                var brep = SurfaceCmd.CreatePatch(geometry);
                Assert.IsValid(brep, "CreatePatch");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateDevLoft", () =>
            {
                // 两条临时曲线作为轨道
                var rail1 = new LineCurve(
                    new Point3d(120,148,0), new Point3d(132,148,4)).ToNurbsCurve();
                var rail2 = new LineCurve(
                    new Point3d(120,152,0), new Point3d(132,152,4)).ToNurbsCurve();
                var brep = SurfaceCmd.CreateDevLoft(rail1, rail2);
                Assert.IsValid(brep, "CreateDevLoft");
                WriteToDoc(brep, C, "Surface");
            });

            Step("SurfaceCmd.CreateDrape", () =>
            {
                // 临时 Brep 作为垂幕对象
                var tempBrep = SurfaceCmd.CreatePlane(
                    new Plane(new Point3d(120,160,0), Vector3d.ZAxis),
                    new Interval(0, 8), new Interval(0, 6));
                var objects = new GeometryBase[] { tempBrep };
                var brep = SurfaceCmd.CreateDrape(objects, Plane.WorldXY, 10, 10);
                Assert.IsValid(brep, "CreateDrape");
                WriteToDoc(brep, C, "Surface");
            });

            // 步骤17：高度场（如果图片文件不存在则跳过）
            string imagePath = "test_heightfield.png";
            if (!File.Exists(imagePath))
            {
                Skip("SurfaceCmd.CreateHeightfield", "测试图片文件不存在");
            }
            else
            {
                Step("SurfaceCmd.CreateHeightfield", () =>
                {
                    var plane = new Plane(new Point3d(120,172,0), Vector3d.ZAxis);
                    var brep = SurfaceCmd.CreateHeightfield(
                        imagePath, plane, 10, 8, 4, 20, 16);
                    Assert.IsValid(brep, "CreateHeightfield");
                    if (brep != null) WriteToDoc(brep, C, "Surface");
                });
            }

            Finish(C);
            return Result.Success;
        }
    }
}
