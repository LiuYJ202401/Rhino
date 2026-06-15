# Dimension 命令接口文档

命名空间：`Rh.Cmd.Dimension`

## 功能

创建尺寸标注、文字、引线、图案填充等注释对象（不写入文档，返回注释几何对象）。

## 默认值机制

- 标注 `[可选]` 的参数可不传入，Command 层自动从 Data 层读取默认值
- 命令执行后，实际使用的值自动更新为新的默认值
- 对应默认值定义见 `Data/Command/basicCommand/Dimension.md`

---

## 一、尺寸标注

### CreateDimLinear

| 项目 | 内容 |
|------|------|
| 功能 | 创建线性尺寸标注（水平/垂直/旋转） |
| 对应命令 | DimLinear |
| 输入 | `Plane plane` — 标注所在平面，`Point3d start` — 起点，`Point3d end` — 终点，`Point3d offset` — 标注线偏移位置，`double rotation` — 旋转角度 [可选，默认 0] |
| 输出 | `LinearDimension` — 线性尺寸标注对象 |
| 报错 | PointsCoincidentException — 起点终点重合 |

### CreateDimAligned

| 项目 | 内容 |
|------|------|
| 功能 | 创建对齐尺寸标注（平行于两点连线） |
| 对应命令 | DimAligned |
| 输入 | `Plane plane` — 标注所在平面，`Point3d start` — 起点，`Point3d end` — 终点，`Point3d offset` — 标注线偏移位置 |
| 输出 | `LinearDimension` — 对齐尺寸标注对象 |
| 报错 | PointsCoincidentException — 起点终点重合 |

### CreateDimAngle

| 项目 | 内容 |
|------|------|
| 功能 | 创建角度尺寸标注 |
| 对应命令 | DimAngle |
| 输入 | `Plane plane` — 标注所在平面，`Point3d apex` — 角度顶点，`Point3d start` — 第一边端点，`Point3d end` — 第二边端点，`Point3d offset` — 标注弧线位置 |
| 输出 | `AngularDimension` — 角度尺寸标注对象 |
| 报错 | PointsCoincidentException — 点重合 |

### CreateDimRadius

| 项目 | 内容 |
|------|------|
| 功能 | 创建半径尺寸标注（圆/圆弧） |
| 对应命令 | DimRadius |
| 输入 | `Plane plane` — 标注所在平面，`Arc arc` — 目标圆弧，`Point3d offset` — 引线端点位置 |
| 输出 | `RadialDimension` — 半径标注对象 |
| 报错 | ArcInvalidException — 圆弧无效 |

### CreateDimDiameter

| 项目 | 内容 |
|------|------|
| 功能 | 创建直径尺寸标注（圆） |
| 对应命令 | DimDiameter |
| 输入 | `Plane plane` — 标注所在平面，`Circle circle` — 目标圆，`Point3d offset` — 引线端点位置 |
| 输出 | `RadialDimension` — 直径标注对象 |
| 报错 | CircleInvalidException — 圆无效 |

### CreateDimOrdinate

| 项目 | 内容 |
|------|------|
| 功能 | 创建坐标尺寸标注（X/Y 坐标） |
| 对应命令 | DimOrdinate |
| 输入 | `Plane plane` — 标注所在平面，`Point3d basePoint` — 基准点，`Point3d measuredPoint` — 测量点，`bool measureX` — 测量 X 轴（false=测量 Y 轴），`Point3d offset` — 标注线端点位置 |
| 输出 | `OrdinateDimension` — 坐标标注对象 |
| 报错 | PointsCoincidentException — 点重合 |

---

## 二、文字

### CreateText

| 项目 | 内容 |
|------|------|
| 功能 | 创建文字对象 |
| 对应命令 | Text |
| 输入 | `Plane plane` — 文字所在平面，`string text` — 文字内容，`Point3d location` — 位置，`double height` — 文字高度 [可选，默认 1.0]，`string font` — 字体名 [可选，默认 Arial]，`bool bold` — 粗体 [可选，默认 false]，`bool italic` — 斜体 [可选，默认 false]，`TextJustification justification` — 对齐方式 [可选，默认 Left] |
| 输出 | `TextEntity` — 文字对象 |
| 报错 | TextEmptyException — 文字内容为空 |

---

## 三、引线

### CreateLeader

| 项目 | 内容 |
|------|------|
| 功能 | 创建引线（带箭头的多段线注释） |
| 对应命令 | Leader |
| 输入 | `Plane plane` — 引线所在平面，`IEnumerable<Point3d> points` — 引线顶点（含箭头点和文字点），`string text` — 注释文字 [可选]，`double arrowSize` — 箭头大小 [可选，默认 1.0] |
| 输出 | `Leader` — 引线对象 |
| 报错 | PointsInsufficientException — 顶点数 < 2 |

---

## 四、图案填充

### CreateHatch

| 项目 | 内容 |
|------|------|
| 功能 | 由闭合曲线创建图案填充 |
| 对应命令 | Hatch |
| 输入 | `IEnumerable<Curve> curves` — 闭合边界曲线（按包含关系排列），`int patternIndex` — 图案索引 [可选，默认 0]，`double rotation` — 图案旋转角 [可选，默认 0]，`double scale` — 图案比例 [可选，默认 1.0] |
| 输出 | `Hatch[]` — 填充对象数组 |
| 报错 | CurveNotClosedException — 曲线未闭合；NoBoundaryException — 无有效边界 |

---

## 五、点注释

### CreateDot

| 项目 | 内容 |
|------|------|
| 功能 | 创建点注释（带文字标签的点） |
| 对应命令 | Dot |
| 输入 | `string text` — 注释文字，`Point3d location` — 位置，`double height` — 注释高度 [可选，默认 1.0] |
| 输出 | `Dot` — 点注释对象 |
| 报错 | TextEmptyException — 文字为空 |

---

## 命令汇总

| 分类 | 命令数 | 命令列表 |
|------|--------|---------|
| 尺寸标注 | 6 | CreateDimLinear, CreateDimAligned, CreateDimAngle, CreateDimRadius, CreateDimDiameter, CreateDimOrdinate |
| 文字 | 1 | CreateText |
| 引线 | 1 | CreateLeader |
| 图案填充 | 1 | CreateHatch |
| 点注释 | 1 | CreateDot |
| **合计** | **10** | |
