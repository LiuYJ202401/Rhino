using System;
using System.Collections.Generic;
using Rhino.Geometry;
using Rh.Geo.Sld;

namespace Rh.Geo.FenghuangCenter
{
    /// <summary>
    /// 节点生成工具。
    /// 自定义算法：网格遍历 + 四边形中心计算 + 爪臂方向投影。
    /// 圆柱委托 SolidGeo.CreateCylinder，爪臂 Box 委托 SolidGeo.CreateFromBox。
    /// </summary>
    public static class JointNodeGeo
    {
        /// <summary>
        /// 在网格交点处生成圆柱节点和爪臂。
        /// </summary>
        /// <param name="division">网格数据</param>
        /// <param name="radius">圆柱节点半径</param>
        /// <param name="innerOffset">圆柱内端偏移（玻璃侧）</param>
        /// <param name="outerOffset">圆柱外端偏移（结构侧）</param>
        /// <param name="glassOuterOffset">爪臂起始偏移（玻璃外侧）</param>
        /// <param name="clawLength">爪臂长度</param>
        /// <param name="clawWidth">爪臂宽度</param>
        /// <param name="clawDepth">爪臂深度</param>
        /// <param name="tolerance">公差</param>
        /// <returns>节点 + 爪臂实体列表；单个失败时跳过</returns>
        public static List<Brep> Build(DivisionResult division,
            double radius, double innerOffset, double outerOffset, double glassOuterOffset,
            double clawLength, double clawWidth, double clawDepth, double tolerance)
        {
            var breps = new List<Brep>();

            if (division.PointGrid == null || division.NormalGrid == null)
                return breps;

            int rows = division.CurveCount;
            int cols = division.PointsPerCurve;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    try
                    {
                        BuildSingleJoint(breps, division, row, col, rows, cols,
                            radius, innerOffset, outerOffset, glassOuterOffset,
                            clawLength, clawWidth, clawDepth);
                    }
                    catch
                    {
                        // 单个节点失败时跳过
                    }
                }
            }

            return breps;
        }

        // ================================================================
        // 单个节点生成
        // ================================================================

        /// <summary>
        /// 在指定网格位置生成圆柱节点 + 4 个爪臂。
        /// </summary>
        private static void BuildSingleJoint(List<Brep> breps, DivisionResult division,
            int row, int col, int rows, int cols,
            double radius, double innerOffset, double outerOffset, double glassOuterOffset,
            double clawLength, double clawWidth, double clawDepth)
        {
            Point3d point = division.PointGrid[row, col];
            Vector3d normal = division.NormalGrid[row, col];

            if (!normal.IsValid || normal.IsZero)
                normal = Vector3d.ZAxis;
            normal.Unitize();

            // 圆柱节点轴线：innerPt → outerPt
            Point3d innerPt = point + normal * innerOffset;
            Point3d outerPt = point + normal * outerOffset;
            double axisLength = outerOffset - innerOffset;

            // 创建圆柱（委托框架 SolidGeo.CreateCylinder）
            Brep cylBrep = SolidGeo.CreateCylinder(innerPt, normal, radius, axisLength, true);
            if (cylBrep != null && cylBrep.IsValid)
                breps.Add(cylBrep);

            // 4 个爪臂：朝向相邻四边形中心
            int[,] quadOffsets = {
                { 0,  0},
                {-1,  0},
                {-1, -1},
                { 0, -1}
            };

            for (int k = 0; k < 4; k++)
            {
                BuildSingleClaw(breps, division, row, col, rows, cols,
                    quadOffsets[k, 0], quadOffsets[k, 1],
                    point, normal, glassOuterOffset, radius, clawLength, clawWidth, clawDepth);
            }
        }

        /// <summary>
        /// 生成单个爪臂 Box。
        /// </summary>
        private static void BuildSingleClaw(List<Brep> breps, DivisionResult division,
            int row, int col, int rows, int cols,
            int dRow, int dCol,
            Point3d point, Vector3d normal,
            double glassOuterOffset, double jointRadius,
            double clawLength, double clawWidth, double clawDepth)
        {
            int qi = (row + dRow + rows) % rows;
            int qj = (col + dCol + cols) % cols;
            int qiNext = (qi + 1) % rows;
            int qjNext = (qj + 1) % cols;

            // 相邻四边形中心
            Point3d quadCenter = (
                division.PointGrid[qi, qj] +
                division.PointGrid[qiNext, qj] +
                division.PointGrid[qiNext, qjNext] +
                division.PointGrid[qi, qjNext]
            ) / 4.0;

            // 爪臂方向：四边形中心到当前点的方向，投影到切平面
            Vector3d dirToQuad = quadCenter - point;
            Vector3d projected = dirToQuad - normal * (dirToQuad * normal);
            if (!projected.IsValid || projected.IsZero)
                return;
            projected.Unitize();

            // 爪臂位置
            Point3d glassOuterPt = point + normal * glassOuterOffset;
            Point3d clawStart = glassOuterPt + projected * jointRadius;
            Point3d clawEnd = glassOuterPt + projected * (jointRadius + clawLength);

            Point3d boxCenter = (clawStart + clawEnd) / 2.0;
            Vector3d clawTangent = clawEnd - clawStart;
            clawTangent.Unitize();

            // 构建 Box（委托框架 SolidGeo.CreateFromBox）
            Plane clawPlane = new Plane(boxCenter, clawTangent, normal);
            Box clawBox = new Box(clawPlane,
                new Interval(-clawLength / 2.0, clawLength / 2.0),
                new Interval(0, clawWidth),
                new Interval(-clawDepth / 2.0, clawDepth / 2.0));

            if (clawBox.IsValid)
            {
                Brep clawBrep = SolidGeo.CreateFromBox(clawBox);
                if (clawBrep != null && clawBrep.IsValid)
                    breps.Add(clawBrep);
            }
        }
    }
}
