# Analyze 命令接口文档

命名空间：`Rh.Cmd.Analyze`

## 功能

对几何对象执行分析计算，返回测量结果（只读，不修改对象）。

## 默认值机制

- 标注 `[可选]` 的参数可不传入，Command 层自动从 Data 层读取默认值
- 命令执行后，实际使用的值自动更新为新的默认值
- 对应默认值定义见 `Data/Command/basicCommand/Analyze.md`

---

## 一、距离与角度

### Distance

| 项目 | 内容 |
|------|------|
| 功能 | 计算两点间距离 |
| 对应命令 | Distance |
| 输入 | `Point3d p1` — 第一点，`Point3d p2` — 第二点 |
| 输出 | `double` — 距离值 |
| 报错 | PointInvalidException — 点无效，返回 `double.NaN` |

### Angle

| 项目 | 内容 |
|------|------|
| 功能 | 计算三点构成的夹角 |
| 对应命令 | Angle |
| 输入 | `Point3d start` — 起点，`Point3d vertex` — 顶点，`Point3d end` — 终点 |
| 输出 | `double` — 角度（弧度） |
| 报错 | PointsCoincidentException — 点重合，返回 `double.NaN` |

### AngleBetweenVectors

| 项目 | 内容 |
|------|------|
| 功能 | 计算两向量夹角 |
| 对应命令 | — |
| 输入 | `Vector3d v1` — 向量1，`Vector3d v2` — 向量2 |
| 输出 | `double` — 角度（弧度） |
| 报错 | ZeroVectorException — 任一向量为零向量，返回 `double.NaN` |

---

## 二、长度/面积/体积

### Length

| 项目 | 内容 |
|------|------|
| 功能 | 计算曲线长度 |
| 对应命令 | Length |
| 输入 | `Curve curve` — 目标曲线 |
| 输出 | `double` — 长度值 |
| 报错 | CurveInvalidException — 曲线无效，返回 0 |

### Area

| 项目 | 内容 |
|------|------|
| 功能 | 计算闭合曲线/曲面/实体的面积 |
| 对应命令 | Area |
| 输入 | `GeometryBase geometry` — 目标对象 |
| 输出 | `double` — 面积值 |
| 报错 | GeometryInvalidException — 对象无法计算面积，返回 0 |

### AreaCentroid

| 项目 | 内容 |
|------|------|
| 功能 | 计算闭合曲线/曲面的面积质心及质量属性 |
| 对应命令 | AreaCentroid |
| 输入 | `GeometryBase geometry` — 目标对象，`double tolerance` — 公差 [可选，动态值] |
| 输出 | `AreaMassProperties` — 含质心、面积、惯性矩等 |
| 报错 | AnalysisFailedException — 计算失败 |

### Volume

| 项目 | 内容 |
|------|------|
| 功能 | 计算闭合实体的体积 |
| 对应命令 | Volume |
| 输入 | `Brep brep` — 闭合实体 |
| 输出 | `double` — 体积值 |
| 报错 | SolidNotClosedException — 实体不闭合，返回 0 |

### VolumeCentroid

| 项目 | 内容 |
|------|------|
| 功能 | 计算闭合实体的体积质心及质量属性 |
| 对应命令 | VolumeCentroid |
| 输入 | `Brep brep` — 闭合实体，`double tolerance` — 公差 [可选，动态值] |
| 输出 | `VolumeMassProperties` — 含质心、体积、惯性矩等 |
| 报错 | SolidNotClosedException — 实体不闭合；AnalysisFailedException — 计算失败 |

---

## 三、几何分析

### BoundingBox

| 项目 | 内容 |
|------|------|
| 功能 | 计算对象包围盒 |
| 对应命令 | BoundingBox |
| 输入 | `GeometryBase geometry` — 目标对象，`Plane coordinateSystem` — 坐标系 [可选，默认世界坐标系] |
| 输出 | `BoundingBox` — 轴对齐包围盒 |
| 报错 | GeometryInvalidException — 对象无效，返回 `BoundingBox.Empty` |

### CurvatureAnalysis

| 项目 | 内容 |
|------|------|
| 功能 | 计算曲面指定点处的曲率 |
| 对应命令 | CurvatureAnalysis |
| 输入 | `Surface surface` — 目标曲面，`double u` — U 参数，`double v` — V 参数 |
| 输出 | `SurfaceCurvature` — 含高斯曲率、平均曲率、主曲率等 |
| 报错 | ParameterOutOfRangeException — 参数超出范围 |

### ClosestPoint

| 项目 | 内容 |
|------|------|
| 功能 | 查找曲线上/曲面上距离指定点最近的点 |
| 对应命令 | ClosestPt |
| 输入 | `GeometryBase geometry` — 目标曲线/曲面，`Point3d testPoint` — 测试点 |
| 输出 | `Point3d` — 最近点 |
| 报错 | GeometryInvalidException — 对象无效 |

### PointContainment

| 项目 | 内容 |
|------|------|
| 功能 | 测试点是否在闭合曲线/曲面/实体内部 |
| 对应命令 | PtInCrv / PtInSrf |
| 输入 | `GeometryBase geometry` — 目标闭合对象，`Point3d point` — 测试点，`double tolerance` — 公差 [可选，动态值] |
| 输出 | `PointContainment` — 包含关系（Inside/Outside/Coincident） |
| 报错 | GeometryInvalidException — 对象无效；GeometryNotClosedException — 对象未闭合 |

---

## 命令汇总

| 分类 | 命令数 | 命令列表 |
|------|--------|---------|
| 距离与角度 | 3 | Distance, Angle, AngleBetweenVectors |
| 长度/面积/体积 | 5 | Length, Area, AreaCentroid, Volume, VolumeCentroid |
| 几何分析 | 4 | BoundingBox, CurvatureAnalysis, ClosestPoint, PointContainment |
| **合计** | **12** | |
