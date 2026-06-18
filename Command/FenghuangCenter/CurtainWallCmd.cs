using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Rh.Data;
using Rh.Geo.FenghuangCenter;

namespace Rh.Cmd
{
    /// <summary>
    /// 凤凰中心幕墙命令方法集。
    /// 负责参数校验、默认值读写、派生计算、错误报告，调用 Geometry 层生成构件。
    /// 不与用户交互，不写入文档。
    /// 对应接口文档：CurtainWallCmd.md
    /// </summary>
    public static class CurtainWallCmd
    {
        private const string DataPath = "Command/FenghuangCenter/CurtainWall.json";

        // ================================================================
        // 默认值参数结构（供 Project 层传递）
        // ================================================================

        /// <summary>
        /// 幕墙参数集合。包含 19 个用户参数，用于 Project ↔ Command 层传递。
        /// </summary>
        public class CurtainWallParams
        {
            public int CurveCount { get; set; } = 20;
            public bool UseVDirection { get; set; } = true;
            public int DivisionCount { get; set; } = 20;
            public bool ClosedLoop { get; set; } = true;
            public bool FlipNormals { get; set; } = false;
            public double OffsetA { get; set; } = 500.0;
            public double OffsetB { get; set; } = 500.0;
            public double BeamWidthA { get; set; } = 200.0;
            public double BeamDepthA { get; set; } = 400.0;
            public double BeamWidthB { get; set; } = 200.0;
            public double BeamDepthB { get; set; } = 400.0;
            public double JointRadius { get; set; } = 150.0;
            public double JointOffset { get; set; } = 500.0;
            public double GlassThickness { get; set; } = 30.0;
            public double FrameWidth { get; set; } = 60.0;
            public double FrameDepth { get; set; } = 80.0;
            public double ClawLength { get; set; } = 120.0;
            public double ClawWidth { get; set; } = 25.0;
            public double ClawDepth { get; set; } = 35.0;
        }

        // ================================================================
        // DataReader 读写封装
        // ================================================================

        private static T GetDefault<T>(string key, T defaultValue)
        {
            return DataReader.GetValue<T>(DataPath, key, defaultValue);
        }

        private static void UpdateDefault<T>(string key, T value)
        {
            DataReader.SetValue<T>(DataPath, key, value);
        }

        private static double ActiveTolerance()
        {
            return DataReader.GetDynamicValue("ModelAbsoluteTolerance");
        }

        // ================================================================
        // 默认值读取 / 更新（供 Project 层）
        // ================================================================

        /// <summary>
        /// 读取全部默认参数。
        /// </summary>
        public static CurtainWallParams GetDefaults()
        {
            return new CurtainWallParams
            {
                CurveCount = GetDefault("curves.curveCount", 20),
                UseVDirection = GetDefault("curves.useVDirection", true),
                DivisionCount = GetDefault("grid.divisionCount", 20),
                ClosedLoop = GetDefault("grid.closedLoop", true),
                OffsetA = GetDefault("structure.offsetA", 500.0),
                OffsetB = GetDefault("structure.offsetB", 500.0),
                BeamWidthA = GetDefault("structure.beamWidthA", 200.0),
                BeamDepthA = GetDefault("structure.beamDepthA", 400.0),
                BeamWidthB = GetDefault("structure.beamWidthB", 200.0),
                BeamDepthB = GetDefault("structure.beamDepthB", 400.0),
                JointRadius = GetDefault("joint.radius", 150.0),
                JointOffset = GetDefault("joint.offset", 500.0),
                GlassThickness = GetDefault("glass.thickness", 30.0),
                FrameWidth = GetDefault("frame.width", 60.0),
                FrameDepth = GetDefault("frame.depth", 80.0),
                ClawLength = GetDefault("claw.length", 120.0),
                ClawWidth = GetDefault("claw.width", 25.0),
                ClawDepth = GetDefault("claw.depth", 35.0)
            };
        }

        /// <summary>
        /// 更新全部默认参数（仅 Project 层 OnFinish 调用）。
        /// </summary>
        public static void UpdateDefaults(CurtainWallParams p)
        {
            UpdateDefault("curves.curveCount", p.CurveCount);
            UpdateDefault("curves.useVDirection", p.UseVDirection);
            UpdateDefault("grid.divisionCount", p.DivisionCount);
            UpdateDefault("grid.closedLoop", p.ClosedLoop);
            UpdateDefault("structure.offsetA", p.OffsetA);
            UpdateDefault("structure.offsetB", p.OffsetB);
            UpdateDefault("structure.beamWidthA", p.BeamWidthA);
            UpdateDefault("structure.beamDepthA", p.BeamDepthA);
            UpdateDefault("structure.beamWidthB", p.BeamWidthB);
            UpdateDefault("structure.beamDepthB", p.BeamDepthB);
            UpdateDefault("joint.radius", p.JointRadius);
            UpdateDefault("joint.offset", p.JointOffset);
            UpdateDefault("glass.thickness", p.GlassThickness);
            UpdateDefault("frame.width", p.FrameWidth);
            UpdateDefault("frame.depth", p.FrameDepth);
            UpdateDefault("claw.length", p.ClawLength);
            UpdateDefault("claw.width", p.ClawWidth);
            UpdateDefault("claw.depth", p.ClawDepth);
        }

