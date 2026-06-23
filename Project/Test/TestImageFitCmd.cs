using System.IO;
using System.Linq;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rh.Cmd;
using Rh.UI;

namespace Rh.Project.Test
{
    /// <summary>
    /// 图片拟合测试命令。
    /// 根据图片灰度值生成大小不同的圆拟合原图，用户可控制精细度。
    ///
    /// 交互流程：
    ///   步骤1（仅选项）：选择拟合模式 + 输入精细度 → 预览圆阵列
    ///   完成：将圆写入文档
    ///
    /// 图片来源：Assets/Test/test_heightfield.png
    /// </summary>
    public class TestImageFitCmd : UICommand
    {
        public override string EnglishName => "RhTestImageFit";

        // ================================================================
        // 共享字段
        // ================================================================

        private int _mode;           // 拟合模式索引（0=圆拟合）
        private int _precision;      // 精细度（最短边圆数量）
        private Circle[] _circles;   // 生成的圆阵列

        // 预览缓存：避免鼠标移动时重复计算
        private int _cachedPrecision = -1;
        private Circle[] _cachedCircles;

        // ================================================================
        // 图片路径
        // ================================================================

        private static string GetImagePath()
        {
            string assemblyDir = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(assemblyDir, "Assets", "Test", "test_heightfield.png");
        }

        // ================================================================
        // 初始化
        // ================================================================

        protected override void OnInit()
        {
            _mode = 0;
            _precision = ImageCmd.GetDefaultPrecision();
            _circles = null;
            _cachedPrecision = -1;
            _cachedCircles = null;
        }

        // ================================================================
        // 步骤定义
        // ================================================================

        protected override StepDef[] DefineSteps()
        {
            return new StepDef[]
            {
                Step.Create("拟合设置", SetupOptions, ProcessOptions, PreviewOptions)
            };
        }

        // ================================================================
        // 步骤 1：选择模式 + 输入精细度
        // ================================================================

        /// <summary>
        /// 声明输入：拟合模式列表 + 精细度整数
        /// </summary>
        private void SetupOptions(InputBuilder input)
        {
            input.List("mode", "拟合模式",
                new string[] { "圆拟合", "（预留）", "（预留）" }, 0);
            input.Integer("precision", "精细度（最短边圆数量）", _precision);
        }

        /// <summary>
        /// 处理输入：调用 Command 层生成圆阵列。
        /// 预览时使用缓存避免鼠标移动导致的重复计算。
        /// </summary>
        private void ProcessOptions(InputResult result, bool isPreview)
        {
            _mode = result.Get<int>("mode");
            _precision = result.Get<int>("precision");

            // 非圆拟合模式暂不处理
            if (_mode != 0)
            {
                _circles = null;
                return;
            }

            // 预览缓存：精细度未变时复用上次结果
            if (isPreview && _precision == _cachedPrecision && _cachedCircles != null)
            {
                _circles = _cachedCircles;
                return;
            }

            string imagePath = GetImagePath();
            if (!File.Exists(imagePath))
            {
                RhinoApp.WriteLine("[RhTestImageFit] 错误：测试图片不存在");
                _circles = null;
                return;
            }

            // 调用 Command 层创建圆阵列
            _circles = ImageCmd.CreateCircleFit(imagePath, Plane.WorldXY, _precision, isPreview);

            // 更新缓存
            if (isPreview)
            {
                _cachedPrecision = _precision;
                _cachedCircles = _circles;
            }
        }

        /// <summary>
        /// 预览：返回当前圆阵列（模板自动绘制）
        /// </summary>
        private object[] PreviewOptions()
        {
            if (_circles == null || _circles.Length == 0)
                return null;
            return _circles.Cast<object>().ToArray();
        }

        // ================================================================
        // 完成：写入文档
        // ================================================================

        protected override void OnFinish(RhinoDoc doc)
        {
            if (_circles == null || _circles.Length == 0)
            {
                RhinoApp.WriteLine("[RhTestImageFit] 无可写入的圆");
                return;
            }

            // 创建图层 Test::ImageFit::Circle
            int layerIndex = GetOrCreateLayer(doc, "Test", "ImageFit", "Circle");

            var attributes = new ObjectAttributes { LayerIndex = layerIndex };
            foreach (Circle circle in _circles)
                doc.Objects.AddCircle(circle, attributes);

            RhinoApp.WriteLine("[RhTestImageFit] 生成 {0} 个圆", _circles.Length);
        }

        // ================================================================
        // 图层创建辅助
        // ================================================================

        /// <summary>
        /// 获取或创建三级图层（如 Test::ImageFit::Circle）。
        /// </summary>
        private static int GetOrCreateLayer(RhinoDoc doc, string root, string mid, string leaf)
        {
            string fullPath = $"{root}::{mid}::{leaf}";

            int index = doc.Layers.FindByFullPath(fullPath, true);
            if (index >= 0)
                return index;

            // 创建根层
            int rootIndex = doc.Layers.FindByFullPath(root, true);
            if (rootIndex < 0)
            {
                rootIndex = doc.Layers.Add(new Layer { Name = root });
            }

            // 创建中层
            string midPath = $"{root}::{mid}";
            int midIndex = doc.Layers.FindByFullPath(midPath, true);
            if (midIndex < 0)
            {
                midIndex = doc.Layers.Add(new Layer
                {
                    Name = mid,
                    ParentLayerId = doc.Layers[rootIndex].Id
                });
            }

            // 创建叶子层
            return doc.Layers.Add(new Layer
            {
                Name = leaf,
                ParentLayerId = doc.Layers[midIndex].Id
            });
        }
    }
}
