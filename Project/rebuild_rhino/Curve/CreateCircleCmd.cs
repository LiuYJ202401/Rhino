using Rhino;
using Rhino.Geometry;
using Rh.Cmd;
using Rh.UI;

namespace Rh.Project.RebuildRhino.Curve
{
    /// <summary>
    /// 画圆命令（圆心 + 半径方式）。
    /// 对应 Rhino 原生命令：Circle
    ///
    /// 流程：
    ///   步骤 1 — 用户选择圆心
    ///   步骤 2 — 用户选择半径方向（拖动鼠标）或输入半径数值
    ///   完成   — 将圆写入 Rhino 文档
    /// </summary>
    public class CreateCircleCmd : UICommand
    {
        public override string EnglishName => "RhCreateCircle";

        // ================================================================
        // 共享字段
        // 在 OnInit 中重置，步骤间通过字段传递数据
        // ================================================================

        private Point3d _center;
        private double _radius;
        private Circle _result;

        // ================================================================
        // 初始化
        // ================================================================

        protected override void OnInit()
        {
            _center = Point3d.Unset;
            _radius = CurveCmd.GetDefaultCircleRadius();
            _result = Circle.Unset;
        }

        // ================================================================
        // 步骤定义
        // ================================================================

        protected override StepDef[] DefineSteps()
        {
            return new StepDef[]
            {
                Step.Create("圆心", SetupCenter, ProcessCenter, PreviewCenter),
                Step.Create("半径", SetupRadius, ProcessRadius, PreviewRadius)
            };
        }

        // ================================================================
        // 步骤 1：选择圆心
        // ================================================================

        /// <summary>
        /// 声明输入：一个点（圆心位置）
        /// </summary>
        private void SetupCenter(InputBuilder input)
        {
            input.Point("point", "选择圆心");
        }

        /// <summary>
        /// 处理圆心输入：保存到字段，供后续步骤使用
        /// </summary>
        private void ProcessCenter(InputResult result, bool isPreview)
        {
            _center = result.Get<Point3d>("point");
        }

        /// <summary>
        /// 预览：显示当前圆心位置（一个点）
        /// </summary>
        private object[] PreviewCenter()
        {
            return new object[] { _center };
        }

        // ================================================================
        // 步骤 2：选择半径
        // ================================================================

        /// <summary>
        /// 声明输入：一个点（半径方向）+ 半径数值选项
        ///
        /// 交互方式：
        ///   - 拖动鼠标 → 半径 = 圆心到鼠标点的距离（基点约束显示距离值）
        ///   - 输入数值 → 半径 = 输入值，然后点击确认
        /// </summary>
        private void SetupRadius(InputBuilder input)
        {
            input.Point("point", "选择半径方向");
            input.BasePoint(_center);
            input.Double("radius", "半径", _radius);
        }

        /// <summary>
        /// 处理半径输入：
        ///   1. 判断半径来源（鼠标距离 or 数值输入）
        ///   2. 调用 Command 层创建圆几何对象
        /// </summary>
        private void ProcessRadius(InputResult result, bool isPreview)
        {
            Point3d radiusPoint = result.Get<Point3d>("point");
            double inputRadius = result.Get<double>("radius");

            // 判断半径来源：鼠标点到圆心有距离时用距离，否则用输入值
            double mouseDist = _center.DistanceTo(radiusPoint);
            if (mouseDist > 0.001)
                _radius = mouseDist;
            else
                _radius = inputRadius;

            // 获取当前视口的构造平面
            Plane plane = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.ConstructionPlane();

            // 调用 Command 层创建圆
            _result = CurveCmd.CreateCircle(plane, _center, _radius, isPreview);
        }

        /// <summary>
        /// 预览：返回当前圆（模板自动绘制）
        /// </summary>
        private object[] PreviewRadius()
        {
            if (_result.IsValid)
                return new object[] { _result };
            return null;
        }

        // ================================================================
        // 完成：写入文档
        // ================================================================

        /// <summary>
        /// 所有步骤完成后调用，将最终结果写入 Rhino 文档。
        /// 以 isPreview=false 调用 Command 层，触发默认值更新。
        /// </summary>
        protected override void OnFinish(RhinoDoc doc)
        {
            // 获取当前视口的构造平面
            Plane plane = doc.Views.ActiveView.ActiveViewport.ConstructionPlane();

            // 调用 Command 层创建最终圆（isPreview=false，更新默认值）
            _result = CurveCmd.CreateCircle(plane, _center, _radius);

            // 写入文档（Project 层是唯一写入文档的层）
            doc.Objects.AddCircle(_result);
        }
    }
}
