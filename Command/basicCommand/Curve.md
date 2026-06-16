# Curve 命令接口文档

命名空间：`Rh.Cmd.BasicCommand`

## 功能

创建各类曲线对象（不写入文档，返回几何对象）。

## 默认值机制

- 标注 `[可选]` 的参数可不传入，Command 层自动从 Data 层读取默认值
- 命令执行后，实际使用的值自动更新为新的默认值
- 对应默认值定义见 `Data/Command/basicCommand/Curve.md`

---

## 线与多段线

### CreateLine

对应 Rhino 命令：`Line`

创建直线。提供两个重载。

**重载 1：两点**

| 项目 | 说明 |
|------|------|
| 功能 | 由两点创建单段直线 |
| 输入 | `Point3d start` — 起点，`Point3d end` — 终点 |
| 输出 | `Line` — 线段对象 |
| 报错 | 两点重合时返回 `Line.Unset` |
| RhinoCommon | `new Line(start, end)` |

**重载 2：多点拟合**

对应 Rhino 命令：`LineThroughPt`

| 项目 | 说明 |
|------|------|
| 功能 | 通过多个点拟合一条最佳直线 |
| 输入 | `IEnumerable<Point3d> points` — 点集合（≥2 个点） |
| 输出 | `Line` — 拟合直线 |
| 报错 | 点数 < 2 时返回 `Line.Unset` |
| RhinoCommon | `Line.TryFitLineToPoints(points, out line)` |

### CreatePolyline

对应 Rhino 命令：`Polyline`

| 项目 | 说明 |
|------|------|
| 功能 | 由点集创建多段线（可含线段和圆弧段） |
| 输入 | `IEnumerable<Point3d> points` — 顶点集合，`bool closed` — 是否闭合 [可选，默认 false] |
| 输出 | `Polyline` — 多段线对象 |
| 报错 | 点数 < 2 时返回 null |

### CreateRectangle

对应 Rhino 命令：`Rectangle`

创建矩形。提供两个重载。

**重载 1：对角点**

| 项目 | 说明 |
|------|------|
| 功能 | 在指定平面以两对角点创建矩形 |
| 输入 | `Plane plane` — 所在平面，`Point3d corner1` — 第一角点，`Point3d corner2` — 对角点 |
| 输出 | `Polyline` — 矩形多段线（闭合） |
| 报错 | 两点重合时返回 null |
| RhinoCommon | 无直接构造，手动计算四角点 |

**重载 2：中心 + 宽高**

对应 Rhino 命令：`Rectangle` > Center

| 项目 | 说明 |
|------|------|
| 功能 | 以中心点方式创建矩形 |
| 输入 | `Plane plane` — 所在平面，`Point3d center` — 中心，`double width` — 宽度，`double height` — 高度 |
| 输出 | `Polyline` — 矩形多段线（闭合） |
| 报错 | width/height ≤ 0 时返回 null |
| RhinoCommon | 无直接构造，手动计算四角点 |

### CreatePolygon

对应 Rhino 命令：`Polygon`

创建正多边形。通过参数类型区分不同的创建方式，共 3 个重载。

**重载 1：外接圆方式**

对应 Rhino 命令：`Polygon` > Center

| 项目 | 说明 |
|------|------|
| 功能 | 以中心和外接圆半径创建正多边形 |
| 输入 | `Plane plane` — 所在平面，`Point3d center` — 中心，`int sides` — 边数，`double radius` — 外接圆半径 |
| 输出 | `Polyline` — 多边形多段线（闭合） |
| 报错 | sides < 3 或 radius ≤ 0 时返回 null |
| RhinoCommon | 无直接构造，手动计算顶点 |

**重载 2：边长方式**

对应 Rhino 命令：`Polygon` > Edge

| 项目 | 说明 |
|------|------|
| 功能 | 以一条边的两端点创建正多边形 |
| 输入 | `Plane plane` — 所在平面，`Point3d start` — 边起点，`Point3d end` — 边终点，`int sides` — 边数 |
| 输出 | `Polyline` — 多边形多段线（闭合） |
| 报错 | sides < 3 或两点重合时返回 null |
| RhinoCommon | 无直接构造，先算外接圆半径再生成顶点 |

**重载 3：星形（内外半径）**

对应 Rhino 命令：`Polygon` > Star

