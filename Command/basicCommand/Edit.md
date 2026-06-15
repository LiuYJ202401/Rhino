# Edit 命令接口文档

命名空间：`Rh.Cmd.Edit`

## 功能

对已有几何对象执行编辑操作（不写入文档，返回编辑后的几何对象）。

## 默认值机制

- 标注 `[可选]` 的参数可不传入，Command 层自动从 Data 层读取默认值
- 命令执行后，实际使用的值自动更新为新的默认值
- 对应默认值定义见 `Data/Command/basicCommand/Edit.md`

---

## 一、曲线编辑

### Trim

| 项目 | 内容 |
|------|------|
| 功能 | 用切割对象修剪目标对象 |
| 对应命令 | Trim |
| 输入 | `Curve target` — 被修剪曲线，`IEnumerable<Curve> cutters` — 切割曲线集合，`double tolerance` — 公差 [可选，动态值] |
| 输出 | `Curve[]` — 修剪后的曲线数组 |
| 报错 | TrimFailedException — 修剪失败 |

### Split

| 项目 | 内容 |
|------|------|
| 功能 | 用切割对象分割目标对象 |
| 对应命令 | Split |
| 输入 | `GeometryBase target` — 被分割对象，`IEnumerable<GeometryBase> cutters` — 切割对象集合，`double tolerance` — 公差 [可选，动态值] |
| 输出 | `GeometryBase[]` — 分割后的对象数组 |
| 报错 | SplitFailedException — 分割失败 |

### Join

| 项目 | 内容 |
|------|------|
| 功能 | 连接多个曲线/曲面为一个对象 |
| 对应命令 | Join |
| 输入 | `IEnumerable<GeometryBase> objects` — 待连接对象集合（曲线或曲面），`double tolerance` — 公差 [可选，动态值] |
| 输出 | `GeometryBase` — 连接后的复合对象（PolyCurve / Brep） |
| 报错 | JoinFailedException — 无法连接 |

### Explode

| 项目 | 内容 |
|------|------|
| 功能 | 炸开复合对象为组成部分 |
| 对应命令 | Explode |
| 输入 | `GeometryBase geometry` — 复合曲线/曲面/Brep |
| 输出 | `GeometryBase[]` — 炸开后的组件数组 |
| 报错 | ExplodeFailedException — 无法炸开时返回原对象 |

### Extend

| 项目 | 内容 |
|------|------|
| 功能 | 延伸曲线到指定边界 |
| 对应命令 | Extend |
| 输入 | `Curve curve` — 待延伸曲线，`CurveEnd side` — 延伸端（Start/End/Both），`double length` — 延伸长度，`CurveExtensionStyle style` — 延伸方式 [可选，默认 Line] |
| 输出 | `Curve` — 延伸后的曲线 |
| 报错 | ExtendFailedException — 延伸失败 |

### Offset

| 项目 | 内容 |
|------|------|
| 功能 | 偏移曲线 |
| 对应命令 | Offset |
| 输入 | `Curve curve` — 源曲线，`Vector3d direction` — 偏移方向（法线），`double distance` — 偏移距离，`double tolerance` — 公差 [可选，动态值]，`CornerStyle cornerStyle` — 角点风格 [可选，默认 Sharp] |
| 输出 | `Curve[]` — 偏移后的曲线数组 |
| 报错 | DistanceZeroException — distance = 0；OffsetFailedException — 偏移失败 |

### Fillet

| 项目 | 内容 |
|------|------|
| 功能 | 对两条曲线倒圆角 |
| 对应命令 | Fillet |
| 输入 | `Curve curve1` — 第一条曲线，`Curve curve2` — 第二条曲线，`double radius` — 圆角半径，`double tolerance` — 公差 [可选，动态值] |
| 输出 | `Curve[]` — 倒角后的曲线数组（含圆角段） |
| 报错 | RadiusInvalidException — radius ≤ 0；NoIntersectionException — 无交点 |

### Chamfer

| 项目 | 内容 |
|------|------|
| 功能 | 对两条曲线倒斜角 |
| 对应命令 | Chamfer |
| 输入 | `Curve curve1` — 第一条曲线，`Curve curve2` — 第二条曲线，`double dist1` — 第一条裁切距离，`double dist2` — 第二条裁切距离 [可选，默认 = dist1]，`double tolerance` — 公差 [可选，动态值] |
| 输出 | `Curve[]` — 倒角后的曲线数组 |
| 报错 | DistanceInvalidException — dist ≤ 0；NoIntersectionException — 无交点 |

