# 测试命令 Overview

## 1. 目标

在 Rhino 中运行测试命令，调用所有 Command/Geometry 层方法，将结果写入文档供人工可视化检查。

## 2. 命令一览

| 命令名 | 功能 | 步骤数 | 覆盖范围 |
|--------|------|--------|---------|
| `RhTestChain1` | 基础创建链：点→线→面→体 | 62 | Point 全部 + Curve 基础 + Surface 从曲线 + Solid 基础 |
| `RhTestChain2` | 自由曲线与曲面 | 19 | Curve 自由曲线 + Surface 点/网络/补面/Heightfield |
| `RhTestChain3` | 对象提取与派生 | 12 | Curve 提取/投影/剖面（含 DupEdge 双重载） |
| `RhTestChain4` | 变换与阵列 | 21 | Transform 全部 21 方法/重载 |
| `RhTestChain5` | 网格与转换 | 19 | Mesh 全部 19 方法/重载（含凸包算法） |
| `RhTestChain6` | Solid 专属与特殊 | 14 | Solid 管道/板/文字/加厚/封盖（含 TextObject 实现） |
| `RhTestImageFit` | 图片圆拟合（交互） | 1 | ImageCmd.CreateCircleFit（灰度采样→圆阵列） |

## 3. 空间布局总览

所有测试结果按 X 轴分区，确保视图中物体不重叠：

```
X轴分区（俯视图，Z=0 为地面）

  X=0~100     链1：基础创建链
  X=120~220   链2：自由曲线与曲面
  X=240~340   链3：对象提取与派生
  X=360~480   链4：变换与阵列
  X=500~620   链5：网格与转换
  X=640~760   链6：Solid 专属
```

链内按子分类沿 Y 轴排列，每步间距 ≥12 单位，确保半径 5 以内的物体不重叠。

## 4. 图层体系

```
Test::
├── Chain1::Point           链1的点/点云
├── Chain1::Curve           链1的线/圆/弧/圆锥曲线
├── Chain1::Surface         链1的曲面
├── Chain1::Solid           链1的实体
├── Chain2::Curve           链2的自由曲线
├── Chain2::Surface         链2的曲面
├── Chain3::Extract         链3的提取结果
├── Chain4::Transform       链4的变换结果
├── Chain5::Mesh            链5的网格
├── Chain6::Solid           链6的专属实体
└── ImageFit::Circle        图片圆拟合结果
```

每个图层使用不同颜色，方便视口中区分。

## 5. 运行效果

每个命令执行后：
1. 命令行逐条输出 `[PASS]` / `[FAIL]` + 方法名 + 简要结果
2. 成功创建的几何对象写入文档，按图层区分
3. 最终输出汇总：`Total X, Passed Y, Failed Z`
4. Rhino 视口可直接查看所有几何结果，旋转检查空间分布

## 6. 测试开发规则

### 6.1 测试程序与插件命令的区别

| 维度 | 插件命令（UICommand） | 测试程序（TestBase） |
|------|----------------------|---------------------|
| 基类 | `UICommand` | `Rhino.Commands.Command` |
| 交互 | 有（InputBuilder/GetPoint） | 无（自动执行） |
| 数据来源 | 用户实时输入 | 硬编码测试数据 |
| 平面来源 | 视口 `ConstructionPlane()` | 硬编码 `Plane`（如 WorldXY@(140,172,0)） |
| 预览 | isPreview 模式 | 无 |
| 结果处理 | OnFinish 写入文档 | Assert 断言 + WriteToDoc 可视化 |
| 目标 | 完成用户意图 | 覆盖所有代码路径 |

### 6.2 断言规则

- **值类型断言用 `NotNull`**：`Circle`/`Arc`/`Line`/`Polyline`/`Ellipse` 等是值类型，不继承 `GeometryBase`，用 `Assert.NotNull`
- **引用类型断言用 `IsValid`**：`Curve`/`Brep`/`Mesh`/`Surface` 等继承 `GeometryBase`，用 `Assert.IsValid`（同时检查 null 和有效性）

| 验证类型 | 规则 | 示例 |
|---------|------|------|
| 非空检查 | 返回值不为 null | `result != null` |
| 有效性检查 | 几何对象 IsValid | `result.IsValid` |
| 数值精度 | 关键数值在容差内 | `Math.Abs(r - 5.0) < 1e-6` |
| 几何属性 | 体积/面积/长度 > 0 | `brep.GetVolume() > 0` |
| 数组长度 | 阵列/提取结果数量正确 | `results.Length == count` |

> 断言失败时记录 `[FAIL]` 并继续执行后续步骤（不中断）。
> 几何对象无论断言是否通过都写入文档，方便人工检查失败原因。

### 6.3 测试覆盖原则

测试的目的不是"让代码通过"，而是"验证代码在各种输入下都能正确工作"。如果有多个重载、多种输入场景，就应该**编写多个测试**，分别覆盖每一种情况。

- 每个重载至少有一个测试
- 同一重载的不同输入场景（如不同平面、不同参数组合）应分别测试
- **所有方法必须实现并测试，不能 Skip**：未实现的方法必须先实现再测试，不存在"跳过"的测试
- **测试层直接调用每个方法/重载，不需要中间选择层**（如枚举）——测试的目的是覆盖所有代码路径，直接分别调用即可
- 不要为了通过断言而改变测试目标——测试目标是固定的，代码实现应该满足测试

### 6.4 断言必须基于固定的预期值

断言的预期值必须是**预先确定的常数**，不能从输入参数推导。

- **错误（恒真断言）**：`Assert.Count(input.Count, result.Length)` —— 无论 result 是什么，只要不丢数据就恒真
- **正确（固定断言）**：`Assert.Count(4, result.Length)` —— 预期值 4 是基于功能理解确定的

恒真断言的危害：即使被测代码有 bug（如跳过了某些元素），测试仍然 PASS，无法发现错误。

### 6.5 断言强度——越精确越好

| 断言类型 | 强度 | 使用场景 |
|---------|------|---------|
| `IsValid` | 弱 | 只验证"有结果"，无法发现质量问题 |
| `GreaterThanZero` | 中 | 验证"有元素"，但 1 个和 100 个都通过 |
| `Count(N)` | **强** | 精确验证数量，能捕获多/少返回的 bug |

优先使用 `Count(N)`。只有在数量确实不确定时（如投影结果依赖几何相交），才降级用 `GreaterThanZero`。

### 6.6 拓扑查询优先于拓扑假设

编写涉及 Brep 拓扑（边数、面数、Naked/Interior 分类）的测试时，**必须先查询实际拓扑，再写断言**。

```
错误流程：假设"矩形挤出有 8 条 Naked 边" → 写 Assert.Count(8) → FAIL
正确流程：查询 _brep.Edges.Count(e => e.Valence == Naked) → 看到 2 → 写 Assert.Count(2)
```

Rhino 的 Brep 拓扑会合并接缝、识别周期性——这些只有运行时查询才知道，不能凭直觉假设。

### 6.7 测试 FAIL 时先确认是被测代码错还是测试预期错

测试 FAIL 有两种可能：
1. **被测代码有 bug** → 修改被测代码
2. **测试预期错误**（如拓扑假设错） → 修改测试

正确流程：FAIL → 查询实际值 → 对比预期值 → 判断哪边错了 → 只改错的那边

错误流程：FAIL → 直接改测试（可能掩盖被测代码的 bug）或直接改被测代码（可能破坏正确逻辑）。

---

## 7. 测试链 1：基础创建链（点→线→面→体）

**流程**：创建点 → 从点建立线/圆/弧 → 从曲线创建曲面 → 从曲线挤出实体

### 7.1 Point 子区（X=0，Y 沿轴排列）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 衔接→ | 断言 |
|------|-------------|---------|---------|------|-------|------|
| 1 | `CreatePoint(#1)` | `pt=(0,0,0)` | (0,0,0) | Point | — | pt != null |
| 2 | `CreatePoint(#2)` | `x=0, y=3, z=0` | (0,3,0) | Point | — | pt != null |
| 3 | `CreatePoints` | `[(0,5,0),(0,6,0),(0,7,0),(0,8,0),(0,9,0)]` | (0,7,0) | Point | — | list.Count==5 |
| 4 | `CreatePointGrid` | `plane=WorldXY, xCount=3, yCount=3, xDom=(0~6), yDom=(12~18)` | (0,15,0) | Point | — | cloud.Count==9 |
| 5 | `CreatePointCloud` | `[(0,22,0),(0,23,0),(0,24,0),(0,25,0)]` | (0,24,0) | Point | → 步骤7 | cloud.Count==4 |
| 6 | `CreatePointCloudFromMesh` | `mesh=临时Box网格(中心(0,30,0))` | (0,30,0) | Point | — | cloud.Count>0 |
| 7 | `AddPointsToCloud` | `cloud=步骤5, pts=[(1,22,0),(1,23,0)]` | (0,24,0) | Point | → 步骤8 | cloud.Count==6 |
| 8 | `RemovePointsFromCloud` | `cloud=步骤7, indices=[0,1]` | (0,24,0) | Point | → 步骤9 | cloud.Count==4 |
| 9 | `ReducePointCloud` | `cloud=步骤8, removeCount=1` | (0,24,0) | Point | — | cloud.Count==3 |

> 步骤6 的临时 Box 网格不写入文档（仅用于测试），使用 `MeshCmd.CreateMeshBox` 创建后立即消耗。

### 7.2 Line/Poly 子区（X=15）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 衔接→ | 断言 |
|------|-------------|---------|---------|------|-------|------|
| 10 | `CreateLine(#1)` | `start=(15,0,0), end=(15,8,0)` | (15,4,0) | Curve | — | line.Length≈8 |
| 11 | `CreateLine(#2)` | `pts=[(15,12,0),(15,15,3),(15,18,0),(15,21,0)]` | (15,16,1) | Curve | — | crv.IsValid |
| 12 | `CreatePolyline` | `pts=[(15,25,0),(15,32,0),(15,32,5),(15,25,5)], closed=true` | (15,28,2) | Curve | → 步骤41 | poly.IsValid |
| 13 | `CreateRectangle(#1)` | `plane=WorldXY, c1=(15,36,0), c2=(15,44,6)` | (15,40,3) | Curve | → 步骤42,49,55 | poly.IsValid |
| 14 | `CreateRectangle(#2)` | `plane=WorldXY, center=(15,54,0), w=8, h=6` | (15,54,0) | Curve | → 步骤41 | poly.IsValid |
| 15 | `CreatePolygon(#1)` | `plane=WorldXY, center=(15,64,0), sides=6, radius=4` | (15,64,0) | Curve | — | poly.IsValid |
| 16 | `CreatePolygon(#2)` | `plane=WorldXY, start=(15,72,0), end=(15,76,0), sides=5` | (15,74,0) | Curve | — | poly.IsValid |
| 17 | `CreatePolygon(#3)` | `plane=WorldXY, center=(15,84,0), sides=5, outerR=5, innerR=2` | (15,84,0) | Curve | — | poly.IsValid |

