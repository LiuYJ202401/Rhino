using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace Rh.Cmd
{
    /// <summary>
    /// Point 基础命令（点与点云的创建）。
    /// 对应接口文档：Command/basicCommand/Point.md
    /// 对应默认值：Data/Command/basicCommand/Point.json
    /// </summary>
    public static class PointCmd
    {
        // ================================================================
        // CreatePoint
        // ================================================================

        /// <summary>
        /// 在指定位置创建单个点（重载：Point3d）。
        /// 对应 Rhino 命令：Point
        /// RhinoCommon：Point3d 本身即为坐标值，无需构造
        /// </summary>
        /// <param name="point">三维坐标</param>
        /// <param name="isPreview">是否为预览模式</param>
        /// <returns>创建的点；无效时返回 Point3d.Unset</returns>
        public static Point3d CreatePoint(Point3d point, bool isPreview = false)
        {
            if (!point.IsValid)
                return Point3d.Unset;

            return point;
        }

        /// <summary>
        /// 在指定位置创建单个点（重载：三分量坐标）。
        /// 对应 Rhino 命令：Point
        /// RhinoCommon：new Point3d(x, y, z)
        /// </summary>
        /// <param name="x">X 坐标</param>
        /// <param name="y">Y 坐标</param>
        /// <param name="z">Z 坐标</param>
        /// <param name="isPreview">是否为预览模式</param>
        /// <returns>创建的点；NaN 或无穷大时返回 Point3d.Unset</returns>
        public static Point3d CreatePoint(double x, double y, double z, bool isPreview = false)
        {
            var point = new Point3d(x, y, z);

            if (!point.IsValid)
                return Point3d.Unset;

            return point;
        }

        // ================================================================
        // CreatePoints
        // ================================================================

        /// <summary>
        /// 创建多个独立点（过滤无效点）。
        /// 对应 Rhino 命令：Points
        /// </summary>
        /// <param name="points">点集合</param>
        /// <param name="isPreview">是否为预览模式</param>
        /// <returns>有效点列表；空集合返回空列表</returns>
        public static List<Point3d> CreatePoints(IEnumerable<Point3d> points, bool isPreview = false)
        {
            var result = new List<Point3d>();

            if (points == null)
                return result;

            foreach (Point3d pt in points)
            {
                if (pt.IsValid)
                    result.Add(pt);
            }

            return result;
        }

        // ================================================================
        // CreatePointGrid
        // ================================================================

        /// <summary>
        /// 在指定平面创建矩形点阵。
        /// 对应 Rhino 命令：PointGrid
        /// 无直接 RhinoCommon 构造，手动嵌套循环生成。
        /// </summary>
        /// <param name="plane">所在平面</param>
        /// <param name="xCount">X 方向点数</param>
        /// <param name="yCount">Y 方向点数</param>
        /// <param name="xDomain">X 方向范围</param>
        /// <param name="yDomain">Y 方向范围</param>
        /// <param name="isPreview">是否为预览模式</param>
        /// <returns>点云（xCount × yCount 个点）；参数无效时返回 null</returns>
        public static PointCloud CreatePointGrid(
            Plane plane,
            int xCount,
            int yCount,
            Interval xDomain,
            Interval yDomain,
            bool isPreview = false)
        {
            if (xCount < 1 || yCount < 1)
                return null;

            // 计算步长（点数-1为间隔数）
            double xStep = 0;
            double yStep = 0;

            if (xCount > 1)
                xStep = xDomain.Length / (xCount - 1);

            if (yCount > 1)
                yStep = yDomain.Length / (yCount - 1);

            var cloud = new PointCloud();

            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    double x = xDomain.T0 + i * xStep;
                    double y = yDomain.T0 + j * yStep;

                    // 将平面坐标转换为世界坐标
                    Point3d worldPt = plane.PointAt(x, y);
                    cloud.Add(worldPt);
                }
            }

            return cloud;
        }

        // ================================================================
        // CreatePointCloud
        // ================================================================

        /// <summary>
        /// 将多个点组合为点云对象。
        /// 对应 Rhino 命令：PointCloud
        /// RhinoCommon：new PointCloud(IEnumerable&lt;Point3d&gt;)
        /// </summary>
        /// <param name="points">点集合</param>
        /// <param name="isPreview">是否为预览模式</param>
        /// <returns>点云对象；空集合返回 null</returns>
        public static PointCloud CreatePointCloud(IEnumerable<Point3d> points, bool isPreview = false)
        {
            if (points == null)
                return null;

            var pointList = new List<Point3d>(points);

            if (pointList.Count == 0)
                return null;

            // 过滤无效点
            pointList.RemoveAll(p => !p.IsValid);

            if (pointList.Count == 0)
                return null;

            return new PointCloud(pointList);
        }

        // ================================================================
        // CreatePointCloudFromMesh
        // ================================================================

        /// <summary>
        /// 从网格顶点创建点云。
        /// 对应 Rhino 命令：PointCloud（选择网格时）
        /// RhinoCommon：mesh.Vertices.ToPoint3dArray()
        /// </summary>
        /// <param name="mesh">源网格</param>
        /// <param name="isPreview">是否为预览模式</param>
        /// <returns>点云对象；mesh 无效时返回 null</returns>
        public static PointCloud CreatePointCloudFromMesh(Mesh mesh, bool isPreview = false)
        {
            if (mesh == null)
                return null;

            Point3d[] vertices = mesh.Vertices.ToPoint3dArray();

            if (vertices.Length == 0)
                return null;

            return new PointCloud(vertices);
        }

        // ================================================================
        // AddPointsToCloud
        // ================================================================

        /// <summary>
        /// 向已有点云添加点（返回新点云，原点云不变）。
        /// 对应 Rhino 命令：PointCloud > Add
        /// </summary>
        /// <param name="cloud">目标点云</param>
        /// <param name="points">待添加点</param>
        /// <param name="isPreview">是否为预览模式</param>
        /// <returns>新点云；cloud 或 points 为 null 时返回原 cloud</returns>
        public static PointCloud AddPointsToCloud(
            PointCloud cloud,
            IEnumerable<Point3d> points,
            bool isPreview = false)
        {
            if (cloud == null || points == null)
                return cloud;

            // 重建点云（PointCloud 无 Clone 方法）
            var result = new PointCloud();

            for (int i = 0; i < cloud.Count; i++)
                result.Add(cloud[i].Location);

            foreach (Point3d pt in points)
            {
                if (pt.IsValid)
                    result.Add(pt);
            }

            return result;
        }

        // ================================================================
        // RemovePointsFromCloud
        // ================================================================

        /// <summary>
        /// 从点云中移除指定索引的点（返回新点云，原点云不变）。
        /// 对应 Rhino 命令：PointCloud > Remove
        /// 注意：从后往前移除以避免索引偏移。
        /// </summary>
        /// <param name="cloud">目标点云</param>
        /// <param name="indices">待移除点索引</param>
        /// <param name="isPreview">是否为预览模式</param>
        /// <returns>新点云；cloud 为 null 时返回 null</returns>
        public static PointCloud RemovePointsFromCloud(
            PointCloud cloud,
            IEnumerable<int> indices,
            bool isPreview = false)
        {
            if (cloud == null)
                return null;

            // 排序并去重，从大到小排序以便从后往前移除
            var sortedIndices = new SortedSet<int>(indices, Comparer<int>.Create((a, b) => b.CompareTo(a)));

            // 重建点云（PointCloud 无 Clone 方法）
            var result = new PointCloud();

            for (int i = 0; i < cloud.Count; i++)
                result.Add(cloud[i].Location);

            int count = result.Count;

            foreach (int index in sortedIndices)
            {
                if (index >= 0 && index < count)
                    result.RemoveAt(index);
            }

            return result;
        }

        // ================================================================
        // ReducePointCloud
        // ================================================================

        /// <summary>
        /// 从点云中随机删除指定数量的点（抽稀）。
        /// 对应 Rhino 命令：ReducePointCloud
        /// </summary>
        /// <param name="cloud">源点云</param>
        /// <param name="removeCount">要删除的点数</param>
        /// <param name="isPreview">是否为预览模式</param>
        /// <returns>抽稀后的点云；参数无效时返回原 cloud 副本</returns>
        public static PointCloud ReducePointCloud(
            PointCloud cloud,
            int removeCount,
            bool isPreview = false)
        {
            if (cloud == null)
                return null;

            if (removeCount < 0 || removeCount >= cloud.Count)
            {
                // 重建点云副本
                var copy = new PointCloud();
                for (int i = 0; i < cloud.Count; i++)
                    copy.Add(cloud[i].Location);
                return copy;
            }

            // 生成全部索引，随机选取 removeCount 个
            var allIndices = new List<int>();
            for (int i = 0; i < cloud.Count; i++)
                allIndices.Add(i);

            // 简单随机抽取（Fisher-Yates 部分洗牌）
            var random = new System.Random();
            for (int i = 0; i < removeCount; i++)
            {
                int j = random.Next(i, allIndices.Count);
                int temp = allIndices[i];
                allIndices[i] = allIndices[j];
                allIndices[j] = temp;
            }

            // 前 removeCount 个是要移除的
            var indicesToRemove = allIndices.GetRange(0, removeCount);

            return RemovePointsFromCloud(cloud, indicesToRemove, isPreview);
        }
    }
}
