using System;
using System.Collections.Generic;
using System.Drawing;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rh.Cmd;
using Rh.Geo.FenghuangCenter;
using Rh.UI;

// 别名解决与 Command 层 CurtainWallCmd 的命名冲突
using WallCommand = Rh.Cmd.CurtainWallCmd;

namespace Rh.Project.FenghuangCenter.CurtainWall
{
    /// <summary>
    /// 凤凰中心幕墙主体生成命令。
    /// 选择 Brep 曲面 → 分步设置参数 → 自动生成完整幕墙构件 → 按图层写入文档。
    ///
    /// 交互流程（7 步，每步少量参数，逐步确认）：
    ///   步骤 1 — 选择幕墙基曲面（Object 输入）
    ///   步骤 2 — 网格划分参数（曲线数、划分数、方向、闭合、反转法线）
    ///   步骤 3 — 方向 A 支撑梁（偏移、截面宽、截面深）
    ///   步骤 4 — 方向 B 支撑梁（偏移、截面宽、截面深）
    ///   步骤 5 — 节点与爪臂（节点半径、节点偏移、爪臂长/宽/深）
    ///   步骤 6 — 玻璃面板（玻璃厚度）
    ///   步骤 7 — 窗框（窗框宽、窗框深）
    ///   完成   — 按图层写入 Rhino 文档
    /// </summary>
    public class CurtainWallCmd : UICommand
    {
        public override string EnglishName => "RhFenghuangCurtainWall";

        // ================================================================
        // 共享字段
        // ================================================================

        private BrepFace _face;
        private WallCommand.CurtainWallParams _params;

        // 预览缓存（步骤间累积传递）
        private DivisionResult _previewDivision;
        private List<Brep> _previewStructureA;
        private List<Brep> _previewStructureB;
        private List<Brep> _previewJoints;
        private List<Brep> _previewGlass;
        private List<Brep> _previewFrames;

        // ================================================================
        // 初始化
        // ================================================================

        protected override void OnInit()
        {
            _face = null;
            _params = WallCommand.GetDefaults();
            ClearPreviewCache();
        }

        private void ClearPreviewCache()
        {
            _previewDivision = null;
            _previewStructureA = null;
            _previewStructureB = null;
            _previewJoints = null;
            _previewGlass = null;
            _previewFrames = null;
        }

        // ================================================================
        // 步骤定义（7 步，按构件类型分组）
        // ================================================================

        protected override StepDef[] DefineSteps()
        {
            return new StepDef[]
            {
                Step.Create("曲面", SetupSurface, ProcessSurface),
                Step.Create("网格", SetupGrid, ProcessGrid, PreviewGrid),
                Step.Create("方向A梁", SetupStructureA, ProcessStructureA, PreviewStructureA),
                Step.Create("方向B梁", SetupStructureB, ProcessStructureB, PreviewStructureB),
                Step.Create("节点", SetupJoints, ProcessJoints, PreviewJoints),
                Step.Create("玻璃", SetupGlass, ProcessGlass, PreviewGlass),
                Step.Create("窗框", SetupFrames, ProcessFrames, PreviewFrames)
            };
        }

        // ================================================================
        // 步骤 1：选择曲面
        // ================================================================

        private void SetupSurface(InputBuilder input)
        {
            input.Object("surface", "第 1 步：请选择幕墙基曲面", ObjectType.Brep | ObjectType.Surface);
        }

        private void ProcessSurface(InputResult result, bool isPreview)
        {
            GeometryBase[] geoms = result.Get<GeometryBase[]>("surface_geom");
            if (geoms == null || geoms.Length == 0)
                return;

            Brep brep = geoms[0] as Brep;
            if (brep == null || brep.Faces.Count == 0)
            {
                RhinoApp.WriteLine("[凤凰中心 错误] 所选对象不是曲面。");
                return;
            }

            _face = brep.Faces[0];
            if (brep.Faces.Count > 1)
                RhinoApp.WriteLine($"[凤凰中心 提示] 所选对象包含 {brep.Faces.Count} 个面，使用第一个面。");
        }

