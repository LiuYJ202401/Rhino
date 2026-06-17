# Surface 命令接口文档

命名空间：`Rh.Cmd.BasicCommand`

## 功能

创建各类曲面/多重曲面对象（不写入文档，返回 Brep 或 Brep[]）。

## 默认值机制

- 标注 `[可选]` 的参数可不传入，Command 层自动从 Data 层读取默认值
- 命令执行后，实际使用的值自动更新为新的默认值
- 对应默认值定义见 `Data/Command/basicCommand/Surface.json`

---

## 一、平面类

### CreatePlane

对应 Rhino 命令：`Plane` / `Plane,3Point` / `Plane,Vertical`

创建矩形平面曲面。提供三个重载。

**重载 1：平面 + UV 范围**

| 项目 | 说明 |
|------|------|
| 功能 | 在指定平面上按 U/V 范围创建矩形平面曲面 |
| 输入 | `Plane plane` — 所在平面，`Interval domainU` — U 方向范围，`Interval domainV` — V 方向范围，`int uDegree = 3` [可选]，`int vDegree = 3` [可选]，`bool isPreview = false` |
| 输出 | `Brep` — 矩形平面 |
| 约束 | pointCount 由 degree+1 自动推导（NURBS 要求 pointCount > degree）；Interval 是平面局部坐标 |
| 报错 | 平面无效或范围为零时输出错误消息并返回 null |
| RhinoCommon | `NurbsSurface.CreateFromPlane(plane, u, v, uDegree, vDegree, uDegree+1, vDegree+1)` → `Brep.CreateFromSurface` |

**重载 2：三点**

对应 Rhino 命令：`Plane,3Point`

| 项目 | 说明 |
|------|------|
| 功能 | 由三个角点确定矩形平面曲面 |
| 输入 | `Point3d first` — 第一点，`Point3d second` — 第二点，`Point3d third` — 第三点，`bool isPreview = false` |
| 输出 | `Brep` — 角点平面 |
| 报错 | 三点共线时输出错误消息并返回 null |
| RhinoCommon | 无直接构造，Geometry 层计算平面+范围后调用 `NurbsSurface.CreateFromPlane` |

**重载 3：垂直平面**

对应 Rhino 命令：`Plane,Vertical`

| 项目 | 说明 |
|------|------|
| 功能 | 创建垂直于工作平面的矩形平面 |
| 输入 | `Point3d start` — 起点，`Point3d end` — 终点，`double height` — 高度，`Vector3d workPlaneNormal` — 工作平面法向量，`bool isPreview = false` |
| 输出 | `Brep` — 垂直矩形平面 |
| 报错 | 高度为 0 或两点重合时输出错误消息并返回 null |
| RhinoCommon | 无直接构造，Geometry 层计算垂直平面后调用 `NurbsSurface.CreateFromPlane` |

### CreatePlaneThroughPt

对应 Rhino 命令：`PlaneThroughPt`

| 项目 | 说明 |
|------|------|
| 功能 | 通过一组点拟合最佳平面（最小二乘法） |
| 输入 | `IEnumerable<Point3d> points` — 点集合（≥3），`bool isPreview = false` |
| 输出 | `Brep` — 拟合平面 |
| 报错 | 点数不足或共线时输出错误消息并返回 null |
| RhinoCommon | `Plane.FitPlaneToPoints` → `NurbsSurface.CreateFromPlane` |

### CreateCutPlane

对应 Rhino 命令：`CutPlane`