### 7.3 Circle/Arc/Conic 子区（X=35）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 衔接→ | 断言 |
|------|-------------|---------|---------|------|-------|------|
| 18 | `CreateCircle(#1)` | `plane=WorldXY, center=(35,0,0), radius=4` | (35,0,0) | Curve | → 步骤43 | circle.Radius≈4 |
| 19 | `CreateCircle(#2)` | `center=(35,10,0), normal=(0,0,1), radius=4` | (35,10,0) | Curve | → 步骤43 | circle.IsValid |
| 20 | `CreateCircle(#3)` | `plane=WorldXY, radius=4`（原点圆心） | (35,20,0) | Curve | — | circle.Radius≈4 |
| 21 | `CreateCircle(#4)` | `plane=WorldXY, p1=(35,26,0), p2=(35,34,0)`（直径） | (35,30,0) | Curve | — | circle.IsValid |
| 22 | `CreateCircle(#5)` | `p1=(31,40,0), p2=(35,44,0), p3=(39,40,0)` | (35,40,0) | Curve | — | circle.IsValid |
| 23 | `CreateCircle(#6)` | `start=(35,48,0), tangent=(1,0,0), end=(35,56,0)` | (35,52,0) | Curve | — | circle.IsValid |
| 24 | `CreateCircle(#7)` | `场景A: L形相交线 r=3; 场景B: 平行线 d=6 r=3(强制d/2)` | (35,60~68,0) | Curve | — | circles.Length>0（两场景均验证） |
| 25 | `CreateArc(#1)` | `plane=WorldXY, center=(35,72,0), r=4, startAng=0, endAng=π` | (35,72,0) | Curve | — | arc.IsValid |
| 26 | `CreateArc(#2)` | `start=(31,80,0), onArc=(35,84,0), end=(39,80,0)` | (35,80,0) | Curve | — | arc.IsValid |
| 27 | `CreateArc(#3)` | `start=(31,90,0), end=(39,90,0), dir=(0,1,0)` | (35,90,0) | Curve | — | arc.IsValid |
| 28 | `CreateArc(#4)` | `场景A: L形相交线 r=3; 场景B: 平行线 d=6 r=3(强制d/2)` | (35,98~106,0) | Curve | — | arcs.Length>0（两场景均验证） |
| 29 | `CreateEllipse(#1)` | `plane=WorldXY, center=(35,112,0), r1=5, r2=3` | (35,112,0) | Curve | → 步骤46 | crv.IsValid |
| 30 | `CreateEllipse(#2)` | `plane=WorldXY, p1=(31,122,0), p2=(39,122,0), r2=3` | (35,122,0) | Curve | — | crv.IsValid |
| 31 | `CreateEllipse(#3)` | `f1=(33,132,0), f2=(37,132,0), pt=(35,135,0)` | (35,132,0) | Curve | — | crv.IsValid |
| 32 | `CreateParabola` | `start=(31,142,0), onParabola=(35,146,0), end=(39,142,0)` | (35,142,0) | Curve | — | crv.IsValid |
| 33 | `CreateHyperbola` | `focus=(35,154,0), vertex=(35,156,0), end=(39,158,0)` | (35,156,0) | Curve | — | crv.IsValid |
| 34 | `CreateConic` | `start=(31,166,0), end=(39,166,0), apex=(35,170,0), rho=0.5` | (35,166,0) | Curve | — | crv.IsValid |

> 步骤24/28 各测试两种相切场景：场景A 为 L 形垂直相交线段（1 个唯一解），场景B 为平行线 + r<d/2（2 个对称解，Rhino Tangent 典型用例）。两场景的临时线段均不写入文档，仅用于测试相切圆/弧。

### 7.4 Surface 子区（X=60）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 衔接→ | 断言 |
|------|-------------|---------|---------|------|-------|------|
| 35 | `CreatePlane(#1)` | `plane=WorldXY, domU=(0~8), domV=(0~6)` 平移到(60,0,0) | (60,3,0) | Surface | — | brep.IsValid |
| 36 | `CreatePlane(#2)` | `p1=(60,12,0), p2=(68,12,0), p3=(60,18,6)` | (60,15,0) | Surface | — | brep.IsValid |
| 37 | `CreatePlane(#3)` | `start=(60,24,0), end=(60,32,0), height=6, normal=(0,0,1)` | (60,28,3) | Surface | — | brep.IsValid |
| 38 | `CreatePlaneThroughPt` | `pts=[(60,36,0),(64,36,0),(60,40,0),(64,40,0),(62,38,2)]` | (60,38,0) | Surface | — | brep.IsValid |
| 39 | `CreateCutPlane` | `plane=WorldXY@ (60,46,0), objects=[步骤35结果]` | (60,46,0) | Surface | — | brep.IsValid |
| 40 | `CreateSrfPt` | `p1=(60,52,0), p2=(66,52,0), p3=(66,58,4), p4=(60,58,4)` | (60,55,1) | Surface | — | brep.IsValid |
| 41 | `CreateEdgeSrf` | `edges=[步骤12的Polyline, 步骤14的Rectangle]` | (60,64,0) | Surface | — | brep.IsValid |
| 42 | `CreatePlanarSrf` | `curves=[步骤13的Rectangle]` | (60,72,0) | Surface | — | breps.Length>0 |
| 43 | `CreateLoft` | `curves=[步骤18的Circle, 步骤19的Circle]` | (60,80,0) | Surface | — | breps.Length>0 |
| 44 | `CreateSweep(#1)` | `rail=临时直线(60,88,0)-(60,96,6), shapes=[临时小圆(60,88,0) r=2]` | (60,92,3) | Surface | — | breps.Length>0 |
| 45 | `CreateSweep(#2)` | `rail1=临时直线, rail2=临时直线, shapes=[临时圆]` | (60,105,0) | Surface | — | breps.Length>0 |
| 46 | `CreateRevolve(#1)` | `profile=步骤29的Ellipse, axis=Line((40,112,0)-(40,112,10))` | (60,118,0) | Surface | — | brep.IsValid |
| 47 | `CreateRevolve(#2)` | `profile=临时弧线, axis=Line, startAng=0, endAng=π/2` | (60,132,0) | Surface | — | brep.IsValid |
| 48 | `CreateRailRevolve` | `profile=临时线, rail=临时弧, axis=Line, scaleHeight=false` | (60,146,0) | Surface | — | brep.IsValid |
| 49 | `CreateExtrude(#1)` | `profile=步骤13 Rectangle副本, dir=(0,0,6)` | (60,158,0) | Surface | — | brep.IsValid |
| 50 | `CreateExtrude(#2)` | `profile=临时圆, path=临时曲线, cap=false` | (60,172,0) | Surface | — | brep.IsValid |
| 51 | `CreateExtrude(#3)` | `profile=临时矩形, dir=(0,0,1), distance=6, draftAng=5°` | (60,186,0) | Surface | — | brep.IsValid |
| 52 | `CreateExtrude(#4)` | `profile=临时圆 r=3@ (60,196,0), apex=(60,196,8)` | (60,196,0) | Surface | — | brep.IsValid |
| 53 | `CreateRibbon` | `curve=临时弧线@ (60,210,0), distance=3, plane=WorldXY` | (60,210,0) | Surface | — | brep.IsValid |
| 54 | `CreateFin` | `curve=临时线@ (60,222,0), face=步骤35的BrepFace, height=5` | (60,222,0) | Surface | — | brep.IsValid |

### 7.5 Solid 子区（X=90）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 衔接→ | 断言 |
|------|-------------|---------|---------|------|-------|------|
| 55 | `CreateExtrudeSolid` | `profile=YZ平面矩形(82,8,0)-(82,16,6), dir=(6,0,0), capEnds=true` | (85,12,3) | Solid | — | brep.IsSolid |
| 56 | `CreateBox(#1)` | `box=Box(WorldXY, (90,8,0)-(98,16,6))` | (90,12,3) | Solid | → 链4/5 引用 | brep.IsSolid |
| 57 | `CreateBox(#2)` | `c1=(90,22,0), c2=(98,30,0), normal=(0,0,1)` | (90,26,3) | Solid | — | brep.IsSolid |
| 58 | `CreateSphere` | `center=(90,38,4), normal=(0,0,1), radius=4` | (90,38,4) | Solid | → 链4 引用 | brep.IsSolid |
| 59 | `CreateCylinder` | `base=(90,50,0), normal=(0,0,1), r=3, h=8, cap=true` | (90,50,4) | Solid | — | brep.IsSolid |
| 60 | `CreateCone` | `base=(90,62,0), normal=(0,0,1), r=3, h=8, cap=true` | (90,62,3) | Solid | — | brep.IsSolid |
| 61 | `CreateTruncatedCone` | `base=(90,74,0), normal=(0,0,1), rBot=3, rTop=1.5, h=8, cap=true` | (90,74,4) | Solid | — | brep.IsSolid |
| 62 | `CreatePyramid` | `base=(90,86,0), normal=(0,0,1), sides=5, r=3, h=8, cap=true` | (90,86,4) | Solid | — | brep.IsSolid |

---

## 8. 测试链 2：自由曲线与曲面

**流程**：创建自由曲线 → 从控制点/插值点创建曲面 → 网络曲面/补面/特殊曲面

### 8.1 自由曲线子区（X=120）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 衔接→ | 断言 |
|------|-------------|---------|---------|------|-------|------|
| 1 | `CreateNurbsCurve(#1)` | `pts=[(120,0,0),(124,2,3),(128,4,0),(132,6,3),(136,8,0)], degree=3` | (120,4,1) | Curve | → 步骤12 | crv.IsValid |
| 2 | `CreateNurbsCurve(#2)` | `pts=[(120,12,0),(124,14,0),(128,16,0)], knots=[0,0,0,1,1,1], degree=2, weights=[1,1,1]` | (120,14,0) | Curve | — | crv.IsValid |
| 3 | `CreateInterpCrv` | `pts=[(120,20,0),(124,22,2),(128,24,0),(132,26,2)], degree=3` | (120,23,1) | Curve | → 步骤13 | crv.IsValid |
| 4 | `CreateHandleCurve` | `handles=[((120,30,0),(122,33,0)),((126,34,0),(128,32,0))], closed=false` | (120,32,0) | Curve | — | crv.IsValid |
| 5 | `CreateCurveThroughPt` | `pts=[(120,38,0),(124,40,1),(128,42,0),(132,44,1)], degree=3` | (120,41,0) | Curve | — | crv.IsValid |
| 6 | `CreateCatenary` | `start=(120,48,0), end=(132,48,0), length=16, gravity=(0,0,-1)` | (120,48,2) | Curve | — | crv.IsValid |
| 7 | `CreateHelix(#1)` | `axis=Line((120,54,0)-(120,54,12)), startR=2, endR=2, turns=3, pitch=4` | (120,54,6) | Curve | — | crv.IsValid |
| 8 | `CreateHelix(#2)` | `rail=临时曲线(120,62,0)-(132,62,0), startR=1, endR=2, turns=2` | (120,62,0) | Curve | — | crv.IsValid |
| 9 | `CreateSpiral` | `plane=WorldXY@(120,70,0), center=(120,70,0), startR=1, endR=5, turns=3` | (120,70,0) | Curve | — | crv.IsValid |