| 项目 | 说明 |
|------|------|
| 功能 | 以中心、内外半径创建星形多边形 |
| 输入 | `Plane plane` — 所在平面，`Point3d center` — 中心，`int sides` — 角数，`double outerRadius` — 外半径，`double innerRadius` — 内半径 |
| 输出 | `Polyline` — 星形多段线（闭合） |
| 报错 | sides < 3 或半径 ≤ 0 时返回 null |
| RhinoCommon | 无直接构造，手动交替内外半径顶点 |

---

## 圆

RhinoCommon 提供 `Rhino.Geometry.Circle` 结构体的 7 种构造函数，本层统一封装为 `CreateCircle` 的多个重载。

### RhinoCommon 构造函数一览

| 构造函数 | 说明 | 本层封装 |
|---------|------|---------|
| `Circle(Plane plane, double radius)` | 平面原点 + 半径 | CreateCircle（重载 3） |
| `Circle(Plane plane, Point3d center, double radius)` | 平面 + 圆心 + 半径 | CreateCircle（重载 1） |
| `Circle(Point3d p1, Point3d p2, Point3d p3)` | 三点定圆 | CreateCircle（重载 5） |
| `Circle(Point3d start, Vector3d tangent, Point3d end)` | 起点 + 切向 + 终点 | CreateCircle（重载 6） |
| `Circle(Arc arc)` | 从圆弧转换 | 不单独封装（Arc 可隐式转换） |

> 注：`Circle(Point3d center, double radius)` 和 `Circle(double radius)` 隐式 WorldXY，本层不封装。圆心 + 法向量方式见重载 2。
> 直径方式和相切曲线方式无直接构造函数，由 Geometry 层手动计算。

### CreateCircle

对应 Rhino 命令：`Circle`

创建圆。通过参数类型区分不同的创建方式，共 7 个重载。

**重载 1：平面 + 圆心 + 半径**

| 项目 | 说明 |
|------|------|
| 功能 | 在指定平面上以圆心和半径创建圆 |
| 输入 | `Plane plane` — 所在平面，`Point3d center` — 圆心，`double radius` — 半径 |
| 输出 | `Circle` — 圆对象 |
| 报错 | radius ≤ 0 时返回 `Circle.Unset` |
| RhinoCommon | `new Circle(plane, center, radius)` |

**重载 2：圆心 + 法向量 + 半径**

| 项目 | 说明 |
|------|------|
| 功能 | 以圆心和法向量确定平面后创建圆 |
| 输入 | `Point3d center` — 圆心，`Vector3d normal` — 平面法向量，`double radius` — 半径 |
| 输出 | `Circle` — 圆对象 |
| 报错 | radius ≤ 0 或 normal 为零时返回 `Circle.Unset` |
| RhinoCommon | `new Plane(center, normal)` → Geometry 层 `CircleGeo.CreateFromCenterRadius` |

**重载 3：平面 + 半径（原点为圆心）**

| 项目 | 说明 |
|------|------|
| 功能 | 以平面原点为圆心，指定半径创建圆 |
| 输入 | `Plane plane` — 所在平面（原点为圆心），`double radius` — 半径 |
| 输出 | `Circle` — 圆对象 |
| 报错 | radius ≤ 0 时返回 `Circle.Unset` |
| RhinoCommon | `new Circle(plane, radius)` |

**重载 4：直径两端点**

对应 Rhino 命令：`Circle` > Diameter

| 项目 | 说明 |
|------|------|
| 功能 | 以直径两端点创建圆 |
| 输入 | `Plane plane` — 所在平面，`Point3d point1` — 直径端点1，`Point3d point2` — 直径端点2 |
| 输出 | `Circle` — 圆对象 |
| 报错 | 两点重合时返回 `Circle.Unset` |
| RhinoCommon | 无直接构造，手动计算：center=中点，radius=半距离，再 `new Circle(plane, center, radius)` |

**重载 5：三点**

对应 Rhino 命令：`Circle` > 3Point

| 项目 | 说明 |
|------|------|
| 功能 | 以圆周上三点创建圆 |
| 输入 | `Point3d p1`、`Point3d p2`、`Point3d p3` — 圆周上三点 |
| 输出 | `Circle` — 圆对象 |
| 报错 | 三点共线时返回 `Circle.Unset` |
| RhinoCommon | `new Circle(p1, p2, p3)` |

**重载 6：起点 + 切向 + 终点**