| 项目 | 说明 |
|------|------|
| 功能 | 创建穿过一组对象的切割平面 |
| 输入 | `Plane plane` — 切割平面的方向和位置，`IEnumerable<GeometryBase> objects` — 被切割的对象，`bool isPreview = false` |
| 输出 | `Brep` — 切割平面 |
| 报错 | 对象列表为空时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreateCutPlane(Plane, IEnumerable<GeometryBase>)` |

---

## 二、从点创建

### CreateSrfPt

对应 Rhino 命令：`SrfPt`

| 项目 | 说明 |
|------|------|
| 功能 | 由 3 或 4 个角点创建曲面 |
| 输入 | `Point3d p1` — 角点1，`Point3d p2` — 角点2，`Point3d p3` — 角点3，`Point3d p4` — 角点4（`Point3d.Unset` 表示三角形），`bool isPreview = false` |
| 输出 | `Brep` — 角点曲面 |
| 报错 | 角点共线时输出错误消息并返回 null |
| RhinoCommon | `NurbsSurface.CreateFromCorners(p1, p2, p3)` 或 `CreateFromCorners(p1, p2, p3, p4)` → `Brep.CreateFromSurface` |

### CreateSrfThroughPts

对应 Rhino 命令：`SrfPtGrid`

| 项目 | 说明 |
|------|------|
| 功能 | 由点网格插值创建曲面（通过所有点） |
| 输入 | `IEnumerable<Point3d> points` — 点集合（行优先排列），`int uCount` — U 向点数，`int vCount` — V 向点数，`int uDegree` — U 向阶数 [可选，默认 3]，`int vDegree` — V 向阶数 [可选，默认 3]，`bool isPreview = false` |
| 输出 | `Brep` — 插值曲面 |
| 报错 | 点数 ≠ uCount × vCount 或网格过小时输出错误消息并返回 null |
| RhinoCommon | `NurbsSurface.CreateThroughPoints(points, uCount, vCount, uDegree, vDegree, false, false)` → `Brep.CreateFromSurface` |

### CreateSrfControlPts

对应 Rhino 命令：`SrfControlPtGrid`

| 项目 | 说明 |
|------|------|
| 功能 | 由控制点网格创建 NURBS 曲面 |
| 输入 | `IEnumerable<Point3d> points` — 控制点集合（行优先排列），`int uCount` — U 向点数，`int vCount` — V 向点数，`int uDegree` — U 向阶数 [可选，默认 3]，`int vDegree` — V 向阶数 [可选，默认 3]，`bool isPreview = false` |
| 输出 | `Brep` — NURBS 曲面 |
| 报错 | 点数 ≠ uCount × vCount 或网格过小时输出错误消息并返回 null |
| RhinoCommon | `NurbsSurface.CreateFromPoints(points, uCount, vCount, uDegree, vDegree)` → `Brep.CreateFromSurface` |

---

## 三、从曲线创建

### CreateEdgeSrf

对应 Rhino 命令：`EdgeSrf`

| 项目 | 说明 |
|------|------|
| 功能 | 由 2、3 或 4 条边缘曲线创建曲面 |
| 输入 | `IEnumerable<Curve> edges` — 边缘曲线（2-4 条，需按顺序，端点相接），`bool isPreview = false` |
| 输出 | `Brep` — 边缘曲面 |
| 报错 | 边缘数不在 2-4 范围时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreateEdgeSurface(edges)` |

### CreatePlanarSrf

对应 Rhino 命令：`PlanarSrf`

| 项目 | 说明 |
|------|------|
| 功能 | 由平面闭合曲线创建平面曲面 |
| 输入 | `IEnumerable<Curve> curves` — 平面闭合曲线，`bool isPreview = false` |
| 输出 | `Brep[]` — 平面曲面数组（可能多个） |
| 报错 | 曲线未闭合或不在同一平面时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreatePlanarBreps(curves, tolerance)` |

### CreateLoft

对应 Rhino 命令：`Loft`

| 项目 | 说明 |
|------|------|
| 功能 | 通过多条截面曲线放样创建曲面 |
| 输入 | `IEnumerable<Curve> curves` — 截面曲线（≥2），`LoftType loftType` — 放样类型 [可选，默认 Normal]，`bool closed` — 是否闭合 [可选，默认 false]，`Point3d start` — 起始点 [可选，默认 Unset]，`Point3d end` — 终点 [可选，默认 Unset]，`bool isPreview = false` |
| 输出 | `Brep[]` — 放样曲面数组 |
| 报错 | 曲线数 < 2 时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreateFromLoft(curves, start, end, loftType, closed)` |

**LoftType 枚举值**：Normal（标准）、Loose（松散）、Tight（紧密）、Straight（直线）、Uniform（均匀）、UniformDevelopable（可展开）

