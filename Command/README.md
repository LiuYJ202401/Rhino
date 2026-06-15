# Command 层（基础命令）

命名空间：`Rh.Cmd`

## 目的

完成一个独立的目的，调用 Geometry 处理或 Math 内容。每个基础命令是一个可复用的功能块。

## 结构

| 子文件夹 | 命名空间 | 内容 |
|----------|---------|------|
| `basicCommand/` | `Rh.Cmd` | 基础命令层，10 个几何功能区 + 文档管理 |

`basicCommand/` 内文件：

| 文件 | 内容 |
|------|------|
| `Point.md` / `Point.cs` | 点、点云创建 |
| `Curve.md` / `Curve.cs` | 线段、圆、圆弧、多段线、矩形、多边形、NURBS、螺旋线、椭圆、从对象派生曲线等 |
| `Surface.md` / `Surface.cs` | 平面、放样、扫掠、旋转、挤出、管道、补面等 |
| `Solid.md` / `Solid.cs` | 长方体、球体、圆柱、圆锥、圆环、圆管、布尔运算、抽壳、加厚、加盖等 |
| `Mesh.md` / `Mesh.cs` | 网格基本体、NURBS 转网格、点云转网格等 |
| `SubD.md` / `SubD.cs` | SubD 基本体、细分、折痕、ToNurbs/FromNurbs、QuadRemesh |
| `Transform.md` / `Transform.cs` | 移动、复制、旋转、缩放、镜像、阵列、定向等 |
| `Edit.md` / `Edit.cs` | 修剪、分割、连接、炸开、延伸、倒角、偏移、混合、重建、光顺、曲面编辑等 |
| `Analyze.md` / `Analyze.cs` | 距离、角度、长度、面积、体积、曲率分析、最近点、点包含等 |
| `Dimension.md` / `Dimension.cs` | 尺寸标注、文字、引线、图案填充、点注释 |
| `Document.md` / `Document.cs` | 图层/图块/群组的创建和修改（返回对象，不直接写入文档） |

## 规则

- **不直接与 RhinoDoc 交互**（不调用 `doc.Objects.AddXxx`、不调用 `GetObject`）
- 可以处理上层传递下来的 Rhino 几何对象（Curve/Brep/Surface/Mesh 等）
- 不获取用户输入（不调用 UI 层交互方法）
- 由 Project 层命令调用并整合结果

## 默认值机制

Command 层是**唯一直接调用 DataReader** 的层，负责默认值的读取和自动更新。

### GetDefault 方法

每个 Command 提供查询默认值的方法，供 Project 层初始化 UI 选项面板：

```csharp
// 查询默认值（供 Project 层初始化 UI）
public static double GetDefaultRadius()
    => DataReader.GetValue<double>("Command/basicCommand/Curve.json", "CreateCircle.radius");
```

### isPreview 参数

所有创建类方法新增 `bool isPreview = false` 参数，区分预览和最终执行：

| isPreview | 读 Data | Geometry 创建 | 更新 Data |
|-----------|---------|-------------|-----------|
| `true`（预览） | 是 | 是 | **否** |
| `false`（执行，默认） | 是 | 是 | **是** |

```csharp
// Command 层方法签名
public static Circle CreateCircle(
    Plane plane, Point3d center,
    double radius, bool isPreview = false)
{
    var circle = Geometry.CreateCircle(plane, center, radius);
    if (!isPreview)
        DataReader.SetValue<double>("...radius", radius);
    return circle;
}
```

### Project 层调用方式

```csharp
// ① 命令启动：获取默认值初始化 UI
double defaultRadius = CurveCommand.GetDefaultRadius();
ui.CreateInputContext(options: new { radius = defaultRadius });

// ② 交互预览：用户修改数值时
var circle = CurveCommand.CreateCircle(plane, center, currentRadius, isPreview: true);

// ③ 最终执行：用户确定后
var finalCircle = CurveCommand.CreateCircle(plane, center, finalRadius);  // isPreview 默认 false
doc.Objects.AddCircle(finalCircle);
```

详细数据流见开发计划 §3.5。
