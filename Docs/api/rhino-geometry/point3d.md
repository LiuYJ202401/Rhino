# Point3d - 三维点

## 命名空间
`Rhino.Geometry`

## 摘要
表示三维空间中的一个点，包含 X、Y、Z 坐标。

## 构造函数

| 构造函数 | 说明 |
|---------|------|
| `Point3d()` | 创建原点 (0, 0, 0) |
| `Point3d(double x, double y, double z)` | 创建指定坐标的点 |
| `Point3d(Point3d)` | 从另一个点复制 |

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| X | double | X 坐标 |
| Y | double | Y 坐标 |
| Z | double | Z 坐标 |
| Origin | static Point3d | 原点 (0, 0, 0) |
| Unset | static Point3d | 未设置的点 |

## 方法

| 名称 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| DistanceTo | Point3d | double | 计算到另一个点的距离 |
| Add | Vector3d | Point3d | 加向量 |
| Subtract | Point3d | Vector3d | 减点得到向量 |

## 静态方法

| 名称 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| Add | Point3d, Vector3d | Point3d | 点加向量 |
| Subtract | Point3d, Point3d | Vector3d | 两点相减得向量 |
| Distance | Point3d, Point3d | double | 两点间距离 |

## 示例

```csharp
// 创建点
Point3d pt1 = new Point3d(10, 20, 30);
Point3d pt2 = Point3d.Origin;

// 计算距离
double distance = pt1.DistanceTo(pt2);

// 点加向量
Vector3d vec = new Vector3d(5, 5, 0);
Point3d pt3 = pt1 + vec;
// 或
Point3d pt4 = Point3d.Add(pt1, vec);
```
