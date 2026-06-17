# Solid 命令接口文档

命名空间：`Rh.Cmd.BasicCommand`

## 功能

创建闭合实体对象（Brep，`IsSolid=true`）。与 Surface 功能区的核心区别：Solid 版本强制调用 `CapPlanarHoles` 或使用带 `cap` 参数的 API，使结果闭合。

## 关键约束：RhinoCommon 返回值语义

以下 API 返回**新的独立对象**，不修改原对象。调用者必须接收返回值，否则结果被丢弃：

| API | 错误用法（丢弃返回值） | 正确用法 |
|-----|---------------------|---------|
| `Brep.CapPlanarHoles(tol)` | `brep.CapPlanarHoles(tol);` | `brep = brep.CapPlanarHoles(tol);` |
| `Brep.JoinBreps(breps, tol)` | （必须接收数组） | `var arr = Brep.JoinBreps(...);` |
| `Brep.CreateBooleanXxx(...)` | （必须接收数组） | `var arr = Brep.CreateBooleanXxx(...);` |

> 本文档中所有标注 `→ CapPlanarHoles` 的方法，其 Geometry 层实现必须遵守此约束。
> 同类陷阱：`PointCloud` 的 Add/Remove 也返回新对象（见 Point.md）。

## 默认值机制

- 标注 `[可选]` 的参数可不传入，Command 层自动从 Data 层读取默认值
- 命令执行后，实际使用的值自动更新为新的默认值
- 对应默认值定义见 `Data/Command/basicCommand/Solid.json`

---

## 一、单曲面实体

### CreateSphere

对应 Rhino 命令：`Sphere`

| 项目 | 说明 |
|------|------|
| 功能 | 创建闭合球体（单一曲面） |
| 输入 | `Point3d center` — 球心，`Vector3d normal` — 方向轴，`double radius` — 半径，`bool isPreview = false` |
| 输出 | `Brep` — 球体实体 |
| 报错 | radius ≤ 0 时输出错误消息并返回 null |
| RhinoCommon | Geometry 层构造 Sphere 后 `sphere.ToBrep()` |

### CreateEllipsoid

对应 Rhino 命令：`Ellipsoid`

| 项目 | 说明 |
|------|------|
| 功能 | 创建三轴椭球体 |
| 输入 | `Point3d center` — 中心，`Vector3d normal` — 方向轴，`Vector3d radii` — 三轴半径，`bool isPreview = false` |
| 输出 | `Brep` — 椭球实体 |
| 报错 | 任一轴半径 ≤ 0 时输出错误消息并返回 null |
| RhinoCommon | 无直接 API，Sphere + `Transform.Scale` |

### CreateTorus

对应 Rhino 命令：`Torus`

| 项目 | 说明 |
|------|------|
| 功能 | 创建圆环体 |
| 输入 | `Point3d center` — 中心，`Vector3d normal` — 方向轴，`double majorRadius` — 主半径，`double minorRadius` — 副半径，`bool isPreview = false` |
| 输出 | `Brep` — 圆环实体 |
| 报错 | 半径 ≤ 0 或 minorRadius ≥ majorRadius 时输出错误消息并返回 null |
| RhinoCommon | Geometry 层构造 Torus 后 `torus.ToRevSurface()` → `Brep.CreateFromSurface` |

---

## 二、多曲面实体

### CreateBox

对应 Rhino 命令：`Box`

提供两个重载。

**重载 1：Box 结构**

