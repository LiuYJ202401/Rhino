# 框架文件架构与设计理念

## 六层架构

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

## 文件夹说明

| 文件夹 | 命名空间 | 职责 |
|--------|---------|------|
| `Data/` | `Rh.Data` | 软编码数据存储，JSON 默认值 + DataReader 缓存，公差等动态值解析 |
| `Math/` | `Rh.Math` | 纯数学运算（点/向量/矩阵），不接触 Rhino 几何对象（待实现） |
| `Geometry/` | `Rh.Geo` | 几何对象创建与操作，按类型分子文件夹（Curve/Surface/Solid/Mesh） |
| `UI/` | `Rh.UI` | UICommand 模板基类、InputBuilder 交互构建器、DrawGeometry 预览渲染 |
| `Command/` | `Rh.Cmd` | 基础命令层，参数校验 + 默认值管理 + 调用 Geometry，不修改文档 |
| `Project/` | `Rh.Project` | 项目命令层，继承 UICommand，编排交互流程并写入 Rhino 文档 |

## 核心设计原则

1. **依赖严格单向**：Project → Command → {Geometry, Math} → Data，禁止反向调用
2. **Geometry 层不假设平面方向**：所有平面几何方法必须接收 `Plane` 参数
3. **Command 层是唯一直接调用 DataReader 的层**
4. **Project 层是唯一写入文档的层**
5. **isPreview 模式**：预览时不更新 Data，避免拖动时频繁写文件
6. **同层允许依赖**：如 Geometry/Solid 调用 Geometry/Curve 的方法
7. **Geometry 子命名空间使用缩写**（Crv/Srf/Sld/Msh/Trs/Mrf），避免与 RhinoCommon 类型名冲突

## 文档格式规范

各层文档遵循统一格式，确保 AI 和开发者能快速定位信息。

### Command 层文档格式（`Command/basicCommand/{功能区}.md`）

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

### Geometry 层文档格式（`Geometry/{类型}/README.md`）

```markdown
## XxxGeo

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|------------|
| `CreateFromYyy` | Plane, Point3d, ... | Circle/NurbsCurve | `new Xxx(...)` |
```

### Project 层文档格式（`Project/{项目名}/Overview.md`）

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

### Data 文档格式（`Data/Command/.../{功能区}.md`）

```markdown
| 命令 | 字段 | 类型 | 默认值 | 说明 |
|------|------|------|--------|------|
| CreateXxx | paramName | double | 1.0 | 参数说明 |
```

## 详细文档

- 框架使用方式：见 `README.md`
- AI 开发指南：见 `.skill/SKILL.md`
- Command 层规则与示例：见 `Command/README.md`
- 测试规则与教训：见 `Project/Test/Overview.md`
