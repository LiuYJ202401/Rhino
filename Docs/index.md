# Rhino 8 C# 插件开发文档

本目录包含从 rhino-docs 整理的 RhinoCommon API 文档和开发指南，专为人和 AI 阅读优化。

## 目录结构

```
Docs/
├── index.md              # 本文件 - 文档导航索引
├── api/                  # API 参考文档
│   ├── index.md         # API 索引
│   ├── rhino-geometry/   # 几何类型 (15个)
│   ├── rhino-commands/   # 命令系统 (1个)
│   ├── rhino-input/      # 输入获取 (6个)
│   ├── rhino-docobjects/ # 文档对象 (3个)
│   ├── rhino-display/    # 显示管道 (1个)
│   └── rhino-fileio/     # 文件读写
├── guides/              # 开发指南
│   ├── index.md         # 指南索引
│   ├── plugin-basics/   # 插件基础 (2个)
│   ├── rhinocommon/     # RhinoCommon 核心 (5个)
│   ├── display/         # 显示管道 (2个)
│   ├── input/           # 用户输入 (1个)
│   ├── fileio/          # 文件读写 (1个)
│   └── ui/              # 用户界面 (1个)
└── samples/             # 示例代码
```

## 快速开始

### 核心命名空间

| 命名空间 | 作用 | 文档 |
|---------|------|------|
| Rhino.Geometry | 点线面体、Brep、Mesh | api/rhino-geometry/ |
| Rhino.DocObjects | 文档对象、属性 | api/rhino-docobjects/ |
| Rhino.Input | 取点、取对象 | api/rhino-input/ |
| Rhino.Commands | 命令系统 | api/rhino-commands/ |
| Rhino.Display | 显示与 conduit | api/rhino-display/ |
| Rhino | 文档、应用 | api/rhino-docobjects/ |

## API 文档索引

### Rhino.Geometry (几何)

| 类型 | 说明 |
|------|------|
| [Point3d](api/rhino-geometry/point3d.md) | 三维点 |
| [Vector3d](api/rhino-geometry/vector3d.md) | 三维向量 |
| [Transform](api/rhino-geometry/transform.md) | 变换矩阵 |
| [Line](api/rhino-geometry/line.md) | 直线 |
| [Circle](api/rhino-geometry/circle.md) | 圆 |
| [Arc](api/rhino-geometry/arc.md) | 圆弧 |
| [Plane](api/rhino-geometry/plane.md) | 平面 |
| [Curve](api/rhino-geometry/curve.md) | 曲线基类 |
| [NurbsCurve](api/rhino-geometry/nurbscurve.md) | NURBS 曲线 |
| [PolyCurve](api/rhino-geometry/polycurve.md) | 多重曲线 |
| [Surface](api/rhino-geometry/surface.md) | 曲面基类 |
| [Brep](api/rhino-geometry/brep.md) | 边界表示 |
| [Mesh](api/rhino-geometry/mesh.md) | 网格 |
| [Box](api/rhino-geometry/box.md) | 长方体/包围盒 |
| [Extrusion](api/rhino-geometry/extrusion.md) | 挤出体 |

### Rhino.Input (输入)

| 类型 | 说明 |
|------|------|
| [GetPoint](api/rhino-input/getpoint.md) | 点输入 |
| [GetObject](api/rhino-input/getobject.md) | 对象选择 |
| [GetNumber](api/rhino-input/getnumber.md) | 数字输入 |
| [GetInteger](api/rhino-input/getinteger.md) | 整数输入 |
| [GetString](api/rhino-input/getstring.md) | 字符串输入 |
| [GetOption](api/rhino-input/getoption.md) | 选项输入 |

### Rhino.DocObjects (文档对象)

| 类型 | 说明 |
|------|------|
| [RhinoDoc](api/rhino-docobjects/rhinodoc.md) | Rhino 文档 |
| [ObjRef](api/rhino-docobjects/objref.md) | 对象引用 |
| [ObjectAttributes](api/rhino-docobjects/objectattributes.md) | 对象属性 |

## 指南索引

### RhinoCommon 核心

- [什么是 RhinoCommon?](guides/rhinocommon/what-is-rhinocommon.md)
- [插件生命周期](guides/plugin-basics/plugin-lifecycle.md)
- [添加命令到项目](guides/plugin-basics/adding-commands.md)
- [运行 Rhino 命令](guides/rhinocommon/run-rhino-command.md)
- [事件监听器](guides/rhinocommon/event-watchers.md)
- [对象选择与选项](guides/rhinocommon/object-selection-options.md)
- [插件用户数据](guides/rhinocommon/plugin-user-data.md)

### 用户输入

- [用户输入指南](guides/input/user-input.md)

### 显示与预览

- [Display Conduits](guides/display/display-conduits.md)
- [动态绘制](guides/display/dynamic-draw.md)

### 文件读写

- [代码驱动文件 IO](guides/fileio/code-driven-file-io.md)

### 用户界面

- [选项卡面板](guides/ui/tabbed-panels.md)

## 原始资源

- API 文档: ../../rhino-docs/rhinocommon/api-static/html/index.html
- 命令索引: ../../rhino-docs/COMMAND_INDEX.md
