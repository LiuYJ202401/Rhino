# Mesh 命令接口文档

命名空间：`Rh.Cmd.BasicCommand`

## 功能

创建网格图元、从几何体/点云转换网格（不写入文档，返回 Mesh 或 Mesh[]）。

## 默认值机制

- 标注 `[可选]` 的参数可不传入，Command 层自动从 Data 层读取默认值
- 命令执行后，实际使用的值自动更新为新的默认值
- 对应默认值定义见 `Data/Command/basicCommand/Mesh.json`

---

## 一、图元创建

### CreateMeshBox

对应 Rhino 命令：`MeshBox`

创建网格长方体。提供两个重载。

**重载 1：包围盒**

| 项目 | 说明 |
|------|------|
| 功能 | 从包围盒创建网格长方体 |
| 输入 | `BoundingBox bbox` — 包围盒，`int xCount` — X 方向面数 [可选]，`int yCount` — Y 方向面数 [可选]，`int zCount` — Z 方向面数 [可选]，`bool isPreview = false` |
| 输出 | `Mesh` — 网格长方体 |
| 报错 | 包围盒无效时返回 null |
| RhinoCommon | `Mesh.CreateFromBox(bbox, xCount, yCount, zCount)` |

**重载 2：角点 + 法向量**

| 项目 | 说明 |
|------|------|
| 功能 | 从两对角点和法向量创建网格长方体 |
| 输入 | `Point3d corner1` — 第一角点，`Point3d corner2` — 对角点，`Vector3d normal` — 底面法向量，`int xCount` — X 面数 [可选]，`int yCount` — Y 面数 [可选]，`int zCount` — Z 面数 [可选]，`bool isPreview = false` |
| 输出 | `Mesh` — 网格长方体 |
| 报错 | 角点重合或法向量无效时返回 null |
| RhinoCommon | Geometry 层构造 Box 后调用 `Mesh.CreateFromBox(Box, ...)` |

### CreateMeshSphere

对应 Rhino 命令：`MeshSphere`

创建网格球体。提供两个重载。

**重载 1：中心 + 半径 + 法向量**

| 项目 | 说明 |
|------|------|
| 功能 | 在指定朝向创建网格球体 |
| 输入 | `Point3d center` — 球心，`Vector3d normal` — 方向轴，`double radius` — 半径，`int segments` — 经度分段数 [可选]，`int rings` — 纬度分段数 [可选]，`bool isPreview = false` |
| 输出 | `Mesh` — 网格球体 |
| 报错 | radius ≤ 0 或分段数 < 3 时返回 null |
| RhinoCommon | Geometry 层构造 Sphere 后调用 `Mesh.CreateFromSphere(sphere, segments, rings)` |

**重载 2：二十面体球**

| 项目 | 说明 |
|------|------|
| 功能 | 创建二十面体细分球（顶点均匀分布） |
| 输入 | `Point3d center` — 球心，`Vector3d normal` — 方向轴，`double radius` — 半径，`int subdivisions` — 细分级别 (0~7) [可选]，`bool isPreview = false` |
| 输出 | `Mesh` — 二十面体球 |
| 报错 | radius ≤ 0 或细分级别越界时返回 null |
| RhinoCommon | `Mesh.CreateIcoSphere(sphere, subdivisions)` |

### CreateMeshCylinder

对应 Rhino 命令：`MeshCylinder`

| 项目 | 说明 |
|------|------|
| 功能 | 创建网格圆柱体 |
| 输入 | `Point3d center` — 底面中心，`Vector3d normal` — 方向轴，`double radius` — 半径，`double height` — 高度，`int vertical` — 垂直分段数 [可选]，`int around` — 环向分段数 [可选]，`bool capEnds` — 是否封盖 [可选]，`bool isPreview = false` |
| 输出 | `Mesh` — 网格圆柱 |
| 报错 | radius/height ≤ 0 或分段数 < 3 时返回 null |
| RhinoCommon | Geometry 层构造 Cylinder 后调用 `Mesh.CreateFromCylinder(cyl, vertical, around)` |

### CreateMeshCone

对应 Rhino 命令：`MeshCone`

