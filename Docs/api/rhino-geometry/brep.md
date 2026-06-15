# Brep - 边界表示

## 命名空间
`Rhino.Geometry`

## 摘要
表示边界表示 (B-Rep) 几何，用于曲面和实体。

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| IsValid | bool | 是否有效 |
| IsSolid | bool | 是否实体 (封闭) |
| IsSurface | bool | 是否单一曲面 |
| Faces | BrepFaceList | 面列表 |
| Edges | BrepEdgeList | 边列表 |
| Vertices | BrepVertexList | 顶点列表 |

## 方法

| 名称 | 参数 | 返回值 | 说明 |
|------|------|--------|------|
| CreateSurface | - | Surface[] | 转为曲面 |
| CapPlanarHoles | double | Brep | 封闭平面洞 |
| Join | IEnumerable<Brep> | Brep[] | 合并 Brep |
| BooleanDifference | Brep | Brep[] | 布尔差 |
| BooleanUnion | Brep | Brep[] | 布尔并 |

## 创建方法

```csharp
// 从 Box 创建
Box box = ...;
Brep brep = box.ToBrep();

// 从挤出
Brep brep = Surface.CreateExtrusion(profile, direction);

// 从旋转
Brep brep = Brep.CreateRevSurface(...);
```

## 示例

```csharp
// 添加到文档
Guid id = doc.Objects.AddBrep(brep);

// 检查是否实体
if (brep.IsSolid)
{
    RhinoApp.WriteLine("这是封闭实体");
}
```
