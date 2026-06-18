# Geometry/FenghuangCenter 说明文档

命名空间：`Rh.Geo.FenghuangCenter`

## 职责

凤凰中心幕墙项目的专属几何工具。包含框架没有的自定义算法（对角网格、法线偏移、截面定向），基础图元创建委托给已有框架 `Rh.Geo.Srf`（SurfaceGeo）和 `Rh.Geo.Sld`（SolidGeo）。

## 规则

- 纯几何计算，不访问 `RhinoDoc.ActiveDoc`
- 公差参数从外部传入（Command 层负责传递）
- 不调用 `RhinoApp.WriteLine`，失败时返回空集合或 null
- 同层复用：调用 `Rh.Geo.Srf.SurfaceGeo` 和 `Rh.Geo.Sld.SolidGeo`

---

## 文件结构

```
Geometry/FenghuangCenter/
├── README.md              本文档
├── DivisionResult.cs      网格数据结构（PointGrid, NormalGrid, LinesA/B, Quads, Edges）
├── IsoCurveGeo.cs         Iso-curve 批量提取
├── DiagonalGridGeo.cs     对角网格划分核心算法
├── StructureBeamGeo.cs    支撑梁生成（沿法线偏移 + 截面定向 + 放样）
├── JointNodeGeo.cs        节点生成（圆柱 + 爪臂 Box）
└── GlassPanelGeo.cs       玻璃面板与窗框
```

---

## DivisionResult

纯数据结构，不包含方法。

| 字段 | 类型 | 说明 |
|------|------|------|
| `PointGrid` | `List<List<Point3d>>` | 网格点（[曲线索引][划分索引]） |
| `NormalGrid` | `List<List<Vector3d>>` | 网格法线（对应 PointGrid） |
| `LinesA` | `List<Curve>` | 方向 A 对角曲线 |
| `LinesB` | `List<Curve>` | 方向 B 对角曲线 |
| `Quads` | `List<Quad>` | 四边形列表 |
| `Edges` | `List<GridEdge>` | 网格边列表 |

### Quad

| 字段 | 类型 | 说明 |
|------|------|------|
| `P0` / `P1` / `P2` / `P3` | `Point3d` | 四边形角点（顺序排列） |
| `N0` / `N1` / `N2` / `N3` | `Vector3d` | 对应角点法线 |

### GridEdge

| 字段 | 类型 | 说明 |
|------|------|------|
| `Start` / `End` | `Point3d` | 边的起点/终点 |
| `StartNormal` / `EndNormal` | `Vector3d` | 起点/终点处法线 |

---

## IsoCurveGeo

iso-curve 批量提取。框架只有单条提取（`CreateExtractIsocurve`），无批量方法，必须自定义。

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|------------|
| `Extract` | Surface, int curveCount, bool useVDirection, bool closedLoop | `List<Curve>` | `surface.IsoCurve(direction, parameter)` 循环 |

**算法**：在曲面的 U 或 V 方向上按等参数间距提取 curveCount 条 iso-curve。闭合环路时首尾相连处理。

---

## DiagonalGridGeo

对角网格划分核心算法。框架无等价能力，全部自定义。

> **闭合曲面接缝处理**：直接在曲面参数域上采样（`surface.PointAt`），用曲面真实法线（`surface.NormalAt`）。不依赖 iso-curve 的 `DivideByCount`，避免闭合 iso-curve 的 seam 不一致导致的法线方向反转（穿模问题）。

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|------------|
| `Divide` | Surface surface, int curveCount, int divisionCount, bool useVDirection, bool closedLoop, bool flipNormals | `DivisionResult` | `surface.PointAt`、`surface.NormalAt`、`surface.IsClosed`、`NurbsCurve.CreateInterpolatedCurve` |

**算法步骤**：
1. 根据 `useVDirection` 确定 rows/cols 对应的参数方向（U 或 V）
2. 用 `surface.IsClosed` 判断各方向闭合性，决定采样间距（闭合方向不重复端点）
3. **对角闭合约束**：当 rows 方向闭合时，对角曲线走 rows 步后必须回到起点。数学条件 `(rows * step) mod cols == 0`（step=±1 时简化为 `rows mod cols == 0`）。若不满足，自动调整 cols 为 rows 的最接近因子（`FindClosestFactor`），避免接缝处扭转穿模
4. 在参数域等间距采样，`pointGrid[i,j] = surface.PointAt(u, v)`
5. **用 `surface.NormalAt(u, v)` 计算真实法线**（全局方向一致，接缝处不反转）
6. 若 `flipNormals=true`，反转整个 NormalGrid
7. 对角遍历网格点，生成方向 A 和方向 B 的对角曲线（`NurbsCurve.CreateInterpolatedCurve`）
8. 由相邻 4 个网格点构建 Quad 列表
9. 由相邻 2 个网格点构建 GridEdge 列表