### Blend

| 项目 | 内容 |
|------|------|
| 功能 | 在两条曲线端点间创建混合曲线 |
| 对应命令 | Blend |
| 输入 | `Curve curve1` — 第一条曲线，`Curve curve2` — 第二条曲线，`int continuity1` — 第一端连续性 [可选，默认 2（G2）]，`int continuity2` — 第二端连续性 [可选，默认 2（G2）] |
| 输出 | `Curve` — 混合曲线 |
| 报错 | BlendFailedException — 混合失败 |

---

## 二、曲线修改

### Rebuild

| 项目 | 内容 |
|------|------|
| 功能 | 重建曲线（修改控制点数和阶数） |
| 对应命令 | Rebuild |
| 输入 | `Curve curve` — 目标曲线，`int pointCount` — 新控制点数，`int degree` — 新阶数 [可选，默认 3]，`bool preserveTangents` — 保持切线 [可选，默认 true] |
| 输出 | `NurbsCurve` — 重建后的曲线 |
| 报错 | PointCountInvalidException — 点数 < 阶数 |

### Fair

| 项目 | 内容 |
|------|------|
| 功能 | 光顺曲线（减少曲率变化） |
| 对应命令 | Fair |
| 输入 | `Curve curve` — 目标曲线，`double tolerance` — 公差 [可选，动态值]，`int iterations` — 迭代次数 [可选，默认 10] |
| 输出 | `NurbsCurve` — 光顺后的曲线 |
| 报错 | FairFailedException — 光顺失败 |

### ChangeDegree

| 项目 | 内容 |
|------|------|
| 功能 | 修改曲线/曲面的阶数 |
| 对应命令 | ChangeDegree |
| 输入 | `GeometryBase geometry` — 目标曲线/曲面，`int newDegree` — 新阶数（1-11），`bool forward` — 是否仅升阶 [可选，默认 false] |
| 输出 | `GeometryBase` — 修改后的几何对象 |
| 报错 | DegreeInvalidException — 阶数超出范围 |

### CurveBoolean

| 项目 | 内容 |
|------|------|
| 功能 | 对两条或多条闭合曲线执行布尔运算 |
| 对应命令 | CurveBoolean |
| 输入 | `IEnumerable<Curve> curves` — 闭合曲线集合（≥2），`BooleanOperation operation` — 运算类型（Union/Difference/Intersection），`double tolerance` — 公差 [可选，动态值] |
| 输出 | `Curve[]` — 布尔结果曲线数组 |
| 报错 | CurveNotClosedException — 曲线未闭合；BooleanFailedException — 布尔失败 |

### ExtractPt

| 项目 | 内容 |
|------|------|
| 功能 | 提取几何对象的控制点/编辑点/网格顶点为点对象 |
| 对应命令 | ExtractPt |
| 输入 | `GeometryBase geometry` — 目标对象，`ExtractPointType type` — 点类型 [可选，默认 ControlPoint]（ControlPoint/EditPoint/MeshVertex） |
| 输出 | `Point3d[]` — 提取的点数组 |
| 报错 | GeometryInvalidException — 对象无效 |

---

## 三、曲面编辑

### TrimSrf

| 项目 | 内容 |
|------|------|
| 功能 | 用切割曲线修剪曲面 |
| 对应命令 | Trim（曲面模式） |
| 输入 | `Brep target` — 被修剪曲面，`IEnumerable<Curve> cutters` — 切割曲线集合，`double tolerance` — 公差 [可选，动态值] |
| 输出 | `Brep` — 修剪后的曲面 |
| 报错 | TrimFailedException — 修剪失败 |

### Untrim

| 项目 | 内容 |
|------|------|
| 功能 | 移除曲面上的修剪曲线，恢复原始曲面 |
| 对应命令 | Untrim |
| 输入 | `Brep surface` — 修剪过的曲面，`IEnumerable<BrepTrim> trims` — 要移除的修剪 [可选，不传则全部移除] |
| 输出 | `Brep` — 未修剪的曲面 |
| 报错 | BrepInvalidException — 曲面无效 |

### OffsetSrf