对应 Rhino 命令：无直接对应（RhinoCommon 独有能力）

| 项目 | 说明 |
|------|------|
| 功能 | 以起点、起点切向量、终点创建圆（圆的起止点都在起点） |
| 输入 | `Point3d startPoint` — 起点，`Vector3d tangentAtStart` — 起点切向量，`Point3d endPoint` — 终点 |
| 输出 | `Circle` — 圆对象 |
| 报错 | 切向量为零或两点重合时返回 `Circle.Unset` |
| RhinoCommon | `new Circle(startPoint, tangentAtStart, endPoint)` |

**重载 7：与两条曲线相切**

对应 Rhino 命令：`Circle` > Tangent

| 项目 | 说明 |
|------|------|
| 功能 | 以与两条曲线相切和半径创建圆 |
| 输入 | `Curve curve1` — 第一条曲线，`Curve curve2` — 第二条曲线，`double radius` — 半径，`double tolerance` — 公差 |
| 输出 | `Circle[]` — 满足条件的圆数组（可能有多个解） |
| 报错 | 无相切解时返回空数组 |
| RhinoCommon | `Curve.CreateFillet(curve1, curve2, radius, t0, t1)` 返回 Arc，由 Arc 构造 Circle |

---

## 圆弧

### CreateArc

对应 Rhino 命令：`Arc`

创建圆弧。通过参数类型区分不同的创建方式，共 4 个重载。

**重载 1：圆心 + 半径 + 角度**

对应 Rhino 命令：`Arc` > Center, Start, Angle

| 项目 | 说明 |
|------|------|
| 功能 | 以圆心、半径、角度范围创建圆弧 |
| 输入 | `Plane plane` — 所在平面，`Point3d center` — 圆心，`double radius` — 半径，`double startAngle` — 起始角（弧度）[可选，默认 0.0]，`double endAngle` — 终止角（弧度）[可选，默认 π/2] |
| 输出 | `Arc` — 圆弧对象 |
| 报错 | radius ≤ 0 或角度范围无效时返回 `Arc.Unset` |
| RhinoCommon | `new Arc(plane, center, radius, sweepAngle)` |

**重载 2：三点**

对应 Rhino 命令：`Arc` > Start, Point, End

| 项目 | 说明 |
|------|------|
| 功能 | 以起点、弧上点、终点创建圆弧 |
| 输入 | `Point3d start` — 起点，`Point3d pointOnArc` — 弧上点，`Point3d end` — 终点 |
| 输出 | `Arc` — 圆弧对象 |
| 报错 | 三点共线时返回 `Arc.Unset` |
| RhinoCommon | `new Arc(start, pointOnArc, end)` |

**重载 3：起点 + 终点 + 起点方向**

对应 Rhino 命令：`Arc` > Start, End, Direction

| 项目 | 说明 |
|------|------|
| 功能 | 以起点、终点、起点方向创建圆弧 |
| 输入 | `Point3d start` — 起点，`Point3d end` — 终点，`Vector3d directionAtStart` — 起点方向 |
| 输出 | `Arc` — 圆弧对象 |
| 报错 | 方向为零向量或两点重合时返回 `Arc.Unset` |
| RhinoCommon | `new Arc(start, directionAtStart, end)` |

**重载 4：与两条曲线相切**

对应 Rhino 命令：`Arc` > Tangent

| 项目 | 说明 |
|------|------|
| 功能 | 以与两条曲线相切和半径创建圆弧 |
| 输入 | `Curve curve1` — 第一条曲线，`Curve curve2` — 第二条曲线，`double radius` — 半径，`double tolerance` — 公差 |
| 输出 | `Arc[]` — 满足条件的圆弧数组 |
| 报错 | 无相切解时返回空数组 |
| RhinoCommon | `Curve.CreateFillet(curve1, curve2, radius, t0, t1)` |

---

## 圆锥曲线

### CreateEllipse

对应 Rhino 命令：`Ellipse`

创建椭圆。通过参数类型区分不同的创建方式，共 3 个重载。

**重载 1：中心 + 两半轴**

对应 Rhino 命令：`Ellipse` > Center

