# Plane - 平面

## 命名空间
`Rhino.Geometry`

## 摘要
表示三维空间中的平面（由原点和三个轴向量定义）。

## 构造函数

| 构造函数 | 说明 |
|---------|------|
| `Plane()` | 默认 (世界 XY) |
| `Plane(Point3d, Vector3d)` | 原点 + 法向量 |
| `Plane(Point3d, Vector3d, Vector3d)` | 原点 + X轴 + Y轴 |

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| Origin | Point3d | 原点 |
| XAxis | Vector3d | X 轴方向 |
| YAxis | Vector3d | Y 轴方向 |
| ZAxis (Normal) | Vector3d | Z 轴/法向量 |

## 预定义平面

| 名称 | 说明 |
|------|------|
| `Plane.WorldXY` | 世界 XY 平面 (Z=0) |
| `Plane.WorldXZ` | 世界 XZ 平面 (Y=0) |
| `Plane.WorldYZ` | 世界 YZ 平面 (X=0) |

## 方法

| 名称 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| ClosestParameter | Point3d | double, double | 点的最近参数 |
| ClosestPoint | Point3d | Point3d | 点在平面上的投影 |
| DistanceTo | Point3d | double | 点到平面距离 |
| Flip | - | void | 翻转法向量 |

## 示例

```csharp
// 创建平面
Plane p1 = new Plane(new Point3d(0, 0, 10), Vector3d.ZAxis);
Plane p2 = Rh.Creation.MakePlane(origin, normal);
Plane p3 = Plane.WorldXY;

// 投影点到平面
Point3d projected = p2.ClosestPoint(somePoint);

// 距离
double dist = p2.DistanceTo(somePoint);
```
