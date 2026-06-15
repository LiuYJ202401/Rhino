# ObjRef - 对象引用

## 命名空间
`Rhino.DocObjects`

## 摘要
对 Rhino 文档中对象的引用，包含对象 ID 和可能的子对象信息。

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| ObjectId | Guid | 对象 ID |
| ObjectType | ObjectType | 对象类型 |

## 方法

| 名称 | 返回值 | 说明 |
|------|--------|------|
| `Geometry()` | Geometry | 获取几何体 |
| `Curve()` | Curve | 获取曲线 (如果是) |
| `Brep()` | Brep | 获取 Brep (如果是) |
| `Surface()` | Surface | 获取曲面 (如果是) |
| `Point()` | Point3d | 获取点 (如果是点对象) |
| `SubObject()` | ObjRef | 获取子对象引用 |

## 示例

```csharp
var getObject = new GetObject();
getObject.GeometryFilter = ObjectType.Curve;
if (getObject.Get() == GetResult.Object)
{
    ObjRef objRef = getObject.Object(0);
    
    // 获取几何
    Geometry geom = objRef.Geometry();
    Curve curve = geom as Curve;
    
    if (curve != null)
    {
        // 使用曲线...
    }
    
    // 或直接获取
    Curve curve2 = objRef.Curve();
}
```