        // ================================================================
        // 步骤 2：网格划分参数
        // ================================================================

        private void SetupGrid(InputBuilder input)
        {
            input.Integer("CurveCount", "第 2 步：设置网格划分参数 · 曲线数", _params.CurveCount);
            input.Integer("DivisionCount", "划分数", _params.DivisionCount);
            input.Toggle("UseVDirection", "使用V方向", _params.UseVDirection);
            input.Toggle("ClosedLoop", "闭合环路", _params.ClosedLoop);
            input.Toggle("FlipNormals", "反转法线", _params.FlipNormals);
        }

        private void ProcessGrid(InputResult result, bool isPreview)
        {
            _params.CurveCount = result.Get<int>("CurveCount");
            _params.DivisionCount = result.Get<int>("DivisionCount");
            _params.UseVDirection = result.Get<bool>("UseVDirection");
            _params.ClosedLoop = result.Get<bool>("ClosedLoop");
            _params.FlipNormals = result.Get<bool>("FlipNormals");

            // 网格参数改变，清除后续构件缓存
            _previewStructureA = null;
            _previewStructureB = null;
            _previewJoints = null;
            _previewGlass = null;
            _previewFrames = null;

            if (_face == null)
                return;

            List<string> errors = WallCommand.Validate(_params);
            if (errors.Count > 0)
            {
                foreach (string error in errors)
                    RhinoApp.WriteLine($"[凤凰中心 错误] {error}");
                return;
            }

            _previewDivision = WallCommand.DivideGrid(_face, _params);
        }

        private object[] PreviewGrid()
        {
            if (_previewDivision == null)
                return null;

            var geometries = new List<object>();
            geometries.AddRange(_previewDivision.LinesA);
            geometries.AddRange(_previewDivision.LinesB);
            return geometries.ToArray();
        }

        // ================================================================
        // 步骤 3：方向 A 支撑梁
        // ================================================================

        private void SetupStructureA(InputBuilder input)
        {
            input.Double("OffsetA", "第 3 步：设置方向 A 支撑梁 · 偏移距离", _params.OffsetA);
            input.Double("BeamWidthA", "截面宽度", _params.BeamWidthA);
            input.Double("BeamDepthA", "截面深度", _params.BeamDepthA);
        }

        private void ProcessStructureA(InputResult result, bool isPreview)
        {
            _params.OffsetA = result.Get<double>("OffsetA");
            _params.BeamWidthA = result.Get<double>("BeamWidthA");
            _params.BeamDepthA = result.Get<double>("BeamDepthA");

            if (!EnsureDivision())
                return;

            _previewStructureA = WallCommand.CreateStructure(
                _previewDivision.LinesA, _previewDivision, _params, "A");
        }

        private object[] PreviewStructureA()
        {
            return CollectPreview(includeStructureA: true);
        }

        // ================================================================
        // 步骤 4：方向 B 支撑梁
        // ================================================================

        private void SetupStructureB(InputBuilder input)
        {
            input.Double("OffsetB", "第 4 步：设置方向 B 支撑梁 · 偏移距离", _params.OffsetB);
            input.Double("BeamWidthB", "截面宽度", _params.BeamWidthB);
            input.Double("BeamDepthB", "截面深度", _params.BeamDepthB);
        }

        private void ProcessStructureB(InputResult result, bool isPreview)
        {
            _params.OffsetB = result.Get<double>("OffsetB");
            _params.BeamWidthB = result.Get<double>("BeamWidthB");
            _params.BeamDepthB = result.Get<double>("BeamDepthB");

            if (!EnsureDivision())
                return;

            _previewStructureB = WallCommand.CreateStructure(
                _previewDivision.LinesB, _previewDivision, _params, "B");
        }

        private object[] PreviewStructureB()
        {
            return CollectPreview(includeStructureB: true);
        }

        // ================================================================
        // 步骤 5：节点与爪臂
        // ================================================================