### CreateNetworkSrf

对应 Rhino 命令：`NetworkSrf`

创建网络曲面。提供两个重载。

**重载 1：自动排序**

| 项目 | 说明 |
|------|------|
| 功能 | 由交叉曲线网络创建曲面，自动区分 U/V 方向 |
| 输入 | `IEnumerable<Curve> curves` — 所有曲线，`int continuity` — 连续性 [可选，默认 1=位置]，`double edgeTolerance` — 边界公差 [可选，动态值]，`double interiorTolerance` — 内部公差 [可选，动态值]，`double angleTolerance` — 角度公差 [可选，动态值]，`bool isPreview = false` |
| 输出 | `Brep` — 网络曲面 |
| 报错 | 曲线不交叉或构建失败时输出错误消息并返回 null |
| RhinoCommon | `NurbsSurface.CreateNetworkSurface(curves, continuity, edgeTol, interiorTol, angleTol, out error)` → `Brep.CreateFromSurface` |

**重载 2：手动指定 U/V**

| 项目 | 说明 |
|------|------|
| 功能 | 手动指定 U 向和 V 向曲线创建网络曲面 |
| 输入 | `IEnumerable<Curve> uCurves` — U 向曲线，`IEnumerable<Curve> vCurves` — V 向曲线，`int continuity` — 连续性 [可选，默认 1]，`double edgeTolerance` — 边界公差 [可选，动态值]，`double interiorTolerance` — 内部公差 [可选，动态值]，`double angleTolerance` — 角度公差 [可选，动态值]，`bool isPreview = false` |
| 输出 | `Brep` — 网络曲面 |
| 报错 | 曲线不交叉或构建失败时输出错误消息并返回 null |
| RhinoCommon | `NurbsSurface.CreateNetworkSurface(uCurves, cont, cont, vCurves, cont, cont, edgeTol, interiorTol, angleTol, out error)` → `Brep.CreateFromSurface` |

---

## 四、扫掠类

### CreateSweep

对应 Rhino 命令：`Sweep1` / `Sweep2`

沿轨道扫掠截面创建曲面。提供两个重载。

**重载 1：单轨**

对应 Rhino 命令：`Sweep1`

| 项目 | 说明 |
|------|------|
| 功能 | 沿一条轨道曲线扫掠截面曲线创建曲面 |
| 输入 | `Curve rail` — 轨道曲线，`IEnumerable<Curve> shapes` — 截面曲线，`bool closed` — 是否闭合 [可选，默认 false]，`bool isPreview = false` |
| 输出 | `Brep[]` — 扫掠曲面数组 |
| 报错 | 轨道无效或截面为空时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreateFromSweep(rail, shapes, closed, tolerance)` |

**重载 2：双轨**

对应 Rhino 命令：`Sweep2`

| 项目 | 说明 |
|------|------|
| 功能 | 沿两条轨道曲线扫掠截面曲线创建曲面 |
| 输入 | `Curve rail1` — 第一轨道，`Curve rail2` — 第二轨道，`IEnumerable<Curve> shapes` — 截面曲线，`bool closed` — 是否闭合 [可选，默认 false]，`bool isPreview = false` |
| 输出 | `Brep[]` — 扫掠曲面数组 |
| 报错 | 轨道无效或截面为空时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreateFromSweep(rail1, rail2, shapes, closed, tolerance)` |

---

## 五、旋转类

### CreateRevolve

对应 Rhino 命令：`Revolve`

轮廓曲线绕轴旋转创建曲面。提供两个重载。

**重载 1：完整旋转**

| 项目 | 说明 |
|------|------|
| 功能 | 轮廓曲线绕轴完整旋转 360° 创建曲面 |
| 输入 | `Curve profile` — 轮廓曲线，`Line axis` — 旋转轴，`bool isPreview = false` |
| 输出 | `Brep` — 旋转曲面 |
| 报错 | 轮廓无效时输出错误消息并返回 null |
| RhinoCommon | `RevSurface.Create(profile, axis, 0, 2π)` → `Brep.CreateFromSurface` |

**重载 2：部分旋转**