| 项目 | 说明 |
|------|------|
| 功能 | 创建网格圆锥体 |
| 输入 | `Point3d baseCenter` — 底面中心，`Vector3d normal` — 方向轴，`double bottomRadius` — 底面半径，`double height` — 高度，`int vertical` — 垂直分段数 [可选]，`int around` — 环向分段数 [可选]，`bool capEnd` — 是否封底 [可选]，`bool isPreview = false` |
| 输出 | `Mesh` — 网格圆锥 |
| 报错 | radius/height ≤ 0 或分段数 < 3 时返回 null |
| RhinoCommon | Geometry 层构造 Cone 后调用 `Mesh.CreateFromCone(cone, vertical, around)` |

### CreateMeshTorus

对应 Rhino 命令：`MeshTorus`

| 项目 | 说明 |
|------|------|
| 功能 | 创建网格圆环 |
| 输入 | `Point3d center` — 中心，`Vector3d normal` — 方向轴，`double majorRadius` — 主半径，`double minorRadius` — 副半径，`int majorSegments` — 主方向分段数 [可选]，`int minorSegments` — 副方向分段数 [可选]，`bool isPreview = false` |
| 输出 | `Mesh` — 网格圆环 |
| 报错 | 半径 ≤ 0 或 minorRadius ≥ majorRadius 时返回 null |
| RhinoCommon | Geometry 层构造 Torus 后调用 `Mesh.CreateFromTorus(torus, majorSegments, minorSegments)` |

### CreateMeshEllipsoid

对应 Rhino 命令：`MeshEllipsoid`

| 项目 | 说明 |
|------|------|
| 功能 | 创建网格椭球体 |
| 输入 | `Point3d center` — 中心，`Vector3d normal` — 方向轴，`Vector3d radii` — 三轴半径（x=赤道, y=赤道, z=极），`int segments` — 经度分段数 [可选]，`int rings` — 纬度分段数 [可选]，`bool isPreview = false` |
| 输出 | `Mesh` — 网格椭球 |
| 报错 | 任一轴半径 ≤ 0 时返回 null |
| RhinoCommon | 无直接 API，Geometry 层创建球体后非均匀缩放（`Transform.Scale`） |

### CreateMeshPlane

对应 Rhino 命令：`MeshPlane`

| 项目 | 说明 |
|------|------|
| 功能 | 创建网格平面 |
| 输入 | `Plane plane` — 所在平面，`Interval domainU` — X 方向范围，`Interval domainV` — Y 方向范围，`int xCount` — X 面数 [可选]，`int yCount` — Y 面数 [可选]，`bool isPreview = false` |
| 输出 | `Mesh` — 网格平面 |
| 报错 | 范围为零时返回 null |
| RhinoCommon | `Mesh.CreateFromPlane(plane, domainU, domainV, xCount, yCount)` |

---

## 二、从几何体转换

### CreateMeshFromBrep

对应 Rhino 命令：`Mesh`（从曲面/实体）

| 项目 | 说明 |
|------|------|
| 功能 | 将 Brep（多重曲面/实体）转换为网格 |
| 输入 | `Brep brep` — 源实体，`MeshingParameters parameters` — 网格化参数 [可选，默认 Default]，`bool isPreview = false` |
| 输出 | `Mesh[]` — 每个面的网格数组 |
| 报错 | brep 无效时返回 null |
| RhinoCommon | `Mesh.CreateFromBrep(brep, parameters)` |

### CreateMeshFromSurface

对应 Rhino 命令：`Mesh`（从单个曲面）

| 项目 | 说明 |
|------|------|
| 功能 | 将单个曲面转换为网格 |
| 输入 | `Surface surface` — 源曲面，`MeshingParameters parameters` — 网格化参数 [可选，默认 Default]，`bool isPreview = false` |
| 输出 | `Mesh[]` — 网格数组 |
| 报错 | surface 无效时返回 null |
| RhinoCommon | `Mesh.CreateFromSurface(surface, parameters)` |

### CreateMeshFromPolyline

对应 Rhino 命令：`MeshPolyline`

| 项目 | 说明 |
|------|------|
| 功能 | 从封闭多段线创建网格（拉伸或平面填充） |
| 输入 | `Polyline polyline` — 封闭多段线，`bool isPreview = false` |
| 输出 | `Mesh` — 网格 |
| 报错 | 多段线未封闭或点数 < 3 时返回 null |
| RhinoCommon | `Mesh.CreateFromClosedPolyline(polyline)` |

### CreateMeshFromPlanarBoundary

对应 Rhino 命令：`PlanarMesh`

