# 测试命令 Overview

## 1. 目标

在 Rhino 中运行测试命令，调用所有 Command/Geometry 层方法，将结果写入文档供人工可视化检查。

## 2. 命令一览

| 命令名 | 功能 | 步骤数 | 覆盖范围 |
|--------|------|--------|---------|
| `RhTestChain1` | 基础创建链：点→线→面→体 | 62 | Point 全部 + Curve 基础 + Surface 从曲线 + Solid 基础 |
| `RhTestChain2` | 自由曲线与曲面 | 17 | Curve 自由曲线 + Surface 点/网络/补面/特殊 |
| `RhTestChain3` | 对象提取与派生 | 11 | Curve 提取/投影/剖面 |
| `RhTestChain4` | 变换与阵列 | 20 | Transform 全部 17 方法/20 重载 |
| `RhTestChain5` | 网格与转换 | 18 | Mesh 全部 16 方法/18 重载 |
| `RhTestChain6` | Solid 专属与特殊 | 14 | Solid 管道/板/文字/加厚/封盖 |

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
└── Chain6::Solid           链6的专属实体
```

每个图层使用不同颜色，方便视口中区分。

## 5. 运行效果

每个命令执行后：
1. 命令行逐条输出 `[PASS]` / `[FAIL]` + 方法名 + 简要结果
2. 成功创建的几何对象写入文档，按图层区分
3. 最终输出汇总：`Total X, Passed Y, Failed Z`
4. Rhino 视口可直接查看所有几何结果，旋转检查空间分布

## 6. 断言规则

| 验证类型 | 规则 | 示例 |
|---------|------|------|
| 非空检查 | 返回值不为 null | `result != null` |
| 有效性检查 | 几何对象 IsValid | `result.IsValid` |
| 数值精度 | 关键数值在容差内 | `Math.Abs(r - 5.0) < 1e-6` |
| 几何属性 | 体积/面积/长度 > 0 | `brep.GetVolume() > 0` |
| 数组长度 | 阵列/提取结果数量正确 | `results.Length == count` |

> 断言失败时记录 `[FAIL]` 并继续执行后续步骤（不中断）。
> 几何对象无论断言是否通过都写入文档，方便人工检查失败原因。

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
| 24 | `CreateCircle(#7)` | `curve1=临时线段A, curve2=临时线段B, radius=3, tol=0.001` | (35,62,0) | Curve | — | circles.Length>0 |
| 25 | `CreateArc(#1)` | `plane=WorldXY, center=(35,72,0), r=4, startAng=0, endAng=π` | (35,72,0) | Curve | — | arc.IsValid |
| 26 | `CreateArc(#2)` | `start=(31,80,0), onArc=(35,84,0), end=(39,80,0)` | (35,80,0) | Curve | — | arc.IsValid |
| 27 | `CreateArc(#3)` | `start=(31,90,0), end=(39,90,0), dir=(0,1,0)` | (35,90,0) | Curve | — | arc.IsValid |
| 28 | `CreateArc(#4)` | `curve1=临时线段C, curve2=临时线段D, radius=3, tol=0.001` | (35,100,0) | Curve | — | arcs.Length>0 |
| 29 | `CreateEllipse(#1)` | `plane=WorldXY, center=(35,112,0), r1=5, r2=3` | (35,112,0) | Curve | → 步骤46 | crv.IsValid |
| 30 | `CreateEllipse(#2)` | `plane=WorldXY, p1=(31,122,0), p2=(39,122,0), r2=3` | (35,122,0) | Curve | — | crv.IsValid |
| 31 | `CreateEllipse(#3)` | `f1=(33,132,0), f2=(37,132,0), pt=(35,135,0)` | (35,132,0) | Curve | — | crv.IsValid |
| 32 | `CreateParabola` | `start=(31,142,0), onParabola=(35,146,0), end=(39,142,0)` | (35,142,0) | Curve | — | crv.IsValid |
| 33 | `CreateHyperbola` | `focus=(35,154,0), vertex=(35,156,0), end=(39,158,0)` | (35,156,0) | Curve | — | crv.IsValid |
| 34 | `CreateConic` | `start=(31,166,0), end=(39,166,0), apex=(35,170,0), rho=0.5` | (35,166,0) | Curve | — | crv.IsValid |

> 步骤24/28 的临时线段不写入文档，仅用于测试相切圆/弧。线段 A=(31,60,0)-(39,60,0)，B=(31,66,0)-(39,66,0)；C=(31,98,0)-(39,98,0)，D=(31,104,0)-(39,104,0)。

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
| 55 | `CreateExtrudeSolid` | `profile=步骤13 Rectangle副本, dir=(0,0,6), capEnds=true` | (90,0,3) | Solid | — | brep.IsSolid |
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
| 17 | `CreateHeightfield` | `imagePath="test_heightfield.png", plane=WorldXY@(120,172,0), w=10, h=8, maxH=4, samplesX=20, samplesY=16` | (120,172,0) | Surface | — | brep.IsValid |

> 步骤17 需要一个测试图像文件。若文件不存在，此步骤标记为 SKIP。

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
| 8 | `CreateDupEdge` | `brep=步骤0c Brep` | (260,60,0) | Extract | crvs.Length>0 |
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
| 10 | `ArrayLinear` | `geo=小Box(420,0,0), dir=(0,1,0), count=4` | Y=0~36 | Transform | arr.Length==4 |
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
| 15 | `CreateMeshFromPoints` | `pts=[(575,0,0),(579,0,0),(575,4,0),(579,4,0)], tol=0.001` | (575,2,0) | Mesh | mesh.IsValid |
| 16 | `CreateMeshFromTessellation` | `pts=[(575,10,0),(579,10,0),(575,14,0),(579,14,0)], edges=[[(575,10,0),(579,10,0)]], plane=WorldXY, allowNew=false` | (575,12,0) | Mesh | mesh.IsValid |
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
| 4 | `CreateTextObject` | `text="Test", plane=WorldXY@(660,36,0), textHeight=4, thickness=1, font="Arial", bold=false, italic=false` | (660,36,0) | Solid | breps != null（可能为 SKIP） |
| 5 | `CreateThicken` | `brep=步骤0d Loft结果, distance=1, bothSides=false` | (660,48,2) | Solid | brep.IsSolid |
| 6 | `CreateCap` | `brep=步骤0e 未封闭Extrude` | (660,60,0) | Solid | brep.IsSolid |
| 7 | `CreateSolidFromBreps` | `breps=[临时Box A(660,72,0), 临时Box B(660,73,0)]（相交）` | (660,72,0) | Solid | brep.IsSolid |
| 8 | `CreateTorus` | `center=(660,84,0), normal=(0,0,1), majorR=5, minorR=1.5` | (660,84,0) | Solid | brep.IsSolid |
| 9 | `CreateEllipsoid` | `center=(660,96,0), normal=(0,0,1), radii=(5,3,2)` | (660,96,0) | Solid | brep.IsSolid |
| 10 | `CreateTube` | `base=(660,108,0), normal=(0,0,1), innerR=2, outerR=4, h=8, cap=true` | (660,108,4) | Solid | brep.IsSolid |
| 11 | `CreateTruncatedPyramid` | `base=(660,124,0), normal=(0,0,1), sides=4, botR=4, topR=2, h=8, cap=true` | (660,124,4) | Solid | brep.IsSolid |
| 12 | `CreateLoftSolid` | `curves=[临时圆C(660,140,0) r=3, 临时圆D(660,140,5) r=2, 临时圆E(660,140,10) r=1], loftType=0, cap=true` | (660,140,5) | Solid | brep.IsSolid |
| 13 | `CreateRevolveSolid` | `profile=临时弧线@ (660,154,0), axis=Line((660,154,0)-(660,154,10)), startAng=0, endAng=2π, cap=true` | (660,154,0) | Solid | brep.IsSolid |
| 14 | `CreateSweepSolid` | `rail1=临时直线A, rail2=临时直线B, sections=[临时圆], cap=true` | (660,170,0) | Solid | brep.IsSolid |

---

## 13. 覆盖率统计

| 功能区 | 方法数 | 重载数 | 测试链覆盖 | 步骤号 |
|--------|--------|--------|-----------|--------|
| Point | 8 | 9 | 链1 步骤1-9 | 全覆盖 |
| Curve | 28 | 39 | 链1 步骤10-34 + 链2 步骤1-9 + 链3 步骤1-11 | 全覆盖 |
| Surface | 20 | 25 | 链1 步骤35-54 + 链2 步骤10-17 | 全覆盖 |
| Solid | 20 | 22 | 链1 步骤55-62 + 链6 步骤1-14 | 全覆盖 |
| Mesh | 16 | 18 | 链5 步骤1-19（QuadRemesh 双重载） | 全覆盖 |
| Transform | 17 | 20 | 链4 步骤1-20 | 全覆盖 |
| **合计** | **109** | **133** | | **全覆盖** |

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

## 16. 依赖链总览

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
