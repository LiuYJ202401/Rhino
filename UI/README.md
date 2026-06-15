# UI 层

命名空间：`Rh.UI`

## 目的

1. 提供命令模板（UICommand），封装交互循环、选项管理、预览触发、异常捕获
2. 让项目命令专注于业务逻辑（声明输入、处理几何、写入文档）
3. 规范化输入获取与预览绘制
4. 视图管理（View）：视图方向切换、缩放、命名视图、显示模式等

## 结构

```
Rh.UI/
├── UICommand.cs      命令模板基类（模板方法模式）
├── InputBuilder.cs   输入声明构建器 + InputItem 类层级
├── StepDef.cs        步骤定义 + 委托 + Step 工厂
├── InputResult.cs    步骤输入结果（字典式存取）
└── README.md         本文档
```

## UI 架构

### 模板方法模式

UICommand 继承自 `Rhino.Commands.Command`，将 `RunCommand` 设为 sealed，
内部完成全部交互流程；关键函数留空（abstract/virtual），由开发者覆盖。

```
Rhino.Commands.Command
  └── Rh.UI.UICommand（模板，封装交互杂活）
        └── Rh.Project.xxx.XxxCmd（开发者命令，只写业务）
```

### 模板内部负责的事

| 职责 | 说明 |
|------|------|
| 交互循环 | GetPoint / GetPoints / GetObject 的循环获取，直到用户确定 |
| 选项面板 | 把 InputBuilder 声明的数值/整数/开关/列表自动加到命令行选项 |
| 选项同步 | 用户切换列表选项后自动同步状态，预览即时反映新选项 |
| 基点约束 | 通过 BasePoint 设置基点，鼠标拖动时显示距离和角度 |
| 预览触发 | 绑定 DynamicDraw 事件，鼠标移动时自动调用开发者的 Process + Preview |
| 几何绘制 | DrawGeometry 支持 Point3d/Line/Circle/Arc/Ellipse/Curve/Brep/Surface/Sphere/Cylinder/Cone/Box/Mesh/Polyline 及其列表 |
| 异常捕获 | try-catch 包裹全流程，错误时输出命令名和异常信息 |
| 取消处理 | 用户取消时返回 Result.Cancel，不执行后续步骤 |

### 四种步骤类型（由 Setup 声明的输入决定）

| 类型 | 触发条件 | 交互方式 |
|------|---------|---------|
| GetPoint | 含 Point 输入 | 选一个点，可带选项 |
| GetPoints | 含 Points 输入 | 循环选多个点，Enter 结束 |
| GetObject | 含 Object/Objects 输入 | 选对象，支持预选和框选 |
| OptionsOnly | 仅含数值/开关选项 | 改选项后确定即可 |

## 开发者需要做的事

### 覆盖的函数

| 函数 | 必须 | 说明 |
|------|------|------|
| `EnglishName` | 是 | 命令英文名 |
| `DefineSteps()` | 是 | 返回 StepDef[]，定义步骤顺序 |
| `OnFinish(RhinoDoc doc)` | 是 | 所有步骤完成后写入文档 |
| `OnInit()` | 否 | 命令开始时重置字段（默认空） |
| `PreviewColor` | 否 | 预览颜色（默认 CornflowerBlue） |

### 每个步骤的三个函数（用命名方法，不用 lambda）

| 函数 | 必须 | 说明 |
|------|------|------|
| `Setup(InputBuilder input)` | 是 | 声明本步输入（点/对象/数值/选项） |
| `Process(InputResult result, bool isPreview)` | 是 | 处理输入，更新类字段或生成几何 |
| `Preview()` | 否 | 返回要预览绘制的几何对象数组 |

### InputBuilder 可用方法

**主输入（决定交互类型，每步只能声明一种）：**

| 方法 | 说明 |
|------|------|
| `Point(name, prompt)` | 单点输入 |
| `Points(name, prompt, minCount)` | 多点输入（循环选点，Enter 结束） |
| `Object(name, prompt, filter)` | 单对象输入 |
| `Objects(name, prompt, filter, minCount)` | 多对象输入 |

**附属选项（可叠加多个）：**

| 方法 | 返回值 | 说明 |
|------|--------|------|
| `Double(name, prompt, default)` | `double` | 数值选项 |
| `Integer(name, prompt, default)` | `int` | 整数选项 |
| `Toggle(name, prompt, default)` | `bool` | 开关选项 |
| `List(name, prompt, options[], defaultIndex)` | `int`（选中索引） | 列表选项 |

**约束设置：**

| 方法 | 说明 |
|------|------|
| `BasePoint(point, showDistance)` | 设置基点，鼠标拖动时显示距离和角度 |

### 数据共享方式

- 在命令类中声明 private 字段（如 `_center`、`_radius`）
- `OnInit()` 每次命令开始时重置字段，避免状态残留
- 步骤之间通过类字段共享数据，无需上下文对象
- `Process(result, isPreview)` 中：
  - `isPreview=true`：预览调用，**不要**写入文档，只更新字段/生成临时几何
  - `isPreview=false`：最终执行，可更新字段供后续步骤或 OnFinish 使用

### 代码风格要求

- 步骤的三个函数用**命名方法**实现，不用 lambda
- Setup / Process / Preview 命名建议：`SetupXxx` / `ProcessXxx` / `PreviewXxx`

## 用户操作的流程感受

以「画圆」为例，用户从启动命令到完成的全过程：

```
1. 用户输入命令 RhCreateCircle
   → 命令行提示：「圆心」
   → 鼠标在视口移动，实时看到一个跟随鼠标的预览点

2. 用户点击确定圆心位置
   → 命令行提示：「半径方向」并出现选项面板
     Radius [3.2]    ← 可直接输入或拖动鼠标

3. 用户拖动鼠标 / 输入数值
   → 视口实时显示以圆心为中心、随鼠标距离变化的预览圆
   → 选项面板的数值同步更新（若用鼠标决定半径）

4. 用户点击确定 / 按 Enter
   → 预览消失，真实的圆写入文档
   → 命令结束
```

### 关键体验

| 体验 | 实现方式 |
|------|---------|
| 所见即所得 | DynamicDraw 事件每帧调用 Process(isPreview=true) + Preview() |
| 选项即时生效 | 改选项后继续循环，下次 DynamicDraw 用新值 |
| 多步引导 | DefineSteps 定义顺序，每步完成后自动进入下一步 |
| 随时可取消 | 任意步骤按 Esc / 右键取消，模板返回 Result.Cancel |
| 默认值可用 | OnInit 从 DataReader 读取默认值，首次显示即为合理值 |

## 规则

- 开发者不直接调用 GetPoint/GetObject，由模板根据 InputBuilder 声明自动分发
- 预览几何通过 Preview() 返回，由模板统一绘制，开发者不直接操作 DisplayPipeline
- Process 中禁止写入文档（doc.Objects.Add），写入只在 OnFinish 中完成
- 默认值由 Command 层从 DataReader 读取，开发者在 OnInit 中赋给字段
