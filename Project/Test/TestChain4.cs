using System;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rh.Cmd;
using Rh.Project.Test.Framework;

namespace Rh.Project.Test
{
    /// <summary>
    /// 测试链 4：变换与阵列
    /// 覆盖：移动、复制、旋转、缩放、镜像、剪切、
    ///       线性/矩形/环形/沿曲线/曲面上阵列、
    ///       定向、曲面上定向、曲线上定向、工作平面重映射、投影到工作平面
    /// 空间布局：X=360~480
    /// </summary>
    public class TestChain4Cmd : TestBase
    {
        public override string EnglishName => "RhTestChain4";

        // 链名常量
        private const string C = "Chain4";

        // 基础几何
        private Brep _sphere0a;     // 0a：球体
        private Brep _box0b;        // 0b：长方体
        private Curve _line0c;      // 0c：直线
        private Brep _plane0d;      // 0d：平面

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine($"\n════════ 测试链 4：变换与阵列 ════════\n");

            // ================================================================
            // 基础几何（X=360）
            // ================================================================

            // 0a：球体 @ (360,0,4) r=3
            _sphere0a = SolidCmd.CreateSphere(
                new Point3d(360,0,4), Vector3d.ZAxis, 3);
            WriteToDoc(_sphere0a, C, "Base");

            // 0b：长方体 c1=(360,10,0) c2=(366,16,4)
            _box0b = SolidCmd.CreateBox(
                new Point3d(360,10,0), new Point3d(366,16,4), Vector3d.ZAxis);
            WriteToDoc(_box0b, C, "Base");

            // 0c：直线 (360,25,0)-(390,25,0)
            _line0c = new LineCurve(
                new Point3d(360,25,0), new Point3d(390,25,0)).ToNurbsCurve();
            WriteToDoc(_line0c, C, "Base");

            // 0d：平面 @(360,30,0) domU=(0~20) domV=(0~15)
            _plane0d = SurfaceCmd.CreatePlane(
                new Plane(new Point3d(360,30,0), Vector3d.ZAxis),
                new Interval(0, 20), new Interval(0, 15));
            WriteToDoc(_plane0d, C, "Base");

            // ================================================================
            // 基本变换（X=380）
            // ================================================================

            Step("TransformCmd.Move", () =>
            {
                var copy = (GeometryBase)_sphere0a.Duplicate();
                var result = TransformCmd.Move(copy, new Vector3d(20,0,0));
                Assert.NotNull(result, "Move");
                if (result != null) WriteToDoc(result, C, "Transform");
            });

            Step("TransformCmd.Copy", () =>
            {
                var result = TransformCmd.Copy(_sphere0a, new Vector3d(20,12,0));
                Assert.NotNull(result, "Copy");
                if (result != null) WriteToDoc(result, C, "Transform");
            });

            Step("TransformCmd.Rotate(#1)", () =>
            {
                var copy = (GeometryBase)_box0b.Duplicate();
                var result = TransformCmd.Rotate(copy, Math.PI / 4, new Point3d(380,24,0));
                Assert.NotNull(result, "Rotate(#1)");
                if (result != null) WriteToDoc(result, C, "Transform");
            });

            Step("TransformCmd.Rotate(#2)", () =>
            {
                var copy = (GeometryBase)_box0b.Duplicate();
                var result = TransformCmd.Rotate(
                    copy, Math.PI / 4, new Vector3d(0,0,1), new Point3d(380,36,0));
                Assert.NotNull(result, "Rotate(#2)");
                if (result != null) WriteToDoc(result, C, "Transform");
            });

            Step("TransformCmd.Scale(#1)", () =>
            {
                var copy = (GeometryBase)_sphere0a.Duplicate();
                var result = TransformCmd.Scale(copy, new Point3d(380,48,0), 1.5);
                Assert.NotNull(result, "Scale(#1)");
                if (result != null) WriteToDoc(result, C, "Transform");
            });

            Step("TransformCmd.Scale(#2)", () =>
            {
                var copy = (GeometryBase)_box0b.Duplicate();
                var result = TransformCmd.Scale(
                    copy, new Plane(new Point3d(380,60,0), Vector3d.ZAxis),
                    2, 1, 1);
                Assert.NotNull(result, "Scale(#2)");
                if (result != null) WriteToDoc(result, C, "Transform");
            });

            Step("TransformCmd.Mirror(#1)", () =>
            {
                var copy = (GeometryBase)_sphere0a.Duplicate();
                var mirrorPlane = new Plane(new Point3d(380,72,0), new Vector3d(0,1,0));
                var result = TransformCmd.Mirror(copy, mirrorPlane);
                Assert.NotNull(result, "Mirror(#1)");
                if (result != null) WriteToDoc(result, C, "Transform");
            });

            Step("TransformCmd.Mirror(#2)", () =>
            {
                var copy = (GeometryBase)_box0b.Duplicate();
                var result = TransformCmd.Mirror(
                    copy, new Point3d(380,84,0), new Vector3d(1,0,0));
                Assert.NotNull(result, "Mirror(#2)");
                if (result != null) WriteToDoc(result, C, "Transform");
            });

            Step("TransformCmd.Shear", () =>
            {
                var copy = (GeometryBase)_box0b.Duplicate();
                var result = TransformCmd.Shear(
                    copy,
                    new Plane(new Point3d(380,96,0), Vector3d.ZAxis),
                    new Vector3d(1.2,0,0),
                    new Vector3d(0,1,0),
                    new Vector3d(0,0,1));
                Assert.NotNull(result, "Shear");
                if (result != null) WriteToDoc(result, C, "Transform");
            });

            // ================================================================
            // 阵列（X=420）
            // ================================================================

