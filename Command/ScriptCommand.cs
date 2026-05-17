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
using PublicContent.JsonData;

namespace RhinoTrial
{
    public class ScriptCommand : Command
    {
        public ScriptCommand()
        {
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static ScriptCommand Instance { get; private set; }

        public override string EnglishName => "Rs_AutoRail";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Rs_AutoRail data = JsonData.LoadFromJsonFile<Rs_AutoRail>("../../../Data/Rs_AutoRail.json");
            //参数列表
            double _Rail_Space = data.Rail_Space;  //栏杆间隔
            double _Rail_Width = data.Rail_Width;   //栏杆厚度
            double _Rail_Length = data.Rail_Length; //栏杆宽度
            double _Rail_Height = data.Rail_Height;  //扶手总高
            double fushou_Width = data.fushou_Width;   //扶手中部宽度
            double fushou_Height =data.fushou_Height;  //扶手厚度
            double delta = data.delta;     //偏移
            bool reverse = data.reverse;
            //函数实现
            Curve _cur = Rs.GetACurve();
            if (reverse) { _cur.Reverse(); }
            //偏移曲线
            Curve[] move = _cur.Offset((Point3d)(_cur.PointAtStart+Vector3d.CrossProduct(_cur.TangentAtStart,new Vector3d(0,0,1))),new Vector3d(0,0,1),delta,doc.ModelAbsoluteTolerance,CurveOffsetCornerStyle.Round);
            _cur = move[0];
            //栏杆创建
            Rectangle3d rec = new Rectangle3d(Plane.WorldXY,new Point3d(_Rail_Length/2, _Rail_Width/2, 0),new Point3d(-_Rail_Length/2, -_Rail_Width/2, 0));
            Curve rect = rec.ToNurbsCurve();
            Brep rail = Rs.MakeAExtrudedBrep(doc.ModelAbsoluteTolerance, rect, new Vector3d(0, 0,_Rail_Height-fushou_Height));
            //栏杆排列
            int _Rail_Num = 2;
            _Rail_Num += (int)(_cur.GetLength() / _Rail_Space);//栏杆数量
            List<Brep> rails = Rs.BrepsAlongCurve(_cur, rail,num:_Rail_Num,headmove:(_Rail_Length/2),tailmove:(_Rail_Length/2));
            //扶手
            Transform t = Transform.Translation(new Vector3d(0, 0, _Rail_Height - fushou_Height / 2));
            _cur.Transform(t);
            Curve _capsule = Rs.MakeACapsule(doc.ModelAbsoluteTolerance, fushou_Width, fushou_Height / 2);
            Brep fushou=Rs.BrepsSweepFromACurve(doc.ModelAbsoluteTolerance,_cur
                , _capsule);
            //添加
            foreach (Brep b in rails) { doc.Objects.AddBrep(b); }
            doc.Objects.AddBrep(fushou);

            return Result.Success;
        }
    }
}