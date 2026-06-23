---
name: rhino-plugin-dev
description: |
  Guides development within a six-layer Rhino C# plugin framework (Project/Command/Geometry/Math/Data + UI). 
  Covers layered architecture, naming conventions, development workflow, and documentation requirements. 
  Invoke when: creating Rhino commands, modifying plugin code, adding geometry tools, working with UICommand/InputBuilder, 
  managing default values, writing tests, or any C# development in this Rhino plugin project.
compatibility: "Requires Rhino 8, Visual Studio 2022 with Rhino plugin template, RhinoCommon SDK. .NET C# project."
---

# Rhino Plugin 开发指南

六层架构的 Rhino C# 插件开发框架。项目根目录指 `Rhino/`（`.csproj` 所在目录）。

---

## 1. 六层架构与依赖规则

```
Project (Rh.Project)   → 项目命令，继承 UICommand，编排交互 + 写入文档
Command (Rh.Cmd)       → 基础命令，参数校验 + 默认值管理 + 调用 Geometry
Geometry (Rh.Geo)      → 几何工具，纯计算，不假设平面方向
Math (Rh.Math)         → 数学工具（待实现）
Data (Rh.Data)         → 默认值存储，JSON + DataReader 缓存
UI (Rh.UI)             → UICommand 模板基类 + InputBuilder 交互构建器（独立链路）
```

**依赖严格单向**：Project → Command → {Geometry, Math} → Data

| 禁止 | 说明 |
|------|------|
| Geometry 及以下层访问 `RhinoDoc.ActiveDoc` | 纯计算层不能依赖文档状态 |
| Command 层获取用户输入或写入文档 | 只做校验 + 调用 Geometry + 管理 Data |
| 跨层反向调用 | 如 Geometry 调用 Command |
| Project 以外的层写入文档 | Project 是唯一调用 `doc.Objects.AddXxx` 的层 |

**允许**：同层互相依赖（如 Geometry/Surface 调用 Geometry/Curve）。

### 命名空间

| 层 | 命名空间 |
|----|---------|
| Data | `Rh.Data` |
| Geometry | `Rh.Geo.{子命名空间}`（缩写：Crv/Srf/Sld/Msh/Trs/Mrf/Img） |
| Command | `Rh.Cmd` |
| Project | `Rh.Project.{项目名}` |
| UI | `Rh.UI` |

> Geometry 子命名空间用缩写避免与 RhinoCommon 类型名冲突（如 `Rh.Geo.Crv` 避免 `Rhino.Geometry.Curve`）。

---

## 2. 核心设计原则

### 2.1 命令命名与重载

- **同一功能统一方法名**，通过参数类型区分重载（不写 `CreateCircleBy3Points`，写 `CreateCircle(...)` 重载）
- 文档标注每个重载对应的 RhinoCommon 构造

### 2.2 平面方向

- **Geometry 层不假设平面方向**：所有平面几何方法必须接收 `Plane` 参数
- **Command 层**接收法向量等输入，内部构造 `Plane` 后传给 Geometry
- **Project 层**从 `ConstructionPlane()` 获取工作平面传入

### 2.3 层级职责

| 层 | 职责 |
|----|------|
| Geometry | 几何计算（直径→中心/半径、UV 投影、牛顿迭代等），**保持静默**（只返回结果，不输出消息） |
| Command | 参数校验、错误输出、默认值管理（读写 Data）、调用 Geometry、isPreview 控制 |
| Project | 用户交互（UI 模板）、调用 Command、写入文档 |

### 2.4 Command 层错误输出

**所有错误必须在 Rhino 命令行报告，不能静默返回 null。**

- 格式：`[方法名] 错误：具体原因`
- 参数校验失败 → 输出错误 + return null
- Geometry 返回 null/空 → 输出错误 + return null
- 错误消息用中文，指出哪个参数/条件导致失败

> 完整代码示例见 `Command/README.md` §错误输出规范。

### 2.5 isPreview 模式

- `isPreview: true`：读 Data + Geometry 创建 + **不更新** Data（避免拖动时频繁写文件）
- `isPreview: false`：读 Data + Geometry 创建 + **更新** Data
- 签名：`bool isPreview = false`

