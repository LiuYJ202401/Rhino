using Eto.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Morphs;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using static Rhino.DocObjects.Font;

namespace PublicContent
{
    public static partial class Rs
    {
        /// <summary>
        /// 将Brep沿曲线从头到尾等距竖直排列
        /// </summary>
        /// <param name="cur">路径曲线</param>
        /// <param name="b">排列的Brep实体</param>
        /// <param name="num">排列数量，默认为2</param>
        /// <param name="vect">Brep要沿曲线方向放置的正方向，默认为X轴方向</param>
        /// <param name="pp">Brep基准点，默认为原点</param>
        /// <param name="delta">Brep实体距离曲线的距离，默认为0</param>
        /// <param name="reverse">左右翻转，默认为false</param>
        /// <param name="headmove">开始放置的位置变化，非负数，默认为0</param>
        /// <param name="tailmove">结束放置的位置变化，非负数，默认为0</param>
        /// <returns></returns>
        public static List<Brep> BrepsAlongCurve(Curve cur,Brep b,int num=2,Vector3d? vect=null,Point3d? pp=null,double delta=0,bool reverse=false,double headmove=0,double tailmove=0)
        {
            if (cur == null||b==null) { return null; }
            if (num < 2) { num = 2; }
            if (reverse) { cur.Reverse(); }
            Vector3d vec = vect ?? new Vector3d(1,0,0);
            Point3d pt = pp ?? new Point3d(0, 0, 0);
            if (vec[2] != 0) { vec = new Vector3d(vec.X, vec.Y, 0); vec.Unitize(); }
            List<Brep> breps = new List<Brep>();
            Interval domain = cur.Domain;
            double len = cur.GetLength();
            if (len > (headmove + tailmove)) { len -= (headmove + tailmove); } else { headmove = 0;tailmove = 0; }
                //初始化输入内容
                for (int i = 0; i < num; i++)
                {
                    double t;
                    cur.LengthParameter(headmove + len * i / (num - 1), out t);//等长得到参数t
                    Point3d p = cur.PointAt(t);//曲线上点
                    Vector3d tangent = cur.TangentAt(t);//切线方向
                    Vector3d pVec = new Vector3d(tangent.X, tangent.Y, 0);
                    pVec.Unitize();
                    double theta = Vector3d.VectorAngle(pVec, vec, Plane.WorldXY);
                    Brep nb = b.DuplicateBrep();
                    Transform rot = Transform.Rotation(-theta, Vector3d.ZAxis, pt);
                    nb.Transform(rot);
                    //绕pt点Z轴旋转theta弧度
                    Vector3d moveVec = (p - pt);
                    if (delta != 0) { Vector3d d = new Vector3d(-pVec[1], pVec[0], 0); moveVec += d * delta; }
                    Transform move = Transform.Translation(moveVec);
                    nb.Transform(move);
                    //移动
                    breps.Add(nb);
                }
            return breps;
        }

        /// <summary>
        /// 通过放样得到截面都是cir形状的path路径管道
        /// </summary>
        /// <param name="tolerance">doc.ModelAbsoluteTolerance</param>
        /// <param name="path">路径曲线，不能是闭合曲线，否则会放样失败</param>
        /// <param name="cir">截面曲线</param>
        /// <param name="degree">精细度</param>
        /// <param name="center">默认原点，cir的基准点</param>
        /// <param name="cirV">默认Z轴，cir的主要方向，沿path切线方向</param>
        /// <param name="secCirV">默认Z轴，cir的副方向，沿path的Z轴正方向</param>
        /// <returns></returns>
        public static Brep BrepsSweepFromACurve(double tolerance,Curve path,Curve cir,int degree=3,Point3d? center=null,Vector3d? cirV=null,Vector3d?secCirV=null) {
            if (cirV == null) { cirV = new Vector3d(0, 0, 1); }
            if (secCirV == null) { secCirV = new Vector3d(0, 1, 0); }
            if (center == null) { center = new Point3d(0, 0, 0); }

            List<Curve> cs = new List<Curve>();
            NurbsCurve nurbsC = cir as NurbsCurve;
            if (nurbsC == null) { nurbsC = cir.ToNurbsCurve(); }
            if (nurbsC == null) { RhinoApp.WriteLine("截面曲线无法处理"); }
            int jieshu = degree * nurbsC.Points.Count;
            double t0 = path.Domain.T0;
            double t1 = path.Domain.T1;
            for (int i = 0; i < jieshu; i++) {
                double x = i*1.0 / (jieshu - 1);
                double tt = t0+x*(t1-t0);
                Vector3d vec = path.TangentAt(tt);
                Point3d p = path.PointAt(tt);
                Curve c = Rs.ChangeState(cir,(Vector3d)cirV, (Vector3d)secCirV, (Point3d)center,vec,new Vector3d(0,0,1),p);
                cs.Add(c);
            }
            Brep s1 = Brep.CreatePlanarBreps(cs[0],tolerance)[0];
            Brep s2 = Brep.CreatePlanarBreps(cs[jieshu-1], tolerance)[0];
            Brep[] b = Brep.CreateFromLoft(cs,      // 断面曲线列表
        Point3d.Unset,      // 起始点 (通常不指定)
        Point3d.Unset,      // 结束点 (通常不指定)
        LoftType.Normal,           // 放样类型
        false            // 是否封闭
    );
           
            if (b == null || b.Length <= 0) { RhinoApp.WriteLine("放样失败"); return null; }
            else
            {
                List<Brep> brepsToJoin = new List<Brep> { s1, s2, b[0] };
                Brep[] joinedBreps = Brep.JoinBreps(brepsToJoin, tolerance);
                Brep brep = joinedBreps[0];
                return brep;
            }
        }
    }

}