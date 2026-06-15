# Extrusion - 挤出体

## 命名空间
`Rhino.Geometry`

## 摘要
表示挤出几何体，一种特殊的轻量级 Brep。

## 创建

```csharp
// 从曲线挤出
Curve profile = ...;
Vector3d direction = ...;
Extrusion ext = Extrusion.Create(profile, direction, true);

// 挤出到点
Extrusion ext = Extrusion.Create(profile, apexPoint, true);

// 沿路径挤出
Extrusion ext = Extrusion.Create(pathCurve, profileCurve, ...);
```

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| Path | Curve | 挤出路径 |
| ProfileCount | int | 剖面数量 |
| IsCapCount(0/1) | bool | 是否有始/末端盖 |

## 方法

| 名称 | 返回值 | 说明 |
|------|--------|------|
| `ToBrep()` | Brep | 转为 Brep |
| `SetCap(int, bool)` | bool | 设置端盖 |
| `CreateOuterSurface()` | Brep | 创建外表面 |

## 示例

```csharp
// 从圆挤出创建圆柱
Circle circle = new Circle(Plane.WorldXY, 5.0);
Vector3d dir = Vector3d.ZAxis * 10.0;
Extrusion cylinder = Extrusion.Create(circle, dir, true);
Brep brep = cylinder.ToBrep();

Guid id = doc.Objects.AddBrep(brep);
```
