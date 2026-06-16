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
        private Brep _brep0c;        // 0c：挤出矩形
        private Brep _sphere0d;      // 0d：球体

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine($"\n════════ 测试链 3：对象提取与派生 ════════\n");

            // ================================================================
            // 基础几何（X=240）
            // ================================================================

            // 0a：圆 @ (240,0,0) r=6
            _circle0a = CurveCmd.CreateCircle(Plane.WorldXY, new Point3d(240,0,0), 6);
            WriteToDoc(_circle0a.ToNurbsCurve(), C, "Base");

            // 0b：矩形 @ (240,15,0) w=10 h=8 → 0c：沿 Z 轴挤出 6
            var rect0b = CurveCmd.CreateRectangle(
                Plane.WorldXY, new Point3d(240,15,0), 10, 8);
            _brep0c = SurfaceCmd.CreateExtrude(
                rect0b.ToNurbsCurve(), new Vector3d(0,0,6));
            WriteToDoc(_brep0c, C, "Base");

            // 0d：球体 @ (240,35,5) r=5
            _sphere0d = SolidCmd.CreateSphere(
                new Point3d(240,35,5), Vector3d.ZAxis, 5);
            WriteToDoc(_sphere0d, C, "Base");

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
                // 临时曲线投影到 0c Brep
                var tempCrv = new LineCurve(
                    new Point3d(260,30,5), new Point3d(260,30,-1)).ToNurbsCurve();
                var curves = new Curve[] { tempCrv };
                var result = CurveCmd.CreateProjectCrv(curves, _brep0c, new Vector3d(0,0,-1));
                Assert.GreaterThanZero(
                    result != null ? result.Length : 0, "CreateProjectCrv.Length");
                if (result != null)
                    foreach (var c in result) WriteToDoc(c, C, "Curve");
            });

            Step("CurveCmd.CreatePullCrv", () =>
            {
                // 临时曲线拉回到 0c Brep
                var tempCrv = new LineCurve(
                    new Point3d(260,40,3), new Point3d(270,40,3)).ToNurbsCurve();
                var curves = new Curve[] { tempCrv };
                var result = CurveCmd.CreatePullCrv(curves, _brep0c, 0.001);
                Assert.GreaterThanZero(
                    result != null ? result.Length : 0, "CreatePullCrv.Length");
                if (result != null)
                    foreach (var c in result) WriteToDoc(c, C, "Curve");
            });

            Step("CurveCmd.CreateApplyCrv", () =>
            {
                // 临时曲线包裹映射到 0c Brep
                var tempCrv = new LineCurve(
                    new Point3d(260,50,3), new Point3d(270,50,3)).ToNurbsCurve();
                var curves = new Curve[] { tempCrv };
                var result = CurveCmd.CreateApplyCrv(curves, _brep0c);
                Assert.GreaterThanZero(
                    result != null ? result.Length : 0, "CreateApplyCrv.Length");
                if (result != null)
                    foreach (var c in result) WriteToDoc(c, C, "Curve");
            });

            Step("CurveCmd.CreateDupEdge", () =>
            {
                var result = CurveCmd.CreateDupEdge(_brep0c);
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
