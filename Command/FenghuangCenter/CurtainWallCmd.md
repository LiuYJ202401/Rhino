# CurtainWallCmd 命令接口文档

命名空间：`Rh.Cmd`

## 功能

凤凰中心幕墙主体生成命令的方法集（不写入文档，返回几何对象）。负责参数校验、默认值读写、派生计算、错误报告，并调用项目 Geometry 层（`Rh.Geo.FenghuangCenter`）和框架 Geometry 层（`Rh.Geo.Srf` / `Rh.Geo.Sld`）生成构件。

## 默认值机制

- 18 个用户参数的默认值存储于 `Data/Command/FenghuangCenter/CurtainWall.json`
- 通过 `DataReader` 读取：`DataReader.GetDynamicValue(key, fallback)`（带公差回退）
- `isPreview=true`（预览模式）：仅读取默认值，不更新
- `isPreview=false`（确认模式）：读取默认值，并在生成成功后更新 JSON

## 公差

- 所有几何操作的公差统一使用 `RhinoDoc.ActiveDoc.ModelAbsoluteTolerance`
- 在 Command 层获取并传入 Geometry 层（Geometry 层禁止访问 ActiveDoc）

---

## 派生计算

以下 4 个派生值由用户参数推导，不存入 Data 层：

### ComputeGlassOffset

| 项目 | 说明 |
|------|------|
| 功能 | 计算玻璃面板相对基曲面的偏移距离 |
| 输入 | `double frameDepth` — 窗框深度 |
| 输出 | `double` — 玻璃偏移 = FrameDepth / 2.0 |
| RhinoCommon | 无（纯数学） |

### ComputeStructureOffset

| 项目 | 说明 |
|------|------|
| 功能 | 计算支撑梁相对基曲面的最小偏移距离（确保梁在玻璃外侧） |
| 输入 | `double userOffset` — 用户指定偏移，`double beamDepth` — 梁截面深度，`double glassOffset`，`double glassThickness`，`double frameDepth` |
| 输出 | `double` — Max(userOffset, glassOffset + glassThickness/2 + frameDepth/2 + beamDepth/2) |
| RhinoCommon | 无（纯数学） |

### ComputeJointCenter

| 项目 | 说明 |
|------|------|
| 功能 | 计算连接节点的中心位置（双向梁外侧中点） |
| 输入 | `double structOffsetA`，`double beamDepthA`，`double structOffsetB`，`double beamDepthB` |
| 输出 | `double` — Max(structOffsetA + beamDepthA/2, structOffsetB + beamDepthB/2) / 2.0 |
| RhinoCommon | 无（纯数学） |

### ComputeJointRadius

| 项目 | 说明 |
|------|------|
| 功能 | 计算连接节点的建议半径 |
| 输入 | `double jointCenter` |
| 输出 | `double` — JointCenter / 12.0 |
| RhinoCommon | 无（纯数学） |

---

## 方法

### ExtractCurves

对应源代码：`SurfaceToCurves.ExtractIsoCurves`

| 项目 | 说明 |
|------|------|
| 功能 | 从 BrepFace 批量提取 iso-curve，作为网格划分的输入 |
| 输入 | `BrepFace face` — 曲面，`int curveCount` — 提取数量，`bool useVDirection` — 使用 V 方向，`bool closedLoop` — 闭合环路，`bool isPreview` |
| 输出 | `(List<Curve> curves, Surface surface)` — 曲线列表 + 用于法线计算的参考曲面 |
| 报错 | curveCount < 3 或 > 200 时输出错误并返回空列表 |
| 调用 | `Rh.Geo.FenghuangCenter.IsoCurveGeo.Extract` |
| RhinoCommon | `surface.IsoCurve(direction, parameter)` |

### DivideGrid

对应源代码：`SurfaceDivider.Divide`

| 项目 | 说明 |
|------|------|
| 功能 | 在曲面参数域上划分网格，生成点网格、法线网格、对角曲线、四边形、边 |
| 输入 | `BrepFace face` — 目标曲面，`bool flipNormals` — 反转法线，`bool isPreview` |
| 输出 | `DivisionResult` — 含 LinesA、LinesB、Quads、Edges、PointGrid、NormalGrid |
| 报错 | 曲面底层几何不可用或划分未生成任何对角曲线时输出错误并返回空 DivisionResult |
| 调用 | `Rh.Geo.FenghuangCenter.DiagonalGridGeo.Divide`（直接从 Surface 参数域采样 + `surface.NormalAt` 真实法线） |
| RhinoCommon | `surface.PointAt`、`surface.NormalAt`、`surface.IsClosed`、`NurbsCurve.CreateInterpolatedCurve` |

### CreateStructureA / CreateStructureB

