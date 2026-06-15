# PolyCurve - 多重曲线

## 命名空间
`Rhino.Geometry`

## 摘要
由多条曲线段连接而成的复合曲线。

## 创建

```csharp
// 从线段数组创建
PolyCurve pc = new PolyCurve();
pc.Append(line1);
pc.Append(arc1);
pc.Append(line2);

// 从曲线数组创建
Curve[] segments = { curve1, curve2, curve3 };
PolyCurve pc = new PolyCurve(segments);
```

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| SegmentCount | int | 曲线段数量 |
| IsContinuous | bool | 是否连续 (G0) |
| IsContinuous(1) | bool | 是否相切连续 (G1) |

## 方法

| 名称 | 返回值 | 说明 |
|------|--------|------|
| `Segment(int index)` | Curve | 获取指定段 |
| `Append(Curve)` | bool | 追加曲线 |
| `RemoveSegment(int)` | bool | 移除指定段 |
| `Explode()` | Curve[] | 分解为各段 |

## 使用场景

```csharp
// 合并共线的曲线
Curve[] inputCurves = { ... };
Curve[] joined = Curve.JoinCurves(inputCurves);

// 创建包含多种类型的路径
PolyCurve path = new PolyCurve();
path.Append(new Line(pt0, pt1));
path.Append(new Arc(pt1, pt2, pt3));
path.Append(new Line(pt3, pt4));
```
