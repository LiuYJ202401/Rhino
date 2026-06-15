# Curve - 曲线基类

## 命名空间
`Rhino.Geometry`

## 摘要
所有曲线类型（线、圆弧、NURBS 等）的抽象基类。

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| IsClosed | bool | 是否闭合 |
| IsPeriodic | bool | 是否周期性 |
| Dimension | int | 维度 (通常为 3) |
| Domain | Interval | 参数域 |
| PointCount | int | 控制点/多边形点数 |
| PointCountStart | int | 起始点索引 |
| PointCountEnd | int | 结束点索引 |

## 方法

| 名称 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| PointAt | double t | Point3d | 参数 t 处的点 |
| TangentAt | double t | Vector3d | 参数 t 处的切线 |
| Length | - | double | 曲线长度 |
| Length | double, double | double | 指定区间长度 |
| ClosestPoint | Point3d | double | 最近点参数 |
| Offset | Plane, double, double | Curve[] | 偏移曲线 |
| Extend | double | Curve | 延长曲线 |
| ToNurbsCurve | - | NurbsCurve | 转 NURBS |

## 派生类

- `Line` - 直线
- `Arc` - 圆弧
- `Circle` - 圆
- `Polyline` - 多段线
- `NurbsCurve` - NURBS 曲线
- `PolyCurve` - 多重曲线

## 示例

```csharp
// 从对象获取曲线
ObjRef objRef = ...;
Curve curve = objRef.Geometry() as Curve;
if (curve != null)
{
    double len = curve.Length;
    Point3d start = curve.PointAtStart;
    Point3d end = curve.PointAtEnd;
}
```