| 项目 | 内容 |
|------|------|
| 功能 | 偏移曲面 |
| 对应命令 | OffsetSrf |
| 输入 | `Brep surface` — 源曲面，`double distance` — 偏移距离，`bool bothSides` — 双侧偏移 [可选，默认 false]，`bool solid` — 创建实体 [可选，默认 false]，`double tolerance` — 公差 [可选，动态值] |
| 输出 | `Brep` — 偏移后的曲面/实体 |
| 报错 | DistanceZeroException — 距离为 0；OffsetFailedException — 偏移失败 |

### ExtendSrf

| 项目 | 内容 |
|------|------|
| 功能 | 延伸未修剪的曲面边缘 |
| 对应命令 | ExtendSrf |
| 输入 | `Brep surface` — 目标曲面，`IEnumerable<BrepEdge> edges` — 要延伸的边缘，`double length` — 延伸长度，`ExtensionType type` — 延伸类型 [可选，默认 Smooth]（Smooth/Line/Arc） |
| 输出 | `Brep` — 延伸后的曲面 |
| 报错 | ExtendFailedException — 延伸失败 |

### BlendSrf

| 项目 | 内容 |
|------|------|
| 功能 | 在两个曲面边缘间创建混合曲面 |
| 对应命令 | BlendSrf |
| 输入 | `Brep surface1` — 第一曲面，`Brep surface2` — 第二曲面，`int continuity1` — 第一端连续性 [可选，默认 2（G2）]，`int continuity2` — 第二端连续性 [可选，默认 2（G2）]，`double[] shapeCurves` — 控制形状的截面曲线 [可选] |
| 输出 | `Brep` — 混合曲面 |
| 报错 | BlendFailedException — 混合失败 |

### FilletSrf

| 项目 | 内容 |
|------|------|
| 功能 | 在两个曲面间创建圆角曲面 |
| 对应命令 | FilletSrf |
| 输入 | `Brep surface1` — 第一曲面，`Brep surface2` — 第二曲面，`double radius` — 圆角半径，`double tolerance` — 公差 [可选，动态值]，`bool extend` — 延伸 [可选，默认 true]，`bool trim` — 修剪 [可选，默认 true] |
| 输出 | `Brep[]` — 圆角曲面数组 |
| 报错 | RadiusInvalidException — radius ≤ 0；FilletFailedException — 倒角失败 |

### ConnectSrf

| 项目 | 内容 |
|------|------|
| 功能 | 连接两个曲面（延伸并合并） |
| 对应命令 | ConnectSrf |
| 输入 | `Brep surface1` — 第一曲面，`Brep surface2` — 第二曲面，`double tolerance` — 公差 [可选，动态值] |
| 输出 | `Brep` — 连接后的曲面 |
| 报错 | ConnectFailedException — 连接失败 |

### MergeSrf

| 项目 | 内容 |
|------|------|
| 功能 | 合并两个共边的曲面为一个曲面 |
| 对应命令 | MergeSrf |
| 输入 | `Brep surface1` — 第一曲面，`Brep surface2` — 第二曲面，`bool smooth` — 平滑合并 [可选，默认 true]，`double tolerance` — 公差 [可选，动态值] |
| 输出 | `Brep` — 合并后的单一曲面 |
| 报错 | MergeFailedException — 合并失败（边不匹配） |

### ShrinkTrimmedSrf

| 项目 | 内容 |
|------|------|
| 功能 | 收缩修剪后的曲面底面到修剪边界 |
| 对应命令 | ShrinkTrimmedSrf |
| 输入 | `Brep surface` — 修剪过的曲面 |
| 输出 | `Brep` — 收缩后的曲面 |
| 报错 | BrepInvalidException — 曲面无效 |

---

## 命令汇总

| 分类 | 命令数 | 命令列表 |
|------|--------|---------|
| 曲线编辑 | 9 | Trim, Split, Join, Explode, Extend, Offset, Fillet, Chamfer, Blend |
| 曲线修改 | 5 | Rebuild, Fair, ChangeDegree, CurveBoolean, ExtractPt |
| 曲面编辑 | 9 | TrimSrf, Untrim, OffsetSrf, ExtendSrf, BlendSrf, FilletSrf, ConnectSrf, MergeSrf, ShrinkTrimmedSrf |
| **合计** | **23** | |