### 8.2 曲面子区（X=120，Y=80 起）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 衔接→ | 断言 |
|------|-------------|---------|---------|------|-------|------|
| 10 | `CreateSrfThroughPts` | `pts=4×4点网格@ (120,80,0), uCount=4, vCount=4, uDeg=3, vDeg=3` | (120,84,0) | Surface | — | brep.IsValid |
| 11 | `CreateSrfControlPts` | `pts=4×4控制点@ (120,96,0), uCount=4, vCount=4, uDeg=3, vDeg=3` | (120,100,0) | Surface | — | brep.IsValid |
| 12 | `CreateNetworkSrf(#1)` | `curves=[步骤1曲线 + 3条临时U/V曲线]` | (120,114,0) | Surface | — | brep.IsValid |
| 13 | `CreateNetworkSrf(#2)` | `uCurves=[步骤3曲线 + 临时1], vCurves=[2条临时曲线]` | (120,126,0) | Surface | — | brep.IsValid |
| 14 | `CreatePatch` | `geometry=[临时弧线 + 临时线段 + 步骤1曲线]` | (120,138,0) | Surface | — | brep.IsValid |
| 15 | `CreateDevLoft` | `rail1=临时曲线A(120,148,0)-(132,148,4), rail2=临时曲线B(120,152,0)-(132,152,4)` | (120,150,0) | Surface | — | brep.IsValid |
| 16 | `CreateDrape` | `objects=[临时Brep@ (120,160,0)], plane=WorldXY, uSpacing=10, vSpacing=10` | (120,160,0) | Surface | — | brep.IsValid |
| 17a | `CreateHeightfield(#1 完全控制)` | `imagePath, plane=WorldXY@(120,172,0), w=10, h=8, maxH=4, samplesX=20, samplesY=16` | (120,172,0) | Surface | — | brep.IsValid |
| 17b | `CreateHeightfield(#2 按比例)` | `imagePath, plane=WorldXY@(140,172,0), w=10, maxH=4, samplesX=20, samplesY=16` | (140,172,0) | Surface | — | brep.IsValid（heightSize 由图片比例推导） |
| 17c | `CreateHeightfield(#3 全自动)` | `imagePath, plane=WorldXY@(160,172,0), w=10, maxH=4` | (160,172,0) | Surface | — | brep.IsValid（heightSize 和 samples 都自动推导，上限 50） |

> 步骤17 需要一个测试图像文件（位于 `Assets/Test/test_heightfield.png`）。若文件不存在，此步骤标记为 SKIP。
> 按测试覆盖原则，3 个重载各有独立测试步骤（17a/17b/17c）。

---

## 9. 测试链 3：对象提取与派生

**流程**：每条链自给自足——先创建基础几何，再执行提取操作

### 9.1 创建基础几何（X=240，供本链提取使用）

| 步骤 | 方法 | 输入参数 | 中心坐标 | 图层 | 衔接→ |
|------|------|---------|---------|------|-------|
| 0a | `CreateCircle` | `plane=WorldXY, center=(240,0,0), radius=6` | (240,0,0) | Extract | → 步骤1,2,3,4 |
| 0b | `CreateRectangle` | `plane=WorldXY, center=(240,15,0), w=10, h=8` | (240,15,0) | Extract | → 步骤8 |
| 0c | `CreateExtrude` | `profile=0b Rectangle, dir=(0,0,6)` | (240,15,3) | Extract | → 步骤5,6,7,9,10,11 |
| 0d | `CreateSphere` | `center=(240,35,5), normal=(0,0,1), radius=5` | (240,35,5) | Extract | → 步骤9,10 |

### 9.2 提取操作（X=260）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 断言 |
|------|-------------|---------|---------|------|------|
| 1 | `CreateDividePoints(#1)` | `curve=步骤0a Circle, segmentCount=6` | (260,0,0) | Extract | pts.Count==6 |
| 2 | `CreateDividePoints(#2)` | `curve=步骤0a Circle, segmentLength=3` | (260,10,0) | Extract | pts.Count>0 |
| 3 | `GetCurveStart` | `curve=步骤0a Circle` | (260,20,0) | Extract | pt != null |
| 4 | `GetCurveEnd` | `curve=步骤0a Circle` | (260,25,0) | Extract | pt != null |
| 5 | `CreateProjectCrv` | `curves=[临时曲线@ (260,30,5)], target=步骤0c Brep, dir=(0,0,-1)` | (260,30,0) | Extract | crvs.Length>0 |
| 6 | `CreatePullCrv` | `curves=[临时曲线@ (260,40,3)], target=步骤0c Brep, tol=0.001` | (260,40,0) | Extract | crvs.Length>0 |
| 7 | `CreateApplyCrv` | `curves=[临时曲线@ (260,50,3)], target=步骤0c Brep` | (260,50,0) | Extract | crvs.Length>0 |
| 8a | `CreateDupEdge(#1 指定边)` | `brep=步骤0c 闭合实体, edges=前4条Interior边` | (260,60,0) | Extract | crvs.Length==4（#1 按调用者指定的数量提取，不做过滤） |
| 8b | `CreateDupEdge(#2 全部Naked边)` | `brep=步骤0e 开放Brep` | (260,65,0) | Extract | crvs.Length==2（#2 只提取Naked边：顶底接缝） |
| 9 | `CreateExtractIsocurve` | `brep=步骤0c Brep, point=(240,15,3), direction=0` | (260,70,0) | Extract | crv.IsValid |
| 10 | `CreateContour` | `geometry=步骤0d Sphere, startPt=(260,80,0), endPt=(260,80,10), interval=2` | (260,80,0) | Extract | crvs.Length>0 |
| 11 | `CreateSection` | `geometry=步骤0d Sphere, cutPlane=WorldXY@(260,90,3)` | (260,90,0) | Extract | crvs.Length>0 |

---

## 10. 测试链 4：变换与阵列

**流程**：创建基础几何 → 施加各种变换 → 阵列分布

### 10.1 基础几何（X=360）

| 步骤 | 方法 | 输入参数 | 中心坐标 | 图层 | 衔接→ |
|------|------|---------|---------|------|-------|
| 0a | `CreateSphere` | `center=(360,0,4), normal=(0,0,1), radius=3` | (360,0,4) | Transform | → 步骤1-9 |
| 0b | `CreateBox` | `c1=(360,10,0), c2=(366,16,4), normal=(0,0,1)` | (360,13,2) | Transform | → 步骤10-15 |
| 0c | `CreateLine` | `start=(360,25,0), end=(390,25,0)` | (360,25,0) | Transform | → 步骤13,14,18 |
| 0d | `CreatePlane` | `plane=WorldXY@(360,30,0), domU=(0~20), domV=(0~15)` | (360,35,0) | Transform | → 步骤15,17 |

### 10.2 基本变换（X=380，每步偏移 Y=12）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 断言 |
|------|-------------|---------|---------|------|------|
| 1 | `Move` | `geo=步骤0a Sphere副本, trans=(20,0,0)` | (380,0,4) | Transform | result != null |
| 2 | `Copy` | `geo=步骤0a Sphere副本, trans=(20,12,0)` | (380,12,4) | Transform | result != null |
| 3 | `Rotate(#1)` | `geo=步骤0b Box副本, ang=π/4, center=(380,24,0)` | (380,24,2) | Transform | result != null |
| 4 | `Rotate(#2)` | `geo=步骤0b Box副本, ang=π/4, axis=(0,0,1), center=(380,36,0)` | (380,36,2) | Transform | result != null |
| 5 | `Scale(#1)` | `geo=步骤0a Sphere副本, anchor=(380,48,0), factor=1.5` | (380,48,6) | Transform | result != null |
| 6 | `Scale(#2)` | `geo=步骤0b Box副本, plane=WorldXY@(380,60,0), xF=2, yF=1, zF=1` | (380,60,2) | Transform | result != null |
| 7 | `Mirror(#1)` | `geo=步骤0a Sphere副本, mirrorPlane=Plane((380,72,0),(0,1,0))` | (380,72,4) | Transform | result != null |
| 8 | `Mirror(#2)` | `geo=步骤0b Box副本, point=(380,84,0), normal=(1,0,0)` | (380,84,2) | Transform | result != null |
| 9 | `Shear` | `geo=步骤0b Box副本, plane=WorldXY@(380,96,0), x=(1.2,0,0), y=(0,1,0), z=(0,0,1)` | (380,96,2) | Transform | result != null |

### 10.3 阵列（X=420）

| 步骤 | 方法（重载） | 输入参数 | 中心区域 | 图层 | 断言 |
|------|-------------|---------|---------|------|------|
| 10a | `ArrayLinear(#1 按间距)` | `geo=小Box, dir=(0,4,0), count=4` | Y=0,4,8,12 | Transform | arr.Length==4 |
| 10b | `ArrayLinear(#2 按总跨度)` | `geo=小Box, from=(420,16,0), to=(420,40,0), count=4` | Y=16,24,32,40 | Transform | arr.Length==4 |
| 11 | `ArrayRectangular` | `geo=小Box(420,48,0), plane=WorldXY, xC=3, yC=2, zC=1, xS=8, yS=8, zS=0` | (420,48~56,0) | Transform | arr.Length==6 |
| 12 | `ArrayPolar` | `geo=小Box(420,72,0), axis=Line((420,80,0)-(420,80,10)), count=6, ang=2π, rotate=true` | (420,72~88,0) | Transform | arr.Length==6 |
| 13 | `ArrayAlongCrv(#1)` | `geo=小Sphere(420,100,0) r=1, rail=步骤0c Line副本, count=4, orient=true` | (420,100,0) | Transform | arr.Length==4 |
| 14 | `ArrayAlongCrv(#2)` | `geo=小Sphere(420,116,0) r=1, rail=步骤0c Line副本, spacing=8, orient=false` | (420,116,0) | Transform | arr.Length>0 |
| 15 | `ArrayOnSrf` | `geo=小Sphere r=1, surface=步骤0d Plane副本, uCount=3, vCount=2` | (420,132,0) | Transform | arr.Length==6 |

