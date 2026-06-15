# Arc - 圆弧

## 命名空间
`Rhino.Geometry`

## 摘要
表示圆的一部分（圆弧）。

## 构造函数

| 构造函数 | 说明 |
|---------|------|
| `Arc()` | 默认构造 |
| `Arc(Circle, double, double)` | 圆 + 起始/结束角度(弧度) |
| `Arc(Point3d, Point3d, Point3d)` | 三点定弧 |
| `Arc(Point3d, double, double, double)` | 圆心、半径、起止角度 |

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| StartPoint | Point3d | 起点 |
| EndPoint | Point3d | 终点 |
| Center | Point3d | 圆心 |
| Radius | double | 半径 |
| StartAngle | double | 起始角度(弧度) |
| EndAngle | double | 结束角度(弧度) |
| SweepAngle | double | 扫过角度 |

## 示例

```csharp
// 三点创建圆弧
Arc arc1 = new Arc(startPt, midPt, endPt);

// 圆心+半径+角度创建
Arc arc2 = Rh.Creation.MakeArc(center, 10.0, 0, 90);
Arc arc3 = new Arc(center, 10.0, 
    new Interval(0, Math.PI / 2));

// 添加到文档
Guid id = doc.Objects.AddArc(arc1);
```