| 项目 | 说明 |
|------|------|
| 功能 | 以中心和两半轴创建椭圆 |
| 输入 | `Plane plane` — 所在平面，`Point3d center` — 中心，`double radius1` — 第一半轴，`double radius2` — 第二半轴 |
| 输出 | `NurbsCurve` — 椭圆曲线 |
| 报错 | radius1/radius2 ≤ 0 时返回 null |
| RhinoCommon | `new Ellipse(plane, radius1, radius2).ToNurbsCurve()`（需先用 center 构造新平面） |

**重载 2：直径两端点 + 第二轴**

对应 Rhino 命令：`Ellipse` > Diameter

| 项目 | 说明 |
|------|------|
| 功能 | 以直径两端点和第二半轴长度创建椭圆 |
| 输入 | `Plane plane` — 所在平面，`Point3d point1` — 直径端点1，`Point3d point2` — 直径端点2，`double secondRadius` — 第二半轴长度 |
| 输出 | `NurbsCurve` — 椭圆曲线 |
| 报错 | 两点重合或 secondRadius ≤ 0 时返回 null |
| RhinoCommon | 无直接构造，手动构造平面再 `new Ellipse(plane, r1, r2).ToNurbsCurve()` |

**重载 3：两焦点 + 椭圆上一点**

对应 Rhino 命令：`Ellipse` > FromFoci

| 项目 | 说明 |
|------|------|
| 功能 | 以两焦点和椭圆上一点创建椭圆 |
| 输入 | `Point3d focus1` — 焦点1，`Point3d focus2` — 焦点2，`Point3d pointOnEllipse` — 椭圆上一点 |
| 输出 | `NurbsCurve` — 椭圆曲线 |
| 报错 | 焦点重合或点无效时返回 null |
| RhinoCommon | 无直接构造，手动计算半轴 a/b 再 `new Ellipse(plane, a, b).ToNurbsCurve()` |

### CreateParabola

对应 Rhino 命令：`Parabola`

创建抛物线。

> 注：`NurbsCurve.CreateParabolaFromFocus(focus, start, end)` 与 `CreateParabolaFromPoints(start, inner, end)` 签名同为 `(Point3d, Point3d, Point3d)`，无法重载区分。本层封装三点版本（`CreateParabolaFromPoints`），如需焦点方式请直接调用 RhinoCommon API。

| 项目 | 说明 |
|------|------|
| 功能 | 以三点（均在抛物线上）创建抛物线 |
| 输入 | `Point3d start` — 起点，`Point3d pointOn` — 抛物线上一点，`Point3d end` — 终点 |
| 输出 | `NurbsCurve` — 抛物线曲线 |
| 报错 | 三点共线时返回 null |
| RhinoCommon | `NurbsCurve.CreateParabolaFromPoints(start, innerPoint, end)` |

### CreateHyperbola

对应 Rhino 命令：`Hyperbola` > Focus, Vertex

| 项目 | 说明 |
|------|------|
| 功能 | 以焦点、顶点、终点创建双曲线 |
| 输入 | `Point3d focus` — 焦点，`Point3d vertex` — 顶点，`Point3d endPoint` — 终点 |
| 输出 | `NurbsCurve` — 双曲线曲线 |
| 报错 | 焦点与顶点重合时返回 null |
| RhinoCommon | 无直接 API，调用 Geometry 层 `ConicGeo.CreateHyperbola`（rho=0.7 的有理二次 NURBS） |

### CreateConic

对应 Rhino 命令：`Conic`

| 项目 | 说明 |
|------|------|
| 功能 | 以起点、终点、顶点、rho 值创建圆锥截面曲线 |
| 输入 | `Point3d start` — 起点，`Point3d end` — 终点，`Point3d apex` — 顶点（切线交点），`double rho` — rho 值 [可选，默认 0.5]（0 < rho < 1，椭圆；rho = 0.5，抛物线；rho > 0.5，双曲线） |
| 输出 | `NurbsCurve` — 圆锥截面曲线 |
| 报错 | rho ≤ 0 或 ≥ 1，或三点共线时返回 null |
| RhinoCommon | 无直接 API，调用 Geometry 层 `ConicGeo.CreateConic`（degree=2 有理 NURBS，w=rho/(1-rho)） |

---

## 自由曲线

### CreateNurbsCurve

对应 Rhino 命令：`Curve`

创建 NURBS 曲线。通过参数类型区分不同的创建方式，共 2 个重载。

**重载 1：控制点方式**

对应 Rhino 命令：`Curve`

