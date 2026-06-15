# SubD 命令接口文档

命名空间：`Rh.Cmd.SubD`

## 功能

创建细分曲面对象和执行 SubD 操作（不写入文档，返回 SubD）。

## 默认值机制

- 标注 `[可选]` 的参数可不传入，Command 层自动从 Data 层读取默认值
- 命令执行后，实际使用的值自动更新为新的默认值
- 对应默认值定义见 `Data/Command/basicCommand/SubD.md`

---

## 一、基本体创建

### CreateSubDSphere

| 项目 | 内容 |
|------|------|
| 功能 | 创建 SubD 球体 |
| 对应命令 | SubDSphere |
| 输入 | `Plane plane` — 中心平面，`double radius` — 半径 |
| 输出 | `SubD` — SubD 球体 |
| 报错 | RadiusInvalidException — radius ≤ 0 |

### CreateSubDBox

| 项目 | 内容 |
|------|------|
| 功能 | 创建 SubD 长方体 |
| 对应命令 | SubDBox |
| 输入 | `Plane plane` — 底面平面，`Interval x` — X 方向范围，`Interval y` — Y 方向范围，`Interval z` — Z 方向范围 |
| 输出 | `SubD` — SubD 长方体 |
| 报错 | RangeInvalidException — 范围无效 |

### CreateSubDCylinder

| 项目 | 内容 |
|------|------|
| 功能 | 创建 SubD 圆柱体 |
| 对应命令 | SubDCylinder |
| 输入 | `Plane plane` — 底面平面，`double radius` — 半径，`double height` — 高度，`bool capEnds` — 是否封口 [可选，默认 true] |
| 输出 | `SubD` — SubD 圆柱体 |
| 报错 | RadiusInvalidException — radius/height ≤ 0 |

### CreateSubDCone

| 项目 | 内容 |
|------|------|
| 功能 | 创建 SubD 圆锥体 |
| 对应命令 | SubDCone |
| 输入 | `Plane plane` — 底面平面，`double radius` — 半径，`double height` — 高度，`bool capEnd` — 是否封口 [可选，默认 true] |
| 输出 | `SubD` — SubD 圆锥体 |
| 报错 | RadiusInvalidException — radius/height ≤ 0 |

### CreateSubDTorus

| 项目 | 内容 |
|------|------|
| 功能 | 创建 SubD 圆环体 |
| 对应命令 | SubDTorus |
| 输入 | `Plane plane` — 中心平面，`double majorRadius` — 主半径，`double minorRadius` — 副半径 |
| 输出 | `SubD` — SubD 圆环体 |
| 报错 | RadiusInvalidException — minorRadius ≥ majorRadius 或半径 ≤ 0 |

### CreateSubDPlane

| 项目 | 内容 |
|------|------|
| 功能 | 创建 SubD 平面 |
| 对应命令 | SubDPlane |
| 输入 | `Plane plane` — 所在平面，`Interval x` — X 方向范围，`Interval y` — Y 方向范围 |
| 输出 | `SubD` — SubD 平面 |
| 报错 | RangeInvalidException — 范围无效 |

### CreateSubDQuadball

| 项目 | 内容 |
|------|------|
| 功能 | 创建 SubD 四面球（基于四边形面的球体） |
| 对应命令 | Quadball |
| 输入 | `Plane plane` — 中心平面，`double radius` — 半径 |
| 输出 | `SubD` — SubD 四面球 |
| 报错 | RadiusInvalidException — radius ≤ 0 |

---

## 二、SubD 编辑

### Subdivide

| 项目 | 内容 |
|------|------|
| 功能 | 对 SubD 进行细分（增加网格密度） |
| 对应命令 | Subdivide |
| 输入 | `SubD subd` — 目标 SubD，`int level` — 细分级别（1-N）[可选，默认 1] |
| 输出 | `SubD` — 细分后的 SubD |
| 报错 | LevelInvalidException — level < 1 |