| 项目 | 说明 |
|------|------|
| 功能 | 从 Box 结构创建长方体 |
| 输入 | `Box box` — 盒体，`bool isPreview = false` |
| 输出 | `Brep` — 长方体实体 |
| 报错 | Box 无效时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreateFromBox(box)` |

**重载 2：底面角点 + 法向量（法向量长度=高度）**

| 项目 | 说明 |
|------|------|
| 功能 | 从两底面角点和法向量创建长方体（法向量的方向定义底面朝向，长度定义高度） |
| 输入 | `Point3d corner1` — 底面第一角点，`Point3d corner2` — 底面对角点，`Vector3d normal` — 方向=底面法线，长度=高度，`bool isPreview = false` |
| 输出 | `Brep` — 长方体实体 |
| 约束 | normal.Length = 高度（禁止 Unitize）；corner1/corner2 是底面对角点（非盒体对角点） |
| 报错 | 法向量为零向量、或两角点在底面上重合时输出错误消息并返回 null |
| RhinoCommon | Geometry 层用 `RemapToPlaneSpace` 转局部坐标后构造 Box，再 `Brep.CreateFromBox` |

### CreateCylinder

对应 Rhino 命令：`Cylinder`

| 项目 | 说明 |
|------|------|
| 功能 | 创建圆柱实体（侧面 + 上下底盖） |
| 输入 | `Point3d baseCenter` — 底面中心，`Vector3d normal` — 方向轴，`double radius` — 半径，`double height` — 高度，`bool capEnds` — 是否封盖 [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 圆柱实体 |
| 报错 | radius/height ≤ 0 时输出错误消息并返回 null |
| RhinoCommon | Geometry 层构造 Cylinder 后 `cylinder.ToBrep(capBottom, capTop)` |

### CreateCone

对应 Rhino 命令：`Cone`

| 项目 | 说明 |
|------|------|
| 功能 | 创建圆锥实体 |
| 输入 | `Point3d baseCenter` — 底面中心，`Vector3d normal` — 方向轴，`double bottomRadius` — 底面半径，`double height` — 高度，`bool capEnd` — 是否封底 [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 圆锥实体 |
| 报错 | radius/height ≤ 0 时输出错误消息并返回 null |
| RhinoCommon | Geometry 层构造 Cone 后 `cone.ToBrep(capBase)` |

### CreateTruncatedCone

对应 Rhino 命令：`TCone`

| 项目 | 说明 |
|------|------|
| 功能 | 创建截锥体（圆台）实体 |
| 输入 | `Point3d baseCenter` — 底面中心，`Vector3d normal` — 方向轴，`double bottomRadius` — 底面半径，`double topRadius` — 顶面半径，`double height` — 高度，`bool capEnds` — 是否封盖 [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 截锥实体 |
| 约束 | 用 RevSurface（旋转曲面）构造，不用 Loft（接缝不对齐导致封盖失败） |
| 报错 | radius ≤ 0 或 height ≤ 0 时输出错误消息并返回 null |
| RhinoCommon | `RevSurface.Create` + `Brep.CreateFromRevSurface(capBottom, capTop)` |

### CreateTube

对应 Rhino 命令：`Tube`

| 项目 | 说明 |
|------|------|
| 功能 | 创建中空圆柱实体 |
| 输入 | `Point3d baseCenter` — 底面中心，`Vector3d normal` — 方向轴，`double innerRadius` — 内半径，`double outerRadius` — 外半径，`double height` — 高度，`bool capEnds` — 是否封盖 [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 管体实体 |
| 报错 | innerRadius ≥ outerRadius 或半径/高度 ≤ 0 时输出错误消息并返回 null |
| RhinoCommon | 外圆柱 + 内圆柱 `Brep.CreateBooleanDifference` |

### CreatePyramid

对应 Rhino 命令：`Pyramid`

| 项目 | 说明 |
|------|------|
| 功能 | 创建 n 边棱锥实体 |
| 输入 | `Point3d baseCenter` — 底面中心，`Vector3d normal` — 方向轴，`int sides` — 边数 (≥3)，`double radius` — 底面外接圆半径，`double height` — 高度，`bool capBase` — 是否封底 [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 棱锥实体 |
| 约束 | 拼侧面 → JoinBreps → CapPlanarHoles（不用 Append + 手动底盖曲线） |
| 报错 | sides < 3 或 radius/height ≤ 0 时输出错误消息并返回 null |
| RhinoCommon | 侧面用 `Brep.CreateFromCornerPoints`，合并用 `Brep.JoinBreps`，封盖用 `CapPlanarHoles` |

### CreateTruncatedPyramid

对应 Rhino 命令：`TruncatedPyramid`

| 项目 | 说明 |
|------|------|
| 功能 | 创建截棱锥体（棱台）实体 |
| 输入 | `Point3d baseCenter` — 底面中心，`Vector3d normal` — 方向轴，`int sides` — 边数，`double bottomRadius` — 底面外接圆半径，`double topRadius` — 顶面外接圆半径，`double height` — 高度，`bool capEnds` — 是否封盖 [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 截棱锥实体 |
| 约束 | 拼侧面 → JoinBreps → CapPlanarHoles（同 CreatePyramid） |
| 报错 | sides < 3 或半径/高度 ≤ 0 时输出错误消息并返回 null |
| RhinoCommon | 侧面用 `Brep.CreateFromCornerPoints`，合并用 `Brep.JoinBreps`，封盖用 `CapPlanarHoles` |

---

## 三、从曲线挤出为实体

### CreateExtrudeSolid

对应 Rhino 命令：`ExtrudeCrv`（Solid 模式）

| 项目 | 说明 |
|------|------|
| 功能 | 闭合平面曲线沿方向挤出为闭合实体（侧面 + 封盖） |
| 输入 | `Curve profile` — 闭合平面曲线，`Vector3d direction` — 挤出方向，`bool capEnds` — 是否封盖 [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 挤出实体 |
| 报错 | 曲线未闭合或方向为零向量时输出错误消息并返回 null |
| RhinoCommon | `Surface.CreateExtrusion` → `Brep.CreateFromSurface` → `CapPlanarHoles` |

> **实现约束（挤出方向）**：挤出方向**不能与轮廓平面平行**（即方向向量不能在平面内）。
> 否则轮廓中与挤出方向平行的边会扫掠出零面积侧面，导致 Brep 退化，`CapPlanarHoles` 无法封盖。
> 安全做法：挤出方向沿轮廓平面的法向，或至少有显著法向分量。

### CreateRevolveSolid

对应 Rhino 命令：`Revolve`（Solid 模式）

| 项目 | 说明 |
|------|------|
| 功能 | 闭合轮廓绕轴旋转后封盖形成实体 |
| 输入 | `Curve profile` — 闭合平面曲线，`Line axis` — 旋转轴，`double startAngle` — 起始角（弧度），`double endAngle` — 终止角（弧度），`bool capEnds` — 是否封盖 [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 旋转实体 |
| 报错 | 曲线/轴无效时输出错误消息并返回 null |
| RhinoCommon | `RevSurface.Create` → `Brep.CreateFromSurface` → `CapPlanarHoles` |

### CreateSweepSolid

对应 Rhino 命令：`Sweep2`（Solid 模式）

| 项目 | 说明 |
|------|------|
| 功能 | 闭合截面沿双轨扫掠后封盖形成实体 |
| 输入 | `Curve rail1` — 轨道 1，`Curve rail2` — 轨道 2，`IEnumerable<Curve> sections` — 截面曲线（闭合），`bool capEnds` — 是否封盖 [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 扫掠实体 |
| 报错 | 轨道/截面无效时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreateFromSweep` → `CapPlanarHoles` |

