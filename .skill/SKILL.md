---
name: "rhino-plugin-dev"
description: "Guides development within the Rh six-layer Rhino C# plugin framework. Invoke when user wants to create commands, modify code, add features, or perform any development activity in this Rhino project."
---

# Rhino Plugin 开发指南

本框架是一个六层架构的 Rhino C# 插件开发框架，旨在用结构化、可复用的方式开发 Rhino 命令。

> **项目根目录**指 `Rhino/` 文件夹（即 `.csproj` 所在目录），这是 Git 仓库的范围。
> 本 Skill 面向所有使用本框架进行开发的开发者，不限特定项目。

---

## 1. 框架概述

### 六层架构

```
Project (Rh.Project)      项目命令，继承 UICommand，编排完整交互流程
    ↓ 调用
Command (Rh.Cmd)          基础命令，参数校验 + 默认值管理 + 调用 Geometry
    ↓ 调用
Geometry (Rh.Geo)         几何工具，纯几何计算，不假设平面方向
    ↓ 可调用
Math (Rh.Math)            数学工具（待实现）
    ↓ 读写
Data (Rh.Data)            默认值存储，JSON 文件 + DataReader 缓存
```

**UI 层** (`Rh.UI`) 独立于上述链路，提供 UICommand 模板基类、InputBuilder 交互构建器。

### 依赖规则（严格单向）

- Project → Command → {Geometry, Math} → Data
- UI 层被 Project 层继承使用
- **禁止**：Geometry 及以下层访问 `RhinoDoc.ActiveDoc`
- **禁止**：Command 层获取用户输入或写入文档
- **禁止**：跨层反向调用
- **允许**：同层之间互相依赖（如 Geometry/Surface 调用 Geometry/Curve 的方法），当确实有大量复用时

### 命名空间

| 层 | 命名空间 | 示例 |
|----|---------|------|
| Data | `Rh.Data` | `DataReader.GetValue<T>()` |
| Geometry | `Rh.Geo` / `Rh.Geo.Crv` | `CircleGeo.CreateFromCenterRadius()` |
| Command | `Rh.Cmd` | `CurveCmd.CreateCircle()` |
| Project | `Rh.Project.{项目名}` | `CreateCircleCmd : UICommand` |
| UI | `Rh.UI` | `UICommand`, `InputBuilder` |

**Geometry 子命名空间使用缩写**，避免与 RhinoCommon 类型名冲突：

| 子命名空间 | 缩写 | 原因 |
|-----------|------|------|
| Curve | `Rh.Geo.Crv` | 避免与 `Rhino.Geometry.Curve` 冲突 |
| Surface | `Rh.Geo.Srf` | 避免与 `Rhino.Geometry.Surface` 冲突 |
| Solid | `Rh.Geo.Sld` | 避免与 `Rhino.Geometry.Brep` 冲突（预留） |
| Mesh | `Rh.Geo.Msh` | 避免与 `Rhino.Geometry.Mesh` 冲突 |
| Transform | `Rh.Geo.Trs` | 避免与 `Rhino.Geometry.Transform` 冲突 |
| Morph | `Rh.Geo.Mrf` | 避免与 `Rhino.Geometry.Morphs` 冲突 |

---

## 2. 核心设计原则

### 2.1 命令命名

- **同一功能统一方法名**，通过参数类型区分重载
  - 正确：`CreateCircle(Plane, Point3d, double)` 和 `CreateCircle(Point3d, Point3d, Point3d)`
  - 错误：`CreateCircle` 和 `CreateCircleBy3Points`
- **文档标注每个重载对应的 RhinoCommon 构造**，注明有无直接构造函数

### 2.2 平面方向

- **Geometry 层不假设平面方向**，所有创建平面几何的方法必须接收 `Plane` 参数
- **Command 层接收法向量** (`Vector3d normal`) 等输入，在内部构造 `Plane` 后传给 Geometry
- **不存在隐式 WorldXY 假设**，Project 层从 `ConstructionPlane()` 获取工作平面传入
- 三点/切向等方式由输入自动确定平面，无需传入

### 2.3 层级职责

- **Geometry 层负责几何计算**：直径→中心/半径、边长→中心/角度、UV 投影、牛顿迭代等
- **Command 层负责**：参数校验、默认值管理（读写 Data）、调用 Geometry、isPreview 控制
- **Project 层负责**：用户交互（UI 模板）、调用 Command、写入文档

### 2.4 isPreview 模式

```csharp
// Command 层方法签名
public static Circle CreateCircle(Plane plane, Point3d center, double radius, bool isPreview = false)
{
    // 预览时不更新 Data（避免拖动时频繁写文件）
    if (!isPreview)
        UpdateDefault("CreateCircle.radius", radius);

    return CircleGeo.CreateFromCenterRadius(plane, center, radius);
}
```

