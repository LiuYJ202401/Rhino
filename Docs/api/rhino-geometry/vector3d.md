# Vector3d - 三维向量

## 命名空间
`Rhino.Geometry`

## 摘要
表示三维空间中的方向向量或位移，包含 X、Y、Z 分量。

## 构造函数

| 构造函数 | 说明 |
|---------|------|
| `Vector3d()` | 创建零向量 (0, 0, 0) |
| `Vector3d(double x, double y, double z)` | 创建指定分量的向量 |
| `Vector3d(Point3d, Point3d)` | 从两点创建向量 (终点 - 起点) |

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| X | double | X 分量 |
| Y | double | Y 分量 |
| Z | double | Z 分量 |
| Length | double | 向量长度 (只读) |
| IsZero | bool | 是否为零向量 |
| IsUnitVector | bool | 是否为单位向量 |

## 预定义向量

| 名称 | 说明 |
|------|------|
| Vector3d.XAxis | X 轴单位向量 (1, 0, 0) |
| Vector3d.YAxis | Y 轴单位向量 (0, 1, 0) |
| Vector3d.ZAxis | Z 轴单位向量 (0, 0, 1) |
| Vector3d.Unset | 未设置的向量 |

## 方法

| 名称 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| Unitize | - | bool | 转换为单位向量 |
| Reverse | - | Vector3d | 反转向量 |
| Rotate | double, Vector3d | Vector3d | 绕轴旋转向量 |

## 运算符

| 运算符 | 说明 |
|--------|------|
| + | 向量相加 |
| - | 向量相减 |
| * | 向量与标量相乘 |
| / | 向量除以标量 |

## 静态方法

| 名称 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| Add | Vector3d, Vector3d | Vector3d | 向量相加 |
| Subtract | Vector3d, Vector3d | Vector3d | 向量相减 |
| Multiply | Vector3d, double | Vector3d | 标量乘法 |
| Divide | Vector3d, double | Vector3d | 标量除法 |
| DotProduct | Vector3d, Vector3d | double | 点积 |
| CrossProduct | Vector3d, Vector3d | Vector3d | 叉积 |
| AngleBetween | Vector3d, Vector3d | double | 夹角 (弧度) |

## 示例

```csharp
// 创建向量
Vector3d v1 = new Vector3d(10, 0, 0);
Vector3d v2 = new Vector3d(0, 10, 0);

// 点积
double dot = Vector3d.DotProduct(v1, v2);  // = 0

// 叉积
Vector3d cross = Vector3d.CrossProduct(v1, v2);  // = (0, 0, 100)

// 单位化
v1.Unitize();
// 现在 v1.Length == 1.0

// 旋转 90 度
Vector3d rotated = v1.Rotate(Math.PI / 2, Vector3d.ZAxis);
```
