using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rh.Cmd;
using Rh.Project.Test.Framework;

namespace Rh.Project.Test
{
    /// <summary>
    /// 测试链 3：对象提取与派生
    /// 覆盖：曲线等分点、起终点、投影/拉回/包裹曲线、边缘复制、等参线、等高线、截面线
    /// 空间布局：X=240~270
    /// </summary>
    public class TestChain3Cmd : TestBase
    {
        public override string EnglishName => "RhTestChain3";

        // 链名常量
        private const string C = "Chain3";

        // 基础几何
        private Circle _circle0a;    // 0a：圆
        private Brep _brep0c;        // 0c：挤出矩形实体（闭合）
        private Brep _sphere0d;      // 0d：球体
        private Brep _openBrep0e;    // 0e：开放挤出面（供 DupEdge 测 Naked 边）

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine($"\n════════ 测试链 3：对象提取与派生 ════════\n");

            // ================================================================
            // 基础几何（X=240）
            // ================================================================

            // 0a：圆 @ (240,0,0) r=6
            _circle0a = CurveCmd.CreateCircle(Plane.WorldXY, new Point3d(240,0,0), 6);
            WriteToDoc(_circle0a.ToNurbsCurve(), C, "Base");

            // 0b：矩形 @ (240,15,0) w=10 h=8 → 0c：沿 Z 轴挤出为实体（带顶底盖）
            var rect0b = CurveCmd.CreateRectangle(
                Plane.WorldXY, new Point3d(240,15,0), 10, 8);
            _brep0c = SolidCmd.CreateExtrudeSolid(
                rect0b.ToNurbsCurve(), new Vector3d(0,0,6), true);
            WriteToDoc(_brep0c, C, "Base");

            // 0d：球体 @ (240,35,5) r=5
            _sphere0d = SolidCmd.CreateSphere(
                new Point3d(240,35,5), Vector3d.ZAxis, 5);
            WriteToDoc(_sphere0d, C, "Base");

            // 0e：开放挤出面 @ (240,50,0) w=8 h=6 h=5（无封盖，有 Naked 边）
            var rect0e = CurveCmd.CreateRectangle(
                Plane.WorldXY, new Point3d(240,50,0), 8, 6);
            _openBrep0e = SurfaceCmd.CreateExtrude(
                rect0e.ToNurbsCurve(), new Vector3d(0,0,5));
            WriteToDoc(_openBrep0e, C, "Base");

            // ================================================================
            // 提取操作（X=260）
            // ================================================================

            Step("CurveCmd.CreateDividePoints(#1)", () =>
            {
                var pts = CurveCmd.CreateDividePoints(_circle0a.ToNurbsCurve(), 6);
                Assert.Count(6, pts.Count, "CreateDividePoints(#1)");
                WritePoints(pts, C, "Curve");
            });

            Step("CurveCmd.CreateDividePoints(#2)", () =>
            {
                var pts = CurveCmd.CreateDividePoints(_circle0a.ToNurbsCurve(), 3.0);
                Assert.GreaterThanZero(pts.Count, "CreateDividePoints(#2)");
                WritePoints(pts, C, "Curve");
            });

            Step("CurveCmd.GetCurveStart", () =>
            {
                var pt = CurveCmd.GetCurveStart(_circle0a.ToNurbsCurve());
                Assert.NotNull(pt, "GetCurveStart");
                WritePoint(pt, C, "Curve");
            });

            Step("CurveCmd.GetCurveEnd", () =>
            {
                var pt = CurveCmd.GetCurveEnd(_circle0a.ToNurbsCurve());
                Assert.NotNull(pt, "GetCurveEnd");
                WritePoint(pt, C, "Curve");
            });

            Step("CurveCmd.CreateProjectCrv", () =>
            {
                // 临时曲线在 Brep 顶面上方，沿 -Z 投影到顶面
                var tempCrv = new LineCurve(
                    new Point3d(243,18,9), new Point3d(247,20,9)).ToNurbsCurve();
                var curves = new Curve[] { tempCrv };
                var result = CurveCmd.CreateProjectCrv(curves, _brep0c, new Vector3d(0,0,-1));
                Assert.GreaterThanZero(
                    result != null ? result.Length : 0, "CreateProjectCrv.Length");
                if (result != null)
                    foreach (var c in result) WriteToDoc(c, C, "Curve");
            });

            Step("CurveCmd.CreatePullCrv", () =>
            {
                // 临时曲线在前墙面附近（Y=12），拉回到前墙面（Y=15）
                var tempCrv = new LineCurve(
                    new Point3d(243,12,2), new Point3d(247,12,4)).ToNurbsCurve();
                var curves = new Curve[] { tempCrv };
                var result = CurveCmd.CreatePullCrv(curves, _brep0c, 0.001);
                Assert.GreaterThanZero(
                    result != null ? result.Length : 0, "CreatePullCrv.Length");
                if (result != null)
                    foreach (var c in result) WriteToDoc(c, C, "Curve");
            });

            Step("CurveCmd.CreateApplyCrv", () =>
            {
                // 临时曲线在前墙面上（Y=15），包裹映射回曲面
                var tempCrv = new LineCurve(
                    new Point3d(243,15,2), new Point3d(247,15,4)).ToNurbsCurve();
                var curves = new Curve[] { tempCrv };
                var result = CurveCmd.CreateApplyCrv(curves, _brep0c);
                Assert.GreaterThanZero(
                    result != null ? result.Length : 0, "CreateApplyCrv.Length");
                if (result != null)
                    foreach (var c in result) WriteToDoc(c, C, "Curve");
            });

            Step("CurveCmd.CreateDupEdge", () =>
            {
                // 开放挤出面有 8 条 Naked 边（4 底 + 4 侧棱），测重载2提取全部裸露边
                var result = CurveCmd.CreateDupEdge(_openBrep0e);
                Assert.GreaterThanZero(
                    result != null ? result.Length : 0, "CreateDupEdge.Length");
                if (result != null)
                    foreach (var c in result) WriteToDoc(c, C, "Curve");
            });

            Step("CurveCmd.CreateExtractIsocurve", () =>
            {
                var crv = CurveCmd.CreateExtractIsocurve(
                    _brep0c, new Point3d(240,15,3), 0);
                Assert.IsValid(crv, "CreateExtractIsocurve");
                if (crv != null) WriteToDoc(crv, C, "Curve");
            });

            Step("CurveCmd.CreateContour", () =>
            {
                var result = CurveCmd.CreateContour(
                    _sphere0d,
                    new Point3d(260,80,0), new Point3d(260,80,10), 2);
                Assert.GreaterThanZero(
                    result != null ? result.Length : 0, "CreateContour.Length");
                if (result != null)
                    foreach (var c in result) WriteToDoc(c, C, "Curve");
            });

            Step("CurveCmd.CreateSection", () =>
            {
                var cutPlane = new Plane(new Point3d(260,90,3), Vector3d.ZAxis);
                var result = CurveCmd.CreateSection(_sphere0d, cutPlane);
                Assert.GreaterThanZero(
                    result != null ? result.Length : 0, "CreateSection.Length");
                if (result != null)
                    foreach (var c in result) WriteToDoc(c, C, "Curve");
            });

            Finish(C);
            return Result.Success;
        }
    }
}