- `isPreview: true`：读 Data 默认值 + Geometry 创建 + **不更新** Data
- `isPreview: false`：读 Data + Geometry 创建 + **更新** Data

### 2.5 代码风格

- 步骤的三个函数用命名方法实现，不用 lambda
- 模板内部交互循环等杂活用私有方法封装
- 公差等参数从 Data 层传入，不直接访问 `ActiveDoc`

### 2.6 文档优先原则

- **查阅信息时优先从文档读取**，而非直接扫描代码
  - 查方法签名 → 读 `Command/basicCommand/{功能区}.md`
  - 查几何 API → 读 `Geometry/{类型}/README.md`
  - 查默认值 → 读 `Data/Command/.../{功能区}.json`
- **文档是即时更新的**：每次修改代码后，同步更新对应文档
- **代码与文档必须契合**：文档中的方法签名、重载数量、参数类型必须与实际代码一致
- **开发流程中文档先行**：创建新功能时先写文档再写代码；修改接口时先改文档再改代码

---

## 3. 开发流程

根据开发任务类型，采用不同的流程。

### 场景判断

| 场景 | 触发条件 | 文档要求 |
|------|---------|---------|
| **A：创建新命令/功能** | 新增方法、新增命令、新增功能区 | 必须先写文档，再写代码 |
| **B：修改已有代码** | 修复 bug、优化实现、调整参数 | 框架解耦性强，纯代码修改可不更新文档 |
| **C：修改接口** | 改变方法签名、增删参数、改变返回类型 | 必须同步修改所有调用方代码和相关文档 |

---

### 场景 A：创建新命令/功能

#### 步骤 1：理解命令目的

- 对应哪个 Rhino 原生命令？交互流程是什么？
- 需要哪些输入参数？哪些有默认值？
- 创建什么类型的几何对象？

#### 步骤 2：撰写文档（自顶向下）

**Project 层文档**（`Project/{项目名}/Overview.md`）：
- 项目目标说明
- 命令一览表（命令名、功能、对应原生命令）
- 每个命令的交互步骤（用户操作流程）

**Command 层文档**：
- 通用命令：`Command/basicCommand/{功能区}.md`
- 项目专属命令：`Command/{项目名}/{名称}.md`
- 方法签名设计（重载如何划分）
- 每个重载的详细说明表（功能、输入、输出、报错条件）
- 每个重载对应的 RhinoCommon 构造方式
- 标注哪些操作需要 Geometry 层支持

**Geometry 层文档**（`Geometry/{类型}/README.md`）：
- 如有新增几何方法，更新对应 README
- 标注 RhinoCommon 是否有直接 API

**Data 文档**：
- 通用命令：`Data/Command/basicCommand/{功能区}.json`
- 项目专属命令：`Data/Command/{项目名}/{名称}.json`
- 列出所有可选参数及其默认值

#### 步骤 3：识别复用与新增

**读取已有方法清单**（按需）：
- `Geometry/README.md` — 已有几何工具一览
- `Geometry/Curve/README.md` — 曲线几何工具详细方法表
- `Command/basicCommand/Curve.md` — 已有曲线命令方法表
- `Command/basicCommand/Point.md` — 已有点命令方法表

**判断逻辑**：
- 已有 Geometry 方法可直接用 → 直接调用
- 缺少的几何操作 → 在 `Geometry/{类型}/` 新建 `XxxGeo.cs`
- 已有 Command 方法可直接用 → 直接调用
- 缺少的命令 → 在对应功能区 `.cs` 文件中添加方法

#### 步骤 4：实现代码（自底向上）

1. **Geometry 层**（如有新增）
   - 通用工具：`Geometry/{类型}/XxxGeo.cs`
   - 项目专属：`Geometry/{项目名}/XxxGeo.cs`
   - 所有方法接收 `Plane`（平面几何）或由输入自动确定平面
   - 纯几何计算，无 ActiveDoc 依赖

2. **Command 层**
   - 通用命令：`Command/basicCommand/{功能区}.cs` 中添加方法
   - 项目专属：`Command/{项目名}/{名称}.cs` 中添加方法
   - 参数校验 + 默认值管理 + 调用 Geometry
   - `isPreview` 控制 Data 更新

3. **Data 文件**
   - 通用：`Data/Command/basicCommand/{功能区}.json`
   - 项目专属：`Data/Command/{项目名}/{名称}.json`
   - 添加新增的默认值键

4. **Project 层**
   - 在 `Project/{项目名}/{类别}/XxxCmd.cs` 创建命令
   - 继承 `UICommand`，实现交互步骤
   - 调用 Command 层方法，写入文档

#### 步骤 5：验证

