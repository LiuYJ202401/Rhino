# Line - 直线

## 命名空间
`Rhino.Geometry`

## 摘要
表示由两个点定义的直线段。

## 构造函数

| 构造函数 | 说明 |
|---------|------|
| `Line()` | 默认构造 (从原点到 (1,0,0)) |
| `Line(Point3d, Point3d)` | 从起点到终点 |
| `Line(Point3d, Vector3d)` | 从起点沿方向 |
| `Line(Point3d, Point3d, double)` | 从起点向终点的延长线 |

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| From | Point3d | 起点 |
| To | Point3d | 终点 |
| Direction | Vector3d | 方向向量 (单位向量) |
| Length | double | 长度 |

## 方法

| 名称 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| PointAt | double t | Point3d | 线上参数 t 处的点 (0=起点, 1=终点) |
| DistanceTo | Point3d | double | 点到线的距离 |
| Extend | double, double | void | 延长线两端 |

## 示例

```csharp
// 创建线
Line line1 = new Line(new Point3d(0, 0, 0), new Point3d(10, 10, 10));
Line line2 = Rh.Creation.MakeLine(pt0, pt1);

// 获取线上中点
Point3d mid = line1.PointAt(0.5);

// 添加到文档
Guid id = doc.Objects.AddLine(line1);
```
