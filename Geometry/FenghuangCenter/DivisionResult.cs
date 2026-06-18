using System.Collections.Generic;
using Rhino.Geometry;

namespace Rh.Geo.FenghuangCenter
{
    /// <summary>
    /// 四边形面板数据：4 个角点 + 对应法线。
    /// 角点顺序排列（P0→P1→P2→P3 形成闭合环路）。
    /// </summary>
    public class Quad
    {
        /// <summary>四边形角点（长度 4）</summary>
        public Point3d[] CornerPoints { get; set; }

        /// <summary>角点处法线（长度 4，与 CornerPoints 一一对应）</summary>
        public Vector3d[] CornerNormals { get; set; }

        public Quad()
        {
            CornerPoints = new Point3d[4];
            CornerNormals = new Vector3d[4];
        }
    }

    /// <summary>
    /// 网格边数据：两端点 + 对应法线。
    /// </summary>
    public class GridEdge
    {
        public Point3d Start { get; set; }
        public Point3d End { get; set; }
        public Vector3d StartNormal { get; set; }
        public Vector3d EndNormal { get; set; }
    }

    /// <summary>
    /// 网格划分结果。包含点网格、法线网格、对角曲线、四边形、边。
    /// Geometry 层失败时返回空结果（LinesA/LinesB 为空），由 Command 层判断并报告。
    /// </summary>
    public class DivisionResult
    {
        /// <summary>方向 A 对角曲线</summary>
        public List<Curve> LinesA { get; set; } = new List<Curve>();

        /// <summary>方向 B 对角曲线</summary>
        public List<Curve> LinesB { get; set; } = new List<Curve>();

        /// <summary>网格交点（扁平化，用于节点生成）</summary>
        public List<Point3d> IntersectionPoints { get; set; } = new List<Point3d>();

        /// <summary>网格交点法线（与 IntersectionPoints 一一对应）</summary>
        public List<Vector3d> IntersectionNormals { get; set; } = new List<Vector3d>();

        /// <summary>四边形面板列表</summary>
        public List<Quad> Quads { get; set; } = new List<Quad>();

        /// <summary>网格边列表</summary>
        public List<GridEdge> Edges { get; set; } = new List<GridEdge>();

        /// <summary>网格点二维数组 [曲线索引, 划分索引]</summary>
        public Point3d[,] PointGrid { get; set; }

        /// <summary>网格法线二维数组 [曲线索引, 划分索引]</summary>
        public Vector3d[,] NormalGrid { get; set; }

        /// <summary>曲线数量（网格第一维）</summary>
        public int CurveCount { get; set; }

        /// <summary>每条曲线的划分数（网格第二维）</summary>
        public int PointsPerCurve { get; set; }
    }
}