        private void SetupJoints(InputBuilder input)
        {
            input.Double("JointRadius", "第 5 步：设置节点与爪臂 · 节点半径", _params.JointRadius);
            input.Double("JointOffset", "节点偏移", _params.JointOffset);
            input.Double("ClawLength", "爪臂长度", _params.ClawLength);
            input.Double("ClawWidth", "爪臂宽度", _params.ClawWidth);
            input.Double("ClawDepth", "爪臂深度", _params.ClawDepth);
        }

        private void ProcessJoints(InputResult result, bool isPreview)
        {
            _params.JointRadius = result.Get<double>("JointRadius");
            _params.JointOffset = result.Get<double>("JointOffset");
            _params.ClawLength = result.Get<double>("ClawLength");
            _params.ClawWidth = result.Get<double>("ClawWidth");
            _params.ClawDepth = result.Get<double>("ClawDepth");

            if (!EnsureDivision())
                return;

            _previewJoints = WallCommand.CreateJoints(_previewDivision, _params);
        }

        private object[] PreviewJoints()
        {
            return CollectPreview(includeJoints: true);
        }

        // ================================================================
        // 步骤 6：玻璃面板
        // ================================================================

        private void SetupGlass(InputBuilder input)
        {
            input.Double("GlassThickness", "第 6 步：设置玻璃面板 · 玻璃厚度", _params.GlassThickness);
        }

        private void ProcessGlass(InputResult result, bool isPreview)
        {
            _params.GlassThickness = result.Get<double>("GlassThickness");

            if (!EnsureDivision())
                return;

            _previewGlass = WallCommand.CreateGlassPanels(_previewDivision, _params);
        }

        private object[] PreviewGlass()
        {
            return CollectPreview(includeGlass: true);
        }

        // ================================================================
        // 步骤 7：窗框
        // ================================================================

        private void SetupFrames(InputBuilder input)
        {
            input.Double("FrameWidth", "第 7 步：设置窗框 · 窗框宽度", _params.FrameWidth);
            input.Double("FrameDepth", "窗框深度", _params.FrameDepth);
        }

        private void ProcessFrames(InputResult result, bool isPreview)
        {
            _params.FrameWidth = result.Get<double>("FrameWidth");
            _params.FrameDepth = result.Get<double>("FrameDepth");

            if (!EnsureDivision())
                return;

            _previewFrames = WallCommand.CreateFrames(_previewDivision, _params);
        }

        private object[] PreviewFrames()
        {
            return CollectPreview(includeFrames: true);
        }

        // ================================================================
        // 预览辅助
        // ================================================================

        /// <summary>
        /// 确保网格已划分（步骤 3-7 在预览时可能尚未计算）。
        /// </summary>
        private bool EnsureDivision()
        {
            if (_face == null)
                return false;

            if (_previewDivision == null)
                _previewDivision = WallCommand.DivideGrid(_face, _params);

            return _previewDivision != null;
        }

        /// <summary>
        /// 累积收集预览几何。每步显示网格曲线 + 该步及之前已生成的构件。
        /// </summary>
        private object[] CollectPreview(
            bool includeStructureA = false,
            bool includeStructureB = false,
            bool includeJoints = false,
            bool includeGlass = false,
            bool includeFrames = false)
        {
            var geometries = new List<object>();

            // 始终显示网格曲线作为参考
            if (_previewDivision != null)
            {
                geometries.AddRange(_previewDivision.LinesA);
                geometries.AddRange(_previewDivision.LinesB);
            }

            if (includeStructureA && _previewStructureA != null)
                geometries.AddRange(_previewStructureA);
            if (includeStructureB && _previewStructureB != null)
                geometries.AddRange(_previewStructureB);
            if (includeJoints && _previewJoints != null)
                geometries.AddRange(_previewJoints);
            if (includeGlass && _previewGlass != null)
                geometries.AddRange(_previewGlass);
            if (includeFrames && _previewFrames != null)
                geometries.AddRange(_previewFrames);

            return geometries.Count > 0 ? geometries.ToArray() : null;
        }