### CreateLoftSolid

对应 Rhino 命令：`Loft`（Solid 模式）

| 项目 | 说明 |
|------|------|
| 功能 | 多条闭合截面放样后封盖形成实体 |
| 输入 | `IEnumerable<Curve> curves` — 截面曲线（闭合），`int loftType` — 放样类型 (0=Normal,1=Loose,2=Tight,3=Straight,4=Developable) [可选]，`bool capEnds` — 是否封盖 [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 放样实体 |
| 报错 | 截面数 < 2 时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreateFromLoft` → `CapPlanarHoles` |

---

## 四、Solid 专属命令

### CreatePipe

对应 Rhino 命令：`Pipe`

提供两个重载。

**重载 1：单壁管道**

| 项目 | 说明 |
|------|------|
| 功能 | 沿曲线创建圆形截面管道（单壁） |
| 输入 | `Curve rail` — 路径曲线，`double radius` — 半径，`int capMode` — 封盖模式 (0=None,1=Flat,2=Round) [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 管道实体 |
| 报错 | 曲线无效或 radius ≤ 0 时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreatePipe(rail, radius, false, capMode, true, tolerance, angleTol)` |

**重载 2：双壁厚壁管道**

| 项目 | 说明 |
|------|------|
| 功能 | 沿曲线创建有壁厚的管道（内外双半径） |
| 输入 | `Curve rail` — 路径曲线，`double innerRadius` — 内半径，`double outerRadius` — 外半径，`int capMode` — 封盖模式 [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 厚壁管道实体 |
| 报错 | 曲线无效或半径 ≤ 0 时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreateThickPipe(rail, parameters, innerR, outerR, false, capMode, true, tolerance, angleTol)` |

### CreateSlab

对应 Rhino 命令：`Slab`

| 项目 | 说明 |
|------|------|
| 功能 | 偏移多段线并挤出加盖形成实体板 |
| 输入 | `PolylineCurve profile` — 轮廓多段线，`double offsetDistance` — 偏移距离，`Vector3d direction` — 挤出方向，`bool capEnds` — 是否封盖 [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 实体板 |
| 报错 | 轮廓无效时输出错误消息并返回 null |
| RhinoCommon | 无直接 API，Geometry 层用 `Curve.Offset` + `CreateRuledSurface` + `CapPlanarHoles` |

### CreateTextObject

对应 Rhino 命令：`TextObject`

| 项目 | 说明 |
|------|------|
| 功能 | 由 TrueType 字体创建 3D 实体文字 |
| 输入 | `string text` — 文字内容，`Plane plane` — 文字所在平面，`double textHeight` — 文字高度，`double solidThickness` — 挤出深度，`string fontName` — 字体名 [可选]，`bool bold` — 粗体 [可选]，`bool italic` — 斜体 [可选]，`bool isPreview = false` |
| 输出 | `Brep[]` — 每个字母一个实体 |
| 报错 | 文字为空或字体不存在时输出错误消息并返回 null |
| RhinoCommon | `TextEntity.CreateCurves` → `Brep.CreatePlanarBreps` → `BrepFace.CreateExtrusion(cap=true)` |

### CreateThicken

对应 Rhino 命令：`Thicken`

| 项目 | 说明 |
|------|------|
| 功能 | 将开放曲面偏移加厚形成闭合实体 |
| 输入 | `Brep brep` — 源曲面，`double distance` — 厚度，`bool bothSides` — 双向偏移 [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 加厚实体 |
| 报错 | brep 无效时输出错误消息并返回 null |
| RhinoCommon | 遍历面 `Brep.CreateFromOffsetFace(createSolid=true)` |

---

## 五、辅助命令

### CreateCap

对应 Rhino 命令：`Cap` / `CapPlanarHoles`

| 项目 | 说明 |
|------|------|
| 功能 | 为开放多重曲面的平面开口加盖 |
| 输入 | `Brep brep` — 开放实体，`bool isPreview = false` |
| 输出 | `Brep` — 闭合实体（或原始 brep 如无法加盖） |
| 报错 | brep 无效时输出错误消息并返回 null |
| RhinoCommon | `brep.CapPlanarHoles(tolerance)` |

### CreateSolidFromBreps

对应 Rhino 命令：`CreateSolid`

| 项目 | 说明 |
|------|------|
| 功能 | 多个相交曲面自动裁剪并合并为闭合实体 |
| 输入 | `IEnumerable<Brep> breps` — 相交的面集合，`bool isPreview = false` |
| 输出 | `Brep` — 合并后的闭合实体 |
| 报错 | 输入为空时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreateSolid(breps, tolerance)` |

---

## 命令汇总

| 分类 | 方法数 | 重载数 | 命令列表 |
|------|--------|--------|---------|
| 单曲面实体 | 3 | 3 | CreateSphere, CreateEllipsoid, CreateTorus |
| 多曲面实体 | 7 | 8 | CreateBox(2), CreateCylinder, CreateCone, CreateTruncatedCone, CreateTube, CreatePyramid, CreateTruncatedPyramid |
| 曲线挤出实体 | 4 | 4 | CreateExtrudeSolid, CreateRevolveSolid, CreateSweepSolid, CreateLoftSolid |
| Solid 专属 | 4 | 5 | CreatePipe(2), CreateSlab, CreateTextObject, CreateThicken |
| 辅助 | 2 | 2 | CreateCap, CreateSolidFromBreps |
| **合计** | **20** | **22** | |
