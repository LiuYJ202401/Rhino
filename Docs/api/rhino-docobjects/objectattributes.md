# ObjectAttributes - 对象属性

## 命名空间
`Rhino.DocObjects`

## 摘要
控制 Rhino 对象的显示和文档属性。

## 构造

```csharp
ObjectAttributes attrs = new ObjectAttributes();

// 或从对象获取
RhinoObject obj = ...;
ObjectAttributes attrs = obj.Attributes;
```

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| ObjectColor | Color | 对象颜色 |
| ColorSource | ObjectColorSource | 颜色来源 |
| LayerIndex | int | 图层索引 |
| MaterialIndex | int | 材质索引 |
| ObjectName | string | 对象名称 |
| Visible | bool | 是否可见 |
| Locked | bool | 是否锁定 |
| PlotColor | Color | 打印颜色 |
| PlotWeight | double | 打印线宽 |

## ObjectColorSource

| 值 | 说明 |
|-----|------|
| ColorFromLayer | 使用图层颜色 |
| ColorFromObject | 使用对象颜色 |
| ColorFromMaterial | 使用材质颜色 |

## 使用示例

```csharp
// 创建属性
var attrs = new ObjectAttributes();
attrs.ObjectColor = Color.Red;
attrs.ColorSource = ObjectColorSource.ColorFromObject;
attrs.LayerIndex = layerIndex;
attrs.ObjectName = "MyObject";

// 添加对象时指定属性
doc.Objects.AddPoint(point, attrs);

// 修改现有对象属性
doc.Objects.ModifyObjectAttributes(objId, attrs);

// 隐藏对象
attrs.Visible = false;
doc.Objects.ModifyObjectAttributes(objId, attrs);
```