- 编译通过（`MSBuild /t:Build`）
- Geometry 层及以下无 `ActiveDoc` 依赖
- 命名空间统一为 `Rh.*`
- 六层数据流贯通（Project → Command → Geometry → Data）

---

### 场景 B：修改已有代码

由于框架解耦性强，纯代码修改（修复 bug、优化算法）**无需更新文档**。

**流程**：
1. 定位需要修改的文件
2. 修改代码
3. 编译验证

**例外**：如果修改了默认值，需同步更新 `Data/Command/.../{功能区}.json`。

---

### 场景 C：修改接口

当方法签名变化时，必须**同步更新所有依赖**。

**流程**：
1. 修改方法签名
2. **全局搜索所有调用方**，逐一修改
3. 更新对应层的文档（Command.md / Geometry README.md）
4. 如有默认值变化，更新 Data JSON
5. 编译验证

---

## 4. 文件结构规则

### 项目目录结构

以下路径均相对于项目根目录 `Rhino/`：

```
Rhino/                                    ← 项目根目录（Git 仓库范围）
├── .skill/
│   └── SKILL.md                          本 Skill 源文件
├── Data/
│   ├── DataReader.cs                     数据读取封装
│   └── Command/
│       ├── basicCommand/                 框架自带基础命令的默认值
│       │   ├── Curve.json
│       │   └── Point.json
│       └── {项目名}/                     项目专属命令的默认值（按需创建）
├── Math/                                 数学工具（待实现）
├── Geometry/
│   ├── PlaneGeo.cs                       平面工具（跨功能区复用）
│   ├── Curve/                            曲线几何工具
│   └── {项目名}/                         项目专属几何（按需创建）
├── UI/
│   ├── UICommand.cs                      命令模板基类
│   ├── InputBuilder.cs                   交互构建器
│   └── StepDef.cs                        步骤定义
├── Command/
│   ├── basicCommand/                     框架自带基础命令（所有项目共享）
│   │   ├── Point.cs
│   │   ├── Curve.cs
│   │   └── ...
│   └── {项目名}/                         项目专属命令（按需创建）
├── Project/
│   └── {项目名}/                         每个项目一个文件夹
│       ├── Overview.md
│       └── {类别}/
│           └── {命令名}Cmd.cs
├── Orders.md                             文件架构 + 设计理念
├── README.md                             框架使用说明
└── RhinoTrial.csproj
```

### basicCommand vs 项目专属

| 类型 | 位置 | 用途 | 示例 |
|------|------|------|------|
| **基础命令** | `Command/basicCommand/` | 框架自带的通用命令，所有项目共享 | Point、Curve、Surface |
| **项目专属命令** | `Command/{项目名}/` | 某个项目特有的命令，不通用 | `Command/my_tool/CustomShape.cs` |

**原则**：
- 通用功能 → 加入 `basicCommand/`（需符合框架整体规划）
- 项目特有功能 → 放入 `Command/{项目名}/`
- Data 层目录结构与 Command 层一一映射

### 何时使用已有框架

- 优先在 `Command/basicCommand/` 中查找是否有对应方法
- 优先在 `Geometry/` 中查找是否有对应几何工具
- 复用 `DataReader` 管理默认值
- 复用 `UICommand` 模板处理交互

### 何时新建文件

| 需求 | 位置 | 命名 |
|------|------|------|
| 新增基础命令（通用） | `Command/basicCommand/{功能区}.cs` | `{功能区}.cs`（如 `Surface.cs`） |
| 新增项目专属命令 | `Command/{项目名}/{名称}.cs` | `{名称}.cs` |
| 新增几何工具 | `Geometry/{类型}/{名称}Geo.cs` | `{名称}Geo.cs`（如 `SphereGeo.cs`） |
| 新增项目专属几何 | `Geometry/{项目名}/{名称}Geo.cs` | `{名称}Geo.cs` |
| 新增项目命令（UICommand） | `Project/{项目名}/{类别}/{名称}Cmd.cs` | `{名称}Cmd.cs` |
| 新增默认值 | `Data/Command/{basicCommand或项目名}/{功能区}.json` | `{功能区}.json` |

---

## 5. 文档格式规范

### Command 层文档格式

```markdown
### CreateXxx

对应 Rhino 命令：`Xxx`

**重载 1：参数方式 A**

| 项目 | 说明 |
|------|------|
| 功能 | 一句话描述 |
| 输入 | `Type param` — 说明 |
| 输出 | `Type` — 说明 |
| 报错 | 条件时返回 null/Unset |
| RhinoCommon | `new Xxx(...)` 或 "无直接构造，调用 Geometry 层 XxxGeo" |
```

### Geometry 层文档格式

