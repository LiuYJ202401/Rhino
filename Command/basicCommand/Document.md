# Document 命令接口文档

命名空间：`Rh.Cmd.Document`

## 功能

创建和修改文档结构对象（图层、图块、群组），返回对象供 Project 层应用到文档。

## 设计说明

本类命令**不直接写入 RhinoDoc**，而是创建/修改对象后返回。Project 层负责将返回的对象应用到文档：

```
Command.CreateLayer(name, color) → 返回 Layer 对象
    ↓
Project 层调用 doc.Layers.Add(layer) 写入文档
```

## 默认值机制

- 标注 `[可选]` 的参数可不传入，Command 层自动从 Data 层读取默认值
- 命令执行后，实际使用的值自动更新为新的默认值
- 对应默认值定义见 `Data/Command/basicCommand/Document.md`

---

## 一、图层（Layer）

### CreateLayer

| 项目 | 内容 |
|------|------|
| 功能 | 创建图层对象 |
| 对应命令 | Layer > New |
| 输入 | `string name` — 图层名，`System.Drawing.Color color` — 颜色 [可选，默认黑色]，`bool visible` — 可见性 [可选，默认 true]，`bool locked` — 锁定 [可选，默认 false]，`ObjectLinetype linetype` — 线型 [可选，默认 Continuous]，`int lineWidth` — 线宽 [可选，默认 0（默认）]，`string parentLayer` — 父图层全路径 [可选，默认无（根图层）] |
| 输出 | `Layer` — 图层对象 |
| 报错 | LayerNameEmptyException — 图层名为空 |

### ModifyLayer

| 项目 | 内容 |
|------|------|
| 功能 | 修改图层属性 |
| 对应命令 | Layer > Edit |
| 输入 | `Layer layer` — 要修改的图层，`System.Drawing.Color? color` — 新颜色 [可选]，`bool? visible` — 新可见性 [可选]，`bool? locked` — 新锁定状态 [可选]，`ObjectLinetype? linetype` — 新线型 [可选]，`int? lineWidth` — 新线宽 [可选] |
| 输出 | `Layer` — 修改后的图层对象 |
| 报错 | LayerInvalidException — 图层无效 |

### DeleteLayer

| 项目 | 内容 |
|------|------|
| 功能 | 检查图层是否可删除（无子图层、无对象） |
| 对应命令 | Layer > Delete |
| 输入 | `Layer layer` — 要删除的图层，`IEnumerable<RhinoObject> objects` — 文档中的对象列表（用于检查图层是否被使用） |
| 输出 | `bool` — 是否可删除（true=可删除，false=有子图层或被使用） |
| 报错 | LayerInvalidException — 图层无效 |

---

## 二、图块（Block）

### CreateBlockDefinition

| 项目 | 内容 |
|------|------|
| 功能 | 由几何对象创建图块定义 |
| 对应命令 | Block |
| 输入 | `string name` — 图块名，`IEnumerable<GeometryBase> geometry` — 图块包含的几何对象，`Plane basePlane` — 图块基点平面（插入原点和方向） |
| 输出 | `InstanceDefinitionGeometry` — 图块定义对象 |
| 报错 | NameEmptyException — 图块名为空；GeometryEmptyException — 几何对象为空 |

### CreateBlockInstance

| 项目 | 内容 |
|------|------|
| 功能 | 创建图块实例（插入图块到指定位置） |
| 对应命令 | Insert |
| 输入 | `InstanceDefinitionGeometry definition` — 图块定义，`Transform insertTransform` — 插入变换（位置、旋转、缩放） |
| 输出 | `InstanceReferenceGeometry` — 图块实例对象 |
| 报错 | DefinitionInvalidException — 图块定义无效 |

### ExplodeBlockInstance

| 项目 | 内容 |
|------|------|
| 功能 | 炸开图块实例为组成几何对象 |
| 对应命令 | Explode（图块模式） |
| 输入 | `InstanceReferenceGeometry instance` — 图块实例 |
| 输出 | `GeometryBase[]` — 炸开后的几何对象数组 |
| 报错 | InstanceInvalidException — 实例无效 |

---

## 三、群组（Group）

### CreateGroup

| 项目 | 内容 |
|------|------|
| 功能 | 将对象组合为群组 |
| 对应命令 | Group |
| 输入 | `IEnumerable<Guid> objectIds` — 要群组的对象 GUID 列表，`string groupName` — 群组名 [可选，默认自动命名] |
| 输出 | `Group` — 群组对象 |
| 报错 | ObjectsEmptyException — 对象列表为空 |

### AddToGroup

| 项目 | 内容 |
|------|------|
| 功能 | 将对象添加到已有群组 |
| 对应命令 | — |
| 输入 | `Group group` — 目标群组，`IEnumerable<Guid> objectIds` — 要添加的对象 GUID 列表 |
| 输出 | `Group` — 修改后的群组对象 |
| 报错 | GroupInvalidException — 群组无效 |

### RemoveFromGroup

| 项目 | 内容 |
|------|------|
| 功能 | 从群组中移除对象 |
| 对应命令 | — |
| 输入 | `Group group` — 目标群组，`IEnumerable<Guid> objectIds` — 要移除的对象 GUID 列表 |
| 输出 | `Group` — 修改后的群组对象 |
| 报错 | GroupInvalidException — 群组无效 |

### Ungroup

| 项目 | 内容 |
|------|------|
| 功能 | 解散群组 |
| 对应命令 | Ungroup |
| 输入 | `Group group` — 要解散的群组 |
| 输出 | `bool` — 是否成功 |
| 报错 | GroupInvalidException — 群组无效 |

---

## 命令汇总

| 分类 | 命令数 | 命令列表 |
|------|--------|---------|
| 图层 | 3 | CreateLayer, ModifyLayer, DeleteLayer |
| 图块 | 3 | CreateBlockDefinition, CreateBlockInstance, ExplodeBlockInstance |
| 群组 | 4 | CreateGroup, AddToGroup, RemoveFromGroup, Ungroup |
| **合计** | **10** | |