| 项目 | 说明 |
|------|------|
| 功能 | 由控制点创建 NURBS 曲线 |
| 输入 | `IEnumerable<Point3d> points` — 控制点（≥2 个），`int degree` — 阶数（1-11）[可选，默认 3]，`bool periodic` — 是否周期性 [可选，默认 false] |
| 输出 | `NurbsCurve` — NURBS 曲线 |
| 报错 | 控制点数 ≤ degree 或 degree < 1 时返回 null |
| RhinoCommon | `NurbsCurve.Create(periodic, degree, points)` |

**重载 2：高级方式（含节点向量和权重）**

对应 Rhino 命令：`Curve`（手动模式）

| 项目 | 说明 |
|------|------|
| 功能 | 由控制点、节点向量、权重创建完整 NURBS 曲线 |
| 输入 | `IEnumerable<Point3d> points` — 控制点，`IEnumerable<double> knots` — 节点向量（数量 = 控制点数 + 阶数 - 1），`int degree` — 阶数 [可选，默认 3]，`IEnumerable<double> weights` — 权重 [可选，默认全 1.0]（数量 = 控制点数，值 > 0） |
| 输出 | `NurbsCurve` — NURBS 曲线 |
| 报错 | 节点数/权数不匹配、degree 无效、权重 ≤ 0 时返回 null |
| RhinoCommon | `NurbsCurve.Create(false, degree, points)` + 手动设置 Knots/Points.SetPoint |

### CreateInterpCrv

对应 Rhino 命令：`InterpCrv`

| 项目 | 说明 |
|------|------|
| 功能 | 通过给定点的插值曲线（曲线经过每个点） |
| 输入 | `IEnumerable<Point3d> points` — 经过点（≥2），`int degree` — 阶数（≥1，必须为奇数）[可选，默认 3]，`CurveKnotStyle knots` — 节点样式 [可选，默认 Uniform]，`Vector3d startTangent` — 起点切向量 [可选]，`Vector3d endTangent` — 终点切向量 [可选] |
| 输出 | `NurbsCurve` — 插值曲线 |
| 报错 | 点数 < 2、degree 非奇数、切向量与周期冲突时返回 null |

### CreateHandleCurve

对应 Rhino 命令：`HandleCurve`

| 项目 | 说明 |
|------|------|
| 功能 | 以编辑柄方式创建链式三次贝塞尔曲线 |
| 输入 | `IEnumerable<Tuple<Point3d, Point3d>> handlePoints` — 每段（锚点 + 控制柄末端），`bool closed` — 是否闭合 [可选，默认 false] |
| 输出 | `NurbsCurve` — 贝塞尔复合曲线 |
| 报错 | 输入 < 2 段时返回 null |
| RhinoCommon | 无直接 API，用 `PolyCurve` 拼接 `BezierCurve` 段（C1 连续对称控制柄） |

### CreateCurveThroughPt

对应 Rhino 命令：`CurveThroughPt`

| 项目 | 说明 |
|------|------|
| 功能 | 拟合通过点对象/点云的曲线 |
| 输入 | `IEnumerable<Point3d> points` — 点集合，`int degree` — 阶数（≥1）[可选，默认 3]，`bool periodic` — 是否周期 [可选，默认 false]，`double tolerance` — 拟合公差 [可选，动态值]，`Vector3d startTangent` — 起点切向量 [可选]，`Vector3d endTangent` — 终点切向量 [可选] |
| 输出 | `NurbsCurve` — 拟合曲线 |
| 报错 | 点数 < degree+1 或 tolerance ≤ 0 时返回 null |

### CreateCatenary

对应 Rhino 命令：`Catenary`

| 项目 | 说明 |
|------|------|
| 功能 | 创建悬链线曲线（两端固定，自然下垂的链/缆形状） |
| 输入 | `Point3d start` — 起点，`Point3d end` — 终点，`double length` — 链条长度（≥ 两点距离），`Vector3d gravity` — 重力方向（单位向量，指向"下"） |
| 输出 | `NurbsCurve` — 悬链线曲线 |
| 报错 | length < 两点距离或 gravity 为零向量时返回 null |
| RhinoCommon | 无直接构造，调用 Geometry 层 `CatenaryGeo.Create`（牛顿迭代 + 沿重力方向采样） |

---

## 螺旋线

### CreateHelix

对应 Rhino 命令：`Helix`

创建螺旋线。通过参数类型区分不同的创建方式，共 2 个重载。

