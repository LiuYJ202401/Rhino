# 凤凰中心幕墙项目

命名空间：`Rh.Project.FenghuangCenter`

## 项目目标

将已验证的凤凰中心参数化幕墙建模代码迁移为符合六层架构规范的项目命令。从用户选择的 Brep 曲面出发，自动生成完整的建筑主体构件：对角网格曲线、双向支撑梁、连接节点、玻璃面板、窗框，并按图层写入 Rhino 文档。

## 命令一览表

| 命令 | 文件 | 说明 |
|------|------|------|
| `RhFenghuangCurtainWall` | `CurtainWall/CurtainWallCmd.cs` | 幕墙主体生成命令（选曲面 → 设参数 → 生成构件 → 写入文档） |

## 交互流程

命令 `RhFenghuangCurtainWall` 继承 `UICommand`，包含 **7 个步骤**（按构件类型分组，每步少量参数，逐步确认）：

### 步骤 1 — 选择曲面

- **输入类型**：Object（单选）
- **过滤**：仅接受 Brep 或 Surface
- **提示语**："第 1 步：请选择幕墙基曲面"
- **处理**：保存选中对象的 BrepFace，供后续步骤使用

### 步骤 2 — 网格划分参数

- **输入类型**：OptionsOnly
- **选项**（5 个）：
  - CurveCount（整数，默认 20）— 曲线数
  - DivisionCount（整数，默认 20）— 划分数
  - UseVDirection（Toggle，默认 true）— 使用 V 方向
  - ClosedLoop（Toggle，默认 true）— 闭合环路
  - FlipNormals（Toggle，默认 false）— 反转法线，控制构件生成在曲面哪一侧
- **预览**：对角曲线（CurveA/CurveB）
- **说明**：网格参数改变时自动清除后续构件缓存

### 步骤 3 — 方向 A 支撑梁

- **输入类型**：OptionsOnly
- **选项**（3 个）：OffsetA / BeamWidthA / BeamDepthA
- **预览**：网格曲线 + 方向 A 支撑梁

### 步骤 4 — 方向 B 支撑梁

- **输入类型**：OptionsOnly
- **选项**（3 个）：OffsetB / BeamWidthB / BeamDepthB
- **预览**：网格曲线 + 方向 A/B 支撑梁

### 步骤 5 — 节点与爪臂

- **输入类型**：OptionsOnly
- **选项**（5 个）：JointRadius / JointOffset / ClawLength / ClawWidth / ClawDepth
- **预览**：网格曲线 + 支撑梁 + 节点

### 步骤 6 — 玻璃面板

- **输入类型**：OptionsOnly
- **选项**（1 个）：GlassThickness
- **预览**：网格曲线 + 支撑梁 + 节点 + 玻璃

### 步骤 7 — 窗框

- **输入类型**：OptionsOnly
- **选项**（2 个）：FrameWidth / FrameDepth
- **预览**：全部构件（累积）

### 完成 — 写入文档

- **OnFinish**：以 `isPreview=false` 调用 Command 层生成最终几何
- **图层写入**（内联创建图层）：
  - `FenghuangCenter::CurveA` — 方向 A 对角曲线
  - `FenghuangCenter::CurveB` — 方向 B 对角曲线
  - `FenghuangCenter::StructureA` — 方向 A 支撑梁
  - `FenghuangCenter::StructureB` — 方向 B 支撑梁
  - `FenghuangCenter::Joints` — 节点 + 爪臂
  - `FenghuangCenter::Glass` — 玻璃面板
  - `FenghuangCenter::Frame` — 窗框

## 依赖关系

```
Project/FenghuangCenter/CurtainWall/CurtainWallCmd.cs
    ↓ 调用
Command/FenghuangCenter/CurtainWallCmd.cs  (Rh.Cmd.CurtainWallCmd)
    ↓ 调用
Geometry/FenghuangCenter/*.cs              (Rh.Geo.FenghuangCenter)
    ↓ 调用（同层复用）
Geometry/Surface/SurfaceGeo.cs             (Rh.Geo.Srf)
Geometry/Solid/SolidGeo.cs                 (Rh.Geo.Sld)
```

## 参数体系

共 19 个用户参数，默认值存储于 `Data/Command/FenghuangCenter/CurtainWall.json`。参数按构件类型分组在 7 个交互步骤中逐步设置，详见 Command 层文档。

## 开发说明

- 本项目为单命令项目，所有逻辑集中在 `CurtainWall/` 子目录
- 幕墙生成算法属于项目专属能力，置于 Geometry 层 `Rh.Geo.FenghuangCenter` 命名空间
- 基础图元（放样/圆柱/Box/平面面/合并）复用框架 `SurfaceGeo` / `SolidGeo`，不重复封装