        // ================================================================
        // 派生计算
        // ================================================================

        /// <summary>玻璃偏移 = FrameDepth / 2</summary>
        public static double ComputeGlassOffset(double frameDepth)
        {
            return frameDepth / 2.0;
        }

        /// <summary>支撑梁偏移 = Max(userOffset, glassOuter + beamDepth/2)</summary>
        public static double ComputeStructureOffset(double userOffset, double beamDepth,
            double glassOffset, double glassThickness, double frameDepth)
        {
            double glassOuter = glassOffset + glassThickness / 2.0 + frameDepth / 2.0;
            double minOffset = glassOuter + beamDepth / 2.0;
            return Math.Max(userOffset, minOffset);
        }

        /// <summary>节点中心 = Max(structOffsetA + beamDepthA/2, structOffsetB + beamDepthB/2) / 2</summary>
        public static double ComputeJointCenter(double structOffsetA, double beamDepthA,
            double structOffsetB, double beamDepthB)
        {
            double outerA = structOffsetA + beamDepthA / 2.0;
            double outerB = structOffsetB + beamDepthB / 2.0;
            return Math.Max(outerA, outerB) / 2.0;
        }

        /// <summary>节点半径 = JointCenter / 12</summary>
        public static double ComputeJointRadius(double jointCenter)
        {
            return jointCenter / 12.0;
        }

        // ================================================================
        // 参数校验
        // ================================================================

        /// <summary>
        /// 校验参数。返回错误消息列表（空列表表示通过）。
        /// </summary>
        public static List<string> Validate(CurtainWallParams p)
        {
            var errors = new List<string>();

            if (p.CurveCount < 3 || p.CurveCount > 200)
                errors.Add($"提取曲线数 {p.CurveCount} 超出有效范围 [3, 200]。");
            if (p.DivisionCount < 2 || p.DivisionCount > 200)
                errors.Add($"划分数 {p.DivisionCount} 超出有效范围 [2, 200]。");
            if (p.OffsetA < 0 || p.OffsetA > 50000)
                errors.Add($"方向A偏移 {p.OffsetA} 超出有效范围 [0, 50000]。");
            if (p.OffsetB < 0 || p.OffsetB > 50000)
                errors.Add($"方向B偏移 {p.OffsetB} 超出有效范围 [0, 50000]。");
            if (p.BeamWidthA < 1 || p.BeamWidthA > 5000)
                errors.Add($"方向A截面宽度 {p.BeamWidthA} 超出有效范围 [1, 5000]。");
            if (p.BeamDepthA < 1 || p.BeamDepthA > 5000)
                errors.Add($"方向A截面深度 {p.BeamDepthA} 超出有效范围 [1, 5000]。");
            if (p.BeamWidthB < 1 || p.BeamWidthB > 5000)
                errors.Add($"方向B截面宽度 {p.BeamWidthB} 超出有效范围 [1, 5000]。");
            if (p.BeamDepthB < 1 || p.BeamDepthB > 5000)
                errors.Add($"方向B截面深度 {p.BeamDepthB} 超出有效范围 [1, 5000]。");
            if (p.JointRadius < 1 || p.JointRadius > 5000)
                errors.Add($"节点半径 {p.JointRadius} 超出有效范围 [1, 5000]。");
            if (p.JointOffset < 0 || p.JointOffset > 50000)
                errors.Add($"节点偏移 {p.JointOffset} 超出有效范围 [0, 50000]。");
            if (p.GlassThickness < 1 || p.GlassThickness > 5000)
                errors.Add($"玻璃厚度 {p.GlassThickness} 超出有效范围 [1, 5000]。");
            if (p.FrameWidth < 1 || p.FrameWidth > 5000)
                errors.Add($"窗框宽度 {p.FrameWidth} 超出有效范围 [1, 5000]。");
            if (p.FrameDepth < 1 || p.FrameDepth > 5000)
                errors.Add($"窗框深度 {p.FrameDepth} 超出有效范围 [1, 5000]。");
            if (p.ClawLength < 10 || p.ClawLength > 2000)
                errors.Add($"爪臂长度 {p.ClawLength} 超出有效范围 [10, 2000]。");
            if (p.ClawWidth < 5 || p.ClawWidth > 500)
                errors.Add($"爪臂宽度 {p.ClawWidth} 超出有效范围 [5, 500]。");
            if (p.ClawDepth < 5 || p.ClawDepth > 500)
                errors.Add($"爪臂深度 {p.ClawDepth} 超出有效范围 [5, 500]。");

            return errors;
        }