```markdown
## XxxGeo

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|------------|
| `CreateFromYyy` | Plane, Point3d, ... | Circle/NurbsCurve | `new Xxx(...)` |
```

### Project 层文档格式

```markdown
# 项目名 Overview

## 命令一览

| 命令名 | 功能 | 对应原生命令 |
|--------|------|------------|
| RhCreateXxx | 创建 XXX | Xxx |

## 交互流程

1. 用户选择/输入...
2. 预览...
3. 确认，写入文档
```

---

## 6. DataReader 使用

### 写入默认值（Command 层）

```csharp
private const string DataPath = "Command/basicCommand/Curve.json";

private static void UpdateDefault<T>(string key, T value)
{
    DataReader.SetValue<T>(DataPath, key, value);
}

// 使用
if (!isPreview)
    UpdateDefault("CreateCircle.radius", radius);
```

### 读取默认值

```csharp
private static T GetDefault<T>(string key, T defaultValue)
{
    return DataReader.GetValue<T>(DataPath, key, defaultValue);
}

// 供 Project 层调用
public static double GetDefaultCircleRadius()
{
    return GetDefault("CreateCircle.radius", 1.0);
}
```

### 读取动态值（公差等）

```csharp
private static double ActiveTolerance()
{
    return DataReader.GetDynamicValue("ModelAbsoluteTolerance");
}
```

---

## 7. InputBuilder 使用

InputBuilder 是 UI 层的交互构建器，在步骤的 Setup 函数中声明输入项。模板根据声明的内容自动选择交互方式（GetPoint / GetPoints / GetObject / OptionsOnly）。

### 主输入（决定交互类型，每步只能声明一种）

| 方法 | 说明 |
|------|------|
| `Point(name, prompt)` | 单点输入 |
| `Points(name, prompt, minCount)` | 多点输入（循环选点，Enter 结束） |
| `Object(name, prompt, filter)` | 单对象输入 |
| `Objects(name, prompt, filter, minCount)` | 多对象输入 |

### 附属选项（可叠加多个）

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `Double(name, prompt, default)` | `double` | 数值选项 |
| `Integer(name, prompt, default)` | `int` | 整数选项 |
| `Toggle(name, prompt, default)` | `bool` | 开关选项 |
| `List(name, prompt, options[], defaultIndex)` | `int`（选中索引） | 列表选项，从多个预设值中选择一个 |

### 约束设置

| 方法 | 说明 |
|------|------|
| `BasePoint(point, showDistance)` | 设置基点，鼠标拖动时显示距离和角度 |

### DrawGeometry 支持的几何类型

Preview() 返回的几何对象通过 DrawGeometry 统一绘制，支持以下类型及其列表：

`Point3d`、`Line`、`Circle`、`Arc`、`Ellipse`、`Curve`、`Brep`、`Surface`、`Sphere`、`Cylinder`、`Cone`、`Box`、`Mesh`、`Polyline`

---

## 8. 快速参考

### 已有方法清单位置

| 层 | 文件 | 内容 |
|----|------|------|
| Geometry | `Geometry/README.md` | 全部几何工具一览 |
| Geometry/Curve | `Geometry/Curve/README.md` | 曲线几何详细方法表 |
| Geometry/Surface | `Geometry/Surface/README.md` | 曲面几何详细方法表 |
| Geometry/Solid | `Geometry/Solid/README.md` | 实体几何详细方法表 |
| Geometry/Mesh | `Geometry/Mesh/README.md` | 网格几何详细方法表 |
| Geometry/Transform | `Geometry/Transform/README.md` | 变换几何详细方法表 |
| Geometry/Morph | `Geometry/Morph/README.md` | 空间变形几何详细方法表（待创建） |
| Command/Point | `Command/basicCommand/Point.md` | 点命令全部方法 |
| Command/Curve | `Command/basicCommand/Curve.md` | 曲线命令全部重载 |
| Command/Surface | `Command/basicCommand/Surface.md` | 曲面命令全部重载 |
| Command/Solid | `Command/basicCommand/Solid.md` | 实体命令全部重载 |
| Command/Mesh | `Command/basicCommand/Mesh.md` | 网格命令全部重载 |
| Command/Transform | `Command/basicCommand/Transform.md` | 变换命令全部重载 |
| Command/Morph | `Command/basicCommand/Morph.md` | 空间变形命令全部重载（待创建） |
| Data | `Data/DataReader.md` | DataReader API 说明 |

### 编译命令

在项目根目录（`.csproj` 所在目录）执行：

```bash
# Windows (MSBuild)
MSBuild RhinoTrial.csproj /t:Build /p:Configuration=Debug /verbosity:minimal

# 或使用 dotnet CLI
dotnet build RhinoTrial.csproj --configuration Debug
```

> 编译工具路径因环境而异，开发者根据本地配置调整。
