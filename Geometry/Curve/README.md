# Geometry/Curve 说明文档

命名空间：`Rh.Geo.Crv`

## 目的

曲线相关的几何工具方法，提供 Command 层复用的底层几何算法。不直接与 RhinoDoc 交互。

**核心原则**：所有方法接收 `Plane` 参数（或由输入点自动确定平面），不假设平面方向。

## 文件结构

| 文件 | 职责 |
|------|------|
| `CircleGeo.cs` | 圆创建（中心半径、直径、三点、切向） |
| `ArcGeo.cs` | 圆弧创建（中心角度、三点、起点终点方向） |
| `EllipseGeo.cs` | 椭圆创建（中心半径、直径、焦点） |
| `RectangleGeo.cs` | 矩形创建（对角点、中心宽高） |
| `PolygonGeo.cs` | 多边形顶点生成（正多边形、星形、边长→中心） |
| `ConicGeo.cs` | 圆锥曲线手工 NURBS 构造（rho 参数化、双曲线） |
| `FilletGeo.cs` | 相切圆角（相交线 CreateFillet + 平行线半圆分支） |
| `CatenaryGeo.cs` | 悬链线牛顿迭代求解（接收重力方向） |
| `HelixGeo.cs` | 螺旋线垂直参考点计算 |

---

## CircleGeo

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|------------|
| `CreateFromCenterRadius` | Plane, Point3d, double | Circle | `new Circle(plane, center, radius)` |
| `CreateFromDiameter` | Plane, Point3d, Point3d | Circle | 计算中点/半距离后构造 |
| `CreateFrom3Points` | Point3d ×3 | Circle | `new Circle(p1, p2, p3)`（平面自动确定） |
| `CreateFromTangent` | Point3d, Vector3d, Point3d | Circle | `new Circle(start, tangent, end)`（平面自动确定） |

## ArcGeo

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|------------|
| `CreateFromCenterAngle` | Plane, Point3d, double, double, double | Arc | 旋转平面后 `new Arc(plane, center, radius, sweep)` |
| `CreateFrom3Points` | Point3d ×3 | Arc | `new Arc(start, mid, end)`（平面自动确定） |
| `CreateFromStartEndDir` | Point3d, Point3d, Vector3d | Arc | `new Arc(start, direction, end)`（平面自动确定） |

## EllipseGeo

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|------------|
| `CreateFromCenterRadii` | Plane, Point3d, double, double | NurbsCurve | 重建平面后 `new Ellipse(plane, r1, r2)` |
| `CreateFromDiameter` | Plane, Point3d, Point3d, double | NurbsCurve | 从直径方向重建平面后构造 |
| `CreateFromFoci` | Point3d ×3 | NurbsCurve | 计算半轴 + 叉积法线后构造（平面自动确定） |

## RectangleGeo

| 方法 | 输入 | 输出 | 说明 |
|------|------|------|------|
| `CreateFromCorners` | Plane, Point3d, Point3d | Polyline | UV 投影计算角点 |
| `CreateFromCenter` | Plane, Point3d, double, double | Polyline | UV 投影 + 中心偏移 |

## PolygonGeo

| 方法 | 输入 | 输出 | 说明 |
|------|------|------|------|
| `CreateRegularPolygon` | Plane, Point3d, int, double, double | Polyline | 正多边形顶点（外接圆方式） |
| `CreateStarPolygon` | Plane, Point3d, int, double, double | Polyline | 星形多边形（内外半径交替） |
| `EdgeLengthToRadius` | double, int | double | 边长 → 外接圆半径 |
| `EdgeToCenter` | Plane, Point3d, Point3d, int | out center, radius, startAngle | 边长方式计算中心/角度 |

## ConicGeo

| 方法 | 输入 | 输出 | 说明 |
|------|------|------|------|
| `CreateConic` | Point3d ×3, double rho | NurbsCurve | rho 参数化圆锥截面（w=rho/(1-rho)） |
| `CreateHyperbola` | Point3d ×3 | NurbsCurve | 双曲线（rho=0.7 的圆锥曲线） |

> **NURBS 参数依赖约束**：圆锥曲线用二次有理 Bezier 表示（order=3, degree=2, pointCount=3）。
> Knots.Count 由公式 `pointCount + degree - 1 = 4` 严格推导，不可独立设置。
> 实现中只维护 2 个独立参数（order、pointCount），degree/knotCount/knots 值全部动态推导，避免硬编码导致越界。

## FilletGeo

| 方法 | 输入 | 输出 | 说明 |
|------|------|------|------|
| `CreateFilletArcs` | Curve×2, double radius, double tol | Arc[] | 相切圆角弧（相交线走 CreateFillet，平行线走半圆分支） |
| `CreateFilletCircles` | Curve×2, double radius, double tol | Circle[] | 相切圆角圆（基于 CreateFilletArcs 构造完整圆） |

> **平行线半圆分支**：`Curve.CreateFillet` 不支持平行线退化情况（圆心不唯一）。
> 平行线相切圆的半径被几何约束严格锁定为 `r = d/2`（d 为两线间距），用半圆连接两线。
> 传入的 `radius` 参数在平行线场景下被忽略，实际使用 `d/2`。
> 仅处理 LineCurve（最常见且可精确分析），其他曲线类型若 CreateFillet 失败则返回空。

## CatenaryGeo

| 方法 | 输入 | 输出 | 说明 |
|------|------|------|------|
| `Create` | Point3d, Point3d, double, Vector3d | NurbsCurve | 悬链线（牛顿迭代 + 沿重力方向采样） |

## HelixGeo

| 方法 | 输入 | 输出 | 说明 |
|------|------|------|------|
| `GetRadiusPoint` | Point3d, Vector3d, double | Point3d | 计算垂直于轴方向的参考点 |

---

## 依赖关系

```
Command 层 (CurveCmd)
    ├── CircleGeo.CreateFromCenterRadius / CreateFromDiameter / ...
    ├── ArcGeo.CreateFromCenterAngle / ...
    ├── EllipseGeo.CreateFromCenterRadii / ...
    ├── RectangleGeo.CreateFromCorners / ...
    ├── PolygonGeo.CreateRegularPolygon / EdgeToCenter / ...
    ├── ConicGeo.CreateConic / CreateHyperbola
    ├── FilletGeo.CreateFilletArcs / CreateFilletCircles
    ├── CatenaryGeo.Create
    ├── HelixGeo.GetRadiusPoint
    └── PlaneGeo.PointToUV（位于 Geometry 根目录）
```

`PlaneGeo` 位于 `Geometry/PlaneGeo.cs`（不在 Curve 子文件夹），因为它被多个功能区复用（Curve、Surface、Solid 等）。
