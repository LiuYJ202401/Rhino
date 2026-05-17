using Eto.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Transactions;

namespace PublicContent
{
    public static partial class Rs
    {
        //获取

        /// <summary>
        /// 从Rhino界面中获取一条曲线
        /// </summary>
        /// <returns>Curve</returns>
        public static Curve GetACurve()
        {
            GetObject go = new GetObject();
            go.SetCommandPrompt("选择一条曲线");
            go.GeometryFilter = ObjectType.Curve;
            go.Get();
            if (go.CommandResult() != Result.Success)
                return null;

            Curve curve = go.Object(0).Curve();
            return curve;
        }

        //修改

        /// <summary>
        /// 改变实体的位置、方向
        /// </summary>
        /// <param name="ori">操作物体</param>
        /// <param name="vec">基准方向</param>
        /// <param name="secV">副方向</param>
        /// <param name="center">基点</param>
        /// <param name="goalVec">目标基准方向</param>
        /// <param name="secgoalVec">目标副方向</param>
        /// <param name="goalP">目标点</param>
        /// <returns></returns>
        public static Brep ChangeState(Brep ori, Vector3d vec, Vector3d secV, Point3d center, Vector3d goalVec, Vector3d secgoalVec, Point3d goalP) {
            Brep A = ori.DuplicateBrep();
            

            if (Vector3d.CrossProduct(vec, secV).IsZero)
            {
                RhinoApp.WriteLine("错误：源向量共线");
                return null;
            }
            if (Vector3d.CrossProduct(goalVec, secgoalVec).IsZero)
            {
                RhinoApp.WriteLine("错误：目标向量共线");
                return null;
            }
            if (Math.Abs(goalVec * secgoalVec) != 0) {
                Vector3d vec3 = Vector3d.CrossProduct(goalVec, secgoalVec);
                secgoalVec = Vector3d.CrossProduct(vec3,goalVec);
            }
            vec.Unitize();
            goalVec.Unitize();
            secV.Unitize();
            secgoalVec.Unitize();

            Plane pl1 = new Plane(center, vec, secV);
            Plane pl2 = new Plane(goalP, goalVec, secgoalVec);
            Transform move = Transform.PlaneToPlane(pl1, pl2);
            A.Transform(move);
            return A;
        }
        /// <summary>
        /// 改变曲线的位置、方向
        /// </summary>
        /// <param name="ori">操作曲线</param>
        /// <param name="vec">基准方向</param>
        /// <param name="secV">副方向</param>
        /// <param name="center">基点</param>
        /// <param name="goalVec">目标基准方向</param>
        /// <param name="secgoalVec">目标副方向</param>
        /// <param name="goalP">目标点</param>
        /// <returns></returns>
        public static Curve ChangeState(Curve ori, Vector3d vec,Vector3d secV, Point3d center, Vector3d goalVec,Vector3d secgoalVec, Point3d goalP)
        {
            Curve A = ori.DuplicateCurve();

            if (Vector3d.CrossProduct(vec, secV).IsZero)
            {
                RhinoApp.WriteLine("错误：源向量共线");
                return null;
            }
            if (Vector3d.CrossProduct(goalVec, secgoalVec).IsZero)
            {
                RhinoApp.WriteLine("错误：目标向量共线");
                return null;
            }
            if (Math.Abs(goalVec * secgoalVec) != 0)
            {
                Vector3d vec3 = Vector3d.CrossProduct(goalVec, secgoalVec);
                secgoalVec = Vector3d.CrossProduct(vec3, goalVec);
            }
            vec.Unitize();
            goalVec.Unitize();
            secV.Unitize();
            secgoalVec.Unitize();

            Plane pl1 = new Plane(center, vec, secV);
            Plane pl2 = new Plane(goalP, goalVec, secgoalVec);
            Transform move = Transform.PlaneToPlane(pl1,pl2);
            A.Transform(move);
            return A;
        }

        // 建模
        /// <summary>
        /// 创建一个挤出物体
        /// </summary>
        /// <param name="tolerance">doc.ModelAbsoluteTolerance</param>
        /// <param name="cur">要挤出的闭合曲线</param>
        /// <param name="vec">挤出方向</param>
        /// <returns></returns>
        public static Brep MakeAExtrudedBrep(double tolerance,Curve cur, Vector3d vec) {
            Brep s1 = Brep.CreatePlanarBreps(cur,tolerance)[0];
            Brep sur=Surface.CreateExtrusion(cur,vec).ToBrep();
            Transform t = Transform.Translation(vec);
            cur.Transform(t);
            Brep s2 = Brep.CreatePlanarBreps(cur, tolerance)[0];
            List<Brep> brepsToJoin = new List<Brep> { s1, s2, sur };
            Brep[] joinedBreps = Brep.JoinBreps(brepsToJoin, tolerance);
            Brep brep = joinedBreps[0];
            return brep;
        }

        /// <summary>
        /// 绘制一个胶囊状曲线，平面垂直Z轴
        /// </summary>
        /// <param name="tolerance">doc.ModelAbsoluteTolerance</param>
        /// <param name="Length">胶囊中部的长度</param>
        /// <param name="R">胶囊圆形部分的班级</param>
        /// <param name="center">中心点坐标</param>
        /// <returns></returns>
        public static Curve MakeACapsule(double tolerance, double Length, double R, Point3d? center = null) {
            if (center == null) { center = new Point3d(0, 0, 0); }
            double half = Length / 2;
            Point3d p1 = new Point3d(center.Value.X - half, center.Value.Y + R, center.Value.Z);
            Point3d p2 = new Point3d(center.Value.X + half, center.Value.Y + R, center.Value.Z);
            Point3d p3 = new Point3d(center.Value.X + half + R, center.Value.Y, center.Value.Z);
            Point3d p4 = new Point3d(center.Value.X + half, center.Value.Y - R, center.Value.Z);
            Point3d p5 = new Point3d(center.Value.X - half, center.Value.Y - R, center.Value.Z);
            Point3d p6 = new Point3d(center.Value.X - half - R, center.Value.Y, center.Value.Z);

            LineCurve topLine = new LineCurve(p1, p2);
            LineCurve botLine = new LineCurve(p4, p5);
            Arc rightArc = new Arc(p2, p3, p4);
            ArcCurve rightArcCurve = new ArcCurve(rightArc);
            Arc leftArc = new Arc(p5, p6, p1);
            ArcCurve leftArcCurve = new ArcCurve(leftArc);
            List<Curve> curs = new List<Curve>{topLine, rightArcCurve, botLine, leftArcCurve};
            Curve[] joined = Curve.JoinCurves(curs,tolerance);
            Curve cur=joined[0];
            return cur;
        }

    }

}