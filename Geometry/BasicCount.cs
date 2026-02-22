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
        

    }

}