### 10.4 定向与投影（X=460）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 断言 |
|------|-------------|---------|---------|------|------|
| 16 | `Orient` | `geo=步骤0a Sphere副本, source=WorldXY@(460,0,0), target=Plane((460,10,0),(0,0,1))` | (460,5,0) | Transform | result != null |
| 17 | `OrientOnSrf` | `geo=步骤0a Sphere副本, source=WorldXY, surface=步骤0d Plane副本, targetPt=(460,24,0)` | (460,24,0) | Transform | result != null |
| 18 | `OrientOnCrv` | `geo=步骤0a Sphere副本, source=WorldXY, rail=步骤0c Line副本, param=0.5` | (460,36,0) | Transform | result != null |
| 19 | `RemapCPlane` | `geo=步骤0a Sphere副本, oldCPlane=WorldXY, newCPlane=Plane((460,48,0),(0,1,0))` | (460,48,0) | Transform | result != null |
| 20 | `ProjectToCPlane` | `geo=步骤0a Sphere副本(抬高), plane=WorldXY@(460,60,0)` | (460,60,0) | Transform | result != null |

---

## 11. 测试链 5：网格与转换

**流程**：创建网格图元 → 从 Brep/Surface 转换 → 从点集创建 → 四边形重网格化

### 11.1 基础几何（X=500）

| 步骤 | 方法 | 输入参数 | 中心坐标 | 图层 | 衔接→ |
|------|------|---------|---------|------|-------|
| 0a | `CreateBox` | `c1=(500,0,0), c2=(506,6,6), normal=(0,0,1)` | (500,3,3) | Mesh | → 步骤10,18 |
| 0b | `CreateSphere` | `center=(500,12,4), normal=(0,0,1), radius=4` | (500,12,4) | Mesh | — |
| 0c | `CreatePlane(#1)` | `plane=WorldXY@(500,18,0), domU=(0~8), domV=(0~6)` | (500,22,0) | Mesh | → 步骤11 |
| 0d | `CreatePolyline` | `pts=[(500,28,0),(506,28,0),(506,34,0),(500,34,0)]` | (500,31,0) | Mesh | → 步骤12,13 |

### 11.2 网格图元（X=520，每步 Y 间距 12）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 断言 |
|------|-------------|---------|---------|------|------|
| 1 | `CreateMeshBox(#1)` | `bbox=BoundingBox((520,0,0),(528,6,6))` | (520,3,3) | Mesh | mesh.IsValid |
| 2 | `CreateMeshBox(#2)` | `c1=(520,12,0), c2=(528,18,6), normal=(0,0,1)` | (520,15,3) | Mesh | mesh.IsValid |
| 3 | `CreateMeshSphere(#1)` | `center=(520,26,4), normal=(0,0,1), radius=4, segments=16, rings=12` | (520,26,4) | Mesh | mesh.IsValid |
| 4 | `CreateMeshSphere(#2)` | `center=(520,38,4), normal=(0,0,1), radius=4, subdivisions=2` | (520,38,4) | Mesh | mesh.IsValid |
| 5 | `CreateMeshCylinder` | `center=(520,48,0), normal=(0,0,1), r=3, h=8, vertical=4, around=12, cap=true` | (520,48,4) | Mesh | mesh.IsValid |
| 6 | `CreateMeshCone` | `base=(520,60,0), normal=(0,0,1), r=3, h=8, vertical=4, around=12, cap=true` | (520,60,3) | Mesh | mesh.IsValid |
| 7 | `CreateMeshTorus` | `center=(520,72,0), normal=(0,0,1), majorR=5, minorR=1.5, majorSeg=16, minorSeg=8` | (520,72,0) | Mesh | mesh.IsValid |
| 8 | `CreateMeshEllipsoid` | `center=(520,84,0), normal=(0,0,1), radii=(5,3,2), segments=16, rings=12` | (520,84,0) | Mesh | mesh.IsValid |
| 9 | `CreateMeshPlane` | `plane=WorldXY@(520,96,0), domU=(0~8), domV=(0~6), xCount=4, yCount=3` | (520,99,0) | Mesh | mesh.IsValid |

### 11.3 从几何体转换（X=550）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 断言 |
|------|-------------|---------|---------|------|------|
| 10 | `CreateMeshFromBrep` | `brep=步骤0a Box, parameters=默认` | (550,0,3) | Mesh | meshes.Length>0 |
| 11 | `CreateMeshFromSurface` | `surface=步骤0c BrepFace, parameters=默认` | (550,12,0) | Mesh | mesh.IsValid |
| 12 | `CreateMeshFromPolyline` | `polyline=步骤0d Polyline` | (550,24,0) | Mesh | mesh.IsValid |
| 13 | `CreateMeshFromPlanarBoundary` | `boundary=步骤0d Polyline副本, tol=0.001` | (550,36,0) | Mesh | mesh.IsValid |
| 14 | `CreateMeshExtrusion` | `profile=临时圆@ (550,48,0), dir=(0,0,6)` | (550,48,3) | Mesh | mesh.IsValid |

### 11.4 从点集创建（X=575）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 断言 |
|------|-------------|---------|---------|------|------|
| 15 | `CreateMeshFromPoints` | `pts=立方体8顶点+顶/底外部点=10个非共面点, tol=0.001` | (577,2,2) | Mesh | mesh.IsValid + Faces.Count > 0 |
| 16 | `CreateMeshFromTessellation` | `pts=5个点(含中心点), edges=null, plane=WorldXY, allowNew=false` | (577,12,0) | Mesh | mesh.IsValid + Faces.Count > 0 |
| 17 | `CreateMeshPatch` | `pts=[(575,20,0),(579,20,0),(575,24,0),(579,24,0)], tol=0.001` | (575,22,0) | Mesh | mesh.IsValid |

### 11.5 重网格化（X=600）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 断言 |
|------|-------------|---------|---------|------|------|
| 18 | `CreateQuadRemesh(#1)` | `brep=步骤0a Box, targetQuadCount=50, adaptSize=0.5` | (600,0,3) | Mesh | mesh.IsValid |
| 19 | `CreateQuadRemesh(#2)` | `mesh=步骤10 MeshFromBrep 结果, targetQuadCount=50, adaptSize=0.5` | (600,12,3) | Mesh | mesh.IsValid |

> 注：`CreateQuadRemesh` 有两个重载（Brep 和 Mesh），分别测试。

---

## 12. 测试链 6：Solid 专属与特殊

**流程**：创建管道/板/文字 → 加厚/封盖 → 布尔合并

### 12.1 基础几何（X=640）

| 步骤 | 方法 | 输入参数 | 中心坐标 | 图层 | 衔接→ |
|------|------|---------|---------|------|-------|
| 0a | `CreateLine` | `start=(640,0,0), end=(640,0,12)` | (640,0,6) | Solid | → 步骤1,2 |
| 0b | `CreateArc` | `plane=WorldXY, center=(640,15,0), r=5, startAng=0, endAng=π` | (640,15,0) | Solid | → 步骤1 |
| 0c | `CreatePolyline` | `pts=[(640,25,0),(648,25,0),(648,33,0),(640,33,0)], closed=true` | (640,29,0) | Solid | → 步骤3 |
| 0d | `CreateLoft` | `curves=[临时圆A(640,40,0) r=3, 临时圆B(640,40,5) r=2]` | (640,40,2) | Solid | → 步骤5 |
| 0e | `CreateExtrude` | `profile=临时矩形, dir=(0,0,4), cap=false` | (640,52,0) | Solid | → 步骤6 |

### 12.2 Solid 专属操作（X=660）

| 步骤 | 方法（重载） | 输入参数 | 中心坐标 | 图层 | 断言 |
|------|-------------|---------|---------|------|------|
| 1 | `CreatePipe(#1)` | `rail=步骤0a Line, radius=1.5, capMode=1(Flat)` | (660,0,6) | Solid | brep.IsSolid |
| 2 | `CreatePipe(#2)` | `rail=步骤0a Line副本, innerR=1, outerR=2, capMode=2(Round)` | (660,12,6) | Solid | brep.IsSolid |
| 3 | `CreateSlab` | `profile=步骤0c PolylineCurve, offset=2, dir=(0,0,1), cap=true` | (660,24,1) | Solid | brep.IsSolid |
| 4 | `CreateTextObject` | `text="Test", plane=WorldXY@(660,36,0), textHeight=4, thickness=1, font="Arial", bold=false, italic=false` | (660,36,0) | Solid | breps.Length > 0 |
| 5 | `CreateThicken` | `brep=步骤0d Loft结果, distance=1, bothSides=false` | (660,48,2) | Solid | brep.IsSolid |
| 6 | `CreateCap` | `brep=步骤0e 未封闭Extrude` | (660,60,0) | Solid | brep.IsSolid |
| 7 | `CreateSolidFromBreps` | `breps=[临时Box A(660,72,0), 临时Box B(660,73,0)]（相交）` | (660,72,0) | Solid | brep.IsSolid |
| 8 | `CreateTorus` | `center=(660,84,0), normal=(0,0,1), majorR=5, minorR=1.5` | (660,84,0) | Solid | brep.IsSolid |
| 9 | `CreateEllipsoid` | `center=(660,96,0), normal=(0,0,1), radii=(5,3,2)` | (660,96,0) | Solid | brep.IsSolid |
| 10 | `CreateTube` | `base=(660,108,0), normal=(0,0,1), innerR=2, outerR=4, h=8, cap=true` | (660,108,4) | Solid | brep.IsSolid |
| 11 | `CreateTruncatedPyramid` | `base=(660,124,0), normal=(0,0,1), sides=4, botR=4, topR=2, h=8, cap=true` | (660,124,4) | Solid | brep.IsSolid |
| 12 | `CreateLoftSolid` | `curves=[临时圆C(660,140,0) r=3, 临时圆D(660,140,5) r=2, 临时圆E(660,140,10) r=1], loftType=0, cap=true` | (660,140,5) | Solid | brep.IsSolid |
| 13 | `CreateRevolveSolid` | `profile=竖直平面(法向X)弧线 r=3 (0→π/2) @(660,154,0), axis=Z轴((660,154,0)-(660,154,10)), startAng=0, endAng=2π, cap=true` | (660,154,0) | Solid | brep.IsSolid（截面在含轴竖直面内，一端触轴） |
| 14 | `CreateSweepSolid` | `rail1=临时直线A, rail2=临时直线B, sections=[临时圆], cap=true` | (660,170,0) | Solid | brep.IsSolid |

---

## 13. 覆盖率统计

