# Circle - 圆

## 命名空间
`Rhino.Geometry`

## 摘要
表示三维空间中的圆（由平面和半径定义）。

## 构造函数

| 构造函数 | 说明 |
|---------|------|
| `Circle()` | 默认构造 (无效圆) |
| `Circle(Point3d, double)` | 圆心和半径 |
| `Circle(Plane, double)` | 平面和半径 |
| `Circle(Plane, Point3d, double)` | 圆周上一点定义半径 |

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| Center | Point3d | 圆心 |
| Radius | double | 半径 |
| Plane | Plane | 所在平面 |
| IsValid | bool | 是否有效 |
| Circumference | double | 周长 |
| Diameter | double | 直径 |

## 方法

| 名称 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| PointAt | double angle | Point3d | 圆周上角度处的点 |
| TangentAt | double angle | Vector3d | 圆周切线 |
| ToNurbsCurve | - | NurbsCurve | 转为 NURBS 曲线 |

## 示例

```csharp
// 创建圆
Circle c1 = new Circle(Point3d.Origin, 10.0);
Circle c2 = new Circle(Plane.WorldXY, 5.0);
Circle c3 = Rh.Creation.MakeCircle(center, radius);

// 添加到文档
Guid id = doc.Objects.AddCircle(c1);

// 转为曲线
NurveCurve nc = c1.ToNurbsCurve();
```
