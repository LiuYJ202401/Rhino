using System;
using System.IO;
using Rhino;
using Rhino.Geometry;
using Rh.Data;
using Rh.Geo.Img;

namespace Rh.Cmd
{
    /// <summary>
    /// 图片命令。封装图片加载、灰度采样、几何生成的完整流程。
    /// 对应接口文档：Image.md
    ///
    /// 规则：
    ///   - 不直接与 RhinoDoc 交互（不调用 doc.Objects.AddXxx）
    ///   - 不获取用户输入
    ///   - 是唯一直接调用 DataReader 的层
    /// </summary>
    public static class ImageCmd
    {
        // ================================================================
        // 内部常量与工具方法
        // ================================================================

        private const string DataPath = "Command/basicCommand/Image.json";

        private static void UpdateDefault<T>(string key, T value)
        {
            DataReader.SetValue<T>(DataPath, key, value);
        }

        private static T GetDefault<T>(string key, T defaultValue)
        {
            return DataReader.GetValue<T>(DataPath, key, defaultValue);
        }

        // ================================================================
        // 默认值查询（供 Project 层初始化 UI）
        // ================================================================

        /// <summary>查询精细度默认值，供 Project 层初始化 UI 选项</summary>
        public static int GetDefaultPrecision()
        {
            return GetDefault("CreateCircleFit.precision", 30);
        }

        // ================================================================
        // CreateCircleFit
        // ================================================================

        /// <summary>
        /// 根据图片灰度值生成大小不同的圆阵列，拟合原图。
        /// 用户只需提供图片路径、放置平面和精细度，其余参数自动推导。
        ///
        /// 自动计算逻辑：
        ///   1. 读取图片尺寸 → 推导采样网格（保持宽高比）
        ///   2. 物理尺寸 = 像素 × unitSize（保持图片比例）
        ///   3. 调用 Geometry 层采样 + 生成圆
        /// </summary>
        /// <param name="imagePath">图片文件路径</param>
        /// <param name="plane">放置平面</param>
        /// <param name="precision">精细度（最短边的圆数量）</param>
        /// <param name="isPreview">预览模式（不更新 Data）</param>
        /// <returns>圆阵列；失败时输出错误消息并返回 null</returns>
        public static Circle[] CreateCircleFit(
            string imagePath, Plane plane, int precision, bool isPreview = false)
        {
            // 1. 参数校验
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                RhinoApp.WriteLine("[CreateCircleFit] 错误：图片路径不存在");
                return null;
            }

            if (precision <= 0)
            {
                RhinoApp.WriteLine("[CreateCircleFit] 错误：精细度必须大于 0");
                return null;
            }

            // 2. 读取 Data 默认值
            double unitSize = GetDefault("CreateCircleFit.unitSize", 10.0);
            double minRadiusRatio = GetDefault("CreateCircleFit.minRadiusRatio", 0.1);

            // 3. 加载图片
            System.Drawing.Bitmap bmp;
            try
            {
                bmp = new System.Drawing.Bitmap(imagePath);
            }
            catch
            {
                RhinoApp.WriteLine("[CreateCircleFit] 错误：图片加载失败");
                return null;
            }

            try
            {
                // 4. 推导采样网格（保持图片宽高比）
                int imgW = bmp.Width;
                int imgH = bmp.Height;

                int samplesX, samplesY;
                if (imgW >= imgH)
                {
                    samplesY = precision;
                    samplesX = (int)Math.Round((double)precision * imgW / imgH);
                }
                else
                {
                    samplesX = precision;
                    samplesY = (int)Math.Round((double)precision * imgH / imgW);
                }

                // 5. 物理尺寸（像素 × unitSize）
                double physWidth = imgW * unitSize;
                double physHeight = imgH * unitSize;

                // 6. Geometry 层：灰度采样
                double[,] grayMatrix = ImageGeo.SampleGrayscale(bmp, samplesX, samplesY);
                if (grayMatrix == null)
                {
                    RhinoApp.WriteLine("[CreateCircleFit] 错误：灰度采样失败");
                    return null;
                }

                // 7. Geometry 层：灰度 → 圆阵列
                Circle[] circles = ImageGeo.CreateCirclesFromGrayscale(
                    grayMatrix, plane, physWidth, physHeight, minRadiusRatio);
                if (circles == null || circles.Length == 0)
                {
                    RhinoApp.WriteLine("[CreateCircleFit] 错误：圆阵列生成失败");
                    return null;
                }

                // 8. 更新默认值
                if (!isPreview)
                    UpdateDefault("CreateCircleFit.precision", precision);

                return circles;
            }
            finally
            {
                bmp.Dispose();
            }
        }
    }
}
