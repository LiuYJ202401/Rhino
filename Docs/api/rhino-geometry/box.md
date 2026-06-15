# Box - 轴对齐长方体

## 命名空间
`Rhino.Geometry`

## 摘要
表示轴对齐的包围盒或长方体。

## 构造函数

| 构造函数 | 说明 |
|---------|------|
| `Box()` | 无效盒子 |
| `Box(Plane, Interval, Interval, Interval)` | 平面 + X/Y/Z 范围 |
| `Box(BoundingBox)` | 从包围盒 |

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| Plane | Plane | 定位平面 |
| X | Interval | X 范围 |
| Y | Interval | Y 范围 |
| Z | Interval | Z 范围 |
| Center | Point3d | 中心点 |
| IsValid | bool | 是否有效 |

## 方法

| 名称 | 返回值 | 说明 |
|------|--------|------|
| ToBrep | Brep | 转为 Brep |
| Transform | Transform | void | 变换 |
| GetCorners | Point3d[] | 获取 8 个角点 |

## 示例

```csharp
// 创建盒子
Interval x = new Interval(-5, 5);
Interval y = new Interval(-5, 5);
Interval z = new Interval(0, 10);
Box box = new Box(Plane.WorldXY, x, y, z);

// 转为 Brep 添加到文档
Brep brep = box.ToBrep();
Guid id = doc.Objects.AddBrep(brep);

// 使用 Rh.Creation
Box box2 = Rh.Creation.MakeBox(Plane.WorldXY, x, y, z);
```
