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

namespace RhinoTrial
{
    public class FirstTrial_Command : Command
    {
        public FirstTrial_Command()
        {
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static FirstTrial_Command Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "hello_Rhino8";///输入的命令名称

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // 开始！
            Curve A = Rs.GetACurve();
            int PostNum = 5;
            double Height = 1000;
            double Width = 500;
            double Length = 200;
            Box b = new Box(Plane.WorldXY,
                new Interval(-Length / 2, Length / 2),
                new Interval(-Width / 2, Width / 2),
                new Interval(0,Height)
                );
            Brep B = b.ToBrep();
            Point3d pt=new Point3d(Length/2,0,0);
            Vector3d vec = new Vector3d(0,1,0);

            List<Brep> bl = Rs.BrepsAlongCurve(A, B, PostNum, vec, pt,delta:100,reverse:true,headmove:500);
            for (int i = 0; i < bl.Count; i++) { doc.Objects.AddBrep(bl[i]); }
            



            return Result.Success;

            //RhinoApp.WriteLine("The {0} command will add a line right now.", EnglishName);

            //Point3d pt0;
            //using (GetPoint getPointAction = new GetPoint())
            //{
            //    getPointAction.SetCommandPrompt("Please select the start point");
            //    if (getPointAction.Get() != GetResult.Point)
            //    {
            //        RhinoApp.WriteLine("No start point was selected.");
            //        return getPointAction.CommandResult();
            //    }
            //    pt0 = getPointAction.Point();
            //}

            //Point3d pt1;
            //using (GetPoint getPointAction = new GetPoint())
            //{
            //    getPointAction.SetCommandPrompt("Please select the end point");
            //    getPointAction.SetBasePoint(pt0, true);
            //    getPointAction.DynamicDraw +=
            //      (sender, e) => e.Display.DrawLine(pt0, e.CurrentPoint, System.Drawing.Color.DarkRed);
            //    if (getPointAction.Get() != GetResult.Point)
            //    {
            //        RhinoApp.WriteLine("No end point was selected.");
            //        return getPointAction.CommandResult();
            //    }
            //    pt1 = getPointAction.Point();
            //}

            //doc.Objects.AddLine(pt0, pt1);
            //doc.Views.Redraw();
            //RhinoApp.WriteLine("The {0} command added one line to the document.", EnglishName);

            //// ---
            //return Result.Success;
        }
    }
}
