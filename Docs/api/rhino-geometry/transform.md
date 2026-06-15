# Transform - 变换矩阵

## 命名空间
`Rhino.Geometry`

## 摘要
4x4 变换矩阵，用于平移、旋转、缩放等几何变换。

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| Identity | static Transform | 单位矩阵 (无变换) |
| ZeroTransformation | static Transform | 零变换 |

## 静态创建方法

| 名称 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| Identity | - | Transform | 单位矩阵 |
| Translation | Vector3d | Transform | 平移变换 |
| Translation | double, double, double | Transform | 平移变换 (dx, dy, dz) |
| Rotation | double, Vector3d, Point3d | Transform | 旋转变换 (角度, 轴, 中心) |
| RotationX, Y, Z | double, Point3d | Transform | 绕 X/Y/Z 轴旋转 |
| Scale | double, Point3d | Transform | 均匀缩放 |
| Scale | Vector3d, Point3d | Transform | 非均匀缩放 |
| Mirror | Plane | Transform | 镜像变换 |

## 方法

| 名称 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| IsIdentity | - | bool | 是否为单位矩阵 |
| TryGetInverse | out Transform | bool | 获取逆矩阵 |

## 运算符

| 运算符 | 说明 |
|--------|------|
| * | 矩阵相乘 (组合变换) |

## 示例

```csharp
// 平移
Vector3d move = new Vector3d(10, 20, 30);
Transform t1 = Transform.Translation(move);

// 旋转 45 度绕 Z 轴
Transform t2 = Transform.Rotation(Math.PI / 4, Vector3d.ZAxis, Point3d.Origin);

// 缩放 2 倍
Transform t3 = Transform.Scale(2.0, Point3d.Origin);

// 组合变换: 先旋转再平移
Transform combined = t1 * t2;

// 应用于对象
doc.Objects.Transform(objId, combined);

// 应用于点
Point3d original = new Point3d(10, 0, 0);
Point3d transformed = original;
transformed.Transform(combined);
```

## 变换顺序

变换矩阵的应用顺序很重要：
- 矩阵乘法 `A * B` 表示先应用 B，再应用 A
- 通常按 "平移 * 旋转 * 缩放" 的顺序构建