### 2.6 代码风格

- **插件命令**：步骤三函数（Setup/Process/Preview）用命名方法，不用 lambda
- **测试程序**：步骤用 `Step(name, action)` 包装，action 内调用 Command + Assert
- 公差等参数从 Data 层传入，不直接访问 `ActiveDoc`

### 2.7 文档优先

- **查阅信息优先读文档**（方法签名→Command.md，几何 API→Geometry README，默认值→Data JSON）
- **文档即时更新**：代码修改后同步更新对应文档
- **文档先行**：创建新功能先写文档再写代码；修改接口先改文档再改代码

### 2.8 接口设计

- **接口自由**：多种独立输入组合 → 提供多个重载（如 Heightfield 可指定物理尺寸或按比例自动适配）
- **增添而非替换**：发现同一功能有两种合理实现 → 作为新重载增添，不替换现有实现
- **冗余输入**：可从其他参数推导的参数（如 degree 从 order 推导）→ 不作为独立参数

> 详细设计步骤与示例见 `Command/README.md` §接口设计原则。

### 2.9 类型创建

- **优先通过 Command/Geometry 层方法创建几何**（如 `CurveCmd.CreateCircle(...)`），不直接 `new Circle(...)`
- **例外**：Geometry 层内部可直接使用 RhinoCommon 构造函数

### 2.10 Geometry 层实体创建规范

> 源于测试链实战教训，详见 `Project/Test/Overview.md`。

| 规则 | 要点 |
|------|------|
| G1 | 优先用 RhinoCommon 原生构造体，不手动拼接面 |
| G2 | 封盖统一用 `CapPlanarHoles`，不手动创建底盖曲线 |
| G3 | 多面体合并用 `Brep.JoinBreps`，不用 `Append` |
| G4 | 创建实体后必须验证 `IsSolid` |
| G5 | `Vector3d` 作为方向+距离时禁止 `Unitize`（丢失高度信息） |
| G6 | `Box`/`Interval` 在 Plane 上下文中是局部坐标，用 `RemapToPlaneSpace()` 转换 |

**实体创建统一模式**：拼侧面 → JoinBreps → CapPlanarHoles → 验证 IsSolid

### 2.11 问题诊断原则

- 编写 Brep 拓扑代码时**先查询实际拓扑再判断**，不凭几何直觉假设
- 遇到行为与预期不符：查询实际值 → 对比预期 → 判断哪边错了 → 只改错的那边

---

## 3. 开发流程

### 场景判断

| 场景 | 触发条件 | 文档要求 |
|------|---------|---------|
| **A：创建新功能** | 新增方法/命令/功能区 | **必须先写文档再写代码** |
| **B：修改实现** | 修复 bug、优化算法 | 纯代码修改可不更新文档 |
| **C：修改接口** | 改签名/增删参数/改返回类型 | **必须同步改所有调用方 + 文档** |

### 场景 A 流程（创建新命令/功能）

1. **撰写文档**（自顶向下）：Project Overview → Command.md → Geometry README → Data JSON
2. **识别复用**：读 `Geometry/README.md` 和 `Command/basicCommand/*.md`，已有方法直接用
3. **实现代码**（自底向上）：
   - Geometry 层（`Geometry/{类型}/XxxGeo.cs`）：纯计算，接收 Plane，无 ActiveDoc
   - Command 层（`Command/basicCommand/{功能区}.cs`）：校验 + Data + 调用 Geometry
   - Data（`Data/Command/.../{功能区}.json`）：默认值
   - Project 层（`Project/{项目名}/{类别}/XxxCmd.cs`）：继承 UICommand，交互 + 写入文档
4. **验证**：编译通过 + 无 ActiveDoc 依赖（Geometry 及以下）+ 命名空间统一 `Rh.*`

---

## 4. 文件结构