        // ================================================================
        // 完成：写入文档
        // ================================================================

        protected override void OnFinish(RhinoDoc doc)
        {
            if (_face == null || _previewDivision == null)
            {
                RhinoApp.WriteLine("[凤凰中心 错误] 无有效数据，命令终止。");
                return;
            }

            // 写入对角曲线
            if (_previewDivision.LinesA.Count > 0)
            {
                int layer = EnsureLayer(doc, "CurveA", Color.FromArgb(255, 0, 0));
                WriteCurves(doc, layer, _previewDivision.LinesA);
            }
            if (_previewDivision.LinesB.Count > 0)
            {
                int layer = EnsureLayer(doc, "CurveB", Color.FromArgb(0, 0, 255));
                WriteCurves(doc, layer, _previewDivision.LinesB);
            }

            // 写入支撑梁
            if (_previewStructureA != null && _previewStructureA.Count > 0)
            {
                int layer = EnsureLayer(doc, "StructureA", Color.FromArgb(200, 100, 0));
                WriteBreps(doc, layer, _previewStructureA);
            }
            if (_previewStructureB != null && _previewStructureB.Count > 0)
            {
                int layer = EnsureLayer(doc, "StructureB", Color.FromArgb(0, 150, 200));
                WriteBreps(doc, layer, _previewStructureB);
            }

            // 写入节点
            if (_previewJoints != null && _previewJoints.Count > 0)
            {
                int layer = EnsureLayer(doc, "Joints", Color.FromArgb(100, 100, 100));
                WriteBreps(doc, layer, _previewJoints);
            }

            // 写入玻璃
            if (_previewGlass != null && _previewGlass.Count > 0)
            {
                int layer = EnsureLayer(doc, "Glass", Color.FromArgb(150, 200, 255));
                WriteBreps(doc, layer, _previewGlass);
            }

            // 写入窗框
            if (_previewFrames != null && _previewFrames.Count > 0)
            {
                int layer = EnsureLayer(doc, "Frame", Color.FromArgb(100, 100, 50));
                WriteBreps(doc, layer, _previewFrames);
            }

            // 更新 Data 层默认值
            WallCommand.UpdateDefaults(_params);

            RhinoApp.WriteLine("[凤凰中心] 幕墙生成完成。");
        }

        // ================================================================
        // 图层与写入工具
        // ================================================================

        private const string LayerParent = "FenghuangCenter";

        /// <summary>
        /// 查找或创建子图层（父图层自动创建）。
        /// </summary>
        private int EnsureLayer(RhinoDoc doc, string childName, Color color)
        {
            // 确保父图层存在
            int parentIdx = doc.Layers.FindByFullPath(LayerParent, -1);
            if (parentIdx < 0)
            {
                var parentLayer = new Layer { Name = LayerParent };
                parentIdx = doc.Layers.Add(parentLayer);
            }

            // 查找或创建子图层
            string fullChildName = $"{LayerParent}::{childName}";
            int childIdx = doc.Layers.FindByFullPath(fullChildName, -1);
            if (childIdx >= 0)
                return childIdx;

            var childLayer = new Layer
            {
                Name = childName,
                ParentLayerId = doc.Layers[parentIdx].Id,
                Color = color
            };
            return doc.Layers.Add(childLayer);
        }

        private void WriteCurves(RhinoDoc doc, int layerIdx, List<Curve> curves)
        {
            var attrs = new ObjectAttributes { LayerIndex = layerIdx };
            foreach (var curve in curves)
            {
                if (curve != null && curve.IsValid)
                    doc.Objects.AddCurve(curve, attrs);
            }
        }

        private void WriteBreps(RhinoDoc doc, int layerIdx, List<Brep> breps)
        {
            var attrs = new ObjectAttributes { LayerIndex = layerIdx };
            foreach (var brep in breps)
            {
                if (brep != null && brep.IsValid)
                    doc.Objects.AddBrep(brep, attrs);
            }
        }
    }
}