        // ================================================================
        // 业务方法
        // ================================================================

        /// <summary>
        /// 从 BrepFace 提取 iso-curve。
        /// </summary>
        public static List<Curve> ExtractCurves(BrepFace face, CurtainWallParams p)
        {
            Surface surface = face.UnderlyingSurface();
            if (surface == null || !surface.IsValid)
            {
                RhinoApp.WriteLine("[FenghuangCenter 错误] 无法获取曲面底层几何。");
                return new List<Curve>();
            }

            List<Curve> curves = IsoCurveGeo.Extract(surface, p.CurveCount, p.UseVDirection, p.ClosedLoop);

            if (curves.Count < 2)
                RhinoApp.WriteLine("[FenghuangCenter 错误] 提取的曲线不足 2 条。");

            return curves;
        }

        /// <summary>
        /// 划分网格，生成对角曲线、四边形、边。
        /// 直接从曲面参数域采样，用曲面真实法线（解决闭合曲面接缝处穿模）。
        /// </summary>
        public static DivisionResult DivideGrid(BrepFace face, CurtainWallParams p)
        {
            Surface surface = face.UnderlyingSurface();
            if (surface == null || !surface.IsValid)
            {
                RhinoApp.WriteLine("[FenghuangCenter 错误] 无法获取曲面底层几何。");
                return new DivisionResult();
            }

            DivisionResult division = DiagonalGridGeo.Divide(
                surface, p.CurveCount, p.DivisionCount, p.UseVDirection, p.ClosedLoop, p.FlipNormals);

            if (division.LinesA.Count == 0 && division.LinesB.Count == 0)
                RhinoApp.WriteLine("[FenghuangCenter 错误] 划分未生成任何对角曲线。");

            return division;
        }

        /// <summary>
        /// 生成支撑梁（方向 A 或 B）。
        /// </summary>
        public static List<Brep> CreateStructure(List<Curve> lines, DivisionResult division,
            CurtainWallParams p, string direction)
        {
            double glassOffset = ComputeGlassOffset(p.FrameDepth);
            double offset, width, depth;

            if (direction == "A")
            {
                offset = ComputeStructureOffset(p.OffsetA, p.BeamDepthA, glassOffset, p.GlassThickness, p.FrameDepth);
                width = p.BeamWidthA;
                depth = p.BeamDepthA;
            }
            else
            {
                offset = ComputeStructureOffset(p.OffsetB, p.BeamDepthB, glassOffset, p.GlassThickness, p.FrameDepth);
                width = p.BeamWidthB;
                depth = p.BeamDepthB;
            }

            List<Brep> result = StructureBeamGeo.Build(lines, division, offset, width, depth, p.ClosedLoop, ActiveTolerance());

            if (result.Count < lines.Count)
                RhinoApp.WriteLine($"[FenghuangCenter 提示] 方向{direction} 支撑梁生成 {result.Count}/{lines.Count}。");

            return result;
        }

        /// <summary>
        /// 生成节点 + 爪臂。
        /// </summary>
        public static List<Brep> CreateJoints(DivisionResult division, CurtainWallParams p)
        {
            double glassOffset = ComputeGlassOffset(p.FrameDepth);
            double glassOuterOffset = glassOffset + p.GlassThickness / 2.0;

            double structOffsetA = ComputeStructureOffset(p.OffsetA, p.BeamDepthA, glassOffset, p.GlassThickness, p.FrameDepth);
            double structOffsetB = ComputeStructureOffset(p.OffsetB, p.BeamDepthB, glassOffset, p.GlassThickness, p.FrameDepth);
            double outerOffset = Math.Max(structOffsetA, structOffsetB);

            double jointCenter = ComputeJointCenter(structOffsetA, p.BeamDepthA, structOffsetB, p.BeamDepthB);
            double jointRadius = ComputeJointRadius(jointCenter);

            return JointNodeGeo.Build(division, jointRadius, glassOffset, outerOffset, glassOuterOffset,
                p.ClawLength, p.ClawWidth, p.ClawDepth, ActiveTolerance());
        }

        /// <summary>
        /// 生成玻璃面板。
        /// </summary>
        public static List<Brep> CreateGlassPanels(DivisionResult division, CurtainWallParams p)
        {
            double glassOffset = ComputeGlassOffset(p.FrameDepth);
            return GlassPanelGeo.BuildPanels(division.Quads, glassOffset, p.GlassThickness, ActiveTolerance());
        }

        /// <summary>
        /// 生成窗框。
        /// </summary>
        public static List<Brep> CreateFrames(DivisionResult division, CurtainWallParams p)
        {
            double glassOffset = ComputeGlassOffset(p.FrameDepth);
            double frameCenter = glassOffset + p.GlassThickness / 2.0;
            return GlassPanelGeo.BuildFrames(division.Edges, frameCenter, p.FrameWidth, p.FrameDepth, ActiveTolerance());
        }
    }
}
