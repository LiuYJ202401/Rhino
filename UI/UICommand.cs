using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Rh.UI
{
    /// <summary>
    /// UI 命令模板基类。
    /// 采用模板方法模式，封装交互循环、选项管理、预览触发、异常捕获。
    /// 开发者覆盖 OnInit、DefineSteps、OnFinish 即可。
    /// </summary>
    public abstract class UICommand : Command
    {
        // ================================================================
        // 开发者覆盖的函数
        // ================================================================

        /// <summary>命令开始时调用，用于重置字段（可选覆盖）</summary>
        protected virtual void OnInit() { }

        /// <summary>定义步骤顺序（必须覆盖）</summary>
        protected abstract StepDef[] DefineSteps();

        /// <summary>所有步骤完成后写入文档（必须覆盖）</summary>
        protected abstract void OnFinish(RhinoDoc doc);

        /// <summary>预览颜色（可选覆盖，默认 CornflowerBlue）</summary>
        protected virtual Color PreviewColor => Color.CornflowerBlue;

        // ================================================================
        // RunCommand（模板封装，不可覆盖）
        // ================================================================

        protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                // 1. 初始化
                OnInit();

                // 2. 获取步骤
                StepDef[] steps = DefineSteps();

                // 3. 逐步执行
                for (int i = 0; i < steps.Length; i++)
                {
                    StepDef step = steps[i];

                    // 3a. 开发者声明输入
                    var input = new InputBuilder();
                    step.Setup(input);

                    // 3b. 模板处理交互循环
                    InputResult finalInput = RunStepInteraction(step, input, doc);

                    // 3c. 用户取消
                    if (finalInput == null)
                        return Result.Cancel;

                    // 3d. 开发者最终处理
                    step.Process(finalInput, false);
                }

                // 4. 写入文档
                OnFinish(doc);
                doc.Views.Redraw();
                return Result.Success;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine("[{0}] 错误: {1}", EnglishName, ex.Message);
                return Result.Failure;
            }
        }

        // ================================================================
        // 步骤交互分发
        // ================================================================

        private InputResult RunStepInteraction(StepDef step, InputBuilder input, RhinoDoc doc)
        {
            if (input.HasPoints)
                return RunGetPointsStep(step, input);

            if (input.HasObject)
                return RunGetObjectStep(step, input);

            if (input.HasPoint)
                return RunGetPointStep(step, input);

            return RunOptionsOnlyStep(step, input);
        }

        // ================================================================
        // GetPoint 步骤
        // ================================================================

        private InputResult RunGetPointStep(StepDef step, InputBuilder input)
        {
            var gp = new GetPoint();
            gp.SetCommandPrompt(input.Prompt);

            if (input.HasBasePoint)
                gp.SetBasePoint(input.BasePointValue, input.ShowBasePointDistance);

            foreach (InputItem item in input.OptionItems)
                item.AddToGetPoint(gp);

            // 绑定预览
            var previewHandler = new DynamicDrawHandler(this, step, input);
            gp.DynamicDraw += previewHandler.OnDynamicDraw;

            while (true)
            {
                GetResult getResult = gp.Get();

                if (gp.CommandResult() != Result.Success)
                    return null;

                if (getResult == GetResult.Option)
                {
                    foreach (InputItem item in input.OptionItems)
                        item.UpdateFromGetter(gp);
                    continue;
                }

                if (getResult == GetResult.Point)
                    return BuildInputResult(input, gp);
            }
        }

        // ================================================================
        // GetPoints 步骤（循环选点）
        // ================================================================

        private InputResult RunGetPointsStep(StepDef step, InputBuilder input)
        {
            var points = new List<Point3d>();
            int minCount = input.PointsItem.MinCount;

            var gp = new GetPoint();
            gp.SetCommandPrompt(input.Prompt);
            gp.AcceptNothing(true);

            if (input.HasBasePoint)
                gp.SetBasePoint(input.BasePointValue, input.ShowBasePointDistance);

            foreach (InputItem item in input.OptionItems)
                item.AddToGetPoint(gp);

            var previewHandler = new PointsDynamicDrawHandler(this, step, input, points);
            gp.DynamicDraw += previewHandler.OnDynamicDraw;

            while (true)
            {
                GetResult getResult = gp.Get();

                if (gp.CommandResult() != Result.Success)
                    return null;

                if (getResult == GetResult.Option)
                {
                    foreach (InputItem item in input.OptionItems)
                        item.UpdateFromGetter(gp);
                    continue;
                }

                if (getResult == GetResult.Point)
                {
                    points.Add(gp.Point());
                    gp.SetCommandPrompt(string.Format("{0}（已选 {1} 个，Enter 结束）", input.Prompt, points.Count));
                }
                else if (getResult == GetResult.Nothing)
                {
                    if (points.Count >= minCount)
                        return BuildPointsInputResult(input, gp, points);

                    RhinoApp.WriteLine("至少需要 {0} 个点", minCount);
                }
            }
        }

        // ================================================================
        // GetObject 步骤
        // ================================================================

        private InputResult RunGetObjectStep(StepDef step, InputBuilder input)
        {
            var go = new GetObject();
            go.SetCommandPrompt(input.Prompt);
            go.EnablePreSelect(true, true);
            go.GroupSelect = true;
            go.GeometryFilter = input.ObjectItem.Filter;

            foreach (InputItem item in input.OptionItems)
                item.AddToGetObject(go);

            while (true)
            {
                GetResult getResult;

                if (input.ObjectItem.Multiple)
                    getResult = go.GetMultiple(input.ObjectItem.MinCount, 0);
                else
                    getResult = go.Get();

                if (go.CommandResult() != Result.Success)
                    return null;

                if (getResult == GetResult.Option)
                {
                    foreach (InputItem item in input.OptionItems)
                        item.UpdateFromGetter(go);
                    continue;
                }

                return BuildObjectInputResult(input, go);
            }
        }

        // ================================================================
        // 仅选项步骤
        // ================================================================

        private InputResult RunOptionsOnlyStep(StepDef step, InputBuilder input)
        {
            var gp = new GetPoint();
            gp.SetCommandPrompt(input.Prompt);
            gp.AcceptNothing(true);
            gp.AcceptPoint(true);

            if (input.HasBasePoint)
                gp.SetBasePoint(input.BasePointValue, input.ShowBasePointDistance);

            foreach (InputItem item in input.OptionItems)
                item.AddToGetPoint(gp);

            var previewHandler = new DynamicDrawHandler(this, step, input);
            gp.DynamicDraw += previewHandler.OnDynamicDraw;

            while (true)
            {
                GetResult getResult = gp.Get();

                if (gp.CommandResult() != Result.Success)
                    return null;

                if (getResult == GetResult.Option)
                {
                    foreach (InputItem item in input.OptionItems)
                        item.UpdateFromGetter(gp);
                    continue;
                }

                if (getResult == GetResult.Nothing || getResult == GetResult.Point)
                    return BuildInputResult(input, gp);
            }
        }

        // ================================================================
        // 构建输入结果
        // ================================================================

        private InputResult BuildInputResult(InputBuilder input, GetPoint gp)
        {
            var result = new InputResult();

            if (input.HasPoint)
                result.Set(input.PointItem.Name, gp.Point());

            foreach (InputItem item in input.OptionItems)
                result.Set(item.Name, item.CurrentValue);

            return result;
        }

        private InputResult BuildPointsInputResult(InputBuilder input, GetPoint gp, List<Point3d> points)
        {
            var result = new InputResult();

            result.Set(input.PointsItem.Name, points);

            foreach (InputItem item in input.OptionItems)
                result.Set(item.Name, item.CurrentValue);

            return result;
        }

        private InputResult BuildObjectInputResult(InputBuilder input, GetObject go)
        {
            var result = new InputResult();

            ObjRef[] objRefs = go.Objects();
            RhinoObject[] rhinoObjects = objRefs.Select(r => r.Object()).ToArray();
            result.Set(input.ObjectItem.Name, rhinoObjects);

            GeometryBase[] geometry = objRefs.Select(r => r.Geometry()).ToArray();
            result.Set(input.ObjectItem.Name + "_geom", geometry);

            foreach (InputItem item in input.OptionItems)
                result.Set(item.Name, item.CurrentValue);

            return result;
        }

        // ================================================================
        // 几何绘制（预览渲染）
        // ================================================================

        internal void DrawGeometry(DisplayPipeline display, object geometry)
        {
            if (geometry == null)
                return;

            Color color = PreviewColor;

            switch (geometry)
            {
                case Point3d pt:
                    display.DrawPoint(pt, color);
                    break;
                case Line line:
                    display.DrawLine(line, color, 1);
                    break;
                case Circle circle:
                    display.DrawCircle(circle, color, 1);
                    break;
                case Arc arc:
                    display.DrawArc(arc, color, 1);
                    break;
                case Ellipse ellipse:
                    display.DrawCurve(ellipse.ToNurbsCurve(), color, 1);
                    break;
                case Curve curve:
                    display.DrawCurve(curve, color, 1);
                    break;
                case Brep brep:
                    display.DrawBrepWires(brep, color, 1);
                    break;
                case Surface surface:
                    display.DrawSurface(surface, color, 1);
                    break;
                case Sphere sphere:
                    display.DrawSphere(sphere, color, 1);
                    break;
                case Cylinder cylinder:
                    display.DrawCylinder(cylinder, color, 1);
                    break;
                case Cone cone:
                    display.DrawCone(cone, color, 1);
                    break;
                case Box box:
                    display.DrawBox(box.BoundingBox, color, 1);
                    break;
                case Mesh mesh:
                    display.DrawMeshWires(mesh, color, 1);
                    break;
                case Polyline polyline:
                    display.DrawPolyline(polyline, color, 1);
                    break;
                case IEnumerable<object> list:
                    foreach (object item in list)
                        DrawGeometry(display, item);
                    break;
            }
        }

        // ================================================================
        // DynamicDraw 事件处理（内部类，避免 lambda）
        // ================================================================

        private class DynamicDrawHandler
        {
            private readonly UICommand _command;
            private readonly StepDef _step;
            private readonly InputBuilder _input;

            public DynamicDrawHandler(UICommand command, StepDef step, InputBuilder input)
            {
                _command = command;
                _step = step;
                _input = input;
            }

            public void OnDynamicDraw(object sender, GetPointDrawEventArgs e)
            {
                var tempInput = new InputResult();

                if (_input.HasPoint)
                    tempInput.Set(_input.PointItem.Name, e.CurrentPoint);

                foreach (InputItem item in _input.OptionItems)
                    tempInput.Set(item.Name, item.CurrentValue);

                _step.Process(tempInput, true);

                if (_step.Preview != null)
                {
                    object[] geometries = _step.Preview();
                    if (geometries != null)
                    {
                        foreach (object geom in geometries)
                            _command.DrawGeometry(e.Display, geom);
                    }
                }
            }
        }

        // ================================================================
        // 多点 DynamicDraw 事件处理
        // ================================================================

        private class PointsDynamicDrawHandler
        {
            private readonly UICommand _command;
            private readonly StepDef _step;
            private readonly InputBuilder _input;
            private readonly List<Point3d> _existingPoints;

            public PointsDynamicDrawHandler(UICommand command, StepDef step, InputBuilder input, List<Point3d> existingPoints)
            {
                _command = command;
                _step = step;
                _input = input;
                _existingPoints = existingPoints;
            }

            public void OnDynamicDraw(object sender, GetPointDrawEventArgs e)
            {
                var tempPoints = new List<Point3d>(_existingPoints) { e.CurrentPoint };

                var tempInput = new InputResult();
                tempInput.Set(_input.PointsItem.Name, tempPoints);

                foreach (InputItem item in _input.OptionItems)
                    tempInput.Set(item.Name, item.CurrentValue);

                _step.Process(tempInput, true);

                if (_step.Preview != null)
                {
                    object[] geometries = _step.Preview();
                    if (geometries != null)
                    {
                        foreach (object geom in geometries)
                            _command.DrawGeometry(e.Display, geom);
                    }
                }
            }
        }
    }
}