| 项目 | 说明 |
|------|------|
| 功能 | 轮廓曲线绕轴旋转指定角度创建曲面 |
| 输入 | `Curve profile` — 轮廓曲线，`Line axis` — 旋转轴，`double startAngle` — 起始角（弧度）[可选，默认 0]，`double endAngle` — 终止角（弧度）[可选，默认 2π]，`bool isPreview = false` |
| 输出 | `Brep` — 旋转曲面 |
| 报错 | 轮廓无效时输出错误消息并返回 null |
| RhinoCommon | `RevSurface.Create(profile, axis, startAngle, endAngle)` → `Brep.CreateFromSurface` |

### CreateRailRevolve

对应 Rhino 命令：`RailRevolve`

| 项目 | 说明 |
|------|------|
| 功能 | 轮廓曲线绕轴旋转同时沿轨道曲线移动创建曲面 |
| 输入 | `Curve profile` — 轮廓曲线，`Curve rail` — 轨道曲线，`Line axis` — 旋转轴，`bool scaleHeight` — 是否沿轨道缩放高度 [可选，默认 false]，`bool isPreview = false` |
| 输出 | `Brep` — 轨道旋转曲面 |
| 报错 | 轴/轮廓/轨道几何关系不满足时输出错误消息并返回 null |
| RhinoCommon | `NurbsSurface.CreateRailRevolvedSurface(profile, rail, axis, scaleHeight)` → `Brep.CreateFromSurface` |

---

## 六、挤出类

### CreateExtrude

对应 Rhino 命令：`ExtrudeCrv` / `ExtrudeCrvAlongCrv` / `ExtrudeCrvTapered` / `ExtrudeCrvToPoint`

挤出曲线创建曲面。提供四个重载。

**重载 1：方向挤出**

对应 Rhino 命令：`ExtrudeCrv`

| 项目 | 说明 |
|------|------|
| 功能 | 沿向量方向挤出曲线创建曲面 |
| 输入 | `Curve profile` — 轮廓曲线，`Vector3d direction` — 挤出方向（长度=距离），`bool isPreview = false` |
| 输出 | `Brep` — 挤出曲面（无封盖） |
| 报错 | 轮廓无效或方向为零向量时输出错误消息并返回 null |
| RhinoCommon | `Surface.CreateExtrusion(profile, direction)` → `Brep.CreateFromSurface` |

**重载 2：沿曲线挤出**

对应 Rhino 命令：`ExtrudeCrvAlongCrv`

| 项目 | 说明 |
|------|------|
| 功能 | 沿路径曲线挤出轮廓曲线创建曲面 |
| 输入 | `Curve profile` — 轮廓曲线，`Curve path` — 路径曲线，`bool cap` — 是否加盖 [可选，默认 false]，`bool isPreview = false` |
| 输出 | `Brep` — 沿路径挤出曲面 |
| 报错 | 轮廓或路径无效时输出错误消息并返回 null |
| RhinoCommon | 无直接挤出沿曲线 API，Geometry 层用 `Brep.CreateFromSweep(path, profile, false, tolerance)` 实现 |

**重载 3：锥状挤出**

对应 Rhino 命令：`ExtrudeCrvTapered`

