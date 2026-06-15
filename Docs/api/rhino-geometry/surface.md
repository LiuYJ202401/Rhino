# Surface - 曲面基类

## 命名空间
`Rhino.Geometry`

## 摘要
所有曲面类型的抽象基类。

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| Domain(0) | Interval | U 方向参数域 |
| Domain(1) | Interval | V 方向参数域 |
| IsClosed(0/1) | bool | U/V 方向是否闭合 |
| IsPeriodic(0/1) | bool | U/V 方向是否周期性 |

## 方法

| 名称 | 返回值 | 说明 |
|------|--------|------|
| `PointAt(u, v)` | Point3d | 参数 (u,v) 处的点 |
| `NormalAt(u, v)` | Vector3d | 参数 (u,v) 处的法线 |
| `FrameAt(u, v)` | Plane | 参数 (u,v) 处的坐标系 |
| `ClosestPoint(Point3d)` | double, double | 最近点参数 |
| `ToNurbsSurface()` | NurbsSurface | 转 NURBS 曲面 |

## 派生类

- `NurbsSurface` - NURBS 曲面
- `PlaneSurface` - 平面
- `RevSurface` - 旋转曲面
- `SumSurface` - 直纹曲面
- `BrepFace` - Brep 中的面

## 常见操作

```csharp
// 创建平面
PlaneSurface ps = PlaneSurface.CreateThroughPlane(Plane.WorldXY);

// 从曲线创建曲面 (放样)
Brep[] breps = Brep.CreateLoft(curves, Point3d.Unset);

// 旋转曲面
Brep brep = Brep.CreateRevSurface(curve, axis, startAngle, endAngle);
```
