using System.Collections.Generic;
using Rhino.Geometry;

namespace Rh.Geo.Img
{
    /// <summary>
    /// 图片几何工具。
    /// 将图片像素数据转换为几何对象，采样与生成解耦：
    ///   SampleGrayscale 输出灰度矩阵（通用，可被多种功能复用）
    ///   CreateCirclesFromGrayscale 只接收灰度矩阵（不依赖图片文件）
    /// </summary>
    public static class ImageGeo
    {
        // ============================================================
        // 灰度采样
        // ============================================================

        /// <summary>
        /// 将图片采样为灰度矩阵，采用单像素中心采样。
        /// 矩阵维度 [samplesX, samplesY]，值域 [0.0=黑, 1.0=白]。
        /// </summary>
        /// <param name="bmp">已加载的图片对象</param>
        /// <param name="samplesX">X 方向（图片宽度方向）采样数</param>
        /// <param name="samplesY">Y 方向（图片高度方向）采样数</param>
        /// <returns>灰度矩阵；bmp 为 null 或采样数 ≤ 0 时返回 null</returns>
        public static double[,] SampleGrayscale(
            System.Drawing.Bitmap bmp, int samplesX, int samplesY)
        {
            if (bmp == null || samplesX <= 0 || samplesY <= 0)
                return null;

            double[,] matrix = new double[samplesX, samplesY];
            double stepX = (double)bmp.Width / samplesX;
            double stepY = (double)bmp.Height / samplesY;

            for (int j = 0; j < samplesY; j++)
            {
                for (int i = 0; i < samplesX; i++)
                {
                    int px = (int)((i + 0.5) * stepX);
                    int py = (int)((j + 0.5) * stepY);
                    px = System.Math.Min(px, bmp.Width - 1);
                    py = System.Math.Min(py, bmp.Height - 1);

                    System.Drawing.Color color = bmp.GetPixel(px, py);
                    double gray = (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) / 255.0;
                    matrix[i, j] = gray;
                }
            }

            return matrix;
        }

        // ============================================================
        // 灰度 → 圆阵列
        // ============================================================

        /// <summary>
        /// 从灰度矩阵生成圆阵列（半调效果：越暗越大，越白越小或跳过）。
        /// </summary>
        /// <param name="grayMatrix">灰度矩阵（来自 SampleGrayscale 或其他来源）</param>
        /// <param name="plane">放置平面</param>
        /// <param name="physWidth">灰度矩阵对应的物理总宽度</param>
        /// <param name="physHeight">灰度矩阵对应的物理总高度</param>
        /// <param name="minRadiusRatio">最小半径比例（0~1），半径低于 maxRadius × 此值时跳过</param>
        /// <returns>圆阵列；grayMatrix 为 null 时返回 null</returns>
        public static Circle[] CreateCirclesFromGrayscale(
            double[,] grayMatrix, Plane plane,
            double physWidth, double physHeight,
            double minRadiusRatio)
        {
            if (grayMatrix == null)
                return null;

            int samplesX = grayMatrix.GetLength(0);
            int samplesY = grayMatrix.GetLength(1);

            double cellW = physWidth / samplesX;
            double cellH = physHeight / samplesY;
            double maxRadius = System.Math.Min(cellW, cellH) / 2.0;
            double minRadius = maxRadius * minRadiusRatio;

            var circles = new List<Circle>();

            for (int j = 0; j < samplesY; j++)
            {
                for (int i = 0; i < samplesX; i++)
                {
                    double gray = grayMatrix[i, j];
                    double darkness = 1.0 - gray;
                    double radius = maxRadius * darkness;

                    if (radius < minRadius)
                        continue;

                    // Y 翻转：图片 j=0 在顶部，Rhino Y 轴向上
                    double localX = i * cellW + cellW / 2.0;
                    double localY = physHeight - (j * cellH + cellH / 2.0);
                    Point3d center = plane.Origin
                        + plane.XAxis * localX
                        + plane.YAxis * localY;

                    circles.Add(new Circle(plane, center, radius));
                }
            }

            return circles.ToArray();
        }
    }
}
