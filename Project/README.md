# Project 层（项目命令）

命名空间：`Rh.Project`

## 目的

包装基础命令，将结果写入 Rhino 文档。每个子文件夹是一个独立项目，包含该项目所有实际使用的命令。

## 结构

<!-- 每个项目一个子文件夹，内含 Overview.md（项目说明+命令一览）和命令代码 -->

（待填充）

## 规则

- 是唯一写入 Rhino 文档的层（调用 `doc.Objects.AddXxx`）
- 命令继承 UI 层模板，获取合法输入
- 调用 Command 层基础命令整合结果
- 每个项目有 `Overview.md`（项目目标、命令一览表、使用方法）

## 三层协作流程

交互式命令的完整生命周期分为三个阶段：

### ① 命令启动（初始化 UI）

```
Project 调用 Command.GetDefault() 获取默认值
→ Project 用默认值初始化 UI 的 InputContext（选项面板显示默认值）
→ 首次预览自动触发
```

### ② 交互预览（用户修改数值）

```
UI 持有当前值 → 触发预览回调 → Project 接收
→ Project 调用 Command(isPreview: true)（读 Data + Geometry 创建 + 不更新 Data）
→ Project 调用 UI.Preview 渲染几何 + 标注
```

### ③ 最终执行（用户确定）

```
UI 返回最终值 → Project
→ Project 调用 Command(isPreview: false)（Geometry 创建 + 更新 Data）
→ Project 写入 Rhino 文档（doc.Objects.Add）
```

详细数据流见开发计划 §3.5。