**重载 1：沿轴线**

| 项目 | 说明 |
|------|------|
| 功能 | 创建螺旋线（3D 螺旋，沿轴向上升） |
| 输入 | `Line axis` — 旋转轴，`double startRadius` — 起始半径 [可选，默认 1.0]，`double endRadius` — 终止半径 [可选，默认 1.0]，`double turns` — 圈数，`double pitch` — 每圈高度（0 为平坦螺旋）[可选，默认 1.0] |
| 输出 | `NurbsCurve` — 螺旋线曲线 |
| 报错 | turns ≤ 0 或半径 ≤ 0 时返回 null |
| RhinoCommon | `NurbsCurve.CreateSpiral(axisStart, axisDir, radiusPoint, pitch, turns, r0, r1)` |

**重载 2：沿任意曲线**

对应 Rhino 命令：`Helix` > AroundCurve

| 项目 | 说明 |
|------|------|
| 功能 | 沿任意曲线创建螺旋线 |
| 输入 | `Curve rail` — 轨道曲线，`double startRadius` — 起始半径，`double endRadius` — 终止半径，`double turns` — 圈数 |
| 输出 | `NurbsCurve` — 沿曲线的螺旋线 |
| 报错 | rail 无效或参数无效时返回 null |
| RhinoCommon | `NurbsCurve.CreateSpiral(rail, t0, t1, radiusPoint, pitch, turns, r0, r1, pointsPerTurn)` |

### CreateSpiral

对应 Rhino 命令：`Spiral`

| 项目 | 说明 |
|------|------|
| 功能 | 创建平面螺旋线（扁平螺旋） |
| 输入 | `Plane plane` — 所在平面，`Point3d center` — 中心，`double startRadius` — 起始半径 [可选，默认 1.0]，`double endRadius` — 终止半径 [可选，默认 5.0]，`double turns` — 圈数 |
| 输出 | `NurbsCurve` — 平面螺旋线 |
| 报错 | turns ≤ 0 或半径 ≤ 0 时返回 null |
| RhinoCommon | `NurbsCurve.CreateSpiral(center, normal, radiusPoint, pitch=0, turns, r0, r1)` |

---

## 从对象提取

### CreateDividePoints

对应 Rhino 命令：`Divide`

沿曲线等分生成点。通过参数类型区分不同的等分方式，共 2 个重载。

**重载 1：等分段数**

对应 Rhino 命令：`Divide`

| 项目 | 说明 |
|------|------|
| 功能 | 按段数沿曲线等分生成点 |
| 输入 | `Curve curve` — 目标曲线，`int segmentCount` — 等分段数 [可选，默认 2] |
| 输出 | `List<Point3d>` — 等分点列表（含两端点） |
| 报错 | segmentCount < 1 或曲线无效时返回空列表 |
| RhinoCommon | `curve.DivideByCount(segmentCount, true, out points)` |

**重载 2：按长度**

对应 Rhino 命令：`Divide` > Length

| 项目 | 说明 |
|------|------|
| 功能 | 按指定长度沿曲线生成点 |
| 输入 | `Curve curve` — 目标曲线，`double segmentLength` — 每段长度 |
| 输出 | `List<Point3d>` — 分割点列表 |
| 报错 | segmentLength ≤ 0 或曲线无效时返回空列表 |
| RhinoCommon | `curve.DivideByLength(segmentLength, true, out points)` |

### GetCurveStart

对应 Rhino 命令：`CrvStart`

| 项目 | 说明 |
|------|------|
| 功能 | 获取曲线起点 |
| 输入 | `Curve curve` — 目标曲线 |
| 输出 | `Point3d` — 起点 |
| 报错 | 曲线无效时返回 `Point3d.Unset` |

### GetCurveEnd

对应 Rhino 命令：`CrvEnd`

| 项目 | 说明 |
|------|------|
| 功能 | 获取曲线终点 |
| 输入 | `Curve curve` — 目标曲线 |
| 输出 | `Point3d` — 终点 |
| 报错 | 曲线无效时返回 `Point3d.Unset` |

---

## 从对象派生曲线

### CreateProjectCrv

对应 Rhino 命令：`Project`

