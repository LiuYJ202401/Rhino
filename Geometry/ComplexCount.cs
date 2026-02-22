using Eto.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;

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
            if (len > headmove + tailmove) { len -= headmove + tailmove; }
            //初始化输入内容
            for (int i = 0; i < num; i++) {
                double t;
                cur.LengthParameter(headmove+len*i/(num-1),out t);//等长得到参数t
                Point3d p= cur.PointAt(t);//曲线上点
                Vector3d tangent = cur.TangentAt(t);//切线方向
                Vector3d pVec = new Vector3d(tangent.X, tangent.Y, 0);
                pVec.Unitize();
                double theta = Vector3d.VectorAngle(pVec, vec,Plane.WorldXY);
                Brep nb = b.DuplicateBrep();
                Transform rot = Transform.Rotation(-theta, Vector3d.ZAxis, pt);
                nb.Transform(rot);
                //绕pt点Z轴旋转theta弧度
                Vector3d moveVec = (p - pt);
                if (delta != 0) { Vector3d d = new Vector3d(-pVec[1], pVec[0], 0); moveVec += d*delta; }
                Transform move = Transform.Translation(moveVec);
                nb.Transform(move);
                //移动
                breps.Add(nb);
            }
            return breps;
        }
    }

}