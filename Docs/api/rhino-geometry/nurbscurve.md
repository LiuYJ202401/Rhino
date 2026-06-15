# NurbsCurve - NURBS 曲线

## 命名空间
`Rhino.Geometry`

## 摘要
表示非均匀有理 B 样条 (NURBS) 曲线。

## 创建

```csharp
// 从点创建 (插值)
Point3d[] points = { ... };
NurbsCurve nc = NurbsCurve.Create(true, 3, points);

// 从控制点创建
Point3d[] controlPoints = { ... };
NurbsCurve nc = NurbsCurve.Create(3, true, controlPoints);

// 从圆/圆弧创建
Circle circle = new Circle(...);
NurbsCurve nc = circle.ToNurbsCurve();
```

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| Degree | int | 阶数 |
| Points | NurbsCurvePointList | 控制点 |
| Knots | NurbsCurveKnotList | 节点向量 |
| IsClosed | bool | 是否闭合 |
| IsPeriodic | bool | 是否周期性 |
| Domain | Interval | 参数域 |

## 方法

| 名称 | 返回值 | 说明 |
|------|--------|------|
| `PointAt(double t)` | Point3d | 参数 t 处的点 |
| `TangentAt(double t)` | Vector3d | 参数 t 处的切线 |
| `ClosestPoint(Point3d)` | double, bool | 最近点参数 |
| `Length()` | double | 曲线长度 |
| `Offset(double, Vector3d, double)` | Curve[] | 偏移曲线 |
| `Extend(double)` | bool | 延长曲线 |
| `Split(double)` | Curve[] | 分割曲线 |

## 示例

```csharp
// 创建通过点的 NURBS 曲线
Point3d[] pts = {
    new Point3d(0, 0, 0),
    new Point3d(10, 10, 0),
    new Point3d(20, 5, 0),
    new Point3d(30, 15, 0)
};
NurbsCurve curve = NurbsCurve.Create(true, 3, pts);

// 添加到文档
Guid id = doc.Objects.AddCurve(curve);
```
