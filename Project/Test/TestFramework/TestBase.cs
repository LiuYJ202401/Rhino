using System.Collections.Generic;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rh.Cmd;

namespace Rh.Project.Test.Framework
{
    /// <summary>
    /// 测试链命令基类。管理图层创建、结果写入和统计输出。
    /// 每条测试链继承此类，在 RunCommand 中按步骤调用 Command 层方法。
    /// </summary>
    public abstract class TestBase : Rhino.Commands.Command
    {
        // ================================================================
        // 统计
        // ================================================================

        protected int TotalSteps { get; private set; }
        protected int PassedSteps { get; private set; }
        protected int FailedSteps { get; private set; }
        protected int SkippedSteps { get; private set; }

        // ================================================================
        // 图层缓存
        // ================================================================

        private readonly Dictionary<string, int> _layerIndexCache = new Dictionary<string, int>();

        // ================================================================
        // 步骤执行辅助
        // ================================================================

        /// <summary>
        /// 执行单个测试步骤。记录 PASS/FAIL，但不中断后续步骤。
        /// </summary>
        protected void Step(string methodName, System.Action action)
        {
            TotalSteps++;
            RhinoApp.Write($"\n  [{TotalSteps}] {methodName} ... ");
            try
            {
                action();
                PassedSteps++;
                RhinoApp.WriteLine("PASS");
            }
            catch (AssertFailedException ex)
            {
                FailedSteps++;
                RhinoApp.WriteLine($"FAIL ({ex.Message})");
            }
            catch (System.Exception ex)
            {
                FailedSteps++;
                RhinoApp.WriteLine($"FAIL (异常: {ex.Message})");
            }
        }

        /// <summary>
        /// 跳过步骤（如缺少依赖文件）。
        /// </summary>
        protected void Skip(string methodName, string reason)
        {
            TotalSteps++;
            SkippedSteps++;
            RhinoApp.WriteLine($"\n  [{TotalSteps}] {methodName} ... SKIP ({reason})");
        }

        // ================================================================
        // 图层管理
        // ================================================================

        /// <summary>
        /// 获取或创建子图层。格式："Test::Chain1::Point"。
        /// </summary>
        protected int GetOrCreateLayer(string chainName, string subName)
        {
            string fullPath = $"Test::{chainName}::{subName}";

            if (_layerIndexCache.TryGetValue(fullPath, out int cachedIndex))
                return cachedIndex;

            var doc = RhinoDoc.ActiveDoc;
            int index = doc.Layers.FindByFullPath(fullPath, true);

            if (index < 0)
            {
                // 创建父层 Test
                int parentIndex = doc.Layers.FindByFullPath("Test", true);
                if (parentIndex < 0)
                {
                    var testLayer = new Layer { Name = "Test" };
                    parentIndex = doc.Layers.Add(testLayer);
                }

                // 创建 Chain 层
                string chainPath = $"Test::{chainName}";
                int chainIndex = doc.Layers.FindByFullPath(chainPath, true);
                if (chainIndex < 0)
                {
                    var chainLayer = new Layer { Name = chainName, ParentLayerId = doc.Layers[parentIndex].Id };
                    chainIndex = doc.Layers.Add(chainLayer);
                }

                // 创建子层
                var subLayer = new Layer { Name = subName, ParentLayerId = doc.Layers[chainIndex].Id };
                index = doc.Layers.Add(subLayer);
            }

            _layerIndexCache[fullPath] = index;
            return index;
        }

        // ================================================================
        // 几何写入辅助
        // ================================================================

        /// <summary>将几何对象写入文档，指定图层</summary>
        protected void WriteToDoc(GeometryBase geo, string chainName, string subName)
        {
            if (geo == null) return;
            int layerIndex = GetOrCreateLayer(chainName, subName);
            var attributes = new ObjectAttributes { LayerIndex = layerIndex };
            RhinoDoc.ActiveDoc.Objects.Add(geo, attributes);
        }

        /// <summary>将点写入文档</summary>
        protected void WritePoint(Point3d pt, string chainName, string subName)
        {
            int layerIndex = GetOrCreateLayer(chainName, subName);
            var attributes = new ObjectAttributes { LayerIndex = layerIndex };
            RhinoDoc.ActiveDoc.Objects.AddPoint(pt, attributes);
        }

        /// <summary>将点列表写入文档</summary>
        protected void WritePoints(IEnumerable<Point3d> pts, string chainName, string subName)
        {
            int layerIndex = GetOrCreateLayer(chainName, subName);
            var attributes = new ObjectAttributes { LayerIndex = layerIndex };
            foreach (var pt in pts)
                RhinoDoc.ActiveDoc.Objects.AddPoint(pt, attributes);
        }

        // ================================================================
        // 汇总输出
        // ================================================================

        /// <summary>在 RunCommand 结尾调用，输出统计并刷新视口</summary>
        protected void Finish(string chainName)
        {
            RhinoApp.WriteLine($"\n  ────────────────────────────────");
            RhinoApp.WriteLine($"  {chainName} 完成: {PassedSteps} 通过, {FailedSteps} 失败, {SkippedSteps} 跳过, 共 {TotalSteps} 步");
            RhinoApp.WriteLine($"  ────────────────────────────────\n");

            RhinoDoc.ActiveDoc.Views.Redraw();
        }

        // ================================================================
        // 默认容差
        // ================================================================

        protected static double Tol => RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
    }
}