```
Rhino/
├── Data/DataReader.cs + Data/Command/{basicCommand|项目名}/*.json
├── Geometry/{PlaneGeo.cs | Curve/ | Surface/ | Solid/ | Mesh/ | Transform/ | Image/ | {项目名}/}
├── Command/{basicCommand/ | {项目名}/}
├── UI/{UICommand.cs | InputBuilder.cs | StepDef.cs}
├── Project/{项目名}/{Overview.md | {类别}/XxxCmd.cs}
└── RhinoTrial.csproj
```

### 文件命名

| 需求 | 位置 | 命名 |
|------|------|------|
| 基础命令（通用） | `Command/basicCommand/{功能区}.cs` | `{功能区}.cs` |
| 项目专属命令 | `Command/{项目名}/{名称}.cs` | `{名称}.cs` |
| 几何工具 | `Geometry/{类型}/{名称}Geo.cs` | `{名称}Geo.cs` |
| UICommand 命令 | `Project/{项目名}/{类别}/{名称}Cmd.cs` | `{名称}Cmd.cs` |
| 默认值 | `Data/Command/{basicCommand或项目名}/{功能区}.json` | `{功能区}.json` |

---

## 5. DataReader API

```csharp
// 路径常量（Command 层内部）
private const string DataPath = "Command/basicCommand/Curve.json";

// 写入（isPreview=false 时调用）
private static void UpdateDefault<T>(string key, T value)
    => DataReader.SetValue<T>(DataPath, key, value);

// 读取（带默认值，key 不存在时不抛异常）
private static T GetDefault<T>(string key, T defaultValue)
    => DataReader.GetValue<T>(DataPath, key, defaultValue);

// 供 Project 层初始化 UI
public static double GetDefaultCircleRadius()
    => GetDefault("CreateCircle.radius", 1.0);

// 动态值（公差等）
DataReader.GetDynamicValue("ModelAbsoluteTolerance");
```

> key 用点号分隔层级，如 `CreateCircle.radius`。

---

## 6. InputBuilder API

在步骤 Setup 函数中声明输入项，模板自动选择交互方式。

### 主输入（每步只能声明一种，决定交互类型）

| 方法 | 说明 |
|------|------|
| `Point(name, prompt)` | 单点（GetPoint） |
| `Points(name, prompt, minCount)` | 多点循环选点 |
| `Object(name, prompt, filter)` | 单对象（GetObject） |
| `Objects(name, prompt, filter, minCount)` | 多对象 |

无主输入时 → OptionsOnly 模式（仅选项，Enter 确认）。

### 附属选项（可叠加多个）

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `Double(name, prompt, default)` | `double` | 数值 |
| `Integer(name, prompt, default)` | `int` | 整数 |
| `Toggle(name, prompt, default)` | `bool` | 开关 |
| `List(name, prompt, options[], defaultIndex)` | `int` | 列表选择 |
| `BasePoint(point, showDistance)` | — | 基点约束 |

### DrawGeometry 支持类型

Preview() 返回的对象通过 DrawGeometry 绘制，支持：`Point3d`、`Line`、`Circle`、`Arc`、`Ellipse`、`Curve`、`Brep`、`Surface`、`Sphere`、`Cylinder`、`Cone`、`Box`、`Mesh`、`Polyline` 及其列表/数组。

---

## 7. 快速参考

### 已有方法清单

| 层 | 文件 |
|----|------|
| Geometry 全部 | `Geometry/README.md` |
| Geometry 各类型 | `Geometry/{Curve|Surface|Solid|Mesh|Transform|Image}/README.md` |
| Command 各功能区 | `Command/basicCommand/{Point|Curve|Surface|Solid|Mesh|Transform|Image}.md` |
| Command 层规则与示例 | `Command/README.md` |
| Data API | `Data/DataReader.md` |
| 文档格式规范 | `Orders.md` §文档格式规范 |
| 测试规则与教训 | `Project/Test/Overview.md` |

### 编译

```bash
MSBuild RhinoTrial.csproj /t:Build /p:Configuration=Debug /verbosity:minimal
```

### 命令发现（新命令不显示时）

Rhino 默认按需加载插件。新增命令后运行任意已知命令（如 `RhCreateCircle`）触发加载即可。开发期间可在 `Plugin.cs` 临时设置 `LoadTime => PlugInLoadTime.AtStartup`，发布时改回。