### Crease

| 项目 | 内容 |
|------|------|
| 功能 | 在 SubD 边缘上添加折痕（保持锐边） |
| 对应命令 | Crease |
| 输入 | `SubD subd` — 目标 SubD，`IEnumerable<SubDEdge> edges` — 要添加折痕的边缘，`bool crease` — 添加/移除 [可选，默认 true（添加）] |
| 输出 | `SubD` — 修改后的 SubD |
| 报错 | EdgeInvalidException — 边缘无效 |

### InsertEdge

| 项目 | 内容 |
|------|------|
| 功能 | 在 SubD 面上插入新边（增加细节） |
| 对应命令 | InsertEdge |
| 输入 | `SubD subd` — 目标 SubD，`SubDFace face` — 目标面，`double parameter` — 插入位置参数（0-1），`int direction` — 方向（0=U，1=V）[可选，默认 0] |
| 输出 | `SubD` — 修改后的 SubD |
| 报错 | FaceInvalidException — 面无效；ParameterOutOfRangeException — 参数超出范围 |

### ExtrudeSubDFace

| 项目 | 内容 |
|------|------|
| 功能 | 挤出 SubD 面 |
| 对应命令 | ExtrudeSubD |
| 输入 | `SubD subd` — 目标 SubD，`IEnumerable<SubDFace> faces` — 要挤出的面，`Vector3d direction` — 挤出方向，`double distance` — 挤出距离 |
| 输出 | `SubD` — 挤出后的 SubD |
| 报错 | FaceInvalidException — 面无效；DistanceZeroException — 距离为 0 |

### MergeFaces

| 项目 | 内容 |
|------|------|
| 功能 | 合并相邻的 SubD 面 |
| 对应命令 | MergeFace |
| 输入 | `SubD subd` — 目标 SubD，`IEnumerable<SubDFace> faces` — 要合并的相邻面 |
| 输出 | `SubD` — 合并后的 SubD |
| 报错 | FacesNotAdjacentException — 面不相邻 |

---

## 三、转换

### ToNurbs

| 项目 | 内容 |
|------|------|
| 功能 | 将 SubD 转换为 NURBS 曲面（Brep） |
| 对应命令 | ToNurbs |
| 输入 | `SubD subd` — 源 SubD |
| 输出 | `Brep` — NURBS 曲面 |
| 报错 | SubDInvalidException — SubD 无效 |

### FromNurbs

| 项目 | 内容 |
|------|------|
| 功能 | 将 NURBS 曲面转换为 SubD |
| 对应命令 | ToSubD |
| 输入 | `Brep brep` — 源 NURBS 曲面 |
| 输出 | `SubD` — 转换后的 SubD |
| 报错 | BrepInvalidException — 曲面无效 |

### QuadRemesh

| 项目 | 内容 |
|------|------|
| 功能 | 将网格/NURBS 转换为四边形网格或 SubD |
| 对应命令 | QuadRemesh |
| 输入 | `GeometryBase geometry` — 源网格或 NURBS，`int targetQuadCount` — 目标四边形面数 [可选，默认 4000]，`bool adaptToCurvature` — 适应曲率 [可选，默认 true]，`double density` — 密度 [可选，默认 0.5] |
| 输出 | `SubD` — 四边形 SubD |
| 报错 | GeometryInvalidException — 对象无效；QuadRemeshFailedException — 转换失败 |

---

## 命令汇总

| 分类 | 命令数 | 命令列表 |
|------|--------|---------|
| 基本体创建 | 7 | CreateSubDSphere, CreateSubDBox, CreateSubDCylinder, CreateSubDCone, CreateSubDTorus, CreateSubDPlane, CreateSubDQuadball |
| SubD 编辑 | 5 | Subdivide, Crease, InsertEdge, ExtrudeSubDFace, MergeFaces |
| 转换 | 3 | ToNurbs, FromNurbs, QuadRemesh |
| **合计** | **15** | |