| 项目 | 说明 |
|------|------|
| 功能 | 以拔模角度锥状挤出曲线创建曲面 |
| 输入 | `Curve profile` — 轮廓曲线，`Vector3d direction` — 挤出方向，`double distance` — 挤出距离，`double draftAngle` — 拔模角度（弧度），`bool isPreview = false` |
| 输出 | `Brep` — 锥状挤出曲面 |
| 报错 | 轮廓无效或拔模角度超出范围时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreateFromTaperedExtrude(profile, distance, direction, profile.CenterPoint(), draftAngle, ExtrudeCornerType.Sharp, tolerance, angleTol)` |

**重载 4：挤出到点**

对应 Rhino 命令：`ExtrudeCrvToPoint`

| 项目 | 说明 |
|------|------|
| 功能 | 将曲线挤出至一点创建锥状曲面 |
| 输入 | `Curve profile` — 轮廓曲线，`Point3d apex` — 目标点，`bool isPreview = false` |
| 输出 | `Brep` — 锥状曲面 |
| 报错 | 轮廓无效或目标点在轮廓上时输出错误消息并返回 null |
| RhinoCommon | `Surface.CreateExtrusionToPoint(profile, apex)` → `Brep.CreateFromSurface` |

---

## 七、补面与拟合

### CreatePatch

对应 Rhino 命令：`Patch`

| 项目 | 说明 |
|------|------|
| 功能 | 通过曲线、点、点云、网格拟合曲面 |
| 输入 | `IEnumerable<GeometryBase> geometry` — 输入对象（曲线/点/点云/网格），`int uSpans` — U 向跨度数 [可选，默认 10]，`int vSpans` — V 向跨度数 [可选，默认 10]，`bool isPreview = false` |
| 输出 | `Brep` — 补面曲面 |
| 报错 | 输入为空时输出错误消息并返回 null |
| RhinoCommon | `Brep.CreatePatch(geometry, uSpans, vSpans, tolerance)` |

---

## 八、特殊曲面

### CreateRibbon

对应 Rhino 命令：`Ribbon`

| 项目 | 说明 |
|------|------|
| 功能 | 偏移曲线并在原曲线和偏移曲线间创建直纹曲面 |
| 输入 | `Curve curve` — 输入曲线，`double distance` — 偏移距离，`Plane plane` — 偏移参考平面，`bool isPreview = false` |
| 输出 | `Brep` — 彩带曲面 |
| 报错 | 曲线无效或距离为 0 时输出错误消息并返回 null |
| RhinoCommon | Geometry 层用 `Curve.Offset` + `NurbsSurface.CreateRuledSurface` 实现 |

### CreateFin

对应 Rhino 命令：`Fin`

| 项目 | 说明 |
|------|------|
| 功能 | 沿曲面法线方向挤出曲面上的曲线 |
| 输入 | `Curve curve` — 曲面上的曲线，`BrepFace face` — 所在曲面（单个面），`double height` — 挤出高度，`bool isPreview = false` |
| 输出 | `Brep` — 翼面曲面 |
| 报错 | 曲线不在曲面上或高度为 0 时输出错误消息并返回 null |
| RhinoCommon | Geometry 层从 BrepFace 获取法线方向后挤出 |

### CreateDrape

对应 Rhino 命令：`Drape`

| 项目 | 说明 |
|------|------|
| 功能 | 向工作平面投影生成覆盖曲面 |
| 输入 | `IEnumerable<GeometryBase> objects` — 被覆盖的对象，`Plane plane` — 投影平面，`int uSpacing` — U 向采样数 [可选，默认 10]，`int vSpacing` — V 向采样数 [可选，默认 10]，`bool isPreview = false` |
| 输出 | `Brep` — 覆盖曲面 |
| 报错 | 对象列表为空时输出错误消息并返回 null |
| RhinoCommon | 无直接构造，Geometry 层手动实现射线投影 + 点网格插值 |

### CreateHeightfield

对应 Rhino 命令：`Heightfield`

| 项目 | 说明 |
|------|------|
| 功能 | 由灰度图像创建曲面 |
| 重载数 | 3 |
| 采样方式 | 点采样（取每个采样区域中心像素，与 Rhino 原生一致） |
| RhinoCommon | `NurbsSurface.CreateThroughPoints`（插值曲面，穿过所有采样点） |

#### 重载 1（完全控制）

用户指定所有物理尺寸和采样密度。

| 项目 | 说明 |
|------|------|
| 输入 | `string imagePath`, `Plane plane`, `double width`, `double heightSize`, `double maxHeight`, `int samplesX = 50`, `int samplesY = 50`, `bool isPreview = false` |
| 输出 | `Brep` — 高度场曲面 |
| 适用 | 需要完全控制物理尺寸和采样精度的场景 |

#### 重载 2（按图片比例自动适配高度）

用户只需指定物理宽度和采样密度，高度由图片宽高比自动推导。

| 项目 | 说明 |
|------|------|
| 输入 | `string imagePath`, `Plane plane`, `double width`, `double maxHeight`, `int samplesX`, `int samplesY`, `bool isPreview = false` |
| 输出 | `Brep` — 高度场曲面（`heightSize = width × imgHeight/imgWidth`） |
| 适用 | 保持图片原始比例，避免拉伸变形 |

#### 重载 3（全自动适配）

用户只需指定物理宽度和最大高度，高度和采样密度都由图片自动推导。

| 项目 | 说明 |
|------|------|
| 输入 | `string imagePath`, `Plane plane`, `double width`, `double maxHeight`, `bool isPreview = false` |
| 输出 | `Brep` — 高度场曲面（自动推导 `heightSize` 和 `samplesX/samplesY`） |
| 适用 | 最简调用，采样密度按图片像素（上限 50 避免过密） |

#### 通用说明

- 图像加载失败时输出错误消息并返回 null
- `imagePath` 必须是**有效的图片文件绝对路径或相对路径**，支持 PNG/JPG/BMP 等常见格式
- 图片的**灰度值**决定高度：白色 = `maxHeight`，黑色 = 0
- 实际使用时，用户可通过文件选择对话框获取路径（如 `Rhino.UI.OpenFileDialog`）
- 测试场景下，测试图片 `test_heightfield.png` 存放在 [Assets 目录](../../Assets/README.md)，通过 csproj 通配符复制到插件输出目录

### CreateDevLoft

对应 Rhino 命令：`DevLoft`

| 项目 | 说明 |
|------|------|
| 功能 | 在两条轨道间创建单一可展开曲面 |
| 输入 | `Curve rail1` — 第一轨道，`Curve rail2` — 第二轨道，`bool isPreview = false` |
| 输出 | `Brep` — 可展开曲面 |
| 报错 | 轨道无效时输出错误消息并返回 null |
| RhinoCommon | 无直接构造，Geometry 层用 `Brep.CreateFromSweep` 近似实现 |

---

## 命令汇总

| 分类 | 方法 | 重载数 | 状态 |
|------|------|--------|------|
| 平面类 | CreatePlane | 3 | ✅ |
| 平面类 | CreatePlaneThroughPt | 1 | ✅ |
| 平面类 | CreateCutPlane | 1 |  |
| 从点创建 | CreateSrfPt | 1 | ✅ |
| 从点创建 | CreateSrfThroughPts | 1 | ✅ |
| 从点创建 | CreateSrfControlPts | 1 | ✅ |
| 从曲线创建 | CreateEdgeSrf | 1 | ✅ |
| 从曲线创建 | CreatePlanarSrf | 1 | ✅ |
| 从曲线创建 | CreateLoft | 1 | ✅ |
| 从曲线创建 | CreateNetworkSrf | 2 | ✅ |
| 扫掠类 | CreateSweep | 2 | ✅ |
| 旋转类 | CreateRevolve | 2 | ✅ |
| 旋转类 | CreateRailRevolve | 1 | ✅ |
| 挤出类 | CreateExtrude | 4 | ✅ |
| 补面与拟合 | CreatePatch | 1 | ✅ |
| 特殊曲面 | CreateRibbon | 1 |  |
| 特殊曲面 | CreateFin | 1 |  |
| 特殊曲面 | CreateDrape | 1 |  |
| 特殊曲面 | CreateHeightfield | 3 |  |
| 特殊曲面 | CreateDevLoft | 1 |  |
| **合计** | **20 方法** | **27 重载** | **20 方法已实现** |

## 归入其他功能区的命令

| 命令 | 归属 | 说明 |
|------|------|------|
| BlendSrf | Edit | 两曲面边缘间创建混合曲面 |
| OffsetSrf | Edit | 偏移曲面 |
| ExtendSrf | Edit | 延伸曲面 |
| Trim | Edit | 修剪曲面 |
| Untrim | Edit | 取消修剪 |
| FilletSrf | Edit | 曲面倒角 |
| ConnectSrf | Edit | 连接曲面 |
| MergeSrf | Edit | 合并曲面 |
| ShrinkTrimmedSrf | Edit | 收缩修剪后的曲面 |
