# RhinoDoc - Rhino 文档

## 命名空间
`Rhino`

## 摘要
表示 Rhino 文档（模型），包含所有对象、图层、材质等。

## 获取文档

```csharp
// 当前活动文档
RhinoDoc doc = RhinoDoc.ActiveDoc;

// 所有打开的文档
RhinoDoc[] docs = RhinoDoc.ActiveRhinoDocList;
```

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| Objects | RhinoObjectTable | 对象集合 |
| Layers | LayerTable | 图层集合 |
| Views | ViewTable | 视图集合 |
| Materials | MaterialTable | 材质集合 |
| Groups | GroupTable | 组集合 |

## 文档属性

| 名称 | 类型 | 说明 |
|------|------|------|
| ModelAbsoluteTolerance | double | 绝对公差 |
| ModelAngleToleranceDegrees | double | 角度公差 |
| ModelRelativeTolerance | double | 相对公差 |
| PageUnitSystem | UnitSystem | 页面单位 |

## 常用方法

```csharp
// 添加对象
Guid id = doc.Objects.AddPoint(point);
Guid id = doc.Objects.AddLine(line);
Guid id = doc.Objects.AddCurve(curve);
Guid id = doc.Objects.AddBrep(brep);
Guid id = doc.Objects.AddMesh(mesh);

// 查找对象
RhinoObject obj = doc.Objects.Find(id);

// 删除对象
doc.Objects.Delete(id, true);

// 修改对象
doc.Objects.ModifyObjectAttributes(id, attrs);

// 重绘视图
doc.Views.Redraw();

// 获取边界
BoundingBox bbox = doc.Objects.GetBoundingBox(true);
```
