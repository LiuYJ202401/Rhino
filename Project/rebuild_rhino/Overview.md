# Rebuild Rhino 项目

命名空间：`Rh.Project.RebuildRhino`

## 项目目标

用 RhinoCommon API 重建 Rhino 原生命令，实现核心的几何创建和编辑功能。每个命令调用基础命令层（Rh.Cmd）的功能块，将结果写入 Rhino 文档。

## 功能区划分

基于 Rhino 8 的命令菜单结构和 RhinoCommon API 覆盖范围，划分为以下 10 个功能区：

### 创建类

| 功能区 | 文件夹 | 说明 | RhinoCommon 可实现性 |
|--------|--------|------|---------------------|
| Point（点） | `Point/` | 创建点、点云 | 完全可实现 |
| Curve（曲线） | `Curve/` | 线段、圆、圆弧、多段线、矩形、多边形、NURBS曲线、螺旋线、椭圆、从对象提取曲线等 | 完全可实现 |
| Surface（曲面） | `Surface/` | 平面、放样、扫掠、旋转、挤出、管道、补面等 | 完全可实现 |
| Solid（实体） | `Solid/` | 长方体、球体、圆柱体、圆锥体、圆环体、布尔运算、抽壳、加厚、加盖等 | 完全可实现 |
| Mesh（网格） | `Mesh/` | 网格基本体创建、NURBS 转网格等 | 完全可实现 |
| SubD（细分曲面） | `SubD/` | SubD 基本体、细分、平滑、折痕、ToSubD/FromSubD、QuadRemesh | 完全可实现 |

### 操作类

| 功能区 | 文件夹 | 说明 | RhinoCommon 可实现性 |
|--------|--------|------|---------------------|
| Transform（变换） | `Transform/` | 移动、复制、旋转、缩放、镜像、阵列、定向等 | 完全可实现 |
| Edit（编辑） | `Edit/` | 修剪、分割、连接、炸开、延伸、倒角、偏移、混合、重建、光顺、修改阶数、曲线布尔等 | 大部分可实现（少数交互式编辑较复杂） |
| Analyze（分析） | `Analyze/` | 距离、角度、长度、面积、体积、曲率分析等 | 完全可实现 |
| Dimension（标注/注释） | `Dimension/` | 尺寸标注、文字、引线、图案填充、点注释等 | 完全可实现 |

## 开发优先级

<!-- 后续讨论确定开发顺序，以下为建议优先级 -->

1. Point — 最基础，依赖最少
2. Curve — 核心创建能力，使用频率最高
3. Surface — 依赖 Curve
4. Solid — 依赖 Surface
5. SubD — Rhino 8 核心功能，依赖 Surface/Mesh
6. Transform — 独立于创建，通用变换
7. Edit — 依赖已有对象
8. Analyze — 独立于创建，只读分析
9. Dimension — 工程出图必备
10. Mesh — 相对独立

## 命令一览表

<!-- 每个功能区的具体命令列表在各自文件夹的 README 中维护 -->

| 功能区 | 命令数 | 状态 |
|--------|--------|------|
| Point | 8 | 接口文档已完成 |
| Curve | 30+ | 接口文档已完成 |
| Surface | 28 | 接口文档已完成 |
| Solid | 待定 | 待撰写 |
| SubD | 待定 | 待撰写 |
| Mesh | 待定 | 待撰写 |
| Transform | 待定 | 待撰写 |
| Edit | 待定 | 待撰写 |
| Analyze | 待定 | 待撰写 |
| Dimension | 待定 | 待撰写 |

## 开发流程

每个功能区的开发流程：
1. 撰写该功能区的接口文档（README.md）
2. 确认底层 Command/Geometry/Math 层是否需要补充
3. 实现 Project 层命令（继承 UI 模板，调用 Command 层，写入文档）
4. 测试验证