            Step("TransformCmd.ArrayLinear", () =>
            {
                var smallBox = SolidCmd.CreateBox(
                    new Point3d(420,0,0), new Point3d(423,3,3), Vector3d.ZAxis);
                var result = TransformCmd.ArrayLinear(smallBox, new Vector3d(0,1,0), 4);
                Assert.Count(4, result != null ? result.Length : 0, "ArrayLinear");
                if (result != null)
                    foreach (var g in result) WriteToDoc(g, C, "Array");
            });

            Step("TransformCmd.ArrayRectangular", () =>
            {
                var smallBox = SolidCmd.CreateBox(
                    new Point3d(420,48,0), new Point3d(423,51,3), Vector3d.ZAxis);
                var result = TransformCmd.ArrayRectangular(
                    smallBox, Plane.WorldXY, 3, 2, 1, 8, 8, 0);
                Assert.Count(6, result != null ? result.Length : 0, "ArrayRectangular");
                if (result != null)
                    foreach (var g in result) WriteToDoc(g, C, "Array");
            });

            Step("TransformCmd.ArrayPolar", () =>
            {
                var smallBox = SolidCmd.CreateBox(
                    new Point3d(420,72,0), new Point3d(423,75,3), Vector3d.ZAxis);
                var axis = new Line(
                    new Point3d(420,80,0), new Point3d(420,80,10));
                var result = TransformCmd.ArrayPolar(
                    smallBox, axis, 6, 2 * Math.PI, true);
                Assert.Count(6, result != null ? result.Length : 0, "ArrayPolar");
                if (result != null)
                    foreach (var g in result) WriteToDoc(g, C, "Array");
            });

            Step("TransformCmd.ArrayAlongCrv(#1)", () =>
            {
                var smallSphere = SolidCmd.CreateSphere(
                    new Point3d(420,100,0), Vector3d.ZAxis, 1);
                var rail = _line0c.DuplicateCurve();
                var result = TransformCmd.ArrayAlongCrv(smallSphere, rail, 4, true);
                Assert.Count(4, result != null ? result.Length : 0, "ArrayAlongCrv(#1)");
                if (result != null)
                    foreach (var g in result) WriteToDoc(g, C, "Array");
            });

            Step("TransformCmd.ArrayAlongCrv(#2)", () =>
            {
                var smallSphere = SolidCmd.CreateSphere(
                    new Point3d(420,116,0), Vector3d.ZAxis, 1);
                var rail = _line0c.DuplicateCurve();
                var result = TransformCmd.ArrayAlongCrv(smallSphere, rail, 8.0, false);
                Assert.GreaterThanZero(
                    result != null ? result.Length : 0, "ArrayAlongCrv(#2)");
                if (result != null)
                    foreach (var g in result) WriteToDoc(g, C, "Array");
            });

            Step("TransformCmd.ArrayOnSrf", () =>
            {
                var smallSphere = SolidCmd.CreateSphere(
                    new Point3d(420,132,0), Vector3d.ZAxis, 1);
                var result = TransformCmd.ArrayOnSrf(smallSphere, _plane0d, 3, 2);
                Assert.Count(6, result != null ? result.Length : 0, "ArrayOnSrf");
                if (result != null)
                    foreach (var g in result) WriteToDoc(g, C, "Array");
            });

            // ================================================================
            // 定向与投影（X=460）
            // ================================================================

            Step("TransformCmd.Orient", () =>
            {
                var copy = (GeometryBase)_sphere0a.Duplicate();
                var source = new Plane(new Point3d(460,0,0), Vector3d.ZAxis);
                var target = new Plane(new Point3d(460,10,0), new Vector3d(0,0,1));
                var result = TransformCmd.Orient(copy, source, target);
                Assert.NotNull(result, "Orient");
                if (result != null) WriteToDoc(result, C, "Orient");
            });

            Step("TransformCmd.OrientOnSrf", () =>
            {
                var copy = (GeometryBase)_sphere0a.Duplicate();
                var result = TransformCmd.OrientOnSrf(
                    copy, Plane.WorldXY, _plane0d, new Point3d(460,24,0));
                Assert.NotNull(result, "OrientOnSrf");
                if (result != null) WriteToDoc(result, C, "Orient");
            });

            Step("TransformCmd.OrientOnCrv", () =>
            {
                var copy = (GeometryBase)_sphere0a.Duplicate();
                var rail = _line0c.DuplicateCurve();
                var result = TransformCmd.OrientOnCrv(
                    copy, Plane.WorldXY, rail, 0.5);
                Assert.NotNull(result, "OrientOnCrv");
                if (result != null) WriteToDoc(result, C, "Orient");
            });

            Step("TransformCmd.RemapCPlane", () =>
            {
                var copy = (GeometryBase)_sphere0a.Duplicate();
                var newCPlane = new Plane(new Point3d(460,48,0), new Vector3d(0,1,0));
                var result = TransformCmd.RemapCPlane(copy, Plane.WorldXY, newCPlane);
                Assert.NotNull(result, "RemapCPlane");
                if (result != null) WriteToDoc(result, C, "Orient");
            });

            Step("TransformCmd.ProjectToCPlane", () =>
            {
                // 抬高后的球体副本投影到 WorldXY 平面
                var copy = (GeometryBase)_sphere0a.Duplicate();
                var raised = TransformCmd.Move(copy, new Vector3d(100,60,10));
                var plane = new Plane(new Point3d(460,60,0), Vector3d.ZAxis);
                var result = TransformCmd.ProjectToCPlane(raised, plane);
                Assert.NotNull(result, "ProjectToCPlane");
                if (result != null) WriteToDoc(result, C, "Orient");
            });

            Finish(C);
            return Result.Success;
        }
    }
}