对应源代码：`StructureBuilder.Build`（方向 A / 方向 B 两次调用）

| 项目 | 说明 |
|------|------|
| 功能 | 沿对角曲线偏移并生成矩形截面支撑梁实体 |
| 输入 | `List<Curve> lines` — 对角曲线（LinesA 或 LinesB），`DivisionResult division` — 网格数据（法线），`double offset` — 偏移距离（派生计算），`double width` — 截面宽度，`double depth` — 截面深度，`bool closedLoop`，`double tolerance`，`bool isPreview` |
| 输出 | `List<Brep>` — 支撑梁实体列表 |
| 报错 | 单根梁生成失败时跳过并计数，最终报告失败数量 |
| 调用 | `Rh.Geo.FenghuangCenter.StructureBeamGeo.Build`（内部委托 `SurfaceGeo.CreateLoft` + `SolidGeo.CreateSolidFromBreps`） |
| RhinoCommon | `curve.Offset`（沿法线）、`Brep.CreateFromLoft`（委托框架） |

### CreateJoints

对应源代码：`JointBuilder.Build`

| 项目 | 说明 |
|------|------|
| 功能 | 在网格交点处生成圆柱节点和爪臂 |
| 输入 | `DivisionResult division`，`double jointRadius`（派生计算覆盖），`double jointOffset`，`double clawLength`，`double clawWidth`，`double clawDepth`，`double tolerance`，`bool isPreview` |
| 输出 | `List<Brep>` — 节点 + 爪臂实体列表 |
| 报错 | 单个节点生成失败时跳过并计数 |
| 调用 | `Rh.Geo.FenghuangCenter.JointNodeGeo.Build`（内部委托 `SolidGeo.CreateCylinder` + `SolidGeo.CreateFromBox`） |
| RhinoCommon | `Brep.CreateFromCylinder`（委托框架）、`Box`（委托框架） |

### CreateGlassPanels

对应源代码：`GlassBuilder.Build`

| 项目 | 说明 |
|------|------|
| 功能 | 在四边形区域内生成玻璃面板实体（底面 + 顶面 + 侧面合并） |
| 输入 | `List<Quad> quads`，`DivisionResult division`（法线），`double glassOffset`（派生计算），`double glassThickness`，`double tolerance`，`bool isPreview` |
| 输出 | `List<Brep>` — 玻璃面板实体列表 |
| 报错 | 单块玻璃生成失败时跳过并计数 |
| 调用 | `Rh.Geo.FenghuangCenter.GlassPanelGeo.BuildPanels`（内部委托 `SurfaceGeo.CreatePlanarBreps` + `SolidGeo.CreateSolidFromBreps`） |
| RhinoCommon | `Brep.CreatePlanarBreps`（委托框架）、`Brep.JoinBreps`（委托框架） |

### CreateFrames

对应源代码：`GlassBuilder.BuildFrames`

| 项目 | 说明 |
|------|------|
| 功能 | 沿网格边生成窗框实体（Box 沿边定向） |
| 输入 | `List<GridEdge> edges`，`DivisionResult division`（法线），`double frameOffset`（派生计算），`double frameWidth`，`double frameDepth`，`double tolerance`，`bool isPreview` |
| 输出 | `List<Brep>` — 窗框实体列表 |
| 报错 | 单根窗框生成失败时跳过并计数 |
| 调用 | `Rh.Geo.FenghuangCenter.GlassPanelGeo.BuildFrames`（内部委托 `SolidGeo.CreateFromBox`） |
| RhinoCommon | `Box`（委托框架） |

---

## 调用顺序

```
ExtractCurves        → (curves, surface)
DivideGrid           → DivisionResult
CreateStructureA     → List<Brep>  (方向 A 梁)
CreateStructureB     → List<Brep>  (方向 B 梁)
CreateJoints         → List<Brep>  (节点 + 爪臂)
CreateGlassPanels    → List<Brep>  (玻璃面板)
CreateFrames         → List<Brep>  (窗框)
```

Project 层按此顺序调用，预览时全部以 `isPreview=true` 调用并返回几何；确认时全部以 `isPreview=false` 调用并按图层写入文档。

## 参数校验

| 参数 | 有效范围 |
|------|---------|
| CurveCount | [3, 200] |
| DivisionCount | [2, 200] |
| OffsetA / OffsetB | [0, 50000] |
| BeamWidthA/B, BeamDepthA/B | [1, 5000] |
| JointRadius | [1, 5000] |
| JointOffset | [0, 50000] |
| GlassThickness | [1, 5000] |
| FrameWidth / FrameDepth | [1, 5000] |
| ClawLength | [10, 2000] |
| ClawWidth / ClawDepth | [5, 500] |

校验失败时通过 `RhinoApp.WriteLine` 输出错误，命令终止。
