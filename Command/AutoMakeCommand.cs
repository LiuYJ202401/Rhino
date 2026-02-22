using PublicContent;
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

///<summary>本文件存放自动生成建筑配置的命令</summary>
namespace RhinoTrial
{
    /// <summary>
    /// 沿一条曲线自动生成默认栏杆
    /// </summary>
    public class AutoRailCommand : InteractiveCommand<List<Brep>>
    {
        public AutoRailCommand()
        {
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static AutoRailCommand Instance { get; private set; }

        public override string EnglishName => "AutoRail";
        protected override string CommandPrompt => "选择曲线和实体放置栏杆，并调整参数";
        //参数区域
        private Curve _cur;
        private Brep _b;
        private int _num = 2;
        private double _postHeight = 80.0;
        private OptionDouble _heightOption;
        protected override void DefineOptions(GetPoint gp)
        {
            _heightOption = new OptionDouble(_postHeight);
            gp.AddOptionDouble("高度", ref _heightOption);
        }
        protected override bool GetRequiredInput(RhinoDoc doc)
        {
            double Height = 1000;
            double Width = 500;
            double Length = 200;
            Box b = new Box(Plane.WorldXY,
                new Interval(-Length / 2, Length / 2),
                new Interval(-Width / 2, Width / 2),
                new Interval(0, Height)
                );
            Brep _b = b.ToBrep();

            _cur = Rs.GetACurve();
            return true;
        }
        protected override List<Brep> ComputeResult(RhinoDoc doc, RunMode mode, Point3d? clickPoint) {
            List<Brep> result = Rs.BrepsAlongCurve(_cur,_b,_num);
            return result;
        }
    }

    /// <summary>
    /// 从矩形和电梯高度等信息生成电梯
    /// </summary>
    public class AutoLiftCommand : Command
    {
        public AutoLiftCommand()
        {
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static AutoLiftCommand Instance { get; private set; }

        public override string EnglishName => "AutoLift";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: complete command.
            return Result.Success;
        }
    }
}