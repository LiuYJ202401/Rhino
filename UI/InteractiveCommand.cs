using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;

namespace RhinoTrial
{
    /// <summary>
    /// 交互式命令基类
    /// 首先重写GetRequiredInput(RhinoDoc doc){},准备交互前完成的操作
    /// 然后重写DefineOptions(GetPoint gp){},定义交互修改的参数
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public abstract class InteractiveCommand<TResult> : Command
    {
        private class InteractiveGetPoint : GetPoint
        {
            private InteractiveCommand<TResult> _parent;
            public InteractiveGetPoint(InteractiveCommand<TResult> parent) => _parent = parent;
            protected override void OnDynamicDraw(GetPointDrawEventArgs e)
            {
                base.OnDynamicDraw(e);
                _parent.DrawPreview(e, e.CurrentPoint);
            }
        }//内部类
        protected abstract TResult ComputeResult(RhinoDoc doc, RunMode mode, Point3d? clickPoint);//子类重写，得到最终结果
        protected virtual void DrawPreview(GetPointDrawEventArgs e, Point3d? currentPoint) { }//子类重写，定义预览逻辑
        protected virtual bool GetRequiredInput(RhinoDoc doc)//子类重写,交互前完成
        {
            return true; // 默认不需要输入，直接继续
        }
        protected virtual void DefineOptions(GetPoint gp) { }//子类重写，定义交互选项
        protected virtual string CommandPrompt => "命令行提示词";
        protected override sealed Result RunCommand(RhinoDoc doc, RunMode mode)//固定处理流程，子类不再处理
        {
            if (!GetRequiredInput(doc)) { return Result.Cancel; }
            // 1. 创建自定义 GetPoint 对象（内嵌预览逻辑）
            var gp = new InteractiveGetPoint(this);
            gp.SetCommandPrompt(CommandPrompt);
            gp.AcceptNothing(true);
            // 2. 交互循环
            while (true)
            {
                gp.ClearCommandOptions();
                DefineOptions(gp);
                var res = gp.Get();
                if (res == GetResult.Option)
                {
                    continue;
                }
                else if (res == GetResult.Point)
                {
                    // 用户点击，生成最终结果
                    var result = ComputeResult(doc, mode, gp.Point());
                    // 处理结果（例如添加到文档）...
                    OnResultReady(result);
                    break;
                }
                else // Cancel 或 Nothing
                {
                    // 用户取消或回车（可视为默认位置）
                    if (res == GetResult.Nothing)
                    {
                        var result = ComputeResult(doc, mode, null);
                        OnResultReady(result);
                    }
                    break;
                }
            }
            doc.Views.Redraw();
            return Result.Success;
        }
        protected virtual void OnResultReady(TResult result) { }//子类重写，将结构添加到rhino文件中
    }
}