| 功能区 | 方法数 | 重载数 | 测试链覆盖 | 步骤号 | 覆盖率 |
|--------|--------|--------|-----------|--------|--------|
| Point | 8 | 9 | 链1 步骤1-9 | 全覆盖 | 100% |
| Curve | 37 | 39 | 链1 步骤10-34 + 链2 步骤1-9 + 链3 步骤1-12 | 全覆盖 | 100% |
| Surface | 30 | 30 | 链1 步骤35-54 + 链2 步骤10-17 | 全覆盖 | 100% |
| Solid | 21 | 21 | 链1 步骤55-62 + 链6 步骤1-14 | 全覆盖 | 100% |
| Mesh | 19 | 19 | 链5 步骤1-19 | 全覆盖 | 100% |
| Transform | 21 | 21 | 链4 步骤1-20 | 全覆盖 | 100% |
| **合计** | **136** | **139** | | | **100%** |

> 所有公开的 Create 方法/重载均有至少一个测试步骤覆盖。Get\* 配置方法由 Create 方法的默认参数间接调用，属于覆盖范围内。FilletGeo 通过 CreateCircle(#7)/CreateArc(#4) 间接覆盖。

## 14. 临时几何说明

部分步骤需要创建"临时几何"作为输入（如相切圆需要两条曲线），这些临时几何：
- **不写入文档**（创建后立即消耗）
- 在代码中用局部变量持有，测试步骤结束后自动 GC
- 在 Overview 中用"临时"前缀标注

---

## 15. 测试链 1 问题记录与反思

### 15.1 遇到的问题清单

| # | 步骤 | 方法 | 问题表现 | 根因 | 修复方式 |
|---|------|------|---------|------|---------|
| 1 | 13 | CreateRectangle | NullReferenceException | 两点在 WorldXY 平面 UV 方向投影重合（X 相同），返回 null | 测试改为传入 YZ 平面，验证多平面支持 |
| 2 | 35 | CreatePlane | NullReferenceException | `NurbsSurface.CreateFromPlane` 的 pointCount(2) 不满足 `> degree(2)` 约束 | pointCount 由 degree+1 自动推导 |
| 3 | 38 | CreateCutPlane | NullReferenceException | 步骤35 的 null 被传入 objects 集合，Geometry 层未检查 | Command 层增加 null 元素检查 + 错误输出 |
| 4 | 57 | CreateBox | 高度为0的退化盒子 | 世界坐标当局部坐标 + `Unitize()` 丢失 normal.Length | 用 `RemapToPlaneSpace` 转局部坐标 + `normal.Length` 作为高度 |
| 5 | 61 | CreateTruncatedCone | 非实体（IsSolid=false） | Loft 放样接缝不对齐，`CapPlanarHoles` 封盖失败 | 改用 `RevSurface`（旋转曲面） |
| 6 | 62 | CreatePyramid | 非实体 / NullReferenceException | 手动 `CreateControlPointCurve` + `CreatePlanarBreps` 创建底盖失败 | 改为：拼侧面 → JoinBreps → CapPlanarHoles |
| 7 | — | CreatePipe/ThickPipe/Slab/Thicken/ExtrudeAlongCrv | 同 #6 | `Append + JoinNakedEdges` 不产生闭合实体 | 全部改为 `JoinBreps` |

### 15.2 问题分类统计

| 类别 | 数量 | 占比 |
|------|------|------|
| RhinoCommon API 参数约束不理解 | 1（#2） | 14% |
| 坐标系语义不理解 | 2（#1, #4） | 29% |
| 实体构造方式选择错误 | 3（#5, #6, #7） | 43% |
| null 传播未防护 | 1（#3） | 14% |

### 15.3 反思：为什么会出现这些问题

#### 从代码调用角度

**核心问题：凭"看起来合理"的逻辑拼接 API，而非基于对 RhinoCommon API 行为的深入理解。**

- `CreateFromPlane`：不知道 NURBS 要求 `pointCount > degree`
- `CreateBox`：不知道 `Box(plane, x, y, z)` 的 Interval 是局部坐标
- `CreateTruncatedCone`：不知道 Loft 的接缝点对齐问题导致封盖失败
- `CreatePyramid`：不知道 `Brep.Append` 只追加面不合并拓扑

每个错误都是因为用了一个"看起来能用"的 API，但没有查阅它的实际行为约束。

#### 从框架角度

**问题：Geometry 层没有自我验证机制。**

Geometry 层方法创建几何后直接返回，不检查结果是否有效（`IsValid`/`IsSolid`）。一个 Brep 可能"创建了"但不是实体，一个 Surface 可能"返回了"但参数非法。无效结果被静默传递给上层，直到某处崩溃。

Command 层虽然有 null 检查，但在本次测试之前**完全没有错误输出**——静默返回 null，调用者无法知道哪一步出了问题。

#### 从文档角度

**问题：API 文档只记录"参数是什么"，不记录"参数的隐含约束"。**

| 文档写了什么 | 文档没写什么 |
|-------------|-------------|
| `corner1, corner2, normal` | normal 的长度就是高度（不是单纯方向） |
| `domainU, domainV` | Interval 是平面局部坐标，不是世界坐标 |
| `degree` | pointCount 必须大于 degree |
| `返回 null` | 什么条件下返回 null（UV 退化？参数非法？） |

#### 从 SKILL 角度

**问题：SKILL.md 规定了架构分层和命名规范，但没有约束 Geometry 层的实现质量。**

缺失的约束：
- Geometry 层输出必须验证有效性（`IsSolid`/`IsValid`）
- 优先使用 RhinoCommon 原生构造体（Cone/Cylinder/RevSurface）
- `Brep.Append` 不产生实体，必须用 `JoinBreps`
- `Vector3d` 作为方向+距离时禁止 `Unitize`

### 15.4 优化方案

#### 方案 A：SKILL.md 新增 Geometry 层实现规范

| 编号 | 规则 |
|------|------|
| G1 | 优先使用 RhinoCommon 原生构造体（Cone/Cylinder/RevSurface/Sphere），不手动拼接面 |
| G2 | 封盖统一用 `CapPlanarHoles`，不手动创建底盖曲线 |
| G3 | 多面体合并用 `Brep.JoinBreps`，不用 `Append + JoinNakedEdges` |
| G4 | 创建实体后必须检查 `IsSolid`，不满足时返回 null |
| G5 | `Vector3d` 作为方向+距离参数时，取 Length 前禁止 `Unitize` |
| G6 | `Box`/`Interval` 在 Plane 上下文中是局部坐标，世界坐标需 `RemapToPlaneSpace` 转换 |

#### 方案 B：Command 层错误输出规范（已实施）

- 所有错误必须在 Rhino 命令行报告，不静默返回 null
- 格式：`[方法名] 错误：具体原因`
- Geometry 返回 null 时，Command 层输出消息后返回 null

#### 方案 C：API 文档补充隐含约束

每个方法的文档表格增加一列"约束"，记录参数的隐含要求：
- normal.Length = 高度
- Interval = 局部坐标
- pointCount > degree
- 两点 UV 投影不能重合

---

## 16. 测试链 2 预检查与维护记录

### 16.1 检查范围

基于测试链 1 的经验教训（第 15 节），对 TestChain2 涉及的所有 Command 层和 Geometry 层方法进行预检查，防止同类问题再次出现。

### 16.2 检查项与结果

| 检查项 | 范围 | 发现 | 修复 |
|--------|------|------|------|
| 几何集合 null 元素检查 | Surface.cs 所有接收 `IEnumerable<Curve>`/`IEnumerable<GeometryBase>` 的方法（8个） | CreateNetworkSrf(#1)/CreateNetworkSrf(#2)/CreatePatch/CreateDrape/CreatePlanarSrf/CreateSweep(单轨)/CreateSweep(双轨)/CreateCutPlane 均缺少 null 元素检查 | ✅ 全部增加 null 元素检查 |
| 几何集合 null 元素检查 | Solid.cs（3个）+ Mesh.cs（1个）+ Curve.cs（4个） | CreateSweepSolid/CreateSolidFromBreps/CreateMeshPatch/CreateProjectCrv/CreatePullCrv/CreateDupEdge 缺少 null 元素检查（CreateLoftSolid/CreateApplyCrv 已有） | ✅ 6 个方法增加检查 |
| NURBS 参数约束 | CreateNurbsCurve(#1): 5点 degree=3 → 5>3 ✅; CreateNurbsCurve(#2): 3点 degree=2 → 3>=2 ✅ | 无问题 | — |
| 坐标系语义 | TestChain2 所有方法均使用 WorldXY/显式构造的 Plane，无坐标系混淆 | 无问题 | — |
| 实体构造方式 | TestChain2 涉及的曲面方法（CreateSrfThroughPts/CreateSrfControlPts/CreateNetworkSrf/CreatePatch/CreateDevLoft/CreateDrape）均为曲面（Brep），不要求 IsSolid | 无问题 | — |
| 参数校验完整性 | CreateCatenary（length>distance ✅）、CreateHelix（turns>0, radius>0 ✅）、CreateSpiral（turns>0, radius>0 ✅） | 无问题 | — |
| 依赖传播保护 | 步骤12/13/14 依赖步骤1/3 的结果；步骤16 依赖 CreatePlane 结果 | 如果上游失败，null 会被传入集合 → 已被 null 元素检查拦截 | ✅ |

### 16.3 TestChain2 测试代码本身

| 步骤 | 方法 | 参数检查 | 结论 |
|------|------|---------|------|
| 1 | CreateNurbsCurve(#1) | 5点 degree=3, 满足 pointCount>degree | ✅ |
| 2 | CreateNurbsCurve(#2) | 3点 degree=2, 6节点 3权重, 数量匹配 | ✅ |
| 3 | CreateInterpCrv | 4点 degree=3 | ✅ |
| 4 | CreateHandleCurve | 2组手柄点 | ✅ |
| 5 | CreateCurveThroughPt | 4点 degree=3 | ✅ |
| 6 | CreateCatenary | 距离12 < length16, gravity非零 | ✅ |
| 7 | CreateHelix(#1) | turns=4>0, radius=2>0 | ✅ |
| 8 | CreateHelix(#2) | turns=2>0, radius=1,2>0 | ✅ |
| 9 | CreateSpiral | turns=3>0, radius=1,5>0 | ✅ |
| 10 | CreateSrfThroughPts | 4x4点, degree=3, 满足 count>degree | ✅ |
| 11 | CreateSrfControlPts | 同上 | ✅ |
| 12 | CreateNetworkSrf(#1) | 4条曲线，可能不构成有效网络（曲线不相交） | ⚠️ 可能返回 null，但会被正确报告 |
| 13 | CreateNetworkSrf(#2) | U/V 曲线各2条 | ✅ |
| 14 | CreatePatch | 弧+线+NurbsCurve | ✅ |
| 15 | CreateDevLoft | 2条轨道曲线 | ✅ |
| 16 | CreateDrape | 先创建 Plane 再垂幕 | ✅ |
| 17 | CreateHeightfield | 文件不存在时跳过 | ✅ |

### 16.4 结论

TestChain2 测试代码本身参数合理，无坐标系错误或 NURBS 约束违反。主要风险来自**依赖传播**（上游步骤失败导致 null 传入集合），已通过全面增加 null 元素检查解决。

**与 TestChain1 的对比**：
- TestChain1 的问题集中在 Geometry 层实现（实体构造方式、坐标系转换、NURBS 约束）
- TestChain2 的关注点转移到 Command 层防护（几何集合的 null 元素检查）
- 两者共同验证了 SKILL.md G1-G6 规范和 Command 层错误输出规范的必要性

### 16.5 深度检查发现的问题

在完成 null 检查防护后，对 TestChain2 涉及的 Geometry 层实现进行深度审查，发现 3 个问题：

| # | 类型 | 文件 | 问题 | 影响 | 修复 |
|---|------|------|------|------|------|
| 1 | **代码 bug** | CatenaryGeo.cs | 悬链线牛顿迭代用了**空间距离**而非**水平距离**。公式 `length=2a·sinh(d/2a)` 中的 d 是水平距离 | 两点在重力方向有高差时，参数 a 和下垂量计算错误 | 改用 `horizontalDist`；增加 `horizontalDist≈0` 时的返回 null 保护 |
| 2 | **测试输入** | TestChain2 步骤16 | CreateDrape 用平面 Brep(Z=0) 作为垂幕对象，射线从 Z=0 向下射，与物体共面无法命中 | Drape 结果退化为平坦曲面，失去垂幕意义 | 改用 Box(高4) + 投影平面设在 Z=6 |
| 3 | **测试输入** | TestChain2 步骤12 | CreateNetworkSrf(#1) 的 4 条曲线不构成有效网络（对角线+三条不交叉的直线） | RhinoCommon 返回 null（error≠0） | 重建为 2 条平行 U 曲线 + 2 条 V 连接曲线 |

#### 问题 1 详解：CatenaryGeo 水平距离 vs 空间距离

悬链线是只在重力作用下悬挂的链条形状。数学上：
```
length = 2a · sinh(d / 2a)
```
其中 `d` 是两悬挂点的**水平距离**，不是空间直线距离。

旧代码计算了 `horizontalDist` 但没有使用它，而是用了 `pointDist`（空间距离）。当两点在同一水平面时两者相等（TestChain2 步骤6 的情况），bug 不触发。但只要两点有高差，计算结果就是错的。

#### 问题 2 详解：CreateDrape 射线与物体共面

`CreateDrape` 的工作原理是从投影平面上发射射线（方向 = -plane.Normal），通过 `RayShoot` 求与物体的交点。如果射线起点和物体在同一平面上，`RayShoot` 可能无法检测到交点（射线从物体内/表面出发）。

#### 问题 3 详解：网络曲面拓扑要求

`NurbsSurface.CreateNetworkSurface` 要求所有曲线相互交叉构成闭合网格。旧测试的 4 条曲线中，`_nurbsCurve1`（对角线）只与 `vCrv1`（底边）在起点相交，其余位置无交叉，不满足网络拓扑要求。

---

## 17. 依赖链总览

```
链1:
  步骤5 PointCloud → 步骤7 AddPoints → 步骤8 Remove → 步骤9 Reduce
  步骤12 Polyline → 步骤41 EdgeSrf
  步骤13 Rectangle → 步骤42 PlanarSrf, 步骤49 Extrude, 步骤55 ExtrudeSolid
  步骤14 Rectangle → 步骤41 EdgeSrf
  步骤18 Circle → 步骤43 Loft
  步骤19 Circle → 步骤43 Loft
  步骤29 Ellipse → 步骤46 Revolve
  步骤35 Plane → 步骤39 CutPlane, 步骤54 Fin

链2:
  步骤1 NurbsCurve → 步骤12 NetworkSrf, 步骤14 Patch
  步骤3 InterpCrv → 步骤13 NetworkSrf

链3:
  步骤0a Circle → 步骤1-4
  步骤0b Rectangle → 步骤0c Extrude → 步骤5-9, 11
  步骤0d Sphere → 步骤10-11

链4:
  步骤0a Sphere → 步骤1-2, 5, 7, 16-20
  步骤0b Box → 步骤3-4, 6, 8-9
  步骤0c Line → 步骤13-14, 18
  步骤0d Plane → 步骤15, 17

链5:
  步骤0a Box → 步骤10, 18
  步骤0c Plane → 步骤11
  步骤0d Polyline → 步骤12-13

链6:
  步骤0a Line → 步骤1-2
  步骤0c Polyline → 步骤3
  步骤0d Loft → 步骤5
  步骤0e Extrude → 步骤6
```

---

## 17. 测试链 3 预检查与修复

### 17.1 检查依据

基于测试链 1 和 2 的经验：
- **空间位置不匹配**（TestChain2 Patch 跨越 Y=0~120 的教训）：操作目标与输入几何的空间范围必须重叠
- **开放 vs 闭合几何体**（TestChain1 实体构造的教训）：投影/拉回等操作需要闭合实体才有足够的面来接收投影

### 17.2 发现的问题

| # | 类型 | 步骤 | 问题 | 影响 | 修复 |
|---|------|------|------|------|------|
| 1 | **基础几何** | 0c | CreateExtrude 产生开放管（无顶底盖），投影沿 -Z 穿过开放顶部无命中 | 步骤5 投影返回空 | 改用 CreateExtrudeSolid(capEnds=true) 产生带顶底盖的实体 |
| 2 | **空间不匹配** | 5 | 临时曲线在 (260,30) 投影不到 Brep(240~250, 15~23) | 投影返回空 | 移至 (243,18,9)→(247,20,9)，在 Brep 顶面上方 |
| 3 | **空间不匹配** | 6 | 临时曲线在 (260~270,40) 离 Brep 太远 | 拉回返回空 | 移至 (243,12,2)→(247,12,4)，在前墙附近 |
| 4 | **空间不匹配** | 7 | 临时曲线在 (260~270,50) 离 Brep 太远 | Pullback 失败 | 移至 (243,15,2)→(247,15,4)，在前墙面上 |

### 17.3 检查通过项

| 步骤 | 方法 | 检查结果 |
|------|------|---------|
| 1-2 | CreateDividePoints | 直接操作圆曲线，无空间依赖 |
| 3-4 | GetCurveStart/End | 直接操作圆曲线，无空间依赖 |
| 8 | CreateDupEdge | 直接操作 Brep 边缘，无空间依赖 |
| 9 | CreateExtractIsocurve | 点 (240,15,3) 在 Brep 面上 |
| 10 | CreateContour | 等高线平面无限延伸，球体 Z=0~10 全部覆盖 |
| 11 | CreateSection | 截面平面 Z=3 无限延伸，与球体 Z=0~10 相交 |

### 17.4 经验总结

**核心教训：提取/派生操作的输入几何必须与操作目标在空间上重叠。**

这是 TestChain2 "Patch 跨越 Y=0~120" 问题的同类变体——临时输入几何的空间位置与操作目标不匹配。区别在于：
- TestChain2：混用了远距离的两个已有几何（_nurbsCurve1 和临时曲线）
- TestChain3：临时曲线完全在目标 Brep 的空间范围之外

**防范规则**：编写涉及投影/拉回/包裹操作的测试时，必须先确认目标几何的空间范围（BoundingBox），再在此范围内或附近放置临时输入几何。

### 17.5 独立深度检查（第二轮）

对修复后的 TestChain3 再次逐步骤审查，发现 1 个遗漏问题：

| # | 步骤 | 方法 | 问题 | 影响 | 修复 |
|---|------|------|------|------|------|
| 5 | 8 | CreateDupEdge(#2) | `_brep0c` 是 capEnds=true 的闭合实体，所有边都是 Interior（两面共享），无 Naked 边。DupEdge 重载2 只提取 `EdgeAdjacency.Naked` 的边，返回空数组 | Assert.GreaterThanZero 失败 | 新增开放 Brep 基础几何 0e（CreateExtrude 无封盖），用其作为 DupEdge 重载2 的输入 |

#### 问题详解：闭合实体的边缘拓扑

挤出实体加封盖后（capEnds=true）是拓扑闭合的 Brep：
- 6 个面：4 侧面 + 1 顶面 + 1 底面
- 12 条边：每条边都被恰好两个面共享
- 所有边的 `Valence == EdgeAdjacency.Interior`（非 Naked）

DupEdge 重载2 的语义是"提取全部裸露边"（对应 Rhino 的 `DupEdge` 命令选中所有 Naked 边）。对闭合实体而言，裸露边数为 0 是正确的拓扑行为。

#### 修复方式：新增开放 Brep 基础几何

**原则**：测试的目的是检验 Command/Geometry 层的有效性，不能为了通过 Assert 而改变测试目标（如从测重载2改为测重载1）。

正确做法是**选择合适的测试输入**：新增 0e 基础几何（`CreateExtrude` 无封盖的开放挤出面），它天然有 8 条 Naked 边（4 底边 + 4 侧棱），专门用于测试 DupEdge 重载2 的"提取全部裸露边"功能。

| 基础几何 | 类型 | 空间范围 | Naked 边数 | 用途 |
|---------|------|---------|-----------|------|
| 0c | 闭合实体（capEnds=true） | X=240~250, Y=15~23, Z=0~6 | 0 | 步骤5-7（投影/拉回/包裹需要闭合面） |
| 0e | 开放挤出面（无封盖） | X=240~248, Y=50~56, Z=0~5 | 8 | 步骤8（DupEdge 重载2 需要 Naked 边） |

#### 其他步骤检查结果（全部通过）

| 步骤 | 检查点 | 结果 |
|------|--------|------|
| 1-4 | 直接操作圆曲线 | ✅ 无空间依赖 |
| 5 | 临时曲线(243~247,18~20,Z=9) 在 Brep 顶面(Z=6)上方，-Z 投影命中 | ✅ |
| 6 | 临时曲线(243~247,Y=12,Z=2~4) 距前墙(Y=15) 3 单位，拉回命中 | ✅ |
| 7 | 临时曲线(243~247,Y=15,Z=2~4) 在前墙面上，Pullback+Pushup | ✅ |
| 9 | 点(240,15,3) 在 Brep 上，等参线提取 | ✅ |
| 10 | 等高线 Z=0~10 间距2，球体 Z=0~10 全覆盖 | ✅ |
| 11 | 截面 Z=3 无限平面，球体 Z=0~10 相交 | ✅ |

---

## 18. 测试链 1 运行时问题记录（第一轮实机执行）

### 18.1 背景

第 15 节记录了 7 个通过**代码审查**发现的问题。修复后编译通过，但实际在 Rhino 中执行 `RhTestChain1` 后，仍有 7 个步骤 FAIL。

> 用户反馈："回顾第一个测试，我发现测试其实没通过，只是没报错而已。"

这说明**静态代码审查无法替代运行时测试**：API 参数约束、返回值语义、几何拓扑要求等问题，只有在实际调用时才会暴露。

### 18.2 运行时发现的问题清单

| # | 步骤 | 方法 | 问题表现 | 根因 | 修复方式 |
|---|------|------|---------|------|---------|
| 1 | 8 | RemovePointsFromCloud | 期望 4 项, 实际 2 项 | 步骤7 `AddPointsToCloud` 返回新 PointCloud，测试代码**未赋值回 cloud5**，导致数据链断裂。步骤8 的 cloud5 仍是原始 4 点 | **正确串联数据流**：步骤7/8/9 均用 `cloud5 = ...` 接收返回值，恢复期望值 6→4→3 |
| 2 | 24 | CreateCircle(#7) | 未能生成相切圆角圆 | 两条**平行**线段间距=6，半径=3（恰为间距一半），`Curve.CreateFillet` 在相切极限条件下无法生成圆角 | 改为**垂直相交**的 L 形线段，交点处可生成相切圆 |
| 3 | 28 | CreateArc(#4) | 未能生成相切圆角弧 | 同 #2，平行线段 + 半径=间距/2 的相切极限 | 同 #2，改为垂直相交线段 |
| 4 | 33 | CreateHyperbola | Index must be less than the number of knots | `NurbsCurve(3, true, 3, 3)` 第 3 参数是 order=3→degree=2，Knots.Count=4，代码却设了 6 个节点（索引 0~5 越界） | 节点改为 4 个：[0,0,1,1] |
| 5 | 34 | CreateConic | 同 #4 | ConicGeo.CreateConic 内部同样的 Knots 越界 | 同 #4 |
| 6 | 41 | CreateEdgeSrf | Geometry 层返回 null | `_poly12`（YZ 平面闭合多边形）和 `_rect14`（XY 平面矩形）端点不相接，违反文档"边缘曲线需端点相接"约束 | 改为局部 4 条端点相接的矩形边界 |
| 7 | 55 | CreateExtrudeSolid | 返回 null 或非实体（Faces=1, Edges=3） | 两层问题：(a) 代码层 `CapPlanarHoles` 返回值被丢弃（已修复）；(b) 测试数据层 `_rect13`（YZ 平面）挤出方向 `(0,0,6)` 在轮廓平面内，导致 2 条边与挤出方向平行、侧面退化为零面积，Brep 只有 1 面 3 边无法封盖。**根因是文档不清楚**——未警告方向退化约束 | (a) 接住返回值；(b) Geometry 层增加方向退化检测（`TryGetPlane` + 点积检查）；(c) 测试改用 YZ 平面 + X 方向挤出 |

### 18.3 问题分类统计

| 类别 | 数量 | 问题编号 | 占比 |
|------|------|---------|------|
| RhinoCommon API 返回值语义（返回新对象 vs 修改原对象） | 3 | #1, #6（间接）, #7 | 43% |
| NURBS 数学约束（Knots.Count 公式） | 2 | #4, #5 | 29% |
| RhinoCommon API 行为约束（CreateFillet 相切条件） | 2 | #2, #3 | 29% |

### 18.4 与第 15 节（设计时问题）的对比

| 维度 | 第 15 节（设计时/代码审查） | 第 18 节（运行时/实机执行） |
|------|--------------------------|--------------------------|
| 发现方式 | 代码审查、API 文档查阅 | Rhino 中执行 `RhTestChain1` |
| 问题数 | 7 | 7 |
| 主要类型 | 坐标系语义（29%）、实体构造方式（43%） | 返回值语义（43%）、NURBS 约束（29%）、API 行为约束（29%） |
| 共同点 | 均源于"对 RhinoCommon API 行为缺乏深入理解" |

**关键发现**：两轮检查的 14 个问题**无一重复**——代码审查能发现架构层面的问题（坐标系、实体构造策略），但无法发现运行时才暴露的问题（返回值语义、节点向量越界、相切极限条件）。

### 18.5 新增经验教训

#### 教训 1：RhinoCommon API 的"返回新对象"模式

以下 API 返回**新对象**，不修改原对象，调用者必须接收返回值：

| API | 错误用法 | 正确用法 |
|-----|---------|---------|
| `Brep.CapPlanarHoles(tol)` | `brep.CapPlanarHoles(tol);` | `brep = brep.CapPlanarHoles(tol);` |
| `PointCmd.AddPointsToCloud` | `AddPointsToCloud(cloud, pts);` | `cloud = AddPointsToCloud(cloud, pts);` |
| `PointCmd.RemovePointsFromCloud` | `RemovePointsFromCloud(cloud, idx);` | `cloud = RemovePointsFromCloud(cloud, idx);` |

> 已在 Solid.md 和 Point.md 中补充"关键约束：返回值语义"小节。
> 已提取为 SKILL.md G6 的补充：API 返回值语义必须在文档中明确标注。

#### 教训 2：NurbsCurve 构造函数参数的真实含义

```csharp
new NurbsCurve(dimension, rational, order, pointCount)
//                                          ↑      ↑
//                                       order   控制 Points.Count
//                                       degree = order - 1
//                                       Knots.Count = pointCount + degree - 1
```

| 参数 | 含义 | 常见误解 |
|------|------|---------|
| 第 3 参数 | **order**（阶数） | 误认为是 degree（次数） |
| Knots.Count | `cvCount + degree - 1` | 误认为与 order 无关 |

实例：`NurbsCurve(3, true, 3, 3)` → order=3, degree=2, Knots.Count=3+2-1=**4**（非 6）

#### 教训 2 补充：NURBS 参数的输入冗余性

**核心洞察**：NURBS 构造函数的 4 个参数中，只有 2 个是真正独立的，其余由数学公式严格推导：

```
独立输入         推导关系
────────         ────────
order    ──┐
            ├─→ degree = order - 1
            │
pointCount ─┤
            └─→ Knots.Count = pointCount + degree - 1
                Knots 具体值 = 钳端格式由 degree 决定
```

**Bug 的真正根源**：旧代码的错误不是数学公式记错，而是**违反了依赖关系**——构造函数已经锁定了 `order=3, pointCount=3`（隐含 Knots.Count=4），但代码却"独立地"写了 6 个节点赋值，两套数字不一致。

**根治方案（已实施）**：Geometry 层实现只维护 2 个独立参数（`order`、`pointCount`），degree/knotCount/knots 值全部由公式推导：

```csharp
const int order = 3;
const int pointCount = 3;
int degree = order - 1;
int knotCount = pointCount + degree - 1;
for (int i = 0; i < knotCount; i++)
    nc.Knots[i] = (i < degree) ? 0.0 : 1.0;
```

这样修改 order 时，所有派生值自动同步，消除了"推导值"与"独立写入"之间不一致的可能。

#### 教训 3：Curve.CreateFillet 的相切极限与平行线增强

`Curve.CreateFillet(c1, c2, radius, tol)` 要求两条曲线在半径为 `radius` 的圆与两者相切时，圆心位置唯一且有限。

**平行线的几何约束（关键数学推导）**：

对于两条平行线（间距 d），相切圆的圆心在两线之间，到两线距离之和 = d：

```
r + r = d  →  r = d/2 （唯一解）
```

- **r < d/2**：圆太小，无法同时接触两线（**不存在相切圆**）
- **r = d/2**：圆恰好填满，圆心可沿线方向任意滑动（**无穷多解** → CreateFillet 返回 null）
- **r > d/2**：圆太大，无法放入两线之间（**不存在相切圆**）

**结论**：平行线场景**在数学上无法通过 `Curve.CreateFillet` 实现**。这不是 API 限制，而是几何约束。Rhino 的 Fillet 命令处理平行线时用半圆（r=d/2），但底层 `CreateFillet` 不支持这种退化情况。

**解决方案（已实施）：在 Geometry 层增加平行线半圆分支**

新建 [FilletGeo.cs](file:///d:/Data/Project/Rhino_Workspace/Rhino/Geometry/Curve/FilletGeo.cs)，封装两种场景：
- **相交线**：走 `Curve.CreateFillet` 正常路径
- **平行线**：检测平行后，强制使用 `r = d/2`，在两线重叠区间中点构造半圆

Command 层 `CreateCircle(#7)` / `CreateArc(#4)` 改为调用 `FilletGeo.CreateFilletCircles` / `CreateFilletArcs`，对调用者透明地支持两种场景。

> **教训记录**：
> 1. 最初我错误地认为"平行线 d=6, r=2, 满足 2r<d 有两个对称解"。这是数学错误——**平行线相切圆的半径被严格锁定为 d/2**。经运行时测试验证后纠正。
> 2. 纠正后，进一步在 Geometry 层实现了平行线半圆分支，使封装 API 完全覆盖 Rhino Fillet 命令的能力（包括平行线退化情况）。

#### 教训 4：数据链测试必须真正串联

步骤 5→7→8→9 设计为一条点云数据链（4→6→4→3），但旧测试代码**没有把每步的返回值赋值回 `cloud5`**，导致链条断裂——每步实际操作的都是原始的 4 点 cloud5。

**错误修复（掩盖问题）**：把 Assert 期望值对齐到断裂后的实际值（4→2→3）。
**正确修复（根治）**：用 `cloud5 = ...` 接收返回值，让链条真正串联，恢复原始期望值（6→4→3）。

编写数据链测试时，每一步必须**显式传递状态**，不能假设上游操作会修改共享变量。

#### 教训 5：挤出方向必须与轮廓平面有法向分量

`Surface.CreateExtrusion(profile, direction)` 要求挤出方向**不在轮廓平面内**。当挤出方向与轮廓平面平行时（即方向向量在平面内），轮廓中与挤出方向平行的边会扫掠出**零面积侧面**，导致 Brep 退化为非预期结构（如 Faces=1, Edges=3），`CapPlanarHoles` 无法封盖。

**安全条件**：挤出方向应**垂直于轮廓平面**（即沿平面法向），或至少有显著的法向分量。

**本案**：`_rect13` 在 YZ 平面（法向 X 轴），挤出方向 `(0,0,6)`（Z 轴在 YZ 平面内）→ 退化。

**三层修复**：
1. **Geometry 层**（[SolidGeo.cs](file:///d:/Data/Project/Rhino_Workspace/Rhino/Geometry/Solid/SolidGeo.cs#L271-L279)）：增加方向退化检测——`profile.TryGetPlane` 获取轮廓平面，检查 `direction · normal` 是否接近零，是则输出错误并返回 null
2. **测试层**：改用 YZ 平面矩形 + X 方向挤出（正确的非 XY 平面测试）
3. **文档层**：[Solid.md](file:///d:/Data/Project/Rhino_Workspace/Rhino/Command/basicCommand/Solid.md#L191-L194) 补充实实现约束

> **是否是文档不清楚导致的？**
> **是**。修改前 RhinoCommon 文档和我们的文档都没有明确警告这种退化情况，Geometry 层也没有防御性检测。程序员无从知道"方向在平面内会退化"。修复后 Geometry 层会主动检测并返回明确错误，文档也补充了约束说明。

### 18.6 对后续测试链的启示

| 启示 | 应用到 |
|------|-------|
| 追踪 PointCloud/Brep 等 mutable 对象的实际数据流 | 链 5 的 Mesh 操作 |
| 确认所有 Fillet 类 API 的相切条件 | 链 6 的 CreatePipe |
| 验证 NurbsCurve/NurbsSurface 构造的 Knots.Count | 链 2 的 CreateNurbsCurve/CreateSrfControlPts |
| 确认所有"封盖/合并"类 API 的返回值是否被接收 | 链 6 的 CreateCap/CreateLoftSolid |

---

## 19. 测试链 2-6 调试记录（第二轮实机执行）

### 19.1 背景

在链 1 调试完成后，对链 2-6 进行代码审查 + 实机执行。这一轮发现了 **20 个问题**，涵盖参数丢弃、假实现、退化输入、断言设计、拓扑假设等类别。

### 19.2 问题分类总览

| 类别 | 数量 | 占比 | 典型问题 |
|------|------|------|---------|
| **参数丢弃**（接受参数但不使用） | 4 | 20% | capEnd/capEnds/tolerance 被忽略 |
| **假实现/未实现** | 2 | 10% | ConvexHull 无面、TextObject 假依赖 |
| **退化输入** | 4 | 20% | 共面点、挤出方向在平面内、投影压扁 |
| **断言设计** | 4 | 20% | 恒真断言、断言太弱、拓扑假设错 |
| **语义混淆** | 3 | 15% | ArrayLinear 间距 vs 总跨度 |
| **基础几何问题** | 3 | 15% | RevolveSolid 截面平面、Tessellation 约束冲突 |

### 19.3 各链问题清单

#### 测试链 2（自由曲线与曲面）

| # | 步骤 | 问题 | 根因 | 修复 |
|---|------|------|------|------|
| 1 | 17a/b | Heightfield 曲面扭曲振荡 | 采样密度不足（20×16=320 点 vs 50×50=2500 点），非算法选择问题 | 确认为预期行为：低密度采样本来就会产生粗糙曲面 |
| 2 | 17 | Heightfield 重载设计 | 最初假设采样密度不可自动推导，后确认重载 3（Min(像素,50)）是正确做法 | 保留 3 重载设计（完全控制/按比例/全自动） |

#### 测试链 3（对象提取与派生）

| # | 步骤 | 问题 | 根因 | 修复 |
|---|------|------|------|------|
| 3 | 8a | CreateDupEdge(#1) 未测试 | 原测试只调用 #2 重载，遗漏 #1 | 新增 8a 测试 #1（指定边），8b 测试 #2（Naked 边） |
| 4 | 8a/8b | 拓扑假设错误 | 假设闭合矩形挤出有 8 条 Naked 边，实际只有 2 条（周期性面合并接缝） | 查询实际拓扑后修正断言（Count(4)/Count(2)） |
| 5 | 8a | 恒真断言 | `Assert.Count(selectedEdges.Count, ...)` 从输入推导预期值，恒真 | 改为固定断言 `Assert.Count(4, ...)` |

#### 测试链 4（变换与阵列）

| # | 步骤 | 问题 | 根因 | 修复 |
|---|------|------|------|------|
| 6 | 10 | ArrayLinear 语义混淆 | 代码实现"总跨度"语义，文档写的是"间距"语义 | 改为双重载：#1 间距(Vector3d)、#2 总跨度(Point3d from/to) |
| 7 | 17 | ProjectToCPlane 退化 | 球体投影后压扁为退化曲面（体积=0） | 改用曲线测试（投影后仍为有效曲线） |
| 8 | 14 | ArrayAlongCrv(#2) 断言太弱 | `GreaterThanZero` 对任何正数都通过 | 改为 `Count(4)`（line=30, spacing=8 → 4 个） |

#### 测试链 5（网格与转换）

| # | 步骤 | 问题 | 根因 | 修复 |
|---|------|------|------|------|
| 9 | 3 | CreateMeshCone capEnd 丢弃 | command 层接受 capEnd 参数但未传给 geo 层 | geo 层增加封盖逻辑（CreateCircleCap） |
| 10 | 2 | CreateMeshCylinder capEnds 丢弃 | 同 #9 | 同 #9 |
| 11 | 15 | CreateConvexHull 假实现 | 只加顶点不加面，不是凸包 | 实现真正的增量凸包算法 |
| 12 | 15 | 凸包测试用共面点 | 4 个 Z=0 的点无法形成 3D 凸包 | 改用立方体 8 顶点 + 外部点（非共面） |
| 13 | 17 | CreatePatch tolerance 忽略 | tolerance 参数从未使用 | 用于去除过近的重复点 |
| 14 | 16 | CreateMeshFromTessellation 失败 | 固定边约束 + allowNewVertices=false 冲突 | 改用 5 点无固定边约束 |

#### 测试链 6（Solid 专属）

| # | 步骤 | 问题 | 根因 | 修复 |
|---|------|------|------|------|
| 15 | 4 | CreateTextObject 未实现 | 假设"需要 doc.Fonts，违反层级规则"——实际 Font/DimensionStyle 可直接 new | 实现 TextEntity.Create + CreateExtrusions → ToBrep |
| 16 | 13 | CreateRevolveSolid 截面退化 | 弧线在水平面（法向 Z），旋转轴也是 Z → 产生扁平环而非回转体 | 截面改为在包含旋转轴的竖直平面内 |

### 19.4 问题分类详解

#### 类型 A：参数丢弃（4 个）

**现象**：方法签名接受参数，但实现中完全未使用。

| 方法 | 丢弃参数 | 危害 |
|------|---------|------|
| CreateMeshCone | capEnd | 用户以为能控制封盖，实际被忽略 |
| CreateMeshCylinder | capEnds | 同上 |
| CreatePatch(Mesh) | tolerance | 用户以为能控制精度，实际被忽略 |
| ~~CreateHeightfield~~ | ~~（最初版本无此问题）~~ | |

**检测方法**：代码审查时检查每个参数是否在方法体中出现。

#### 类型 B：假实现/未实现（2 个）

**现象**：方法名承诺的功能与实际实现不符。

| 方法 | 承诺 | 实际 |
|------|------|------|
| CreateConvexHull | 计算点集的凸包 | 只加顶点到 Mesh，无面 |
| CreateTextObject | 创建 3D 文字 | 返回 null（假注释说"需要 doc.Fonts"） |

**根因**：ConvexHull 是因为 RhinoCommon 无内置 API 就放弃了；TextObject 是因为错误假设了 API 依赖。

**教训**：不要轻信"未实现"的注释，应先验证 API 的真实依赖关系。

#### 类型 C：退化输入（4 个）

**现象**：测试输入的几何关系导致结果退化（零体积、零面积、无解）。

| 测试 | 退化原因 | 后果 |
|------|---------|------|
| ProjectToCPlane(球体) | PlanarProjection 压扁 3D 实体 | 体积=0 的退化曲面 |
| CreateConvexHull(共面点) | 4 个 Z=0 点无法形成 3D 凸包 | 无面生成 |
| CreateMeshFromTessellation(固定边) | 边约束与 allowNewVertices=false 冲突 | 三角化无解 |
| CreateRevolveSolid(截面平面=旋转轴方向) | 截面在垂直于轴的平面内 | 旋转后所有点在同一水平面 |

**检测方法**：编写测试前思考"这个输入是否会让结果退化？"。

#### 类型 D：断言设计（4 个）

**现象**：断言无法有效检测 bug。

| 问题 | 错误做法 | 正确做法 |
|------|---------|---------|
| 恒真断言 | `Assert.Count(input.Count, result.Length)` | `Assert.Count(4, result.Length)` |
| 断言太弱 | `GreaterThanZero` | `Count(精确数字)` |
| 拓扑假设错 | 假设 8 条 Naked 边 | 查询实际 2 条 |
| 覆盖遗漏 | 只测 #2 重载 | #1 和 #2 分别测试 |

### 19.5 经验教训总结

#### 教训 1：参数丢弃是最容易检测的 bug

代码审查时逐个参数检查"是否在方法体中使用"，能在 30 秒内发现。这类 bug 100% 是疏忽。

#### 教训 2：不要假设 API 依赖

CreateTextObject 的"未实现"源于一个**未经证实的假设**（"需要 doc.Fonts"）。实际调研后发现 Font/DimensionStyle/TextEntity 都不需要 doc 上下文。

**规则**：遇到"未实现"或"无法实现"时，必须先查证 API 文档，确认真实依赖关系。

#### 教训 3：恒真断言比没有断言更危险

恒真断言给出"测试通过"的假象，让开发者以为代码正确。没有断言至少会提醒开发者"这里需要补充"。

**规则**：断言的预期值必须是预先确定的常数，不能从输入推导。

#### 教训 4：拓扑查询优先于拓扑假设

Brep 的拓扑结构（边数、面数、Naked/Interior 分类）受 Rhino 内部合并/识别策略影响，不能凭几何直觉假设。

**规则**：写拓扑相关断言前，先用代码查询实际值。

#### 教训 5：FAIL 时先诊断再修改

测试 FAIL 有两种原因：被测代码错 或 测试预期错。必须先查询实际值，判断哪边错了，只改错的那边。

**错误流程**：FAIL → 直接改测试（可能掩盖 bug）或直接改代码（可能破坏正确逻辑）。

### 19.6 与链 1 调试的对比

| 维度 | 链 1（第一轮） | 链 2-6（第二轮） |
|------|--------------|----------------|
| 问题数 | 14（设计 7 + 运行 7） | 20 |
| 主要类型 | 坐标系语义(29%)、实体构造(43%)、返回值语义(43%) | 参数丢弃(20%)、退化输入(20%)、断言设计(20%) |
| 代码修改 | 重构核心算法 | 补全参数逻辑 + 修正测试设计 |
| 测试修改 | 数据链串联 + 空间位置修正 | 拓扑查询 + 断言强度 + 覆盖补全 |

**两轮共发现 34 个问题，无一重复**——说明每一轮检查的侧重点不同：第一轮聚焦 API 行为和坐标系语义，第二轮聚焦参数完整性和测试有效性。