| 项目 | 说明 |
|------|------|
| 功能 | 将曲线沿指定方向投影到 Brep 上 |
| 输入 | `IEnumerable<Curve> curves` — 要投影的曲线，`Brep target` — 投影目标曲面，`Vector3d direction` — 投影方向，`bool isPreview = false` |
| 输出 | `Curve[]` — 投影后的曲线数组 |
| 报错 | curves 或 target 为 null 时返回空数组 |
| RhinoCommon | `Curve.ProjectToBrep(curve, brep, direction, tolerance)` |

### CreatePullCrv

| 项目 | 内容 |
|------|------|
| 功能 | 将曲线拉到曲面上（最近点方式） |
| 对应命令 | Pull |
| 输入 | `IEnumerable<Curve> curves` — 要拉的曲线，`Brep target` — 目标曲面，`double tolerance` — 公差 [可选，动态值] |
| 输出 | `Curve[]` — 拉到曲面上的曲线数组 |
| 报错 | curves 为空或 target 无效时返回空数组 |
| RhinoCommon | `Curve.PullToBrepFace(face, tolerance)` 返回 `Curve[]` |

### CreateApplyCrv

| 项目 | 内容 |
|------|------|
| 功能 | 将曲线包裹映射到曲面上（保持 UV 关系） |
| 对应命令 | ApplyCrv |
| 输入 | `IEnumerable<Curve> curves` — 要包裹的曲线，`Brep target` — 目标曲面 |
| 输出 | `Curve[]` — 包裹后的曲线数组 |
| 报错 | curves 为空或 target 无效时返回空数组 |
| RhinoCommon | `BrepFace.Pullback(3D curve, tol)` → 2D 曲线，再 `BrepFace.Pushup(2D curve, tol)` → 3D 曲面曲线 |

### CreateDupEdge

对应 Rhino 命令：`DupEdge`

| 项目 | 说明 |
|------|------|
| 功能 | 复制曲面/多重曲面的边缘为独立曲线 |
| 输入 | `Brep brep` — 源曲面，`IEnumerable<BrepEdge> edges` — 指定边缘 [可选，不传则复制全部裸露边缘]，`bool isPreview = false` |
| 输出 | `Curve[]` — 边缘曲线数组 |
| 报错 | brep 为 null 时返回空数组 |
| RhinoCommon | `BrepEdge.DuplicateCurve()` |

### CreateExtractIsocurve

| 项目 | 内容 |
|------|------|
| 功能 | 从曲面提取指定方向的等参线 |
| 对应命令 | ExtractIsocurve |
| 输入 | `Brep surface` — 源曲面，`Point3d point` — 提取位置，`int direction` — 方向（0=U，1=V）[可选，默认 0] |
| 输出 | `Curve` — 等参线曲线 |
| 报错 | SurfaceInvalidException — 曲面无效；PointNotOnSrfException — 点不在曲面上 |

### CreateContour

对应 Rhino 命令：`Contour`

| 项目 | 说明 |
|------|------|
| 功能 | 在 Brep 或 Mesh 上按起始点和方向生成等高线 |
| 输入 | `GeometryBase geometry` — 目标对象（Brep 或 Mesh），`Point3d startPt` — 等高线起始点，`Point3d endPt` — 方向终点（endPt - startPt 定义等高方向），`double interval` — 间距，`bool isPreview = false` |
| 输出 | `Curve[]` — 等高线数组 |
| 报错 | geometry 为 null 或 interval ≤ 0 时返回空数组 |
| RhinoCommon | `Brep.CreateContourCurves(brep, startPt, endPt, interval)` / `Mesh.CreateContourCurves(mesh, startPt, endPt, interval)` |

### CreateSection

| 项目 | 内容 |
|------|------|
| 功能 | 用平面切割对象生成截面线 |
| 对应命令 | Section |
| 输入 | `GeometryBase geometry` — 目标对象，`Plane cutPlane` — 切割平面 |
| 输出 | `Curve[]` — 截面线数组 |
| 报错 | GeometryInvalidException — 对象无效；NoIntersectionException — 无交线 |

---

## 备注

以下 Rhino 曲线命令归入其他功能区：

| Rhino 命令 | 归属 | 原因 |
|------------|------|------|
| `InterpCrvOnSrf` | Surface | 在曲面上创建插值曲线 |
| `Sketch` > OnSurface | Surface | 在曲面上手绘曲线 |
| `ShortPath` | Surface | 在曲面上创建测地线 |
| `ExtractPt` | Edit | 提取控制点/编辑点/网格顶点 |
