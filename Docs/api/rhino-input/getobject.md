# GetObject - 获取对象

## 命名空间
`Rhino.Input.Custom`

## 摘要
获取用户在视图中选择的对象。

## 常用方法

| 方法 | 说明 |
|------|------|
| `SetCommandPrompt(string)` | 设置命令提示 |
| `GeometryFilter` | 设置对象类型过滤器 |
| `Get()` | 获取输入 |
| `Object(int)` | 获取选中的对象引用 |

## 对象类型过滤器

```csharp
// 单一类型
getObject.GeometryFilter = ObjectType.Curve;
getObject.GeometryFilter = ObjectType.Surface;
getObject.GeometryFilter = ObjectType.Brep;
getObject.GeometryFilter = ObjectType.Point;

// 组合类型
getObject.GeometryFilter = ObjectType.Curve | ObjectType.Surface;
```

## 基本用法

```csharp
var getObject = new GetObject();
getObject.SetCommandPrompt("选择曲线");
getObject.GeometryFilter = ObjectType.Curve;

if (getObject.Get() == GetResult.Object)
{
    ObjRef objRef = getObject.Object(0);
    Curve curve = objRef.Geometry() as Curve;
    
    if (curve != null)
    {
        // 处理曲线...
    }
}
```

## 多选

```csharp
var getObject = new GetObject();
getObject.SetCommandPrompt("选择对象");
getObject.GetMultiple(1, 0); // 至少 1 个，无上限

if (getObject.CommandResult() == Result.Success)
{
    for (int i = 0; i < getObject.ObjectCount; i++)
    {
        ObjRef objRef = getObject.Object(i);
        // 处理每个对象...
    }
}
```

## 子对象选择

```csharp
var getObject = new GetObject();
getObject.GeometryFilter = ObjectType.PolysrfFilter;
getObject.SubObjectSelect = true; // 允许选择子对象
getObject.Get();

ObjRef objRef = getObject.Object(0);
if (objRef.SubObject() != null)
{
    // 选择了面或边
}
```
