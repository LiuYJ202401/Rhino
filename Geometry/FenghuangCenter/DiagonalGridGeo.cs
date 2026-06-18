using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace Rh.Geo.FenghuangCenter
{
    /// <summary>
    /// 对角网格划分核心算法。
    /// 直接在曲面参数域上采样点网格，用曲面真实法线（NormalAt）保证方向全局一致。
    /// 框架无等价能力，全部自定义。
    /// </summary>
    public static class DiagonalGridGeo
    {
        /// <summary>
        /// 在曲面参数域上划分网格，生成对角网格数据。
        /// 直接用 surface.PointAt 采样点、surface.NormalAt 计算法线，
        /// 确保闭合曲面（如甜甜圈）接缝处法线方向一致，避免构件穿模。
        /// </summary>
        /// <param name="surface">目标曲面</param>
        /// <param name="curveCount">rows 方向采样数（≥2）</param>
        /// <param name="divisionCount">cols 方向划分段数（≥2）</param>
        /// <param name="useVDirection">true=rows 沿 U 域、cols 沿 V 域；false=反过来</param>
        /// <param name="closedLoop">对角曲线是否按闭合环路生成</param>
        /// <param name="flipNormals">是否反转法线（控制构件在曲面哪一侧）</param>
        /// <returns>DivisionResult；失败时 LinesA/LinesB 为空</returns>
        public static DivisionResult Divide(
            Surface surface, int curveCount, int divisionCount,
            bool useVDirection, bool closedLoop, bool flipNormals)
        {
            var result = new DivisionResult();

            if (surface == null || !surface.IsValid)
                return result;
            if (curveCount < 2 || divisionCount < 2)
                return result;

            // 确定 rows/cols 对应的参数方向与闭合性
            int rowDir = useVDirection ? 0 : 1;
            int colDir = useVDirection ? 1 : 0;
            Interval rowDomain = surface.Domain(rowDir);
            Interval colDomain = surface.Domain(colDir);
            bool rowClosed = surface.IsClosed(rowDir);
            bool colClosed = surface.IsClosed(colDir);

            int rows = curveCount;
            // 闭合方向不重复采样端点（T0 与 T1 重合），非闭合方向包含两端
            int cols = colClosed ? divisionCount : divisionCount + 1;

            // 对角曲线闭合约束：当 rows 方向闭合时，对角曲线走 rows 步后必须回到起点
            // 数学条件：(rows * step) mod cols == 0，对于 step=±1 简化为 rows mod cols == 0
            // 若不满足，对角曲线实际是螺旋的，强制闭合会导致接缝处扭转穿模
            // 自动调整 cols 为 rows 的最接近因子，保证环绕一周后精确对齐
            if (rowClosed && cols > 0 && rows % cols != 0)
            {
                cols = FindClosestFactor(rows, cols);
            }

            // 等间距采样参数
            double[] rowParams = SampleParams(rowDomain, rows, rowClosed);
            double[] colParams = SampleParams(colDomain, cols, colClosed);

            // 构建点网格 + 法线网格
            // 直接从曲面采样，确保法线方向全局一致（解决闭合曲面接缝处穿模）
            var pointGrid = new Point3d[rows, cols];
            var normalGrid = new Vector3d[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double u = useVDirection ? rowParams[i] : colParams[j];
                    double v = useVDirection ? colParams[j] : rowParams[i];
                    pointGrid[i, j] = surface.PointAt(u, v);
                    normalGrid[i, j] = surface.NormalAt(u, v);
                }
            }

            // 反转法线：将全部构件翻到曲面另一侧（仍保持同一侧）
            if (flipNormals)
            {
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j++)
                        normalGrid[i, j].Reverse();
            }

            result.PointGrid = pointGrid;
            result.NormalGrid = normalGrid;
            result.CurveCount = rows;
            result.PointsPerCurve = cols;

            // 生成对角曲线
            result.LinesA = GenerateDiagonalCurves(pointGrid, rows, cols, +1, closedLoop);
            result.LinesB = GenerateDiagonalCurves(pointGrid, rows, cols, -1, closedLoop);

            // 扁平化交点（用于节点生成）
            ComputeIntersectionsFromGrid(result);

            // 构建四边形和边
            BuildQuadsFromGrid(result);
            BuildEdgesFromGrid(result);

            return result;
        }

        // ================================================================
        // 私有方法
        // ================================================================

        /// <summary>
        /// 在参数域上等间距采样。
        /// 闭合域：不重复端点（i/Count * Length）。
        /// 非闭合域：包含两端（i/(Count-1) * Length）。
        /// </summary>
        private static double[] SampleParams(Interval domain, int count, bool closed)
        {
            var ps = new double[count];
            for (int i = 0; i < count; i++)
            {
                ps[i] = closed
                    ? domain.T0 + (double)i / count * domain.Length
                    : domain.T0 + (double)i / (count - 1) * domain.Length;
            }
            return ps;
        }

        /// <summary>
        /// 找 n 的最接近 target 的因子（含 1 和 n 自身）。
        /// 用于对角闭合约束：调整 cols 使 rows mod cols == 0。
        /// 算法：遍历 2..sqrt(n)，对每个因子 f 同时考虑配对因子 n/f。
        /// </summary>
        private static int FindClosestFactor(int n, int target)
        {
            if (target < 1) target = 1;
            int best = 1;
            int bestDiff = Math.Abs(1 - target);

            void Consider(int f)
            {
                int diff = Math.Abs(f - target);
                if (diff < bestDiff) { best = f; bestDiff = diff; }
            }

            for (int f = 2; f * f <= n; f++)
            {
                if (n % f != 0) continue;
                Consider(f);
                Consider(n / f);
            }
            Consider(n);  // n 自身也是因子
            return best;
        }

        /// <summary>
        /// 生成对角曲线。
        /// step=+1 为方向 A，step=-1 为方向 B。
        /// </summary>
        private static List<Curve> GenerateDiagonalCurves(Point3d[,] pointGrid, int rows, int cols, int step, bool closedLoop)
        {
            var curves = new List<Curve>();

            for (int startJ = 0; startJ < cols; startJ++)
            {
                var points = new List<Point3d>();

                for (int i = 0; i < rows; i++)
                {
                    int j = ((startJ + i * step) % cols + cols) % cols;
                    points.Add(pointGrid[i, j]);
                }

                if (points.Count >= 3)
                {
                    Curve curve;
                    if (closedLoop)
                    {
                        // 闭合环路：尝试周期插值曲线
                        curve = NurbsCurve.CreateInterpolatedCurve(points, 3, CurveKnotStyle.UniformPeriodic);
                        if (curve == null || !curve.IsValid || !curve.IsClosed)
                        {
                            // 回退：追加首点闭合
                            var closedPoints = new List<Point3d>(points) { points[0] };
                            curve = Curve.CreateInterpolatedCurve(closedPoints, 3);
                        }
                    }
                    else
                    {
                        curve = Curve.CreateInterpolatedCurve(points, 3);
                    }

                    if (curve != null && curve.IsValid)
                        curves.Add(curve);
                }
                else if (points.Count == 2)
                {
                    curves.Add(new LineCurve(points[0], points[1]));
                }
            }

            return curves;
        }

        /// <summary>
        /// 将点网格扁平化为交点列表（用于节点生成）。
        /// </summary>
        private static void ComputeIntersectionsFromGrid(DivisionResult result)
        {
            for (int i = 0; i < result.CurveCount; i++)
            {
                for (int j = 0; j < result.PointsPerCurve; j++)
                {
                    result.IntersectionPoints.Add(result.PointGrid[i, j]);
                    result.IntersectionNormals.Add(result.NormalGrid[i, j]);
                }
            }
        }

        /// <summary>
        /// 由相邻 4 个网格点构建四边形。
        /// </summary>
        private static void BuildQuadsFromGrid(DivisionResult result)
        {
            int rows = result.CurveCount;
            int cols = result.PointsPerCurve;
            var grid = result.PointGrid;
            var normals = result.NormalGrid;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int iNext = (i + 1) % rows;
                    int jNext = (j + 1) % cols;

                    result.Quads.Add(new Quad
                    {
                        CornerPoints = new Point3d[]
                        {
                            grid[i, j],
                            grid[iNext, j],
                            grid[iNext, jNext],
                            grid[i, jNext]
                        },
                        CornerNormals = new Vector3d[]
                        {
                            normals[i, j],
                            normals[iNext, j],
                            normals[iNext, jNext],
                            normals[i, jNext]
                        }
                    });
                }
            }
        }

        /// <summary>
        /// 由相邻网格点构建边（每个网格点向 i+1 和 j+1 方向各生成一条边）。
        /// </summary>
        private static void BuildEdgesFromGrid(DivisionResult result)
        {
            int rows = result.CurveCount;
            int cols = result.PointsPerCurve;
            var grid = result.PointGrid;
            var normals = result.NormalGrid;

            for (int i = 0; i < rows; i++)
            {
                int iNext = (i + 1) % rows;
                for (int j = 0; j < cols; j++)
                {
                    int jNext = (j + 1) % cols;

                    result.Edges.Add(new GridEdge
                    {
                        Start = grid[i, j],
                        End = grid[iNext, j],
                        StartNormal = normals[i, j],
                        EndNormal = normals[iNext, j]
                    });

                    result.Edges.Add(new GridEdge
                    {
                        Start = grid[i, j],
                        End = grid[i, jNext],
                        StartNormal = normals[i, j],
                        EndNormal = normals[i, jNext]
                    });
                }
            }
        }
    }
}