> **法线方向与"同一侧"保证**：所有构件（梁/玻璃/节点/窗框）均读取同一个 NormalGrid 进行偏移，因此无论 `flipNormals` 取何值，全部构件始终位于曲面同一侧。`flipNormals` 仅控制具体是哪一侧。

---

## StructureBeamGeo

支撑梁生成。自定义算法（沿法线偏移 + 截面定向），放样/合并委托框架。

| 方法 | 输入 | 输出 | 委托 / RhinoCommon |
|------|------|------|-------------------|
| `Build` | List\<Curve\> lines, DivisionResult division, double offset, double width, double depth, bool closedLoop, double tolerance | `List<Brep>` | 见下 |

**算法步骤**：
1. 对每条对角曲线，沿其各点的法线（取自 division.NormalGrid）偏移 offset 距离，生成偏移曲线
2. 在偏移曲线的每个采样点构建矩形截面（width × depth）：**每个点独立用曲面法线定向**
   - `depthDir` = 曲面法线投影到垂直切线的平面（梁厚度沿法线伸出曲面）
   - `widthDir` = `tangent × depthDir`（在曲面切平面内，梁宽面贴着曲面）
   - 由于曲面法线本身平滑变化，无需 refUp 传递，自然避免扭转
3. 将截面曲线列表委托 `SurfaceGeo.CreateLoft` 放样成 Brep
4. 委托 `SolidGeo.CreateSolidFromBreps` 合并多段梁
5. 失败项跳过，返回成功生成的梁列表

---

## JointNodeGeo

节点生成。自定义算法（网格遍历 + 中心计算），圆柱/Box 委托框架。

| 方法 | 输入 | 输出 | 委托 / RhinoCommon |
|------|------|------|-------------------|
| `Build` | DivisionResult division, double radius, double offset, double clawLength, double clawWidth, double clawDepth, double tolerance | `List<Brep>` | 见下 |

**算法步骤**：
1. 遍历 PointGrid 的每个交点
2. 计算交点处的节点中心位置（沿法线偏移 offset）
3. 委托 `SolidGeo.CreateCylinder` 创建圆柱节点
4. 计算爪臂方向（向相邻四边形中心投影），委托 `SolidGeo.CreateFromBox` 创建 4 个爪臂
5. 失败项跳过，返回成功生成的节点列表

---

## GlassPanelGeo

玻璃面板与窗框。自定义算法（四边形偏移 + 面拼合），平面面/合并/Box 委托框架。

| 方法 | 输入 | 输出 | 委托 / RhinoCommon |
|------|------|------|-------------------|
| `BuildPanels` | List\<Quad\> quads, DivisionResult division, double offset, double thickness, double tolerance | `List<Brep>` | 见下 |
| `BuildFrames` | List\<GridEdge\> edges, DivisionResult division, double offset, double width, double depth, double tolerance | `List<Brep>` | 见下 |

**BuildPanels 算法**：
1. 对每个 Quad 的 4 个角点沿法线偏移 offset（底面）和 offset+thickness（顶面）
2. 委托 `SurfaceGeo.CreatePlanarBreps` 创建底面和顶面
3. 构建侧面（4 个），委托 `SurfaceGeo.CreatePlanarBreps` 创建
4. 委托 `SolidGeo.CreateSolidFromBreps` 合并为单个实体
5. 失败项跳过

**BuildFrames 算法**：
1. 对每条 GridEdge 沿法线偏移 offset
2. 计算窗框截面定向（沿边方向为长度，法线方向为深度）
3. 委托 `SolidGeo.CreateFromBox` 创建 Box 实体
4. 失败项跳过

---

## 框架复用对照

| 本层方法 | 委托的框架方法 | 命名空间 |
|---------|--------------|---------|
| StructureBeamGeo.Build（放样） | `SurfaceGeo.CreateLoft` | `Rh.Geo.Srf` |
| StructureBeamGeo.Build（合并） | `SolidGeo.CreateSolidFromBreps` | `Rh.Geo.Sld` |
| JointNodeGeo.Build（圆柱） | `SolidGeo.CreateCylinder` | `Rh.Geo.Sld` |
| JointNodeGeo.Build（爪臂 Box） | `SolidGeo.CreateFromBox` | `Rh.Geo.Sld` |
| GlassPanelGeo.BuildPanels（平面面） | `SurfaceGeo.CreatePlanarBreps` | `Rh.Geo.Srf` |
| GlassPanelGeo.BuildPanels（合并） | `SolidGeo.CreateSolidFromBreps` | `Rh.Geo.Sld` |
| GlassPanelGeo.BuildFrames（Box） | `SolidGeo.CreateFromBox` | `Rh.Geo.Sld` |