| 项目 | 说明 |
|------|------|
| 功能 | 从封闭平面曲线创建平面网格 |
| 输入 | `Curve boundary` — 封闭平面曲线，`double tolerance` — 公差 [可选，动态值]，`bool isPreview = false` |
| 输出 | `Mesh` — 平面网格 |
| 报错 | 曲线未封闭或非平面时返回 null |
| RhinoCommon | `Mesh.CreateFromPlanarBoundary(boundary, MeshingParameters.Default, tolerance)` |

### CreateMeshExtrusion

对应 Rhino 命令：`ExtrudeCrv` → Mesh 模式

| 项目 | 说明 |
|------|------|
| 功能 | 沿向量挤出曲线创建网格 |
| 输入 | `Curve profile` — 轮廓曲线，`Vector3d direction` — 挤出方向（长度即高度），`bool isPreview = false` |
| 输出 | `Mesh` — 挤出网格 |
| 报错 | 曲线无效或方向为零向量时返回 null |
| RhinoCommon | `Mesh.CreateExtrusion(profile, direction)` |

---

## 三、从点集创建

### CreateMeshFromPoints

对应 Rhino 命令：`MeshFromPoints`

| 项目 | 说明 |
|------|------|
| 功能 | 从点集创建凸包网格 |
| 输入 | `IEnumerable<Point3d> points` — 点集，`double tolerance` — 公差 [可选，动态值]，`bool isPreview = false` |
| 输出 | `Mesh` — 凸包网格 |
| 报错 | 点数不足或共面时返回 null |
| RhinoCommon | `Mesh.CreateConvexHull3D(points, out facets, tolerance, angleTolerance)` |

### CreateMeshFromTessellation

对应 Rhino 命令：无（API 方法）

| 项目 | 说明 |
|------|------|
| 功能 | 从点集和固定边约束创建三角化网格 |
| 输入 | `IEnumerable<Point3d> points` — 点集，`IEnumerable<IEnumerable<Point3d>> edges` — 固定边，`Plane plane` — 所在平面，`bool allowNewVertices` — 是否允许新增顶点，`bool isPreview = false` |
| 输出 | `Mesh` — 三角化网格 |
| 报错 | 点数不足时返回 null |
| RhinoCommon | `Mesh.CreateFromTessellation(points, edges, plane, allowNewVertices)` |

### CreateMeshPatch

对应 Rhino 命令：`MeshPatch`

| 项目 | 说明 |
|------|------|
| 功能 | 从点集和曲线创建网格曲面（补面） |
| 输入 | `IEnumerable<Point3d> points` — 点集，`IEnumerable<Curve> curves` — 边界曲线 [可选]，`int uSpacing` — U 方向间距 [可选]，`int vSpacing` — V 方向间距 [可选]，`bool isPreview = false` |
| 输出 | `Mesh` — 网格曲面 |
| 报错 | 点数不足时返回 null |
| RhinoCommon | 无直接 API，Geometry 层用 Delaunay 三角化近似 |

---

## 四、重网格化

### CreateQuadRemesh

对应 Rhino 命令：`QuadRemesh`

| 项目 | 说明 |
|------|------|
| 功能 | 对 Brep 或 Mesh 进行四边形重网格化 |
| 输入 | `object geometry` — 源几何体（Brep 或 Mesh），`int targetQuadCount` — 目标四边形数量 [可选]，`double adaptSize` — 自适应尺寸 [可选]，`bool isPreview = false` |
| 输出 | `Mesh` — 四边形网格 |
| 报错 | 几何体无效时返回 null |
| RhinoCommon | `Mesh.QuadRemesh(Brep/Mesh, QuadRemeshParameters)` |

---

## 命令汇总

| 分类 | 方法数 | 重载数 | 命令列表 |
|------|--------|--------|---------|
| 图元创建 | 7 | 9 | CreateMeshBox(2), CreateMeshSphere(2), CreateMeshCylinder, CreateMeshCone, CreateMeshTorus, CreateMeshEllipsoid, CreateMeshPlane |
| 从几何体转换 | 5 | 5 | CreateMeshFromBrep, CreateMeshFromSurface, CreateMeshFromPolyline, CreateMeshFromPlanarBoundary, CreateMeshExtrusion |
| 从点集创建 | 3 | 3 | CreateMeshFromPoints, CreateMeshFromTessellation, CreateMeshPatch |
| 重网格化 | 1 | 1 | CreateQuadRemesh |
| **合计** | **16** | **18** | |
